using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PeriodManagement.Testing
{
	public class DeletePeriodsRangeSettingValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2023, 04, 02)]
		public void TestCheckDeletePeriodsFrom()
		{
			deletePeriodsRangeSetting.DeletePeriodsFrom = ZDateTime.Empty;
			deletePeriodsRangeSetting.GetNewValidation().ValidateDeletePeriodsFrom();
			AssertHasError(deletePeriodsRangeSetting.DeletePeriodsFromInfo, "Please enter a Start Date.");

			deletePeriodsRangeSetting.DeletePeriodsFrom = ZDateTime.Now;
			AssertHasError(deletePeriodsRangeSetting.DeletePeriodsFromInfo, "Start Date must be equal to First Day of an Accounting Period in a Financial Year");

			deletePeriodsRangeSetting.DeletePeriodsFrom = ZDateTime.Invalid;
			AssertHasError(deletePeriodsRangeSetting.DeletePeriodsFromInfo, "Enter a valid selection.");

			deletePeriodsRangeSetting.DeletePeriodsFrom = new ZDateTime(2023, 04, 01);
			AssertNoErrors(deletePeriodsRangeSetting.DeletePeriodsFromInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			deletePeriodsRangeSetting = new DeletePeriodsFromSetting();

			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			Factory.Save();
		}

		DeletePeriodsFromSetting deletePeriodsRangeSetting;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
