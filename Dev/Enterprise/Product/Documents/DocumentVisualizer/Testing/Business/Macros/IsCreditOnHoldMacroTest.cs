using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class IsCreditOnHoldMacroTest : TestCaseWithFactory
	{
		public void TestIsCreditOnHold_ICreditControlledDocumentDelivery()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			var shipment = Factory.New<DummyShipmentBusinessObject>();
			shipment.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			var expr = "@data.IsCreditOnHold()".With<FilterLibrary>().CreateExpression();
			AssertEquals("Shipment is credit on hold.", true, (bool)expr.Evaluate(shipment));

			orgHeader.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();
			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals(false, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = "@data.IsCreditOnHold()".With<FilterLibrary>().CreateExpression();
			expr.Evaluate(Factory.New<DummyBusinessObject>());
			AssertEquals(true, expr.HasErrors());
			AssertEquals("The current data does not applicable for checking credit on hold.", expr.Errors.Single().Message);
		}

		public void TestIsCreditOnHold_OrgHeader()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.CompanyData.OB_AROnCreditHold = false;
			orgHeader1.OH_IsDebtor = true;
			Factory.Save();

			var expr = "@data.IsCreditOnHold()".With<FilterLibrary>().CreateExpression();
			AssertEquals(false, (bool)expr.Evaluate(orgHeader1));

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CompanyData.OB_AROnCreditHold = true;
			orgHeader2.OH_IsDebtor = true;
			Factory.Save();
			AssertEquals("Is credit on hold.", true, (bool)expr.Evaluate(orgHeader2));

			orgHeader1.CompanyData.OB_ARCreditLimit = 129m;
			CreateNewTransactionHeader(orgHeader1, TransactionTypes.Invoice, LedgerTypes.AccountsReceivable, 130m);

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			orgHeader1.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("Delivery Agent has run out of credit limit.", true, (bool)expr.Evaluate(orgHeader1));
			AssertEquals(false, expr.HasErrors());
		}

		public void TestIsCreditOnHold_Usages()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsDebtor = true;
			orgHeader.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();

			var expr1 = "IsCreditOnHold()".With<FilterLibrary>().CreateExpression();
			AssertEquals(false, (bool)expr1.Evaluate(orgHeader));

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_OH_DeliveryAgent = orgHeader.PK;

			var expr2 = "@data.DeliveryAgent.IsCreditOnHold()".With<FilterLibrary>().CreateExpression();
			AssertEquals(false, (bool)expr2.Evaluate(shipment));
			AssertEquals(false, expr2.HasErrors());

			orgHeader.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			orgHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			AssertEquals(true, (bool)expr1.Evaluate(orgHeader));
			AssertEquals(false, expr1.HasErrors());

			AssertEquals(true, (bool)expr2.Evaluate(shipment));
			AssertEquals(false, expr2.HasErrors());

			expr2 = "DeliveryAgent.IsCreditOnHold()".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr2.Evaluate(shipment));
			AssertEquals(false, expr2.HasErrors());

			expr2 = "IsCreditOnHold(DeliveryAgent)".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr2.Evaluate(shipment));
			AssertEquals(false, expr2.HasErrors());

			expr2 = "@data.IsCreditOnHold(DeliveryAgent)".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr2.Evaluate(shipment));
			AssertEquals(false, expr2.HasErrors());
		}

		#region Implementation

		void CreateNewTransactionHeader(OrgHeader org, string transactionType, string ledger, decimal outstandingAmount)
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

		#endregion
	}
}
