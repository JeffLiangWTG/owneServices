using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsMessageTypeListTest : TestCase
	{
		public void TestArrivalMessageTypeList()
		{
			AssertEquals("DESNOT, DESREM", NctsMessageTypeList.NctsArrivalMessageTypeList.CodesAsString);
		}

		public void TestDepartureMessageTypeList()
		{
			AssertEquals("DEPDAT", NctsMessageTypeList.NctsDepartureMessageTypeList.CodesAsString);
		}
	}
}
