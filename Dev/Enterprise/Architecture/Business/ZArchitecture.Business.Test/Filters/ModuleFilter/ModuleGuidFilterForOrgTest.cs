using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidFilterForOrg))]
	sealed class ModuleGuidFilterForOrgTest : ModuleFilterTestCase<ModuleGuidFilterForOrg>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Organisations;

		protected override FilterCategory InitialTestCatergory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ModuleGuidFilterForOrg GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleGuidFilterForOrg("moo", ModuleIDs.Organisation, DummyBizoSchema.Z0_Guid, list);
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

			Filter.Property = activeOrg.PK;
			AssertNoWarning(Filter.PropertyInfo, CommonValidationMessage.WarningMessage);

			Filter.Property = ZGuid.Missing;
			AssertNoWarning(Filter.PropertyInfo, CommonValidationMessage.WarningMessage);

			Filter.Property = ZGuid.Empty;
			AssertNoWarning(Filter.PropertyInfo, CommonValidationMessage.WarningMessage);

			Filter.Property = ZGuid.Invalid;
			AssertNoWarning(Filter.PropertyInfo, CommonValidationMessage.WarningMessage);

			Filter.Property = inactiveOrg.PK;
			AssertHasWarning(Filter.PropertyInfo, CommonValidationMessage.WarningMessage);
		}
	}
}
