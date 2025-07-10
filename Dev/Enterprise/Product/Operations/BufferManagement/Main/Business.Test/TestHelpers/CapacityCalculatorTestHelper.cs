using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public static class CapacityCalculatorTestHelper
	{
		#region Assertions

		public static void AssertCapacity(ChannelCapacity capacity, string expectedCaption, string expectedFullCapacity, string expectedAvailableCapacity, string expectedAllocated, string calculatedTime)
		{
			AssertCapacity(string.Empty, capacity, expectedCaption, expectedFullCapacity, expectedAvailableCapacity, expectedAllocated, calculatedTime);
		}

		public static void AssertCapacity(string message, ChannelCapacity capacity, string expectedCaption, string expectedFullCapacity, string expectedAvailableCapacity, string expectedAllocated, string calculatedTime)
		{
			var actualMessage = capacity.Message;
			var expectedMessage = new[] { expectedFullCapacity, expectedAllocated, expectedAvailableCapacity, calculatedTime }
				.Aggregate((current, next) => string.IsNullOrEmpty(next) ? current : string.IsNullOrEmpty(current) ? next : current + System.Environment.NewLine + System.Environment.NewLine + next);

			Assertion.AssertMultilineASCIIEquals(message, expectedMessage, actualMessage);
			Assertion.AssertEquals(message, expectedCaption, capacity.Caption);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static void AssertReservedCapacityBreakdown(string message, IResourceCapacity capacity, decimal zone3Hours = 0m, decimal zone2Hours = 0m, decimal zone1Hours = 0m, decimal zone0Hours = 0m)
		{
			AssertionWithHtml.CombineAssertions(message, () =>
			{
				Assertion.AssertEquals("Zone 3", zone3Hours, capacity.GetZoneReservedCapacity(3));
				Assertion.AssertEquals("Zone 2", zone2Hours, capacity.GetZoneReservedCapacity(2));
				Assertion.AssertEquals("Zone 1", zone1Hours, capacity.GetZoneReservedCapacity(1));
				Assertion.AssertEquals("Zone 0", zone0Hours, capacity.GetZoneReservedCapacity(0));
			});
		}

		#endregion

		#region HelperMethods

		public static void EnableSimpleCapacityCalculation()
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		#endregion
	}
}
