using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(EDIProjectFilterBusinessObject))]
	public class ProjectFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		[TestDate(2014, 5, 13)]
		public void TestFollowUpAndInstallFilters()
		{
			var project1 = Factory.NewWithValidTestData<EDIProject>();
			project1.CallbackBy = new ZDateTime(2014, 1, 1);
			project1.PlannedInstall = new ZDateTime(2014, 1, 1);
			project1.InstallDate = new ZDateTime(2014, 1, 1);
			var project2 = Factory.NewWithValidTestData<EDIProject>();
			project2.CallbackBy = new ZDateTime(2014, 2, 28);
			project2.PlannedInstall = new ZDateTime(2014, 2, 28);
			project2.InstallDate = new ZDateTime(2014, 2, 28);
			var project3 = Factory.NewWithValidTestData<EDIProject>();
			project3.CallbackBy = new ZDateTime(2014, 3, 15);
			project3.PlannedInstall = new ZDateTime(2014, 3, 15);
			project3.InstallDate = new ZDateTime(2014, 3, 15);
			Factory.Save();
			ModuleDateFilter followUpFilter = (ModuleDateFilter)FilterBizO["Follow Up Date"];
			followUpFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			ModuleDateFilter plannedInstallFilter = (ModuleDateFilter)FilterBizO["Planned Install"];
			plannedInstallFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			ModuleDateFilter installDateFilter = (ModuleDateFilter)FilterBizO["Install Date"];
			installDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertFollowUpOrInstallDateFilter(followUpFilter, project1, project2, project3);
			AssertFollowUpOrInstallDateFilter(plannedInstallFilter, project1, project2, project3);
			AssertFollowUpOrInstallDateFilter(installDateFilter, project1, project2, project3);
		}

		void AssertFollowUpOrInstallDateFilter(ModuleDateFilter filter, EDIProject project1, EDIProject project2, EDIProject project3)
		{
			ProjectCollection collection = new ProjectCollection(Factory);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2014, 3, 15);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(project3, collection);
			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = new ZDateTime(2014, 3, 14);
			collection.Load(FilterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(project1, collection);
			AssertCollectionContains(project2, collection);
			filter.IsActive = false;
		}

		public void TestGoLiveFilters()
		{
			EDIProject project1 = Factory.NewWithValidTestData<EDIProject>();
			project1.LicenceHeaderPK = Factory.NewWithValidTestData<LicenceHeader>().PK;
			project1.Licence.LA_EstimatedLiveDate = new ZDateTime(2009, 2, 15);
			EDIProject project2 = Factory.NewWithValidTestData<EDIProject>();
			project2.LicenceHeaderPK = Factory.NewWithValidTestData<LicenceHeader>().PK;
			project2.Licence.LA_EstimatedLiveDate = new ZDateTime(2009, 1, 26);
			project2.Licence.LA_SiteLiveDate = new ZDateTime(2009, 1, 29);
			EDIProject project3 = Factory.NewWithValidTestData<EDIProject>();
			project3.LicenceHeaderPK = Factory.NewWithValidTestData<LicenceHeader>().PK;
			project3.Licence.LA_SiteLiveDate = new ZDateTime(2009, 2, 4);
			EDIProject project4 = Factory.NewWithValidTestData<EDIProject>();
			project4.LicenceHeaderPK = Factory.NewWithValidTestData<LicenceHeader>().PK;
			project4.Licence.LA_EstimatedLiveDate = new ZDateTime(2009, 1, 27);
			EDIProject project5 = Factory.NewWithValidTestData<EDIProject>();
			EDIProject project6 = Factory.NewWithValidTestData<EDIProject>();
			project6.LicenceHeaderPK = Factory.NewWithValidTestData<LicenceHeader>().PK;
			project6.Licence.LA_AgreedLiveDate = new ZDateTime(1998, 6, 30);
			Factory.Save();
			EDIProjectFilterBusinessObject filterBizO = new EDIProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);
			ModuleDateFilter plannedGoLiveFilter = (ModuleDateFilter)filterBizO["Planned Go-Live"];
			plannedGoLiveFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			ModuleDateFilter goLiveCompleteFilter = (ModuleDateFilter)filterBizO["Go-Live Complete"];
			goLiveCompleteFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			ModuleDateFilter agreedGoLiveFilter = (ModuleDateFilter)FilterBizO["Agreed Go-Live"];
			agreedGoLiveFilter.IsActive = true;
			agreedGoLiveFilter.Property1 = new ZDate(1998, 6, 30);
			collection.Load(filterBizO.Filter);
			AssertEquals(6, collection.Count);
			AssertCollectionContains(project6, collection);
			plannedGoLiveFilter.IsActive = true;
			plannedGoLiveFilter.Property1 = new ZDateTime(2009, 2, 1);
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(project1, collection);
			plannedGoLiveFilter.Property1 = ZDateTime.Empty;
			plannedGoLiveFilter.Property2 = new ZDateTime(2009, 2, 1);
			collection.Load(filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(project2, collection);
			AssertCollectionContains(project4, collection);
			goLiveCompleteFilter.IsActive = true;
			goLiveCompleteFilter.Property1 = ZDateTime.Empty;
			goLiveCompleteFilter.Property2 = new ZDateTime(2009, 2, 1);
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(project2, collection);
			plannedGoLiveFilter.IsActive = false;
			goLiveCompleteFilter.Property1 = new ZDateTime(2009, 2, 1);
			goLiveCompleteFilter.Property2 = ZDateTime.Empty;
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(project3, collection);
		}

		public void TestOldProjectIDFilter()
		{
			EDIProject project1 = Factory.New<EDIProject>();
			project1.WKP_ProjectNumber = "PRO00000234";
			EDIProject project2 = Factory.New<EDIProject>();
			project2.WKP_ProjectNumber = "PRJ00000234";
			EDIProjectFilterBusinessObject filterBizO = new EDIProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);
			ModuleFountainFilter oldProjectIdFilter = ((ModuleFountainFilter)filterBizO["Old Project ID"]);
			oldProjectIdFilter.IsActive = true;
			oldProjectIdFilter.Property = "PRO00000234";
			collection.Load(filterBizO.Filter);
			AssertEquals(1, collection.Count);
		}

		public void TestRelatedWorkItemFilters()
		{
			EDIProject project1 = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest1 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest1.SetupForProjectFeatureRequest();
			featureRequest1.RelatedProjectPK = project1.PK;
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_WorkItemNumber = "WI00001001";
			featureRequest1.RelatedWorkItems.Add(workItem1);
			EDIProject project2 = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest2 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest2.SetupForProjectFeatureRequest();
			featureRequest2.RelatedProjectPK = project2.PK;
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem2.WKI_WorkItemNumber = "WI00001002";
			featureRequest2.RelatedItems.Add(workItem2);
			EDIProject project3 = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest3 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest3.SetupForProjectFeatureRequest();
			project3.RelatedItems.Add(featureRequest3);
			NewWorkItem workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem3.WKI_WorkItemNumber = "WI00001003";
			featureRequest3.RelatedWorkItems.Add(workItem3);
			EDIProject project4 = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest4 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest4.SetupForProjectFeatureRequest();
			project4.RelatedItems.Add(featureRequest4);
			NewWorkItem workItem4 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem4.WKI_WorkItemNumber = "WI00001004";
			featureRequest4.RelatedItems.Add(workItem4);
			EDIProject project5 = Factory.NewWithValidTestData<EDIProject>();
			NewWorkItem workItem5 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem5.WKI_WorkItemNumber = "WI00001005";
			workItem5.RelatedItems.Add(project5);
			EDIProject project6 = Factory.NewWithValidTestData<EDIProject>();
			NewWorkItem workItem6 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem6.WKI_WorkItemNumber = "WI00001006";
			project6.RelatedItems.Add(workItem6);
			Factory.Save();
			ModuleTextFilter workItemNumberFilter = (ModuleTextFilter)FilterBizO["Work Item Number"];
			workItemNumberFilter.IsActive = true;
			workItemNumberFilter.Property = "1001";
			EDIProject[] projects = Factory.Load<EDIProject>(FilterBizO.Filter);
			AssertEquals(1, projects.Length);
			AssertEquals(project1, projects[0]);
			workItemNumberFilter.Property = "1002";
			projects = Factory.Load<EDIProject>(FilterBizO.Filter);
			AssertEquals(1, projects.Length);
			AssertEquals(project2, projects[0]);
			workItemNumberFilter.Property = "1003";
			projects = Factory.Load<EDIProject>(FilterBizO.Filter);
			AssertEquals(1, projects.Length);
			AssertEquals(project3, projects[0]);
			workItemNumberFilter.Property = "1004";
			projects = Factory.Load<EDIProject>(FilterBizO.Filter);
			AssertEquals(1, projects.Length);
			AssertEquals(project4, projects[0]);
			workItemNumberFilter.Property = "1005";
			projects = Factory.Load<EDIProject>(FilterBizO.Filter);
			AssertEquals(1, projects.Length);
			AssertEquals(project5, projects[0]);
			workItemNumberFilter.Property = "1006";
			projects = Factory.Load<EDIProject>(FilterBizO.Filter);
			AssertEquals(1, projects.Length);
			AssertEquals(project6, projects[0]);
		}

		public void TestEnterpriseCodeFilter()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var address1 = org1.Addresses[0];
			var address2 = org2.Addresses[0];
			var address3 = org3.Addresses[0];
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org2.Contacts.AddNew();
			var contact3 = org3.Contacts.AddNew();
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_OH = org1.PK;
			var company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise1.PK;
			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_OH = org2.PK;
			var company2 = Factory.NewWithValidTestData<LicenceCompany>();
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise2.PK;
			var company3 = Factory.NewWithValidTestData<LicenceCompany>();
			company3.LC_OH = org3.PK;
			company3.LC_LE = enterprise2.PK;
			var project1 = Factory.NewWithValidTestData<EDIProject>();
			project1.WKP_OA_ClientAddress = address1.PK;
			project1.WKP_OC_Contact = contact1.PK;
			var project2 = Factory.NewWithValidTestData<EDIProject>();
			project2.WKP_OA_ClientAddress = address2.PK;
			project2.WKP_OC_Contact = contact2.PK;
			var project3 = Factory.NewWithValidTestData<EDIProject>();
			project3.WKP_OA_ClientAddress = address3.PK;
			project3.WKP_OC_Contact = contact3.PK;
			Factory.Save();
			FilterBizO["Enterprise Code"].IsActive = true;
			((ModuleGuidFilter)FilterBizO["Enterprise Code"]).Property = enterprise1.PK;
			var collection = new ProjectCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(project1, collection);
			((ModuleGuidFilter)FilterBizO["Enterprise Code"]).Property = enterprise2.PK;
			collection.Load(FilterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(project2, collection);
			AssertCollectionContains(project3, collection);
		}

		public void TestEnterpriseIDFilter()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var address1 = org1.Addresses[0];
			var address2 = org2.Addresses[0];
			var address3 = org3.Addresses[0];
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org2.Contacts.AddNew();
			var contact3 = org3.Contacts.AddNew();
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_OH = org1.PK;
			var company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_OH = org1.PK;
			company1.LC_LE = enterprise1.PK;
			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_OH = org2.PK;
			var company2 = Factory.NewWithValidTestData<LicenceCompany>();
			company2.LC_OH = org2.PK;
			company2.LC_LE = enterprise2.PK;
			var company3 = Factory.NewWithValidTestData<LicenceCompany>();
			company3.LC_OH = org3.PK;
			company3.LC_LE = enterprise2.PK;
			var project1 = Factory.NewWithValidTestData<EDIProject>();
			project1.WKP_OA_ClientAddress = address1.PK;
			project1.WKP_OC_Contact = contact1.PK;
			var project2 = Factory.NewWithValidTestData<EDIProject>();
			project2.WKP_OA_ClientAddress = address2.PK;
			project2.WKP_OC_Contact = contact2.PK;
			var project3 = Factory.NewWithValidTestData<EDIProject>();
			project3.WKP_OA_ClientAddress = address3.PK;
			project3.WKP_OC_Contact = contact3.PK;
			Factory.Save();
			FilterBizO["Enterprise ID"].IsActive = true;
			((ModuleGuidFilter)FilterBizO["Enterprise ID"]).Property = enterprise1.PK;
			var collection = new ProjectCollection(Factory);
			collection.Load(FilterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(project1, collection);
			((ModuleGuidFilter)FilterBizO["Enterprise ID"]).Property = enterprise2.PK;
			collection.Load(FilterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(project2, collection);
			AssertCollectionContains(project3, collection);
		}

		EDIProjectFilterBusinessObject FilterBizO
		{
			get
			{
				return (EDIProjectFilterBusinessObject)CachedBusinessObject;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIProjectFilterBusinessObject();
		}
	}
}
