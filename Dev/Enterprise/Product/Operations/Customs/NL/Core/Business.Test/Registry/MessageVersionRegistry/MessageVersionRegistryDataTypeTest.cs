using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(MessageVersionRegistryDataType))]
sealed class MessageVersionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<MessageVersionRegistryDataType>
{
	protected override MessageVersionRegistryDataType GetNewDataType() => new MessageVersionRegistryDataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var ncts = new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = MessageVersionRegistry.NCTSP5DefaultTarget };
		var dms = new MessageVersionRegistry { DomainCode = MessageVersionRegistry.DMSDomainCode, TargetSystemName = MessageVersionRegistry.DMSDefaultTarget };
		var collection = new MessageVersionRegistryCollection { dms, ncts };
		var emptyCollection = new MessageVersionRegistryCollection();

		return new[]
		{
			new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
			new ValidSampleAndBinaryValueInDB(emptyCollection, DataType.Serialise(emptyCollection))
		};
	}

	protected override string ExpectedEditorName => "MessageVersionRegistryItemEditor";
}
