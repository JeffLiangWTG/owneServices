using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFee))]
sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
{
	public void TestLookups()
	{
		AssertType(typeof(CusEntryLineFeeLookups), fee.Lookups);
	}

	public void TestValidation()
	{
		AssertType(typeof(CusEntryLineFeeValidation), fee.Validation);
	}

	public void TestCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew();
		var fee = entryLine.Fees.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("[UCC 4/3] Type", DataBoundResourceStrings.GetDataForProperty(fee.CF_ChargeTypeInfo).Caption);
			AssertEquals("[UCC 4/8] Method of payment", DataBoundResourceStrings.GetDataForProperty(fee.CF_MethodOfPaymentInfo).Caption);
			AssertEquals("[UCC 4/4] Total amount", DataBoundResourceStrings.GetDataForProperty(fee.CF_ChargeAmountInfo).Caption);
			AssertEquals("Tax Rate", DataBoundResourceStrings.GetDataForProperty(fee.CF_RateInfo).Caption);
			AssertEquals("Method of Calculation", DataBoundResourceStrings.GetDataForProperty(fee.CF_MethodOfCalculationInfo).Caption);
			AssertEquals("Base Amount", DataBoundResourceStrings.GetDataForProperty(fee.CF_BaseValueInfo).Caption);
			AssertEquals("Action", DataBoundResourceStrings.GetDataForProperty(fee.CF_RateOverrideReasonCodeInfo).Caption);
		});
	}

	protected override void SetUp()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_PaymentMethod = "A";
		var header = declaration.CustomsEntryHeaders.AddNew();
		var line = header.MergedLines.AddNew();
		fee = line.Fees.AddNew();
	}
	CusEntryLineFee fee;
}
