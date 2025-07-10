using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(EDIWorkItemFilterBusinessObject))]
	public class EDIWorkItemFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLegacyTaskNumberFilter()
		{
			var workItem = Factory.NewWithValidTestData<ProcessManagement.Business.WorkItem>();
			workItem.WKI_WorkItemNumber = "T00001";
			workItem.WKI_WorkItemType = "ENG";
			Factory.Save();
			var filters = new EDIWorkItemFilterBusinessObject();
			var filter = ((ModuleFountainFilter)filters["Legacy Task Number"]);
			filter.Property = "T00001";
			filter.IsActive = true;
			var items = new ProcessManagement.Business.WorkItemCollection(Factory, filters.Filter);
			AssertEquals(1, items.Count);
			filter.Property = "T00002";
			items = new ProcessManagement.Business.WorkItemCollection(Factory, filters.Filter);
			AssertEquals(0, items.Count);
		}

		public void TestRelatedIncidentFilter()
		{
			#region Setup test data
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem4 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem5 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem6 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem7 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem8 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem9 = Factory.NewWithValidTestData<NewWorkItem>();
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client1.PK;
			ProfessionalServicesQuote profServQuote1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			profServQuote1.IM_OH_Client = client2.PK;
			ProfessionalServicesQuote profServQuote2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			profServQuote2.IM_OH_Client = client3.PK;
			ProfessionalServicesQuote profServQuote3 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			profServQuote3.IM_OH_Client = client1.PK;
			incident.RelatedWorkItems.Add(workItem1);
			incident.RelatedWorkItems.Add(workItem2);
			profServQuote1.RelatedWorkItems.Add(workItem3);
			profServQuote1.RelatedWorkItems.Add(workItem4);
			profServQuote1.RelatedWorkItems.Add(workItem5);
			profServQuote2.RelatedWorkItems.Add(workItem6);
			profServQuote3.RelatedWorkItems.Add(workItem7);
			incident.RelatedWorkItems.Add(workItem8);
			Factory.Save();
			#endregion
			EDIWorkItemFilterBusinessObject filter = new EDIWorkItemFilterBusinessObject();
			NewWorkItemCollection workItems = new NewWorkItemCollection(Factory);
			((ModuleGuidFilter)filter["Related Client"]).Property = client1.PK;
			((ModuleGuidFilter)filter["Related Client"]).IsActive = true;
			workItems.Load(filter.Filter);
			AssertEquals("WorkItems.Count", 4, workItems.Count);
			Assert("Correct work items loaded", workItems.Contains(workItem1.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem2.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem3.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem4.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem5.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem6.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem7.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem8.PK));
			((ModuleGuidFilter)filter["Related Client"]).Property = client2.PK;
			workItems.Load(filter.Filter);
			AssertEquals("WorkItems.Count", 3, workItems.Count);
			Assert("Correct work items loaded", !workItems.Contains(workItem1.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem2.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem3.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem4.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem5.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem6.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem7.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem8.PK));
			((ModuleGuidFilter)filter["Related Client"]).Property = client3.PK;
			workItems.Load(filter.Filter);
			AssertEquals("WorkItems.Count", 1, workItems.Count);
			Assert("Correct work items loaded", !workItems.Contains(workItem1.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem2.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem3.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem4.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem5.PK));
			Assert("Correct work items loaded", workItems.Contains(workItem6.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem7.PK));
			Assert("Correct work items loaded", !workItems.Contains(workItem8.PK));
		}

		public void TestIncidentFiltersDontHaveIsBlank()
		{
			EDIWorkItemFilterBusinessObject filter = new EDIWorkItemFilterBusinessObject();
			var hasIsBlank = ((ModuleGuidFilter)filter["Related Client"]).ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			var hasIsNotBlank = ((ModuleGuidFilter)filter["Related Client"]).ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			AssertFilterHasNoBlankOptions(hasIsBlank, hasIsNotBlank);
			hasIsBlank = ((ModuleFountainFilter)filter["Incident Number"]).ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			hasIsNotBlank = ((ModuleFountainFilter)filter["Incident Number"]).ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			AssertFilterHasNoBlankOptions(hasIsBlank, hasIsNotBlank);
			hasIsBlank = ((ModuleTextFilter)filter["Incident Criticality"]).ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			hasIsNotBlank = ((ModuleTextFilter)filter["Incident Criticality"]).ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			AssertFilterHasNoBlankOptions(hasIsBlank, hasIsNotBlank);
		}

		void AssertFilterHasNoBlankOptions(bool isBlank, bool isNotBlank)
		{
			AssertEquals("no isblank filter option", false, isBlank);
			AssertEquals("no isnotblank filter option", false, isNotBlank);
		}

		public void TestIncidentNumberFilter()
		{
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "CS00000001";
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			incident1.RelatedWorkItems.Add(workItem1);
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "CS00000002";
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			incident2.RelatedWorkItems.Add(workItem2);
			Factory.Save();
			ModuleFountainFilter numberFilter = (ModuleFountainFilter)FilterBizO["Incident Number"];
			numberFilter.IsActive = true;
			numberFilter.Property = "1";
			NewWorkItem[] workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem1, workItems[0]);
			numberFilter.Property = "2";
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem2, workItems[0]);
		}

		public void TestRelatedProjectFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST2";
			EDIProject project1 = Factory.NewWithValidTestData<EDIProject>();
			project1.WKP_ProjectNumber = "PRJ00000001";
			project1.WKP_GS_NKProjectManager = staff1.GS_Code;
			SupportIncident featureRequest1 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest1.SetupForProjectFeatureRequest();
			featureRequest1.RelatedProjectPK = project1.PK;
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			featureRequest1.RelatedWorkItems.Add(workItem1);
			EDIProject project2 = Factory.NewWithValidTestData<EDIProject>();
			project2.WKP_ProjectNumber = "PRJ00000002";
			project2.WKP_GS_NKProjectManager = staff1.GS_Code;
			SupportIncident featureRequest2 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest2.SetupForProjectFeatureRequest();
			featureRequest2.RelatedProjectPK = project2.PK;
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			featureRequest2.RelatedWorkItems.Add(workItem2);
			EDIProject project3 = Factory.NewWithValidTestData<EDIProject>();
			project3.WKP_ProjectNumber = "PRJ00000003";
			project3.WKP_GS_NKProjectManager = staff2.GS_Code;
			SupportIncident featureRequest3 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest3.SetupForProjectFeatureRequest();
			featureRequest3.RelatedProjectPK = project3.PK;
			NewWorkItem workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			featureRequest3.RelatedWorkItems.Add(workItem3);
			EDIProject project4 = Factory.NewWithValidTestData<EDIProject>();
			project4.WKP_ProjectNumber = "PRJ00000004";
			project4.WKP_GS_NKProjectManager = staff2.GS_Code;
			SupportIncident featureRequest4 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest4.SetupForProjectFeatureRequest();
			featureRequest4.RelatedProjectPK = project4.PK;
			NewWorkItem workItem4 = Factory.NewWithValidTestData<NewWorkItem>();
			featureRequest4.RelatedItems.Add(workItem4);
			EDIProject project5 = Factory.NewWithValidTestData<EDIProject>();
			project5.WKP_ProjectNumber = "PRJ00000005";
			project5.WKP_GS_NKProjectManager = staff2.GS_Code;
			SupportIncident featureRequest5 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest5.SetupForProjectFeatureRequest();
			project5.RelatedItems.Add(featureRequest5);
			NewWorkItem workItem5 = Factory.NewWithValidTestData<NewWorkItem>();
			featureRequest5.RelatedWorkItems.Add(workItem5);
			EDIProject project6 = Factory.NewWithValidTestData<EDIProject>();
			project6.WKP_ProjectNumber = "PRJ00000006";
			project6.WKP_GS_NKProjectManager = staff2.GS_Code;
			SupportIncident featureRequest6 = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest6.SetupForProjectFeatureRequest();
			project6.RelatedItems.Add(featureRequest6);
			NewWorkItem workItem6 = Factory.NewWithValidTestData<NewWorkItem>();
			featureRequest6.RelatedItems.Add(workItem6);
			EDIProject project7 = Factory.NewWithValidTestData<EDIProject>();
			project7.WKP_ProjectNumber = "PRJ00000007";
			project7.WKP_GS_NKProjectManager = staff2.GS_Code;
			NewWorkItem workItem7 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem7.RelatedItems.Add(project7);
			EDIProject project8 = Factory.NewWithValidTestData<EDIProject>();
			project8.WKP_ProjectNumber = "PRJ00000008";
			project8.WKP_GS_NKProjectManager = staff2.GS_Code;
			NewWorkItem workItem8 = Factory.NewWithValidTestData<NewWorkItem>();
			project8.RelatedItems.Add(workItem8);
			Factory.Save();
			ModuleTextFilter projectNumberFilter = (ModuleTextFilter)FilterBizO["Project Number"];
			projectNumberFilter.IsActive = true;
			projectNumberFilter.Property = "1";
			NewWorkItem[] workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem1, workItems[0]);
			projectNumberFilter.Property = "2";
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem2, workItems[0]);
			projectNumberFilter.Property = "3";
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem3, workItems[0]);
			projectNumberFilter.Property = "7";
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem7, workItems[0]);
			projectNumberFilter.Property = "8";
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(1, workItems.Length);
			AssertEquals(workItem8, workItems[0]);
			ModuleNkFilter projectManagerFilter = (ModuleNkFilter)FilterBizO["Project Manager"];
			projectManagerFilter.IsActive = true;
			projectManagerFilter.Property = staff1.GS_Code;
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(0, workItems.Length);
			projectNumberFilter.IsActive = false;
			workItems = Factory.Load<NewWorkItem>(FilterBizO.Filter);
			AssertEquals(2, workItems.Length);
			AssertCollectionContains(workItem1, workItems);
			AssertCollectionContains(workItem2, workItems);
			projectManagerFilter.Property = staff2.GS_Code;
			NewWorkItemCollection collection = new NewWorkItemCollection(Factory, FilterBizO.Filter);
			collection.Load();
			AssertEquals(6, collection.Count);
			AssertCollectionContains(workItem3, collection);
			AssertCollectionContains(workItem4, collection);
			AssertCollectionContains(workItem5, collection);
			AssertCollectionContains(workItem6, collection);
			AssertCollectionContains(workItem7, collection);
			AssertCollectionContains(workItem8, collection);
		}

		public void TestNumberOfRelatedIncidentsFilter()
		{
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem4 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem5 = Factory.NewWithValidTestData<NewWorkItem>();
			NewWorkItem workItem6 = Factory.NewWithValidTestData<NewWorkItem>();

			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident4 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident5 = Factory.NewWithValidTestData<SupportIncident>();

			EDIProject project1 = Factory.NewWithValidTestData<EDIProject>();
			EDIProject project2 = Factory.NewWithValidTestData<EDIProject>();

			incident1.RelatedItems.Add(workItem1);

			incident1.RelatedItems.Add(workItem2);
			incident2.RelatedItems.Add(workItem2);

			incident1.RelatedItems.Add(workItem3);
			incident2.RelatedItems.Add(workItem3);
			project1.RelatedItems.Add(workItem3);

			incident1.RelatedItems.Add(workItem4);
			incident2.RelatedItems.Add(workItem4);
			incident3.RelatedItems.Add(workItem4);

			incident1.RelatedItems.Add(workItem5);
			incident2.RelatedItems.Add(workItem5);
			incident3.RelatedItems.Add(workItem5);
			project1.RelatedItems.Add(workItem5);
			project2.RelatedItems.Add(workItem5);

			incident4.RelatedItems.Add(workItem6);
			incident5.RelatedItems.Add(workItem6);

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBizO["Number of related Incidents"];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 2;

			NewWorkItemCollection collection = new NewWorkItemCollection(Factory, FilterBizO.Filter);
			collection.Load();
			AssertEquals(4, collection.Count);

			AssertCollectionContains(workItem1, collection);
			AssertCollectionContains(workItem2, collection);
			AssertCollectionContains(workItem3, collection);
			AssertCollectionContains(workItem6, collection);
		}

		public void TestInitialCodeForSearch()
		{
			FilterBizO.SetInitialCodeForSearch("123", WorkItemSchema.WKI_WorkItemNumber.Name);
			string assertMessage = "NewWorkItemFilterBusinessObject should default to Work Item Number for the WKI_WorkItemNumber column";
			AssertEquals(assertMessage, string.Empty, ((ModuleFountainFilter)FilterBizO["Incident Number"]).Property);
			AssertEquals(assertMessage, "WI00000123", ((ModuleFountainFilter)FilterBizO["Work Item Number"]).Property);
		}

		public void TestInitialCodeForSearchWhenIndexSearchTypeShouldNotThrowException()
		{
			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();

			using (GetIndexSearchRegistryMock())
			using (GetGlowIndexQueryEngineForTest())
			{
				var filterBusinessObject = (EDIWorkItemFilterBusinessObject)GetNewFilterStripBusinessObject();
				filterBusinessObject.IndexSearchFields = GetSearchFieldCollection();
				filterBusinessObject.SearchType = SearchType.Index;
				filterBusinessObject.LoadModuleFilters();

				AssertNoExceptionThrown("Should not throw exception when trying to apply work item number", () => filterBusinessObject.SetInitialCodeForSearch(workItem1.WKI_WorkItemNumber, WorkItemSchema.WKI_WorkItemNumber.Name));

				var filter = filterBusinessObject["Common"] as IndexSearchModuleTextFilter;
				AssertEquals(workItem1.WKI_WorkItemNumber, filter.Property);
			}
		}

		public void TestReleaseBuildFilterValidation()
		{
			var releaseBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			releaseBuild.VersionNumber = new VersionNumber(1, 2, 3, 4);
			Factory.Save();

			var filter = (ModuleGuidsFilter)FilterBizO["Patched to Upgrade"];
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			filter.Validation.ValidateAll();
			AssertHasError(filter.Property1Info, "Please enter a value.");
			AssertHasError(filter.Property2Info, "Please enter a value.");

			filter.Property1 = ZGuid.NewZGuid();
			filter.Property2 = ZGuid.NewZGuid();
			filter.Validation.ValidateAll();
			AssertHasError(filter.Property1Info, "Enter a valid selection.");
			AssertHasError(filter.Property2Info, "Enter a valid selection.");

			filter.Property1 = releaseBuild.PK;
			filter.Property2 = releaseBuild.PK;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);
		}

		public void Test_class_RelatedIncidentFilterSubGroup_method_GetSubQuery()
		{
			(object objRelatedIncidentFilterSubGroup, MethodInfo methodGetSubQuery) = generate_instance_RelatedIncidentFilterSubGroup_method_GetSubQuery();
			AssertNotNull("objRelatedIncidentFilterSubGroup", objRelatedIncidentFilterSubGroup);
			AssertNotNull("methodGetSubQuery", methodGetSubQuery);

			ZQuery result = (ZQuery)methodGetSubQuery.Invoke(objRelatedIncidentFilterSubGroup, new object[] { new ZQuery() });
			AssertEquals("result.FilterPartsHashKey not contains : XX_RelationType = 'WRK' and XX_Relation1TableCode = 'WKI' and XX_Relation2TableCode = 'IM'",
				"WKI_PK IN (SELECT XX_Relation2ID FROM dbo.GenPivot WHERE XX_RelationType = 'WRK' and XX_Relation1TableCode = 'IM' and XX_Relation2TableCode = 'WKI' and XX_Relation1ID IN (SELECT IM_PK FROM dbo.IncidentMain))",
				result.FilterPartsHashKey);
		}

		(object objRelatedIncidentFilterSubGroup, MethodInfo methodGetSubQuery) generate_instance_RelatedIncidentFilterSubGroup_method_GetSubQuery()
		{
			Type[] innerTypeList = typeof(EDIWorkItemFilterBusinessObject).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals("innerTypeList.Count() > 0", expected: true, innerTypeList.Length > 0);
			foreach (Type innerType in innerTypeList)
			{
				if (innerType.Name != "RelatedIncidentFilterSubGroup")
				{
					continue;
				}

				ConstructorInfo ci = innerType.GetConstructor(Array.Empty<Type>());
				object relatedIncidentFilterSubGroup = ci.Invoke(Array.Empty<object>());

				MethodInfo methodGetSubQuery = innerType.GetMethod("GetSubQuery", new Type[1] { typeof(ZQuery) });

				return (relatedIncidentFilterSubGroup, methodGetSubQuery);
			}

			return (null, null);
		}

		#region Implementation
		EDIWorkItemFilterBusinessObject FilterBizO
		{
			get
			{
				return (EDIWorkItemFilterBusinessObject)CachedBusinessObject;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIWorkItemFilterBusinessObject();
		}

		IDisposable GetIndexSearchRegistryMock()
		{
			var registryMock = new Mock<IGlowRegistry>();
			_ = registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
			_ = registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
			return ObjectFactory.Substitute(registryMock.Object);
		}

		IDisposable GetGlowIndexQueryEngineForTest()
		{
			var mock = new Mock<IGlowIndexQueryEngine>();
			_ = mock.Setup(e => e.Query(It.IsAny<GlowIndexQueryParam>())).Returns(GetIndexQueryResultCollection);
			_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(GetSearchFieldCollection);
			_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IWorkItem" });
			return ObjectFactory.Substitute(mock.Object);
		}

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");
			var ret = new SearchFieldCollection("IWorkItem", new SearchField[] { field1, field2 });
			return ret;
		}

		GlowIndexQueryResultCollection GetIndexQueryResultCollection()
		{
			var ret = new GlowIndexQueryResultCollection();
			ret.Status = GlowIndexQueryStatus.Success;
			ret.Results.Add(new GlowIndexQueryResult(ZGuid.BrettsGuid.ToString(), "Dummy"));
			return ret;
		}

		#endregion
	}
}
