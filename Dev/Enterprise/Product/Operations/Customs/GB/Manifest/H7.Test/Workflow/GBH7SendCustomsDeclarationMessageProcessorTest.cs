using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class GBH7SendCustomsDeclarationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSendCustomsDeclarationMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var processor = new GBH7SendCustomsDeclarationMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			CombineAssertions("Send customs declaration message", () =>
			{
				AssertContainsExactElementsInAnyOrder("Should create 1 message for each bill", new[] { bill1.PK, bill2.PK }, messages.Select(message => message.EM_LinkUniqueID));
				AssertContains("Should have notification information", $"Sent 2 Customs Declaration for {header.HumanReadableName}", notification.AsString);
			});
		}
	}
}
