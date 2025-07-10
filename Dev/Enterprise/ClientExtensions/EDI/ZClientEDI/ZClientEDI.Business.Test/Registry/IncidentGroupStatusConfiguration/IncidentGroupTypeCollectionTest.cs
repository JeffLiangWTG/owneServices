using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IncidentGroupTypeCollection))]
	public class IncidentGroupTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IncidentGroupTypeCollection>
	{
		public void TestGetActiveStages()
		{
			var collection = GetCollectionToTest();
			var groupType1 = collection.AddNew("AAA", "number1");
			var groupType2 = collection.AddNew("BBB", "number2");

			groupType1.IncidentGroupStatusConfigurations.AddNew("C01", "stage1", IncidentGroupStatusConfigurationConstants.TriggerOnNew);
			groupType1.IncidentGroupStatusConfigurations.AddNew("C02", "stage2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			groupType2.IncidentGroupStatusConfigurations.AddNew("D01", "page1", IncidentGroupStatusConfigurationConstants.TriggerOnNew);
			groupType2.IncidentGroupStatusConfigurations.AddNew("D02", "page2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, enabled: false);

			AssertEquals(groupType1.IncidentGroupStatusConfigurations.Count, collection.GetActiveStages(groupType1.GroupType).Count);
			var type1Stages = collection.GetActiveStages(groupType1.GroupType).Cast<IncidentGroupStatusConfiguration>();
			AssertNotNull(type1Stages.FirstOrDefault(x => x.Code.EqualsIgnoringCase("C01")));
			AssertNotNull(type1Stages.FirstOrDefault(x => x.Code.EqualsIgnoringCase("C02")));
			AssertEquals(groupType2.IncidentGroupStatusConfigurations.Count - 1, collection.GetActiveStages(groupType2.GroupType).Count);
			var type2Stages = collection.GetActiveStages(groupType2.GroupType).Cast<IncidentGroupStatusConfiguration>();
			AssertNotNull(type2Stages.FirstOrDefault(x => x.Code.EqualsIgnoringCase("D01")));
			AssertNull(type2Stages.FirstOrDefault(x => x.Code.EqualsIgnoringCase("D02")));
		}

		public void TestGetAllStages()
		{
			var collection = GetCollectionToTest();
			collection.RemoveAndDeleteAll();
			var groupType1 = collection.AddNew("AAA", "number1");
			var groupType2 = collection.AddNew("BBB", "number2");

			groupType1.IncidentGroupStatusConfigurations.RemoveAndDeleteAll();
			groupType2.IncidentGroupStatusConfigurations.RemoveAndDeleteAll();

			string[] codes = new string[] { "ABC","C02","D01","D02","ABC" };

			groupType1.IncidentGroupStatusConfigurations.AddNew(codes[0], "stage1", IncidentGroupStatusConfigurationConstants.TriggerOnNew);
			groupType1.IncidentGroupStatusConfigurations.AddNew(codes[1], "stage2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			groupType2.IncidentGroupStatusConfigurations.AddNew(codes[2], "page1", IncidentGroupStatusConfigurationConstants.TriggerOnNew);
			groupType2.IncidentGroupStatusConfigurations.AddNew(codes[3], "page2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			groupType2.IncidentGroupStatusConfigurations.AddNew(codes[4], "page2", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);

			var query = collection.GetAllStages();

			var stageList = new CodeDescriptionPairList();
			collection.GetAllStages().Cast<IncidentGroupStatusConfiguration>().ForEach(x => stageList.AddPair(x.Code, x.DescriptionOnGroup));

			AssertEquals(codes.Length - 1, stageList.Count);
			foreach (var code in codes.Distinct())
			{
				Assert($"{code} missed", stageList.ContainsCode(code));
			}
		}

		#region Implementation
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override IncidentGroupTypeCollection GetCollectionToTest()
		{
			return new IncidentGroupTypeCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IncidentGroupType(NewFallbackLevel(), Factory);
		}
		#endregion
	}
}
