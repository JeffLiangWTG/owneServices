using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaImportItemDetail))]
sealed class DfiaImportItemDetailTest : Customs.Business.Testing.CusSupportingInfoTest<DfiaImportItemDetail>
{
	public void TestCSI_LineNo_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_LineNoInfo, "Serial Number", "Serial No.", "Sr. No.");
	}

	public void TestCSI_ReferenceNumber_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_ReferenceNumberInfo, "License Import Item Serial Number", "Lic. IMP. Item. Sr. No.", "Lic. Sr. No.");
		AssertEquals("MaxLength", DfiaImportItemDetail.Schema.ReferenceNumberMaxLength, SupportingInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestCSI_Quantity_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_QuantityInfo, "License Import Quantity", "Lic. IMP. Qty.", "Qty.");
		AssertEquals("DecimalPlaces", DfiaImportItemDetail.Schema.QuantityDecimalPlaces, SupportingInfo.CSI_QuantityInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
	}

	public void TestCSI_UnitOfQuantity_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_UnitOfQuantityInfo, "License Import Quantity Unit", "UOM", "UOM");
	}

	public void TestCSI_IssuerType_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_IssuerTypeInfo, "Item Type", "Type", "Type");
		AssertEquals("MaxLength", DfiaImportItemDetail.Schema.IssuerTypeMaxLength, SupportingInfo.CSI_IssuerTypeInfo.MaxLength);
	}

	public void TestLookupsType()
	{
		AssertType<DfiaImportItemDetailLookup>(SupportingInfo.Lookups);
	}

	public void TestValidationType()
	{
		AssertType<DfiaImportItemDetailValidation>(SupportingInfo.Validation);
	}

	public void TestCSI_LineNoSequence()
	{
		var exportItem = Factory.New<JobComInvoiceLine>().DfiaExportItemDetails.AddNew();
		var importItem1 = exportItem.DfiaImportItemDetails.AddNew();
		var importItem2 = exportItem.DfiaImportItemDetails.AddNew();
		var importItem3 = exportItem.DfiaImportItemDetails.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", 1, importItem1.CSI_LineNo);
			AssertEquals("Order 2", 2, importItem2.CSI_LineNo);
			AssertEquals("Order 3", 3, importItem3.CSI_LineNo);

			importItem2.CSI_LineNo = 5;
			AssertEquals("Order 1", 1, importItem1.CSI_LineNo);
			AssertEquals("Order 3 after recalculate", 3, importItem2.CSI_LineNo);
			AssertEquals("Order 2 after recalculate", 2, importItem3.CSI_LineNo);

			exportItem.DfiaImportItemDetails.Remove(importItem2);
			AssertEquals("Order 1 stay same", 1, importItem1.CSI_LineNo);
			AssertEquals("Order 3 change to 2", 2, importItem3.CSI_LineNo);

			importItem1.Delete();
			AssertEquals("Order 2 change to 1", 1, importItem3.CSI_LineNo);
		});
	}

	public void TestISequenceNumberLine()
	{
		CombineAssertions(() =>
		{
			var sequenceLine = (IHugeSequenceNumberLine)SupportingInfo;
			AssertEquals("FKToHeader", SupportingInfo.Parent.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZInt)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestSetDefaultValues()
	{
		AssertEquals("CSI_Code", Constants.CusSupportingInfo.DutyFreeImportAuthorization, SupportingInfo.CSI_Code);
		AssertEquals("CSI_SubType", Constants.CusSupportingInfo.Export, SupportingInfo.CSI_SubType);
	}

	protected override IEnumerable<DfiaImportItemDetail> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewBusinessObject(factory);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObject(Factory);
	}

	DfiaImportItemDetail GetNewBusinessObject(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<JobComInvoiceLine>().DfiaExportItemDetails.AddNew().DfiaImportItemDetails.AddNew();
	}

	void AssertCaptions(ZPropertyInfo info, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(info);
		AssertNotNull("Res String data", resData);
		AssertEquals("Caption", expectedCaption, resData.Caption);
		AssertEquals("MediumCaption", expectedMediumCaption, resData.MediumCaption);
		AssertEquals("ShortCaption", expectedShortCaption, resData.ShortCaption);
	}

	DfiaImportItemDetail SupportingInfo => supportingInfo ??= GetNewBusinessObject(Factory);
	DfiaImportItemDetail supportingInfo;
}
