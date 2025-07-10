using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(SupportingDocumentWrapper))]
sealed class SupportingDocumentWrapperTest : DataProviderTestCase<SupportingDocumentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new SupportingDocumentWrapper(null));
	}

	public void TestCode()
	{
		AssertEquals(nameof(SupportingDocumentWrapper.Code), "YYY", Provider.Code);
	}

	public void TestReferenceNumber()
	{
		AssertEquals(nameof(SupportingDocumentWrapper.ReferenceNumber), "123456", Provider.ReferenceNumber);
	}

	public void TestAmount()
	{
		AssertNull(nameof(SupportingDocumentWrapper.Amount), Provider.Amount);
	}

	public void TestCurrency()
	{
		AssertNull(nameof(SupportingDocumentWrapper.Currency), Provider.Currency);
	}

	public void TestExpiryDate()
	{
		AssertNull(nameof(SupportingDocumentWrapper.ExpiryDate), Provider.ExpiryDate);
	}

	public void TestQuantity()
	{
		AssertNull(nameof(SupportingDocumentWrapper.Quantity), Provider.Quantity);
	}

	public void TestIssuingAuthority()
	{
		AssertNull(nameof(SupportingDocumentWrapper.IssuingAuthority), Provider.IssuingAuthority);
	}

	public void TestUnitOfQuantity()
	{
		AssertNull(nameof(SupportingDocumentWrapper.UnitOfQuantity), Provider.UnitOfQuantity);
	}

	public void TestItemNumber()
	{
		AssertNull(nameof(SupportingDocumentWrapper.ItemNumber), Provider.ItemNumber);
	}

	protected override SupportingDocumentWrapper GetProvider()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_ReferenceNumber = "123456";

		return new SupportingDocumentWrapper(supportingDocument);
	}
}
