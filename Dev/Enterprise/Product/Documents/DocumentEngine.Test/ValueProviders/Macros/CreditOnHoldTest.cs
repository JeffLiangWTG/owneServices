using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CreditOnHold))]
	sealed class CreditOnHoldTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CreditOnHold()>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CreditOnHold  ( )  >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CreditOnHold  (  asdf )  >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CreditOnHold  (  as.df )  >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<CreditOnHold(  pk,  abc)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			var shipment = Factory.New<DummyShipmentBusinessObject>();
			shipment.OrganisationsForCreditChecksForTest = new[] { orgHeader };
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
			AssertEquals("shipment is credit on hold.", "Y", ValueProviderToTest.GetReplacement("<CreditOnHold ()>", Report));

			orgHeader.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();
			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("N", ValueProviderToTest.GetReplacement("<CreditOnHold ()>", Report));

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_AROnCreditHold = false;
			header.OH_IsDebtor = true;
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			Factory.Save();
			AssertEquals("consignee is not credit on hold.", "N", ValueProviderToTest.GetReplacement("<CreditOnHold (Consignee)>", Report));

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.CompanyData.OB_AROnCreditHold = true;
			header2.OH_IsDebtor = true;
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header2;
			Factory.Save();
			AssertEquals("consignee is credit on hold.", "Y", ValueProviderToTest.GetReplacement("<CreditOnHold (Consignee)>", Report));

			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			header.CompanyData.OB_ARCreditLimit = 129m;
			AccTransactionHeader aRInv = GetNewTransactionHeader(header, ZArchitecture.Core.TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 130m);

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			header.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("consignee has run out of credit limit.", "Y", ValueProviderToTest.GetReplacement("<CreditOnHold (Consignee)>", Report));
		}

		[GuiTest]
		public void TestCreditOnHold_DocumentDelivery()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_AROnCreditHold = true;
			factory.Save();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				var shipment = factory.New<DummyShipmentBusinessObject>();
				shipment.OrganisationsForCreditChecksForTest = new[] { orgHeader };
				var guiManager = ObjectFactory.Get<IDocumentDeliveryRestrictionGUIManager>();
				guiManager.Initialise(shipment);
				IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, factory);
				((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
				using (Globals.SetIsUnitTestingProductionFunctionality())
				{
					string result = null;
					AssertNoExceptionThrown(() => result = (string)ValueProviderToTest.GetReplacement("<CreditOnHold ()>", Report));
					AssertEquals("Credit-on-hold is yes when check it during factory save operation.", "Y", result);
				}
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string transactionType, string ledger, decimal outstandingAmount)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
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

		public void TestReplacement_ErrorMessages()
		{
			var valueProvider = new CreditOnHold();
			object value = null;
			var shipment = Factory.New<DummyShipmentBusinessObject>();
			Factory.Save();
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertEquals("Value should be null", null, value);
			AssertNoExceptionThrown(delegate
			{ value = valueProvider.GetReplacement("<CreditOnHold (Consignee)>", Report); });
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			Assert("report.ErrorManager.ToString()", Report.ErrorManager.ToString().Contains("Couldn't find business object for checking credit on hold."));
			Report.ErrorManager.ClearErrors();

			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			docDataProvider = new DummyDocumentWrapper(menuItem, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertNoExceptionThrown(delegate
			{ value = valueProvider.GetReplacement("<CreditOnHold ()>", Report); });
			AssertEquals("Value should be null", null, value);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			Assert("report.ErrorManager.ToString()", Report.ErrorManager.ToString().Contains("Business Object is not applicable for checking credit on hold."));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CreditOnHold();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_AROnCreditHold = true;

			var shipment = Factory.New<DummyShipmentBusinessObject>();
			shipment.OrganisationsForCreditChecksForTest = new[] { orgHeader };
			IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
			((IReportForUnitTesting)Report).SetBusinessObjectForTesting(docDataProvider);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_AROnCreditHold = false;
			header.OH_IsDebtor = true;
			((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
			Factory.Save();
		}
	}
}
