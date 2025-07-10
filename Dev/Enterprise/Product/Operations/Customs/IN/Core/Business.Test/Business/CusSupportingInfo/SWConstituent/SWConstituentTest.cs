using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWConstituent))]
sealed class SWConstituentTest : CusSupportingInfoTest<SWConstituent>
{
	public void TestCSI_LineNo()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWConstituent.CSI_LineNoInfo);
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
			var sequenceLine = (IHugeSequenceNumberLine)SWConstituent;
			AssertEquals("FKToHeader", SWConstituent.Parent.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZInt)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestCSI_LineNoSequence()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var constituent1 = invoiceLine.SWConstituents.AddNew();
		var constituent2 = invoiceLine.SWConstituents.AddNew();
		var constituent3 = invoiceLine.SWConstituents.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", 1, constituent1.CSI_LineNo);
			AssertEquals("Order 2", 2, constituent2.CSI_LineNo);
			AssertEquals("Order 3", 3, constituent3.CSI_LineNo);

			invoiceLine.SWConstituents.Remove(constituent2);
			AssertEquals("Order 1 stay same", 1, constituent1.CSI_LineNo);
			AssertEquals("Order 3 change to 2", 2, constituent3.CSI_LineNo);

			constituent1.Delete();
			AssertEquals("Order 2 change to 1", 1, constituent3.CSI_LineNo);
		});
	}

	public void TestCSI_Description()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWConstituent.CSI_DescriptionInfo);
		CombineAssertions(() =>
		{
			AssertEquals(SWConstituent.Schema.CSI_DescriptionMaxLength, SWConstituent.CSI_DescriptionInfo.MaxLength);
			AssertEquals("Caption", "Constituent Element Name", resData.Caption);
			AssertEquals("MediumCaption", "Element Name", resData.MediumCaption);
			AssertEquals("ShortCaption", "Name", resData.ShortCaption);
		});
	}

	public void TestCSI_Code()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWConstituent.CSI_CodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals(SWConstituent.Schema.CSI_CodeMaxLength, SWConstituent.CSI_CodeInfo.MaxLength);
			AssertEquals("Caption", "Constituent Code", resData.Caption);
			AssertEquals("MediumCaption", "Code", resData.MediumCaption);
			AssertEquals("ShortCaption", "Code", resData.ShortCaption);
		});
	}

	public void TestCSI_Quantity()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWConstituent.CSI_QuantityInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Constituent Percentage", resData.Caption);
			AssertEquals("MediumCaption", "Percentage", resData.MediumCaption);
			AssertEquals("ShortCaption", "%%", resData.ShortCaption);
		});
	}

	public void TestCSI_Quantity2()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWConstituent.CSI_Quantity2Info);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Constituent Yield Percentage", resData.Caption);
			AssertEquals("MediumCaption", "Yield Percentage", resData.MediumCaption);
			AssertEquals("ShortCaption", "Yield %%", resData.ShortCaption);
		});
	}

	public void TestCSI_Status()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWConstituent.CSI_StatusInfo);
		CombineAssertions(() =>
		{
			AssertEquals(SWConstituent.Schema.CSI_StatusMaxLength, SWConstituent.CSI_StatusInfo.MaxLength);
			AssertEquals("Caption", "Constituent Active Ingredient", resData.Caption);
			AssertEquals("MediumCaption", "Active Ingredient", resData.MediumCaption);
			AssertEquals("ShortCaption", "Act. Indnt.", resData.ShortCaption);
		});
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(CusSupportingInfoTypeList.Codes.SingleWindowConstituent, SWConstituent.CSI_Type);
	}

	public void TestLookups()
	{
		AssertType<SWConstituentLookups>(SWConstituent.Lookups);
	}

	public void TestValidation()
	{
		AssertType<SWConstituentValidation>(SWConstituent.Validation);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBizObj(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBizObj(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override IEnumerable<SWConstituent> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var constituent = GetNewBizObj(factory);
		factory.Save();
		yield return constituent;
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, SWConstituent bizObj)
	{
		factory.Load<JobComInvoiceLine>(bizObj.CSI_ParentID);
	}

	SWConstituent GetNewBizObj(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SWConstituents.AddNew();
	}

	SWConstituent SWConstituent => fSWConstituent ??= GetNewBizObj(Factory);
	SWConstituent fSWConstituent;
}
