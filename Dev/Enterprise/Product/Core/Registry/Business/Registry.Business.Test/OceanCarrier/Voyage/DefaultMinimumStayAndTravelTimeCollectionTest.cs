using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultMinimumStayAndTravelTimeCollection))]
	sealed class DefaultMinimumStayAndTravelTimeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultMinimumStayAndTravelTimeCollection>
	{
		// Temporarily disable this unit test in .NET 8 since it causes the build to fail on DAT with the following error:
		// System.InvalidOperationException: Target name is ambiguous, rename the test without arguments to a unique name
#if NETFRAMEWORK
		[Test]
		public void TestGetDefaultCollectionCount()
		{
			AssertEquals(4, ActualDefaultValueCollection.Count);
		}
#endif

		[TestCase(Core.Constants.TransportModes.Sea, 120, 60)]
		[TestCase(Core.Constants.TransportModes.InlandWaterwayTransport, 60, 30)]
		[TestCase(Core.Constants.TransportModes.Air, 60, 30)]
		[TestCase(Core.Constants.TransportModes.Rail, 30, 15)]
		public void TestGetDefaultMinimumStayAndTravelTimes(string transportMode, int stayTime, int travelTime)
		{
			var expectedDefaultStayAndTravelTime = new DefaultMinimumStayAndTravelTime
			{
				TransportMode = transportMode,
				StayTime = stayTime,
				TravelTime = travelTime,
			};
			AssertEquals(expected: true, ActualDefaultValueCollection.Cast<DefaultMinimumStayAndTravelTime>().Any(t => t.TransportMode == expectedDefaultStayAndTravelTime.TransportMode && t.StayTime.ToString() == expectedDefaultStayAndTravelTime.StayTime.ToString() && t.TravelTime == expectedDefaultStayAndTravelTime.TravelTime));
		}

		protected override DefaultMinimumStayAndTravelTimeCollection GetCollectionToTest()
		{
			return new DefaultMinimumStayAndTravelTimeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultMinimumStayAndTravelTime();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		readonly DefaultMinimumStayAndTravelTimeCollection ActualDefaultValueCollection = DefaultMinimumStayAndTravelTimeCollection.DefaultValue;
	}
}
