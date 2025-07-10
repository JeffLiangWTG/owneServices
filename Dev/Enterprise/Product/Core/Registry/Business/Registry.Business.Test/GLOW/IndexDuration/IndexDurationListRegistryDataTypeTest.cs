using CargoWise.Glow.Model.CW1.Resources;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexDurationListRegistryDataType))]
	sealed class IndexDurationListRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<IndexDurationListRegistryDataType>
	{
		#region Implementation

		protected override string ExpectedEditorName => "IndexDurationRegistryItemEditor";

		protected override IndexDurationListRegistryDataType GetNewDataType()
		{
			return new IndexDurationListRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var indexDurationList1 = new IndexDurationList();
			var entry1 = indexDurationList1.AddNew();
			entry1.Table = LowWatermarkTableList.TableList[0];
			entry1.DurationInMonths = 31;

			var indexDurationList2 = new IndexDurationList();

			var dataType = new IndexDurationListRegistryDataType();
			var bytes1 = dataType.Serialise(indexDurationList1);
			var bytes2 = dataType.Serialise(indexDurationList2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(indexDurationList1, bytes1),
				new ValidSampleAndBinaryValueInDB(indexDurationList2, bytes2)
			};
		}

		#endregion
	}
}
