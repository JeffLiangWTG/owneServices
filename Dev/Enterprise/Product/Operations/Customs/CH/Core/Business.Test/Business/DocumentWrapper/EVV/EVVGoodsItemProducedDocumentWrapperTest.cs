using System;
using System.Globalization;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemProducedDocumentWrapper))]
sealed class EVVGoodsItemProducedDocumentWrapperTest : TestCaseWithFactory
{
	public void TestNullConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null detail", () => EVVGoodsItemProducedDocumentWrapper.New(null, Factory));

		var producedDocumentMock = new Mock<IEvvGoodsItemProducedDocument>();
		AssertExceptionThrown<ArgumentNullException>("Null factory", () => EVVGoodsItemProducedDocumentWrapper.New(producedDocumentMock.Object, null));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateSupportingDocumentImportDirectionTypeList(Factory);

		var producedDocumentMock = new Mock<IEvvGoodsItemProducedDocument>();
		producedDocumentMock.Setup(m => m.AdditionalInformation).Returns("Info");
		producedDocumentMock.Setup(m => m.IssueDate).Returns(new DateTime(2024, 11, 18));
		producedDocumentMock.Setup(m => m.DocumentType).Returns("1");
		producedDocumentMock.Setup(m => m.DocumentReferenceNumber).Returns("ABC123");

		var wrapper = EVVGoodsItemProducedDocumentWrapper.New(producedDocumentMock.Object, Factory);

		AssertEquals("AdditionalInformation", "Info", wrapper.AdditionalInformation);
		AssertEquals("IssueDate", new DateTime(2024, 11, 18).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), wrapper.IssueDate);
		AssertEquals("DocumentType", "1 - Description", wrapper.DocumentType);
		AssertEquals("DocumentReferenceNumber", "ABC123", wrapper.DocumentReferenceNumber);

		producedDocumentMock.Setup(m => m.DocumentType).Returns("99");

		AssertEquals("DocumentType - Unknown code", "99", wrapper.DocumentType);
	});

	public void TestEmptyProperties() => CombineAssertions(() =>
	{
		var producedDocumentMock = new Mock<IEvvGoodsItemProducedDocument>();
		producedDocumentMock.Setup(m => m.AdditionalInformation).Returns(string.Empty);
		producedDocumentMock.Setup(m => m.IssueDate).Returns(null as DateTime?);
		producedDocumentMock.Setup(m => m.DocumentType).Returns(string.Empty);
		producedDocumentMock.Setup(m => m.DocumentReferenceNumber).Returns(string.Empty);

		var wrapper = EVVGoodsItemProducedDocumentWrapper.New(producedDocumentMock.Object, Factory);

		AssertEquals("AdditionalInformation", "---", wrapper.AdditionalInformation);
		AssertEquals("IssueDate", "---", wrapper.IssueDate);
		AssertEquals("DocumentType", "---", wrapper.DocumentType);
		AssertEquals("DocumentReferenceNumber", "---", wrapper.DocumentReferenceNumber);
	});
}
