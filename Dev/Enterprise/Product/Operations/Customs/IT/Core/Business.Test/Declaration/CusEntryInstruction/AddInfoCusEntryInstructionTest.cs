using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AddInfoCusEntryInstruction))]
sealed class AddInfoCusEntryInstructionTest : NonPersistentBusinessObjectTestCase
{
	public void TestLookups()
	{
		AssertType<AddInfoCusEntryInstructionLookups>(addInfoCusEntryInstruction.Lookups);
	}

	public void TestZG_ParticipantTypeMaxLength()
	{
		AssertEquals(3, addInfoCusEntryInstruction.ZG_ParticipantTypeInfo.MaxLength);
	}

	public void TestZG_PreviousInvoiceCurrencyMaxLength()
	{
		AssertEquals(3, addInfoCusEntryInstruction.ZG_PreviousInvoiceCurrencyInfo.MaxLength);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewAddInfoCusEntryInstruction();

	protected override void SetUp()
	{
		base.SetUp();
		addInfoCusEntryInstruction = GetNewAddInfoCusEntryInstruction();
	}

	AddInfoCusEntryInstruction addInfoCusEntryInstruction;

	AddInfoCusEntryInstruction GetNewAddInfoCusEntryInstruction() => new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
}
