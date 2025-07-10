using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class AWBMessageProcessorTest : TestCaseWithFactory
	{
		readonly CLBranchMessageProcessor processor = new CLBranchMessageProcessor { Logger = new LoggingInformation() };

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAcceptedMessageFirstSubmit()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLUKB239892";
			bill.ABL_MessageStatus = "AWA";
			bill.ABL_BillStatus = "SNT";

			var message = CreateMessage(acceptedResponse, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();
			bill.CustomsEntryNumbers.Reload(true);

			var cusEntryNumCreated = Factory.Load<CusEntryNumber>(new ZQuery());
			var cusEntryNum = cusEntryNumCreated[0];

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): 349805154SAO
Customs ID (id-documento-servidor): 18933739

Status: Aprobado

Datetime sent: 2021-03-10 18:09:43
Datetime response: 2021-03-10 18:09:58
";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLUKB239892 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("When an original message is accepted, the Bill Status should be set to ACP.", CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals("When an original message is accepted, the Message Status should be set to ACP.", CustomsStatusList.Codes.ACP, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals("18933739", bill.CustomsEntryNumber);
				AssertEquals("IDS", bill.CustomsEntryNumberType);

				AssertEquals(cusEntryNum.CE_ParentID, bill.PK);
				AssertEquals(cusEntryNum.CE_ParentTable, "AsycudaBill");
				AssertEquals(cusEntryNum.CE_EntryNum, bill.CustomsEntryNumber);
				AssertEquals(cusEntryNum.CE_EntryType, bill.CustomsEntryNumberType);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedMessageFirstSubmit()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLTAO239432";
			bill.ABL_MessageStatus = "AWA";
			bill.ABL_BillStatus = "SNT";

			var message = CreateMessage(rejectedResponse, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): 350021348MIA

Status: Rechazado

Datetime sent: 2021-03-11 11:18:15
Datetime response: 2021-03-11 11:18:30

Response error details: 
En Referencias [1] el MFTOA con fecha de Emision 22-02-2021 y Numero Referencia 888022 no se encuentra en los registros del sistema
En Referencias, la Guía Aérea:[045-41244932], con fecha Emision:05-03-2021 no se encuentra en los registros del sistema.-
Peso bruto indicado en la GA hija supera en peso bruto señalado en la Guía Aérea Madre:[045-41244932].
En Fechas, para [FPRES] [valor] [07-03-2021] debe ser igual a fecha del día [11-03-2021]

Response warning details: 
En Referencias, la Guía Aérea:[045-41244932], con fecha Emision:05-03-2021 no se encuentra en los registros del sistema.
";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLTAO239432 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("On failure of the first Submit the Bill Status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_BillStatus);
				AssertEquals("On failure of the first Submit the Message Status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAcceptedMessageResend()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLUKB239892";
			bill.ABL_MessageStatus = "AWA";
			bill.ABL_BillStatus = "ACP";

			var message = CreateMessage(acceptedResponse, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();
			bill.CustomsEntryNumbers.Reload(true);

			var cusEntryNumCreated = Factory.Load<CusEntryNumber>(new ZQuery());
			var cusEntryNum = cusEntryNumCreated[0];

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): 349805154SAO
Customs ID (id-documento-servidor): 18933739

Status: Aprobado

Datetime sent: 2021-03-10 18:09:43
Datetime response: 2021-03-10 18:09:58
";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLUKB239892 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("When a Resend Message is accepted, the Bill Status should be set to ACP.", CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals("When a Resend Message is accepted, the Message Status should be set to ACP.", CustomsStatusList.Codes.ACP, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals("18933739", bill.CustomsEntryNumber);
				AssertEquals("IDS", bill.CustomsEntryNumberType);

				AssertEquals(cusEntryNum.CE_ParentID, bill.PK);
				AssertEquals(cusEntryNum.CE_ParentTable, "AsycudaBill");
				AssertEquals(cusEntryNum.CE_EntryNum, bill.CustomsEntryNumber);
				AssertEquals(cusEntryNum.CE_EntryType, bill.CustomsEntryNumberType);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedMessageResend()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLTAO239432";
			bill.ABL_MessageStatus = "AWA";
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

			var expectedresult = @"Bill Number (numero-referencia): 350021348MIA

Status: Rechazado

Datetime sent: 2021-03-11 11:18:15
Datetime response: 2021-03-11 11:18:30

Response error details: 
En Referencias [1] el MFTOA con fecha de Emision 22-02-2021 y Numero Referencia 888022 no se encuentra en los registros del sistema
En Referencias, la Guía Aérea:[045-41244932], con fecha Emision:05-03-2021 no se encuentra en los registros del sistema.-
Peso bruto indicado en la GA hija supera en peso bruto señalado en la Guía Aérea Madre:[045-41244932].
En Fechas, para [FPRES] [valor] [07-03-2021] debe ser igual a fecha del día [11-03-2021]

Response warning details: 
En Referencias, la Guía Aérea:[045-41244932], con fecha Emision:05-03-2021 no se encuentra en los registros del sistema.
";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLTAO239432 has been rejected. For details please follow the Link to the Manifest"));
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
			message.EM_MessageType = MessageTypes.Codes.CHE;
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

		readonly ZString acceptedResponse = CLInboundInterchangeProcessorTest.GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.AirAcceptedResponse));
		readonly ZString rejectedResponse = CLInboundInterchangeProcessorTest.GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.AirRejectedResponse));
	}
}
