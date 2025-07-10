using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineValidation))]
sealed class CusTempStorageRegLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckSRL_LineNumber_NotZero()
	{
		CombineAssertions(() =>
		{
			regLine.SRL_LineNumber = 0;
			AssertHasErrorContaining("Zero", regLine.SRL_LineNumberInfo, MandatoryValidation.ValueCannotBeZero);

			regLine.SRL_LineNumber = 1;
			AssertNoErrorContaining("Not Zero", regLine.SRL_LineNumberInfo, MandatoryValidation.ValueCannotBeZero);
		});
	}

	public void TestCheckSRL_LineNumber_NotNegative()
	{
		regLine.SRL_LineNumber = -1;
		AssertHasErrorContaining("Negative", regLine.SRL_LineNumberInfo, MandatoryValidation.ValueCannotBeNegative);

		regLine.SRL_LineNumber = 1;
		AssertNoErrorContaining("Positive", regLine.SRL_LineNumberInfo, MandatoryValidation.ValueCannotBeNegative);
	}

	public void TestCheckSRL_PackagesRemaining()
	{
		const string error = "Packages Remaining should not be negative after calculating based on all the transactions Package Quantity.";
		CombineAssertions(() =>
		{
			regLine.SRL_PackagesRemaining = -1;
			AssertHasError("Negative", regLine.SRL_PackagesRemainingInfo, error);

			regLine.SRL_PackagesRemaining = 8;
			AssertNoError("Positive", regLine.SRL_PackagesRemainingInfo, error);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		regLine = Factory.New<CusTempStorageRegLine>();
	}
	CusTempStorageRegLine regLine;
}
