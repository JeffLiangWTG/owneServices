using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentManagementGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			var newCode1 = "OZZ";
			var otherIncidentTypeCode = "OTH";
			var otherIncidentTypeDescription = "Other Management group type";
			var newCode2 = "OZY";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(newCode1, "description", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			var otherGroup = registryValue.AddNew(otherIncidentTypeCode, otherIncidentTypeDescription);
			otherGroup.IncidentGroupStatusConfigurations.AddNew(newCode2, "description2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals(2, group.Lookups.Types.Count);
			Assert(group.Lookups.Types.Contains(new CodeDescriptionPair(IncidentGroupStatusConfigurationConstants.MajorIncidentCode, IncidentGroupStatusConfigurationConstants.MajorIncidentDescription)));
			Assert(group.Lookups.Types.Contains(new CodeDescriptionPair(otherIncidentTypeCode, otherIncidentTypeDescription)));
		}

		public void TestStageList()
		{
			var newCode1 = "OZZ";
			var newDescription1 = "description";
			var otherIncidentTypeCode = "OTH";
			var newCode2 = "OZY";
			var newDescription2 = "description2";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(newCode1, newDescription1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			var otherGroup = registryValue.AddNew(otherIncidentTypeCode, "Other Management group type");
			otherGroup.IncidentGroupStatusConfigurations.AddNew(newCode2, newDescription2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals(string.Empty, group.ING_Type);
			AssertEquals(0, group.Lookups.StageList.Count);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			AssertEquals(6, group.Lookups.StageList.Count);
			Assert(group.Lookups.StageList.Contains(new CodeDescriptionPair(IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, IncidentGroupStatusConfigurationConstants.ConfigurationINV.DescriptionOnGroup)));
			Assert(group.Lookups.StageList.Contains(new CodeDescriptionPair(newCode1, newDescription1)));

			group.ING_Type = otherIncidentTypeCode;
			Assert(group.Lookups.StageList.Contains(new CodeDescriptionPair(IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, IncidentGroupStatusConfigurationConstants.ConfigurationINV.DescriptionOnGroup)));
			Assert(group.Lookups.StageList.Contains(new CodeDescriptionPair(newCode2, newDescription2)));
		}

		public void TestStageListShouldOnlyShowEnabled()
		{
			var newCode1 = "OZZ";
			var newDescription1 = "description";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(newCode1, newDescription1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, enabled: false);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			AssertEquals(string.Empty, group.ING_Type);
			AssertEquals(0, group.Lookups.StageList.Count);

			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			AssertEquals("PRecondition", 6, EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value.Cast<IncidentGroupType>().FirstOrDefault(x => x.GroupType.EqualsIgnoringCase(group.ING_Type)).IncidentGroupStatusConfigurations.Count);
			AssertEquals(5, group.Lookups.StageList.Count);
			Assert(group.Lookups.StageList.Contains(new CodeDescriptionPair(IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, IncidentGroupStatusConfigurationConstants.ConfigurationINV.DescriptionOnGroup)));
			Assert(!group.Lookups.StageList.Contains(new CodeDescriptionPair(newCode1, newDescription1)));
		}

		public void TestProductListShouldMatchSupportIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var expectedList = incident.Lookups.ProductList;

			AssertEquals(expectedList, group.Lookups.ProductList);
			AssertEquals(expectedList, new SupportIncidentLookups(Factory).ProductList);
		}

		public void TestProductAreaListShouldMatchSupportIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			AssertEquals(incident.Lookups.FilteredProductAreaList, group.Lookups.ProductAreaList);
		}

		public void TestCriticalityListShouldMatchSupportIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			AssertEquals(incident.Lookups.CriticalityList, group.Lookups.CriticalityList);
		}

		public void TestModuleListEnabledModulesOnlyShouldMatchSupportIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			incident.IM_Product = group.ING_Product;

			AssertEquals(incident.Lookups.ModuleListEnabledModulesOnly, group.Lookups.ModuleListEnabledModulesOnly);
		}

		public void TestIncidentsNotLinked()
		{
			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();

			link1.INL_ING_Group = group1.PK;
			link1.INL_IM_Incident = incident1.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			group1.Lookups.IncidentsNotLinked.Load();
			AssertCollectionNotContains(incident1, group1.Lookups.IncidentsNotLinked);
			AssertCollectionContains(incident2, group1.Lookups.IncidentsNotLinked);

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			link2.INL_ING_Group = group2.PK;
			link2.INL_IM_Incident = incident2.PK;
			Factory.Save();

			group1.Lookups.IncidentsNotLinked.Load();
			AssertCollectionNotContains(incident1, group1.Lookups.IncidentsNotLinked);
			AssertCollectionNotContains(incident2, group1.Lookups.IncidentsNotLinked);
		}

		public void TestReversible()
		{
			var incidentGroupStatusConfigurations =
				EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value.Cast<IncidentGroupType>().First().IncidentGroupStatusConfigurations;

			AssertEquals("Default should be false", true, incidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().All(x => !x.IsReversible));
			incidentGroupStatusConfigurations[0].IsReversible = true;
			AssertEquals("Value was changed", true, incidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().Any(x => x.IsReversible));
		}
	}
}
