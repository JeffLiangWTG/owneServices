using NUnit.Framework;
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class SupportingDocument33YYWrapperTest : TestCase
{
	public void TestAmount()
	{
		var supportingDocument33YYWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.Amount), supportingDocument33YYWrapper.Amount);
	}

	public void TestCode()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Code), "33YY", supportingDocumentWrapper.Code);
	}

	public void TestCurrency()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.Currency), supportingDocumentWrapper.Currency);
	}

	public void TestExpiryDate()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.ExpiryDate), supportingDocumentWrapper.ExpiryDate);
	}

	public void TestIssuingAuthority()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.IssuingAuthority), supportingDocumentWrapper.IssuingAuthority);
	}

	public void TestQuantity()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.Quantity), supportingDocumentWrapper.Quantity);
	}

	public void TestReferenceNumber()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.ReferenceNumber), "-", supportingDocumentWrapper.ReferenceNumber);
	}

	public void TestUnitOfQuantity()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.UnitOfQuantity), supportingDocumentWrapper.UnitOfQuantity);
	}

	CustomsMessageBuilder.ISupportingDocument GetNewSupportingDocumentWrapper() => new SupportingDocument33YYWrapper();
}
