using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ArrivalAtExitSendMessageWrapperTest : WrapperHelperTest<ArrivalAtExitSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null CusExitDetail", () => new ArrivalAtExitSendMessageWrapper(null, Certificate));
				AssertExceptionThrown<ArgumentNullException>("Null Certificate", () => new ArrivalAtExitSendMessageWrapper(exitDetail, null));
			});
		}

		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				exitDetail.CED_MovementReferenceNumber = ExitSummaryData.MRN;
				AssertEquals("Expected filled LocalReferenceNumber", ExitSummaryData.ReferenceNumber, wrapper.LocalReferenceNumber);

				exitDetail.CED_MovementReferenceNumber = ZString.Empty;
				AssertEquals("Expected empty LocalReferenceNumber", ZString.Empty, wrapper.LocalReferenceNumber);
			});
		}

		public void TestCustomsProcedureCategory5()
		{
			exitDetail.CED_MovementReferenceNumber = ExitSummaryData.MRN;
			AssertEquals("Expected filled CustomsProcedureCategory5", ExitSummaryData.MRN, wrapper.CustomsProcedureCategory5);
		}

		public void TestCustomsOfficeofExitCountryCode()
		{
			CombineAssertions(() =>
			{
				exitDetail.CED_CustomsOffice = ExitSummaryData.CustomsOfficeOfExit;
				AssertEquals("Expected filled CustomsOfficeofExitCountryCode", ExitSummaryData.CustomsOfficeOfExitCountry, wrapper.CustomsOfficeofExitCountryCode);

				exitDetail.CED_CustomsOffice = ZString.Empty;
				AssertEquals("Expected empty CustomsOfficeofExitCountryCode", ZString.Empty, wrapper.CustomsOfficeofExitCountryCode);
			});
		}

		public void TestCustomsOfficeofExit()
		{
			CombineAssertions(() =>
			{
				exitDetail.CED_CustomsOffice = ExitSummaryData.CustomsOfficeOfExit;
				AssertEquals("Expected filled CustomsOfficeofExit", ExitSummaryData.CustomsOfficeOfExitCode, wrapper.CustomsOfficeofExit);

				exitDetail.CED_CustomsOffice = ZString.Empty;
				AssertEquals("Expected empty CustomsOfficeofExit", ZString.Empty, wrapper.CustomsOfficeofExit);
			});
		}

		public void TestLocationOfGoodsExamCustomsOffice()
		{
			CombineAssertions(() =>
			{
				exitDetail.CED_ArrivalNotificationPlace = ExitSummaryData.LocationOfGoods;
				AssertEquals("Expected filled LocationOfGoodsExamCustomsOffice", ExitSummaryData.LocationOfGoodsCustomsOffice, wrapper.LocationOfGoodsExamCustomsOffice);

				exitDetail.CED_ArrivalNotificationPlace = ZString.Empty;
				AssertEquals("Expected empty LocationOfGoodsExamCustomsOffice", ZString.Empty, wrapper.LocationOfGoodsExamCustomsOffice);

				exitDetail.CED_ArrivalNotificationPlace = ExitSummaryData.LocationOfGoodsShort;
				AssertEquals("Expected filled LocationOfGoodsExamCustomsOffice", ExitSummaryData.LocationOfGoodsCustomsOffice, wrapper.LocationOfGoodsExamCustomsOffice);
			});
		}

		public void TestLocationOfGoodsExam()
		{
			CombineAssertions(() =>
			{
				exitDetail.CED_ArrivalNotificationPlace = ExitSummaryData.LocationOfGoods;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected filled LocationOfGoodsExam", ExitSummaryData.LocationOfGoodsCode, wrapper.LocationOfGoodsExam);

				exitDetail.CED_ArrivalNotificationPlace = ZString.Empty;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected empty LocationOfGoodsExam", ZString.Empty, wrapper.LocationOfGoodsExam);

				exitDetail.CED_ArrivalNotificationPlace = ExitSummaryData.LocationOfGoodsShort;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected filled LocationOfGoodsExam", ExitSummaryData.LocationOfGoodsCode, wrapper.LocationOfGoodsExam);
			});
		}

		public void TestDateOfArrival()
		{
			ZDateTime.TryParseExact(ExitSummaryData.ArrivalNotifDate, out var notifDate, CustomsDateTimeExtension.DateFormat);
			exitDetail.CED_ArrivalNotificationDate = notifDate.Date;
			AssertEquals("Expected filled DateOfArrival", notifDate, wrapper.DateOfArrival);
		}

		public void TestNullDeclarant()
		{
			CombineAssertions(() =>
			{
				exitHeader.CEH_OA_Agent = ZGuid.Empty;
				AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());

				exitDetail.CED_OA_Carrier = ZGuid.Empty;
				AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());

				exitHeader.CEH_OA_Carrier = ZGuid.Empty;
				AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
			});
		}

		public void TestDeclarant()
		{
			var orgHeaderAgent = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAgent.OH_FullName = "AGENT";

			var orgHeaderCarrier = Factory.New<OrgHeader>();
			orgHeaderCarrier.OH_FullName = "HeaderCarrier";

			var orgHeaderDetailCarrier = Factory.New<OrgHeader>();
			orgHeaderDetailCarrier.OH_FullName = "DetailCarrier";

			const string mailboxEmail = "mail2.mail@mail.com";

			CombineAssertions(() =>
			{
				exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;

				var declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertEquals("Expected Declarant filled with Agent's data", "AGENT", declarant.Name);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);

				exitHeader.CEH_OA_Agent = ZGuid.Empty;
				exitDetail.CED_OA_Carrier = orgHeaderDetailCarrier.MainAddress.PK;

				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertEquals("Expected Declarant filled with detail Carrier's data", "DetailCarrier", declarant.Name);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);

				exitHeader.CEH_OA_Agent = ZGuid.Empty;
				exitDetail.CED_OA_Carrier = ZGuid.Empty;
				exitHeader.CEH_OA_Carrier = orgHeaderCarrier.MainAddress.PK;

				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertEquals("Expected Declarant filled with header Carrier's data", "HeaderCarrier", declarant.Name);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);

				exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;
				exitDetail.CED_OA_Carrier = orgHeaderDetailCarrier.MainAddress.PK;
				exitHeader.CEH_OA_Carrier = orgHeaderCarrier.MainAddress.PK;

				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertEquals("Expected Declarant filled with Agent's data when agent and detail carrier and header carrier are declared", "AGENT", declarant.Name);
				AssertEquals("Expected Declarant's NameCode is always empty", ZString.Empty, declarant.NameCode);
				AssertEquals("Expected Declarant's PartyQualifier is always 1", "1", declarant.PartyQualifier);

				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					exitHeader = Factory.New<CusExitControlHeader>();
					exitDetail = exitHeader.CusExitDetails.AddNew();
					exitHeader.CEH_OA_Agent = ZGuid.Empty;
					exitDetail.CED_OA_Carrier = orgHeaderDetailCarrier.MainAddress.PK;
					exitHeader.CEH_OA_Carrier = orgHeaderCarrier.MainAddress.PK;

					wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
					declarant = wrapper.Declarant;

					AssertNotNull("Expected filled Declarant", declarant);
					AssertEquals("Expected Declarant filled with detail Carrier's data when agent is not declared and both detail carrier and header carrier are declared", "DetailCarrier", declarant.Name);
					AssertEquals("Expected Declarant's NameCode is always empty", ZString.Empty, declarant.NameCode);
					AssertEquals("Expected Declarant's PartyQualifier is always 1", "1", declarant.PartyQualifier);
					AssertEquals("Expected Declarant's EmailAddress empty when no email is declared in the registry", ZString.Empty, declarant.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					exitHeader = Factory.New<CusExitControlHeader>();
					exitDetail = exitHeader.CusExitDetails.AddNew();
					exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;
					wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
					declarant = wrapper.Declarant;

					AssertNotNull("Expected filled Declarant", declarant);
					AssertEquals("Expected MailboxEmailAddress when declared in registry and CustomsClearanceEmailRecipient is empty", mailboxEmail, declarant.EmailAddress);
				}
			});
		}
		public void TestDeclarantEmailAddress()
		{
			var orgHeaderAgent = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAgent.OH_FullName = "AGENT";
			exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;

			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
					AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, wrapper.Declarant.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
				{
					exitHeader = Factory.New<CusExitControlHeader>();
					exitDetail = exitHeader.CusExitDetails.AddNew();
					exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;
					wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
					AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.Declarant.EmailAddress);
				}

				using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
				using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
				{
					exitHeader = Factory.New<CusExitControlHeader>();
					exitDetail = exitHeader.CusExitDetails.AddNew();
					exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;
					wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
					AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.Declarant.EmailAddress);
				}
			});
		}

		public void TestDeclarantIdForUNBSegment()
		{
			var orgHeaderAgent = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAgent.OH_FullName = ExitSummaryData.AgentCode;

			exitHeader.CEH_OA_Agent = ZGuid.Empty;
			exitHeader.CEH_OA_Carrier = ZGuid.Empty;

			AssertEquals("Expected empty Id when neither angent nor carrier are declared", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

			CombineAssertions("For Agent declared", () =>
			{
				exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected empty Id", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				orgHeaderAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.DeclarantIdForUNBSegment);

				orgHeaderAgent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderAgent.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected NIF Id when category is NAT and NIF is declared", "NIF22222222", wrapper.DeclarantIdForUNBSegment);

				orgHeaderAgent.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderAgent.OH_Category = OrgConstants.Category.Government;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected Country+NIF Id when category is not NAT and there is no EORI declared", "ESNIF22222222", wrapper.DeclarantIdForUNBSegment);

				OrgCusCode eoriCusCode = orgHeaderAgent.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is declared", "FR22222222", wrapper.DeclarantIdForUNBSegment);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.DeclarantIdForUNBSegment);
			});

			var orgHeaderCarrier = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderCarrier.OH_FullName = ExitSummaryData.CarrierCode;

			CombineAssertions("For Carrier declared", () =>
			{
				exitHeader.CEH_OA_Agent = ZGuid.Empty;
				exitHeader.CEH_OA_Carrier = orgHeaderCarrier.MainAddress.PK;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected empty Id", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				orgHeaderCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.DeclarantIdForUNBSegment);

				orgHeaderCarrier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderCarrier.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected NIF Id when category is NAT and NIF is declared", "NIF22222222", wrapper.DeclarantIdForUNBSegment);

				orgHeaderCarrier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderCarrier.OH_Category = OrgConstants.Category.Government;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected Country+NIF Id when category is not NAT and there is no EORI declared", "ESNIF22222222", wrapper.DeclarantIdForUNBSegment);

				OrgCusCode eoriCusCode = orgHeaderCarrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is declared", "FR22222222", wrapper.DeclarantIdForUNBSegment);

				eoriCusCode.OK_CustomsRegNo = "DE22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "DE";
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected EORI Id with country code not repeated", "DE22222222", wrapper.DeclarantIdForUNBSegment);
			});

			CombineAssertions("For both Agent and Carrier declared", () =>
			{
				exitHeader.CEH_OA_Agent = orgHeaderAgent.MainAddress.PK;
				exitHeader.CEH_OA_Carrier = orgHeaderCarrier.MainAddress.PK;
				wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
				AssertEquals("Expected EORI Id with country code not repeated for Agent when both agent and carrier are declared with id", "ES22222222", wrapper.DeclarantIdForUNBSegment);
			});
		}

		public void TestIsTest()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				AssertEquals("Is declaration a test one? (when no parent)", false, wrapper.IsTest);

				exitHeader.CEH_Parent = declaration;

				declaration.ZG_IsTrainingDeclaration = true;
				AssertEquals("It is a test declaration", true, wrapper.IsTest);

				declaration.ZG_IsTrainingDeclaration = false;
				AssertEquals("Is declaration a test one?", false, wrapper.IsTest);
			});
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				exitDetail.Messages.AddNew();
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same references", exitDetail.Messages, wrapper.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.Factory);
				AssertSame("Expected same references", exitDetail.Factory, wrapper.Factory);
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

		public void TestCertificateID()
		{
			AssertEquals("Expected filled CertificateID", Certificate.CertificateID, wrapper.CertificateID);
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

		public void TestBusinessObjectReference()
		{
			exitDetail.CED_MovementReferenceNumber = "11ES00113112683757";
			AssertEquals("Expected filled BusinessObjectReference", "11ES00113112683757", wrapper.BusinessObjectReference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();

			wrapper = new ArrivalAtExitSendMessageWrapper(exitDetail, Certificate);
		}

		CusExitDetail exitDetail;
		CusExitControlHeader exitHeader;
		ArrivalAtExitSendMessageWrapper wrapper;

		protected override ArrivalAtExitSendMessageWrapper GetProvider() => wrapper;
	}
}
