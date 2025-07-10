using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface.Testing
{
	public class PODDataRowTest : TestCaseWithFactory
	{
		public void TestRowProperties()
		{
			PODDataRow sourceFields = new PODDataRow();

			SetFieldValues(sourceFields);

			AssertEquals("Field count", 18, sourceFields.FieldCount);
			AssertFieldValues(sourceFields);
		}

		void SetFieldValues(PODDataRow row)
		{
			row.AirwayBillNumber = "QG123";
			row.ActualDeliveryDate = new ZDateTime(2009, 5, 25);
			row.ContactName = "Barry While";
			row.ContactPhoneNumber = "XXX";
			row.QuantityDelivered = 5;
			row.Remarks = "remarks";
			row.SignedBy = "Simon Bother";
			row.UserId = "sb";
			row.VehicleNumber = "VEH-987";
		}

		void AssertFieldValues(PODDataRow row)
		{
			AssertEquals("AirwayBillNumber", "QG123", row.AirwayBillNumber);
			AssertEquals("ActualDeliveryDate", new ZDateTime(2009, 5, 25), row.ActualDeliveryDate);
			AssertEquals("ContactName", "Barry While", row.ContactName);
			AssertEquals("DepartureCityCode", "XXX", row.ContactPhoneNumber);
			AssertEquals("QuantityDelivered", 5, row.QuantityDelivered);
			AssertEquals("Remarks", "remarks", row.Remarks);
			AssertEquals("SignedBy", "Simon Bother", row.SignedBy);
			AssertEquals("UserId", "sb", row.UserId);
			AssertEquals("VehicleNumber", "VEH-987", row.VehicleNumber);
		}
	}
}
