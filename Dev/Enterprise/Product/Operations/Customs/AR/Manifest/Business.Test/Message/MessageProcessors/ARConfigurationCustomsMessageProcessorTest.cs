using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(ARConfigurationCustomsMessageProcessor))]
	sealed class ARConfigurationCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessageRejected()
		{
			var helper = new ARMessageTestingHelper();
			var (credential, ediMessageToProcess) = CreateCompanyMessagesAndInterchanges(helper.ReadManifestResourceContent(ARMessageTestingConstants.XTRejectedResponse), helper.ReadManifestResourceContent(ARMessageTestingConstants.XTOutgoingInterchange));

			var processor = new ARConfigurationCustomsMessageProcessor();
			processor.ProcessMessage(ediMessageToProcess, null);
			Factory.Save();

			AssertEquals("Status", PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, ediMessageToProcess.EM_Status);
		}

		public void TestProcessMessageAccepted()
		{
			var helper = new ARMessageTestingHelper();
			var (credential, ediMessageToProcess) = CreateCompanyMessagesAndInterchanges(helper.ReadManifestResourceContent(ARMessageTestingConstants.XTAcceptedResponse), helper.ReadManifestResourceContent(ARMessageTestingConstants.XTOutgoingInterchange));

			var processor = new ARConfigurationCustomsMessageProcessor();
			processor.ProcessMessage(ediMessageToProcess, null);
			Factory.Save();

			AssertEquals("Status", "REG", credential.GP_PasswordStatus);
			AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, ediMessageToProcess.EM_Status);
		}

		public (GlbCompanyCredential, EDIMessage) CreateCompanyMessagesAndInterchanges(ZString messageText, ZString interchangeText)
		{
			var credential = CreateCompanyCredential();

			var receivedEdiMessage = CreateResponseMessage(ZGuid.NewZGuid(), messageText, interchangeText);

			return (credential, receivedEdiMessage);
		}

		public GlbCompanyCredential CreateCompanyCredential()
		{
			var company = GlbBranch.CurrentBranch.Company;
			company.GC_RN_NKCountryCode = CountryCodes.Argentina;

			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company).GlbExternalPassword;
			credential.GP_UserID = "ADMIN13";
			credential.GP_CurrentPassword = "9974567890";

			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_PasswordType = PasswordTypesList.Codes.ARB;

			GlbCompany.CurrentCompany.Factory.Save();

			Factory.Save();
			return credential;
		}

		EDIMessage CreateResponseMessage(ZGuid billPK, ZString body, ZString interchangeText)
		{
			var requestInterchange = CreateInterchange(ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received, GlbARExternalPassword.InterchangeTypeForSending, "WTLDARCTU", "CustomsCredentialChange", interchangeText);
			CreateMessage(body, requestInterchange.PK, billPK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received, MessageTypes.Codes.ARD);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, EDIInterchangeTypeList.Codes.Configuration, "CustomsCredentialChange", "WTLDARCTU", ZString.Empty);
			return CreateMessage(body, responseInterchange.PK, billPK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, EDIInterchangeTypeList.Codes.Configuration);
		}

		EDIInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status, ZString interchangeType, ZString eiFrom, ZString eiTo, ZString messageText)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = EDIInterchangeTypeList.Codes.Configuration;
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_From = eiFrom;
			interchange.EI_To = eiTo;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;
			interchange.EI_BodyText = messageText;

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
			message.EM_SystemCreateUser = "E";

			return message;
		}
	}
}
