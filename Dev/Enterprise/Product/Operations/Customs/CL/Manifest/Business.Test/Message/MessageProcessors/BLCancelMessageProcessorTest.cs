using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class BLCancelMessageProcessorTest : TestCaseWithFactory
	{
		readonly CLBranchMessageProcessor processor = new CLBranchMessageProcessor { Logger = new LoggingInformation() };

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAcceptedMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)KEYBUN7880";
			bill.ABL_BillStatus = "ACP";

			var message = CreateMessage(acceptedResponse, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();
			bill.CustomsEntryNumbers.Reload(true);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): (H)KEYBUN7880
Customs ID (id-documento-servidor): 15715152

Status: Aprobado

Datetime sent: 2020-09-21 08:37:08
Datetime response: 2020-09-21 08:38:23

";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)KEYBUN7880 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("When an Cancel message is accepted, the Bill Status should be set to CAN.", CustomsStatusList.Codes.CAN, bill.ABL_BillStatus);
				AssertEquals("When an Cancel message is accepted, the Message Status should be set to CAN.", CustomsStatusList.Codes.CAN, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)KEYBUN7880";
			bill.ABL_BillStatus = "ACP";

			var message = CreateMessage(rejectedResponse, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): (H)KEYBUN7880
Customs ID (id-documento-servidor): 68630860

Status: Rechazado

Datetime sent: 2020-11-04 12:19:37
Datetime response: 2020-11-04 12:19:52

Response error details: 
El [id-documento-servidor] 68630860 es incorrecto. No se encuentra registrado en el sistema
En Participaciones, [valor-id] [76301972-1] es incorrecto. No se encuentra registrado en el sistema
";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)KEYBUN7880 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("On failure of the Resend Submit the Bill Status should be kept as ACP", CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals("On failure of the Resend Submit the Message Status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_JobReference = "MAN0000001";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Chile;

			Factory.Save();
			return header;
		}

		CLMessage CreateMessage(ZString messageText, ZGuid billPK)
		{
			var message = Factory.New<CLMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_ApplicationReference = "0000000001";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = "0000000001";
			message.EM_MessageText = messageText;
			message.EM_MessageType = MessageTypes.Codes.CHC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			message.EM_LinkUniqueID = billPK;
			message.EM_SystemCreateUser = Staff.GS_Code;

			Factory.Save();
			return message;
		}

		GlbStaff Staff => staff ?? (staff = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com"));
		GlbStaff staff;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		readonly ZString acceptedResponse = CLInboundInterchangeProcessorTest.GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaCancelAcceptedResponse));
		readonly ZString rejectedResponse = CLInboundInterchangeProcessorTest.GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaCancelRejectedResponse));
	}
}
