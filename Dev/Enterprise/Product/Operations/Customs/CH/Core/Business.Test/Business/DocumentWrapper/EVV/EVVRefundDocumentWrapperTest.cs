using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVRefundDocumentWrapper))]
sealed class EVVRefundDocumentWrapperTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>("Null argument", () => EVVRefundDocumentWrapper.New(null, Factory));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		EvvRefundProviderMock.Setup(m => m.DocumentNumber).Returns("DN123");
		EvvRefundProviderMock.Setup(m => m.DocumentVersion).Returns("1");
		EvvRefundProviderMock.Setup(m => m.DocumentTitle).Returns("DT123");
		EvvRefundProviderMock.Setup(m => m.DocumentDateTime).Returns(new DateTime(2024, 9, 26, 23, 55, 0));
		EvvRefundProviderMock.Setup(m => m.CustomsOfficeName).Returns("COname");
		EvvRefundProviderMock.Setup(m => m.CustomsOfficeStreet).Returns("COstreet");
		EvvRefundProviderMock.Setup(m => m.CustomsOfficeCountry).Returns("CO");
		EvvRefundProviderMock.Setup(m => m.CustomsOfficePostalCode).Returns("COpcode");
		EvvRefundProviderMock.Setup(m => m.CustomsOfficeCity).Returns("COcity");
		EvvRefundProviderMock.Setup(m => m.BordereauNumber).Returns("BN123");
		EvvRefundProviderMock.Setup(m => m.PersonInCharge).Returns("PICname");
		EvvRefundProviderMock.Setup(m => m.CustomsReference).Returns("CR123");
		EvvRefundProviderMock.Setup(m => m.TraderReference).Returns("TR123");
		EvvRefundProviderMock.Setup(m => m.VATNumber).Returns("VN123");
		var accountMock = new Mock<IEvvAccount>();
		accountMock.Setup(m => m.Number).Returns("AN123");
		accountMock.Setup(m => m.Name).Returns("AName");
		EvvRefundProviderMock.Setup(m => m.Account).Returns(accountMock.Object);
		EvvRefundProviderMock.Setup(m => m.AccountHolderAddressLines).Returns(new[] { "AHL1", "AHL2", "AHL3" });
		EvvRefundProviderMock.Setup(m => m.TotalAmount).Returns(365.5m);
		EvvRefundProviderMock.Setup(m => m.AmountNotCharged).Returns(true);
		EvvRefundProviderMock.Setup(m => m.DutyAndTaxes).Returns(new IEvvGoodsItemDutyOrTax[] { new Mock<IEvvGoodsItemDutyOrTax>().Object });
		EvvRefundProviderMock.Setup(m => m.LegalAdvisor).Returns(Mock.Of<IEvvLegalAdvisory>(x => x.Title == "Test Title" && x.Text == "Test Text"));

		var wrapper = EVVRefundDocumentWrapper.New(EdiMesssageMock.Object, Factory);

		AssertEquals("DocumentNumber", "DN123", wrapper.DocumentNumber);
		AssertEquals("DocumentVersion", "1", wrapper.DocumentVersion);
		AssertEquals("DocumentTitle", "DT123", wrapper.DocumentTitle);
		AssertEquals("DocumentDateTime", new ZDateTime(2024, 9, 26, 23, 55, 0), wrapper.DocumentDateTime);
		AssertEquals("CustomsOfficeName", "COname", wrapper.CustomsOfficeName);
		AssertEquals("CustomsOfficeStreet", "COstreet", wrapper.CustomsOfficeStreet);
		AssertEquals("CustomsOfficeCountry", "CO", wrapper.CustomsOfficeCountry);
		AssertEquals("CustomsOfficePostalCode", "COpcode", wrapper.CustomsOfficePostalCode);
		AssertEquals("CustomsOfficeCity", "COcity", wrapper.CustomsOfficeCity);
		AssertEquals("BordereauNumber", "BN123", wrapper.BordereauNumber);
		AssertEquals("CustomsOfficer", "PICname", wrapper.CustomsOfficer);
		AssertEquals("CustomsReference", "CR123", wrapper.CustomsReference);
		AssertEquals("TraderReference", "TR123", wrapper.TraderReference);
		AssertEquals("VATNumber", "VN123", wrapper.VATNumber);
		AssertEquals("AccountNumber", "AN123", wrapper.AccountNumber);
		AssertEquals("AccountName", "AName", wrapper.AccountName);
		AssertEquals("AccountHolderAddress1", "AHL1", wrapper.AccountHolderLine1);
		AssertEquals("AccountHolderAddress2", "AHL2", wrapper.AccountHolderLine2);
		AssertEquals("AccountHolderAddress3", "AHL3", wrapper.AccountHolderLine3);
		AssertEquals("TotalAmount", 365.5m, wrapper.TotalAmount);
		AssertEquals("VATChargedTotalAmount", 0m, wrapper.VATChargedTotalAmount);
		AssertEquals("DutyAndTaxes", 1, wrapper.DutyAndTaxes.Count);
		AssertEquals("LegalAdvisor.Title", "Test Title", wrapper.LegalAdvisor.Title);
		AssertEquals("LegalAdvisor.Text", "Test Text", wrapper.LegalAdvisor.Text);
	});

	public void TestVATSuffix() => CombineAssertions(() =>
	{
		AssertVATSuffix(true, "VAT");
		AssertVATSuffix(false, ZString.Empty);

		void AssertVATSuffix(bool messageVATSuffix, string expectedVATSuffix)
		{
			EvvRefundProviderMock.Setup(m => m.VATSuffix).Returns(messageVATSuffix);
			var wrapper = EVVRefundDocumentWrapper.New(EdiMesssageMock.Object, Factory);
			AssertEquals($"Message: VATSuffix={messageVATSuffix}", expectedVATSuffix, wrapper.VATSuffix);
		}
	});

	public void TestDocumentLanguage() => CombineAssertions(() =>
	{
		AssertLanguage(SwissCustomsLanguageList.Codes.German, "DE-DE");
		AssertLanguage(SwissCustomsLanguageList.Codes.French, "FR-FR");
		AssertLanguage(SwissCustomsLanguageList.Codes.Italian, "IT-IT");

		void AssertLanguage(string customsLanguage, ZString expectedDocumentLanguage)
		{
			EvvRefundProviderMock.Setup(x => x.DocumentLanguage).Returns(customsLanguage);
			var wrapper = EVVRefundDocumentWrapper.New(EdiMesssageMock.Object, Factory);
			AssertEquals(expectedDocumentLanguage, wrapper.DocumentLanguage);
		}
	});

	public void TestCorrectionReason()
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory, languages: [
			Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.French),
		]);

		EvvRefundProviderMock.Setup(m => m.CorrectionReason).Returns(RefCusCodeTestHelper.ValidCorrectionReasonTypeCode);
		EvvRefundProviderMock.Setup(m => m.DocumentLanguage).Returns(SwissCustomsLanguageList.Codes.French);
		var wrapper = EVVRefundDocumentWrapper.New(EdiMesssageMock.Object, Factory);
		AssertEquals(nameof(RefCusCodeTestHelper.ValidCorrectionReasonTypeCode) + "FR", wrapper.CorrectionReason);
	}

	public void TestDocumentFilename()
	{
		EvvRefundProviderMock.Setup(m => m.DocumentType).Returns("docType");
		EvvRefundProviderMock.Setup(m => m.DocumentNumber).Returns("docNumber");
		EvvRefundProviderMock.Setup(m => m.DocumentVersion).Returns("docVersion");
		EvvRefundProviderMock.Setup(m => m.RequestorTraderIdentificationNumber).Returns("requestorTIN");
		var wrapper = EVVRefundDocumentWrapper.New(EdiMesssageMock.Object, Factory);
		AssertEquals("e-dec_receiptResponse_receipt_docType_docNumber_docVersion_requestorTIN", wrapper.DocumentFilename);
	}

	Mock<ICHEDIMessage> EdiMesssageMock => MessageMocks.EdiMessageMock;

	Mock<IEvvRefundProvider> EvvRefundProviderMock => MessageMocks.EvvRefundProviderMock;

	(Mock<ICHEDIMessage> EdiMessageMock, Mock<IEvvRefundProvider> EvvRefundProviderMock) MessageMocks => messageMocks ??= CreateMessageMocks();
	(Mock<ICHEDIMessage>, Mock<IEvvRefundProvider>)? messageMocks;

	(Mock<ICHEDIMessage>, Mock<IEvvRefundProvider>) CreateMessageMocks()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var ediMesssageMock = new Mock<ICHEDIMessage>();
		var evvRefundProviderMock = new Mock<IEvvRefundProvider>();
		ediMesssageMock.Setup(m => m.EM_LinkedObject).Returns(entryHeader);
		ediMesssageMock.Setup(m => m.MessageDetail).Returns(evvRefundProviderMock.Object);
		return (ediMesssageMock, evvRefundProviderMock);
	}
}
