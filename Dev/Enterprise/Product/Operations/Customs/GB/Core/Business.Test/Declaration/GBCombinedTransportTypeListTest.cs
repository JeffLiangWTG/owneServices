using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class GBCombinedTransportTypeListTest : TestCase
	{
		public void TestCombinedList()
		{
			var list = new GBCombinedTransportTypeList();
			Assert(list.ContainsCode(GBTransportTypeList.Codes.ROR));
			Assert(list.ContainsCode(TransportTypeList.Codes.Air));
			Assert(list.ContainsCode(TransportTypeList.Codes.Sea));
			Assert(list.ContainsCode(TransportTypeList.Codes.Mail));
			Assert(list.ContainsCode(TransportTypeList.Codes.Road));
			Assert(list.ContainsCode(TransportTypeList.Codes.Rail));
			Assert(list.ContainsCode(TransportTypeList.Codes.FixedTransportInstallations));
			Assert(list.ContainsCode(TransportTypeList.Codes.InlandWaterwayTransport));
			Assert(list.ContainsCode(TransportTypeList.Codes.OwnPropulsion));
		}
	}
}
