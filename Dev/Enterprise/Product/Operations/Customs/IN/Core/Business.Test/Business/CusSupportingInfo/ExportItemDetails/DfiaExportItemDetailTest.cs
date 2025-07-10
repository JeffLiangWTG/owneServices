using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaExportItemDetail))]
sealed class DfiaExportItemDetailTest : Customs.Business.Testing.CusSupportingInfoTest<DfiaExportItemDetail>
{
	public void TestCSI_LineNo_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_LineNoInfo, "Serial Number", "Serial No.", "Sr. No.");
	}

	public void TestCSI_ReferenceNumber_Attributes()
	{
		AssertCaptions(SupportingInfo.CSI_ReferenceNumberInfo, "License Number", "Lic. No.", "Lic.");
		AssertEquals("MaxLength", DfiaExportItemDetail.Schema.ReferenceNumberMaxLength, SupportingInfo.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestCSI_DateOfIssue()
	{
		AssertCaptions(SupportingInfo.CSI_DateOfIssueInfo, "License Date", "Date", "Date");
	}

	public void TestCSI_ReferenceNumber2()
	{
		AssertCaptions(SupportingInfo.CSI_ReferenceNumber2Info, "License Export Item Serial Number", "Lic. EXP. Item. Sr. No.", "Lic. Sr. No.");
		AssertEquals("MaxLength", DfiaExportItemDetail.Schema.ReferenceNumber2MaxLength, SupportingInfo.CSI_ReferenceNumber2Info.MaxLength);
	}

	public void TestCSI_Quantity()
	{
		AssertCaptions(SupportingInfo.CSI_QuantityInfo, "License Export Quantity", "Lic. Exp. Qty.", "Qty.");
		AssertEquals("DecimalPlaces", DfiaExportItemDetail.Schema.QuantityDecimalPlaces, SupportingInfo.CSI_QuantityInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
	}

	public void TestCSI_UnitOfQuantity()
	{
		AssertCaptions(SupportingInfo.CSI_UnitOfQuantityInfo, "License Export Quantity Unit", "UOM", "UOM");
	}

	public void TestLookupsType()
	{
		AssertType<DfiaExportItemDetailLookup>(SupportingInfo.Lookups);
	}

	public void TestValidationType()
	{
		AssertType<DfiaExportItemDetailValidation>(SupportingInfo.Validation);
	}

	public void TestDfiaImportItemDetailsCollection()
	{
		var collection = SupportingInfo.DfiaImportItemDetails;
		AssertType<DfiaImportItemDetailCollection>(collection);
	}

	public void TestSetDefaultValues()
	{
		AssertEquals("CSI_Code", Constants.CusSupportingInfo.DutyFreeImportAuthorization, SupportingInfo.CSI_Code);
		AssertEquals("CSI_SubType", Constants.CusSupportingInfo.Export, SupportingInfo.CSI_SubType);
	}

	public void TestCSI_LineNoSequence()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var exportItem1 = invoiceLine.DfiaExportItemDetails.AddNew();
		var exportItem2 = invoiceLine.DfiaExportItemDetails.AddNew();
		var exportItem3 = invoiceLine.DfiaExportItemDetails.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", 1, exportItem1.CSI_LineNo);
			AssertEquals("Order 2", 2, exportItem2.CSI_LineNo);
			AssertEquals("Order 3", 3, exportItem3.CSI_LineNo);

			exportItem2.CSI_LineNo = 5;
			AssertEquals("Order 1", 1, exportItem1.CSI_LineNo);
			AssertEquals("Order 3 after recalculate", 3, exportItem2.CSI_LineNo);
			AssertEquals("Order 2 after recalculate", 2, exportItem3.CSI_LineNo);

			invoiceLine.DfiaExportItemDetails.Remove(exportItem2);
			AssertEquals("Order 1 stay same", 1, exportItem1.CSI_LineNo);
			AssertEquals("Order 3 change to 2", 2, exportItem3.CSI_LineNo);

			exportItem1.Delete();
			AssertEquals("Order 2 change to 1", 1, exportItem3.CSI_LineNo);
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

	public void TestDfiaImportItemDetailsCollectionType()
	{
		AssertType<DfiaImportItemDetailCollection>(SupportingInfo.DfiaImportItemDetails);
	}

	public void TestDfiaImportItemDetailsLineNumberGeneratorType()
	{
		AssertType<HugeSequenceNumberGenerator>(SupportingInfo.DfiaImportItemDetailsLineNumberGenerator);
	}

	public void TestICusSupportingInfoTypeSupporter()
	{
		Integration.Customs.ICusSupportingInfoTypeSupporter supporter = SupportingInfo;
		var supportingInfoTypes = supporter.GetCusSupportingInfoTypes();
		AssertEquals(typeof(DfiaImportItemDetail), supportingInfoTypes[CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization]);

		AssertType<CusSupportingInfoTypeSupporterFetchStrategy>(supporter.GetFetchStrategies().First());
	}

	protected override IEnumerable<DfiaExportItemDetail> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewBusinessObject(factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetNewBusinessObject(factory);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObject(Factory);
	}

	void AssertCaptions(ZPropertyInfo info, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(info);
		AssertNotNull("Res String data", resData);
		AssertEquals("Caption", expectedCaption, resData.Caption);
		AssertEquals("MediumCaption", expectedMediumCaption, resData.MediumCaption);
		AssertEquals("ShortCaption", expectedShortCaption, resData.ShortCaption);
	}

	DfiaExportItemDetail GetNewBusinessObject(BusinessObjectFactory factory)
	{
		return factory.NewWithValidTestData<JobComInvoiceLine>().DfiaExportItemDetails.AddNew();
	}

	DfiaExportItemDetail SupportingInfo => supportingInfo ??= GetNewBusinessObject(Factory);
	DfiaExportItemDetail supportingInfo;
}
