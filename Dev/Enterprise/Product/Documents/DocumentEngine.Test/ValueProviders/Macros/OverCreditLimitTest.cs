using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(OverCreditLimit))]
	sealed class OverCreditLimitTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<OverCreditLimit>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   Over Credit Limit   >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   OverCreditLimit   >", Passes.FirstPass));
			Assert("should match <OverCreditLimit(AField)>", ValueProviderToTest.IsResponsibleForReplacing("<OverCreditLimit(AField)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   OverCreditLimit(AField, other)   >", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			var shipment = Factory.New<DummyShipmentBusinessObject>();
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
			Report.Analyser = new ReportAnalyser(Report);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			Factory.Save();

			AssertEquals("consignee does not have credit limit.", "N", ValueProviderToTest.GetReplacement(string.Format("<OverCreditLimit({0})>", header.PK), Report));

			header.CompanyData.OB_ARCreditLimit = 129m;
			var aRInv = GetNewTransactionHeader(header, ZArchitecture.Core.TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 130m);

			Factory.Save();

			header.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("consignee has run out of credit limit.", "Y", ValueProviderToTest.GetReplacement(string.Format("<OverCreditLimit ({0})>", header.PK), Report));

			header.CompanyData.OB_ARCreditLimit = 130m;
			Factory.Save();

			header.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("consignee does not run out of credit limit.", "N", ValueProviderToTest.GetReplacement(string.Format("<OverCreditLimit ({0})>", header.PK), Report));
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string transactionType, string ledger, decimal outstandingAmount)
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = transactionType;
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_InvoiceAmount = outstandingAmount;
			invoice.AH_OSTotal = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_IsCancelled = false;
			return invoice;
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new OverCreditLimit();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var shipment = Factory.New<DummyShipmentBusinessObject>();
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsDebtor = true;
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			Factory.Save();

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OH_PK", header.PK));
		}
	}
}
