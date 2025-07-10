using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ConsolImportBranchRuleRegistryDataType))]
	sealed class ConsolImportBranchRuleRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ConsolImportBranchRuleRegistryDataType>
	{
		protected override ConsolImportBranchRuleRegistryDataType GetNewDataType()
		{
			return new ConsolImportBranchRuleRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ConsolImportBranchRuleItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var importBranchRule = new ImportBranchRule();
			importBranchRule.UseBranchFromXml = true;

			importBranchRule.DefaultToBranchRelatedToOriginLoadPort = 1;
			importBranchRule.DefaultToBranchRelatedToDestinationDischargePort = 2;
			importBranchRule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			return new[] { new ValidSampleAndBinaryValueInDB(importBranchRule, DataType.Serialise(importBranchRule)) };
		}
	}
}
