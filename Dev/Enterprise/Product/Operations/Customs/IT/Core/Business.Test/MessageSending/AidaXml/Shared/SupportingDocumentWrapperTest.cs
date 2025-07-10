using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using CustomsMessageBuilder = CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class SupportingDocumentWrapperTest : TestCaseWithFactory
{
	public void TestContructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new SupportingDocumentWrapper(null));
	}

	public void TestAmount()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.Amount), supportingDocumentWrapper.Amount);

		supportingDocument.CSI_Value = 20;
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Amount), 20m, supportingDocumentWrapper.Amount);
	}

	public void TestCode()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Code), "", supportingDocumentWrapper.Code);

		supportingDocument.CSI_Code = "CODE";
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Code), "CODE", supportingDocumentWrapper.Code);
	}

	public void TestCurrency()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Currency), "", supportingDocumentWrapper.Currency);

		supportingDocument.CSI_RX_NKCurrency = "USD";
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Currency), "USD", supportingDocumentWrapper.Currency);
	}

	public void TestExpiryDate()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.ExpiryDate), supportingDocumentWrapper.ExpiryDate);

		supportingDocument.CSI_DateOfExpiry = new ZDateTime(2022, 06, 21);
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.ExpiryDate), new ZDateTime(2022, 06, 21), supportingDocumentWrapper.ExpiryDate);
	}

	public void TestIssuingAuthority()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.IssuingAuthority), "", supportingDocumentWrapper.IssuingAuthority);

		supportingDocument.CSI_ReferenceNumber2 = "ISSUING AUTH";
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.IssuingAuthority), "ISSUING AUTH", supportingDocumentWrapper.IssuingAuthority);
	}

	public void TestQuantity()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.Quantity), supportingDocumentWrapper.Quantity);

		supportingDocument.CSI_Quantity = 20;
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.Quantity), 20m, supportingDocumentWrapper.Quantity);
	}

	public void TestReferenceNumber()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.ReferenceNumber), "-", supportingDocumentWrapper.ReferenceNumber);

		supportingDocument.CSI_YearOfIssue = "2022";
		supportingDocument.CSI_RN_NKCountryCode = "DE";
		supportingDocument.CSI_ReferenceNumber = "REF NUMBER";
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.ReferenceNumber), "2022-DE-REF NUMBER", supportingDocumentWrapper.ReferenceNumber);
	}

	public void TestUnitOfQuantity()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.UnitOfQuantity), "", supportingDocumentWrapper.UnitOfQuantity);

		supportingDocument.CSI_UnitOfQuantity = "UOM";
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.UnitOfQuantity), "UOM", supportingDocumentWrapper.UnitOfQuantity);
	}

	public void TestItemNumber()
	{
		var supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.ItemNumber), supportingDocumentWrapper.ItemNumber);

		supportingDocument.CSI_LineNo = 0;
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertNull(nameof(CustomsMessageBuilder.ISupportingDocument.ItemNumber), supportingDocumentWrapper.ItemNumber);

		supportingDocument.CSI_LineNo = 5;
		supportingDocumentWrapper = GetNewSupportingDocumentWrapper();
		AssertEquals(nameof(CustomsMessageBuilder.ISupportingDocument.ItemNumber), 5, supportingDocumentWrapper.ItemNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		supportingDocument = declaration.SupportingDocuments.AddNew();
	}

	SupportingDocument supportingDocument;

	CustomsMessageBuilder.ISupportingDocument GetNewSupportingDocumentWrapper() => new SupportingDocumentWrapper(supportingDocument);
}
