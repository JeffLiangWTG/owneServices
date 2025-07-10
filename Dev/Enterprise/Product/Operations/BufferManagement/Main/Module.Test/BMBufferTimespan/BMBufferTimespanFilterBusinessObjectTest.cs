using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBufferTimespanFilterBusinessObject))]
	public class BMBufferTimespanFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNameFilter()
		{
			var timespan1 = Factory.New<BMBufferTimespan>();
			timespan1.BMT_Name = "Name1";
			timespan1.BMT_BufferTimespanInMinutes = 420;

			var timespan2 = Factory.New<BMBufferTimespan>();
			timespan2.BMT_Name = "Name2";
			timespan2.BMT_BufferTimespanInMinutes = 420;

			Factory.Save();

			var bizo = new BMBufferTimespanFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Name"];
			filter.Property = "Name1";
			filter.IsActive = true;

			var results = Factory.Load<BMBufferTimespan>(bizo.Filter);
			AssertCollectionContains(timespan1, results);
			AssertCollectionNotContains(timespan2, results);
		}

		public void TestTimespanFilter()
		{
			var timespan1 = Factory.New<BMBufferTimespan>();
			timespan1.BMT_Name = "Name1";
			timespan1.BMT_BufferTimespanInMinutes = 42;
			var timespan2 = Factory.New<BMBufferTimespan>();
			timespan2.BMT_Name = "Name2";
			timespan2.BMT_BufferTimespanInMinutes = 420;
			Factory.Save();

			var bizo = new BMBufferTimespanFilterBusinessObject();

			var filter = (ModuleDurationFilter)bizo[ProcessHeader.ModuleFilterConstants.BufferTimespan];

			filter.IsActive = true;
			filter.Scope = "Between";
			filter.MinDurationMinutes = 0;
			filter.MaxDurationMinutes = 300;

			var results = Factory.Load<BMBufferTimespan>(bizo.Filter);
			AssertCollectionContains(timespan1, results);
			AssertCollectionNotContains(timespan2, results);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMBufferTimespanFilterBusinessObject();
		}

		#endregion
	}
}
