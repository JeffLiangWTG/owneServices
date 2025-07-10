using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class UserEnteredStashManagerTest : TestCaseWithFactory
	{
		public void TestStashAndApply()
		{
			var lineFeeA00 = Factory.New<CusEntryLineFee>();
			lineFeeA00.CF_ChargeType = "A00";
			lineFeeA00.CF_MethodOfCalculation = "MC";
			lineFeeA00.CF_MethodOfPayment = "MPA";
			lineFeeA00.NationalFeeTypeCode = "NTA";

			var lineFeeB00 = Factory.New<CusEntryLineFee>();
			lineFeeB00.CF_ChargeType = "B00";
			lineFeeB00.CF_MethodOfCalculation = "MC";
			lineFeeB00.CF_MethodOfPayment = "MPB";
			lineFeeB00.NationalFeeTypeCode = "NTB";

			var stashManager = new UserEnteredStashManager();
			stashManager.Stash(lineFeeA00.UserEnteredStashSource);
			stashManager.Stash(lineFeeB00.UserEnteredStashSource);

			var lineFee1 = Factory.New<CusEntryLineFee>();
			lineFee1.CF_ChargeType = "C00";
			var lineFee1Applied = stashManager.Apply(lineFee1.UserEnteredStashSource);
			AssertEquals("Nothing is applied because the charge type don't match.", false, lineFee1Applied);
			AssertEquals("Method of payment keeps empty.", "", lineFee1.CF_MethodOfPayment);
			AssertEquals("National type keeps empty.", "", lineFee1.NationalFeeTypeCode);

			var lineFee2 = Factory.New<CusEntryLineFee>();
			lineFee2.CF_ChargeType = "A00";
			var lineFee2Applied = stashManager.Apply(lineFee2.UserEnteredStashSource);
			AssertEquals("Nothing is applied because the charge type don't match.", false, lineFee2Applied);
			AssertEquals("Method of payment keeps empty.", "", lineFee2.CF_MethodOfPayment);
			AssertEquals("National type keeps empty.", "", lineFee2.NationalFeeTypeCode);

			var lineFee3 = Factory.New<CusEntryLineFee>();
			lineFee3.CF_ChargeType = "B00";
			lineFee3.CF_MethodOfCalculation = "MC";
			var lineFee3Applied = stashManager.Apply(lineFee3.UserEnteredStashSource);
			AssertEquals("Applied because the charge type and method of calculation match.", true, lineFee3Applied);
			AssertEquals("Method of payment is applied.", "MPB", lineFee3.CF_MethodOfPayment);
			AssertEquals("National type is applied.", "NTB", lineFee3.NationalFeeTypeCode);
		}
	}
}
