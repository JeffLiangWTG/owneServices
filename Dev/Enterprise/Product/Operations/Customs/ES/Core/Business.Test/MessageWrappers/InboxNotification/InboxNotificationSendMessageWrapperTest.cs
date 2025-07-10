using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationSendMessageWrapperTest : WrapperHelperTest<InboxNotificationSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Empty messageType", () => GetWrapper(ZString.Empty));
				AssertExceptionThrown<ArgumentNullException>("Null Certificate", () => new InboxNotificationSendMessageWrapper(Factory, declaration?.Declarant?.Header, declaration.ZG_IsTrainingDeclaration, "AA", null));
			});
		}

		public void TestResponseType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected ResponseType for NPI messageType", InboxNotificationResponseTypes.Import, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.InBoxNotificationForExport);
				AssertEquals("Expected ResponseType for NPE messageType", InboxNotificationResponseTypes.Export, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication);
				AssertEquals("Expected ResponseType for INE messageType", InboxNotificationResponseTypes.AESInvalidation, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportClearanceCommunication);
				AssertEquals("Expected ResponseType for LVE messageType", InboxNotificationResponseTypes.AESClearance, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication);
				AssertEquals("Expected ResponseType for DIE messageType", InboxNotificationResponseTypes.AESNonConformity, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportCceControlCommunication);
				AssertEquals("Expected ResponseType for CCE messageType", InboxNotificationResponseTypes.AESCceControl, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportExitResultCommunication);
				AssertEquals("Expected ResponseType for RES messageType", InboxNotificationResponseTypes.AESExitResult, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification);
				AssertEquals("Expected ResponseType for LVS messageType", InboxNotificationResponseTypes.AESExitClearance, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification);
				AssertEquals("Expected ResponseType for DIS messageType", InboxNotificationResponseTypes.AESExitNonConformity, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture);
				AssertEquals("Expected ResponseType for DIT messageType", InboxNotificationResponseTypes.NCTSNonConformity, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls);
				AssertEquals("Expected ResponseType for CCT messageType", InboxNotificationResponseTypes.NCTSCceControl, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance);
				AssertEquals("Expected ResponseType for LVT messageType", InboxNotificationResponseTypes.NCTSClearance, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation);
				AssertEquals("Expected ResponseType for INT messageType", InboxNotificationResponseTypes.NCTSInvalidation, wrapper.ResponseType);

				wrapper = GetWrapper(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2);
				AssertEquals("Expected ResponseType for NPD messageType", InboxNotificationResponseTypes.DVD, wrapper.ResponseType);
			});
		}

		public void TestDeclarantName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected correct DeclarantName", "Declarant Full Name", wrapper.DeclarantName);

				declaration.Declarant.OA_OH = ZGuid.Empty;
				wrapper = GetWrapper();
				AssertEquals("Expected empty DeclarantName when not declared", ZString.Empty, wrapper.DeclarantName);
			});
		}

		public void TestCertificateID()
		{
			AssertEquals("Expected filled CertificateID", Certificate.CertificateID, wrapper.CertificateID);
		}

		public void TestDeclarantID()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected correct DeclarantID", "NIF22222222", wrapper.DeclarantID);

				declaration.Declarant.OA_OH = ZGuid.Empty;
				wrapper = GetWrapper();
				AssertEquals("Expected empty DeclarantID when not declared", ZString.Empty, wrapper.DeclarantID);
			});
		}

		public void TestIsTest()
		{
			CombineAssertions(() =>
			{
				wrapper = GetWrapper(isTest: false);
				AssertEquals("Is declaration a test one?", false, wrapper.IsTest);

				wrapper = GetWrapper(isTest: true);
				AssertEquals("It is a test declaration", true, wrapper.IsTest);
			});
		}

		public void TestBusinessObjectReference()
		{
			entryHeader.CH_BGMReference = "Reference";
			AssertEquals("Expected empty BusinessObjectReference", ZString.Empty, wrapper.BusinessObjectReference);
		}

		public void TestMessages()
		{
			AssertNull("Expected null messages", wrapper.Messages);
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.Factory);
				AssertSame("Expected same references", entryHeader.Factory, wrapper.Factory);
			});
		}

		public void TestBrokerCode()
		{
			AssertEquals("Expected filled BrokerCode", Certificate.BrokerCode, wrapper.BrokerCode);
		}

		public void TestCertificateName()
		{
			AssertEquals("Expected filled CertificateName", Certificate.CertificateName, wrapper.CertificateName);
		}

		public void TestCertificateThumbPrint()
		{
			AssertEquals("Expected filled CertificateThumbPrint", Certificate.CertificateThumbPrint, wrapper.CertificateThumbPrint);
		}

		public void TestCertificateBytes()
		{
			AssertEquals("Expected filled CertificateBytes", Certificate.CertificateBytes, wrapper.CertificateBytes);
		}

		public void TestDecryptedCertificatePassphrase()
		{
			AssertEquals("Expected filled DecryptedCertificatePassphrase", Certificate.DecryptedCertificatePassphrase, wrapper.DecryptedCertificatePassphrase);
		}

		public void TestCertificatePK()
		{
			AssertEquals("Expected filled CertificatePK", Certificate.CertificatePK, wrapper.CertificatePK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = "Declarant Full Name";
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			declaration.Declarant.OA_OH = declarant.PK;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			wrapper = GetWrapper();
		}

		CusEntryHeader entryHeader;
		JobDeclaration declaration;
		InboxNotificationSendMessageWrapper wrapper;

		InboxNotificationSendMessageWrapper GetWrapper(string declarationType = DeclarationMessageTypeList.Codes.InBoxNotificationForImport, bool isTest = false) => new InboxNotificationSendMessageWrapper(Factory, declaration?.Declarant?.Header, isTest, declarationType, Certificate);

		protected override InboxNotificationSendMessageWrapper GetProvider() => wrapper;
	}
}
