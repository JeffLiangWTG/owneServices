using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(MessageVersionDataType))]
	class MessageVersionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MessageVersionDataType>
	{
		protected override MessageVersionDataType GetNewDataType() => new MessageVersionDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var atlas = new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = MessageVersionRegistry.AtlasDefaultVersionNumber };
			var emcs = new MessageVersionRegistry { SystemCode = MessageVersionRegistry.EmcsSystemCode, VersionNumber = MessageVersionRegistry.EmcsDefaultVersionNumber };
			var collection = new MessageVersionRegistryCollection { atlas, emcs };
			var emptyCollection = new MessageVersionRegistryCollection();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
			};
		}

		protected override string ExpectedEditorName => "MessageVersionRegistryItemEditor";
	}
}
