using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWProduction))]
sealed class SWProductionTest : CusSupportingInfoTest<SWProduction>
{
	public void TestCSI_LineNo()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWProduction.CSI_LineNoInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Serial Number", resData.Caption);
			AssertEquals("MediumCaption", "Serial No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Sr. No.", resData.ShortCaption);
		});
	}

	public void TestISequenceNumberLine()
	{
		CombineAssertions(() =>
		{
			var sequenceLine = (IHugeSequenceNumberLine)SWProduction;
			AssertEquals("FKToHeader", SWProduction.Parent.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZInt)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestCSI_LineNoSequence()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var production1 = invoiceLine.SWProductions.AddNew();
		var production2 = invoiceLine.SWProductions.AddNew();
		var production3 = invoiceLine.SWProductions.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", 1, production1.CSI_LineNo);
			AssertEquals("Order 2", 2, production2.CSI_LineNo);
			AssertEquals("Order 3", 3, production3.CSI_LineNo);

			invoiceLine.SWProductions.Remove(production2);
			AssertEquals("Order 1 stay same", 1, production1.CSI_LineNo);
			AssertEquals("Order 3 change to 2", 2, production3.CSI_LineNo);

			production1.Delete();
			AssertEquals("Order 2 change to 1", 1, production3.CSI_LineNo);
		});
	}

	public void TestCSI_ReferenceNumber()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWProduction.CSI_ReferenceNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Batch ID", resData.Caption);
			AssertEquals("MediumCaption", "Batch ID", resData.MediumCaption);
			AssertEquals("ShortCaption", "ID", resData.ShortCaption);
		});
	}

	public void TestCSI_Quantity()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWProduction.CSI_QuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Batch Quantity", resData.Caption);
			AssertEquals("MediumCaption", "Batch Qty.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Qty.", resData.ShortCaption);
		});
	}

	public void TestCSI_UnitOfQuantity()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWProduction.CSI_UnitOfQuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Unit Of Quantity", resData.Caption);
			AssertEquals("MediumCaption", "Unit Qty.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Qty.", resData.ShortCaption);
		});
	}

	public void TestCSI_DateOfIssue()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWProduction.CSI_DateOfIssueInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Manufacturing Date", resData.Caption);
			AssertEquals("MediumCaption", "Mfg. Dt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "M. Dt.", resData.ShortCaption);
		});
	}

	public void TestCSI_DateOfExpiry()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWProduction.CSI_DateOfExpiryInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Expiry Date", resData.Caption);
			AssertEquals("MediumCaption", "Exp. Dt.", resData.MediumCaption);
			AssertEquals("ShortCaption", "E. Dt.", resData.ShortCaption);
		});
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(CusSupportingInfoTypeList.Codes.SingleWindowProduction, SWProduction.CSI_Type);
	}

	public void TestLookups()
	{
		AssertType<SWProductionLookups>(SWProduction.Lookups);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBizObj(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBizObj(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override IEnumerable<SWProduction> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var production = GetNewBizObj(factory);
		yield return production;
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, SWProduction bizObj)
	{
		factory.Load<JobComInvoiceLine>(bizObj.CSI_ParentID);
	}

	SWProduction GetNewBizObj(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SWProductions.AddNew();
	}

	SWProduction SWProduction => fSWProduction ??= GetNewBizObj(Factory);
	SWProduction fSWProduction;
}
