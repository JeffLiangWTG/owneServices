using System;
using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ResourceCapacityTest : BMSTestCaseWithFactory
	{
		public void TestConstruction()
		{
			var staffCode = "MIKU";
			var date = DateTime.Now;
			var fullCapacity = 39m;

			var resourceCapacity = ResourceCapacity.CreateResourceCapacity(staffCode, date, fullCapacity, null, null);

			AssertEquals(staffCode, resourceCapacity.StaffCode);
			AssertEquals(date, resourceCapacity.CalculatedTimeUtc);
			AssertEquals(fullCapacity, resourceCapacity.FullCapacity);
		}

		public void TestFullCapacityForWorkInvolvingCCR_Default()
		{
			var fullCapacity = 20m;
			var expectedResult = 20m;
			var resourceCapacity = ResourceCapacity.CreateForResourceWithFullCapacity("RIN", fullCapacity);
			AssertEquals(expectedResult, resourceCapacity.FullCapacityForWorkInvolvingCCR);
		}

		public void TestFullCapacityForWorkInvolvingCCR_Default_WhenDecimal()
		{
			var fullCapacity = 3.14159m;
			var expectedResult = 3.14m;
			var resourceCapacity = ResourceCapacity.CreateForResourceWithFullCapacity("RIN", fullCapacity);
			AssertEquals(expectedResult, resourceCapacity.FullCapacityForWorkInvolvingCCR);
		}

		public void TestFullCapacityForWorkInvolvingCCR_WithMultiplier()
		{
			var fullCapacity = 20m;
			var expectedResult = 40m;
			var multiplier = 2;
			var resourceCapacity = ResourceCapacity.CreateForResourceWithFullCapacity("RIN", fullCapacity).WithNonCCROverloadMultiplier(multiplier);
			AssertEquals(expectedResult, resourceCapacity.FullCapacityForWorkInvolvingCCR);
		}

		public void TestFullCapacityForWorkInvolvingCCR_WithMultiplier_WhenDecimals()
		{
			var fullCapacity = 140.625m;
			var expectedResult = 140.63m;
			var multiplier = 1;
			var resourceCapacity = ResourceCapacity.CreateForResourceWithFullCapacity("RIN", fullCapacity).WithNonCCROverloadMultiplier(multiplier);
			AssertEquals(expectedResult, resourceCapacity.FullCapacityForWorkInvolvingCCR);
		}

		public void GetZonesAllocatedCapacity_WhenArrayIsNull()
		{
			var zone = 10;
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("LEN", ZDateTime.Now, 1, null, null);
			AssertEquals(default(decimal), resourceCapacity.GetZoneAllocatedCapacity(zone));
		}

		public void GetZonesAllocatedCapacity_WhenValueWithinArray()
		{
			var zone = 0;
			var expectedResult = 55;
			var allocatedCapacity = new Dictionary<int, decimal> { { zone, expectedResult } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("LEN", ZDateTime.Now, 1, allocatedCapacity, null);

			AssertEquals(expectedResult, resourceCapacity.GetZoneAllocatedCapacity(zone));
		}

		public void TestGetZonesAllocatedCapacity_WhenOutOfBounds()
		{
			var zone = 10;
			var expectedResult = 0m;
			var allocatedCapacity = new Dictionary<int, decimal> { { 0, 6m } };

			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("LEN", ZDateTime.Now, 1, allocatedCapacity, null);
			AssertEquals(expectedResult, resourceCapacity.GetZoneAllocatedCapacity(zone));
		}

		public void TestGetZonesReservedCapacity_WhenArrayIsNull()
		{
			var zone = 10;
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("LUKA", ZDateTime.Now, 1, null, null);
			AssertEquals(default(decimal), resourceCapacity.GetZoneAllocatedCapacity(zone));
		}

		public void TestGetZonesReservedCapacity_WhenValueWithinArray()
		{
			var zone = 0;
			var expectedResult = 55m;
			var reservedCapacity = new Dictionary<int, decimal> { { zone, expectedResult } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("LEN", ZDateTime.Now, 1, null, reservedCapacity);

			AssertEquals(expectedResult, resourceCapacity.GetZoneReservedCapacity(zone));
		}

		public void TestGetZonesReservedCapacity_WhenOutOfBounds()
		{
			var zone = 10;
			var expectedResult = 0m;
			var reservedCapacity = new Dictionary<int, decimal> { { 0, 6m } };

			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("LEN", ZDateTime.Now, 1, null, reservedCapacity);
			AssertEquals(expectedResult, resourceCapacity.GetZoneReservedCapacity(zone));
		}

		public void TestUtilisedCapacity_WhenReservedCapacityIsNull()
		{
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("IA", ZDateTime.Now, 1, null, null);
			AssertEquals(0m, resourceCapacity.UtilisedCapacity);
		}

		public void TestUtilisedCapacity_WithReservedCapacity_DefaultAddition_Rounded()
		{
			var reservedCapacity = new Dictionary<int, decimal> { { 0, 55.005m }, { 1, 45m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("GUMI", ZDateTime.Now, 1, null, reservedCapacity).WithRoundedBreakdown(2);
			AssertEquals(100.01m, resourceCapacity.UtilisedCapacity);
		}

		public void TestUtilisedCapacity_WithReservedCapacity_DefaultAddition()
		{
			var reservedCapacity = new Dictionary<int, decimal> { { 0, 55.005m }, { 1, 45m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("GUMI", ZDateTime.Now, 1, null, reservedCapacity);
			AssertEquals(100.005m, resourceCapacity.UtilisedCapacity);
		}

		public void TestNonZoneWeightedUtilisedCapacity_NoSetup()
		{
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("2B", ZDateTime.Now, 1, null, null);
			AssertEquals(0m, resourceCapacity.NonZoneWeightedUtilisedCapacity);
		}

		public void TestAvailableCapacity()
		{
			var reservedCapacity = new Dictionary<int, decimal> { { 0, 10.223m }, { 1, 22.111m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("GACKPO", ZDateTime.Now, 100, null, reservedCapacity).Deduct(3m);
			AssertEquals(64.666m, resourceCapacity.AvailableCapacity);
		}

		public void TestAvailableCapacityForWorkInvolvingCCR()
		{
			var reservedCapacity = new Dictionary<int, decimal> { { 0, 10m }, { 1, 22m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("MAYU", ZDateTime.Now, 100, null, reservedCapacity).WithNonCCROverloadMultiplier(2).Deduct(3m);
			AssertEquals(165m, resourceCapacity.AvailableCapacityForWorkInvolvingCCR);
		}

		public void TestDeduct()
		{
			var reservedCapacity = new Dictionary<int, decimal> { { 0, 10m }, { 1, 20m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("NEMU", ZDateTime.Now, 100, null, reservedCapacity);
			var deductedCapacity = resourceCapacity.Deduct(10m);

			AssertEquals(30m, resourceCapacity.UtilisedCapacity);
			AssertEquals(0m, resourceCapacity.NonZoneWeightedUtilisedCapacity);
			AssertEquals(100m - 30m, resourceCapacity.AvailableCapacity);

			AssertEquals(30m, deductedCapacity.UtilisedCapacity);
			AssertEquals(10m, deductedCapacity.NonZoneWeightedUtilisedCapacity);
			AssertEquals(100m - 40m, deductedCapacity.AvailableCapacity);
		}

		[TestUtcOffset(10, 0, 0)]
		[TestDate(2007, 08, 31, 11, 39, 39)]
		public void TestCreateForResourceWithFullCapacity_UtcNow()
		{
			var resourceCapacity = ResourceCapacity.CreateForResourceWithFullCapacity("Miku", 0);

			AssertEquals(new ZDateTime(2007, 08, 31, 11, 39, 39), resourceCapacity.CalculatedTimeUtc);
		}

		public void TestClone()
		{
			var allocated = new Dictionary<int, decimal> { { 0, 1.1234m }, { 1, 2.3456m } };
			var reserved = new Dictionary<int, decimal> { { 0, 8.876m }, { 1, 9.55m }, { 2, 10.2313m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("MAIKA", ZDateTime.Now, 222, allocated, reserved).WithNonCCROverloadMultiplier(2).Deduct(3m);
			var newResourceCapacity = resourceCapacity.Clone();

			AssertEquals(resourceCapacity.StaffCode, newResourceCapacity.StaffCode);
			AssertEquals(resourceCapacity.CalculatedTimeUtc, newResourceCapacity.CalculatedTimeUtc);
			AssertEquals(resourceCapacity.FullCapacity, newResourceCapacity.FullCapacity);
			AssertEquals(resourceCapacity.NonZoneWeightedUtilisedCapacity, newResourceCapacity.NonZoneWeightedUtilisedCapacity);
			AssertEquals(resourceCapacity.UtilisedCapacity, newResourceCapacity.UtilisedCapacity);
			AssertEquals(resourceCapacity.AvailableCapacity, newResourceCapacity.AvailableCapacity);
			AssertEquals(resourceCapacity.GetZoneAllocatedCapacity(0), newResourceCapacity.GetZoneAllocatedCapacity(0));
			AssertEquals(resourceCapacity.GetZoneAllocatedCapacity(1), newResourceCapacity.GetZoneAllocatedCapacity(1));
			AssertEquals(resourceCapacity.GetZoneReservedCapacity(0), newResourceCapacity.GetZoneReservedCapacity(0));
			AssertEquals(resourceCapacity.GetZoneReservedCapacity(1), newResourceCapacity.GetZoneReservedCapacity(1));
			AssertEquals(resourceCapacity.GetZoneReservedCapacity(2), newResourceCapacity.GetZoneReservedCapacity(2));
		}

		public void TestWithCapacities()
		{
			var allocated = new Dictionary<int, decimal> { { 0, 1.1234m }, { 1, 2.3456m } };
			var reserved = new Dictionary<int, decimal> { { 0, 8.876m }, { 1, 9.55m }, { 2, 12.2313m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("MAIKA", ZDateTime.Now, 222, null, null).WithNonCCROverloadMultiplier(3);
			var newResourceCapacity = resourceCapacity.WithZoneCapacities(allocated, reserved);

			AssertEquals(resourceCapacity.StaffCode, newResourceCapacity.StaffCode);
			AssertEquals(resourceCapacity.CalculatedTimeUtc, newResourceCapacity.CalculatedTimeUtc);
			AssertEquals(resourceCapacity.FullCapacity, newResourceCapacity.FullCapacity);
			AssertEquals(allocated[0], newResourceCapacity.GetZoneAllocatedCapacity(0));
			AssertEquals(allocated[1], newResourceCapacity.GetZoneAllocatedCapacity(1));
			AssertEquals(reserved[0], newResourceCapacity.GetZoneReservedCapacity(0));
			AssertEquals(reserved[1], newResourceCapacity.GetZoneReservedCapacity(1));
			AssertEquals(reserved[2], newResourceCapacity.GetZoneReservedCapacity(2));
		}

		public void TestWithNewRoundedBreakdown()
		{
			var allocated = new Dictionary<int, decimal> { { 0, 1.1234m }, { 1, 2.3456m } };
			var reserved = new Dictionary<int, decimal> { { 0, 8.876m }, { 1, 9.55m }, { 2, 12.125m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("KAITO", ZDateTime.Now, 222, allocated, reserved).WithNonCCROverloadMultiplier(3);
			var resourceCapacityWithNewBreakdown = resourceCapacity.WithRoundedBreakdown(2);

			AssertEquals(resourceCapacity.StaffCode, resourceCapacityWithNewBreakdown.StaffCode);
			AssertEquals(resourceCapacity.CalculatedTimeUtc, resourceCapacityWithNewBreakdown.CalculatedTimeUtc);
			AssertEquals(resourceCapacity.FullCapacity, resourceCapacityWithNewBreakdown.FullCapacity);
			AssertEquals(30.56m, resourceCapacityWithNewBreakdown.UtilisedCapacity);
			AssertEquals(191.44m, resourceCapacityWithNewBreakdown.AvailableCapacity);
			AssertEquals(1.12m, resourceCapacityWithNewBreakdown.GetZoneAllocatedCapacity(0));
			AssertEquals(2.35m, resourceCapacityWithNewBreakdown.GetZoneAllocatedCapacity(1));
			AssertEquals(8.88m, resourceCapacityWithNewBreakdown.GetZoneReservedCapacity(0));
			AssertEquals(9.55m, resourceCapacityWithNewBreakdown.GetZoneReservedCapacity(1));
			AssertEquals(12.13m, resourceCapacityWithNewBreakdown.GetZoneReservedCapacity(2));
		}

		public void TestWithNonCCROverloadMultiplier()
		{
			var allocated = new Dictionary<int, decimal> { { 0, 1.1234m }, { 1, 2.3456m } };
			var reserved = new Dictionary<int, decimal> { { 0, 8.876m }, { 1, 9.55m }, { 2, 12.125m } };
			var resourceCapacity = ResourceCapacity.CreateResourceCapacity("KAITO", ZDateTime.Now, 222, allocated, reserved).WithRoundedBreakdown(2);
			var resourceCapacityWithNewBreakdown = resourceCapacity.WithNonCCROverloadMultiplier(3);

			AssertEquals(resourceCapacity.StaffCode, resourceCapacityWithNewBreakdown.StaffCode);
			AssertEquals(resourceCapacity.CalculatedTimeUtc, resourceCapacityWithNewBreakdown.CalculatedTimeUtc);
			AssertEquals(resourceCapacity.FullCapacity, resourceCapacityWithNewBreakdown.FullCapacity);
			AssertEquals(30.56m, resourceCapacityWithNewBreakdown.UtilisedCapacity);
			AssertEquals(191.44m, resourceCapacityWithNewBreakdown.AvailableCapacity);
			AssertEquals(1.12m, resourceCapacityWithNewBreakdown.GetZoneAllocatedCapacity(0));
			AssertEquals(2.35m, resourceCapacityWithNewBreakdown.GetZoneAllocatedCapacity(1));
			AssertEquals(8.88m, resourceCapacityWithNewBreakdown.GetZoneReservedCapacity(0));
			AssertEquals(9.55m, resourceCapacityWithNewBreakdown.GetZoneReservedCapacity(1));
			AssertEquals(12.13m, resourceCapacityWithNewBreakdown.GetZoneReservedCapacity(2));
		}
	}
}
