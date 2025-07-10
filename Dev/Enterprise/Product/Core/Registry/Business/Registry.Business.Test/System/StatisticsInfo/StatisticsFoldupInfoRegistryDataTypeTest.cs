using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatisticsFoldupInfoRegistryDataType))]
	sealed class StatisticsFoldupInfoRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StatisticsFoldupInfoRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new StatisticsFoldupInfoCollection();
			var statsInfo = collection.AddNew();
			statsInfo.AggregateScale = TimeFrameList.Codes.Day;
			statsInfo.WaitScale = TimeFrameList.Codes.Month;
			statsInfo.AggregateAmount = 1;
			statsInfo.WaitAmount = 1;
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, new StatisticsFoldupInfoRegistryDataType().Serialise(collection)) };
		}

		protected override StatisticsFoldupInfoRegistryDataType GetNewDataType()
		{
			return new StatisticsFoldupInfoRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "StatisticsFoldupInfoRegistryItemEditor";
			}
		}
	}
}
