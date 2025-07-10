using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class OutturnLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeepCopyOfOutturnLine()
		{
			var testTime = ZDateTime.Now;
			var houseBill = GetNewHouseBill();
			var cusUnderbond = Factory.NewWithValidTestData<CusUnderbond>();

			var line = (OutturnLine)GetNewBusinessObject();
			line.ConsignmentRef = "Reference1234567890123456789012345";
			line.Status = "Held";
			line.ScannedDateTime = testTime;
			line.Count = 12;
			line.HouseBill = houseBill;
			line.Underbond = cusUnderbond;

			var lineCopy = line.DeepCopy();

			AssertNotNull("Copy line not null", lineCopy);
			AssertEquals("ConsignmentRef", lineCopy.ConsignmentRef, "Reference1234567890123456789012345");
			AssertEquals("Status", lineCopy.Status, "Held");
			AssertEquals("ScannedDateTime", lineCopy.ScannedDateTime, testTime);
			AssertEquals("Count", lineCopy.Count, 12);
			AssertEquals("CusHAWB", lineCopy.HouseBill, houseBill);
			AssertEquals("Underbond", lineCopy.Underbond.PK, cusUnderbond.PK);
		}

		protected abstract IScanHouseBillProvider GetNewHouseBill();
	}
}
