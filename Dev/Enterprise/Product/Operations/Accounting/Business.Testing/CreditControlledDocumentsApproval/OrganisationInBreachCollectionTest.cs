using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.CreditControlledDocumentsApproval
{
	[TestedType(typeof(OrganisationInBreachCollection))]
	public class OrganisationInBreachCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrganisationInBreachCollection>
	{
		public void TestPopulateOrganisationsAndBreachReasons()
		{
			#region Data Setup

			var objectCreator = new TestObjectCreator(Factory);
			var localClient = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			localClient.CompanyData.OB_ARCreditLimit = 1m;
			localClient.CompanyData.OB_AROnCreditHold = true;

			var consignee = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			consignee.CompanyData.OB_ARCreditLimit = 1m;
			consignee.CompanyData.OB_AROnCreditHold = false;

			var consignor = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			consignor.CompanyData.OB_ARCreditLimit = 1m;
			consignor.CompanyData.OB_AROnCreditHold = false;

			var shipment = objectCreator.CreateShipment("S00001");
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var shipmentJob = objectCreator.CreateJob(shipment, localClient, 0m, objectCreator.Agent, 0m);
			Factory.Save();

			Action clearCreditCheckerCacheForTest = () =>
			{
				localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				consignee.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			};

			clearCreditCheckerCacheForTest();

			#endregion

			var orgCollection = new OrganisationInBreachCollection();
			orgCollection.PopulateOrganisations(shipment);
			AssertEquals(1, orgCollection.Count);
			AssertEquals(localClient.OH_Code, orgCollection[0].OrgCode);
			AssertEquals("Credit On Hold", orgCollection[0].BreachReasons);

			var unpaidPIAInvoiceForLocalClient = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(3), objectCreator.AUD, 1m, 20m, 0m, 20m, 0m, localClient, objectCreator.CC1.PK);
			unpaidPIAInvoiceForLocalClient.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			unpaidPIAInvoiceForLocalClient.AH_InvoiceTerm = InvoiceTerms.PaymentInAdvance;
			Factory.Save();

			clearCreditCheckerCacheForTest();

			orgCollection = new OrganisationInBreachCollection();
			orgCollection.PopulateOrganisations(shipment);
			AssertEquals(1, orgCollection.Count);
			AssertEquals(localClient.OH_Code, orgCollection[0].OrgCode);
			AssertEquals("Credit On Hold,Credit Term PIA", orgCollection[0].BreachReasons);

			var unpaidInvoiceForConsignor = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(3), objectCreator.AUD, 1m, 60m, 0m, 60m, 0m, consignor, objectCreator.CC1.PK);
			Factory.Save();

			var requirementCollection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var requirement = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement() { Amount = 0, Percentage = 0, Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly };
			requirementCollection.Add(requirement);
			using (AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, requirementCollection))
			{
				clearCreditCheckerCacheForTest();
				orgCollection = new OrganisationInBreachCollection();
				orgCollection.PopulateOrganisations(shipment);
				AssertEquals(2, orgCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { localClient.OH_Code, consignor.OH_Code }, orgCollection.Cast<OrganisationInBreach>().Select(x => x.OrgCode));
				AssertEquals("Over Credit Limit", orgCollection.Cast<OrganisationInBreach>().First(x => x.OrgCode == consignor.OH_Code).BreachReasons);
				AssertEquals("Credit On Hold,Over Credit Limit,Credit Term PIA", orgCollection.Cast<OrganisationInBreach>().First(x => x.OrgCode == localClient.OH_Code).BreachReasons);
			}

			var overdueInvoiceForConsignee = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), TestObjectCreator.GetRandomString(4), objectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			overdueInvoiceForConsignee.AH_OH = consignee.PK;
			overdueInvoiceForConsignee.AH_PostDate = ZDateTime.Today.AddDays(-10);
			overdueInvoiceForConsignee.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
			overdueInvoiceForConsignee.AH_DueDate = ZDateTime.Today.AddDays(-5);
			Factory.Save();

			var configurationCollection = new CreditControlledDocumentsCheckConfigurationCollection();
			var upToConfiguration = configurationCollection.AddNew();
			upToConfiguration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			upToConfiguration.NumberOfDaysOverdue = 1;
			upToConfiguration.Amount = 1;
			upToConfiguration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToConfiguration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			var configuration = configurationCollection.AddNew();
			configuration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			configuration.NumberOfDaysOverdue = 1;
			configuration.Amount = 1;
			configuration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			configuration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			using (AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, requirementCollection))
			using (AccountingConfigurationRegistry.Instance.CreditLimitCheckOverdueInvoicesStatusCheck.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection))
			{
				consignee.Factory.ClearCachedValue<int>("CreditControlledDocumentDeliveryDueToCreditCheck" + consignee.CompanyData.PK);
				clearCreditCheckerCacheForTest();
				orgCollection = new OrganisationInBreachCollection();
				orgCollection.PopulateOrganisations(shipment);
				AssertEquals(3, orgCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { localClient.OH_Code, consignor.OH_Code, consignee.OH_Code }, orgCollection.Cast<OrganisationInBreach>().Select(x => x.OrgCode));
				AssertEquals("Over Credit Limit,Overdue Transactions", orgCollection.Cast<OrganisationInBreach>().First(x => x.OrgCode == consignee.OH_Code).BreachReasons);
				AssertEquals("Over Credit Limit", orgCollection.Cast<OrganisationInBreach>().First(x => x.OrgCode == consignor.OH_Code).BreachReasons);
				AssertEquals("Credit On Hold,Over Credit Limit,Credit Term PIA", orgCollection.Cast<OrganisationInBreach>().First(x => x.OrgCode == localClient.OH_Code).BreachReasons);
			}
		}

		#region NonPersistentBusinessObjectCollectionTestCase Overrides
		protected override OrganisationInBreachCollection GetCollectionToTest() => new OrganisationInBreachCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new OrganisationInBreach();

		#endregion
	}
}
