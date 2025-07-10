using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	[TestedType(typeof(UpdateGldPeriodsDateRangeSetting))]
	public class UpdateGldPeriodsDateRangeSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertEquals(typeof(UpdateGldPeriodsDateRangeSettingValidation), updateGldPeriodsDateRangeSetting.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpdateGldPeriodsDateRangeSetting();
		}

		protected override void SetUp()
		{
			base.SetUp();
			updateGldPeriodsDateRangeSetting = (UpdateGldPeriodsDateRangeSetting)GetNewBusinessObject();
		}
		UpdateGldPeriodsDateRangeSetting updateGldPeriodsDateRangeSetting;
	}
}
