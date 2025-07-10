using System.Text;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PutawaySequenceRegistryDataType))]
	sealed class PutawaySequenceRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PutawaySequenceRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "PutawaySequenceRegistryItemEditor"; }
		}

		protected override PutawaySequenceRegistryDataType GetNewDataType()
		{
			return new PutawaySequenceRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			PutawaySequence sequence = new PutawaySequence();

			sequence.ClientArea = 1;
			sequence.Location = 2;
			sequence.PickFace = 3;
			sequence.ProductArea = 4;

			sequence.Column = 1;
			sequence.Level = 2;
			sequence.Row = 3;

			string xml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				@"<PutawaySequence>
					<ClientArea>1</ClientArea>
					<Location>2</Location>
					<PickFace>3</PickFace>
					<ProductArea>4</ProductArea>
					<Column>1</Column>
					<Level>2</Level>
					<Row>3</Row>
				</PutawaySequence>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sequence, Encoding.Unicode.GetBytes(xml))
			};
		}
	}
}
