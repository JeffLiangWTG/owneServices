using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CartonGroupSequenceRegistryDataType))]
	sealed class CartonGroupSequenceRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CartonGroupSequenceRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "CartonGroupSequenceRegistryItemEditor"; }
		}

		protected override CartonGroupSequenceRegistryDataType GetNewDataType()
		{
			return new CartonGroupSequenceRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sequence1 = new CartonGroupSequence();
			sequence1.Product = 1;
			sequence1.Carrier = 2;
			sequence1.Consignee = 3;
			sequence1.Client = 4;
			sequence1.Warehouse = 5;

			var sequence2 = new CartonGroupSequence();
			sequence2.Product = 3;
			sequence2.Carrier = 1;
			sequence2.Consignee = 4;
			sequence2.Warehouse = 5;
			sequence2.Client = 2;

			var serializer = new CartonGroupSequenceRegistryDataType();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sequence1, serializer.Serialise(sequence1)),
				new ValidSampleAndBinaryValueInDB(sequence2, serializer.Serialise(sequence2)),
			};
		}
	}
}
