using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.SystemDataRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemMergeCompanyCodeMappingDataType))]
	sealed class SystemMergeCompanyCodeMappingDataTypeTest : RegistryDataTypeTestCase<SystemMergeCompanyCodeMappingDataType>
	{
		protected override SystemMergeCompanyCodeMappingDataType GetNewDataType()
		{
			return new SystemMergeCompanyCodeMappingDataType(8);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			byte[] bytes1 = { 60, 78, 101, 119, 68, 97, 116, 97, 83, 101, 116, 62, 13, 10, 32, 32, 60, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 32, 32, 60, 67, 111, 100, 101, 62, 49, 60, 47, 67, 111, 100, 101, 62, 13, 10, 32, 32, 32, 32, 60, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 79, 110, 101, 60, 47, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 13, 10, 32, 32, 60, 47, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 60, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 32, 32, 60, 67, 111, 100, 101, 62, 50, 60, 47, 67, 111, 100, 101, 62, 13, 10, 32, 32, 32, 32, 60, 68, 101, 115, 99, 114, 105, 112, 116, 105, 110, 111, 62, 84, 119, 111, 60, 47, 68, 101, 115, 99, 114, 105, 112, 116, 105, 110, 111, 62, 13, 10, 32, 32, 60, 47, 84, 97, 98, 108, 101, 49, 62, 13, 10, 60, 47, 78, 101, 119, 68, 97, 116, 97, 83, 101, 116, 62 };
			byte[] bytes2 = { 60, 78, 101, 119, 68, 97, 116, 97, 83, 101, 116, 62, 13, 10, 32, 32, 60, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 32, 32, 60, 67, 111, 100, 101, 62, 49, 60, 47, 67, 111, 100, 101, 62, 13, 10, 32, 32, 32, 32, 60, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 79, 110, 101, 60, 47, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 13, 10, 32, 32, 60, 47, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 60, 84, 97, 98, 108, 101, 49, 62, 13, 10, 32, 32, 32, 32, 60, 67, 111, 100, 101, 62, 50, 60, 47, 67, 111, 100, 101, 62, 13, 10, 32, 32, 32, 32, 60, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 84, 119, 111, 60, 47, 68, 101, 115, 99, 114, 105, 112, 116, 105, 111, 110, 62, 13, 10, 32, 32, 60, 47, 84, 97, 98, 108, 101, 49, 62, 13, 10, 60, 47, 78, 101, 119, 68, 97, 116, 97, 83, 101, 116, 62 };

			var result = new ReadOnlyCodeDescriptionPairList(bytes1);
			result.DefaultCode = "rt";
			var result2 = new ReadOnlyCodeDescriptionPairList(bytes2);
			result2.DefaultCode = "tt";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new SystemMergeCompanyCodeMappingDataType(8).Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new SystemMergeCompanyCodeMappingDataType(9).Serialise(result2))
			};
		}
	}
}
