using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class GuaranteeWrapperTest : TestCaseWithFactory
{
	public void TestContructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new GuaranteeWrapper(null));
	}

	public void TestAccessCode()
	{
		var guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.AccessCode), "", guarantee.AccessCode);

		guaranteeEntryInstruction.PW_Password = "012A";
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.AccessCode), "012A", guarantee.AccessCode);
	}

	public void TestAdditionalReference()
	{
		var guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.AdditionalReference), "", guarantee.AdditionalReference);

		guaranteeEntryInstruction.PW_BondNumber2 = "ADDITIONAL REFERENCE";
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.AdditionalReference), "ADDITIONAL REFERENCE", guarantee.AdditionalReference);
	}

	public void TestCurrencyCode()
	{
		var guarantee = GetNewGuarantee();
		guaranteeEntryInstruction.PW_RX_NKCurrency = ZString.Empty;
		AssertEquals(nameof(IGuarantee.CurrencyCode), "", guarantee.CurrencyCode);

		guaranteeEntryInstruction.PW_RX_NKCurrency = "USD";
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.CurrencyCode), "USD", guarantee.CurrencyCode);
	}

	public void TestCustomsOffice()
	{
		var guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.CustomsOffice), "", guarantee.CustomsOffice);

		guaranteeEntryInstruction.PW_BondFiledPort = "ITCUSOFF";
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.CustomsOffice), "ITCUSOFF", guarantee.CustomsOffice);
	}

	public void TestDutyAmount()
	{
		var guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.DutyAmount), 0m, guarantee.DutyAmount);

		guaranteeEntryInstruction.PW_BondAmount = 100;
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.DutyAmount), 100m, guarantee.DutyAmount);
	}

	public void TestGrn()
	{
		var guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.Grn), "", guarantee.Grn);

		guaranteeEntryInstruction.PW_BondNumber = "GRN";
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(IGuarantee.Grn), "GRN", guarantee.Grn);
	}

	public void TestHolderIdentification()
	{
		var guarantee = GetNewGuarantee();
		AssertNullOrEmpty($"[PRE-CONDITION] {guarantee.HolderIdentification}", guarantee.HolderIdentification);

		guaranteeEntryInstruction.PW_HolderIdentification = "IT123456789";
		guarantee = GetNewGuarantee();
		AssertEquals(nameof(guarantee.HolderIdentification), "IT123456789", guarantee.HolderIdentification);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		guaranteeEntryInstruction = entryInstruction.Guarantees.AddNew();
	}

	GuaranteeForEntryInstruction guaranteeEntryInstruction;

	IGuarantee GetNewGuarantee() => new GuaranteeWrapper(guaranteeEntryInstruction);
}
