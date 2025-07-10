using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	sealed class BLMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAcceptedMessage()
		{
			var header = CreateHeader();
			var bill1 = CreateBill(header);
			var bill2 = CreateBill(header);
			CreateMessage(string.Empty, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var acceptedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaAcceptedResponse));
			var message = CreateMessage(acceptedResponse, ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill1.Reload();
			bill2.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill1.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill1.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Accepted, bill2.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill2.ABL_BillStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job C123456 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedMessage()
		{
			var header = CreateHeader();
			var bill1 = CreateBill(header);
			var bill2 = CreateBill(header);
			CreateMessage(ZString.Empty, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var rejectedResponse = ARMessageTestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, ARMessageTestingConstants.SeaRejectedResponse));
			var message = CreateMessage(rejectedResponse, ZGuid.Empty, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill1.Reload();
			bill2.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill1.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.SNT, bill1.ABL_BillStatus);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill2.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.SNT, bill2.ABL_BillStatus);

				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job C123456 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "2021087894512445";
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

			Factory.Save();
			return header;
		}

		AsycudaBill CreateBill(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.SNT;
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

			Factory.Save();
			return bill;
		}

		ARMessage CreateMessage(ZString bodyText, ZGuid headerPK, ZString direction, ZString status)
		{
			var message = Factory.New<ARMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_SystemCreateUser = Staff.GS_Code;
			message.EM_MessageText = bodyText;
			message.EM_MessageType = MessageTypes.Codes.ARB;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;

			if (direction == EDIMessage.Direction.Transmit)
			{
				message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
				message.EM_LinkUniqueID = headerPK;
			}

			Factory.Save();
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			registryDisposable = ARCustomsDataRegistry.Instance.EnableARManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			processor = new ARBranchMessageProcessor { Logger = new LoggingInformation() };

			Factory.Save();
		}
		ARBranchMessageProcessor processor;
		IDisposable registryDisposable;

		protected override void TearDown()
		{
			registryDisposable.Dispose();
			base.TearDown();
		}

		#region Staff

		GlbStaff Staff => staff ?? (staff = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com"));
		GlbStaff staff;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;

			Factory.Save();
			return staff;
		}

		#endregion
	}
}
