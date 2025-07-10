using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Messaging;

namespace Enterprise.Customs.GB.Chief.CusDec.Testing
{
	public class MiscCusDecTests : TestCaseWithFactory
	{
		public void TestChiefDateParser()
		{
			ZDateTime dansBirthday = new ZDateTime(1979, 8, 9, 8, 56, 0);  //GMT
			ZDateTime dansBirthdayDateOnly = new ZDateTime(1979, 8, 9, 0, 0, 0);  //GMT
			AssertEquals(dansBirthday, GbHeader.GetDateFromChiefFormat("197908090856", "203"));  // 203 = CCYYMMDDHHMM
			AssertEquals(dansBirthdayDateOnly, GbHeader.GetDateFromChiefFormat("19790809", "102"));   // 102  CCYYMMDD

			AssertEquals(ZDateTime.Empty, GbHeader.GetDateFromChiefFormat("19790809", "xxxx"));
			AssertEquals(ZDateTime.Invalid, GbHeader.GetDateFromChiefFormat("xxxxxxxx", "102"));
			AssertEquals(ZDateTime.Invalid, GbHeader.GetDateFromChiefFormat("     ", "102"));
			AssertEquals(ZDateTime.Empty, GbHeader.GetDateFromChiefFormat("", "102"));
			AssertEquals(ZDateTime.Empty, GbHeader.GetDateFromChiefFormat("", ""));
			AssertEquals(ZDateTime.Empty, GbHeader.GetDateFromChiefFormat("19790809", ""));
			AssertEquals(ZDateTime.Invalid, GbHeader.GetDateFromChiefFormat("99999999999999999999999999999999", "102"));
		}
	}
}
