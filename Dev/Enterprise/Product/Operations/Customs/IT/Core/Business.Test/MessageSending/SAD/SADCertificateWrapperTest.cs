using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADCertificateWrapperTest : TestCaseWithFactory
{
	public void TestDocumentType()
	{
		supportingDocumentMock.Setup(m => m.Type).Returns("AAA");
		AssertEquals(nameof(wrapper.DocumentType), "AAA", wrapper.DocumentType);
	}

	public void TestCountryOfIssue()
	{
		supportingDocumentMock.Setup(m => m.CountryOfIssue).Returns("XX");
		AssertEquals(nameof(wrapper.CountryOfIssue), "XX", wrapper.CountryOfIssue);
	}

	public void TestIssuingYear()
	{
		supportingDocumentMock.Setup(m => m.YearOfIssue).Returns("2021");
		AssertEquals(nameof(wrapper.IssuingYear), "2021", wrapper.IssuingYear);
	}

	public void TestReference()
	{
		supportingDocumentMock.Setup(m => m.ReferenceNumber).Returns("AABBCC");
		AssertEquals(nameof(wrapper.Reference), "AABBCC", wrapper.Reference);
	}

	public void TestQuantity()
	{
		supportingDocumentMock.Setup(m => m.Quantity).Returns(1m);
		AssertEquals(nameof(wrapper.Quantity), 1m, wrapper.Quantity);

		supportingDocumentMock.Setup(m => m.Quantity).Returns(0m);
		AssertNull($"{nameof(wrapper.Quantity)} when amount is zero", wrapper.Quantity);

		supportingDocumentMock.Setup(m => m.Quantity).Returns(-1m);
		AssertEquals(nameof(wrapper.Quantity), -1m, wrapper.Quantity);
	}

	public void TestUnitOfMeasurement()
	{
		supportingDocumentMock.Setup(m => m.UnitOfQuantity).Returns("KG");
		AssertEquals(nameof(wrapper.UnitOfMeasurement), "KG", wrapper.UnitOfMeasurement);
	}

	public void TestDerogationFlag()
	{
		supportingDocumentMock.Setup(m => m.Status).Returns(SADConstants.CertificateFlag.DER);
		AssertEquals($"{nameof(wrapper.DerogationFlag)} when Status is DER", true, wrapper.DerogationFlag);

		supportingDocumentMock.Setup(m => m.Status).Returns("XXX");
		AssertEquals($"{nameof(wrapper.DerogationFlag)} when Status is not DER", false, wrapper.DerogationFlag);
	}

	public void TestRetrospectiveDerogationFlag()
	{
		supportingDocumentMock.Setup(m => m.Status).Returns(SADConstants.CertificateFlag.PAP);
		AssertEquals($"{nameof(wrapper.RetrospectiveDerogationFlag)} when Status is PAP", true, wrapper.RetrospectiveDerogationFlag);

		supportingDocumentMock.Setup(m => m.Status).Returns("XXX");
		AssertEquals($"{nameof(wrapper.RetrospectiveDerogationFlag)} when Status is not PAP", false, wrapper.RetrospectiveDerogationFlag);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("The supporting document is required", () => { new SADCertificateWrapper(null); });
		AssertNoExceptionThrown(() => new SADCertificateWrapper(supportingDocumentMock.Object));
	}

	protected override void SetUp()
	{
		base.SetUp();
		supportingDocumentMock = new Mock<ISupportingDocument>();
		wrapper = new SADCertificateWrapper(supportingDocumentMock.Object);
	}

	Mock<ISupportingDocument> supportingDocumentMock;
	SADCertificateWrapper wrapper;
}
