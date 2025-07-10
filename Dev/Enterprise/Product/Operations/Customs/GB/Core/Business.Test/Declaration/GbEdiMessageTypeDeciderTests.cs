using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GbEdiMessageTypeDeciderTests : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new GbEDIMessageTypeDecider();
			var message = Factory.New<GbEDIMessage>();
			var row = ((INeedRow)message).Row;

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			Assert(typeDecider.GetTypeForLoad(row, Factory).GetInterfaces().Contains(typeof(Integration.Customs.GB.GBCDS.IGbCDSEdiMessage)));

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCcsuk;
			AssertEquals(typeof(GbEDIMessage), typeDecider.GetTypeForLoad(row, Factory));

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCDSDISQuery;
			AssertEquals(typeof(CDS.CDSDISQueryMessage), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
