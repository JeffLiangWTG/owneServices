using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	sealed class AirFirstResponseProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAcceptedMessage()
		{
			var header = CreateHeader();
			var bill = CreateBill(header);

			var messsageResponse = CreateResponseMessage(bill.PK, GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirAcceptedFirstResponse)));

			processor.ExecuteBatch();
			messsageResponse.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101, bill 1 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, messsageResponse.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, messsageResponse.EM_LinkTable);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messsageResponse.EM_Status);

				AssertEquals(CustomsStatusList.Codes.SNT, bill.ABL_MessageStatus);
				AssertEquals("HBL : 1 accepted by Customs\r\nStatus : true", messsageResponse.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedMessage()
		{
			var header = CreateHeader();
			var bill = CreateBill(header);

			var messsageResponse = CreateResponseMessage(bill.PK, GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirRejectedFirstResponse)));

			processor.ExecuteBatch();
			messsageResponse.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101, bill 1 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, messsageResponse.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, messsageResponse.EM_LinkTable);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messsageResponse.EM_Status);

				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);

				AssertEquals("HBL : 1 has been rejected\r\nStatus : false\r\nDescription:\r\nFalta campo obligatorio", messsageResponse.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedWithEnvelopeMessage()
		{
			var header = CreateHeader();
			var bill = CreateBill(header);

			var messsageResponse = CreateResponseMessage(bill.PK, GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirRejectedWithEnvelopeFirstResponse)));

			processor.ExecuteBatch();
			messsageResponse.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101, bill 1 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, messsageResponse.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, messsageResponse.EM_LinkTable);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messsageResponse.EM_Status);

				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);

				AssertEquals("HBL : 1 has been rejected\r\nStatus : false\r\nDescription:\r\nFalta campo obligatorio", messsageResponse.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessFaultMessage()
		{
			var header = CreateHeader();
			var bill = CreateBill(header);

			var messsageResponse = CreateResponseMessage(bill.PK, GetExpectedMessageXML(Path.Combine(BaseSourcePath, MXMessagingConstants.AirFaultFirstResponse)));

			processor.ExecuteBatch();
			messsageResponse.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101, bill 1 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, messsageResponse.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, messsageResponse.EM_LinkTable);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messsageResponse.EM_Status);

				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);

				AssertEquals("HBL : 1 has been rejected\r\nStatus : fault\r\nDescription:\r\nFalta campo obligatorio\r\nMensaje comprimido incorrectamente", messsageResponse.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_JobReference = "ETG0010101";
			header.AMA_TransportMode = "AIR";
			return header;
		}

		AsycudaBill CreateBill(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "1";
			return bill;
		}

		EDIMessage CreateResponseMessage(ZGuid billPK, ZString body)
		{
			var requestInterchange = CreateInterchange(ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MessageTypes.Codes.MXE, MXMessageConstants.MXCustomsForAirMode);
			var requestMessage = CreateMessage(body, requestInterchange.PK, billPK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXE);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypes.Codes.MXF, MXMessageConstants.MXCustomsForAirMode);
			return CreateMessage(body, responseInterchange.PK, billPK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypes.Codes.MXF);
		}

		MXInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status, ZString interchangeType, ZString eiFrom)
		{
			var interchange = Factory.New<MXInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.MXCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;

			Factory.Save();
			return interchange;
		}

		MXMessage CreateMessage(ZString bodyText, ZGuid interchangePK, ZGuid billPK, ZString direction, ZString status, ZString messageType)
		{
			var message = Factory.New<MXMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.MXCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;
			message.EM_EI = interchangePK;
			message.EM_LinkTable = AsycudaBillSchema.Constants.TableName;
			message.EM_LinkUniqueID = billPK;
			message.EM_SystemCreateUser = Staff.GS_Code;

			Factory.Save();
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			processor = new MXBranchMessageProcessor { Logger = new LoggingInformation() };
		}
		MXBranchMessageProcessor processor;

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

		static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}
	}
}
