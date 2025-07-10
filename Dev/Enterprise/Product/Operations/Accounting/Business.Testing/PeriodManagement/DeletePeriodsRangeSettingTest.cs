using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	[TestedType(typeof(DeletePeriodsFromSetting))]
	public class DeletePeriodsRangeSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertEquals(typeof(DeletePeriodsFromSettingValidation), deletePeriodsRangeSetting.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeletePeriodsFromSetting();
		}

		protected override void SetUp()
		{
			base.SetUp();
			deletePeriodsRangeSetting = (DeletePeriodsFromSetting)GetNewBusinessObject();
		}
		DeletePeriodsFromSetting deletePeriodsRangeSetting;
	}
}
