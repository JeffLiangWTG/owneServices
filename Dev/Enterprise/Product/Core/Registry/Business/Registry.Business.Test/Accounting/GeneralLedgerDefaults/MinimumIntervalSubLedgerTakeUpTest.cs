using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MinimumIntervalSubLedgerTakeUp))]
	sealed class MinimumIntervalSubLedgerTakeUpTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestInterval()
		{
			var obj = new MinimumIntervalSubLedgerTakeUp();
			AssertEquals("Default value", 15, obj.Interval);
			Assert("Initially no errors", !obj.HasErrors);
			obj.Interval = 0;
			Assert("Interval has error on 0 value", obj.IntervalInfo.HasErrors());
			obj.Interval = -1;
			Assert("Interval has error on negative value", obj.IntervalInfo.HasErrors());
			obj.Interval = 1;
			Assert("Interval has no errors when valid value:", !obj.IntervalInfo.HasErrors());
		}

		public void TestIntervalType()
		{
			var obj = new MinimumIntervalSubLedgerTakeUp();
			AssertEquals("Default value", MinimumIntervalSubLedgerTakeUp.IntervalTypes.Minutes, obj.IntervalType);
			Assert("Initially no errors", !obj.HasErrors);
			obj.IntervalType = "Invalid";
			Assert("IntervalType has errors on invalid value", obj.HasErrors);
			obj.IntervalType = "";
			Assert("IntervalType has errors on empty value", obj.HasErrors);
		}

		public void TestIntervalTypesList()
		{
			var obj = new MinimumIntervalSubLedgerTakeUp();
			AssertNotNull(obj.IntervalTypeList);
			AssertEquals(4, obj.IntervalTypeList.Count);
			AssertEquals(true, obj.IntervalTypeList.ContainsCode(MinimumIntervalSubLedgerTakeUp.IntervalTypes.Minutes));
			AssertEquals(true, obj.IntervalTypeList.ContainsCode(MinimumIntervalSubLedgerTakeUp.IntervalTypes.Hours));
			AssertEquals(true, obj.IntervalTypeList.ContainsCode(MinimumIntervalSubLedgerTakeUp.IntervalTypes.Days));
			AssertEquals(true, obj.IntervalTypeList.ContainsCode(MinimumIntervalSubLedgerTakeUp.IntervalTypes.Months));
		}

		#region Overrides

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new MinimumIntervalSubLedgerTakeUp();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
