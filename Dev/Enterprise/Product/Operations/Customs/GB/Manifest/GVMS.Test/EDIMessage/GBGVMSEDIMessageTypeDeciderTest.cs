using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GBGVMSEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var typeDecider = new GBGVMSEDIMessageTypeDecider();
			AssertEquals(typeof(GVMSEDIMessage), typeDecider.GetTypeForLoad(null, Factory));
		}

		public void TestGetTypeForLoadGeneral()
		{
			var typeDecider = new GbEDIMessageTypeDecider();
			var message = Factory.New<GbEDIMessage>();
			var row = ((INeedRow)message).Row;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsGVMSManifest;
			AssertEquals(typeof(GVMSEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
