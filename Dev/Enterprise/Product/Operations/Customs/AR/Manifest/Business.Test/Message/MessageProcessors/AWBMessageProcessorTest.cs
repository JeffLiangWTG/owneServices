using System.IO;
using System.Xml;
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
	sealed class AWBMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessAcceptedMessage()
		{
			var header = CreateHeader();
			var bill = CreateBill(header);
			var bodyText = GetExpectedMessageXML(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\AR\Manifest\Business.Test\Message\TestFiles\AirMode\Response\Accepted.xml"));
			var messsageResponse = CreateResponseMessage(bill.PK, bodyText);

			processor.ExecuteBatch();
			messsageResponse.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference + "_" + bill.ABL_BillNumber);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101_1 has been cleared. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, messsageResponse.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, messsageResponse.EM_LinkTable);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messsageResponse.EM_Status);

				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals("HBL : 1 Request Accepted by AFIP\r\nID : 123456789/01\r\nName : House Waybill\r\nStatus : Processed", messsageResponse.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectedMessage()
		{
			var header = CreateHeader();
			var bill = CreateBill(header);
			var bodyTextError = GetExpectedMessageXML(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\AR\Manifest\Business.Test\Message\TestFiles\AirMode\Response\Rejected.xml"));
			var messsageResponse = CreateResponseMessage(bill.PK, bodyTextError);

			processor.ExecuteBatch();
			messsageResponse.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference + "_" + bill.ABL_BillNumber);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101_1 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, messsageResponse.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, messsageResponse.EM_LinkTable);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messsageResponse.EM_Status);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_BillStatus);

				AssertEquals("HBL : 1 Request Rejected by AFIP\r\nID : 123456789/01\r\nName : House Waybill\r\nStatus : Rejected\r\nCode of the error : 003  Description:  El campo operación es incorrecto\r\nCode of the error : 013  Description:  No existe información para los criterios de búsquedas proporcionados.", messsageResponse.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			header.AMA_JobReference = "ETG0010101";
			header.AMA_TransportMode = "AIR";
			return header;
		}

		AsycudaBill CreateBill(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "1";
			bill.ABL_BillStatus = "SNT";
			return bill;
		}

		EDIMessage CreateResponseMessage(ZGuid billPK, ZString body)
		{
			var requestInterchange = CreateInterchange(ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MessageTypes.Codes.ARD, ARMessageConstants.ARCustomsAirMode);
			CreateMessage(body, requestInterchange.PK, billPK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.ARD);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypes.Codes.ARE, ARMessageConstants.ARCustomsAirMode);
			return CreateMessage(body, responseInterchange.PK, billPK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypes.Codes.ARE);
		}

		EDIInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status, ZString interchangeType, ZString eiFrom)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ARCustoms;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;

			Factory.Save();
			return interchange;
		}

		ARMessage CreateMessage(ZString bodyText, ZGuid interchangePK, ZGuid billPK, ZString direction, ZString status, ZString messageType)
		{
			var message = Factory.New<ARMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
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

			processor = new ARBranchMessageProcessor { Logger = new LoggingInformation() };
		}
		ARBranchMessageProcessor processor;

		GlbStaff staff;
		GlbStaff Staff => staff ?? (staff = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com"));

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
			var doc = new XmlDocument();
			doc.Load(path);
			return doc.OuterXml;
		}
	}
}
