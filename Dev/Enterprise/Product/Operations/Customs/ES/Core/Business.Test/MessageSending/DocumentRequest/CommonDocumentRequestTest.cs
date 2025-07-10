using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class CommonDocumentRequestTest<T, BO> : TestCaseWithFactory
		where T : CommonDocumentRequest<BO>
		where BO : BusinessObject
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("entryHeader null", () => GetDocumentRequestClass(null, "A"));
			AssertExceptionThrown<ArgumentException>("CertName null", () => GetDocumentRequestClass(businessObject, null));
			AssertExceptionThrown<ArgumentException>("certName empty", () => GetDocumentRequestClass(businessObject, ZString.Empty));
		}

		protected BO businessObject;

		protected abstract T GetDocumentRequestClass(BO businessObject, ZString certName);

		protected ZString mrnCode => "20ES00999830001277";

		protected void AssertDocumentRequestEDIMessage(BO businessObject, ZString filename, ZString urlParameter, ZGuid certificatePK)
		{
			var expectedMessageText = ZString.Format(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<{XMLTestFileConstants.XmlElementNamespace}ESCustoms xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""http://www.wisetechglobal.com/eServices/Schemas/ESCustoms/DocumentRequest"">
  <{XMLTestFileConstants.XmlElementNamespace}File>
    <{XMLTestFileConstants.XmlElementNamespace}name>{filename}</{XMLTestFileConstants.XmlElementNamespace}name>
    <{XMLTestFileConstants.XmlElementNamespace}url_parameter>{urlParameter}</{XMLTestFileConstants.XmlElementNamespace}url_parameter>
  </{XMLTestFileConstants.XmlElementNamespace}File>
</{XMLTestFileConstants.XmlElementNamespace}ESCustoms>", filename, urlParameter);

			var newRequestMessage = GetLastMessage(businessObject);

			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, newRequestMessage.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", DeclarationMessageTypeList.Codes.EsDocumentRequest, newRequestMessage.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", DeclarationMessageSubTypeList.Codes.EsDocumentRequest, newRequestMessage.EM_MessageSubType);
			AssertEquals("message.EM_IsTestMessage", IsTrain(), newRequestMessage.EM_IsTestMessage);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newRequestMessage.EM_ReceiveTransmit);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, newRequestMessage.EM_Status);
			AssertEquals("message.EM_ApplicationReference", certificate.CertificateName, newRequestMessage.EM_ApplicationReference);
			AssertEquals("message.EM_MessageText", expectedMessageText, newRequestMessage.EM_MessageText);
			AssertEquals("message.EM_GP", certificatePK, newRequestMessage.EM_GP);
			AssertNull("message doesn't have interchange", newRequestMessage.Interchange);
		}

		protected void AssertNoDuplicatedDocumentRequests(ZString filename, ZString urlParameter, ZGuid certificatePK)
		{
			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("No New message created for status QUE", 0, documentRequestMessage);

			var lastMessage = GetLastMessage(businessObject);
			lastMessage.EM_Status = "RCV";
			lastMessage.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("No New message created for status RCV", 0, documentRequestMessage);

			lastMessage.EM_Status = "SNT";
			lastMessage.Factory.Save();

			Thread.Sleep(5);
			documentRequestMessage = RequestMissingDocument();
			AssertEquals("New Message created successfully when message status SNT", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, filename, urlParameter, certificatePK);

			lastMessage = GetLastMessage(businessObject);
			lastMessage.EM_Status = "HQU";
			lastMessage.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("No New message created for status HQU", 0, documentRequestMessage);

			lastMessage.EM_Status = "FAL";
			lastMessage.Factory.Save();

			Thread.Sleep(5);
			documentRequestMessage = RequestMissingDocument();
			AssertEquals("New Message created successfully when message status FAL", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, filename, urlParameter, certificatePK);
		}

		protected abstract EDIMessage GetLastMessage(BO businessObject);

		protected abstract ZBool IsTrain();

		protected void CreateOrUpdateCusEntryNumber(BO businessObject, ZString entryType, ZString entryNum, ZString entryStatus, ZDateTime issueDate, ZDateTime expiryDate)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(businessObject, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			newEntryNumber.CE_EntryStatus = entryStatus;
			newEntryNumber.CE_IssueDate = issueDate;
			newEntryNumber.CE_ExpiryDate = expiryDate;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}

		protected ZInt RequestMissingDocument()
		{
			var documentRequest = GetDocumentRequestClass(businessObject, certificate.CertificateName);
			var documentRequestMessage = documentRequest.RequestMissingDocuments();

			return documentRequestMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;
			certificate = staffWithCertificateHelperTest.Certificate;
		}

		protected CertificateProviderTestClass certificate;
		protected GlbStaff staff;
	}
}
