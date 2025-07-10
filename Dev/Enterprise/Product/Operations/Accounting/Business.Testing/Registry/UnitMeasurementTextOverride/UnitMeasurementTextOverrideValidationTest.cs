using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class UnitMeasurementTextOverrideValidationTest : TestCaseWithFactory
	{
		public void TestValidateUnitMeasurement()
		{
			BizObj.UnitMeasurementInfo.Value = (ZString)"";
			AssertHasError(BizObj.UnitMeasurementInfo, "Please enter a value.");

			BizObj.UnitMeasurementInfo.Value = (ZString)"1";
			AssertHasError(BizObj.UnitMeasurementInfo, "Enter a valid selection.");

			BizObj.UnitMeasurementInfo.Value = (ZString)"KG";
			AssertNoErrors(BizObj.UnitMeasurementInfo);

			var newBizObj = BizObjCollection.AddNew();
			newBizObj.UnitMeasurementInfo.Value = (ZString)"KG";
			AssertHasError(newBizObj.UnitMeasurementInfo, "The unit measurement override of 'KG' already exists.");
		}

		public void TestValidateTextOverride()
		{
			BizObj.UnitMeasurementInfo.Value = (ZString)"KG";
			BizObj.TextOverrideInfo.Value = (ZString)"";
			AssertHasError(BizObj.TextOverrideInfo, "Please enter a value.");

			BizObj.TextOverrideInfo.Value = (ZString)"1";
			AssertNoErrors(BizObj.TextOverrideInfo);
		}

		UnitMeasurementTextOverride BizObj;
		UnitMeasurementTextOverrideCollection BizObjCollection;

		protected override void SetUp()
		{
			BizObjCollection = new UnitMeasurementTextOverrideCollection();
			BizObj = BizObjCollection.AddNew();
		}
	}
}
