using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentCloseAction))]
	public class SupportIncidentCloseActionTest : SupportIncidentActionTestCase
	{
		public void TestResolutionCommentAddingToEConversationMaxLength()
		{
			var action = new SupportIncidentCloseAction(Factory.New<SupportIncident>());
			AssertEquals("MaxLength", 32000, action.CommentInfo.MaxLength);
		}

		[TestDate(2014, 6, 30)]
		public void TestPerformAction_SendDevelopmentEstimate()
		{
			var incident = Factory.New<SupportIncident>();
			incident.SetupForProjectFeatureRequest();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			var action = new SupportIncidentCloseAction(incident);
			action.SendDevelopmentEstimate = true;
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;
			action.Comment = "Please find estimate attached.";
			action.SynchroniseToIncident();
			AssertEquals(TestDateAttribute.Date, action.Incident.Estimate.CIE_EstimateSentDateUTC);
		}

		[TestDate(2014, 6, 30)]
		public void TestPerformAction_SendSoftwareQuote()
		{
			var incident = Factory.New<SupportIncident>();
			incident.SetupForProjectFeatureRequest();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			var action = new SupportIncidentCloseAction(incident);
			action.SendSoftwareQuote = true;
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;
			action.Comment = "Please find quote attached.";
			action.SynchroniseToIncident();
			AssertEquals(TestDateAttribute.Date, action.Incident.Quote.CIQ_QuoteSentDateUTC);
		}

		public void TestCriticality()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var action = new SupportIncidentCloseAction(incident);
			AssertEquals(Constants.CustomerService.CriticalityCodes.CR5_Training, action.Criticality);

			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;
			action.RunPreSaveValidation();
			AssertNoErrors(action.CriticalityInfo);

			action.Criticality = "XXX";
			action.RunPreSaveValidation();
			AssertHasErrors(action.CriticalityInfo);

			action.Criticality = "";
			action.RunPreSaveValidation();
			AssertHasErrors(action.CriticalityInfo);

			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.FeatureAccepted;
			action.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			action.RunPreSaveValidation();
			AssertHasErrors(action.CriticalityInfo);

			action.Criticality = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			action.RunPreSaveValidation();
			AssertNoErrors(action.CriticalityInfo);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			action.SynchroniseToIncident();
			AssertEquals(Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest, incident.IM_Priority);
		}

		public void TestValidateCriticality_CheckIncidentSupportsCr8Cr9()
		{
			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;

			var releaseBuild = Factory.New<ReleaseBuild>();

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = releaseBuild.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;

			var action = new SupportIncidentCloseAction(incident);

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			releaseBuild.VersionNumber = new VersionNumber("1.1.2.0");
			AssertEquals("Precondition", false, incident.ClientSupportsCr8Cr9);
			CombineAssertions("Should have warning when trying to escalate to cr8/cr9 if client doesn't support", () =>
			{
				incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				action.Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				action.RunPreSaveValidation();
				AssertNoErrors("CR8", action.CriticalityInfo);
				AssertHasWarning("CR8", action.CriticalityInfo, "The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");

				incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
				action.Criticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				action.RunPreSaveValidation();
				AssertNoErrors("CR9", action.CriticalityInfo);
				AssertHasWarning("CR9", action.CriticalityInfo, "The version that the client is currently on does not support CR8/CR9 criticalities. It will appear as CR5 for the client.");
			});

			releaseBuild.VersionNumber = new VersionNumber("1.1.2.2");
			AssertEquals("Precondition", true, incident.ClientSupportsCr8Cr9);
			CombineAssertions("No error nor warning when trying to escalate to cr8/cr9 if client supports", () =>
			{
				incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				action.Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				action.RunPreSaveValidation();
				AssertNoErrors("CR8", action.CriticalityInfo);
				AssertNoWarnings("CR8", action.CriticalityInfo);

				incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
				action.Criticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				action.RunPreSaveValidation();
				AssertNoErrors("CR9", action.CriticalityInfo);
				AssertNoWarnings("CR9", action.CriticalityInfo);
			});
		}

		public void TestValidateResolutionComment()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);

			action.Comment = "";
			AssertNoErrors(action.CommentInfo);

			action.ResolutionMethod = "OTH";
			action.Comment = "a";
			action.Comment = "";
			AssertHasErrors(action.CommentInfo);

			action.Comment = "hello";
			AssertHasErrors(action.CommentInfo);

			action.Comment = "lo hello hello hello";
			AssertNoErrors(action.CommentInfo);

			action.ResolutionMethod = "PDM";
			action.Comment = "hello";
			AssertNoErrors(action.CommentInfo);
		}

		public void TestValidateResolutionMethod()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);

			action.ResolutionMethod = "a";
			AssertHasErrors(action.ResolutionMethodInfo);

			action.ResolutionMethod = "XXX";
			AssertHasErrors(action.ResolutionMethodInfo);

			action.ResolutionMethod = "";
			AssertHasErrors(action.ResolutionMethodInfo);

			incident.RelatedWorkItems.AddNew();
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			AssertHasError(action.ResolutionMethodInfo, "Cannot be marked as Awaiting Auto Upgrade if the incident is not linked to a database");
		}

		public void TestValidateResolutionMethod_FeatureRequest()
		{
			var incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			var action = new SupportIncidentCloseAction(incident);

			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;
			action.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, incident.IM_ResolutionCode);

			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;
			action.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, incident.IM_ResolutionCode);
		}

		public void TestValidateResolutionMethod_CloseWithCancelledWorkitem()
		{
			var incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			incident.IM_LD = Factory.NewWithValidTestData<LicenceDatabase>().PK;
			var action = new SupportIncidentCloseAction(incident);

			var workitemClosed = incident.RelatedWorkItems.AddNew();
			workitemClosed.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var workitemCancelled = incident.RelatedWorkItems.AddNew();
			workitemCancelled.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled;
			AssertNoNotifications(action.ResolutionMethodInfo);
		}

		public void TestCloseStatusDispositionList()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCloseAction(incident);
			incident.SetupForNewCreatedFeatureRequest();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			action = new SupportIncidentCloseAction(incident);
			AssertEquals(false, action.ActiveCloseStatusDispositionList.ContainsCode(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided));
			AssertEquals(false, action.ActiveCloseStatusDispositionList.ContainsCode(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided));
			AssertEquals(true, action.ActiveCloseStatusDispositionList.ContainsCode(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted));
			AssertEquals(true, action.ActiveCloseStatusDispositionList.ContainsCode(SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted));
			AssertEquals(true, action.ActiveCloseStatusDispositionList.ContainsCode(SupportIncidentLookups.DispositionList.Constants.FormalQuotationDeclined));

			CombineAssertions(() =>
			{
				foreach (CodeDescriptionPair stagePair in new SupportIncidentCategoriesList())
				{
					incident.IM_Category = stagePair.Code;
					AssertNotEquals("There should be at least one close status disposition for stage " + stagePair.Description, 0, action.ActiveCloseStatusDispositionList.Count);
				}
			});
		}

		public void TestProductAreaList()
		{
			#region Test Data
			{
				var menuSectionCollection = new SystemProductCollection();
				var entProduct = menuSectionCollection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("AR1", "ediArchiveManager", ProductAreaList.Codes.ARC, false);
				entProduct.ModuleMappings.AddNew("CCC", "Test Module C", ProductAreaList.Codes.CUS, false);
				entProduct.ModuleMappings.AddNew("DDD", "Test Module D", ProductAreaList.Codes.CUS, false);

				var glwProduct = menuSectionCollection.AddNew("GLW", "Glow", false);
				glwProduct.ModuleMappings.AddNew("WEB", "Web Module", ProductAreaList.Codes.PAV, false);

				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuSectionCollection);
			}
			{
				var cr8Collection = new SystemProductCollection();
				var entProduct = cr8Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("8EA", "8EA", ProductAreaList.Codes.GEO, false);
				entProduct.ModuleMappings.AddNew("8EB", "8EB", ProductAreaList.Codes.CUS, false);

				var glwProduct = cr8Collection.AddNew("GLW", "Glow", true);
				glwProduct.ModuleMappings.AddNew("8GA", "8GA", ProductAreaList.Codes.CIL, false);
				glwProduct.ModuleMappings.AddNew("8GB", "8GB", ProductAreaList.Codes.CIL, false);

				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Collection);
			}
			{
				var cr9Collection = new SystemProductCollection();
				var entProduct = cr9Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("9EA", "9EA", ProductAreaList.Codes.RAT, false);

				var glwProduct = cr9Collection.AddNew("GLW", "Glow", true);

				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Collection);
			}
			#endregion

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			var action = new SupportIncidentCloseAction(incident);
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided;
			action.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ProductAreaList.Codes.ARC,
					ProductAreaList.Codes.CUS
				},
				action.ProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

			action.Criticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ProductAreaList.Codes.GEO,
					ProductAreaList.Codes.CUS
				},
				action.ProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

			action.Criticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ProductAreaList.Codes.RAT
				},
				action.ProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestRecalculateProductAreaOnModuleChange()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "desc", true);
			var sysMapping = product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			sysMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Category = "SUP";
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			incident.IM_Module = "XXX";

			AssertEquals("PA1", incident.ProductArea);

			Factory.Save();

			var action = new SupportIncidentCloseAction(incident);
			action.Criticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			action.SendSoftwareQuote = true;
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			action.Comment = "Please find estimate attached.";
			action.SectionRequirementService = "XXX";

			action.SynchroniseToIncident();

			AssertEquals("CR4", incident.IM_Priority);
			AssertEquals("XXX", incident.IM_Module);
			AssertEquals("PA1", incident.ProductArea);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<SupportIncident>();
			return new SupportIncidentCloseAction(incident);
		}
	}
}
