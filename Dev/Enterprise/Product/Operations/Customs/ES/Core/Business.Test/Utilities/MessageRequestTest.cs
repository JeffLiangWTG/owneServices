using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class MessageRequestTest : TestCaseWithFactory
	{
		public void TestGetInboxRequestBodyText()
		{
			var expectedOnlyMRNBodyText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Body>
  <MRN>20ES00999910000035</MRN>
  <DeclarantName />
</Body>";

			var expectedCompleteBodyText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Body>
  <MRN>20ES00999910000035</MRN>
  <DeclarantName>Declarant Full Name</DeclarantName>
  <DeclarantID>NIF22222222</DeclarantID>
</Body>";

			var mrnCode = "20ES00999910000035";

			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals("BodyText only contains MRN when declarant is null", expectedOnlyMRNBodyText, MessageRequest.GetInboxRequestBodyText(mrnCode, null));

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				declarant.OH_FullName = "Declarant Full Name";
				declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				AssertMultilineASCIIEquals("BodyText is complete when declaration and declarant are not null", expectedCompleteBodyText, MessageRequest.GetInboxRequestBodyText(mrnCode, declarant));
			});
		}

		public void TestCreateEDIMessageForInboxRequest_EHub()
		{
			var expectedOnlyMRNBodyText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Body>
  <MRN>20ES00999910000035</MRN>
  <DeclarantName />
</Body>";

			var expectedCompleteBodyText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Body>
  <MRN>20ES00999910000035</MRN>
  <DeclarantName>Declarant Full Name</DeclarantName>
  <DeclarantID>NIF22222222</DeclarantID>
</Body>";

			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var certificateName = "CertName";
				var messageType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport;
				var mrnCode = "20ES00999910000035";
				var entryReference = "ES001";

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				MessageRequest.CreateEDIMessageForInboxRequest(Factory, entryHeader, entryHeader.PK, entryHeader.TablePrefix, mrnCode, entryReference, messageType, certificateName);
				Factory.Save();

				AssertNewRequestEDIMessageCreatedForEntry(entryHeader, messageType, ZString.Empty, certificateName, entryReference, expectedOnlyMRNBodyText, ZGuid.Empty);

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				declarant.OH_FullName = "Declarant Full Name";
				declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				var declarantAddress = declarant.MainAddress;
				declarantAddress.OA_Address1 = "Declarant Address";
				declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

				MessageRequest.CreateEDIMessageForInboxRequest(Factory, entryHeader, entryHeader.PK, entryHeader.TablePrefix, mrnCode, "ES001", messageType, certificateName);
				Factory.Save();

				AssertNewRequestEDIMessageCreatedForEntry(entryHeader, messageType, ZString.Empty, certificateName, "ES001", expectedCompleteBodyText, ZGuid.Empty, messageNum: "2");
			}
		}

		public void TestCreateEDIMessageForInboxRequest_GeneralxT_InboxRegistryItemFalse()
		{
			var expectedBodyText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Body>
  <MRN>20ES00999910000035</MRN>
  <DeclarantName>Declarant Full Name</DeclarantName>
  <DeclarantID>NIF22222222</DeclarantID>
</Body>";

			var messageType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport;
			var mrnCode = "20ES00999910000035";
			var entryReference = "ES001";

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			var staff = staffWithCertificateHelperTest.Staff;

			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				
				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				declarant.OH_FullName = "Declarant Full Name";
				declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				declaration.Declarant.OA_OH = declarant.PK;

				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				MessageRequest.CreateEDIMessageForInboxRequest(Factory, entryHeader, entryHeader.PK, entryHeader.TablePrefix, mrnCode, entryReference, messageType, staffWithCertificateHelperTest.Certificate.CertificateName);
				Factory.Save();

				AssertNewRequestEDIMessageCreatedForEntry(entryHeader, messageType, ZString.Empty, staffWithCertificateHelperTest.Certificate.CertificateName, entryReference, expectedBodyText, ZGuid.Empty);
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestCreateEDIMessageForInboxRequest_xT_ExistingTransactions()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var messageType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport;
				var mrnCode = "20ES00999910000035";
				var entryReference = "ES001";
				var oldDate = ZDateTime.Now.AddDays(-10);

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				
				var existingOPNTransaction = AddCusPollingTransaction("OPN", oldDate, messageType, mrnCode);
				var existingWrongAppCodeTransaction = AddCusPollingTransaction("OPN", oldDate, "T2O", mrnCode, applicationCode: "TRC");
				var existingCLSTransaction = AddCusPollingTransaction("CLS", oldDate, messageType, mrnCode);
				var existingWrongTypeTransaction = AddCusPollingTransaction("OPN", oldDate, "NPI", mrnCode);
				var existingWrongIDTransaction = AddCusPollingTransaction("OPN", oldDate, messageType, "AAA");

				Factory.Save();

				MessageRequest.CreateEDIMessageForInboxRequest(Factory, entryHeader, entryHeader.PK, entryHeader.TablePrefix, mrnCode, entryReference, messageType, ZString.Empty);
				Factory.Save();

				CombineAssertions(() =>
				{
					var newTransactions = GetNewOPNCusPollingTransactionsForES();
					AssertEquals("There are 3 OPN ESC transactions", 3, newTransactions.Length);
					AssertCusPollingTransaction("New OPN ESC transaction for messageType and mrn (existing was removed and a new one was added)", newTransactions.FirstOrDefault(x => x.CPT_Type == messageType && x.CPT_TransactionID == mrnCode), "OPN", ZDateTime.Now, messageType, mrnCode, entryHeader: entryHeader);
					AssertCusPollingTransaction("Not ESC transaction (left as is)", existingWrongAppCodeTransaction, "OPN", oldDate, "T2O", mrnCode, applicationCode: "TRC");
					AssertCusPollingTransaction("Not OPN transaction (left as is)", existingCLSTransaction, "CLS", oldDate, messageType, mrnCode);
					AssertCusPollingTransaction("Different type transaction (left as is)", existingWrongTypeTransaction, "OPN", oldDate, "NPI", mrnCode);
					AssertCusPollingTransaction("Different id transaction (left as is)", existingWrongIDTransaction, "OPN", oldDate, messageType, "AAA");
				});
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestCreateEDIMessageForInboxRequest_xT_NoExistingTransactions()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			{
				var messageType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport;
				var mrnCode = "20ES00999910000035";
				var entryReference = "ES001";

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

				Factory.Save();

				MessageRequest.CreateEDIMessageForInboxRequest(Factory, entryHeader, entryHeader.PK, entryHeader.TablePrefix, mrnCode, entryReference, messageType, ZString.Empty);
				Factory.Save();

				CombineAssertions(() =>
				{
					var newTransactions = GetNewOPNCusPollingTransactionsForES();
					AssertEquals("There should only be 1 OPN ESC transaction", 1, newTransactions.Length);
					AssertCusPollingTransaction("OPN ESC transaction", newTransactions[0], "OPN", ZDateTime.Now, messageType, mrnCode, entryHeader: entryHeader);
				});
			}
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestCreateEDIMessageForEffectiveDepCertRequest()
		{
			var expectedBodyText = (string randomSuffix) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}CCCSECV1Ent Id=""ES200109161323{randomSuffix}"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCCSECV1Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}CCCSEC>
    <{XMLTestFileConstants.XmlElementNamespace}messageSender>ESNIF22222222</{XMLTestFileConstants.XmlElementNamespace}messageSender>
    <{XMLTestFileConstants.XmlElementNamespace}messageRecipient>NECA.ES</{XMLTestFileConstants.XmlElementNamespace}messageRecipient>
    <{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>2020-01-09T16:13:23</{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>
    <{XMLTestFileConstants.XmlElementNamespace}messageIdentification>ES001</{XMLTestFileConstants.XmlElementNamespace}messageIdentification>
    <{XMLTestFileConstants.XmlElementNamespace}messageType>CCCSEC</{XMLTestFileConstants.XmlElementNamespace}messageType>
    <{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
      <{XMLTestFileConstants.XmlElementNamespace}MRN>20ES00999910000035</{XMLTestFileConstants.XmlElementNamespace}MRN>
    </{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
  </{XMLTestFileConstants.XmlElementNamespace}CCCSEC>
</{XMLTestFileConstants.XmlElementNamespace}CCCSECV1Ent>
  </soapenv:Body>
</soapenv:Envelope>";

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			var staff = staffWithCertificateHelperTest.Staff;

			using (RegistryTemporarySetterHelper.SetEnableESMessagingThroughDirectxTInterface(true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.ZG_IsTrainingDeclaration = true;
				declaration.JE_GS_NKCusAgent = staff.GS_Code;

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				declarant.OH_FullName = "Declarant Full Name";
				declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				declaration.Declarant.OA_OH = declarant.PK;

				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				entryHeader.MovementReferenceNumberSetter("20ES00999910000035", ZDateTime.Today);

				Factory.Save();
				entryHeader.CH_BGMReference = "ES001";

				MessageRequest.CreateEDIMessageForEffectiveDepCertRequest(Factory, entryHeader, staffWithCertificateHelperTest.Certificate.CertificateName);
				Factory.Save();

				var randomSuffix = TransactionIdHelper.GetRandomSuffix(entryHeader.Messages.LastMessage.EM_MessageText, "ES200109161323", 4);
				AssertNewRequestEDIMessageCreatedForEntry(entryHeader, DeclarationMessageTypeList.Codes.RequestExportExitCertificate, DeclarationMessageSubTypeList.Codes.OriginalDeclaration, staffWithCertificateHelperTest.Certificate.CertificateName, "ES001", expectedBodyText(randomSuffix), staffWithCertificateHelperTest.Certificate.CertificatePK);
			}
		}

		public void TestGetRandomSuffix()
		{
			CombineAssertions("When getting random value added in XMLMessageBuilder TransactionId ", () =>
			{
				AssertGetRandomSuffix("12345", 5);
				AssertGetRandomSuffix("1234", 4);
				AssertGetRandomSuffix("123", 4);
			});
		}

		void AssertGetRandomSuffix(string expectedValue, int suffixLength)
		{
			var messageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}CCCSECV1Ent Id=""ES200109161323{expectedValue}"" xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CCCSECV1Ent.xsd"">
  <{XMLTestFileConstants.XmlElementNamespace}CCCSEC>
    <{XMLTestFileConstants.XmlElementNamespace}messageSender>ESNIF22222222</{XMLTestFileConstants.XmlElementNamespace}messageSender>
    <{XMLTestFileConstants.XmlElementNamespace}messageRecipient>NECA.ES</{XMLTestFileConstants.XmlElementNamespace}messageRecipient>
    <{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>2020-01-09T16:13:23</{XMLTestFileConstants.XmlElementNamespace}preparationDateAndTime>
    <{XMLTestFileConstants.XmlElementNamespace}messageIdentification>ES001</{XMLTestFileConstants.XmlElementNamespace}messageIdentification>
    <{XMLTestFileConstants.XmlElementNamespace}messageType>CCCSEC</{XMLTestFileConstants.XmlElementNamespace}messageType>
    <{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
      <{XMLTestFileConstants.XmlElementNamespace}MRN>20ES00999910000035</{XMLTestFileConstants.XmlElementNamespace}MRN>
    </{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
  </{XMLTestFileConstants.XmlElementNamespace}CCCSEC>
</{XMLTestFileConstants.XmlElementNamespace}CCCSECV1Ent>
  </soapenv:Body>
</soapenv:Envelope>";

			var randomSuffix = TransactionIdHelper.GetRandomSuffix(messageText, "ES200109161323", suffixLength);
			AssertEquals("randomSuffix should be ", expectedValue, randomSuffix);
		}

		void AssertNewRequestEDIMessageCreatedForEntry(CusEntryHeader entryHeader, ZString messageType, ZString messageSubType, ZString certName, ZString boReference, ZString expectedBodyText, ZGuid certPK, string messageNum = "1")
			=> AssertNewRequestEDIMessageCreated((ESEDIMessage)entryHeader.Messages.LastMessage, messageType, messageSubType, certName, boReference, expectedBodyText, certPK, messageNum: messageNum);

		void AssertNewRequestEDIMessageCreated(ESEDIMessage newRequestMessage, ZString messageType, ZString messageSubType, ZString certName, ZString boReference, ZString expectedBodyText, ZGuid certPK, bool isTest = true, string messageNum = "1")
		{
			CombineAssertions(() =>
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, newRequestMessage.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", messageType, newRequestMessage.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", messageSubType, newRequestMessage.EM_MessageSubType);
				AssertEquals("message.EM_IsTestMessage", isTest, newRequestMessage.EM_IsTestMessage);
				AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newRequestMessage.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, newRequestMessage.EM_Status);
				AssertEquals("message.EM_MessageNum", messageNum, newRequestMessage.EM_MessageNum);
				AssertEquals("message.EM_ApplicationReference", certName, newRequestMessage.EM_ApplicationReference);
				AssertEquals("message.BusinessObjectReference", boReference, newRequestMessage.BusinessObjectReference);
				AssertContains("message.EM_MessageText", expectedBodyText, newRequestMessage.EM_MessageText);
				AssertEquals("message.EM_GP", certPK, newRequestMessage.EM_GP);
				AssertNull("message doesn't have interchange", newRequestMessage.Interchange);
			});
		}

		CusPollingTransaction AddCusPollingTransaction(ZString status, ZDateTime createTime, ZString type, ZString transactionID, string applicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage)
		{
			var transaction = Factory.NewWithValidTestData<CusPollingTransaction>();
			transaction.CPT_ApplicationCode = applicationCode;
			transaction.CPT_Status = status;
			transaction.CPT_SystemCreateTimeUtc = createTime;
			transaction.CPT_Type = type;
			transaction.CPT_TransactionID = transactionID;
			transaction.CPT_NumberOfAttempts = 1;
			return transaction;
		}

		CusPollingTransaction[] GetNewOPNCusPollingTransactionsForES()
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			query.AddToFilter(CusPollingTransactionSchema.CPT_Status, Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN);
			return Factory.Load<CusPollingTransaction>(query);
		}

		void AssertCusPollingTransaction(ZString assertMessage, CusPollingTransaction transaction, ZString status, ZDateTime createTime, ZString type, ZString transactionID, string applicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage, CusEntryHeader entryHeader = null)
		{
			AssertEquals(assertMessage + " CPT_ApplicationCode", applicationCode, transaction.CPT_ApplicationCode);
			AssertEquals(assertMessage + " CPT_Status", status, transaction.CPT_Status);
			AssertEquals(assertMessage + " CPT_SystemCreateTimeUtc", createTime, transaction.CPT_SystemCreateTimeUtc);
			AssertEquals(assertMessage + " CPT_Type", type, transaction.CPT_Type);
			AssertEquals(assertMessage + " CPT_TransactionID", transactionID, transaction.CPT_TransactionID);
			if (entryHeader != null)
			{
				AssertEquals(assertMessage + " CPT_ParentID", entryHeader.PK, transaction.CPT_ParentID);
				AssertEquals(assertMessage + " CPT_ParentTableCode", entryHeader.TablePrefix, transaction.CPT_ParentTableCode);
			}
		}
	}
}
