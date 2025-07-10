using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public class DepositSchemaTest : TestCaseWithFactory
	{
		#region Deposit Charge Codes
		public void TestGetDepositChargeCodes()
		{
			AssertDepositChargeCodes();
			var newList = new CodeDescriptionPairList();
			newList.AddPair("AAA", "");
			newList.AddPair("BBB", "AAA");
			newList.AddPair("CCC", "");
			EDIDataRegistry.Instance.DepositChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newList);
			AssertDepositChargeCodes();
		}

		public void AssertDepositChargeCodes()
		{
			var list = EDIDataRegistry.Instance.DepositChargeCodes.Value;
			using (var cmd = Db.Connection.Command("select * from dbo.EdiGetDepositChargeCodes()"))
			{
				var table = new DataTable();
				cmd.NewDataAdapter().Fill(table);
				AssertEquals(list.Count, table.Rows.Count);
				for (int i = 0; i < list.Count; ++i)
				{
					AssertEquals(list[i].Code, (string)table.Rows[i][0]);
					if (list[i].Description.Length == 0)
					{
						AssertEquals(DBNull.Value, table.Rows[i][1]);
					}
					else
					{
						AssertEquals(list[i].Description, (string)table.Rows[i][1]);
					}
				}
			}
		}

		#endregion
		#region TriggerEdiDepositAdjust
		[TestDate(2015, 8, 25, 11, 30, 0)]
		public void TestTriggerEdiDepositAdjust()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var adjust = Factory.New<EdiDepositAdjust>();
			adjust.DEA_OH = org.PK;
			adjust.DEA_ChargeCode = "ODPL";
			adjust.DEA_Amount = 100;
			adjust.DEA_GC = Env.CurrentCompany.PK;
			adjust.DEA_RX_NKCurrency = "AUD";
			Factory.Save();
			int n = (int)Db.Connection.ExecuteScalar("select count(*) from dbo.EdiDepositBalanceChanges where DEC_AH is null" + " and DEC_DEA = '" + adjust.PK + "'" + " and DEC_OH = '" + org.PK + "'" + " and DEC_SystemLastEditTimeUtc = '2015-8-25 11:30'");
			AssertEquals("trigger fired", 1, n);
		}

		#endregion
		#region EdiDepositBalanceUpdate
		public void TestEdiDepositBalanceUpdate()
		{
			AssertEquals("Same deposit code for all companies", Enterprise.Integration.RegistryStorageFlags.System, EDIDataRegistry.Instance.OdplDepositChargeCode.Storage);
			AccTaxRate audRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 25);
			Factory.Save();
			GlbBranch sgdBranch = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch audBranch = Factory.NewWithValidTestData<GlbBranch>();
			sgdBranch.Company.GC_RX_NKLocalCurrency = "SGD";
			audBranch.Company.GC_RX_NKLocalCurrency = "AUD";
			audBranch.Company.GC_IsReciprocal = false;
			sgdBranch.Company.GC_IsReciprocal = true;
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			LicenceHeader anotherLic = BillingTestHelper.CreateLicence(Factory, "XYZ");
			ClientLicencePriceHeader prices = BillingTestHelper.CreatePriceList(lic);
			prices.L6_RX_NKCurrency = "USD";
			var billing = lic.Company.SelfBilling;
			Factory.Save();
			AccTaxRate sgdRate;
			// Setup SGD branch
			using (sgdBranch.SetAsTemporaryContext())
			{
				BusinessObjectFactory sgdFactory = new BusinessObjectFactory();
				RefExchangeRate usdExchange = BillingTestHelper.CreateExchangeRate(sgdFactory, "USD", 2m);
				sgdRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(sgdFactory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
				BillingTestHelper.CreateChargeCodeForBranch(sgdFactory, sgdBranch, sgdRate, LicenceCompany.OldOdplDepositChargeCode);
				BillingTestHelper.CreateChargeCodeForBranch(sgdFactory, sgdBranch, sgdRate, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				BillingTestHelper.CreateChargeCodeForBranch(sgdFactory, sgdBranch, sgdRate, EDIDataRegistry.SecurityDepositChargeCode);
				BillingTestHelper.CreateChargeCodeForBranch(sgdFactory, sgdBranch, sgdRate, "NOTDEPOSIT");
				sgdFactory.Save();
			}

			// Setup AUD branch
			using (audBranch.SetAsTemporaryContext())
			{
				BusinessObjectFactory audFactory = new BusinessObjectFactory();
				RefExchangeRate usdExchange = BillingTestHelper.CreateExchangeRate(audFactory, "USD", 1.25m);
				audRate = audFactory.Load<AccTaxRate>(audRate.PK);
				BillingTestHelper.CreateChargeCodeForBranch(audFactory, audBranch, audRate, LicenceCompany.OldOdplDepositChargeCode);
				BillingTestHelper.CreateChargeCodeForBranch(audFactory, audBranch, audRate, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				BillingTestHelper.CreateChargeCodeForBranch(audFactory, audBranch, audRate, EDIDataRegistry.SecurityDepositChargeCode);
				BillingTestHelper.CreateChargeCodeForBranch(audFactory, audBranch, audRate, "NOTDEPOSIT");
				audFactory.Save();
			}

			ARInvoice depositInvoice;
			ARInvoice securityDepositInvoice;
			// Create invoices
			using (sgdBranch.SetAsTemporaryContext())
			{
				BusinessObjectFactory sgdFactory = new BusinessObjectFactory();
				depositInvoice = BillingTestHelper.CreateDepositInvoice(sgdFactory, lic, 3, 1200m, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "NOTDEPOSIT");
				securityDepositInvoice = BillingTestHelper.CreateDepositInvoice(sgdFactory, lic, 4, 1300m, EDIDataRegistry.SecurityDepositChargeCode, "NOTDEPOSIT");
				BillingTestHelper.CreateDepositInvoice(sgdFactory, anotherLic, 5, 150m, LicenceCompany.OldOdplDepositChargeCode, "NOTDEPOSIT");
				BillingTestHelper.CreateDepositInvoice(sgdFactory, anotherLic, 6, 160m, LicenceCompany.OldOdplDepositChargeCode, "NOTDEPOSIT");
				AssertEquals("deposit in price currency", "USD", depositInvoice.Lines[0].AL_RX_NKTransactionCurrency);
				AssertEquals("exchange rate", 2m, depositInvoice.Lines[0].AL_ExchangeRate);
				AssertEquals("deposit in price currency", "USD", securityDepositInvoice.Lines[0].AL_RX_NKTransactionCurrency);
				AssertEquals("exchange rate", 2m, securityDepositInvoice.Lines[0].AL_ExchangeRate);
				// create some deposit usage
				ARInvoice invoice = sgdFactory.New<ARInvoice>();
				invoice.AH_OH = lic.Company.LC_OH;
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
				invoice.AH_Desc = "Usage";
				invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				invoice.AH_RX_NKTransactionCurrency = "USD";
				invoice.AH_GB = sgdBranch.PK;
				invoice.AH_GC = sgdBranch.GB_GC;
				invoice.AH_ExchangeRate = invoice.TransactionCurrency.CurrentSellRate;
				BillingInvoicingHelper.AddAmountLine(invoice, -80, ZDateTime.Now, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, "USD", null, "Deposit Deducted");
				// create some deposit usage
				ARInvoice invoiceSecurityDeposit = sgdFactory.New<ARInvoice>();
				invoiceSecurityDeposit.AH_OH = lic.Company.LC_OH;
				invoiceSecurityDeposit.AH_FullyPaidDate = ZDateTime.Empty;
				invoiceSecurityDeposit.AH_Desc = "Usage";
				invoiceSecurityDeposit.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				invoiceSecurityDeposit.AH_RX_NKTransactionCurrency = "USD";
				invoiceSecurityDeposit.AH_GB = sgdBranch.PK;
				invoiceSecurityDeposit.AH_GC = sgdBranch.GB_GC;
				invoiceSecurityDeposit.AH_ExchangeRate = invoiceSecurityDeposit.TransactionCurrency.CurrentSellRate;
				BillingInvoicingHelper.AddAmountLine(invoiceSecurityDeposit, -90, ZDateTime.Now, EDIDataRegistry.SecurityDepositChargeCode, "USD", null, "Deposit Deducted");
				sgdFactory.Save();
				Assert("there should be tax", 0m != invoice.Lines[0].AL_GSTVAT);
				Assert("there should be tax", 0m != invoiceSecurityDeposit.Lines[0].AL_GSTVAT);
			}

			var expectedOdpl = 3 * 1200m - 80m;
			var expectedSecurity = 4 * 1300m - 90m;
			using (audBranch.SetAsTemporaryContext())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var company2 = factory2.Load<LicenceCompany>(lic.Company.PK);
				DepositBalance.Update(true);
				// deposit correct in another branch with different GC_IsReciprocal and different USD exchange rate
				AssertBalance(expectedOdpl, expectedOdpl * 0.1m, "USD", company2, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(expectedSecurity, expectedSecurity * 0.1m, "USD", company2, EDIDataRegistry.SecurityDepositChargeCode);
			}

			// Reverse invoice
			using (sgdBranch.SetAsTemporaryContext())
			{
				depositInvoice.GenerateReverseTransaction(true);
				depositInvoice.Factory.Save();
				securityDepositInvoice.GenerateReverseTransaction(true);
				securityDepositInvoice.Factory.Save();
				DepositBalance.Update(false);
				lic.Company.DepositBalances.Load();
				AssertBalance(-80m, -8m, "USD", lic.Company, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(-90m, -9m, "USD", lic.Company, EDIDataRegistry.SecurityDepositChargeCode);
			}

			using (audBranch.SetAsTemporaryContext())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var company2 = factory2.Load<LicenceCompany>(lic.Company.PK);
				// balance is the same in another branch
				AssertBalance(-80m, -8m, "USD", company2, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(-80m, -8m, "USD", company2, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(-90m, -9m, "USD", company2, EDIDataRegistry.SecurityDepositChargeCode);
				depositInvoice = BillingTestHelper.CreateDepositInvoice(factory2, lic, 6, 3000m, LicenceCompany.OldOdplDepositChargeCode, "NOTDEPOSIT");
				securityDepositInvoice = BillingTestHelper.CreateDepositInvoice(factory2, lic, 7, 4000m, EDIDataRegistry.SecurityDepositChargeCode, "NOTDEPOSIT");
				var adjust1 = company2.DepositAdjustments.AddNew();
				adjust1.DEA_ChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
				adjust1.DEA_Amount = -1000m;
				adjust1.DEA_RX_NKCurrency = "USD";
				var adjust2 = company2.DepositAdjustments.AddNew();
				adjust2.DEA_ChargeCode = EDIDataRegistry.SecurityDepositChargeCode;
				adjust2.DEA_Amount = -2000m;
				adjust2.DEA_Tax = -200m;
				adjust2.DEA_RX_NKCurrency = "USD";
				factory2.Save();
			}

			expectedOdpl = 6 * 3000m - 80m - 1000m;
			expectedSecurity = 7 * 4000m - 90m - 2000m;
			using (audBranch.SetAsTemporaryContext())
			{
				DepositBalance.Update(false);
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var company2 = factory2.Load<LicenceCompany>(lic.Company.PK);
				AssertBalance(expectedOdpl, (expectedOdpl + 1000m) * 0.1m, "USD", company2, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(expectedSecurity, expectedSecurity * 0.1m, "USD", company2, EDIDataRegistry.SecurityDepositChargeCode);
			}

			// default branch
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var company2 = factory2.Load<LicenceCompany>(lic.Company.PK);
				AssertBalance(expectedOdpl, (expectedOdpl + 1000m) * 0.1m, "USD", company2, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(expectedSecurity, expectedSecurity * 0.1m, "USD", company2, EDIDataRegistry.SecurityDepositChargeCode);
			}

			using (sgdBranch.SetAsTemporaryContext())
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var company2 = factory2.Load<LicenceCompany>(lic.Company.PK);
				AssertBalance(expectedOdpl, (expectedOdpl + 1000m) * 0.1m, "USD", company2, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertBalance(expectedSecurity, expectedSecurity * 0.1m, "USD", company2, EDIDataRegistry.SecurityDepositChargeCode);
			}

			//missing ex. rates
			using (sgdBranch.SetAsTemporaryContext())
			{
				var sgdFactory = new BusinessObjectFactory();
				var gbpExchange = BillingTestHelper.CreateExchangeRate(sgdFactory, "GBP", 1.23m);
				Func<DataTable> getDepositBalances = () => Utilities.GetDataTableFromQuery("SELECT CONCAT(DEB_ChargeCode, '|', DEB_RX_NKCurrency, '|', DEB_Amount, '|', DEB_IsValid) AS Txt FROM dbo.EdiDepositBalance ORDER BY 1");
				DepositBalance.Update(true);
				var balances = getDepositBalances();
				AssertEquals(3, balances.Rows.Count);
				AssertEquals("DEPOSIT|AUD|1710.00|1", balances.Rows[0][0]);
				AssertEquals("DEPOSIT|USD|16920.00|1", balances.Rows[1][0]);
				AssertEquals("ODPLSECDEP|USD|25910.00|1", balances.Rows[2][0]);
				var invoice = sgdFactory.New<ARInvoice>();
				invoice.AH_OH = lic.Company.LC_OH;
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
				invoice.AH_Desc = "Usage";
				invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
				invoice.AH_RX_NKTransactionCurrency = "GBP";
				invoice.AH_GB = sgdBranch.PK;
				invoice.AH_GC = sgdBranch.GB_GC;
				invoice.AH_ExchangeRate = invoice.TransactionCurrency.CurrentSellRate;
				BillingInvoicingHelper.AddAmountLine(invoice, -90, ZDateTime.Now, EDIDataRegistry.SecurityDepositChargeCode, "GBP", null, "Deposit Deducted");
				sgdFactory.Save();
				DepositBalance.Update(true);
				balances = getDepositBalances();
				AssertEquals(3, balances.Rows.Count);
				AssertEquals("DEPOSIT|AUD|1710.00|1", balances.Rows[0][0]);
				AssertEquals("DEPOSIT|USD|16920.00|1", balances.Rows[1][0]);
				AssertEquals("The amount updated and the IsValid = 1", "ODPLSECDEP|USD|25854.65|1", balances.Rows[2][0]);
				TestConnection.ExecuteNonQuery("DELETE dbo.RefExchangeRate;");
				DepositBalance.Update(true);
				balances = getDepositBalances();
				AssertEquals(3, balances.Rows.Count);
				AssertEquals("DEPOSIT|AUD|1710.00|1", balances.Rows[0][0]);
				AssertEquals("DEPOSIT|USD|16920.00|1", balances.Rows[1][0]);
				AssertEquals("The IsValid = 0", "ODPLSECDEP|USD|25910.00|0", balances.Rows[2][0]);
			}
		}

		public void TestDepositWithDifferentCurrencyForPricelistAndInvoice()
		{
			AccTaxRate rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			Factory.Save();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.Company.GC_RX_NKLocalCurrency = "HKD";
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			ClientLicencePriceHeader prices = BillingTestHelper.CreatePriceList(lic);
			prices.L6_RX_NKCurrency = "USD";
			ClientLicenceBilling billing = lic.Company.SelfBilling;
			BillingTestHelper.SetInvoicing(lic, branch.PK, "HKD");
			const decimal usdPerHkd = 1m / 4m;
			Factory.Save();
			using (branch.SetAsTemporaryContext())
			{
				BusinessObjectFactory branchFactory = new BusinessObjectFactory();
				BillingTestHelper.CreateExchangeRate(branchFactory, "USD", 1m / usdPerHkd);
				RefCurrency usd = branchFactory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				branchFactory.Save();
			}

			ARInvoice depositInvoice;
			AccChargeCode depositChargeCode;
			ARInvoice securityDepositInvoice;
			AccChargeCode securityDepositChargeCode;
			using (branch.SetAsTemporaryContext())
			{
				depositChargeCode = BillingTestHelper.CreateChargeCodeForBranch(lic.Factory, branch, rate, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				depositInvoice = BillingTestHelper.CreateDepositInvoice(lic.Factory, lic, 3, 1200m, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, null);
				depositInvoice.AH_ExchangeRate = usdPerHkd;
				securityDepositChargeCode = BillingTestHelper.CreateChargeCodeForBranch(lic.Factory, branch, rate, EDIDataRegistry.SecurityDepositChargeCode);
				securityDepositInvoice = BillingTestHelper.CreateDepositInvoice(lic.Factory, lic, 4, 1200m, EDIDataRegistry.SecurityDepositChargeCode, null);
				securityDepositInvoice.AH_ExchangeRate = usdPerHkd;
			}

			Factory.Save();
			var expectedOdpl = 3 * 1200m;
			var expectedSecurity = 4 * 1200m;
			AssertBalance(expectedOdpl, expectedOdpl * 0.1m, "USD", lic.Company, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
			AssertBalance(expectedSecurity, expectedSecurity * 0.1m, "USD", lic.Company, EDIDataRegistry.SecurityDepositChargeCode);
			ARInvoice usageInvoice;
			ARInvoice usageInvoiceSecurityDeposit;
			using (branch.SetAsTemporaryContext())
			{
				usageInvoice = BillingTestHelper.CreateInvoice(lic.Factory, lic, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, -100m, 1, "", 0m);
				usageInvoice.AH_RX_NKTransactionCurrency = "HKD";
				usageInvoice.AH_ExchangeRate = 1m;
				usageInvoiceSecurityDeposit = BillingTestHelper.CreateInvoice(lic.Factory, lic, EDIDataRegistry.SecurityDepositChargeCode, -200m, 1, "", 0m);
				usageInvoiceSecurityDeposit.AH_RX_NKTransactionCurrency = "HKD";
				usageInvoiceSecurityDeposit.AH_ExchangeRate = 1m;
			}

			Factory.Save();
			AssertEquals("USD", depositInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("deposit currency needs exchanging", usdPerHkd, depositInvoice.AH_ExchangeRate);
			AssertEquals("deposit local currency", 3 * 1200m / usdPerHkd, depositInvoice.AH_LocalExTaxAmount);
			AssertEquals("USD", securityDepositInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("deposit currency needs exchanging", usdPerHkd, securityDepositInvoice.AH_ExchangeRate);
			AssertEquals("deposit local currency", 4 * 1200m / usdPerHkd, securityDepositInvoice.AH_LocalExTaxAmount);
			AssertEquals("HKD", usageInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("usage currency not exchanged", 1m, usageInvoice.AH_ExchangeRate);
			AssertEquals("HKD", usageInvoiceSecurityDeposit.AH_RX_NKTransactionCurrency);
			AssertEquals("usage currency not exchanged", 1m, usageInvoiceSecurityDeposit.AH_ExchangeRate);
			expectedOdpl = (3 * 1200m) - (100m / usdPerHkd);
			expectedSecurity = (4 * 1200m) - (200m / usdPerHkd);
			AssertBalance(expectedOdpl, expectedOdpl * 0.1m, "USD", lic.Company, EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
			AssertBalance(expectedSecurity, expectedSecurity * 0.1m, "USD", lic.Company, EDIDataRegistry.SecurityDepositChargeCode);
		}

		void AssertBalance(decimal amount, decimal tax, string currency, LicenceCompany licenceCompany, string chargeCode)
		{
			var balance = licenceCompany.DepositBalances.Cast<DepositBalance>().FirstOrDefault(x => x.ChargeCode == chargeCode);
			CombineAssertions(() =>
			{
				AssertNotNull(balance);
				AssertEquals("amount", amount, balance.Amount);
				AssertEquals("tax", tax, balance.Tax);
				AssertEquals("charge code", chargeCode, balance.ChargeCode);
			});
		}

		#endregion
		public void TestTriggerAccTransactionHeaderToDeposit_ExcludeNullLastEditTime()
		{
			void AssertEdiDepositBalanceChangesCount(ZGuid ah, ZGuid oh, bool hasRow)
			{
				var n = (int)TestConnection.ExecuteScalar($@"select count(*) from dbo.EdiDepositBalanceChanges where DEC_AH = '{ah}' and DEC_OH = '{oh}' AND DEC_DEA IS NULL;");
				AssertEquals(hasRow, n != 0);
			}

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var ar1 = Factory.NewWithValidTestData<ARInvoice>();
			var ar2 = Factory.NewWithValidTestData<ARInvoice>();
			ar1.AH_OH = ar2.AH_OH = org1.PK;
			Factory.Save();
			AssertEdiDepositBalanceChangesCount(ar1.PK, ar1.AH_OH, true);
			AssertEdiDepositBalanceChangesCount(ar2.PK, ar2.AH_OH, true);
			TestConnection.ExecuteNonQuery($"update dbo.AccTransactionHeader set AH_SystemLastEditTimeUtc = null, AH_SystemLastEditUser = AH_SystemLastEditUser where AH_PK = '{ar2.PK}';");
			TestConnection.ExecuteNonQuery($"delete dbo.EdiDepositBalanceChanges;");
			TestConnection.ExecuteNonQuery($"update dbo.AccTransactionHeader set AH_Desc = 'AH_Desc - 123', AH_SystemLastEditTimeUtc = AH_SystemLastEditTimeUtc, AH_SystemLastEditUser = AH_SystemLastEditUser;");
			AssertEdiDepositBalanceChangesCount(ar1.PK, ar1.AH_OH, true);
			AssertEdiDepositBalanceChangesCount(ar2.PK, ar2.AH_OH, false);
		}
		public void TestEdiDepositAdjustDEAChargeCodeMaxMatchesChargeCode()
		{
			AssertEquals(AccChargeCodeSchema.AC_Code.MaxLength, EdiDepositAdjustSchema.DEA_ChargeCode.MaxLength);
		}
	}
}
