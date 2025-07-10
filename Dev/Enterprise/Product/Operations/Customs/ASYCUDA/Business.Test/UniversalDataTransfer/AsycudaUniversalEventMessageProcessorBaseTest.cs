using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(AsycudaUniversalEventMessageProcessor))]
	sealed class AsycudaUniversalEventMessageProcessorBaseTest : TestCaseWithFactory
	{
		public void TestSendEmail()
		{
			pmgGroup = Factory.NewWithPrimaryKey<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			pmgGroup.GG_Code = "PMG";
			groupStaff = pmgGroup.Staff.AddNew();
			groupStaff.GS_Code = "PG1";
			groupStaff.GS_LoginName = "PMGS1";
			groupStaff.GS_EmailAddress = "pmgu1@cargowise.com";

			using (ManifestCustomsDataRegistry.Instance.SendSuccessNotifications.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, "ESG"))
			{
				using (ManifestCustomsDataRegistry.Instance.GroupToSendSucceedNotification.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, newGroup.PK.ToGuid()))
				{
					var processor = new AsycudaUniversalEventMessageSuccessProcessor(logger, universalEvent, message, header);
					processor.Process();

					var email = Env.OutgoingMailManager.EmailsCreated.Find(obj => obj.Subject == "Global Manifest Universal Event Response Received: Success");
					CombineAssertions(() =>
					{
						AssertNotNull(email);
						AssertEquals("Should have 2 recipients", 2, email.Recipients.Count);
						Assert("Should have a recipient ns1@cargowise.com", email.Recipients.Contains("ns1@cargowise.com"));
						Assert("Should have a recipient ns2@cargowise.com", email.Recipients.Contains("ns2@cargowise.com"));
					});

					var emptyEmailStaff = Factory.New<GlbStaff>();
					emptyEmailStaff.GS_Code = "EEU";
					emptyEmailStaff.GS_LoginName = "EEU";
					emptyEmailStaff.GS_EmailAddress = ZString.Empty;
					message.EM_SystemCreateUser = "EEU";

					Env.OutgoingMailManager.EmailsCreated.Clear();

					processor.Process();
					email = Env.OutgoingMailManager.EmailsCreated.Find(obj => obj.Subject == "Global Manifest Universal Event Response Received: Success");
					CombineAssertions("user's email is empty", () =>
					{
						AssertNotNull(email);
						AssertEquals("Should have 1 recipients", 1, email.Recipients.Count);
						Assert("Should have a recipient ns1@cargowise.com", email.Recipients.Contains("ns1@cargowise.com"));
					});
				}

				Env.OutgoingMailManager.EmailsCreated.Clear();
				using (ManifestCustomsDataRegistry.Instance.GroupToSendSucceedNotification.SetTemporaryValue(Guid.Empty, Env.CurrentCompany.PK, Guid.Empty, Guid.Empty))
				{
					var processor = new AsycudaUniversalEventMessageSuccessProcessor(logger, universalEvent, message, header);
					processor.Process();
					var email = Env.OutgoingMailManager.EmailsCreated.Find(obj => obj.Subject == "Global Manifest Universal Event Response Received: Success");
					CombineAssertions("group is empty, send email to PMG", () =>
					{
						AssertNotNull(email);
						AssertEquals("Should have 1 recipients", 1, email.Recipients.Count);
						Assert("Should have a PMG recipient", email.Recipients.Contains("pmgu1@cargowise.com"));
					});
				}
			}
		}

		public void TestInvalidFormattedUXML()
		{
			message.EM_MessageText = AsycudaUniversalEventMessageProcessorTest.InvalidMessageText;
			var processor = new AsycudaUniversalEventMessageSuccessProcessor(logger, universalEvent, message, header);
			AssertNoExceptionThrown(() => processor.Process());
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			newGroup = Factory.New<GlbGroup>();
			newGroup.GG_Code = "NG1";
			groupStaff = newGroup.Staff.AddNew();
			groupStaff.GS_Code = "NS1";
			groupStaff.GS_LoginName = "NS1";
			groupStaff.GS_EmailAddress = "ns1@cargowise.com";

			senderStaff = Factory.New<GlbStaff>();
			senderStaff.GS_Code = "NS2";
			senderStaff.GS_LoginName = "NS2";
			senderStaff.GS_EmailAddress = "ns2@cargowise.com";

			message = Factory.New<AsycudaEDIMessage>();
			message.EM_MessageText = AsycudaUniversalEventMessageProcessorTest.SuccessMessageText;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_SystemCreateUser = "NS2";
			universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			header = CreateManifestHeader();
		}

		AsycudaManifestHeader CreateManifestHeader() => (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();

		XmlSessionTracker logger;

		GlbGroup newGroup;
		GlbGroup pmgGroup;
		GlbStaff groupStaff;
		GlbStaff senderStaff;

		AsycudaEDIMessage message;
		AsycudaManifestHeader header;
		UniversalEvent universalEvent;
	}
}
