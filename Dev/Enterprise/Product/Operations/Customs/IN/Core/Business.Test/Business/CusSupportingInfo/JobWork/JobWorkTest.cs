using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobWork))]
sealed class JobWorkTest : CusSupportingInfoTest<JobWork>
{
	public void TestCSI_LineNo()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_LineNoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Serial Number", resData.Caption);
			AssertEquals("MediumCaption", "Serial No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Sr. No.", resData.ShortCaption);
		});
	}

	public void TestCSI_ReferenceNumber()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_ReferenceNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", JobWork.Schema.CSI_ReferenceNumberMaxLength, JobWork.CSI_ReferenceNumberInfo.MaxLength);
			AssertEquals("Caption", "BE Number", resData.Caption);
			AssertEquals("MediumCaption", "BE No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "BE No.", resData.ShortCaption);
		});
	}

	public void TestCSI_DateOfIssue()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_DateOfIssueInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "BE Date", resData.Caption);
			AssertEquals("MediumCaption", "BE Dt", resData.MediumCaption);
			AssertEquals("ShortCaption", "BE Dt", resData.ShortCaption);
		});
	}

	public void TestCSI_CustomsOffice()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_CustomsOfficeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", JobWork.Schema.CSI_CustomsOfficeMaxLength, JobWork.CSI_CustomsOfficeInfo.MaxLength);
			AssertEquals("Caption", "BE Customs House", resData.Caption);
			AssertEquals("MediumCaption", "Customs House", resData.MediumCaption);
			AssertEquals("ShortCaption", "Cus. House", resData.ShortCaption);
		});
	}

	public void TestCSI_ReferenceNumber2()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_ReferenceNumber2Info);
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", JobWork.Schema.CSI_ReferenceNumber2MaxLength, JobWork.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals("Caption", "BE Invoice Serial No", resData.Caption);
			AssertEquals("MediumCaption", "Invoice Sr. No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Inv. Sr. No.", resData.ShortCaption);
		});
	}

	public void TestCSI_ItemNumber()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_ItemNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "BE Item Sr. No.", resData.Caption);
			AssertEquals("MediumCaption", "Item Sr.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Item Sr.", resData.ShortCaption);
		});
	}

	public void TestCSI_Quantity()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_QuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "BE Quantity Utilized", resData.Caption);
			AssertEquals("MediumCaption", "Qty. Utilized", resData.MediumCaption);
			AssertEquals("ShortCaption", "Qty.", resData.ShortCaption);
		});
	}

	public void TestCSI_UnitOfQuantity()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(JobWork.CSI_UnitOfQuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("MaxLength", JobWork.Schema.CSI_UnitOfQuantityMaxLength, JobWork.CSI_UnitOfQuantityInfo.MaxLength);
			AssertEquals("Caption", "BE UOM", resData.Caption);
			AssertEquals("MediumCaption", "UOM", resData.MediumCaption);
			AssertEquals("ShortCaption", "UOM", resData.ShortCaption);
		});
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(CusSupportingInfoTypeList.Codes.JobWork, JobWork.CSI_Type);
	}

	public void TestLookups()
	{
		AssertType<JobWorkLookups>(JobWork.Lookups);
	}

	public void TestValidation()
	{
		AssertType<JobWorkValidation>(JobWork.Validation);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBizObj(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBizObj(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override IEnumerable<JobWork> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var jobwork = GetNewBizObj(factory);
		yield return jobwork;
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, JobWork bizObj)
	{
		var parent = factory.Load<JobComInvoiceLine>(bizObj.CSI_ParentID);
	}

	JobWork GetNewBizObj(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.JobWorks.AddNew();
	}

	JobWork JobWork => fJobWork ??= GetNewBizObj(Factory);
	JobWork fJobWork;
}
