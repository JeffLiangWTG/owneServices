using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWConstituentValidation))]
sealed class SWConstituentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Description()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SWConstituent.CSI_DescriptionInfo);
	}

	public void TestCheckCSI_Code()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SWConstituent.CSI_CodeInfo);
	}

	public void TestCheckCSI_Status()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SWConstituent.CSI_StatusInfo);
	}

	public void TestCheckCSI_Quantity()
	{
		const string message = "Constituent Percentage should not greater than 100.000%%.";
		SWConstituent.CSI_Quantity = 0m;
		var info = SWConstituent.CSI_QuantityInfo;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(info);

			AssertNoMessageError(info, message);

			SWConstituent.CSI_Quantity = 100m;
			AssertNoMessageError(info, message);

			SWConstituent.CSI_Quantity = 100.001m;
			AssertHasMessageError(info, message);
		});
	}

	public void TestCheckCSI_Quantity2()
	{
		const string message = "Constituent Yield Percentage should not greater than 100.000%%.";
		SWConstituent.CSI_Quantity2 = 0m;
		var info = SWConstituent.CSI_Quantity2Info;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(info);

			AssertNoMessageError(info, message);

			SWConstituent.CSI_Quantity2 = 100m;
			AssertNoMessageError(info, message);

			SWConstituent.CSI_Quantity2 = 100.001m;
			AssertHasMessageError(info, message);
		});
	}

	SWConstituent SWConstituent => fSWConstituent ??= Factory.New<SWConstituent>();
	SWConstituent fSWConstituent;
}
