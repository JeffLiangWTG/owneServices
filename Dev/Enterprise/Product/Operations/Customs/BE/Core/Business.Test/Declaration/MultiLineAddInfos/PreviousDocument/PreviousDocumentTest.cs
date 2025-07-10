using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocument))]
sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
{
	public void TestCSI_LineNo_Caption()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(PreviousDocument), nameof(PreviousDocument.CSI_LineNo), false, attribute => attribute.Caption == "Article No.");
	}

	public void TestCSI_ReferenceNumber2_Caption()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(PreviousDocument), nameof(PreviousDocument.CSI_ReferenceNumber2), false, attribute => attribute.Caption == "Bill of Loading");
	}

	public void TestCSI_ItemNumber_Caption()
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(PreviousDocument), nameof(PreviousDocument.CSI_ItemNumber), false, attribute => attribute.Caption == "Item No.");
	}

	public void TestCSI_PackQty_Caption()
	{
		var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<PreviousDocument>().CSI_PackQtyInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Number of Packages", resourceStringDataAttribute.Caption);
			AssertEquals("MediumCaption", "Pack Qty", resourceStringDataAttribute.MediumCaption);
			AssertEquals("ShortCaption", "#Pkgs.", resourceStringDataAttribute.ShortCaption);
		});
	}

	public void TestCSI_PackType_Caption()
	{
		var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<PreviousDocument>().CSI_PackTypeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Type of Packages", resourceStringDataAttribute.Caption);
			AssertEquals("MediumCaption", "Pack Type", resourceStringDataAttribute.MediumCaption);
			AssertEquals("ShortCaption", "Pack Type", resourceStringDataAttribute.ShortCaption);
		});
	}

	public void TestLookupsShouldBeBE()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		AssertType<PreviousDocumentLookups>(previousDocument.Lookups);
	}

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.PreviousDocuments.AddNew();
	}

	public void TestPropertyAttributes()
	{
		var previousDocument = (PreviousDocument)GetNewBusinessObject();
		CombineAssertions(() =>
		{
			AssertEquals("CSI_ItemNumber: Caption", "Item No.", previousDocument.CSI_ItemNumberInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
			AssertEquals("CSI_UnitOfQuantity2: Caption", "Package Code", previousDocument.CSI_UnitOfQuantity2Info.GetAttribute<ResourceStringDataAttribute>().Caption);
			AssertEquals("CSI_UnitOfQuantity2: ShortCaption", "Pkg. Code", previousDocument.CSI_UnitOfQuantity2Info.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
			AssertEquals("CSI_Quantity2: Caption", "Number of Packages", previousDocument.CSI_Quantity2Info.GetAttribute<ResourceStringDataAttribute>().Caption);
			AssertEquals("CSI_Quantity2: ShortCaption", "No. of Pkgs.", previousDocument.CSI_Quantity2Info.GetAttribute<ResourceStringDataAttribute>().ShortCaption);

			AssertEquals("CSI_UnitOfQuantity2: List", "Lookups.PackageCodeList", previousDocument.CSI_UnitOfQuantity2Info.GetAttribute<ListAttribute>().ListDataSourceMember);

			AssertEquals("CSI_Code: MaxLength", 4, previousDocument.CSI_CodeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("CSI_ReferenceNumber: MaxLength", 70, previousDocument.CSI_ReferenceNumberInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("CSI_SubType: MaxLength", 1, previousDocument.CSI_SubTypeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("CSI_ReferenceNumber2: MaxLength", 35, previousDocument.CSI_ReferenceNumber2Info.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("CSI_Description: MaxLength", 35, previousDocument.CSI_DescriptionInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("CSI_UnitOfQuantity2: MaxLength", 2, previousDocument.CSI_UnitOfQuantity2Info.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("CSI_PackQtyMaxLength: MaxLength", 8, previousDocument.CSI_PackQtyInfo.GetAttribute<MaxLengthAttribute>().MaxLength);

			AssertEquals("CSI_Quantity: DecimalPlaces", 15, previousDocument.CSI_QuantityInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
			AssertEquals("CSI_Quantity2: DecimalPlaces", 8, previousDocument.CSI_Quantity2Info.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);

			AssertEquals("CSI_Quantity: DecimalPrecision", 5, previousDocument.CSI_QuantityInfo.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
			AssertEquals("CSI_Quantity2: DecimalPrecision", 0, previousDocument.CSI_Quantity2Info.GetAttribute<DecimalPrecisionAttribute>().DecimalPrecision);
		});
	}

	public void TestJobDeclarationMessageTypeChanged()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var previousDocument = invoiceLine.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			previousDocument.CSI_Quantity2 = new ZDecimal(100);
			previousDocument.CSI_UnitOfQuantity2 = "1A";
			previousDocument.JobDeclarationMessageTypeChanged(JobMessageTypeList.Codes.Export);
			AssertEquals("PreviousDocument CSI_Quantity2 should be 0", ZDecimal.Zero, previousDocument.CSI_Quantity2);
			AssertEquals("PreviousDocument CSI_UnitOfQuantity2 should be empty", ZString.Empty, previousDocument.CSI_UnitOfQuantity2);

			previousDocument.CSI_Quantity2 = new ZDecimal(100);
			previousDocument.CSI_UnitOfQuantity2 = "1A";
			previousDocument.JobDeclarationMessageTypeChanged(JobMessageTypeList.Codes.Import);
			AssertEquals("PreviousDocument CSI_Quantity2 should not be cleared", new ZDecimal(100), previousDocument.CSI_Quantity2);
			AssertEquals("PreviousDocument CSI_UnitOfQuantity2 should not be cleared", "1A", previousDocument.CSI_UnitOfQuantity2);
		});
	}

	public void TestCSI_Quantity()
	{
		var previousDocument = (PreviousDocument)GetNewBusinessObject();
		previousDocument.CSI_Quantity = new ZDecimal(100.666666);
		AssertEquals(new ZDecimal(100.66667), previousDocument.CSI_Quantity);
	}

	public void TestCSI_Quantity2()
	{
		var previousDocument = (PreviousDocument)GetNewBusinessObject();
		previousDocument.CSI_Quantity2 = new ZDecimal(100.6666666);
		AssertEquals(new ZDecimal(101), previousDocument.CSI_Quantity2);
	}
}
