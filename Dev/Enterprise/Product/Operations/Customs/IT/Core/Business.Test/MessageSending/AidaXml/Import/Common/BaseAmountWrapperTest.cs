using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class BaseAmountWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new BaseAmountWrapper(null));
	}

	public void TestAmount()
	{
		var baseAmountWrapper = GetNewBaseAmountWrapper();
		AssertEquals(nameof(IBaseAmount.Amount), 0m, baseAmountWrapper.Amount);

		fee.CF_BaseValue = 124.34;
		baseAmountWrapper = GetNewBaseAmountWrapper();
		AssertEquals(nameof(IBaseAmount.Amount), 124.34m, baseAmountWrapper.Amount);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		fee = entryHeader.MergedLines.AddNew().Fees.AddNew();
	}

	CusEntryLineFee fee;

	IBaseAmount GetNewBaseAmountWrapper() => new BaseAmountWrapper(fee);
}
