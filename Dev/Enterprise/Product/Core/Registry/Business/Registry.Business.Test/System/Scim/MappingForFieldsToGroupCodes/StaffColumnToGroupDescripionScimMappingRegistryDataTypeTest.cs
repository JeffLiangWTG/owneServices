using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StaffColumnToGroupDescriptionScimMappingRegistryDataType))]
	public class StaffColumnToGroupDescriptionScimMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StaffColumnToGroupDescriptionScimMappingRegistryDataType>
	{
		protected override StaffColumnToGroupDescriptionScimMappingRegistryDataType GetNewDataType()
		{
			return new StaffColumnToGroupDescriptionScimMappingRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "StaffColumnToGroupDescriptionScimMappingRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			var data1 = collection.AddNew();
			data1.GroupDescriptionMapping = "Test1";
			data1.StaffColumnName = "GS_CanLogin";
			var collection2 = new StaffColumnToGroupDescriptionScimMappingCollection();
			var data2 = collection2.AddNew();
			data2.GroupDescriptionMapping = "Test1";
			data2.StaffColumnName = "GS_IsController";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2)),
			};
		}
	}
}
