using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : CusSupportingInfoTest<SupportingDocument>
{
	public void TestCSI_Status()
	{
		var doc = (SupportingDocument)GetNewBusinessObject();

		doc.ArchiveLocationIndicator = "1";
		AssertEquals("1", doc.ArchiveLocationIndicator);
		AssertEquals("1?", doc.CSI_Status);

		doc.ArchiveSupport = "0";
		AssertEquals("0", doc.ArchiveSupport);
		AssertEquals("10", doc.CSI_Status);

		doc.ArchiveLocationIndicator = "";
		AssertEquals("0", doc.ArchiveSupport);
		AssertEquals("?0", doc.CSI_Status);
		doc.ArchiveLocationIndicator = " ";
		AssertEquals("?0", doc.CSI_Status);

		doc.CSI_Status = "??";
		AssertEquals("", doc.ArchiveLocationIndicator);
		AssertEquals("", doc.ArchiveSupport);

		doc.CSI_Status = "01";
		AssertEquals("0", doc.ArchiveLocationIndicator);
		AssertEquals("1", doc.ArchiveSupport);
	}

	public void TestMaxLengths()
	{
		var doc = (SupportingDocument)GetNewBusinessObject();
		AssertEquals(4, doc.CSI_CodeInfo.MaxLength);
		AssertEquals(35, doc.CSI_ReferenceNumberInfo.MaxLength);
		AssertEquals(4, doc.CSI_UnitOfQuantityInfo.MaxLength);
		AssertEquals(1, doc.ArchiveLocationIndicatorInfo.MaxLength);
		AssertEquals(1, doc.ArchiveSupportInfo.MaxLength);
		AssertEquals(2, doc.CSI_StatusInfo.MaxLength);
		AssertEquals(70, doc.CSI_DescriptionInfo.MaxLength);
		AssertEquals(70, doc.CSI_AdditionalDescriptionInfo.MaxLength);
		AssertEquals(4, doc.CSI_SubTypeInfo.MaxLength);
		AssertEquals(17, doc.CSI_ReferenceNumber2Info.MaxLength);
		AssertEquals(35, doc.ValidationOfficeInfo.MaxLength);
	}

	public void TestCSI_Quantity()
	{
		var doc = (SupportingDocument)GetNewBusinessObject();
		doc.CSI_Quantity = new ZDecimal(123456789012345.123);
		AssertEquals(new ZDecimal(123456789012345.123), doc.CSI_Quantity);

		doc.CSI_Quantity = new ZDecimal(1.1235);
		AssertEquals(new ZDecimal(1.124), doc.CSI_Quantity);

		doc.CSI_Quantity = new ZDecimal(1.1234);
		AssertEquals(new ZDecimal(1.123), doc.CSI_Quantity);
	}

	public void TestValidationOffice()
	{
		var doc = (SupportingDocument)GetNewBusinessObject();
		doc.ValidationOffice = "ValidationOfficeTest";
		AssertEquals("ValidationOfficeTest", doc.ValidationOffice);
	}

	public void TestLookupsType()
	{
		AssertType(typeof(SupportingDocumentLookups), ((SupportingDocument)GetNewBusinessObject()).Lookups);
	}

	public void TestCSI_ItemNumber_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(((SupportingDocument)GetNewBusinessObject()).CSI_ItemNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Document Line Item Number", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Doc. Item Num.", resourceStringData.ShortCaption);
		});
	}

	public void TestCSI_ItemNumber_Caption_CaptionKeyExportUCC6()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(((SupportingDocument)GetNewBusinessObject()).CSI_ItemNumberInfo, EU.Business.Declaration.JobDeclaration.CaptionKeyExportUCC6);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Document Line Item Number", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Doc. Item Num.", resourceStringData.ShortCaption);
		});
	}

	public void TestCSI_Value_Caption()
	{
		AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(((SupportingDocument)GetNewBusinessObject()).CSI_ValueInfo).Caption);
	}

	public void TestCSI_AdditionalDescription_Caption()
	{
		AssertEquals("Issuing Authority Name", DataBoundResourceStrings.GetDataForProperty(((SupportingDocument)GetNewBusinessObject()).CSI_AdditionalDescriptionInfo).Caption);
	}

	public void TestCSI_DateOfExpiry_Caption()
	{
		AssertEquals("Date of Validity", DataBoundResourceStrings.GetDataForProperty(((SupportingDocument)GetNewBusinessObject()).CSI_DateOfExpiryInfo).Caption);
	}

	public void TestCSI_UnitOfQuantity()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		AssertEquals("Lookups.UnitOfQuantityList", supportingDocument.CSI_UnitOfQuantityInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	#region Implementation
	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SupportingDocuments.AddNew();
	}
	#endregion
}
