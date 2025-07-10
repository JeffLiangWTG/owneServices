using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ExportSendMessageCommonWrapper))]
	class ExportSendMessageCommonWrapperBaseOnlyTest : ExportSendMessageCommonWrapperAbstractTest<ExportSendMessageCommonWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null CusEntryHeader", () => GetWrapper(null, Certificate));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergedLines in CusEntryHeader", () => GetWrapper(Factory.New<CusEntryHeader>(), Certificate));
				AssertExceptionThrown<ArgumentNullException>("Null Certificate", () => GetWrapper(entryHeader, null));
			});
		}

		public void TestNullDeclarant()
		{
			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = HeaderData.DeclarantCode;
				orgHeader.Addresses.AddNew();
				declaration.Declarant.OA_OH = orgHeader.PK;

				var declarant = wrapper.Declarant;

				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestDeclarantEmailAddress_CustomsClearanceEmailRecipient()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.DeclarantCode;
			orgHeader.Addresses.AddNew();
			declaration.Declarant.OA_OH = orgHeader.PK;

			const string clearanceEmail = "mail1.mail@mail.com";
			const string mailboxEmail = "mail2.mail@mail.com";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(clearanceEmail))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled DeclarationEmail with clearance email recipient when filled", clearanceEmail, wrapper.Declarant.EmailAddress);
			}
		}

		public void TestDeclarantEmailAddress_MailboxEmailAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.DeclarantCode;
			orgHeader.Addresses.AddNew();
			declaration.Declarant.OA_OH = orgHeader.PK;

			const string mailboxEmail = "mail2.mail@mail.com";

			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(mailboxEmail))
			{
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected filled DeclarationEmail with mailbox email address when filled and clearance email recipient is empty", mailboxEmail, wrapper.Declarant.EmailAddress);
			}
		}

		public void TestDeclarantEmailAddress_Empty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.DeclarantCode;
			orgHeader.Addresses.AddNew();
			declaration.Declarant.OA_OH = orgHeader.PK;
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(string.Empty))
			using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
			{
				wrapper = GetWrapper(entryHeader, Certificate);
				AssertEquals("Expected empty DeclarationEmail when clearance email recipient and mailbox email address are empty", ZString.Empty, wrapper.Declarant.EmailAddress);
			}
		}

		public void TestTermsOfDeliveryCode()
		{
			declaration.JE_ShipmentIncoTerm = HeaderData.TermsOfDeliveryCode;
			AssertEquals("Expected filled TermsOfDeliveryCode", HeaderData.TermsOfDeliveryCode, wrapper.TermsOfDeliveryCode);
		}

		public void TestDeliveryLocation()
		{
			declaration.JE_ShipmentIncoTermPlace = HeaderData.DeliveryLocation;
			AssertEquals("Expected filled DeliveryLocation", HeaderData.DeliveryLocation, wrapper.DeliveryLocation);
		}

		public void TestTotalAmount()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_LinePrice = HeaderData.TotalAmount / 2;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = HeaderData.TotalAmount / 2;

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);

				entryHeader = declaration.CustomsEntryHeaders[0];

				AssertEquals("Expected filled TotalAmount", HeaderData.TotalAmount, wrapper.TotalAmount);
			});
		}

		public void TestTotalAmountCurrencyCode()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = HeaderData.TotalAmountCurrency;
			AssertEquals("Expected filled ", HeaderData.TotalAmountCurrency, wrapper.TotalAmountCurrencyCode);
		}

		public void TestTotalNumberOfGoods()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 TotalNumberOfGoods (mandatory at least one)", 1, wrapper.TotalNumberOfGoods);

				entryHeader.MergedLines.AddNew();
				entryHeader.MergedLines.AddNew();
				AssertEquals("Expected 3 TotalNumberOfGoods", 3, wrapper.TotalNumberOfGoods);
			});
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				entryHeader.Messages.AddNew();
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same references", entryHeader.Messages, wrapper.Messages);
			});
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

		public void TestIsTest()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_IsTrainingDeclaration = false;
				AssertEquals("Is declaration a test one?", false, wrapper.IsTest);

				declaration.ZG_IsTrainingDeclaration = true;
				AssertEquals("It is a test declaration", true, wrapper.IsTest);
			});
		}

		public void TestDeclarantIdForUNBSegment()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = HeaderData.DeclarantCode;
				orgHeader.Addresses.AddNew();
				declaration.Declarant.OA_OH = orgHeader.PK;

				AssertEquals("Expected empty Id", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.DeclarantIdForUNBSegment);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id when category is NAT and NIF is declared", "NIF22222222", wrapper.DeclarantIdForUNBSegment);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF Id when category is not NAT and there is no EORI declared", "ESNIF22222222", wrapper.DeclarantIdForUNBSegment);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is declared", "FR22222222", wrapper.DeclarantIdForUNBSegment);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.DeclarantIdForUNBSegment);
			});
		}

		public void TestBusinessObjectReference()
		{
			entryHeader.CH_BGMReference = "Reference";
			AssertEquals("Expected filled BusinessObjectReference", "Reference", wrapper.BusinessObjectReference);
		}

		protected override ZString ExpectedLocalReferenceNumber => LocalReferenceNumberForTest;

		protected override ExportSendMessageCommonWrapper GetWrapper(CusEntryHeader entryheader, ICertificateProvider certificateData) => new ExportSendMessageCommonWrapperForTest(entryheader, certificateData);

		protected override ExportSendMessageCommonWrapper GetProvider() => GetWrapper(entryHeader, Certificate);

		class ExportSendMessageCommonWrapperForTest : ExportSendMessageCommonWrapper
		{
			public ExportSendMessageCommonWrapperForTest(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
			{
			}

			protected override ZString LocalReferenceNumberCore => LocalReferenceNumberForTest;
		}

		const string LocalReferenceNumberForTest = "DummyReferenceNumber";
	}
}
