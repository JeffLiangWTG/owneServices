using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DraftTransactionStatusReasonCodeRegistryDataType))]
	public class DraftTransactionStatusReasonCodeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DraftTransactionStatusReasonCodeRegistryDataType>
	{
		protected override DraftTransactionStatusReasonCodeRegistryDataType GetNewDataType()
		{
			return new DraftTransactionStatusReasonCodeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new DraftTransactionStatusReasonCodeCollection
			{
				new DraftTransactionStatusReasonCode { Code = "AAA", EnglishDescription = "AAADesc" }
			};

			var collection2 = new DraftTransactionStatusReasonCodeCollection
			{
				new DraftTransactionStatusReasonCode { Code = "BBB", EnglishDescription = "BBBDesc" }
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "DraftTransactionStatusReasonCodeRegistryItemEditor"; }
		}
	}
}
