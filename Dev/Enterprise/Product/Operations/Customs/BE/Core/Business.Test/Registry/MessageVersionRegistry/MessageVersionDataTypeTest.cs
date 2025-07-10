using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(MessageVersionDataType))]
class MessageVersionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MessageVersionDataType>
{
	protected override MessageVersionDataType GetNewDataType() => new MessageVersionDataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var ncts = new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget };
		var idms = new MessageVersionRegistry { DomainCode = MessageVersionRegistry.IDMSDomainCode, TargetSystemName = MessageVersionRegistry.IDMSDefaultTarget };
		var collection = new MessageVersionRegistryCollection { ncts, idms };
		var emptyCollection = new MessageVersionRegistryCollection();

		return new[]
		{
			new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
			new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
		};
	}

	protected override string ExpectedEditorName => "MessageVersionRegistryItemEditor";
}
