using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Registry.Business.AutomaticProcessRegistryBusinessObject;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutomaticProcessRegistryBusinessObject))]
	sealed class AutomaticProcessRegistryBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Overrides

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.NextRunDateTime = ZDateTime.Now.AddDays(1);
			BizObj.UpdateRuns(ZDateTime.Now);
			BizObj.Interval = 1;

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new AutomaticProcessRegistryBusinessObject BizObj
		{
			get { return (AutomaticProcessRegistryBusinessObject)base.BizObj; }
		}

		#endregion

		public void TestUpdateRuns()
		{
			ZDateTime testDate = ZDateTime.Now;
			AssertEquals("Initial value", true, BizObj.NextRunDateTime.IsEmpty);
			AssertEquals("Initial value", true, BizObj.LastRunDateTime.IsEmpty);

			BizObj.NextRunDateTime = testDate;
			AssertEquals(testDate, BizObj.NextRunDateTime);

			BizObj.Interval = 2;
			BizObj.UpdateRuns(testDate);
			AssertEquals("LastRunDateTime as assigned", testDate, BizObj.LastRunDateTime);
			AssertEquals("Exactly one Interval added to the previous NextRunDateTime", testDate.AddDays(BizObj.Interval), BizObj.NextRunDateTime);

			testDate = BizObj.NextRunDateTime;
			ZDateTime runDate = testDate.AddDays(1);
			BizObj.UpdateRuns(testDate.AddDays(1));
			AssertEquals("LastRunDateTime as assigned", runDate, BizObj.LastRunDateTime);
			AssertEquals("Exactly one Interval added to the previous NextRunDateTime", testDate.AddDays(BizObj.Interval), BizObj.NextRunDateTime);

			testDate = BizObj.NextRunDateTime;
			runDate = testDate.AddDays(BizObj.Interval * 2).AddMinutes(15);
			BizObj.UpdateRuns(runDate);
			AssertEquals("LastRunDateTime as assigned", runDate, BizObj.LastRunDateTime);
			AssertEquals("Exactly three Intervals added to the previous NextRunDateTime", testDate.AddDays(BizObj.Interval * 3), BizObj.NextRunDateTime);
		}

		public void TestCopyValuesToClone_ShouldCheckIntervalNotLessThanMinimumValue()
		{
			Assert("Precondition", !BizObj.ShouldCheckIntervalNotLessThanMinimumValue);
			var clonedBizO = (AutomaticProcessRegistryBusinessObject)BizObj.Clone(null, Factory);

			Assert(!clonedBizO.ShouldCheckIntervalNotLessThanMinimumValue);

			BizObj.ShouldCheckIntervalNotLessThanMinimumValue = true;
			clonedBizO = (AutomaticProcessRegistryBusinessObject)BizObj.Clone(null, Factory);
			Assert(clonedBizO.ShouldCheckIntervalNotLessThanMinimumValue);
		}

		public void TestCopyValuesToClone_ShouldMaintainMinimumInterval()
		{
			Assert("Precondition", !BizObj.ShouldCheckIntervalNotLessThanMinimumValue);
			var clonedBizO = (AutomaticProcessRegistryBusinessObject)BizObj.Clone(null, Factory);

			Assert(!clonedBizO.ShouldCheckIntervalNotLessThanMinimumValue);

			BizObj.ShouldCheckIntervalNotLessThanMinimumValue = true;
			BizObj.MinimumIntervalType = IntervalTypes.Hours;
			BizObj.MinimumInterval = 5;
			clonedBizO = (AutomaticProcessRegistryBusinessObject)BizObj.Clone(null, Factory);
			Assert(clonedBizO.ShouldCheckIntervalNotLessThanMinimumValue);
			AssertEquals(IntervalTypes.Hours, clonedBizO.MinimumIntervalType);
			AssertEquals(5, clonedBizO.MinimumInterval);
		}

		public void TestInterval()
		{
			AssertEquals("Default value", 1, BizObj.Interval);
			BizObj.Interval = 0;
			AssertEquals(true, BizObj.IntervalInfo.HasErrors());
			BizObj.Interval = -1;
			AssertEquals(true, BizObj.IntervalInfo.HasErrors());
			BizObj.Interval = 1;
			AssertEquals("No errors when valid value:", false, BizObj.IntervalInfo.HasErrors());
		}

		public void TestIntervalMinimumValueAccountingValidation()
		{
			BizObj.ShouldCheckIntervalNotLessThanMinimumValue = true;
			BizObj.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);

			var minimumValue = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.Value;
			AssertEquals("Precondition", 15, minimumValue.Interval);
			AssertEquals("Precondition", AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes, minimumValue.IntervalType);

			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes;
			BizObj.Interval = 2;

			AssertEquals(true, BizObj.IntervalInfo.HasError($"The specified interval cannot be less than {TimeSpan.FromMinutes(15)}."));

			BizObj.Interval = 15;
			AssertEquals("No errors Interval equal to minimum value.", false, BizObj.IntervalInfo.HasErrors());

			BizObj.Interval = 16;
			AssertEquals("No errors Interval greater than minimum value.", false, BizObj.IntervalInfo.HasErrors());

			var newValue = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.Value;
			newValue.Interval = 18;
			AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, newValue);
			minimumValue = AccountingDataRegistry.Instance.MinimumIntervalSubLedgerTakeUp.Value;

			BizObj.Interval = 17;
			AssertEquals(true, BizObj.IntervalInfo.HasError($"The specified interval cannot be less than {TimeSpan.FromMinutes(18)}."));

			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Days;
			AssertEquals("No errors Interval greater than minimum value.", false, BizObj.IntervalTypeInfo.HasErrors());

			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes;
			AssertEquals(true, BizObj.IntervalTypeInfo.HasError($"The specified interval cannot be less than {TimeSpan.FromMinutes(18)}."));

			BizObj.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes;
			AssertEquals("No errors since the minimum Interval is 15 minutes of the current fallback level.", false, BizObj.IntervalTypeInfo.HasErrors());
		}

		public void TestIntervalMinimumValueSpecifiedValidation()
		{
			BizObj.ShouldCheckIntervalNotLessThanMinimumValue = true;
			BizObj.MinimumInterval = 15;
			BizObj.MinimumIntervalType = IntervalTypes.Minutes;

			BizObj.IntervalType = IntervalTypes.Minutes;
			BizObj.Interval = 2;

			AssertEquals(true, BizObj.IntervalInfo.HasError($"The specified interval cannot be less than {TimeSpan.FromMinutes(15)}."));

			BizObj.Interval = 15;
			AssertEquals("No errors Interval equal to minimum value.", false, BizObj.IntervalInfo.HasErrors());

			BizObj.Interval = 16;
			AssertEquals("No errors Interval greater than minimum value.", false, BizObj.IntervalInfo.HasErrors());

			BizObj.MinimumInterval = 18;
			BizObj.Interval = 17;
			AssertEquals(true, BizObj.IntervalInfo.HasError($"The specified interval cannot be less than {TimeSpan.FromMinutes(18)}."));

			BizObj.IntervalType = IntervalTypes.Days;
			AssertEquals("No errors Interval greater than minimum value.", false, BizObj.IntervalTypeInfo.HasErrors());

			BizObj.IntervalType = IntervalTypes.Minutes;
			AssertEquals(true, BizObj.IntervalTypeInfo.HasError($"The specified interval cannot be less than {TimeSpan.FromMinutes(18)}."));
		}

		public void TestIntervalTypesList()
		{
			AssertNotNull(BizObj.IntervalTypeList);
			AssertEquals(4, BizObj.IntervalTypeList.Count);
			AssertEquals(true, BizObj.IntervalTypeList.ContainsCode(AutomaticProcessRegistryBusinessObject.IntervalTypes.Months));
			AssertEquals(true, BizObj.IntervalTypeList.ContainsCode(AutomaticProcessRegistryBusinessObject.IntervalTypes.Days));
			AssertEquals(true, BizObj.IntervalTypeList.ContainsCode(AutomaticProcessRegistryBusinessObject.IntervalTypes.Hours));
			AssertEquals(true, BizObj.IntervalTypeList.ContainsCode(AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes));
		}

		[TestDate(2001, 1, 3, 3, 3, 3)]
		public void TestIntervalType()
		{
			AssertEquals("Initial value", AutomaticProcessRegistryBusinessObject.IntervalTypes.Days, BizObj.IntervalType);
			BizObj.Interval = 2;
			BizObj.NextRunDateTime = ZDateTime.Now;
			BizObj.UpdateRuns(ZDateTime.Now);
			ZDateTime expectedDate = ZDateTime.Now.AddDays(BizObj.Interval);
			AssertEquals("Days interval type (default)", expectedDate.ToShortDateString(), BizObj.NextRunDateTime.ToShortDateString());
			Assert("NextRunDateTimeInfo.HasErrors", !BizObj.HasErrors);

			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Months;
			BizObj.NextRunDateTime = ZDateTime.Now;
			BizObj.UpdateRuns(ZDateTime.Now);
			expectedDate = ZDateTime.Now.AddMonths(BizObj.Interval);
			AssertEquals("Months interval type", expectedDate.ToShortDateString(), BizObj.NextRunDateTime.ToShortDateString());
			Assert("NextRunDateTimeInfo.HasErrors", !BizObj.HasErrors);

			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Hours;
			BizObj.NextRunDateTime = ZDateTime.Now;
			BizObj.UpdateRuns(ZDateTime.Now);
			expectedDate = ZDateTime.Now.AddHours(BizObj.Interval);
			AssertEquals("Hours interval type", expectedDate.ToShortTimeString(), BizObj.NextRunDateTime.ToShortTimeString());
			Assert("NextRunDateTimeInfo.HasErrors", !BizObj.HasErrors);

			BizObj.IntervalType = AutomaticProcessRegistryBusinessObject.IntervalTypes.Minutes;
			BizObj.NextRunDateTime = ZDateTime.Now;
			BizObj.UpdateRuns(ZDateTime.Now);
			expectedDate = ZDateTime.Now.AddMinutes(BizObj.Interval);
			AssertEquals("Minutes interval type", expectedDate.ToShortTimeString(), BizObj.NextRunDateTime.ToShortTimeString());
			Assert("NextRunDateTimeInfo.HasErrors", !BizObj.HasErrors);

			BizObj.IntervalType = "InterType";
			Assert("NextRunDateTimeInfo.HasErrors", BizObj.HasErrors);

			BizObj.IntervalType = "";
			Assert("NextRunDateTimeInfo.HasErrors", BizObj.HasErrors);
		}

		public void TestNextRunDateTime()
		{
			BizObj.NextRunDateTime = ZDateTime.Empty;
			AssertEquals("Has Errors when empty", true, BizObj.NextRunDateTimeInfo.HasErrors());
			BizObj.NextRunDateTime = new ZDateTime(2007, 5, 15);
			AssertEquals("No errors when valid date", false, BizObj.NextRunDateTimeInfo.HasErrors());
		}
	}
}
