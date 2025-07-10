using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.GovernmentGateway.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.GovernmentGateway.NCTS.Testing
{
	class CTCMessageTypeListTests : TestCaseWithFactory
	{
		public void TestCTCMessageTypeList()
		{
			var list = new CTCMessageTypeList();
			Assert(list.ContainsCode("CC007A"));
			Assert(list.ContainsCode("CC008A"));
			Assert(list.ContainsCode("CC009A"));
			Assert(list.ContainsCode("CC014A"));
			Assert(list.ContainsCode("CC015B"));
			Assert(list.ContainsCode("CC016A"));
			Assert(list.ContainsCode("CC025A"));
			Assert(list.ContainsCode("CC028A"));
			Assert(list.ContainsCode("CC029B"));
			Assert(list.ContainsCode("CC043A"));
			Assert(list.ContainsCode("CC044A"));
			Assert(list.ContainsCode("CC045A"));
			Assert(list.ContainsCode("CC051B"));
			Assert(list.ContainsCode("CC055A"));
			Assert(list.ContainsCode("CC058A"));
			Assert(list.ContainsCode("CC060A"));
			Assert(list.ContainsCode("CC928A"));
		}

		public void TestCTCOutgoingDepartureMessageTypeList()
		{
			var list = new CTCOutgoingDepartureMessageTypeList();
			AssertEquals("013, 014, 015, 170", list.CodesAsString);
		}

		public void TestCTCIncomingDepartureMessageTypeList()
		{
			var list = new CTCIncomingDepartureMessageTypeList();
			AssertEquals("04, 09, 16, 182, 19, 22, 28, 29, 35, 45, 51, 55, 56, 60, 928", list.CodesAsString);
		}

		public void TestCTCOutgoingArrivalMessageTypeList()
		{
			var list = new CTCOutgoingArrivalMessageTypeList();
			AssertEquals("007, 044", list.CodesAsString);
		}

		public void TestCTCIncomingArrivalMessageTypeList()
		{
			var list = new CTCIncomingArrivalMessageTypeList();
			AssertEquals("25, 43, 57", list.CodesAsString);
		}
	}
}
