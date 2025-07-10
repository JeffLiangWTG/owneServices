using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportModeCombinationBufferTimeRegistryDataType))]
	sealed class TransportModeCombinationBufferTimeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TransportModeCombinationBufferTimeRegistryDataType>
	{
		protected override TransportModeCombinationBufferTimeRegistryDataType GetNewDataType()
		{
			return new TransportModeCombinationBufferTimeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collectionA = new TransportModeCombinationBufferTimeCollection();
			var bufferTimeA = collectionA.AddNew();
			bufferTimeA.LoadTransportMode = Core.Constants.TransportModes.Sea;
			bufferTimeA.UnloadTransportMode = Core.Constants.TransportModes.Sea;
			bufferTimeA.BufferTimeInHours = 24;

			var collectionB = new TransportModeCombinationBufferTimeCollection();
			var bufferTimeB = collectionB.AddNew();
			bufferTimeB.LoadTransportMode = Core.Constants.TransportModes.Road;
			bufferTimeB.UnloadTransportMode = Core.Constants.TransportModes.Rail;
			bufferTimeB.BufferTimeInHours = 12;

			return
			[
				new ValidSampleAndBinaryValueInDB(collectionA, new TransportModeCombinationBufferTimeRegistryDataType().Serialise(collectionA)),
				new ValidSampleAndBinaryValueInDB(collectionB, new TransportModeCombinationBufferTimeRegistryDataType().Serialise(collectionB)),
			];
		}

		protected override string ExpectedEditorName
		{
			get { return "TransportModeCombinationBufferTimeRegistryItemEditor"; }
		}
	}
}
