using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DependenceLookupBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new DependenceLookupBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			var filterBuilder =
				new DependenceLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(LookupField), filterBuilder.GetFilterField().GetType());
		}

		public void TestFilterFieldMaxLength()
		{
			var filterBuilder = new DependenceLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator,
				ReportRunningType.Report);
			var newField = new CodeLookupField(Factory);
			filterBuilder.SetupListAndModuleIDs("vessel", newField);
			AssertEquals(RefVesselSchema.RV_Code.MaxLength, newField.ZValueInfo.MaxLength);
			filterBuilder.SetupListAndModuleIDs(CollectionProviderTypeCodeDescriptionList.Codes.Unloco, newField);
			AssertEquals(RefUNLOCOSchema.RL_Code.MaxLength, newField.ZValueInfo.MaxLength);
			filterBuilder.SetupListAndModuleIDs("country", newField);
			AssertEquals(RefCountrySchema.RN_Code.MaxLength, newField.ZValueInfo.MaxLength);
			filterBuilder.SetupListAndModuleIDs(CollectionProviderTypeCodeDescriptionList.Codes.Address, newField);
			AssertEquals(OrgAddressSchema.OA_Code.MaxLength, newField.ZValueInfo.MaxLength);
			filterBuilder.SetupListAndModuleIDs(CollectionProviderTypeCodeDescriptionList.Codes.Staff, newField);
			AssertEquals(GlbStaffSchema.GS_Code.MaxLength, newField.ZValueInfo.MaxLength);
			filterBuilder.SetupListAndModuleIDs(CollectionProviderTypeCodeDescriptionList.Codes.ServiceLevel, newField);
			AssertEquals(RefServiceLevelSchema.RS_Code.MaxLength, newField.ZValueInfo.MaxLength);
		}

		public void TestCanBuild()
		{
			var filterBuilder =
				new DependenceLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("dependence lookup"));
			AssertEquals(false, filterBuilder.CanBuild("vessel dependence lookup"));
			AssertEquals(false, filterBuilder.CanBuild("vessel multiple selection lookup"));
		}

		public void TestRegularExpressionToMatchFilterType()
		{
			var filterBuilder =
				new DependenceLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals("Should match string ending in 'lookup' only", true,
				Regex.IsMatch("dependence lookup", filterBuilder.RegularExpressionToMatchFilterType,
					RegexOptions.IgnoreCase));
			AssertEquals("Should match string ending in 'lookup' only", true,
				Regex.IsMatch("vessel multiple selection lookup", filterBuilder.RegularExpressionToMatchFilterType,
					RegexOptions.IgnoreCase));
			AssertEquals("Shouldn't match string ending in 'lookup code'", false,
				Regex.IsMatch("vessel lookup code", filterBuilder.RegularExpressionToMatchFilterType,
					RegexOptions.IgnoreCase));
		}

		public void TestConstructorRegisteredBindTos()
		{
			var filterBuilder =
				new DependenceLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertRegisteredBindToExists("vessel", ModuleIDs.RefVessel);
			AssertRegisteredBindToExists("unloco", ModuleIDs.RefUNLOCO);
			AssertRegisteredBindToExists("country", ModuleIDs.RefCountry);
			AssertRegisteredBindToExists("address", ModuleIDs.OrgAddresses);
			AssertRegisteredBindToExists("staff", ModuleIDs.GlbStaff);
			AssertRegisteredBindToExists("service level", ModuleIDs.ServiceLevel);
		}

		void AssertRegisteredBindToExists(string code, ModuleIdentifier moduleID)
		{
			var filterBuilder =
				new DependenceLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, code);
			AssertNotNull(
				string.Format(
					"Shouldn't remove existing properties - current reports may break. Expected to find '{0}' type in the available types",
					code), provider);
			AssertEquals("ModuleID should be correct", moduleID, provider.ModuleID);
		}

		public class DependenceLookupBuilderForTesting : DependenceLookupBuilder
		{
			public DependenceLookupBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory,
				MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
				: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			protected internal new void SetupListAndModuleIDs(string lookupTypeName, FilterField newField,
				StringTreeNode fieldTree = null) => base.SetupListAndModuleIDs(lookupTypeName, newField, fieldTree);

			protected internal new string RegularExpressionToMatchFilterType => base.RegularExpressionToMatchFilterType;

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
