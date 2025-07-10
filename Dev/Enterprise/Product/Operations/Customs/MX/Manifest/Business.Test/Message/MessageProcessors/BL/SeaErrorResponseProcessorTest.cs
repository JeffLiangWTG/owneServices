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

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	sealed class SeaErrorResponseProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomsErrorMessage()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";

			var headerTextErrorNotification = Path.Combine(BaseSourcePath, MXMessagingConstants.HeaderTextErrorNotification);
			var responseMessage = CreateResponseMessage(bill.PK, MXMessageConstants.XER, GetExpectedMessageTXT(headerTextErrorNotification));
			Factory.Save();

			processor.ExecuteBatch();
			responseMessage.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Error Type : ERR
Notification Type : Failure
Error Description : Error Message
Contract: xt-contract:/Customs/MX.AsyncSeaOutbound/MX.AsyncSeaOutbound.Contract
Reference Object: xt-httpclientaddress:{4abb1ed8-32a3-47fd-819e-722073db0e6a}
Reference Object: xt-node:{1ed5d982-4d5e-4cd6-b3f8-4e173ecba727}

Error description: Message transmission to https://wwwqa.ventanillaunica.gob.mx/ManifiestoMaritimo309SOImpl/ManifiestoMaritimo309SO?wsdl rejected by peer: xT certificate validation failed
Reference Object: xt-httpclientaddress:{4abb1ed8-32a3-47fd-819e-722073db0e6a}
Reference Object: xt-node:{1ed5d982-4d5e-4cd6-b3f8-4e173ecba727}

Error description: Message transmission to https://wwwqa.ventanillaunica.gob.mx/ManifiestoMaritimo309SOImpl/ManifiestoMaritimo309SO?wsdl rejected by peer: xT certificate validation failed
Reference Object: xt-httpclientaddress:{4abb1ed8-32a3-47fd-819e-722073db0e6a}
Reference Object: xt-node:{1ed5d982-4d5e-4cd6-b3f8-4e173ecba727}

Error description: Message transmission to https://wwwqa.ventanillaunica.gob.mx/ManifiestoMaritimo309SOImpl/ManifiestoMaritimo309SO?wsdl rejected by peer: xT certificate validation failed
Reference Object: xt-httpclientaddress:{4abb1ed8-32a3-47fd-819e-722073db0e6a}
Reference Object: xt-node:{1ed5d982-4d5e-4cd6-b3f8-4e173ecba727}

Error description: Message transmission to https://wwwqa.ventanillaunica.gob.mx/ManifiestoMaritimo309SOImpl/ManifiestoMaritimo309SO?wsdl rejected by peer: xT certificate validation failed
Reference Object: xt-httpclientaddress:{4abb1ed8-32a3-47fd-819e-722073db0e6a}
Reference Object: xt-node:{1ed5d982-4d5e-4cd6-b3f8-4e173ecba727}

Error description: Message transmission to https://wwwqa.ventanillaunica.gob.mx/ManifiestoMaritimo309SOImpl/ManifiestoMaritimo309SO?wsdl rejected by peer: xT certificate validation failed
Reference Object: xt-httpclientaddress:{4abb1ed8-32a3-47fd-819e-722073db0e6a}
Reference Object: xt-node:{1ed5d982-4d5e-4cd6-b3f8-4e173ecba727}

Error description: Message transmission to https://wwwqa.ventanillaunica.gob.mx/ManifiestoMaritimo309SOImpl/ManifiestoMaritimo309SO?wsdl rejected by peer: xT certificate validation failed
";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job ETG0010101, bill 8AZ8493 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, responseMessage.EM_LinkTable);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
				AssertContains(expectedresult, responseMessage.EM_MessageInterpretation);

				AssertEquals(MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
				AssertEquals(CustomsStatusList.Codes.ERR, bill.ABL_BillStatus);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_JobReference = "ETG0010101";
			return header;
		}

		EDIMessage CreateResponseMessage(ZGuid billPK, ZString messageType, ZString bodyText)
		{
			var requestInterchange = CreateInterchange(ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, MessageTypes.Codes.MXA, MXMessageConstants.MXCustomsForSeaMode);
			var requestMessage = CreateMessage(bodyText, requestInterchange.PK, billPK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.MXA);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, messageType, MXMessageConstants.MXCustomsForSeaMode);
			return CreateMessage(bodyText, responseInterchange.PK, billPK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, messageType);
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

		public static ZString GetExpectedMessageTXT(ZString path)
		{
			StreamReader sr = new StreamReader(path);
			return sr.ReadToEnd();
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

			Factory.Save();
			return staff;
		}

		readonly MXBranchMessageProcessor processor = new MXBranchMessageProcessor { Logger = new LoggingInformation() };
	}
}
