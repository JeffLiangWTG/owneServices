using CargoWise.PAVE.Common.Interfaces;
using CargoWise.PAVE.Common.TestUtils;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ZoneCalculatorTest : BMSTestCaseWithFactory
	{
		public void TestCalculateZone_ForPenetrationPercent()
		{
			AssertEquals(3, ZoneCalculator.CalculateZone(-1.0m));
			AssertEquals(3, ZoneCalculator.CalculateZone(0.01m));
			AssertEquals(3, ZoneCalculator.CalculateZone(0.3m));

			AssertEquals(2, ZoneCalculator.CalculateZone(0.34m));
			AssertEquals(2, ZoneCalculator.CalculateZone(0.65m));

			AssertEquals(1, ZoneCalculator.CalculateZone(0.67m));
			AssertEquals(1, ZoneCalculator.CalculateZone(0.99m));

			AssertEquals(0, ZoneCalculator.CalculateZone(1.0m));
			AssertEquals(0, ZoneCalculator.CalculateZone(100.0m));
		}

		[TestDate(2014, 9, 3)]
		public void TestCalculateZone_ForBufferedItem()
		{
			var context = WorkingTimeContext.Create(Factory);

			var buffer = new DummyBuffer { SizeInMinutes = 10, Type = BufferType.Project };
			var item1 = new DummyBufferedItem(buffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-10) };
			var item2 = new DummyChildBufferedItem(item1, buffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-7) };
			var item3 = new DummyChildBufferedItem(item1, buffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-5) };
			var item4 = new DummyChildBufferedItem(item1, buffer) { StartableTime = ZDateTime.UtcNow.ToDateTime().AddMinutes(-3) };

			buffer.RelatedBufferedItems = new[] { item1, item2, item3, item4 };

			AssertEquals(0, ZoneCalculator.CalculateZone(item1, context, Factory));
			AssertEquals(1, ZoneCalculator.CalculateZone(item2, context, Factory));
			AssertEquals(2, ZoneCalculator.CalculateZone(item3, context, Factory));
			AssertEquals(3, ZoneCalculator.CalculateZone(item4, context, Factory));
		}
	}
}
