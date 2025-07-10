using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportModeCombinationBufferTimeCollection))]
	sealed class TransportModeCombinationBufferTimeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TransportModeCombinationBufferTimeCollection>
	{
		// Temporarily disable this unit test in .NET 8 since it causes the build to fail on DAT with the following error:
		// System.InvalidOperationException: Target name is ambiguous, rename the test without arguments to a unique name
#if NETFRAMEWORK
		[Test]
		public void TestGetDefaultCollectionCount()
		{
			AssertEquals(25, ActualDefaultValueCollection.Count);
		}
#endif

		[Test]
		public void TestGetDefaultTransportModeCombinationBufferTimes([ValueSource(nameof(TransportModes))] string loadTransportMode, [ValueSource(nameof(TransportModes))] string unloadTransportMode)
		{
			var expectedDefaultCombinationAndBufferTime = new TransportModeCombinationBufferTime
			{
				LoadTransportMode = loadTransportMode,
				UnloadTransportMode = unloadTransportMode,
				BufferTimeInHours = 24,
			};

			AssertEquals(true,
				ActualDefaultValueCollection.Cast<TransportModeCombinationBufferTime>().Any(t =>
					t.LoadTransportMode == expectedDefaultCombinationAndBufferTime.LoadTransportMode &&
					t.UnloadTransportMode == expectedDefaultCombinationAndBufferTime.UnloadTransportMode &&
					t.BufferTimeInHours == expectedDefaultCombinationAndBufferTime.BufferTimeInHours));
		}

		protected override TransportModeCombinationBufferTimeCollection GetCollectionToTest()
		{
			return new TransportModeCombinationBufferTimeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransportModeCombinationBufferTime();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		readonly TransportModeCombinationBufferTimeCollection ActualDefaultValueCollection = TransportModeCombinationBufferTimeCollection.DefaultValue;

		static IEnumerable<string> TransportModes { get; } = [
			Core.Constants.TransportModes.Sea,
			Core.Constants.TransportModes.Air,
			Core.Constants.TransportModes.Road,
			Core.Constants.TransportModes.Rail,
			Core.Constants.TransportModes.InlandWaterwayTransport
		];
	}
}
