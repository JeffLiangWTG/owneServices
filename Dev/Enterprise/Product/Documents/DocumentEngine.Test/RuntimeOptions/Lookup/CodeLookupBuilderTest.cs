using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CodeLookupBuilderTest : FilterBuilderTest
	{
		protected override FilterBuilder GetFilterBuilderToTest()
		{
			return new CodeLookupBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
		}

		public void TestGetFilterField()
		{
			CodeLookupBuilderForTesting filterBuilder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(CodeLookupField), filterBuilder.GetFilterField().GetType());
		}

		public void TestDoCustomBuilding_CodeLookUpWithoutMaxLengthColumn()
		{
			var builder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var newField = new CodeLookupField(Factory);
			builder.SetupListAndModuleIDs("department", newField);

			var departmentNode = new StringTreeNode();
			departmentNode.Value = "department lookup code";

			var typeNode = new StringTreeNode();
			typeNode.Value = "type";
			typeNode.Children.Add(departmentNode);

			var tree = new StringTreeNode();
			tree.Value = "test";
			tree.Children.Add(typeNode);

			AssertExceptionThrown(typeof(TemplateDefinitionException), "The filter type 'department lookup code' is not configured for code lookups. Please use a lookup without the 'code' option.", () => builder.DoCustomBuilding(tree, newField));
		}

		public void TestDoCustomBuilding_CodeLookUpWithMaxLengthColumn()
		{
			var builder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			var newField = new CodeLookupField(Factory);
			builder.SetupListAndModuleIDs("country", newField);

			var countryNode = new StringTreeNode();
			countryNode.Value = "country lookup code";

			var typeNode = new StringTreeNode();
			typeNode.Value = "type";
			typeNode.Children.Add(countryNode);

			var tree = new StringTreeNode();
			tree.Value = "test";
			tree.Children.Add(typeNode);

			AssertNoExceptionThrown(() => builder.DoCustomBuilding(tree, newField));
		}

		public void TestFilterFieldMaxLength()
		{
			CodeLookupBuilderForTesting filterBuilder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			CodeLookupField newField = new CodeLookupField(Factory);
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
			CodeLookupBuilderForTesting filterBuilder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("vessel lookup code"));
			AssertEquals(false, filterBuilder.CanBuild("vessel lookup"));
			AssertEquals(false, filterBuilder.CanBuild("vessel multiple selection lookup"));
		}

		public void TestRegularExpressionToMatchFilterType()
		{
			CodeLookupBuilderForTesting filterBuilder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals("Shouldn't match string ending in 'lookup' only", false, Regex.IsMatch("vessel lookup", filterBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
			AssertEquals("Shouldn't match string ending in 'multi selection lookup'", false, Regex.IsMatch("vessel multiple selection lookup", filterBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
			AssertEquals("Should match string ending in 'lookup code'", true, Regex.IsMatch("vessel lookup code", filterBuilder.RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase));
		}

		public void TestConstructorRegisteredBindTos()
		{
			CodeLookupBuilderForTesting filterBuilder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertRegisteredBindToExists("vessel", ModuleIDs.RefVessel);
			AssertRegisteredBindToExists("unloco", ModuleIDs.RefUNLOCO);
			AssertRegisteredBindToExists("country", ModuleIDs.RefCountry);
			AssertRegisteredBindToExists("address", ModuleIDs.OrgAddresses);
			AssertRegisteredBindToExists("staff", ModuleIDs.GlbStaff);
			AssertRegisteredBindToExists("service level", ModuleIDs.ServiceLevel);
		}

		void AssertRegisteredBindToExists(string code, ModuleIdentifier moduleID)
		{
			CodeLookupBuilderForTesting filterBuilder = new CodeLookupBuilderForTesting(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, code);
			AssertNotNull(string.Format("Shouldn't remove existing properties - current reports may break. Expected to find '{0}' type in the available types", code), provider);
			AssertEquals("ModuleID should be correct", moduleID, provider.ModuleID);
		}

		public class CodeLookupBuilderForTesting : CodeLookupBuilder
		{
			public CodeLookupBuilderForTesting(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle) : base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
			{
			}

			protected internal new void SetupListAndModuleIDs(string lookupTypeName, FilterField newField,
				StringTreeNode fieldTree = null) => base.SetupListAndModuleIDs(lookupTypeName, newField, fieldTree);

			protected internal new string RegularExpressionToMatchFilterType => base.RegularExpressionToMatchFilterType;

			internal new FilterField GetFilterField() => base.GetFilterField();
		}
	}
}
