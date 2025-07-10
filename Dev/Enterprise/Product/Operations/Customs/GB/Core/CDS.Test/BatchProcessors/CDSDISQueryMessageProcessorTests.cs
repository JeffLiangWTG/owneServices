using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDISQueryMessageProcessorTests : TestCaseWithFactory
	{
		public void TestProcess()
		{
			using (Factory.AddDisposableService())
			{
				var msg1 = Factory.New<CDSDISQueryMessage>();
				msg1.EM_MessageOwner = $"12345678901234.XYZ";
				var msg2 = Factory.New<CDSDISQueryMessage>();
				msg2.EM_MessageOwner = $"12345678901234.QWE";
				msg2.EM_IsActive = false;
				Factory.Save();

				var loggingInformation = new LoggingInformation();
				new CDSDISQueryMessageProcessor(loggingInformation).ProcessMessage(CancellationToken.None);

				msg1.Reload();
				AssertNotNull(msg1);
				AssertEquals("CDQ", msg1.EM_ApplicationCode);
				AssertEquals("XUE", msg1.EM_MessageType);
				AssertEquals("TRX", msg1.EM_ReceiveTransmit);
				AssertEquals("SNT", msg1.EM_Status);

				Assert("Should be linked to interchange", !msg1.EM_EI.IsEmpty);

				var interchange = Factory.Load<EDIInterchange>(msg1.EM_EI);
				AssertNotNull("Interchange should exist", interchange);
				AssertEquals("AppCode", "UDM", interchange.EI_ApplicationCode);
				AssertEquals("Int Type", "XUE", interchange.EI_InterchangeType);
				AssertEquals("From Badge", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("TO GB Customs", "GBCustoms", interchange.EI_To);

				msg2.Reload();
				AssertEquals("Msg should not be sent", EDIMessageStatusList.Codes.Queued, msg2.EM_Status);
				AssertEquals("Should have succesful log string", expected: true, loggingInformation.Logs.Any(x => x.Message.Equals(ZString.Format("Message {0} has been packaged into interchange {1} and queued for sending via eHub", msg1.EM_MessageNum, msg1.Interchange.EI_InterchangeNum), System.StringComparison.OrdinalIgnoreCase)));
			}
		}
	}
}

