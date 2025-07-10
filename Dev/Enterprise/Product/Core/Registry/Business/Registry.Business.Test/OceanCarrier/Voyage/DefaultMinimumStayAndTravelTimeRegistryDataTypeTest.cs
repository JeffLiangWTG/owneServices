using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultMinimumStayAndTravelTimeRegistryDataType))]
	sealed class DefaultMinimumStayAndTravelTimeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultMinimumStayAndTravelTimeRegistryDataType>
	{
		protected override DefaultMinimumStayAndTravelTimeRegistryDataType GetNewDataType()
		{
			return new DefaultMinimumStayAndTravelTimeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collectionA = new DefaultMinimumStayAndTravelTimeCollection();
			var stayAndTravelTimeA = collectionA.AddNew();
			stayAndTravelTimeA.TransportMode = Core.Constants.TransportModes.Sea;
			stayAndTravelTimeA.StayTime = 120;
			stayAndTravelTimeA.TravelTime = 60;

			var collectionB = new DefaultMinimumStayAndTravelTimeCollection();
			var stayAndTravelTimeB = collectionB.AddNew();
			stayAndTravelTimeB.TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			stayAndTravelTimeB.StayTime = 60;
			stayAndTravelTimeB.TravelTime = 30;

			var collectionC = new DefaultMinimumStayAndTravelTimeCollection();
			var stayAndTravelTimeC = collectionC.AddNew();
			stayAndTravelTimeC.TransportMode = Core.Constants.TransportModes.Air;
			stayAndTravelTimeC.StayTime = 60;
			stayAndTravelTimeC.TravelTime = 30;

			var collectionD = new DefaultMinimumStayAndTravelTimeCollection();
			var stayAndTravelTimeD = collectionD.AddNew();
			stayAndTravelTimeD.TransportMode = Core.Constants.TransportModes.Rail;
			stayAndTravelTimeD.StayTime = 30;
			stayAndTravelTimeD.TravelTime = 15;

			return
			[
				new ValidSampleAndBinaryValueInDB(collectionA, new DefaultMinimumStayAndTravelTimeRegistryDataType().Serialise(collectionA)),
				new ValidSampleAndBinaryValueInDB(collectionB, new DefaultMinimumStayAndTravelTimeRegistryDataType().Serialise(collectionB)),
				new ValidSampleAndBinaryValueInDB(collectionC, new DefaultMinimumStayAndTravelTimeRegistryDataType().Serialise(collectionC)),
				new ValidSampleAndBinaryValueInDB(collectionD, new DefaultMinimumStayAndTravelTimeRegistryDataType().Serialise(collectionD)),
			];
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultMinimumStayAndTravelTimeRegistryItemEditor"; }
		}
	}
}
