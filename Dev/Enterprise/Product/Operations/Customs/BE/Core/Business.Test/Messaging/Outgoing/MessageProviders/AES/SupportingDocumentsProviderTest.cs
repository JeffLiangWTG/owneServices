using System;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class SupportingDocumentsProviderTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentsProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(1, provider.SequenceNumber);
	}

	public void TestType()
	{
		supportingDocument.CSI_Code = "123A";
		AssertEquals("123A", provider.Type);
	}

	public void TestReferenceNumber()
	{
		supportingDocument.CSI_ReferenceNumber = "asd123";
		AssertEquals("asd123", provider.ReferenceNumber);
	}

	public void TestDocumentLineItemNumber()
	{
		supportingDocument.CSI_ItemNumber = 593;
		AssertEquals(593, provider.DocumentLineItemNumber);
	}

	public void TestComplementOfInformation()
	{
		AssertNullOrEmpty(provider.ComplementOfInformation);
	}

	public void TestIssuingAuthorityName()
	{
		supportingDocument.CSI_AdditionalDescription = "AuthorityName";
		AssertEquals("AuthorityName", provider.IssuingAuthorityName);
	}

	public void TestValidityDate()
	{
		supportingDocument.CSI_DateOfExpiry = new CargoWise.Types.ZDateTime(2023, 05, 25);

		CombineAssertions(() =>
		{
			AssertEquals("value", new CargoWise.Types.ZDateTime(2023, 05, 25), provider.ValidityDate);
			AssertEquals("milliseconds", 0, Provider.ValidityDate.Value.Millisecond);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, provider.ValidityDate.Value.Kind);
		});
	}

	public void TestAmount()
	{
		AssertEquals(new decimal(0), provider.Amount);
	}

	public void TestCurrency()
	{
		AssertNullOrEmpty(provider.Currency);
	}

	public void TestMeasurementUnitAndQualifier()
	{
		AssertNullOrEmpty(provider.MeasurementUnitAndQualifier);
	}

	public void TestQuantity()
	{
		AssertEquals(0m, provider.Quantity);
	}

	protected override SupportingDocumentsProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		supportingDocument = declaration.SupportingDocuments.AddNew();
		provider = new SupportingDocumentsProvider(supportingDocument, 1);
	}
	EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument;
	SupportingDocumentsProvider provider;
}
