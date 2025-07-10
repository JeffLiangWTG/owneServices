using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidsFilterForOrg))]
	sealed class ModuleGuidsFilterForOrgTest : ModuleFilterTestCase<ModuleGuidsFilterForOrg>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Organisations;

		protected override FilterCategory InitialTestCatergory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ModuleGuidsFilterForOrg GetNewModuleFilter()
		{
			var list1 = new StmNoteNonDependentCollection(Factory);
			var list2 = new StmNoteNonDependentCollection(Factory);
			return new ModuleGuidsFilterForOrg("moo", ModuleIDs.Organisation, DummyBizoSchema.Z0_Guid, list1, DummyBizoSchema.Z0_Guid, list2);
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(ModuleGuidsFilterForOrg filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.PropertyCode1), nameof(filter.PropertyCode2) }).ToArray();
		}

		public void TestValidateInactiveOrg()
		{
			var activeOrg = Factory.New<IOrgHeader>();
			activeOrg.OH_IsActive = true;
			activeOrg.OH_Code = "ORG1";
			activeOrg.OH_FullName = "Org Header 1";

			var inactiveOrg = Factory.New<IOrgHeader>();
			inactiveOrg.OH_IsActive = false;
			inactiveOrg.OH_Code = "ORG2";
			inactiveOrg.OH_FullName = "Org Header 2";
			Factory.Save();

			Filter.Property1 = activeOrg.PK;
			Filter.Property2 = inactiveOrg.PK;
			AssertNoWarning(Filter.Property1Info, CommonValidationMessage.WarningMessage);
			AssertHasWarning(Filter.Property2Info, CommonValidationMessage.WarningMessage);

			Filter.Property1 = inactiveOrg.PK;
			Filter.Property2 = activeOrg.PK;
			AssertHasWarning(Filter.Property1Info, CommonValidationMessage.WarningMessage);
			AssertNoWarning(Filter.Property2Info, CommonValidationMessage.WarningMessage);
		}
	}
}
