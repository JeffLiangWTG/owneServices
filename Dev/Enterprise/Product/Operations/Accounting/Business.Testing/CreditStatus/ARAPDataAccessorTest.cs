using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CreditStatus
{
	public class ARAPDataAccessorTest : TestCaseWithFactory
	{
		public void TestGetStandardAROverdueAmount()
		{
			decimal actual = new ARAPDataAccessor().GetStandardAROverdueAmount(AROrgHeaderPK);
			AssertEquals("AR Standard Overdue amount", 271.5M, actual);
		}

		public void TestGetDisbursementAROverdueAmount()
		{
			decimal actual = new ARAPDataAccessor().GetDisbursementAROverdueAmount(AROrgHeaderPK);
			AssertEquals("AR Disbursement Overdue amount", 1056.6M, actual);
		}

		public void TestGetLastReceipt()
		{
			decimal actual = new ARAPDataAccessor().GetLastReceipt(AROrgHeaderPK);
			AssertEquals("AR Org Last Receipt", 150.00M, actual);
		}

		public void TestGetLastPayment()
		{
			decimal actual = new ARAPDataAccessor().GetLastPayment(APOrgHeaderPK);
			AssertEquals("AP Org Last Payment", 300.00M, actual);
		}

		public void TestGetLastReceiptDate()
		{
			DateTime actual = new ARAPDataAccessor().GetLastReceiptDate(AROrgHeaderPK);
			AssertEquals("Last Sale Date", CurrentPeriodDate.Date, actual.Date);
		}

		public void TestGetLastPaymentDate()
		{
			DateTime actual = new ARAPDataAccessor().GetLastPaymentDate(APOrgHeaderPK);
			AssertEquals("Last Purchase Date", CurrentPeriodDate.Date, actual.Date);
		}

		public void TestGetLastPurchase()
		{
			decimal actual = new ARAPDataAccessor().GetLastPurchase(APOrgHeaderPK);
			AssertEquals("Last Purchase", 952.00M, actual);
		}

		public void TestGetLastSale()
		{
			decimal actual = new ARAPDataAccessor().GetLastSale(AROrgHeaderPK);
			AssertEquals("Last Sale", 476.00M, actual);
		}

		public void TestGetCurrentARAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetCurrentARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", 215.00M, actual);

			actual = new ARAPDataAccessor().GetCurrentARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", 215.00M, actual);

			actual = new ARAPDataAccessor().GetCurrentARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 215.00M, actual);
		}

		public void TestGetCurrentARDueAgeOutstandingAmountWithReceipt()
		{
			var receipt = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			receipt.AH_OH = AROrgHeaderPK;
			receipt.AH_DueDate = ZDateTime.Empty;
			Factory.Save();

			var actual = new ARAPDataAccessor().GetCurrentARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 115.00M, actual);
		}

		public void TestGetCurrentAPAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetCurrentAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AP Period Oustanding Amount", 580.00M, actual);

			actual = new ARAPDataAccessor().GetCurrentAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AP Period Oustanding Amount", 580.00M, actual);

			actual = new ARAPDataAccessor().GetCurrentAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AP Period Oustanding Amount", 580.00M, actual);
		}

		public void TestGetCurrentAPDueAgeOutstandingAmountWithPayment()
		{
			var payment = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Payment, LedgerTypes.AccountsPayable, 100m, TestObjectCreator.AUDBankAccount2.PK);
			payment.AH_OH = APOrgHeaderPK;
			payment.AH_DueDate = ZDateTime.Empty;
			Factory.Save();

			var actual = new ARAPDataAccessor().GetCurrentAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 680.00M, actual);
		}

		public void TestGetSingleARAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetSingleARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", 20.00M, actual);

			actual = new ARAPDataAccessor().GetSingleARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", 20.00M, actual);

			actual = new ARAPDataAccessor().GetSingleARAgeOutStandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 20.00M, actual);
		}

		public void TestGetSingleAPAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetSingleAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", 240.00M, actual);

			actual = new ARAPDataAccessor().GetSingleAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", 240.00M, actual);

			actual = new ARAPDataAccessor().GetSingleAPAgeOutStandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 240.00M, actual);
		}

		public void TestGetTwoARAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetTwoARAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", 72.50M, actual);

			actual = new ARAPDataAccessor().GetTwoARAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", 72.50M, actual);

			actual = new ARAPDataAccessor().GetTwoARAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 72.50M, actual);
		}

		public void TestGetTwoAPAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetTwoAPAgeOutstandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", 255.00M, actual);

			actual = new ARAPDataAccessor().GetTwoAPAgeOutstandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", 255.00M, actual);

			actual = new ARAPDataAccessor().GetTwoAPAgeOutstandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 255.00M, actual);
		}

		public void TestGetThreeARAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetThreeARAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", -36.00M, actual);

			actual = new ARAPDataAccessor().GetThreeARAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", -36.00M, actual);

			actual = new ARAPDataAccessor().GetThreeARAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", -36.00M, actual);
		}

		public void TestGetThreeAPAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetThreeAPAgeOutstandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current AR Period Oustanding Amount", 218.00M, actual);

			actual = new ARAPDataAccessor().GetThreeAPAgeOutstandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current AR Period Oustanding Amount", 218.00M, actual);

			actual = new ARAPDataAccessor().GetThreeAPAgeOutstandingAmount(APOrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current AR Period Oustanding Amount", 218.00M, actual);
		}

		public void TestGetDSBCurrentAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetDSBCurrentAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current Disbursement Period Oustanding Amount", 440.10M, actual);

			actual = new ARAPDataAccessor().GetDSBCurrentAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current Disbursement Period Oustanding Amount", 440.10M, actual);

			actual = new ARAPDataAccessor().GetDSBCurrentAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current Disbursement Period Oustanding Amount", 440.10M, actual);
		}

		public void TestGetDSBSingleAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetDSBSingleAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Single Disbursement Period Oustanding Amount", 150.00M, actual);

			actual = new ARAPDataAccessor().GetDSBSingleAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Single Disbursement Period Oustanding Amount", 150.00M, actual);

			actual = new ARAPDataAccessor().GetDSBSingleAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Single Disbursement Period Oustanding Amount", 150.00M, actual);
		}

		public void TestGetDSBTwoAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetDSBTwoAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Two Disbursement Period Oustanding Amount", 122.50M, actual);

			actual = new ARAPDataAccessor().GetDSBTwoAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Two Disbursement Period Oustanding Amount", 122.50M, actual);

			actual = new ARAPDataAccessor().GetDSBTwoAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Two Disbursement Period Oustanding Amount", 122.50M, actual);
		}

		public void TestGetDSBThreeAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetDSBThreeAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Three Disbursement Period Oustanding Amount", 344.00M, actual);

			actual = new ARAPDataAccessor().GetDSBThreeAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Three Disbursement Period Oustanding Amount", 344.00M, actual);

			actual = new ARAPDataAccessor().GetDSBThreeAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Three Disbursement Period Oustanding Amount", 344.00M, actual);
		}

		public void TestGetBatchedCurrentAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetBatchedCurrentAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Current Batched Period Oustanding Amount", 320.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedCurrentAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Current Batched Period Oustanding Amount", 320.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedCurrentAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Current Batched Period Oustanding Amount", 320.00M, actual);
		}

		public void TestGetBatchedSingleAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetBatchedSingleAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Single Batched Period Oustanding Amount", 120.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedSingleAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Single Batched Period Oustanding Amount", 120.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedSingleAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Single Batched Period Oustanding Amount", 120.00M, actual);
		}

		public void TestGetBatchedTwoAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetBatchedTwoAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Two Batched Period Oustanding Amount", 90.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedTwoAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Two Batched Period Oustanding Amount", 90.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedTwoAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Two Batched Period Oustanding Amount", 90.00M, actual);
		}

		public void TestGetBatchedThreeAgeOutstandingAmount()
		{
			var actual = new ARAPDataAccessor().GetBatchedThreeAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.InvoiceDate);
			AssertEquals("Three Batched Period Oustanding Amount", 14.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedThreeAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.PostDate);
			AssertEquals("Three Batched Period Oustanding Amount", 14.00M, actual);

			actual = new ARAPDataAccessor().GetBatchedThreeAgeOutstandingAmount(AROrgHeaderPK, AccountingConstants.AgingOptions.DueDate);
			AssertEquals("Three Batched Period Oustanding Amount", 14.00M, actual);
		}

		public void TestGetPTDSales()
		{
			decimal actual = new ARAPDataAccessor().GetPTDSales(AROrgHeaderPK);
			AssertEquals("Period to Date Sales", 1022.10M, actual);
		}

		public void TestGetPTDPurchases()
		{
			decimal actual = new ARAPDataAccessor().GetPTDPurchases(APOrgHeaderPK);
			AssertEquals("Period to Date Sales", 942.00M, actual);
		}

		public void TestGetYTDPurchases()
		{
			decimal actual = new ARAPDataAccessor().GetYTDPurchases(APOrgHeaderPK);
			AssertEquals("YTD Purchases", 2596.00M, actual);
		}

		public void TestGetYTDSales()
		{
			decimal actual = new ARAPDataAccessor().GetYTDSales(AROrgHeaderPK);
			AssertEquals("YTD Purchases", 2396.10M, actual);
		}

		public void TestGetLYRSales()
		{
			SetAllTransactionsToLastYear();
			SetPeriodsToLastYear();
			decimal actual = new ARAPDataAccessor().GetLYRSales(AROrgHeaderPK);
			AssertEquals("Last Year Sales", 2396.10M, actual);
		}

		public void TestGetLYRPurchases()
		{
			SetAllTransactionsToLastYear();
			SetPeriodsToLastYear();
			decimal actual = new ARAPDataAccessor().GetLYRPurchases(APOrgHeaderPK);
			AssertEquals("Last Year Purchases", 2596.00M, actual);
		}

		public void TestGetARCreditLimit()
		{
			UpdateOrganisation(AROrgHeaderPK, 500.00M, Constants.InvoiceTerms.FromMonthEnd, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsReceivable);
			decimal actual = new ARAPDataAccessor().GetARCreditLimit(AROrgHeaderPK);
			AssertEquals("AR Organisation Credit Limit", 500.00M, actual);
		}

		public void TestGetARCreditLimitWithTemoraryIncrease()
		{
			UpdateOrganisation(AROrgHeaderPK, 500.00M, Constants.InvoiceTerms.FromMonthEnd, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsReceivable);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(AROrg.CompanyData, 500.00M, 10.00M);
			Factory.Save();
			decimal actual = new ARAPDataAccessor().GetARCreditLimit(AROrgHeaderPK);
			AssertEquals("AR Organisation Credit Limit, with a temporary increase", 510.00M, actual);
		}

		public void TestGetAPCreditLimit()
		{
			UpdateOrganisation(APOrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromMonthEnd, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable);
			decimal actual = new ARAPDataAccessor().GetAPCreditLimit(APOrgHeaderPK);
			AssertEquals("AP Organisation Credit Limit", 300.00M, actual);
		}

		public void TestGetStandardInvoiceTermsAPOrg()
		{
			UpdateOrganisation(APOrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromMonthEnd, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable);
			var termsText = new ARAPDataAccessor().GetStandardInvoiceTerms(APOrgHeaderPK);
			AssertEquals("AP Organisation Standard Invoice Terms", "0/" + Constants.InvoiceTerms.FromMonthEnd, termsText);
		}

		public void TestGetPaymentTerms()
		{
			UpdateOrganisation(APOrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromMonthEnd, 10, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable);
			var termsText = new ARAPDataAccessor().GetPaymentTerms(APOrgHeaderPK);
			AssertEquals("10/MTH", termsText);

			UpdateOrganisation(APOrgHeaderPK, 300.00M, Constants.InvoiceTerms.CashOnDelivery, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable);
			termsText = new ARAPDataAccessor().GetPaymentTerms(APOrgHeaderPK);
			AssertEquals("COD", termsText);
		}

		public void TestGetPaymentTermsIfOrganisationHasDefaultTerm()
		{
			APOrg.APSettlementGroupPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			APOrg.APSettlementGroup.CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromInvoiceDate.Code;
			APOrg.APSettlementGroup.CompanyData.OB_APPaymentTermDays = 5;
			APOrg.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			Factory.Save();
			var term = new ARAPDataAccessor().GetPaymentTerms(APOrgHeaderPK);
			AssertEquals("AP Organisation Standard Terms", "5/INV", term);
		}

		public void TestGetStandardInvoiceTermsAROrg()
		{
			UpdateOrganisation(AROrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromPeriodEnd, 14, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable);
			var termsText = new ARAPDataAccessor().GetStandardInvoiceTerms(AROrgHeaderPK);
			AssertEquals("AP Organisation Standard Invoice Terms", "14/" + Constants.InvoiceTerms.FromPeriodEnd, termsText);
		}

		public void TestGetDisbursementInvoiceTermsAROrg()
		{
			UpdateOrganisation(AROrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromPeriodEnd, 14, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsReceivable);
			var termsText = new ARAPDataAccessor().GetDisbursementInvoiceTerms(AROrgHeaderPK);
			AssertEquals("AR Organisation Standard Invoice Terms", "14/PER, (DSB)->COD", termsText);
		}

		public void TestGetAPOutstandingBalance()
		{
			var sqlQuery = $@"INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) 
VALUES ('{APOrgHeaderPK}', '{GlbCompany.CurrentCompany.PK}', 'AP', 0, 0, 0, 15)";

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}

			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			UpdateOrganisation(APOrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromMonthEnd, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable, GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals("AP credit balance", -993.00M, enquiryDataAccessor.GetAPOutstandingBalance(APOrgHeaderPK));
			Guid aPCompanyPK = Factory.NewWithValidTestData<OrgHeader>().PK.ToGuid();
			string code = TestObjectCreator.GetRandomString(3);
			InsertCompany(aPCompanyPK, code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			UpdateOrganisation(APOrgHeaderPK, 300.00M, Constants.InvoiceTerms.FromMonthEnd, 0, Constants.InvoiceTerms.CashOnDelivery, 0, LedgerTypes.AccountsPayable, aPCompanyPK);
			AssertEquals("AP credit balance", -1293.00M, enquiryDataAccessor.GetAPOutstandingBalance(APOrgHeaderPK));

			using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("AP credit balance", -1278.00M, enquiryDataAccessor.GetAPOutstandingBalance(APOrgHeaderPK));
			}
		}

		public void TestGetGlobalCreditGroupName()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			var group = enquiryDataAccessor.GetGlobalCreditGroupName(AROrgHeaderPK);
			AssertEquals("AR Organisation Global Credit Group", GlobalCreditOrg.OH_Code, group);
		}

		public void TestGetGlobalCreditLimit()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			var limit = enquiryDataAccessor.GetGlobalCreditLimit(AROrgHeaderPK);
			AssertEquals("AR Organisation Global Credit Limit", 3000m, limit);
		}

		public void TestGetGlobalCreditCurrency()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			var currency = enquiryDataAccessor.GetGlobalCreditCurrency(AROrgHeaderPK);
			AssertEquals("AR Orgnaisation Global Credit Currency", Core.Constants.CurrencyCodes.Australia, currency);
		}

		public void TestGetGlobalCreditAvailable()
		{
			var includeUnpostedRevenueInCreditLimitCalculation = ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInGlobalCreditLimitCalculation as CodePairRegistryItem;
			var registryItem = includeUnpostedRevenueInCreditLimitCalculation;
			var originalValue = registryItem.Value;

			var sGDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD");
			var sellRate = sGDCurrency.ExchangeRates.AddNew();
			sellRate.RE_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.Equal, "SIN")).PK.ToGuid();
			sellRate.RE_StartDate = ZDateTime.Now.AddDays(-1);
			sellRate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
			sellRate.RE_RX_NKExCurrency = "AUD";
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			sellRate.RE_SellRate = 0.8m;

			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
				var credit = enquiryDataAccessor.GetGlobalCreditAvailable(GlobalCreditOrgPK);
				AssertEquals("AR Orgnaisation Global Credit Available", 328.75m, credit);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestGetARAPAverageDaysFromInvoiceDateToFullyPaidDate()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("ARAverageDaysFromInvoiceDateToFullyPaidDate", 0, enquiryDataAccessor.GetARAverageDaysFromInvoiceDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("APAverageDaysFromInvoiceDateToFullyPaidDate", 0, enquiryDataAccessor.GetAPAverageDaysFromInvoiceDateToFullyPaidDate(APOrgHeaderPK));
			bool isCancelled = false;
			bool isDisbursement = false;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv1 = InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv1.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv1 = InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv1.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			TransactionHeader arInv2 = InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv2.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv2 = InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv2.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			Factory.Save();
			arInv1.AH_FullyPaidDate = arInv1.AH_InvoiceDate.AddDays(5);
			apInv1.AH_FullyPaidDate = apInv1.AH_InvoiceDate.AddDays(6);
			arInv2.AH_FullyPaidDate = arInv2.AH_InvoiceDate.AddDays(9);
			apInv2.AH_FullyPaidDate = apInv2.AH_InvoiceDate.AddDays(10);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("ARAverageDaysFromInvoiceDateToFullyPaidDate", 7, enquiryDataAccessor.GetARAverageDaysFromInvoiceDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("APAverageDaysFromInvoiceDateToFullyPaidDate", 8, enquiryDataAccessor.GetAPAverageDaysFromInvoiceDateToFullyPaidDate(APOrgHeaderPK));
			isDisbursement = true;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv3 = InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv3.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			TransactionHeader apInv3 = InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			apInv3.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv3.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			Factory.Save();
			arInv3.AH_FullyPaidDate = arInv3.AH_InvoiceDate.AddDays(11);
			apInv3.AH_FullyPaidDate = apInv3.AH_InvoiceDate.AddDays(12);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("ARAverageDaysFromInvoiceDateToFullyPaidDate", 8, enquiryDataAccessor.GetARAverageDaysFromInvoiceDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("APAverageDaysFromInvoiceDateToFullyPaidDate", 9, enquiryDataAccessor.GetAPAverageDaysFromInvoiceDateToFullyPaidDate(APOrgHeaderPK));
		}

		public void TestGetARAPAverageDaysFromDueDateToFullyPaidDate()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetARAverageDaysFromDueDateToFullyPaidDate", 0, enquiryDataAccessor.GetARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetAPAverageDaysFromDueDateToFullyPaidDate", 0, enquiryDataAccessor.GetAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
			bool isCancelled = false;
			bool isDisbursement = false;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv1 = InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv1.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv1 = InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv1.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			TransactionHeader arInv2 = InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv2.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv2 = InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv2.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			Factory.Save();
			arInv1.AH_FullyPaidDate = arInv1.AH_DueDate.AddDays(5);
			apInv1.AH_FullyPaidDate = apInv1.AH_DueDate.AddDays(6);
			arInv2.AH_FullyPaidDate = arInv2.AH_DueDate.AddDays(9);
			apInv2.AH_FullyPaidDate = apInv2.AH_DueDate.AddDays(10);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetARAverageDaysFromDueDateToFullyPaidDate", 7, enquiryDataAccessor.GetARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetAPAverageDaysFromDueDateToFullyPaidDate", 8, enquiryDataAccessor.GetAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
			isDisbursement = true;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv3 = InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv3.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			TransactionHeader apInv3 = InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			apInv3.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv3.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			Factory.Save();
			arInv3.AH_FullyPaidDate = arInv3.AH_DueDate.AddDays(11);
			apInv3.AH_FullyPaidDate = apInv3.AH_DueDate.AddDays(12);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetARAverageDaysFromDueDateToFullyPaidDate", 8, enquiryDataAccessor.GetARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetAPAverageDaysFromDueDateToFullyPaidDate", 9, enquiryDataAccessor.GetAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
		}

		public void TestGetARAPStandardAverageDaysFromDueDateToFullyPaidDate()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetStandardARAverageDaysFromDueDateToFullyPaidDate", 0, enquiryDataAccessor.GetStandardARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetStandardAPAverageDaysFromDueDateToFullyPaidDate", 0, enquiryDataAccessor.GetStandardAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
			bool isCancelled = false;
			bool isDisbursement = false;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv1 = InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv1.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv1 = InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv1.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			TransactionHeader arInv2 = InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv2.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv2 = InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv2.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			Factory.Save();
			arInv1.AH_FullyPaidDate = arInv1.AH_DueDate.AddDays(5);
			apInv1.AH_FullyPaidDate = apInv1.AH_DueDate.AddDays(6);
			arInv2.AH_FullyPaidDate = arInv2.AH_DueDate.AddDays(9);
			apInv2.AH_FullyPaidDate = apInv2.AH_DueDate.AddDays(10);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetStandardARAverageDaysFromDueDateToFullyPaidDate", 7, enquiryDataAccessor.GetStandardARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetStandardAPAverageDaysFromDueDateToFullyPaidDate", 8, enquiryDataAccessor.GetStandardAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
			isDisbursement = true;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv3 = InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv3.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			TransactionHeader apInv3 = InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			apInv3.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv3.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			Factory.Save();
			arInv3.AH_FullyPaidDate = arInv3.AH_DueDate.AddDays(11);
			apInv3.AH_FullyPaidDate = apInv3.AH_DueDate.AddDays(11);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetStandardARAverageDaysFromDueDateToFullyPaidDate", 7, enquiryDataAccessor.GetStandardARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetStandardAPAverageDaysFromDueDateToFullyPaidDate", 8, enquiryDataAccessor.GetStandardAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
		}

		public void TestGetARAPDisbursementAverageDaysFromDueDateToFullyPaidDate()
		{
			ARAPDataAccessor enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetDisbursementARAverageDaysFromDueDateToFullyPaidDate", 0, enquiryDataAccessor.GetDisbursementARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate", 0, enquiryDataAccessor.GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
			bool isCancelled = false;
			bool isDisbursement = true;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv1 = InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv1.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			TransactionHeader apInv1 = InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 160.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv1.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			TransactionHeader arInv2 = InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 175.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv2.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement).AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			TransactionHeader apInv2 = InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 185.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv2.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			Factory.Save();
			arInv1.AH_FullyPaidDate = arInv1.AH_DueDate.AddDays(5);
			apInv1.AH_FullyPaidDate = apInv1.AH_DueDate.AddDays(6);
			arInv2.AH_FullyPaidDate = arInv2.AH_DueDate.AddDays(9);
			apInv2.AH_FullyPaidDate = apInv2.AH_DueDate.AddDays(10);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetDisbursementARAverageDaysFromDueDateToFullyPaidDate", 7, enquiryDataAccessor.GetDisbursementARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate", 8, enquiryDataAccessor.GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
			isDisbursement = false;
			MatchingGroup = new TransactionMatchLinkGroup(Factory);
			TransactionHeader arInv3 = InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 130.00M, AROrgHeaderPK, TransactionTypes.Receipt, arInv3.AH_FullyPaidDate, LedgerTypes.AccountsReceivable, 0M, isCancelled, isDisbursement);
			TransactionHeader apInv3 = InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 140.00M, APOrgHeaderPK, TransactionTypes.Payment, apInv3.AH_FullyPaidDate, LedgerTypes.AccountsPayable, 0M, isCancelled, isDisbursement);
			Factory.Save();
			arInv3.AH_FullyPaidDate = arInv3.AH_DueDate.AddDays(11);
			apInv3.AH_FullyPaidDate = apInv3.AH_DueDate.AddDays(11);
			Factory.Save();
			enquiryDataAccessor = new ARAPDataAccessor();
			AssertEquals("GetDisbursementARAverageDaysFromDueDateToFullyPaidDate", 7, enquiryDataAccessor.GetDisbursementARAverageDaysFromDueDateToFullyPaidDate(AROrgHeaderPK));
			AssertEquals("GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate", 8, enquiryDataAccessor.GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate(APOrgHeaderPK));
		}

		public void TestGetUnpostedRevenueFieldValues()
		{
			var sqlQuery = $@"INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) 
VALUES ('{AROrgHeaderPK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 0, 0, 0, 15)";

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}

			var enquiryDataAccessor = new ARAPDataAccessor();
			var unpostedRevenueFieldValuesTuple = enquiryDataAccessor.GetUnpostedRevenueFieldValues(AROrgHeaderPK);
			AssertEquals("AR Organisation Posted Revenue", 2531.25M, unpostedRevenueFieldValuesTuple.PostedRevenue);
			AssertEquals("AR Organisation Recognised WIP", 321.00M, unpostedRevenueFieldValuesTuple.RecognisedWIP);
			AssertEquals("AR Organisation Unrecognised WIP", 123.00M, unpostedRevenueFieldValuesTuple.UnrecognisedWIP);
			AssertEquals("AR Organisation Claim", 15.00M, unpostedRevenueFieldValuesTuple.Claim);
		}

		public void TestCalculatedCreditBalance()
		{
			var enquiryDataAccessor = new ARAPDataAccessor();
			var creditLimit = 100M;
			var postedRevenue = 10M;
			var recognisedWIP = 20M;
			var unrecognisedWIP = 30M;
			var creditBalance = 0M;
			var claim = 5M;

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.Posted))
			{
				creditBalance = enquiryDataAccessor.CalculatedCreditBalance(creditLimit, postedRevenue, recognisedWIP, unrecognisedWIP, claim);
				AssertEquals(90M, creditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditBalance = enquiryDataAccessor.CalculatedCreditBalance(creditLimit, postedRevenue, recognisedWIP, unrecognisedWIP, claim);
					AssertEquals(95M, creditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedAndRecognized))
			{
				creditBalance = enquiryDataAccessor.CalculatedCreditBalance(creditLimit, postedRevenue, recognisedWIP, unrecognisedWIP, claim);
				AssertEquals(70M, creditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditBalance = enquiryDataAccessor.CalculatedCreditBalance(creditLimit, postedRevenue, recognisedWIP, unrecognisedWIP, claim);
					AssertEquals(75M, creditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				creditBalance = enquiryDataAccessor.CalculatedCreditBalance(creditLimit, postedRevenue, recognisedWIP, unrecognisedWIP, claim);
				AssertEquals(40M, creditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditBalance = enquiryDataAccessor.CalculatedCreditBalance(creditLimit, postedRevenue, recognisedWIP, unrecognisedWIP, claim);
					AssertEquals(45M, creditBalance);
				}
			}
		}

		public void TestCalculatedCreditBalanceForNotMatchingCodes()
		{
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.DataType.SuspendValidation())
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX"))
			{
				var includeUnpostedRevenueInCreditLimitCalculation = AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value;
				AssertNotEquals("Precondition not match PST", Constants.CreditLimitChecking.Posted, includeUnpostedRevenueInCreditLimitCalculation);
				AssertNotEquals("Precondition not match REC", Constants.CreditLimitChecking.PostedAndRecognized, includeUnpostedRevenueInCreditLimitCalculation);
				AssertNotEquals("Precondition not match ALL", Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized, includeUnpostedRevenueInCreditLimitCalculation);

				var enquiryDataAccessor = new ARAPDataAccessor();
				var argumentException = AssertExceptionThrown<ArgumentException>(() => enquiryDataAccessor.CalculatedCreditBalance(0, 0, 0, 0, 0));
				AssertEquals("Invalid registry IncludeUnpostedRevenueInCreditLimitCalculation.", argumentException.Message);
			}
		}

		public void TestGetSettlementGroupARCreditBalance()
		{
			var settlementGroupOrg = Factory.NewWithValidTestData<OrgHeader>();
			settlementGroupOrg.OH_IsDebtor = true;
			settlementGroupOrg.MiscServ.OM_ARCreditLimit = 600m;
			settlementGroupOrg.CompanyData.OB_ARCreditApproved = true;

			var testSubOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testSubOrg1.OH_IsDebtor = true;
			testSubOrg1.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			testSubOrg1.CompanyData.OB_ARCreditApproved = true;
			testSubOrg1.ARSettlementGroupPK = settlementGroupOrg.PK;

			Factory.Save();

			var sqlQuery = $@"INSERT INTO dbo.AccOrgBalanceChanges (Y2_OH, Y2_GC, Y2_Ledger, Y2_RecognizedDelta, Y2_UnrecognizedDelta, Y2_BalanceDelta, Y2_ClaimDelta) 
VALUES ('{settlementGroupOrg.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 10, 20, 30, 15) , 
('{testSubOrg1.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 20, 40, 60, 30); 
INSERT INTO dbo.AccOrgBalance (Y3_OH, Y3_GC, Y3_Ledger, Y3_Balance, Y3_Recognized, Y3_Unrecognized, Y3_Claim) 
VALUES ('{settlementGroupOrg.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 40, 50, 60, 20) , 
('{testSubOrg1.PK}', '{GlbCompany.CurrentCompany.PK}', 'AR', 80, 100, 120, 40)";

			using (var cmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				cmd.ExecuteNonQuery();
			}

			var enquiryDataAccessor = new ARAPDataAccessor();
			var creditLimit = enquiryDataAccessor.GetARCreditLimit(settlementGroupOrg.PK.ToGuid());
			var unpostedRevenueFieldValuesTuple = enquiryDataAccessor.GetUnpostedRevenueFieldValues(settlementGroupOrg.PK.ToGuid());
			AssertEquals("Precondition AR Organisation CreditLimit", 600M, creditLimit);
			AssertEquals("Precondition AR Organisation Posted Revenue", 70M, unpostedRevenueFieldValuesTuple.PostedRevenue);
			AssertEquals("Precondition AR Organisation Recognised WIP", 60M, unpostedRevenueFieldValuesTuple.RecognisedWIP);
			AssertEquals("Precondition AR Organisation Unrecognised WIP", 80M, unpostedRevenueFieldValuesTuple.UnrecognisedWIP);
			AssertEquals("Precondition AR Organisation Claim", 35M, unpostedRevenueFieldValuesTuple.Claim);

			unpostedRevenueFieldValuesTuple = enquiryDataAccessor.GetUnpostedRevenueFieldValues(testSubOrg1.PK.ToGuid());
			AssertEquals("Precondition AR Organisation Posted Revenue", 140M, unpostedRevenueFieldValuesTuple.PostedRevenue);
			AssertEquals("Precondition AR Organisation Recognised WIP", 120M, unpostedRevenueFieldValuesTuple.RecognisedWIP);
			AssertEquals("Precondition AR Organisation Unrecognised WIP", 160M, unpostedRevenueFieldValuesTuple.UnrecognisedWIP);
			AssertEquals("Precondition AR Organisation Claim", 70M, unpostedRevenueFieldValuesTuple.Claim);

			var creditBalance = 0M;
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.Posted))
			{
				creditBalance = enquiryDataAccessor.GetSettlementGroupARCreditBalance(settlementGroupOrg);
				AssertEquals(390M, creditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditBalance = enquiryDataAccessor.GetSettlementGroupARCreditBalance(settlementGroupOrg);
					AssertEquals(495M, creditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedAndRecognized))
			{
				creditBalance = enquiryDataAccessor.GetSettlementGroupARCreditBalance(settlementGroupOrg);
				AssertEquals(210M, creditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditBalance = enquiryDataAccessor.GetSettlementGroupARCreditBalance(settlementGroupOrg);
					AssertEquals(315M, creditBalance);
				}
			}

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			{
				creditBalance = enquiryDataAccessor.GetSettlementGroupARCreditBalance(settlementGroupOrg);
				AssertEquals(-30M, creditBalance);
				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					creditBalance = enquiryDataAccessor.GetSettlementGroupARCreditBalance(settlementGroupOrg);
					AssertEquals(75M, creditBalance);
				}
			}
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}

		void InsertCompany(Guid companyPK, string code, string localCurrency)
		{
			ListDictionary randomParameters = new ListDictionary();
			string sQL = "INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_StartDate) VALUES (@PK, @Code, 'AU company', @Currency, @Country, @StartDate)";
			DbCommand command = ((IDbConnected)Factory).Connection.Command(sQL);
			SetCommandParam(command, randomParameters, "@PK", SqlDbType.UniqueIdentifier, companyPK);
			SetCommandParam(command, randomParameters, "@Code", SqlDbType.Char, code, 3);
			SetCommandParam(command, randomParameters, "@Currency", SqlDbType.VarChar, localCurrency, 3);
			SetCommandParam(command, randomParameters, "@Country", SqlDbType.VarChar, GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString(), 2);
			SetCommandParam(command, randomParameters, "@StartDate", SqlDbType.SmallDateTime, ZDateTime.Now.ToDateTime());
			ExecuteWithRandomValuesModified(command, randomParameters);
		}

		void SetCommandParam(DbCommand command, ListDictionary randomList, string name, SqlDbType type, object value, int size = 0)
		{
			int randomStringLength = GetRandomParamLength(value.ToString());
			if (randomStringLength <= 0)
			{
				if (size == 0)
				{
					command.AddParameter(name, type, value);
				}
				else
				{
					command.AddParameter(name, type, size, value);
				}
			}
			else
			{
				SqlParameter sqlParameterToAdd;
				if (size == 0)
				{
					sqlParameterToAdd = new SqlParameter(name, type);
				}
				else
				{
					sqlParameterToAdd = new SqlParameter(name, type, size);
				}

				sqlParameterToAdd.Value = value;
				randomList.Add(sqlParameterToAdd, randomStringLength);
			}
		}

		void ExecuteWithRandomValuesModified(DbCommand command, ListDictionary randomParameters)
		{
			for (int i = 0; i < 10; i++)
			{
				try
				{
					ExecuteWithRandomValuesOnce(command, randomParameters);
					return; // If succeded, this is a EXIT point.
				}
				catch (System.Data.Common.DbException)
				{
				}
			}

			throw new ApplicationException("Inserting random test data failed 10 times in a row");
		}

		int GetRandomParamLength(string parameterValue)
		{
			int stringLength = 0;
			Match match = Regex.Match(parameterValue, @"^RANDOM ([0-9]+)$", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				stringLength = int.Parse(match.Groups[1].Value);
			}

			return stringLength;
		}

		void ExecuteWithRandomValuesOnce(DbCommand command, ListDictionary randomParameters)
		{
			foreach (SqlParameter parameter in randomParameters.Keys)
			{
				command.RemoveParameterIfExists(parameter.ParameterName);
			}

			foreach (SqlParameter parameter in randomParameters.Keys)
			{
				command.AddParameter(parameter.ParameterName, parameter.SqlDbType, TestObjectCreator.GetRandomString((int)randomParameters[parameter]));
			}

			command.ExecuteNonQuery();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CleanUpAccountingData();
			RemoveExistingPeriods();
			AROrg = Factory.LoadTop1<OrgHeader>(CompanyDataQuery(OrgCompanyDataSchema.OB_IsDebtor));
			AROrgHeaderPK = AROrg.PK.ToGuid();
			APOrg = Factory.LoadTop1<OrgHeader>(CompanyDataQuery(OrgCompanyDataSchema.OB_IsCreditor));
			APOrgHeaderPK = APOrg.PK.ToGuid();

			GlobalCreditOrg = Factory.NewWithValidTestData<OrgHeader>();
			GlobalCreditOrg.CompanyData.OB_IsDebtor = true;
			GlobalCreditOrg.CompanyData.OB_IsCreditor = true;
			GlobalCreditOrgPK = GlobalCreditOrg.PK.ToGuid();
			GlobalCreditOrg.CompanyData.OB_AROnCreditHold = ZBool.True;
			GlobalCreditOrg.CompanyData.OB_ARCreditLimit = 500m;
			GlobalCreditOrg.MiscServ.OM_ARGlobalCreditApproved = true;
			GlobalCreditOrg.MiscServ.OM_RX_NKARGlobalCreditCurrency = Core.Constants.CurrencyCodes.Australia;
			GlobalCreditOrg.MiscServ.OM_ARGlobalCreditLimit = 3000m;
			GlobalCreditOrg.MiscServ.OM_ARGlobalOnCreditHold = true;

			AROrg.MiscServ.OM_OH_ARGlobalCreditGroup = GlobalCreditOrgPK;

			bool isCancelled = false;
			bool isDisbursement = false;
			PeriodManagementTestHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			PeriodManagementTestHelper.SetupPeriods();
			CurrentPeriodDateLatest = PeriodManagementTestHelper.CurrentPeriod.AM_StartDate.AddDays(6);
			CurrentPeriodDate = PeriodManagementTestHelper.CurrentPeriod.AM_StartDate.AddDays(5);
			SingleAgePeriodDate = PeriodManagementTestHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			TwoAgePeriodDate = PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5);
			ThreeAgePeriodDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(5);
			MatchingGroup = new TransactionMatchLinkGroup(Factory);

			InsertTransaction(Factory, 300.00M, APOrgHeaderPK, TransactionTypes.Payment, CurrentPeriodDate, LedgerTypes.AccountsPayable, 50.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 60.00M, APOrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsPayable, 10.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 90.00M, APOrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsPayable, 85.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 450.00M, APOrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 210.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 952.00M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsPayable, 640.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 280.00M, APOrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsPayable, 240.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 180.00M, APOrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsPayable, 180.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 634.00M, APOrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 28.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 40.00M, APOrgHeaderPK, TransactionTypes.CreditNote, CurrentPeriodDate, LedgerTypes.AccountsPayable, 40.00M, isCancelled, false);
			InsertTransaction(Factory, 50.00M, APOrgHeaderPK, TransactionTypes.CreditNote, SingleAgePeriodDate, LedgerTypes.AccountsPayable, 50.00M, isCancelled, false);
			InsertTransaction(Factory, 70.00M, APOrgHeaderPK, TransactionTypes.CreditNote, TwoAgePeriodDate, LedgerTypes.AccountsPayable, 70.00M, isCancelled, false);
			InsertTransaction(Factory, 90.00M, APOrgHeaderPK, TransactionTypes.CreditNote, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 90.00M, isCancelled, false);
			InsertTransaction(Factory, 30.00M, APOrgHeaderPK, TransactionTypes.AdjustmentNote, CurrentPeriodDate, LedgerTypes.AccountsPayable, 30.00M, isCancelled, false);
			InsertTransaction(Factory, 40.00M, APOrgHeaderPK, TransactionTypes.AdjustmentNote, SingleAgePeriodDate, LedgerTypes.AccountsPayable, 40.00M, isCancelled, false);
			InsertTransaction(Factory, 60.00M, APOrgHeaderPK, TransactionTypes.AdjustmentNote, TwoAgePeriodDate, LedgerTypes.AccountsPayable, 60.00M, isCancelled, false);
			InsertTransaction(Factory, 70.00M, APOrgHeaderPK, TransactionTypes.AdjustmentNote, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 70.00M, isCancelled, false);

			isCancelled = true;
			InsertTransaction(Factory, 300.05M, APOrgHeaderPK, TransactionTypes.Payment, CurrentPeriodDateLatest, LedgerTypes.AccountsPayable, 50.05M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 75.00M, APOrgHeaderPK, TransactionTypes.CreditNote, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 75.00M, isCancelled, false);
			InsertTransaction(Factory, 85.00M, APOrgHeaderPK, TransactionTypes.AdjustmentNote, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 85.00M, isCancelled, false);
			InsertTransaction(Factory, 952.05M, APOrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDateLatest, LedgerTypes.AccountsPayable, 640.05M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 280.00M, APOrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsPayable, 240.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 180.00M, APOrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsPayable, 180.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 634.00M, APOrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsPayable, 28.00M, isCancelled, isDisbursement);

			isCancelled = false;
			InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Receipt, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 25.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 30.00M, AROrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 20.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 45.00M, AROrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 42.50M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 225.00M, AROrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 90.00M, isCancelled, isDisbursement);
			var arInvoice1 = InsertTransaction(Factory, 476.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDateLatest, LedgerTypes.AccountsReceivable, 320.00M, isCancelled, isDisbursement);
			var arInvoice2 = InsertTransaction(Factory, 140.00M, AROrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 120.00M, isCancelled, isDisbursement);
			var arInvoice3 = InsertTransaction(Factory, 90.00M, AROrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 90.00M, isCancelled, isDisbursement);
			var arInvoice4 = InsertTransaction(Factory, 317.00M, AROrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 14.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 50.00M, AROrgHeaderPK, TransactionTypes.CreditNote, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 50.00M, isCancelled, false);
			InsertTransaction(Factory, 70.00M, AROrgHeaderPK, TransactionTypes.CreditNote, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 70.00M, isCancelled, false);
			InsertTransaction(Factory, 40.00M, AROrgHeaderPK, TransactionTypes.CreditNote, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 40.00M, isCancelled, false);
			InsertTransaction(Factory, 90.00M, AROrgHeaderPK, TransactionTypes.CreditNote, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 90.00M, isCancelled, false);
			InsertTransaction(Factory, 30.00M, AROrgHeaderPK, TransactionTypes.CreditNote, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 30.00M, isCancelled, false);
			InsertTransaction(Factory, 50.00M, AROrgHeaderPK, TransactionTypes.CreditNote, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 50.00M, isCancelled, false);
			InsertTransaction(Factory, 20.00M, AROrgHeaderPK, TransactionTypes.CreditNote, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 20.00M, isCancelled, false);
			InsertTransaction(Factory, 50.00M, AROrgHeaderPK, TransactionTypes.CreditNote, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 50.00M, isCancelled, false);

			InsertTransaction(Factory, 100.00M, GlobalCreditOrgPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 70.00M, isCancelled, false);
			InsertTransaction(Factory, 75.00M, GlobalCreditOrgPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 70.00M, isCancelled, false);

			isCancelled = true;
			InsertTransaction(Factory, 150.05M, AROrgHeaderPK, TransactionTypes.Receipt, CurrentPeriodDateLatest, LedgerTypes.AccountsReceivable, 25.05M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 90.00M, AROrgHeaderPK, TransactionTypes.CreditNote, CurrentPeriodDateLatest, LedgerTypes.AccountsReceivable, 90.00M, isCancelled, false);
			InsertTransaction(Factory, 70.00M, AROrgHeaderPK, TransactionTypes.AdjustmentNote, CurrentPeriodDateLatest, LedgerTypes.AccountsReceivable, 70.00M, isCancelled, false);
			InsertTransaction(Factory, 476.05M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDateLatest, LedgerTypes.AccountsReceivable, 320.05M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 140.00M, AROrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 120.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 90.00M, AROrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 90.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 317.00M, AROrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 14.00M, isCancelled, isDisbursement);

			isCancelled = false;
			isDisbursement = true;
			List<TransactionHeader> disbursementTransactions = new List<TransactionHeader>();
			disbursementTransactions.Add(InsertTransaction(Factory, 150.00M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 80.00M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 30.00M, AROrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 20.00M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 45.00M, AROrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 42.50M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 225.00M, AROrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 100.00M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 476.10M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 360.10M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 140.00M, AROrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 130.00M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 90.00M, AROrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 80.00M, isCancelled, isDisbursement));
			disbursementTransactions.Add(InsertTransaction(Factory, 317.00M, AROrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 244.00M, isCancelled, isDisbursement));
			// Set various Disbursement types to all disbursement transactions
			int i = 0;
			int maxIndex = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes.Length - 1;
			foreach (TransactionHeader header in disbursementTransactions)
			{
				header.AH_TransactionCategory = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes[i];
				i = i < maxIndex ? i + 1 : 0;
			}

			isCancelled = true;
			InsertTransaction(Factory, 476.15M, AROrgHeaderPK, TransactionTypes.Invoice, CurrentPeriodDate, LedgerTypes.AccountsReceivable, 360.15M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 140.00M, AROrgHeaderPK, TransactionTypes.Invoice, SingleAgePeriodDate, LedgerTypes.AccountsReceivable, 130.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 90.00M, AROrgHeaderPK, TransactionTypes.Invoice, TwoAgePeriodDate, LedgerTypes.AccountsReceivable, 80.00M, isCancelled, isDisbursement);
			InsertTransaction(Factory, 317.00M, AROrgHeaderPK, TransactionTypes.Invoice, ThreeAgePeriodDate, LedgerTypes.AccountsReceivable, 134.00M, isCancelled, isDisbursement);
			ZDecimal matchingGroupBalance = ZDecimal.Zero;
			foreach (TransactionMatchLink link in MatchingGroup)
			{
				matchingGroupBalance += link.AP_Amount;
			}

			if (matchingGroupBalance != ZDecimal.Zero)
			{
				AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_InvoiceAmount = -matchingGroupBalance;
				TransactionMatchLink link = MatchingGroup.AddNew();
				link.AP_AH = header.PK;
				link.AP_Amount = -matchingGroupBalance;
			}

			TestObjectCreator.SetupMatchLinkMatchDate(MatchingGroup);
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, 321, AROrg);
			Factory.Save();
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			var shipmentUnrecognized = TestObjectCreator.CreateShipment("S002");
			var jobUnrecognized = TestObjectCreator.CreateJob(shipmentUnrecognized);
			var chargeUnrecognized = TestObjectCreator.CreateCharge(jobUnrecognized, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, 123, AROrg);

			var batch = Factory.NewWithValidTestData<AccCollectionBatch>();
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_TotalAmount = 100m;

			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.ABIGAS, "00000001", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order, (ARInvoice)arInvoice1, false);
			TestObjectCreator.CreateCollectionOrderLine(order, (ARInvoice)arInvoice2, false);
			TestObjectCreator.CreateCollectionOrderLine(order, (ARInvoice)arInvoice3, false);
			TestObjectCreator.CreateCollectionOrderLine(order, (ARInvoice)arInvoice4, false);
			Factory.Save();
		}

		void CleanUpAccountingData()
		{
			string sQL = @"
								DELETE FROM dbo.JobCharge
								DELETE FROM dbo.ACCHOTCHEQUE
								DELETE FROM dbo.ACCTRANSACTIONMATCHLINK
								DELETE FROM dbo.ACCTRANSACTIONLINES
								DELETE FROM dbo.ACCTRANSACTIONHEADER
						";
			((IDbConnected)Factory).Connection.ExecuteNonQuery(sQL);
		}

		protected OrgHeader AROrg;
		protected Guid AROrgHeaderPK;
		protected OrgHeader APOrg;
		protected Guid APOrgHeaderPK;
		protected OrgHeader GlobalCreditOrg;
		protected Guid GlobalCreditOrgPK;
		AccountingPeriodTestHelper PeriodManagementTestHelper;
		ZDateTime CurrentPeriodDate;
		ZDateTime SingleAgePeriodDate;
		ZDateTime TwoAgePeriodDate;
		ZDateTime ThreeAgePeriodDate;
		ZDateTime CurrentPeriodDateLatest;
		TransactionMatchLinkGroup MatchingGroup;
		protected void RemoveExistingPeriods()
		{
			((IDbConnected)Factory).Connection.ExecuteNonQuery("DELETE FROM dbo.ACCPERIODMANAGEMENT");
		}

		protected TransactionHeader InsertTransaction(BusinessObjectFactory factory, decimal amount, Guid organisation, string transactionType, ZDateTime postAndDueDate, string ledger, decimal outstandingAmount, bool isCancelled, bool isDisbursement)
		{
			TransactionHeader headerToInsert = (TransactionHeader)factory.New(GetBizOType(transactionType, ledger));
			headerToInsert.AH_Ledger = ledger;
			headerToInsert.AH_TransactionType = transactionType;
			headerToInsert.AH_OH = organisation;
			headerToInsert.AH_InvoiceDate = postAndDueDate;
			headerToInsert.AH_OSTotalAmount = amount;
			headerToInsert.AH_GE = GlbDepartment.CurrentDepartment.PK;
			headerToInsert.AH_GB = GlbBranch.CurrentBranch.PK;
			headerToInsert.AH_PostDate = postAndDueDate;
			headerToInsert.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			headerToInsert.AH_LocalExTaxAmount = amount;
			var compatibleLineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(ledger, transactionType);
			if (headerToInsert is InvoicingBase && !string.IsNullOrEmpty(compatibleLineType))
			{
				TestObjectCreator.CreateInvoiceLine(headerToInsert as InvoicingBase, headerToInsert.TransactionCurrency, headerToInsert.AH_ExchangeRate, amount, 0m, 0m, amount, 0m, 0m);
			}

			if (ledger == LedgerTypes.AccountsPayable && (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.AdjustmentNote))
			{
				((IDbConnected)factory).Connection.BeginTransaction();
				headerToInsert.AH_TransactionNum = AccountingNumberFountainWrapperFactory.Instance.APInvoiceNo.Generate(headerToInsert);
				((IDbConnected)factory).Connection.CommitTransaction();
			}

			headerToInsert.AH_Desc = "Description";
			if (isDisbursement)
			{
				headerToInsert.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			}

			headerToInsert.AH_LocalOutstandingAmount = outstandingAmount;
			if (outstandingAmount == 0m)
			{
				headerToInsert.AH_FullyPaidDate = postAndDueDate;
			}

			headerToInsert.AH_DueDate = postAndDueDate;
			AccTransactionMatchLink matchLink = MatchingGroup.AddNew();
			if ((headerToInsert.AH_Ledger == "AR" && headerToInsert.AH_TransactionType != "REC") || (headerToInsert.AH_Ledger == "AP" && headerToInsert.AH_TransactionType == "PAY"))
			{
				matchLink.AP_Amount = amount - outstandingAmount;
			}

			if ((headerToInsert.AH_Ledger == "AP" && headerToInsert.AH_TransactionType != "PAY") || (headerToInsert.AH_Ledger == "AR" && headerToInsert.AH_TransactionType == "REC"))
			{
				matchLink.AP_Amount = outstandingAmount - amount;
			}

			matchLink.AP_AH = headerToInsert.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			headerToInsert.AH_IsCancelled = isCancelled;
			if (isCancelled && ((IMatching)headerToInsert).CurrentMatchGroup.Count == 0)
			{
				((IMatching)headerToInsert).CurrentMatchGroup.AddNew().AP_AH = headerToInsert.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(headerToInsert as IMatching);
			}

			return headerToInsert;
		}

		protected Type GetBizOType(string transactionType, string ledger)
		{
			switch (transactionType)
			{
				case TransactionTypes.Receipt:
					switch (ledger)
					{
						case LedgerTypes.AccountsReceivable:
							return typeof(ARReceipt);
						case LedgerTypes.AccountsPayable:
							return typeof(APReceipt);
					}

					break;
				case TransactionTypes.Payment:
					switch (ledger)
					{
						case LedgerTypes.AccountsReceivable:
							return typeof(ARPayment);
						case LedgerTypes.AccountsPayable:
							return typeof(APPayment);
					}

					break;
				case TransactionTypes.Invoice:
					switch (ledger)
					{
						case LedgerTypes.AccountsPayable:
							return typeof(APInvoice);
						case LedgerTypes.AccountsReceivable:
							return typeof(ARInvoice);
					}

					break;
				case TransactionTypes.CreditNote:
					switch (ledger)
					{
						case LedgerTypes.AccountsPayable:
							return typeof(APCreditNote);
						case LedgerTypes.AccountsReceivable:
							return typeof(ARCreditNote);
					}

					break;
				case TransactionTypes.AdjustmentNote:
					switch (ledger)
					{
						case LedgerTypes.AccountsPayable:
							return typeof(APAdjustmentNote);
						case LedgerTypes.AccountsReceivable:
							return typeof(ARAdjustmentNote);
					}

					break;
				case TransactionTypes.InvoiceBatch:
					switch (ledger)
					{
						case LedgerTypes.AccountsPayable:
							return typeof(APInvoice);
						case LedgerTypes.AccountsReceivable:
							return typeof(ARInvoice);
					}

					break;
			}

			return null;
		}

		protected void SetAllTransactionsToLastYear()
		{
			string sQL = String.Format("UPDATE dbo.AccTransactionHeader SET AH_PostDate = @Date, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST'");
			DbCommand command = ((IDbConnected)Factory).Connection.Command(sQL);
			command.AddParameter("@Date", SqlDbType.SmallDateTime, PeriodManagementTestHelper.FuturePeriod.AM_StartDate.AddYears(-1).AddDays(5).ToDateTime());
			command.ExecuteNonQuery();
		}

		protected void SetPeriodsToLastYear()
		{
			int period = (PeriodManagementTestHelper.CurrentPeriod.AM_Year - 1) * 100 + (PeriodManagementTestHelper.CurrentPeriod.AM_Period - (PeriodManagementTestHelper.CurrentPeriod.AM_Period / 100 * 100));
			PeriodManagementTestHelper.SetupSinglePeriod(period, PeriodManagementTestHelper.CurrentPeriod.AM_StartDate.AddYears(-1), PeriodManagementTestHelper.CurrentPeriod.AM_EndDate.AddYears(-1));
			period = (PeriodManagementTestHelper.FuturePeriod.AM_Year - 1) * 100 + (PeriodManagementTestHelper.FuturePeriod.AM_Period - (PeriodManagementTestHelper.FuturePeriod.AM_Period / 100 * 100));
			PeriodManagementTestHelper.SetupSinglePeriod(period, PeriodManagementTestHelper.FuturePeriod.AM_StartDate.AddYears(-1), PeriodManagementTestHelper.FuturePeriod.AM_EndDate.AddYears(-1));
			period = (PeriodManagementTestHelper.PreviousOpenPeriod.AM_Year - 1) * 100 + (PeriodManagementTestHelper.PreviousOpenPeriod.AM_Period - (PeriodManagementTestHelper.PreviousOpenPeriod.AM_Period / 100 * 100));
			PeriodManagementTestHelper.SetupSinglePeriod(period, PeriodManagementTestHelper.PreviousOpenPeriod.AM_StartDate.AddYears(-1), PeriodManagementTestHelper.PreviousOpenPeriod.AM_EndDate.AddYears(-1));
			period = (PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_Year - 1) * 100 + (PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_Period - (PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_Period / 100 * 100));
			PeriodManagementTestHelper.SetupSinglePeriod(period, PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddYears(-1), PeriodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_EndDate.AddYears(-1));
			period = (PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_Year - 1) * 100 + (PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_Period - (PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_Period / 100 * 100));
			PeriodManagementTestHelper.SetupSinglePeriod(period, PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_StartDate.AddYears(-1), PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate.AddYears(-1));
		}

		protected void UpdateOrganisation(Guid orgPK, decimal creditLimit, string stdInvoiceTerms, ZByte stdInvoiceTermDays, string dsbInvoiceTerms, int dsbInvoiceTermDays, string ledger)
		{
			UpdateOrganisation(orgPK, creditLimit, stdInvoiceTerms, stdInvoiceTermDays, dsbInvoiceTerms, dsbInvoiceTermDays, ledger, Guid.Empty);
		}

		protected void UpdateOrganisation(Guid orgPK, decimal creditLimit, string stdInvoiceTerms, ZByte stdInvoiceTermDays, string dsbInvoiceTerms, int dsbInvoiceTermDays, string ledger, Guid companyPK)
		{
			string creditLimitFieldName = ledger == LedgerTypes.AccountsPayable ? OrgCompanyDataSchema.Constants.OB_APCreditLimit : OrgCompanyDataSchema.Constants.OB_ARCreditLimit;
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader orgToSave = factory.Load<OrgHeader>(orgPK);
			orgToSave.CompanyData[creditLimitFieldName] = creditLimit;
			orgToSave.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = stdInvoiceTerms;
			orgToSave.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = stdInvoiceTermDays;
			orgToSave.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = dsbInvoiceTerms;
			orgToSave.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = (ZByte)dsbInvoiceTermDays;
			orgToSave.CompanyData.OB_APPaymentTerms = stdInvoiceTerms;
			orgToSave.CompanyData.OB_APPaymentTermDays = stdInvoiceTermDays;
			if (companyPK != Guid.Empty)
			{
				orgToSave.CompanyData.OB_GC = companyPK;
			}

			factory.Save();
		}

		ZDBOnlyQuery CompanyDataQuery(SchemaColumn field)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(field, ZBool.True);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}
