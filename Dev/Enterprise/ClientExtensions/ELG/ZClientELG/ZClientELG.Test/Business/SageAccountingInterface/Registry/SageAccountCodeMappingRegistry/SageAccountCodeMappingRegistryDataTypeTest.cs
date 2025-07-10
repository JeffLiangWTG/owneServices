using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SageAccountCodeMappingRegistryDataType))]
	sealed class SageAccountCodeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SageAccountCodeMappingRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get
			{
				return "SageAccountCodeMappingRegistryItemEditor";
			}
		}

		protected override SageAccountCodeMappingRegistryDataType GetNewDataType()
		{
			return new SageAccountCodeMappingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			SageAccountCodeMappingRegistryBusinessObjectCollection collection = new SageAccountCodeMappingRegistryBusinessObjectCollection();
			SageAccountCodeMappingRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.SageAccountCode = "60AA200BB";
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}
	}
}
