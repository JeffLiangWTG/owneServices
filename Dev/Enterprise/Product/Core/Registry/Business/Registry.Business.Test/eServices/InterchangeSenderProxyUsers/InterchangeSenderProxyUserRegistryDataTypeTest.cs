using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterchangeSenderProxyUserRegistryDataType))]
	sealed class InterchangeSenderProxyUserRegistryDataTypeTest : NonPersistentBusinessObjectCollectionRegistryDataTypeTestCase<InterchangeSenderProxyUserRegistryDataType, InterchangeSenderProxyUserCollection>
	{
		protected override InterchangeSenderProxyUserRegistryDataType GetNewDataType()
		{
			return new InterchangeSenderProxyUserRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InterchangeSenderProxyUserRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new InterchangeSenderProxyUserCollection();
			var parent = collection.AddNew();
			parent.Code = "WRK";
			parent.DescriptionValue = "~BP";

			var collection2 = new InterchangeSenderProxyUserCollection();
			var parent2 = collection.AddNew();
			parent2.Code = "BINGO";
			parent2.DescriptionValue = "~BP";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}
	}
}
