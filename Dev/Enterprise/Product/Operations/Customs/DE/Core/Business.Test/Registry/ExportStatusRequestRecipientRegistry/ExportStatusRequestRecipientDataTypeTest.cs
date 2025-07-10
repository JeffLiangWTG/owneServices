using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(ExportStatusRequestRecipientDataType))]
	class ExportStatusRequestRecipientDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ExportStatusRequestRecipientDataType>
	{
		protected override ExportStatusRequestRecipientDataType GetNewDataType() => new ExportStatusRequestRecipientDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ExportStatusRequestRecipientRegistryCollection
			{
				new ExportStatusRequestRecipientRegistry { SystemCode = "ATLAS", MessageRecipient = "DE001348" },
				new ExportStatusRequestRecipientRegistry { SystemCode = "AES", MessageRecipient = "DE001342" }
			};
			var emptyCollection = new ExportStatusRequestRecipientRegistryCollection();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
			};
		}

		protected override string ExpectedEditorName => "ExportStatusRequestRecipientRegistryItemEditor";
	}
}
