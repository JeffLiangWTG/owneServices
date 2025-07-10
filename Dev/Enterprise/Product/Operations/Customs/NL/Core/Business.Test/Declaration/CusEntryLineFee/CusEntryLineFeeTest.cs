using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFee))]
class CusEntryLineFeeTest : EU.Business.Declaration.Testing.CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var fee = entryLine.Fees.AddNew();

		return fee;
	}

	public void TestTypeOfValidation()
	{
		AssertType<CusEntryLineFeeValidation>(fee.Validation);
	}

	public void TestLookups()
	{
		AssertType<CusEntryLineFeeLookups>(fee.Lookups);
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
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew();
		fee = entryLine.Fees.AddNew();
	}

	CusEntryLineFee fee;
}

