using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BranchLevelPostingConfigurationRegistryItem))]
	public class BranchLevelPostingConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<BranchLevelPostingConfiguration>
	{
		protected override StronglyTypedRegistryItem<BranchLevelPostingConfiguration, BranchLevelPostingConfiguration> GetNewRegistryItem()
		{
			return new BranchLevelPostingConfigurationRegistryItem(string.Empty, null, null, null
				, Enterprise.Integration.RegistryStorageFlags.Company, Enterprise.Integration.RegistryOptions.IsOnlyForCargoWise, new BranchLevelPostingConfiguration());
		}
	}

	[TestedType(typeof(BranchLevelPostingConfigurationRegistryDataType))]
	public class BranchLevelPostingConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BranchLevelPostingConfigurationRegistryDataType>
	{
		protected override BranchLevelPostingConfigurationRegistryDataType GetNewDataType()
		{
			return new BranchLevelPostingConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName => "BranchLevelPostingConfigurationRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BranchLevelPostingConfiguration clone = new BranchLevelPostingConfiguration();

			var branch1 = new ZGuid("F08FB862-D617-4F41-80A8-C511413BDF40");
			var branch2 = new ZGuid("7F8EF1B5-FBCB-4CA2-B820-3E967096C5EB");

			//Setup branch grouping and defauling configuration
			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = true;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = false;

			clone.EnableBranchLevelPosting = true;

			clone.BranchGroupSettingsCollection.Add(settings1);
			clone.BranchGroupSettingsCollection.Add(settings2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(clone, DataType.Serialise(clone))
			};
		}
	}
}
