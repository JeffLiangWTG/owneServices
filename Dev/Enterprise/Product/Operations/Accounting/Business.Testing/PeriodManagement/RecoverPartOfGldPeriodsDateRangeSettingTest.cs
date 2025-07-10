using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	[TestedType(typeof(RecoverPartOfGldPeriodsDateRangeSetting))]
	public class RecoverPartOfGldPeriodsDateRangeSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidationType()
		{
			AssertEquals(typeof(RecoverPartOfGldPeriodsDateRangeSettingValidation), RecoverPartOfGldPeriodsDateRangeSetting.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RecoverPartOfGldPeriodsDateRangeSetting();
		}

		protected override void SetUp()
		{
			base.SetUp();
			RecoverPartOfGldPeriodsDateRangeSetting = (RecoverPartOfGldPeriodsDateRangeSetting)GetNewBusinessObject();
		}

		RecoverPartOfGldPeriodsDateRangeSetting RecoverPartOfGldPeriodsDateRangeSetting;
	}
}
