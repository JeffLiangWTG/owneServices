using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Billing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class TransactionCreatorTest : TransactionCreatorBaseTest
	{
		#region RoundingForJapan

		public void TestRoundChargeAmountsForJapan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYen);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "JPAAM";
				shipment.JS_RL_NKOrigin = "USLAX";

				Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
				job.Parent = shipment;

				CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

				RefCurrency jPY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");

				for (int i = 0; i < 4; i++)
				{
					CreateCharge(job, CC1, "Charge Code 1", jPY, 100M, Creditor1, jPY, 150M, LocalClient);
				}

				job.Charges[0].JR_InvoiceType = job.Charges[1].JR_InvoiceType = "FIN";
				job.Charges[2].JR_InvoiceType = job.Charges[3].JR_InvoiceType = "ITC";

				job.Charges[0].JR_LocalSellAmt = job.Charges[2].JR_LocalSellAmt = 50;
				job.Charges[1].JR_LocalSellAmt = job.Charges[3].JR_LocalSellAmt = 17;

				Factory.Save();

				InvoicingPostManager postManager = new InvoicingPostManager(job);

				TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
				AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);

				AssertEquals(53m, job.Charges[0].JR_LocalSellAmt);
				AssertEquals(17m, job.Charges[1].JR_LocalSellAmt);
				AssertEquals(53m, job.Charges[2].JR_LocalSellAmt);
				AssertEquals(17m, job.Charges[3].JR_LocalSellAmt);

				InvoicingBase[] invoices = postManager.Poster.GetInvoices(jPY, LocalClient);
				AssertEquals(2, invoices.Length);

				for (int i = 0; i < 2; i++)
				{
					ARInvoice invoice = (ARInvoice)invoices[i];
					AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
					AssertEquals(53m, invoice.Lines[0].AL_LocalExTaxAmount);
					AssertEquals(17m, invoice.Lines[1].AL_LocalExTaxAmount);
				}
			}
		}

		public void TestRoundChargeAmountsForJapanJPX()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainNotReportableTaxRegistryID, Env.CurrentCompanyPK);
				rate.SetRateNumerator_ForTestOnly(0);
				rate.Factory.Save();

				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYenWithCharge);
				AccountingConfigurationRegistry.Instance.RoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CC2.PK.ToGuid());
				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "JPAAM";
				shipment.JS_RL_NKOrigin = "USLAX";

				Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
				job.Parent = shipment;

				CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
				CC2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

				RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				RefCurrency jPY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
				ExchangeRate rate1 = CreateExchangeRate(job, uSD, 1M);
				CreateCharge(job, CC1, "Charge Code 1", uSD, 100M, Creditor1, uSD, 153M, LocalClient);

				Factory.Save();

				InvoicingPostManager postManager = new InvoicingPostManager(job);

				TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
				AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

				AssertEquals(2, job.Charges.Count);
				AssertEquals(161m, job.Charges[0].JR_LocalSellAmt);
				AssertEquals(9m, job.Charges[1].JR_LocalSellAmt);

				InvoicingBase[] invoices = postManager.Poster.GetInvoices(jPY, LocalClient);
				AssertEquals(1, invoices.Length);

				ARInvoice invoice = (ARInvoice)invoices[0];
				AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
				AssertEquals(161m, invoice.Lines[0].AL_LocalExTaxAmount);
				AssertEquals(9m, invoice.Lines[1].AL_LocalExTaxAmount);
			}
		}

		#endregion

		#region Create Invoices With Local Client Only Bill In Local Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesWithLocalClientOnlyBillInLocalCurrency()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge2.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, LocalClient);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			#region AUD Invoice

			InvoicingBase[] currentInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, currentInvoices.Length);
			ARInvoice currentInvoice = (ARInvoice)currentInvoices[0];
			AssertEquals("Invoice Line Count", 4, currentInvoice.Lines.Count);

			AssertTransactionHeaderValues(currentInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1430.83M, 67.90M, 10M, 1498.73M, AUD, 1M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(currentInvoice);

			TransactionLine cC1Line = currentInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
				ZBool.False, currentInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = currentInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
				ZBool.True, currentInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC4Line = currentInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 751.88M, GSTFREE1, 0M, WHTFREE1, 0M, 751.88M, AUD, 1, Now,
				ZBool.False, currentInvoice, job, CC4, CC4.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC5Line = currentInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 328.95M, GST1, 32.90M, WHTFREE1, 0M, 361.85M, AUD, 1, Now,
				ZBool.False, currentInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create Invoices With Local Client Only Bill In Foreign Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesWithLocalClientOnlyBillInForeignCurrency()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);

			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, LocalClient);
			charge4.JR_PreventInvoicePrintGrouping = ZBool.True;
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 3, transactions.ARTransactionsCount);
			#region AUD Invoice

			InvoicingBase[] aUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, aUDInvoices.Length);
			ARInvoice aUDInvoice = (ARInvoice)aUDInvoices[0];
			AssertEquals("Invoice Line Count", 2, aUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(aUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 35.00M, 10.00M, 385.00M, AUD, 1M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(aUDInvoice);

			TransactionLine cC1Line = aUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
				ZBool.False, aUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = aUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
				ZBool.False, aUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region USD Invoice

			InvoicingBase[] uSDInvoices = creator.Poster.GetInvoices(USD, LocalClient);
			AssertEquals(1, uSDInvoices.Length);
			ARInvoice uSDInvoice = (ARInvoice)uSDInvoices[0];
			AssertEquals("Invoice Line Count", 1, uSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(uSDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				714.29M, 0M, 0M, 500.00M, USD, .7M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(uSDInvoice);

			TransactionLine cC4Line = uSDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, .7M, Now,
				ZBool.True, uSDInvoice, job, CC4, CC4.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC4Line);

			#endregion

			#region GBP Invoice

			InvoicingBase[] gBPInvoices = creator.Poster.GetInvoices(GBP, LocalClient);
			AssertEquals(1, gBPInvoices.Length);
			InvoicingBase gBPInvoice = gBPInvoices[0];
			AssertEquals("Invoice Line Count", 1, gBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(gBPInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				312.50M, 31.25M, 0M, 137.50M, GBP, .4M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(gBPInvoice);

			TransactionLine cC5Line = gBPInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, Now,
				ZBool.False, gBPInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/B");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create Invoices With Agent Only Bill In Local Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesWithAgentOnlyBillInLocalCurrency()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			charge3.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Agent);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			#region AUD Invoice

			InvoicingBase[] aUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, aUDInvoices.Length);
			ARInvoice aUDInvoice = (ARInvoice)aUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, aUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(aUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1580.16M, 43.65M, 39.33M, 1623.81M, AUD, 1M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(aUDInvoice);

			TransactionLine cC3Line = aUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now,
				ZBool.True, aUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = aUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
				ZBool.False, aUDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = aUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now,
				ZBool.False, aUDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion

		}

		#endregion

		#region Create Invoices With Agent Only Bill In Foreign Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesWithAgentOnlyBillInForeignCurrency()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			charge6.JR_PreventInvoicePrintGrouping = ZBool.True;

			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Agent);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);

			#region AUD Invoice

			InvoicingBase[] aUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, aUDInvoices.Length);
			ARInvoice aUDInvoice = (ARInvoice)aUDInvoices[0];
			AssertEquals("Invoice Line Count", 1, aUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(aUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 0M, 17.50M, 350.00M, AUD, 1M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(aUDInvoice);

			TransactionLine cC3Line = aUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350.00M, AUD, 1, Now,
				ZBool.False, aUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region USD Invoice

			InvoicingBase[] uSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, uSDInvoices.Length);
			InvoicingBase uSDInvoice = uSDInvoices[0];

			AssertEquals("Invoice Line Count", 2, uSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(uSDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1107.15M, 39.29M, 19.64M, 802.50M, USD, .7M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(uSDInvoice);

			TransactionLine cC4Line = uSDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, .7M, Now,
				ZBool.False, uSDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = uSDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now,
				ZBool.True, uSDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create Invoices with Agent Only Calculate Tax at Header Level

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesWithAgentOnlyWhenTaxIsCalculatedAtHeaderLevel()
		{
			bool expectAdjustment = true;

			ZInt originalSubUnitRatio = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			bool originalIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value;

			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GST1.SetRateNumerator_ForTestOnly(5);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			bool actualCalculateTaxAtHeaderLevel = AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value;
			AssertEquals("Precondition: CalculateTaxAtHeaderLevel", true, actualCalculateTaxAtHeaderLevel);

			try
			{
				Job job = CreateJob("Z00001000", LocalClient, 0M, Agent, 0M);
				AssertEquals("Precondition: CalculateTaxAtHeaderLevel", true, AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value);

				RefCurrency tWD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Taiwan);
				ExchangeRate tWDRate = CreateExchangeRate(job, tWD, 1M);

				Charge charge1 = CreateCharge(job, CC1, "Charge Code 3", AUD, 90M, Creditor3, tWD, 90M, Agent);
				Charge charge2 = CreateCharge(job, CC2, "Charge Code 4", AUD, 0M, null, tWD, 110M, Agent);
				Charge charge3 = CreateCharge(job, CC6, "Charge Code 6", AUD, 130M, Creditor2, tWD, 130M, Agent);

				charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

				Factory.Save();

				InvoicingPostManager creator = new InvoicingPostManager(job);
				AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

				TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Agent);
				AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

				InvoicingBase[] tWDInvoices = creator.Poster.GetInvoices(tWD, Agent);
				AssertEquals(1, tWDInvoices.Length);
				ARInvoice tWDInvoice = (ARInvoice)tWDInvoices[0];

				AssertEquals("Invoice Line Count", 3, tWDInvoice.Lines.Count);

				AssertEquals("OSExTaxAmount", 330m, tWDInvoice.AH_OSExTaxAmount);
				AssertEquals("OSTaxAmount", expectAdjustment ? 17m : 18m, tWDInvoice.AH_OSTaxAmount);
				AssertEquals("Invoice Lines", 3, tWDInvoice.Lines.Count);

				AssertEquals("Charge 1 OS Sell Amount", 90m, charge1.JR_OSSellAmt);
				AssertEquals("Charge 1 Revenue Line Amount", 90m, charge1.Revenue.AL_LineAmount);
				AssertEquals("Charge 1 OS Sell Tax Amount", 5m, charge1.JR_OSSellGSTAmt_Calc);
				AssertEquals("Charge 1 Revenue Tax Amount", 5m, charge1.Revenue.AL_GSTVAT);

				AssertEquals("Charge 2 OS Sell Amount", 110m, charge2.JR_OSSellAmt);
				AssertEquals("Charge 2 Revenue Line Amount", 110m, charge2.Revenue.AL_LineAmount);
				AssertEquals("Charge 2 OS Sell Tax Amount", 6m, charge2.JR_OSSellGSTAmt_Calc);
				AssertEquals("Charge 2 Revenue Tax Amount", 6m, charge2.Revenue.AL_GSTVAT);

				AssertEquals("Charge 3 OS Sell Amount", 130m, charge3.JR_OSSellAmt);
				AssertEquals("Charge 3 Revenue Line Amount", 130m, charge3.Revenue.AL_LineAmount);
				AssertEquals("Charge 3 OS Sell Tax Amount", expectAdjustment ? 6m : 7m, charge3.JR_OSSellGSTAmt_Calc);
				AssertEquals("Charge 3 Revenue Tax Amount", expectAdjustment ? 6m : 7m, charge3.Revenue.AL_GSTVAT);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = originalSubUnitRatio;
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalIsGSTRegistered;
				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, originalRegistryValue);
			}
		}

		#endregion

		#region Create All Revenue Invoices Bill In Local Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesAllRevenueChargesBillInLocalCurrency()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);

			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			charge3.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);

			#region AUD Agent Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			ARInvoice agentAUDInvoice = (ARInvoice)agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1580.16M, 43.65M, 39.33M, 1623.81M, AUD, 1, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now,
				ZBool.True, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = agentAUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentAUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region AUD Local Client Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				678.95M, 67.90M, 10.00M, 746.85M, AUD, 1M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC5Line = localClientAUDInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 328.95M, GST1, 32.90M, WHTFREE1, 0M, 361.85M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create All Revenue Invoices Bill Client In Local Bill Agent In Foreign

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesAllRevenueInvoicesBillClientInLocalBillAgentInForeign()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge2.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 3, transactions.ARTransactionsCount);

			#region AUD Agent Invoice

			InvoicingBase[] aUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, aUDInvoices.Length);
			InvoicingBase aUDInvoice = aUDInvoices[0];
			AssertEquals("Invoice Line Count", 1, aUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(aUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 0M, 17.50M, 350.00M, AUD, 1M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(aUDInvoice);

			TransactionLine cC3Line = aUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350.00M, AUD, 1, Now,
				ZBool.False, aUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region USD Agent Invoice

			InvoicingBase[] uSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, uSDInvoices.Length);
			InvoicingBase uSDInvoice = uSDInvoices[0];
			AssertEquals("Invoice Line Count", 2, uSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(uSDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1107.15M, 39.29M, 19.64M, 802.50M, USD, .7M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(uSDInvoice);

			TransactionLine cC4Line = uSDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, .7M, Now,
				ZBool.False, uSDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = uSDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now,
				ZBool.False, uSDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region AUD Local Client Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				678.95M, 67.90M, 10.00M, 746.85M, AUD, 1M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
				ZBool.True, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC5Line = localClientAUDInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 328.95M, GST1, 32.90M, WHTFREE1, 0M, 361.85M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/B");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create All Revenue Invoices Bill Client In Foreign Bill Agent In Local

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesAllRevenueInvoicesBillClientInForeignBillAgentInLocal()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			charge3.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 3, transactions.ARTransactionsCount);

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1580.16M, 43.65M, 39.33M, 1623.81M, AUD, 1M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now,
				ZBool.True, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = agentAUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentAUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Local Client AUD Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 2, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 35.00M, 10.00M, 385.00M, AUD, 1, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150.00M, GST1, 15.00M, WHTFREE1, 0M, 165.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200.00M, GST1, 20.00M, WHT1, 10.00M, 220.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Local Client GBP Invoice

			InvoicingBase[] localClientGBPInvoices = creator.Poster.GetInvoices(GBP, LocalClient);
			AssertEquals(1, localClientGBPInvoices.Length);
			InvoicingBase localClientGBPInvoice = localClientGBPInvoices[0];
			AssertEquals("Invoice Lines Count", 1, localClientGBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientGBPInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				312.50M, 31.25M, 0M, 137.50M, GBP, .4M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientGBPInvoice);

			TransactionLine cC5Line = localClientGBPInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, Now,
				ZBool.False, localClientGBPInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/B");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create All Revenue Invoices Bill In Foreign Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesAllRevenueChargesBillInForeignCurrency()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 4, transactions.ARTransactionsCount);

			#region AUD Agent Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 1, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 0M, 17.50M, 350.00M, AUD, 1M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350.00M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region USD Agent Invoice

			InvoicingBase[] agentUSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, agentUSDInvoices.Length);
			InvoicingBase agentUSDInvoice = agentUSDInvoices[0];
			AssertEquals("Invoice Line Count", 2, agentUSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentUSDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1107.15M, 39.29M, 19.64M, 802.50M, USD, .7M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentUSDInvoice);

			TransactionLine cC4Line = agentUSDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, .7M, Now,
				ZBool.False, agentUSDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentUSDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now,
				ZBool.False, agentUSDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Local Client AUD Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 2, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 35.00M, 10.00M, 385.00M, AUD, 1, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150.00M, GST1, 15.00M, WHTFREE1, 0M, 165.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200.00M, GST1, 20.00M, WHT1, 10.00M, 220.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Local Client GBP Invoice

			InvoicingBase[] localClientGBPInvoices = creator.Poster.GetInvoices(GBP, LocalClient);
			AssertEquals(1, localClientGBPInvoices.Length);
			InvoicingBase localClientGBPInvoice = localClientGBPInvoices[0];
			AssertEquals("Invoice Lines Count", 1, localClientGBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientGBPInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				312.50M, 31.25M, 0M, 137.50M, GBP, .4M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientGBPInvoice);

			TransactionLine cC5Line = localClientGBPInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, Now,
				ZBool.False, localClientGBPInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/B");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/C");

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Post Revenue With CFX

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostAllRevenueWithCFX()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			charge3.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, AUD, 300M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, AUD, 400M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;
			AccTransactionLines charge7WIP = charge7.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;
			AccTransactionLines charge7Accrual = charge7.Accrual;

			ChargePoster poster = new ChargePoster(Factory);
			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			foreach (IReceivablesPostingChargeCollection postingCharges in distributedCharges)
			{
				poster.Post(postingCharges);
			}

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1543.65M, 40.00M, 37.50M, 1583.65M, AUD, 1, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, Now,
				ZBool.True, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = agentAUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentAUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 400.00M, GST1, 40.00M, WHT1, 20.00M, 440.00M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Local Client AUD Invoice

			InvoicingBase[] localClientAUDInvoices = poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Line Count", 4, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				950.00M, 65.00M, 10.00M, 1015.00M, AUD, 1, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC5Line = localClientAUDInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 300M, GST1, 30M, WHTFREE1, 0M, 330.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			TransactionLine cC7Line = localClientAUDInvoice.FindTransactionLine("REV", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "REV", 7, "Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(poster.PostedInvoices, "Z00001000/A");

			#region CFX Journal

			InvoicingBase[] invoices = poster.GetInvoices(AUD, Agent);
			AssertEquals(1, invoices.Length);
			ARInvoice invoice = (ARInvoice)invoices[0];

			JCJournalHeader cFXJournal = distributedCharges.GetCharges(Agent, AUD).CFXJournal;
			AssertEquals("CFX Line Count", 1, cFXJournal.Lines.Count);

			AssertTransactionHeaderValues(cFXJournal, "JC", "JNL", null, "Job Costing Journal (CFX)", Now, Now,
				0M, 0M, 0M, 0M, AUD, 1, Now, ZBool.False, null, null, "", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(cFXJournal);

			TransactionLine cFXLine1 = cFXJournal.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cFXLine1, "REV", 1, "Charge Code 4", -79.36M, null, 0M, null, 0M, -79.36M, AUD, 1, Now,
				ZBool.False, cFXJournal, job, CC4, CC4.RevenueAccount, null);
			AssertTransactionLineDefaults(cFXLine1);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", true, charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", false, charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Create All Cost Invoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnly()
		{
			SetupCharges();

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			InvoicingPostManager creator = new InvoicingPostManager(Job);
			TransactionCreatorHashtable transactions;

			SetUpRegistryForTest();
			try
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;
				transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			}
			finally
			{
				ResetRegistryForTest();
			}
			AssertEquals("Payables Transaction Count", 4, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, Job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, Job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, Job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, Job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, Job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, Job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, Job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, Job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, Job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, Job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, Charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, Charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#region TestCreateInvoicesCostsOnlyAsUAInvoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsUAInvoices()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCreateInvoicesCostsOnlyAsUAInvoices(false);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsRequests_WhenChargeApprovalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertCreateInvoicesCostsOnlyAsUAInvoices(true);
		}

		void AssertCreateInvoicesCostsOnlyAsUAInvoices(bool isChargeApprovalActivated)
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SetupCharges();
			Charge3.JR_OSCostAmt = -300;

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(Job.PK, (ZString)Job.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns((ZDialogResult)DialogResult.OK);
			}
			InvoicingPostManager creator = new InvoicingPostManager(Job, guiWrapperMock.Object);
			TransactionCreatorHashtable transactions;

			SetUpRegistryForTest();
			try
			{
				if (isChargeApprovalActivated)
				{
					Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
				}
				else
				{
					Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				}
				transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", isChargeApprovalActivated ? 2 : 4, transactions.Count);
			AssertEquals("AP Transactions Count", 2, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 2 : 0, requests.Length);

			#region Creditor 1 Invoice 1

			if (isChargeApprovalActivated)
			{
				var request = requests.Where(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "1").FirstOrDefault();
				AssertEquals("Request lines", 2, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
				charge = charges.FirstOrDefault(x => x.ChargeCode == CC7.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
				AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
				AssertEquals("Should be correct type of invoice", LedgerTypes.UnapprovedPayableTransactions, creditor1Inv1.AH_Ledger);
				AssertEquals("Should be correct type of invoice", TransactionTypes.UAInvoice, creditor1Inv1.AH_TransactionType);

				TransactionLine cC1Line = creditor1Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, Job.PK);
				AssertNotNull("Should be correct type of line", cC1Line);

				TransactionLine cC7Line = creditor1Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC7, Job.PK);
				AssertNotNull("Should be correct type of line", cC7Line);
			}

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor2Inv1.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable);
			Assert("Should be APInvoice", creditor2Inv1.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice);
			Assert("Should be APInvoice", creditor2Inv1 is APInvoice);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC2, Job.PK);
			Assert("Should be APInvoiceLine", cC2Line is APInvoiceLine);

			#endregion

			#region Creditor 3 Invoice 1

			APCreditNote creditor3Inv1 = transactions.RetrieveAPCreditNote(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor3Inv1.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable);
			Assert("Should be APInvoice", creditor3Inv1.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote);
			Assert("Should be APCreditNote", creditor3Inv1 is APCreditNote);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC3, Job.PK);
			AssertNull("Should be approved", cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			if (isChargeApprovalActivated)
			{
				var request = requests.Where(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "2").FirstOrDefault();
				AssertEquals("Request lines", 1, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC5.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
				AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
				AssertEquals("Should be correct type of invoice", LedgerTypes.UnapprovedPayableTransactions, creditor1Inv2.AH_Ledger);
				AssertEquals("Should be correct type of invoice", TransactionTypes.UAInvoice, creditor1Inv2.AH_TransactionType);

				TransactionLine cC5Line = creditor1Inv2.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC5, Job.PK);
				AssertNotNull("Should be UAInvoiceLine", cC5Line);
			}

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", !isChargeApprovalActivated, Charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, Charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 hasn't Accrual", true, Charge3Accrual == null);
			AssertEquals("Charge 5 Accrual Reversed", !isChargeApprovalActivated, Charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, Charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", !isChargeApprovalActivated, Charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsUAInvoicesWithNegativeCharges()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 300M, Creditor1, AUD, 150M, LocalClient);
			Charge7 = CreateCharge(Job, CC7, "Charge Code 7", AUD, -200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(Charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge7, "1", Now.AddDays(10), Now.AddDays(20));
			Factory.Save();

			InvoicingPostManager creator = new InvoicingPostManager(Job);
			TransactionCreatorHashtable transactions;

			SetUpRegistryForTest();
			try
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", 1, transactions.Count);

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor1Inv1.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable);
			Assert("Should be APInvoice", creditor1Inv1.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice);
			Assert("Should be APInvoice", creditor1Inv1 is APInvoice);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC1, Job.PK);
			AssertNotNull("Should be APInvoiceLine", cC1Line is APInvoiceLine);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC7, Job.PK);
			AssertNotNull("Should be APInvoiceLine", cC7Line is APInvoiceLine);
		}

		#endregion

		#region Post All With Revenue Bill In Local Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoiceAllCostsAndRevenuesBillInLocalCurrency()
		{
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "1", Now.AddDays(10), Now.AddDays(20));
			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;
			AccTransactionLines charge7WIP = charge7.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;
			AccTransactionLines charge7Accrual = charge7.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Payables Transaction Count", 4, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals("1 invoice only", 1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];

			AssertEquals("Invoice Line Count", 3, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1580.16M, 43.65M, 39.33M, 1623.81M, AUD, 1, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.5M, 350M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);

			TransactionLine cC4Line = agentAUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC4, CC4.RevenueAccount, Agent);

			TransactionLine cC6Line = agentAUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC6, CC6.RevenueAccount, Agent);

			#endregion

			#region Local Client AUD Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals("1 invoice only", 1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				978.95M, 67.90M, 10.00M, 1046.85M, AUD, 1, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC5Line = localClientAUDInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 328.95M, GST1, 32.90M, WHTFREE1, 0M, 361.85M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			TransactionLine cC7Line = localClientAUDInvoice.FindTransactionLine("REV", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "REV", 7, "Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", true, charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Post All With Revenue Bill In Foreign Currency

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoiceAllCostsAndRevenuesDontBillInLocalCurrency()
		{
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge6.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "1", Now.AddDays(10), Now.AddDays(20));
			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;
			AccTransactionLines charge7WIP = charge7.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;
			AccTransactionLines charge7Accrual = charge7.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Payable Transaction Count", 4, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 4, transactions.ARTransactionsCount);

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals("1 invoice only", 1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 1, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				350.00M, 0M, 17.50M, 350.00M, AUD, 1M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350.00M, AUD, 1, Now,
				ZBool.False, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Agent USD Invoice

			InvoicingBase[] agentUSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals("1 invoice only", 1, agentUSDInvoices.Length);
			InvoicingBase agentUSDInvoice = agentUSDInvoices[0];
			AssertEquals("Invoice Line Count", 2, agentUSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentUSDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1107.15M, 39.29M, 19.64M, 802.50M, USD, .7M, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentUSDInvoice);

			TransactionLine cC4Line = agentUSDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, .7M, Now,
				ZBool.False, agentUSDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentUSDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, .7M, Now,
				ZBool.True, agentUSDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Local Client AUD Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals("1 invoice only", 1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 3, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				650.00M, 35.00M, 10.00M, 685.00M, AUD, 1, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150.00M, GST1, 15.00M, WHTFREE1, 0M, 165.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200.00M, GST1, 20.00M, WHT1, 10.00M, 220.00M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC7Line = localClientAUDInvoice.FindTransactionLine("REV", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "REV", 7, "Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300M, AUD, 1, Now,
				ZBool.False, localClientAUDInvoice, job, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Local Client GBP Invoice

			InvoicingBase[] localClientGBPInvoices = creator.Poster.GetInvoices(GBP, LocalClient);
			AssertEquals("1 invoice only", 1, localClientGBPInvoices.Length);
			InvoicingBase localClientGBPInvoice = localClientGBPInvoices[0];

			AssertEquals("Invoice Lines Count", 1, localClientGBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientGBPInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				312.50M, 31.25M, 0M, 137.50M, GBP, .4M, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientGBPInvoice);

			TransactionLine cC5Line = localClientGBPInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, Now,
				ZBool.False, localClientGBPInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/B");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/C");

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", true, charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Post All With Revenue Bill In Local Currency With Payments

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostAllCostsWithPayments()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			CreateExchangeRate(job, USD, .7M);
			CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge7 = CreateCharge(job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge6, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge7, "3", Now.AddDays(10), Now.AddDays(20));

			SetAPPaymentInfo(charge1, "CSH", AUDBankAccount, "CASH");
			SetAPPaymentInfo(charge6, "CCD", USDBankAccount, "CC");
			SetAPPaymentInfo(charge7, "CSH", AUDBankAccount, "CASH");
			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;
			AccTransactionLines charge7WIP = charge7.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;
			AccTransactionLines charge7Accrual = charge7.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			AssertEquals("Payables Transaction Count", 8, transactions.APTransactionsCount + transactions.GetAllAPPaymentApprovals().Length);
			AssertEquals("Receivable Transactions Count", 2, transactions.ARTransactionsCount);

			#region Agent AUD Invoice

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals("1 invoice only", 1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				1580.16M, 43.65M, 39.33M, 1623.81M, AUD, 1, Now, ZBool.False, Agent, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.5M, 350M, AUD, 1, Now, Now, ZBool.False, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);

			TransactionLine cC4Line = agentAUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 793.65M, GSTFREE1, 0M, WHTFREE1, 0M, 793.65M, AUD, 1, Now, Now, ZBool.False, agentAUDInvoice, job, CC4, CC4.RevenueAccount, Agent);

			TransactionLine cC6Line = agentAUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 436.51M, GST1, 43.65M, WHT1, 21.83M, 480.16M, AUD, 1, Now, Now, ZBool.False, agentAUDInvoice, job, CC6, CC6.RevenueAccount, Agent);

			#endregion

			#region Local Client AUD Invoice

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals("Should only be one invoice", 1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Line Count", 4, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "Z00001000", Now, Now,
				978.95M, 67.90M, 10.00M, 1046.85M, AUD, 1, Now, ZBool.False, LocalClient, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, Now, Now, ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, Now, Now, ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC5Line = localClientAUDInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 328.95M, GST1, 32.90M, WHTFREE1, 0M, 361.85M, AUD, 1, Now, Now, ZBool.False, localClientAUDInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			TransactionLine cC7Line = localClientAUDInvoice.FindTransactionLine("REV", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "REV", 7, "Charge Code 7", 300M, GSTFREE1, 0M, WHTFREE1, 0M, 300M, AUD, 1, Now, Now, ZBool.False, localClientAUDInvoice, job, CC7, CC7.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000");
			AssertInvoicesContainConsolidatedInvoiceRef(creator.Poster.PostedInvoices, "Z00001000/A");

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 1, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-100M, -10M, 0M, -110M, AUD, 1, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, job, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -0M, 0M, -200M, AUD, 1, Now, ZBool.False, Creditor1, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			cC7Line = creditor1Inv3.FindTransactionLine("CST", CC7, job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 1, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv3, job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-285.71M, -28.57M, -14.29M, -220.00M, USD, 0.700013M, Now, ZBool.False, Creditor2, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.True);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor2Inv2);

			cC6Line = creditor2Inv2.FindTransactionLine("CST", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "CST", 1, "Charge Code 6", -285.71M, GST1, -28.57M, WHT1, -14.29M, -220.00M, USD, 0.7000105M, Now, Now,
				ZBool.False, creditor2Inv2, job, CC6, CC6.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC6Line);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv1);

			cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, job, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Cash AUD Bank Account Payment

			PaymentApprovalBase cashAUDBankAccountPaymentApproval = transactions.RetrieveAPPaymentApproval(Creditor1, AUDBankAccount, "CSH", "CASH", job.JH_JobNum);
			APPayment cashAUDBankAccountPayment = (APPayment)cashAUDBankAccountPaymentApproval.NewPayment;
			AssertTransactionHeaderValues(cashAUDBankAccountPayment, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				310.00M, 0M, 0M, 310M, AUD, 1, Now, ZBool.False, Creditor1, null, ZString.Empty, 0, "CASH", "CSH",
				AUDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(cashAUDBankAccountPayment);

			#endregion

			#region Credit Card USD Bank Account Payment

			PaymentApprovalBase cardUSDBankAccountPaymentApproval = transactions.RetrieveAPPaymentApproval(Creditor2, USDBankAccount, "CCD", "CC", job.JH_JobNum);
			APPayment cardUSDBankAccountPayment = (APPayment)cardUSDBankAccountPaymentApproval.NewPayment;
			AssertTransactionHeaderValues(cardUSDBankAccountPayment, "AP", "PAY", null, "AP Payment Z00001000", Now, ZDateTime.Empty,
				314.28M, 0M, 0M, 220M, USD, 0.700013M, Now, ZBool.False, Creditor2, null, ZString.Empty, 0, "CC", "CCD",
				USDBankAccount, ZBool.True);
			AssertTransactionHeaderDefaults(cardUSDBankAccountPayment);

			#endregion

			#region Match Links

			TransactionMatchLinkGroup aUDCashMatchLinks = new TransactionMatchLinkGroup(cashAUDBankAccountPaymentApproval.NewPaymentMatchingObject.MatchLinks);
			TransactionMatchLinkGroup uSDCardMatchLinks = new TransactionMatchLinkGroup(cardUSDBankAccountPaymentApproval.NewPaymentMatchingObject.MatchLinks);

			#region AUD Cash Match Links

			AssertEquals("AUD Cash Match Link Count", 3, aUDCashMatchLinks.Count);
			TransactionMatchLink payment1MatchLink = aUDCashMatchLinks.FindMatchLinkByTransactionHeaderAndAmount(cashAUDBankAccountPayment, 310M);
			TransactionMatchLink invoice1aMatchLink = aUDCashMatchLinks.FindMatchLinkByTransactionHeaderAndAmount(creditor1Inv1, -110M);
			TransactionMatchLink invoice1bMatchLink = aUDCashMatchLinks.FindMatchLinkByTransactionHeaderAndAmount(creditor1Inv3, -200M);

			AssertNotNull("Payment1 Match Link Found", payment1MatchLink);
			AssertNotNull("Invoice1a Match Link Found", invoice1aMatchLink);
			AssertNotNull("Invoice1b Match Link Found", invoice1bMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(aUDCashMatchLinks, invoice1aMatchLink, payment1MatchLink);
			AssertInvoiceAndPaymentAreInSameGroup(aUDCashMatchLinks, invoice1bMatchLink, payment1MatchLink);

			AssertEquals("Payment Match Date", Now.ToString("yyyy MMM dd"), payment1MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice Match Date", Now.ToString("yyyy MMM dd"), invoice1aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice Match Date", Now.ToString("yyyy MMM dd"), invoice1bMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment1MatchLink);
			AssertMatchLinkDefaults(invoice1aMatchLink);
			AssertMatchLinkDefaults(invoice1bMatchLink);

			//AssertAPInvoiceShowsAsPaid(Creditor1Inv1);
			//AssertAPInvoiceShowsAsPaid(Creditor1Inv3);

			#endregion

			#region USD Card Match Links

			AssertEquals("USD Card Match Count", 2, uSDCardMatchLinks.Count);
			TransactionMatchLink payment2MatchLink = uSDCardMatchLinks.FindMatchLinkByTransactionHeaderAndAmount(cardUSDBankAccountPayment, 314.28M);
			TransactionMatchLink invoice2aMatchLink = uSDCardMatchLinks.FindMatchLinkByTransactionHeaderAndAmount(creditor2Inv2, -314.28M);

			AssertNotNull("Payment 2 Match Link Found", payment2MatchLink);
			AssertNotNull("Invoice 2a Match Link Found", invoice2aMatchLink);

			AssertInvoiceAndPaymentAreInSameGroup(uSDCardMatchLinks, invoice2aMatchLink, payment2MatchLink);
			AssertEquals("Payment Match Date", Now.ToString("yyyy MMM dd"), payment2MatchLink.AP_MatchDate.ToString("yyyy MMM dd"));
			AssertEquals("Invoice Match Date", Now.ToString("yyyy MMM dd"), invoice2aMatchLink.AP_MatchDate.ToString("yyyy MMM dd"));

			AssertMatchLinkDefaults(payment2MatchLink);
			AssertMatchLinkDefaults(invoice2aMatchLink);
			//AssertAPInvoiceShowsAsPaid(Creditor2Inv2);

			#endregion

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", true, charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", true, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", true, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", true, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", true, charge6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region Can't post AR invoices from a job with invoicing on hold

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCannotPostARInvoicesFromJobWithInvoicingOnHold()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			CreateExchangeRate(job, USD, .7M);

			Charge charge1 = CreateCharge(job, CC1, "Charge code 1 tries to make an AP/AR invoice and an AP payment and a CFX line.", USD, 100M, Creditor1, USD, 800M, LocalClient);
			Charge charge2 = CreateCharge(job, CC8, "Charge code 2 tries to make an AP/AR credit note.", AUD, -50M, Creditor3, AUD, -50M, LocalClient2);

			USDBankAccount.AB_ChequeNumDigits = 5;
			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPPaymentInfo(charge1, ReceiptTypes.Cheque, USDBankAccount, "1");
			SetAPInvoiceInfo(charge2, "3", Now.AddDays(10), Now.AddDays(20));
			Factory.Save();

			InvoicingPostManager postManager = new InvoicingPostManager(job);
			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();
			string generatedTransactions = GetContentsStringPayable(transactions);

			string message = "No AR transactions should have been created, because the Job has invoicing on hold. " + generatedTransactions;
			AssertEquals(message, false, postManager.Poster.ContainsInvoice(USD, Creditor1)); // charge 1
			AssertEquals(message, false, postManager.Poster.ContainsInvoice(AUD, Creditor3)); // charge 2

			message = "Invoicing on hold should prohibit AR transactions, but not AP transactions. " + generatedTransactions;
			AssertEquals(message, true, transactions.ContainsAPInvoice(Creditor1.OH_Code, charge1.JR_APInvoiceNum)); // charge 1
			AssertEquals(message, true, transactions.ContainsAPPaymentApproval(Creditor1.OH_Code, USDBankAccount.AB_Code, ReceiptTypes.Cheque, "00001", job.JH_JobNum)); // charge 1
			AssertEquals(message, true, transactions.ContainsAPCreditNote(Creditor3.OH_Code, charge2.JR_APInvoiceNum)); // charge 2
			AssertEquals("Expected exactly 3 transactions to be created. " + generatedTransactions, 3, transactions.Count + transactions.GetAllAPPaymentApprovals().Length);
		}

		#endregion

		#region Possible to post revenue for a previously posted apportioned cost

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCanPostRevenueForPostedApportionedCost()
		{
			ForwardingConsol consol = CreateConsol("AUSYD", "USLAX", "Y00001000");
			ForwardingShipment shipment1 = consol.Shipments.AddNew();

			Job job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			job1.LocalChargesPK = LocalClient.PK;
			job1.AgentCollectPK = Agent.PK;
			job1.PlugInData = shipment1;
			job1.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job1.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OSCostAmount = 1m;
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = "1";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = LocalClient.PK;
			Factory.Save();
			job1.Charges[0].JR_InvoiceType = "";
			cost.CalculationStrategy.UpdateApportionmentChargesListing();

			job1.Charges[0].JR_InvoiceType = "FIN";

			AssertEquals(false, cost.ApportionmentCharges[0].HasErrors);
			AssertEquals(false, job1.HasErrors);

			Factory.Save();

			// post apportioned cost
			var jobs = new[] { job1 };
			ConsolInvoicingPostManager creator = new ConsolInvoicingPostManager(Factory, jobs, consol, apps);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Costs);
			AssertEquals("Should have posted apportioned cost.", true, transactions.ContainsAPInvoice(Creditor1.OH_Code, "1"));
			AssertEquals("Should have posted only apportioned cost.", 1, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);

			// post revenue
			InvoicingPostManager invoicingCreator = new InvoicingPostManager(job1);
			transactions = invoicingCreator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Should have posted revenue.", true, invoicingCreator.Poster.ContainsInvoice(AUD, LocalClient));
		}

		#endregion

		#region Comment Charge Code to be Included in the Invoice

		public void TestCommentChargeCodePosting()
		{
			// To bypass JobCharge validation
			TestObjectCreator.LocalClient.MiscServ.OM_ARWHTApplicable = false;
			TestObjectCreator.LocalClient.MiscServ.OM_APWHTApplicable = false;
			TestObjectCreator.Creditor1.MiscServ.OM_ARWHTApplicable = false;
			TestObjectCreator.Creditor1.MiscServ.OM_APWHTApplicable = false;
			TestObjectCreator.Creditor2.MiscServ.OM_ARWHTApplicable = false;
			TestObjectCreator.Creditor2.MiscServ.OM_APWHTApplicable = false;

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, TestObjectCreator.CommentChargeCode, "Comment Charge 1", AUD, 0M, Creditor2, AUD, 0M, LocalClient);

			Factory.Save();

			InvoicingPostManager creator = new InvoicingPostManager(job);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			InvoicingBase[] currentInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, currentInvoices.Length);
			ARInvoice currentInvoice = (ARInvoice)currentInvoices[0];
			AssertEquals("Invoice Line Count", 2, currentInvoice.Lines.Count);

			TransactionLine cC1Line = currentInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertNotNull(cC1Line);
			AssertEquals(150M, cC1Line.AL_LineAmount);
			TransactionLine commentLine = currentInvoice.FindTransactionLine("REV", TestObjectCreator.CommentChargeCode, job.PK);
			AssertNotNull(commentLine);
			AssertEquals(0M, commentLine.AL_LineAmount);
		}

		public void TestCommentChargeCodeOnlyJobInvoice_WhenAllowZeroValueARInvoices()
		{
			AssertCommentChargeCodeOnlyJobInvoice(true);
		}

		public void TestCommentChargeCodeOnlyJobInvoice_WhenDisallowZeroValueARInvoices()
		{
			AssertCommentChargeCodeOnlyJobInvoice(false);
		}

		public void AssertCommentChargeCodeOnlyJobInvoice(bool allowZeroValueARInvoices)
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowZeroValueARInvoices);

			// To bypass JobCharge validation
			TestObjectCreator.LocalClient.MiscServ.OM_ARWHTApplicable = false;
			TestObjectCreator.LocalClient.MiscServ.OM_APWHTApplicable = false;
			TestObjectCreator.Creditor2.MiscServ.OM_ARWHTApplicable = false;
			TestObjectCreator.Creditor2.MiscServ.OM_APWHTApplicable = false;

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);

			CreateCharge(job, TestObjectCreator.CommentChargeCode, "Comment Charge 1", AUD, 0M, Creditor2, AUD, 0M, LocalClient);
			Factory.Save();

			InvoicingPostManager creator = new InvoicingPostManager(job);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			if (allowZeroValueARInvoices)
			{
				AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

				InvoicingBase[] currentInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
				AssertEquals(1, currentInvoices.Length);
				AssertEquals(0m, currentInvoices[0].AH_OSTotal);
				AssertEquals(0m, currentInvoices[0].AH_InvoiceAmount);

				Factory.Save();
				TransactionHeader invoiceRaised = new BusinessObjectFactory().Load(typeof(TransactionHeader), currentInvoices[0].PK) as TransactionHeader;
				AssertNotNull(invoiceRaised);
				AssertEquals(0m, invoiceRaised.AH_OSTotal);
				AssertEquals(0m, invoiceRaised.AH_InvoiceAmount);
			}
			else
			{
				AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
			}
		}

		#endregion

		#region TEST: Invoices and CreditNotes without Lines are detected as Critical Errors

		public void TestInvoicesAndCreditNotesWithoutLinesAreDetectedAsCriticalErrors()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var shipment1Job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, null, 0);

			InvoicingPostManagerForTest testPostManager = new InvoicingPostManagerForTest(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesAndCreditNotesWithoutLines);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			var aPInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);

			transactions.AddAPInvoice(aPInvoice, TestObjectCreator.AALSHI.OH_Code, aPInvoice.AH_TransactionNum);
			aPInvoice.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			InvoiceCreditNoteWithoutLinesError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			transactions = new TransactionCreatorHashtable();
			var aPCreditNote = TestObjectCreator.CreateAPCreditNote("222", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "desc");

			transactions.AddAPCreditNote(aPCreditNote, TestObjectCreator.AALSHI.OH_Code, aPCreditNote.AH_TransactionNum);
			aPCreditNote.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			InvoiceCreditNoteWithoutLinesError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			transactions = new TransactionCreatorHashtable();
			var aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("111", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);

			transactions.AddARInvoice(aRInvoice);
			aRInvoice.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			InvoiceCreditNoteWithoutLinesError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			transactions = new TransactionCreatorHashtable();
			var aRCreditNote = TestObjectCreator.CreateARCreditNote("222", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "desc");

			transactions.AddARInvoice(aRCreditNote);
			aRCreditNote.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", InvoiceCreditNoteWithoutLinesError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesAndCreditNotesWithoutLines);
			shipment1Job.Dispose();
		}

		void TestPostManager_InvoicesAndCreditNotesWithoutLines(object sender, CriticalPostingErrorEventArgs e)
		{
			if (e is CriticalTransactionPostingErrorEventArgs)
			{
				foreach (INotification notification in ((CriticalTransactionPostingErrorEventArgs)e).Header.RowErrors)
				{
					if (notification.Message.Contains(" has no lines and cannot be posted"))
					{
						InvoiceCreditNoteWithoutLinesError = true;
					}
				}
			}
		}

		bool InvoiceCreditNoteWithoutLinesError;

		class InvoicingPostManagerForTest : InvoicingPostManager
		{
			public InvoicingPostManagerForTest(Job job)
				: base(job)
			{
			}

			public void CheckForCriticalErrors_Exposed(TransactionCreatorHashtable transactions)
			{
				CheckForCriticalErrors(transactions);
			}
		}

		#endregion

		#region TEST: Invoices with inactive branch are detected as Critical Errors

		public void TestInvoiceswithInactiveBranchAreDetectedAsCriticalErrors()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var shipment1Job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, null, 0);

			var testPostManager = new InvoicingPostManagerForTest(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithInactiveBranch);
			Factory.Save();

			shipment1Job.Branch.GB_IsActive = false;
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);

			transactions.AddAPInvoice(apInvoice1, TestObjectCreator.AALSHI.OH_Code, apInvoice1.AH_TransactionNum);
			transactions.AddAPInvoice(apInvoice2, TestObjectCreator.AALSHI.OH_Code, apInvoice2.AH_TransactionNum);
			apInvoice1.Lines.RemoveAndDeleteAll();
			apInvoice2.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", hasInvoicesWithInactiveBranchError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			hasInvoicesWithInactiveBranchError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithInactiveBranch);
			shipment1Job.Dispose();
		}

		void TestPostManager_InvoicesWithInactiveBranch(object sender, CriticalPostingErrorEventArgs e)
		{
			var eventArgs = e as CriticalTransactionPostingErrorEventArgs;
			if (eventArgs != null)
			{
				hasInvoicesWithInactiveBranchError = eventArgs.BusinessEntities.Cast<TransactionHeader>().All(header => header.RowErrors.Cast<INotification>().Any(m =>
																								m.Message.Contains(string.Format("Invoice cannot be posted against inactive branch '{0}'. Please check the job branch.", header.Branch.GB_Code))));
			}
		}

		bool hasInvoicesWithInactiveBranchError;

		#endregion

		#region TEST: Invoices with future invoice dates are detected (In Portugal)

		[TestDate(2018, 07, 24)]
		public void TestCheckIfInvoiceDateIsInFuture()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var shipment1Job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, null, 0);

			var testPostManager = new InvoicingPostManagerForTest(shipment1Job);
			testPostManager.OnUserWarningNotification += new EventHandler<UserMessageEventArgs>(DummyOnDetectFutureDateHandler);
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var aPInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice1.AH_InvoiceDate = ZDateTime.Today.AddDays(5);

			var aPInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice2.AH_InvoiceDate = ZDateTime.Today.AddDays(1);

			transactions.AddAPInvoice(aPInvoice1, TestObjectCreator.AALSHI.OH_Code, aPInvoice1.AH_TransactionNum);
			transactions.AddAPInvoice(aPInvoice2, TestObjectCreator.AALSHI.OH_Code, aPInvoice2.AH_TransactionNum);
			aPInvoice1.Lines.RemoveAndDeleteAll();
			aPInvoice2.Lines.RemoveAndDeleteAll();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				testPostManager.CheckIfInvoiceDateIsInTheFuture(transactions);
				AssertEquals(@"Invoice Date is in the future. Please check your system date setting.
AP Invoice for Organization AALSHI has future invoice date '29-Jul-18'.
AP Invoice for Organization AALSHI has future invoice date '25-Jul-18'.", warningMessage);
				warningMessage = string.Empty;
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				testPostManager.CheckIfInvoiceDateIsInTheFuture(transactions);
				AssertEquals("", warningMessage);
			}

			testPostManager.OnUserWarningNotification -= new EventHandler<UserMessageEventArgs>(DummyOnDetectFutureDateHandler);
			shipment1Job.Dispose();
		}

		void DummyOnDetectFutureDateHandler(object sender, UserMessageEventArgs e)
		{
			warningMessage = e.Message;
		}

		string warningMessage = string.Empty;

		#endregion

		#region TEST: Invoices with empty compliance sub type are detected as Critical Errors (In italy)

		public void TestInvoiceswithEmptyComplianceSubTypeAreDetectedAsCriticalErrors()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var shipment1Job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, null, 0);

			var testPostManager = new InvoicingPostManagerForTest(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithEmptyComplianceSubType);
			Factory.Save();

			shipment1Job.Branch.GB_IsActive = false;
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var aPInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice1.AH_ComplianceSubType = ZString.Empty;

			var aPInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice2.AH_ComplianceSubType = ZString.Empty;

			transactions.AddAPInvoice(aPInvoice1, TestObjectCreator.AALSHI.OH_Code, aPInvoice1.AH_TransactionNum);
			transactions.AddAPInvoice(aPInvoice2, TestObjectCreator.AALSHI.OH_Code, aPInvoice2.AH_TransactionNum);
			aPInvoice1.Lines.RemoveAndDeleteAll();
			aPInvoice2.Lines.RemoveAndDeleteAll();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				((AccountingConfigurationRegistry.CountryEnabledBooleanRegistryItemImpl)AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Inner).ClearCacheForTestOnly();
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Assert(AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value);
				testPostManager.CheckForCriticalErrors_Exposed(transactions);
				Assert("Error should be found", hasInvoicesWithEmptyComplianceSubTypeError);
				Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
				hasInvoicesWithEmptyComplianceSubTypeError = false;
				ExceptionReporterTestListener.Instance.Clear();
				CargoWise.Common.ErrorReporter.Clear();
			}

			foreach (TransactionHeader inv in transactions.Values)
			{
				inv.ClearAllNotifications();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				((AccountingConfigurationRegistry.CountryEnabledBooleanRegistryItemImpl)AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Inner).ClearCacheForTestOnly();
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				Assert(!AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value);
				testPostManager.CheckForCriticalErrors_Exposed(transactions);
				Assert("Error should not be found", !hasInvoicesWithEmptyComplianceSubTypeError);
				ExceptionReporterTestListener.Instance.Clear();
				CargoWise.Common.ErrorReporter.Clear();
			}

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithEmptyComplianceSubType);
			shipment1Job.Dispose();
		}

		void TestPostManager_InvoicesWithEmptyComplianceSubType(object sender, CriticalPostingErrorEventArgs e)
		{
			var eventArgs = e as CriticalTransactionPostingErrorEventArgs;
			if (eventArgs != null)
			{
				hasInvoicesWithEmptyComplianceSubTypeError = eventArgs.BusinessEntities.Cast<TransactionHeader>().All(header => header.RowErrors.Cast<INotification>().Any(m =>
											m.Message.Contains("You cannot post invoices with a blank compliance sub-type.")));
			}
		}

		bool hasInvoicesWithEmptyComplianceSubTypeError;

		#endregion

		#region TEST: Invoices with invalid agreed payment methods override are detected as Critical Errors

		public void TestInvoiceswithInvalidAgreedPaymentMethodOverrideAreDetectedAsCriticalErrors()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var shipment1Job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, null, 0);

			var testPostManager = new InvoicingPostManagerForTest(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithInvalidAgreedPaymentMethodOverride);
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var aPInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice1.AH_AgreedPaymentMethodOverride = "XXX";

			var aPInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			aPInvoice2.AH_AgreedPaymentMethodOverride = "XXX";

			transactions.AddAPInvoice(aPInvoice1, TestObjectCreator.AALSHI.OH_Code, aPInvoice1.AH_TransactionNum);
			transactions.AddAPInvoice(aPInvoice2, TestObjectCreator.AALSHI.OH_Code, aPInvoice2.AH_TransactionNum);
			aPInvoice1.Lines.RemoveAndDeleteAll();
			aPInvoice2.Lines.RemoveAndDeleteAll();

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", hasInvoicesWithInvalidAgreedPaymentMethodOverrideError);
			Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
			hasInvoicesWithInvalidAgreedPaymentMethodOverrideError = false;
			ExceptionReporterTestListener.Instance.Clear();
			CargoWise.Common.ErrorReporter.Clear();

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithInvalidAgreedPaymentMethodOverride);
			shipment1Job.Dispose();
		}

		void TestPostManager_InvoicesWithInvalidAgreedPaymentMethodOverride(object sender, CriticalPostingErrorEventArgs e)
		{
			var eventArgs = e as CriticalTransactionPostingErrorEventArgs;
			if (eventArgs != null)
			{
				hasInvoicesWithInvalidAgreedPaymentMethodOverrideError = eventArgs.BusinessEntities.Cast<TransactionHeader>().All(header => header.RowErrors.Cast<INotification>().Any(m =>
						m.Message.Contains("Organization AALSHI cannot post Invoice against invalid agreed payment method 'XXX'. Please check the organization setup and registry setting at Organization > AR/AP > Agreed Payment Method.")));
			}
		}

		bool hasInvoicesWithInvalidAgreedPaymentMethodOverrideError;

		#endregion

		#region TEST: Receivable Invoices with zero balance amount are detected as Critical Errors When disallowed

		public void TestARInvoicesWithZeroBalanceAmountAreDetectedAsCriticalErrorsWhenDisallowed()
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var hasInvoicesWithZeroBalanceAmountError = false;

			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var shipment1Job = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, null, 0);

			var testPostManager = new InvoicingPostManagerForTest(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithZeroBalanceAmount);
			Factory.Save();

			var transactions = new TransactionCreatorHashtable();
			var aRInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var line1 = TestObjectCreator.CreateInvoiceLine(aRInvoice, TestObjectCreator.AUD, 1m, 100m);
			var line2 = TestObjectCreator.CreateInvoiceLine(aRInvoice, TestObjectCreator.AUD, 1m, -100m);

			transactions.AddARInvoice(aRInvoice);

			testPostManager.CheckForCriticalErrors_Exposed(transactions);
			Assert("Error should be found", hasInvoicesWithZeroBalanceAmountError);
			hasInvoicesWithZeroBalanceAmountError = false;

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_InvoicesWithZeroBalanceAmount);
			shipment1Job.Dispose();

			void TestPostManager_InvoicesWithZeroBalanceAmount(object sender, CriticalPostingErrorEventArgs e)
			{
				var eventArgs = e as CriticalTransactionPostingErrorEventArgs;
				if (eventArgs != null)
				{
					hasInvoicesWithZeroBalanceAmountError = eventArgs.BusinessEntities.Cast<TransactionHeader>().All(header => header.RowErrors.Cast<INotification>().Any(m =>
																									m.Message.Contains(string.Format("Transaction Total cannot be zero. This is controlled by the registry: {0}", AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.HumanReadableRegistryPath()))));
				}
			}
		}

		#endregion

		#region Cancel Posting

		public void TestCancelPosting()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge2.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, LocalClient);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;

			InvoicingPostManager creator = new InvoicingPostManager(job);
			creator.SetCancelPostingForTestOnly(true);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			creator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			AssertEquals("Shouldn't be any transactions posted", 0, creator.Poster.PostedInvoices.Count);

			creator.SetCancelPostingForTestOnly(false);
			creator.CreateTransactions(JobInvoicingPostingOption.LocalClient);
			Assert("Shouldn't be any transactions posted", creator.Poster.PostedInvoices.Count > 0);
		}

		#endregion

		#region TEST: Post AgencyBillOfLadiing LocalPrePaid Charges split by Sell Invoice Currency

		public void TestPostAgencyBillOfLadiingLocalPrePaidChargesSplitBySellInvoiceCurrency()
		{
			var agencyBOLJob = TestObjectCreator.CreateJob(TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.AgencyBillOfLading), false);
			var localPrePaidChargeWithSellInvoiceCurrency = agencyBOLJob.Charges.AddNew();
			localPrePaidChargeWithSellInvoiceCurrency.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			localPrePaidChargeWithSellInvoiceCurrency.JR_AC = TestObjectCreator.FRT.PK;
			localPrePaidChargeWithSellInvoiceCurrency.JR_RX_NKSellCurrency = TestObjectCreator.AUD.Code;
			localPrePaidChargeWithSellInvoiceCurrency.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
			localPrePaidChargeWithSellInvoiceCurrency.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
			localPrePaidChargeWithSellInvoiceCurrency.JR_OSSellAmt = 100m;
			var exRate = agencyBOLJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(x => x.JF_RX_NKRateCurrency == TestObjectCreator.USD.Code && x.JF_OrgType == "DEB");
			AssertNotNull(exRate);
			exRate.JF_BaseRate = 0.6642;

			var localPrePaidChargeWithForeignCurrency = agencyBOLJob.Charges.AddNew();
			localPrePaidChargeWithForeignCurrency.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			localPrePaidChargeWithForeignCurrency.JR_AC = TestObjectCreator.CC1.PK;
			localPrePaidChargeWithForeignCurrency.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
			localPrePaidChargeWithForeignCurrency.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			localPrePaidChargeWithForeignCurrency.JR_OSSellAmt = 100m;

			Factory.Save();

			var testPostManager = new InvoicingPostManager(agencyBOLJob);

			var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);
			TransactionCreatorHashtable transactions;

			using (ObjectFactory.Substitute(registry.Object))
			{
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(true);
				registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);

				transactions = testPostManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			}

			AssertEquals("Should have two Invoices for the different currencies (USD and AUD)", 2, transactions.Count);
			AssertNoExceptionThrown("Should not throw a Critical Validation Error about Line Amount is not equal to Charge Amount", () => Factory.Save());
		}

		#endregion

		public void TestRevenueOptionTriggersREVBillingEventForGatewayConsol()
		{
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			var job = TestObjectCreator.CreateJob(setup.s0001, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
				charge.JR_OSSellAmt = 3333m;
				charge.JR_RX_NKSellCurrency = "AUD";
				Factory.Save();

				var transactionCreator = new InvoicingPostManagerForTest(gatewayJob);
				_ = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Revenue);

				var events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
				AssertEquals(1, events.Count());
				AssertEquals(AccBillingEvents.Codes.RevenuePosted, events.First().EventCode);

				Factory.Save();
				Assert("Should not post cost side", !charge.IsCostPosted);
				Assert("Should post sell side", charge.IsRevenuePosted);

				events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
				AssertEquals(0, events.Count());

				var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
				AssertEquals("BillingCounter", 3, summary.BilledItemCount);
				AssertEquals("Billed Shipments", "S0001, S0002, S0003", summary.BilliedShipmentNumbersAsCSV);
				AssertEquals("BillingHeader", 1, summary.BillingHeaders.Count());

				var header = summary.BillingHeaders.First();
				AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
				AssertEquals("Billing Counter", 3, header.ABH_BillingCounter);
				AssertEquals("Billing EventType", "REV", header.ABH_EventType);
				AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
				AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
				AssertEquals("Billing Reference Number", "00001000", header.ABH_InternalReferenceNumber);
				AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
				AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
				AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

				var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
				AssertEquals("BillingLines", 3, lineItems.Length);

				var line = lineItems[0];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0001.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0001.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[1];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0002.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0002.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[2];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0003.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0003.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
			}
		}

		public void TestCostOptionTriggersCSTBillingEventForGatewayConsol()
		{
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			var job = TestObjectCreator.CreateJob(setup.s0001, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;

			using (var gatewayJob = TestObjectCreator.CreateJob(setup.gC0002))
			{
				var charge = gatewayJob.Charges.AddNew();
				charge.JR_AC = TestObjectCreator.FRT.PK;
				charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
				charge.JR_OSCostAmt = 3333m;
				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_APInvoiceNum = "TR00001";
				charge.JR_APInvoiceDate = ZDateTime.Today;
				Factory.Save();

				var transactionCreator = new InvoicingPostManagerForTest(gatewayJob);
				_ = transactionCreator.CreateTransactions(JobInvoicingPostingOption.Costs);

				var events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
				AssertEquals(1, events.Count());
				AssertEquals(AccBillingEvents.Codes.CostPosted, events.First().EventCode);

				Factory.Save();
				Assert("Should not post cost side", charge.IsCostPosted);
				Assert("Should post sell side", !charge.IsRevenuePosted);

				events = AccBillingEventCollector.GetInstance(Factory).GetEvents(AccBillingCodes.GatewayBilling);
				AssertEquals(0, events.Count());

				var summary = AccBillingHandler.GetBillingSummary(Factory, AccBillingCodes.GatewayBilling, setup.gC0002.PK, "JK");
				AssertEquals("BillingCounter", 3, summary.BilledItemCount);
				AssertEquals("Billed Shipments", "S0001, S0002, S0003", summary.BilliedShipmentNumbersAsCSV);
				AssertEquals("BillingHeader", 1, summary.BillingHeaders.Count());

				var header = summary.BillingHeaders.First();
				AssertEquals("Billing Code", "GSH", header.ABH_BillingCode);
				AssertEquals("Billing Counter", 3, header.ABH_BillingCounter);
				AssertEquals("Billing EventType", "CST", header.ABH_EventType);
				AssertEquals("Billing Company PK", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
				AssertEquals("Billing Staff", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
				AssertEquals("Billing Reference Number", "00001000", header.ABH_InternalReferenceNumber);
				AssertEquals("Billing Parent", setup.gC0002.PK, header.ABH_ParentId);
				AssertEquals("Billing Parent Reference Number", setup.gC0002.JK_UniqueConsignRef, header.ABH_ParentReferenceNumber);
				AssertEquals("Billing Parent Table Code", "JK", header.ABH_ParentTableCode);

				var lineItems = header.BillingItems.OfType<AccBillingItem>().OrderBy(b => b.ABI_ParentReferenceNumber).ToArray();
				AssertEquals("BillingLines", 3, lineItems.Length);

				var line = lineItems[0];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0001.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0001.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[1];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0002.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0002.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);

				line = lineItems[2];
				AssertEquals("Line Parent PK", header.PK, line.ABI_ABH);
				AssertEquals("Shipment PK", setup.s0003.PK, line.ABI_ParentId);
				AssertEquals("Shipment Number", setup.s0003.JS_UniqueConsignRef, line.ABI_ParentReferenceNumber);
				AssertEquals("Shipment Table Code", "JS", line.ABI_ParentTableCode);
			}
		}

		public void TestErrorOnDuplicateAPInvoicePosting()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = Creditor1.PK;
			Factory.Save();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Add(shipment1Job);

			Charge postingCharge = shipment1Job.Charges.AddNew();
			postingCharge.JR_AC = CC1.PK;
			postingCharge.JR_OSCostAmt = 500m;
			postingCharge.JR_OH_CostAccount = Creditor1.PK;
			postingCharge.JR_APInvoiceNum = invoice.AH_TransactionNum;
			postingCharge.JR_APInvoiceDate = ZDateTime.Now;
			Factory.Save();

			InvoicingPostManager testPostManager = new InvoicingPostManager(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.Costs);

			AssertEquals("ExceptionReporter should have no exceptions caught", 0, ExceptionReporterTestListener.Instance.Count);

			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			Assert(DuplicateAPInvoiceError);
			DuplicateAPInvoiceError = false;
			shipment1Job.Charges[0].JR_APInvoiceNum = "XYZASDEVF";
			testPostManager = new InvoicingPostManager(shipment1Job);
			testPostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.Costs);
			testPostManager.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_DuplicateAPTransactionNumber);
			Assert(!DuplicateAPInvoiceError);
		}

		public void TestSIVEventIsCreatedOnInvoiceCreationAndReverse()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var shipmentJob = Job.CreateWithMutex(Factory, shipment);
			shipmentJob.PlugInData = shipment;
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var postingCharge = shipmentJob.Charges.AddNew();
			postingCharge.JR_AC = CC1.PK;
			postingCharge.JR_OSCostAmt = 500m;
			postingCharge.JR_OH_CostAccount = Creditor1.PK;
			postingCharge.JR_OH_SellAccount = LocalClient.PK;
			Factory.Save();

			var testPostManager = new InvoicingPostManager(shipmentJob);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Precondition: Should post 1 invoice", 1, testPostManager.Poster.PostedInvoices.Count);
			var postedInvoice = testPostManager.Poster.PostedInvoices[0];
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, shipmentJob.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("1 log found with FIN INV", 1, retrievedLogs.Count(x => x.SL_Reference == "FIN INV S1"));

			var reverser = new JobInvoicingReverser(shipmentJob);
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("Precondition: ContinueWithSave", reverser.ContinueWithSave);
			reverser.ReversingFactory.Save();

			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("1 log found with FIN CRD for reverse invoice", 1, retrievedLogs.Count(x => x.SL_Reference == "FIN CRD S1"));
		}

		public void TestSIVEventIsCreatedOnMultipleInvoicesCreationAndReverse()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var shipmentJob = Job.CreateWithMutex(Factory, shipment);
			shipmentJob.PlugInData = shipment;
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var postingCharge1 = shipmentJob.Charges.AddNew();
			postingCharge1.JR_AC = CC1.PK;
			postingCharge1.JR_OSSellAmt = 500m;
			postingCharge1.JR_OH_SellAccount = LocalClient.PK;
			postingCharge1.JR_InvoiceType = "FIN";

			var postingCharge2 = shipmentJob.Charges.AddNew();
			postingCharge2.JR_AC = CC1.PK;
			postingCharge2.JR_OSSellAmt = 500m;
			postingCharge2.JR_OH_SellAccount = LocalClient.PK;
			postingCharge2.JR_InvoiceType = "DBT";

			var postingCharge3 = shipmentJob.Charges.AddNew();
			postingCharge3.JR_AC = CC1.PK;
			postingCharge3.JR_OSSellAmt = 500m;
			postingCharge3.JR_OH_SellAccount = LocalClient2.PK;
			postingCharge3.JR_InvoiceType = "FIN";

			var postingCharge4 = shipmentJob.Charges.AddNew();
			postingCharge4.JR_AC = CC1.PK;
			postingCharge4.JR_OSSellAmt = 500m;
			postingCharge4.JR_OH_SellAccount = LocalClient2.PK;
			postingCharge4.JR_InvoiceType = "DBT";
			Factory.Save();

			var testPostManager = new InvoicingPostManager(shipmentJob);
			testPostManager.CreateTransactions(JobInvoicingPostingOption.All);
			AssertEquals("Precondition: Should post 4 invoice", 4, testPostManager.Poster.PostedInvoices.Count);
			var postedInvoices = testPostManager.Poster.PostedInvoices;
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, shipmentJob.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);

			AssertEquals("2 log found with FIN INV", 2, retrievedLogs.Count(x => x.SL_Reference == "FIN INV S1"));
			AssertEquals("2 log found with DBT INV", 2, retrievedLogs.Count(x => x.SL_Reference == "DBT INV S1"));

			var reverser = new JobInvoicingReverser(shipmentJob);
			reverser.ReverseAllInvoices("Incorrect Data Entry", "IDE");
			Assert("Precondition: ContinueWithSave", reverser.ContinueWithSave);
			reverser.ReversingFactory.Save();

			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("2 log found with FIN CRD for reverse invoice", 2, retrievedLogs.Count(x => x.SL_Reference == "FIN CRD S1"));
			AssertEquals("2 log found with DBT CRD for reverse invoice", 2, retrievedLogs.Count(x => x.SL_Reference == "DBT CRD S1"));
		}

		void TestPostManager_DuplicateAPTransactionNumber(object sender, CriticalPostingErrorEventArgs e)
		{
			DuplicateAPInvoiceError = true;
		}

		bool DuplicateAPInvoiceError;

		public void TestRollBackPosting()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge2.JR_PreventInvoicePrintGrouping = ZBool.True;

			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, LocalClient);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Factory.Save();

			int numberOfTransactions = Factory.GetDatabaseCount(typeof(AccTransactionHeader));

			InvoicingPostManager creator = new InvoicingPostManager(job);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("Should be some transactions posted", creator.Poster.PostedInvoices.Count > 0);

			creator.RollbackPosting();

			AssertEquals("Posting should be cancelled", true, creator.CancelPosting);
		}

		public void TestAPCreditNoteIgnoresBankDetails()
		{
			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, -100M, Creditor1, AUD, -100M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, -200M, Creditor2, AUD, -200M, LocalClient);

			charge1.JR_APInvoiceNum = "1";
			charge1.JR_APInvoiceDate = ZDateTime.Today;
			charge1.JR_PaymentType = "CSH";
			charge1.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			charge1.JR_ChequeNo = "CASH";

			charge2.JR_APInvoiceNum = "2";
			charge2.JR_APInvoiceDate = ZDateTime.Today;
			charge2.JR_PaymentType = "CHQ";
			charge2.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			charge2.JR_AK = TestObjectCreator.AUDChequeBook.PK;

			job.RunPreSaveValidation();
			Assert("JR_OSCostAmt should have warning", charge1.JR_OSCostAmtInfo.HasWarning("A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			Assert("JR_OSCostAmt should have warning", charge1.JR_PaymentTypeInfo.HasWarning("A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			Assert("JR_OSCostAmt should have warning", charge2.JR_OSCostAmtInfo.HasWarning("A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			Assert("JR_OSCostAmt should have warning", charge2.JR_PaymentTypeInfo.HasWarning("A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			Factory.Save();

			int numberOfTransactions = Factory.GetDatabaseCount(typeof(AccTransactionHeader));

			InvoicingPostManager creator = new InvoicingPostManager(job);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.All);
			var aPCredNotes = transactions.GetAllAPInvoicesAndCreditNotes();
			Job temp = (Job)aPCredNotes.ElementAt(0).Job;
			var charge = temp.Charges[0];
			Assert("Payment Type should be empty", charge.JR_PaymentType.Equals(""));
			Assert("Cheque Number should be empty", charge.JR_ChequeNo.Equals(""));
			Assert("Bank account should be empty", charge.JR_AB.Equals(ZGuid.Empty));
			Assert("Cheque Book should be empty", charge.JR_AK.Equals(ZGuid.Empty));

			charge = temp.Charges[1];
			Assert("Payment Type should be empty", charge.JR_PaymentType.Equals(""));
			Assert("Cheque Number should be empty", charge.JR_ChequeNo.Equals(""));
			Assert("Bank account should be empty", charge.JR_AB.Equals(ZGuid.Empty));
			Assert("Cheque Book should be empty", charge.JR_AK.Equals(ZGuid.Empty));
		}

		#region Implementation

		void SetupCharges()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(Job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(Job, GBP, .4M);

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge2 = CreateCharge(Job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge3 = CreateCharge(Job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge4 = CreateCharge(Job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge5 = CreateCharge(Job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge6 = CreateCharge(Job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge7 = CreateCharge(Job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
		}

		void SetupAPInvoiceInfo()
		{
			SetAPInvoiceInfo(Charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge7, "1", Now.AddDays(10), Now.AddDays(20));
		}

		void InitializeWIPAccruals()
		{
			Charge1WIP = Charge1.WIP;
			Charge2WIP = Charge2.WIP;
			Charge3WIP = Charge3.WIP;
			Charge4WIP = Charge4.WIP;
			Charge5WIP = Charge5.WIP;
			Charge6WIP = Charge6.WIP;
			Charge7WIP = Charge7.WIP;

			Charge1Accrual = Charge1.Accrual;
			Charge2Accrual = Charge2.Accrual;
			Charge3Accrual = Charge3.Accrual;
			Charge5Accrual = Charge5.Accrual;
			Charge6Accrual = Charge6.Accrual;
			Charge7Accrual = Charge7.Accrual;
		}

		#region Registry Setup

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		protected void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Value;

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 250, AuthorisationCodes.NoApprovalRequired);
			var above = GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 250, AuthorisationCodes.FirstApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		PaymentTwelveLevelAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;

		#endregion

		Job Job;
		Charge Charge1;
		Charge Charge2;
		Charge Charge3;
		Charge Charge4;
		Charge Charge5;
		Charge Charge6;
		Charge Charge7;

		AccTransactionLines Charge1WIP;
		AccTransactionLines Charge2WIP;
		AccTransactionLines Charge3WIP;
		AccTransactionLines Charge4WIP;
		AccTransactionLines Charge5WIP;
		AccTransactionLines Charge6WIP;
		AccTransactionLines Charge7WIP;

		AccTransactionLines Charge1Accrual;
		AccTransactionLines Charge2Accrual;
		AccTransactionLines Charge3Accrual;
		AccTransactionLines Charge5Accrual;
		AccTransactionLines Charge6Accrual;
		AccTransactionLines Charge7Accrual;

		#endregion
	}
}
