using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.HK.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class SenderTest : TestCaseWithFactory
	{
		public void TestSendInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.Traxon;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_HeaderText = "HEADERTEXT";
			interchange.EI_BodyText = "BODYTEXT";
			interchange.EI_FooterText = "FOOTERTEXT";
			interchange.EI_InterchangeNum = "12345";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				sender.ExecuteBatch();

				interchange.Reload();
				AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange.EI_Status);
				var expectedFilename = HKDataRegistry.Instance.HKTraxonOutputDirectory.Value + "\\" + interchange.EI_InterchangeNum + ".edi";
				File.Exists(expectedFilename);
				AssertEquals("FileContents", interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText, File.ReadAllText(expectedFilename));
				File.Delete(expectedFilename);
			}
		}

		public void TestSendMessageDirect()
		{
			using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testTransmitMessage = Factory.New<TraxonMessage>();
				testTransmitMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				Factory.Save();

				sender.ExecuteBatch();

				testTransmitMessage.Reload();
				AssertEquals("MessageState", EDIMessage.Status.Sent, testTransmitMessage.EM_Status);
				var interchange = testTransmitMessage.Interchange;
				AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange.EI_Status);
				var expectedFilename = HKDataRegistry.Instance.HKTraxonOutputDirectory.Value + "\\" + interchange.EI_InterchangeNum + ".edi";
				File.Exists(expectedFilename);
				AssertEquals("FileContents", interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText, File.ReadAllText(expectedFilename));
				File.Delete(expectedFilename);
			}
		}

		public void TestSendMessageeHub()
		{
			traxonOutputDirectoryDisposer?.Dispose();

			using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testTransmitMessage = Factory.New<TraxonMessage>();
				testTransmitMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				Factory.Save();

				sender.ExecuteBatch();

				testTransmitMessage.Reload();
				AssertEquals("MessageState", EDIMessage.Status.Sent, testTransmitMessage.EM_Status);
				var interchange = testTransmitMessage.Interchange;
				AssertEquals("InterchangeState", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			traxonSenderIDDisposer = HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
			traxonRecipientReferencePasswordDisposer = HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027");
			tempTraxonOutPath = Path.Combine(Env.TempPath, "TraxonOut");
			Directory.CreateDirectory(tempTraxonOutPath);
			traxonOutputDirectoryDisposer = HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, tempTraxonOutPath);
			sender = new Sender();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TempDirectory.DeleteDirectory(new DirectoryInfo(tempTraxonOutPath));
			traxonSenderIDDisposer?.Dispose();
			traxonRecipientReferencePasswordDisposer?.Dispose();
			traxonOutputDirectoryDisposer?.Dispose();
		}

		Sender sender;
		protected string tempTraxonOutPath;
		IDisposable traxonSenderIDDisposer;
		IDisposable traxonRecipientReferencePasswordDisposer;
		IDisposable traxonOutputDirectoryDisposer;
	}
}
