using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class ESH7SendG3CustomsDeclarationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSendG3CustomsDeclarationMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var processor = new ESH7SendG3CustomsDeclarationMessageProcessor(header);
			var notification = new NotificationBuffer();
			processor.Process(notification);

			CombineAssertions("Send G3 customs declaration message", () =>
			{
				AssertContains("Could not create G3 message for H7 Job.", notification.AsString);

				notification.Clear();
				ErrorReporter.Clear();

				header.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES1230789654", "ES");
				processor.Process(notification);
				var messages = Factory.Load<EDIMessage>(new ZQuery());

				AssertEquals("Number of message sent", 1, messages.Length);
				AssertEquals("Should create 1 message for header", header.PK, messages[0].EM_LinkUniqueID);
				AssertContains("Should have notification information", $"Successfully sent G3 Customs Declaration for {header.HumanReadableName}.", notification.AsString);
			});
		}
	}
}
