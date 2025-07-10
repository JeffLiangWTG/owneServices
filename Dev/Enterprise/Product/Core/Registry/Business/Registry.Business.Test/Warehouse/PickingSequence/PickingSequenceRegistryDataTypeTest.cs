using System.Text;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickingSequenceRegistryDataType))]
	sealed class PickingSequenceRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PickingSequenceRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "PickingSequenceRegistryItemEditor"; }
		}

		protected override PickingSequenceRegistryDataType GetNewDataType()
		{
			return new PickingSequenceRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			PickingSequence sequence = new PickingSequence();

			sequence.BrokenPallets = 1;
			sequence.ConsolidatedPallets = 2;
			sequence.ExpiryDate = 7;
			sequence.FifoFallback = 3;
			sequence.FullPallets = 4;
			sequence.PalletOverflow = 5;
			sequence.PickFaces = 6;
			sequence.HighPriorityLocations = 8;

			sequence.Column = 1;
			sequence.Level = 2;
			sequence.Row = 3;

			string xml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				@"<PickingSequence>
					<BrokenPallets>1</BrokenPallets>
					<ConsolidatedPallets>2</ConsolidatedPallets>
					<ExpiryDate>7</ExpiryDate>
					<FifoFallback>3</FifoFallback>
					<FullPallets>4</FullPallets>
					<PalletOverflow>5</PalletOverflow>
					<PickFaces>6</PickFaces>
					<HighPriorityLocations>8</HighPriorityLocations>
					<Column>1</Column>
					<Level>2</Level>
					<Row>3</Row>
				</PickingSequence>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sequence, Encoding.Unicode.GetBytes(xml))
			};
		}
	}
}
