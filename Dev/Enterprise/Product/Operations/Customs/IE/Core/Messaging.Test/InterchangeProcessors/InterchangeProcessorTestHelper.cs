using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MailboxAcknowledgeResponse;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public static class TestHelperExtensions
	{
		public static string PopulateDataText<T>(this T xmlObject) where T : IXMLMessageObject
		{
			var outputText = IEXmlObjectSerializer.Serialize(xmlObject);
			outputText = Regex.Replace(outputText, @"T00:00:00\+\d{1,2}:00", "T00:00:00");

			return outputText;
		}
	}

	public static class InterchangeProcessorTestHelper
	{
		#region Common
		public const string MessageSenderEORI = "IE12345ABCDE12345";

		public static GlbCompanyCredential CreateValidCredential(GlbCompany company)
		{
			var companyCredential = Enterprise.Customs.IE.Business.GlbCompanyWrapper.Get(company)?.GlbExternalPassword;
			if (!companyCredential.GP_PasswordStatus.EqualsIgnoringCase(PasswordStatusList.Codes.Valid))
			{
				companyCredential.CurrentDecryptedCertificatePassphrase = ROSCertificateTestHelper.ValidPassword;
				companyCredential.GP_Certificate = ROSCertificateTestHelper.ValidCertificate;
				companyCredential.GP_MailBoxID = MessageSenderEORI;
			}
			return companyCredential;
		}

		public static (GlbCompany company, GlbBranch branch) CreateCompanyAndBranch(BusinessObjectFactory factory, ZString countryCode, ZString? id = null)
		{
			var code = id ?? countryCode;
			var company = factory.New<GlbCompany>();
			company.GC_Code = "C" + code;
			company.GC_Name = $"TEST {code} COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = countryCode;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B" + code;
			branch.GB_BranchName = $"TEST {code} BRANCH";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch.GB_RN_NKCountryCode = countryCode;
			return (company, branch);
		}

		public static string FormatXml(ZString inputXml)
		{
			string result;
			using (var stream = new MemoryStream())
			using (var writer = new XmlTextWriter(stream, Encoding.Unicode) { Formatting = Formatting.Indented })
			{
				var document = new XmlDocument();
				document.LoadXml(inputXml);
				document.WriteContentTo(writer);
				writer.Flush();
				stream.Flush();
				stream.Position = 0;
				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}

			return result;
		}

		public static T CreateInboundEDIMessage<T>(BusinessObjectFactory factory, string messageType, string messageText, string transactionID)
			where T : InboundEDIMessage
		{
			var message = factory.New<T>();
			message.EM_MessageType = messageType;
			message.EM_MessageText = messageText;
			message.EM_ApplicationReference = transactionID;
			return message;
		}

		public static EDIInterchange CreateIncomingInterchange(BusinessObjectFactory factory, string applicationCode, string interchangeType, string bodyText, ZGuid? sessionGUID = null, ZGuid? branchPK = null, bool wrapInSOAPEnvelope = true)
		{
			var result = factory.New<EDIInterchange>();
			result.EI_ApplicationCode = applicationCode;
			result.EI_InterchangeType = interchangeType;
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			result.EI_SessionGUID = sessionGUID ?? ZGuid.NewZGuid();
			result.EI_From = "IECustomsTest";
			result.EI_To = "IECustomsTest";
			result.EI_Status = EDIInterchange.Status.Queued;
			result.EI_BodyText = wrapInSOAPEnvelope ? (applicationCode == EDIInterchange.ApplicationCodes.IECustomsEMCS ? GetEMCSSOAPEnvelopeXml(bodyText) : GetSOAPEnvelopeXml(bodyText)) : bodyText;
			if (branchPK.HasValue)
			{
				result.EI_GB = branchPK.Value;
			}
			return result;
		}

		public static string GetEMCSSOAPEnvelopeXml(string xmlBodyText)
		{
			return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<soapenv:Body>
{xmlBodyText}
	</soapenv:Body>
</soapenv:Envelope>";
		}
		public static string GetSOAPEnvelopeXml(string xmlBodyText)
		{
			return $@"<env:Envelope xmlns:env=""http://www.w3.org/2003/05/soap-envelope"">
	<env:Header/>
	<env:Body>
{xmlBodyText}
	</env:Body>
</env:Envelope>";
		}

		public static string GetMailboxCollectResponseMessage(params string[] mailBoxItemMessages)
		{
			return $@"		<mcr:MailboxCollectResponse xmlns:mcr=""http://www.ros.ie/schemas/customs/collectresponse/v1"">
			<mcr:MailboxItemList moremessages=""false"" messagecount=""{mailBoxItemMessages.Length}"">
		{string.Join("\r\n", mailBoxItemMessages)}
			</mcr:MailboxItemList>
		</mcr:MailboxCollectResponse>";
		}

		public static string GetMailboxItemText(string transactionID, string messageText, string mailboxId = "ce45c655-c780-43be-94f8-69ef936ea871", bool includeResponseWrap = true, bool includeEncoding = true)
		{
			var result = $@"<cr:MailboxItem xmlns:cr=""http://www.ros.ie/schemas/customs/collectresponse/v1"">
	<cr:MailboxId>{mailboxId}</cr:MailboxId>
	<cr:TransactionId>{transactionID}</cr:TransactionId>
	<cr:Message>
{messageText}
	</cr:Message>
</cr:MailboxItem>";
			if (includeResponseWrap)
			{
				result = GetMailboxCollectResponseMessage(result);
			}
			return (!includeResponseWrap && includeEncoding ? @"<?xml version=""1.0"" encoding=""utf-8"" ?>
" : string.Empty) + result;
		}

		#endregion

		#region MessageType specific

		#region Common

		public static EDIInterchange CreateMailboxAcknowledgeResponse(BusinessObjectFactory factory, string mailboxId)
		{
			return CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxAcknowledge, GetMailboxAcknowledgeResponseText(mailboxId));
		}

		public static string GetMailboxAcknowledgeResponseText(string mailboxId)
		{
			var response = new MailboxAcknowledgeResponse()
			{
				MailboxAcknowledgementList = new System.Collections.ObjectModel.Collection<MailboxAcknowledgement>()
			};
			response.MailboxAcknowledgementList.Add(new MailboxAcknowledgement()
			{
				MailboxId = mailboxId,
				AcknowledgementStatus = CargoWise.Customs.IE.MessageDefinitions.Common.CustomsTypes.IeAcknowledgementStatus.Success
			});
			return IEXmlObjectSerializer.Serialize(response);
		}

		public static EDIInterchange CreateMessageAcknowledgementResponse(BusinessObjectFactory factory, string transactionId, bool valid = true, string applicationCode = EDIInterchange.ApplicationCodes.IECustomsExport)
		{
			return CreateIncomingInterchange(factory, applicationCode, CommonInterchangeTypeList.Codes.MessageAcknowledge, CreateMessageAcknowledgementText(transactionId, valid: valid));
		}

		public static EDIInterchange CreateMessageAcknowledgementServiceErrorResponse(BusinessObjectFactory factory, string errorCode, ZGuid sessionGUID, string applicationCode = EDIInterchange.ApplicationCodes.IECustomsExport)
		{
			return CreateIncomingInterchange(factory, applicationCode, CommonInterchangeTypeList.Codes.MessageAcknowledge, CreateMessageAcknowledgementServiceErrorText(errorCode), sessionGUID: sessionGUID);
		}

		public static T CreateMessageAcknowledgementMessage<T>(BusinessObjectFactory factory, string applicationCode, string messageType, string transactionId, MessageStatus messageStatus = MessageStatus.Accepted, TransactionIdStatus transactionIdStatus = TransactionIdStatus.Accepted, string errorCode = "")
			where T : EDIMessage
		{
			var message = factory.New<T>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = CommonInterchangeTypeList.Codes.MailboxAcknowledge;
			message.EM_MessageText = CreateMessageAcknowledgementText(transactionId: transactionId, messageStatus: messageStatus, transactionIdStatus: transactionIdStatus, errorCode: errorCode);

			return message;
		}

		public static T CreateMessageAcknowledgementServiceErrorMessage<T>(BusinessObjectFactory factory, string applicationCode, string messageType, string errorCode = "")
			where T : EDIMessage
		{
			var message = factory.New<T>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = CommonInterchangeTypeList.Codes.MailboxAcknowledge;
			message.EM_MessageText = CreateMessageAcknowledgementServiceErrorText(errorCode);

			return message;
		}

		public static string CreateMessageAcknowledgementText(string transactionId, bool valid = true, MessageStatus messageStatus = MessageStatus.Accepted, TransactionIdStatus transactionIdStatus = TransactionIdStatus.Accepted, string errorCode = "")
		{
			var messageAcknowlegement = new MessageAcknowledgement
			{
				TransactionId = transactionId,
				Status = new Status
				{
					MessageStatus = messageStatus,
					TransactionIdStatus = transactionIdStatus
				},
				ErrorReference = new CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.ErrorReference
				{
					ErrorCode = errorCode
				}
			};
			IEXmlObjectSerializer.Serialize(new MessageAcknowledgement());
			return valid ? IEXmlObjectSerializer.Serialize(messageAcknowlegement) : IEXmlObjectSerializer.Serialize(new MailboxAcknowledgeResponse());
		}

		public static string CreateMessageAcknowledgementServiceErrorText(string errorCode)
		{
			var messageAcknowlegement = new MessageAcknowledgement
			{
				ErrorReference = new CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.ErrorReference
				{
					ErrorCode = errorCode
				}
			};
			IEXmlObjectSerializer.Serialize(new MessageAcknowledgement());
			return IEXmlObjectSerializer.Serialize(messageAcknowlegement);
		}

		#endregion

		#endregion
	}
}
