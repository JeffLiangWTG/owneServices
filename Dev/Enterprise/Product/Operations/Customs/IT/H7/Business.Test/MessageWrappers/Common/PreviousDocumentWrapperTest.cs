using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(PreviousDocumentWrapper))]
sealed class PreviousDocumentWrapperTest : DataProviderTestCase<PreviousDocumentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new PreviousDocumentWrapper(null));
	}

	public void TestLineNo()
	{
		AssertNull(nameof(PreviousDocumentWrapper.LineNo), Provider.LineNo);
	}

	public void TestNumberOfPackages()
	{
		AssertNull(nameof(PreviousDocumentWrapper.NumberOfPackages), Provider.NumberOfPackages);
	}

	public void TestPackageType()
	{
		AssertNull(nameof(PreviousDocumentWrapper.PackageType), Provider.PackageType);
	}

	public void TestQuantity()
	{
		AssertNull(nameof(PreviousDocumentWrapper.Quantity), Provider.Quantity);
	}

	public void TestUnitOfQuantity()
	{
		AssertNull(nameof(PreviousDocumentWrapper.UnitOfQuantity), Provider.UnitOfQuantity);
	}

	public void TestDocumentType()
	{
		AssertEquals(nameof(PreviousDocumentWrapper.DocumentType), "270", Provider.DocumentType);
	}

	public void TestReferenceNumber()
	{
		AssertEquals(nameof(PreviousDocumentWrapper.ReferenceNumber), "123456", Provider.ReferenceNumber);
	}

	protected override PreviousDocumentWrapper GetProvider()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_Code = "270";
		previousDocument.CSI_ReferenceNumber = "123456";

		return new PreviousDocumentWrapper(previousDocument);
	}
}
