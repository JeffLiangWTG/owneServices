using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.CW1.Resources;

namespace Enterprise.Registry.Business.Testing
{
	sealed class IndexDurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTable()
		{
			var tableList = new IndexDurationList();
			var indexDuration = tableList.AddNew();
			indexDuration.RunPreSaveValidation();
			AssertMandatoryValidationError(indexDuration.TableInfo, true);

			indexDuration.Table = " ";
			AssertMandatoryValidationError(indexDuration.TableInfo, true);

			indexDuration.Table = "ThisIsDefinitelyNotTheNameOfATable";
			Assert(indexDuration.TableInfo.GetErrors().Any(e => e.Message == IndexDuration.InvalidTable));

			indexDuration.Table = LowWatermarkTableList.TableList[0];
			Assert(!indexDuration.TableInfo.HasErrors());

			var indexDuration2 = tableList.AddNew();
			indexDuration2.Table = LowWatermarkTableList.TableList[0];
			AssertHasError("Table must be unique.", indexDuration2.TableInfo, "The Table has been duplicated and must be unique.");

			indexDuration.RunPreSaveValidation();
			AssertHasError("Table must be unique.", indexDuration.TableInfo, "The Table has been duplicated and must be unique.");

			tableList.Remove(indexDuration);
			indexDuration2.RunPreSaveValidation();
			AssertNoErrors(indexDuration2.TableInfo);
		}

		public void TestValidateDurationInMonths()
		{
			var mapEntry = new IndexDuration() { DurationInMonths = 0 };
			AssertMandatoryValidationError(mapEntry.DurationInMonthsInfo, false);

			mapEntry.DurationInMonths = 365;
			Assert(!mapEntry.DurationInMonthsInfo.HasErrors());

			mapEntry.DurationInMonths = -60;
			Assert(mapEntry.DurationInMonthsInfo.GetErrors().Any(e => e.Message == IndexDuration.NegativeDuration));
		}
	}
}
