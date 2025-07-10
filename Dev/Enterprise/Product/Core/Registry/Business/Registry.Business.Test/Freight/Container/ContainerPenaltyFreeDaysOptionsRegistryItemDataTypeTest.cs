using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContainerPenaltyFreeDaysOptionsRegistryItemDataType))]
	sealed class ContainerPenaltyFreeDaysOptionsRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ContainerPenaltyFreeDaysOptionsRegistryItemDataType>
	{
		protected override ContainerPenaltyFreeDaysOptionsRegistryItemDataType GetNewDataType()
		{
			return new ContainerPenaltyFreeDaysOptionsRegistryItemDataType(new ContainerPenaltyFreeDaysOptions { FreeDays = 12, UnlimitedFreeDays = false }, 0, 255);
		}

		protected override string ExpectedEditorName => "ContainerPenaltyFreeDaysOptionsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var option1 = new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = false };
			var option2 = new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false };
			var option3 = new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = true };

			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ContainerPenaltyFreeDaysOptions><FreeDays>0</FreeDays><UnlimitedFreeDays>False</UnlimitedFreeDays></ContainerPenaltyFreeDaysOptions>";
			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ContainerPenaltyFreeDaysOptions><FreeDays>10</FreeDays><UnlimitedFreeDays>False</UnlimitedFreeDays></ContainerPenaltyFreeDaysOptions>";
			var xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ContainerPenaltyFreeDaysOptions><FreeDays>0</FreeDays><UnlimitedFreeDays>True</UnlimitedFreeDays></ContainerPenaltyFreeDaysOptions>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(option1, xml1),
				new ValidSampleAndBinaryValueInDB(option2, xml2),
				new ValidSampleAndBinaryValueInDB(option3, xml3)
			};
		}
	}
}
