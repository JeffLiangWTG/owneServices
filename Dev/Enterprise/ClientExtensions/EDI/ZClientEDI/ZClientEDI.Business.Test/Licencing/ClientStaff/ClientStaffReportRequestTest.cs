using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.ZArchitecture.Environment;
using ZClientEDI.Business.Licencing;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	class ClientStaffReportRequestTest : TestCaseWithFactory
	{
		public void TestSendSystemMessage()
		{
			DebugOnlyOutgoingSystemMessage.Initialize();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(new DebugOnlyOutgoingSystemMessage());

			var db = BillingTestHelper.CreateLicence(Factory, "DDD").Database;
			ClientStaffReportRequest.Send(db);
			string expectedXml = "<StaffReportRequest />";
			var actualXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;
			AssertEquals(expectedXml, actualXml);
		}
	}
}
