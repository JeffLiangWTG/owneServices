using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	sealed class EUH7SendCustomsDeclarationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSendCustomsDeclarationMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();

				var processor = new EUH7SendCustomsDeclarationMessageProcessor(header);
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
}
