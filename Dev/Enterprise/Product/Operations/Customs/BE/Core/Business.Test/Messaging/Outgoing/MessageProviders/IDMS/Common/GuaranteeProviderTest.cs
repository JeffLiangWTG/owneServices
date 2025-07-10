using System;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GuaranteeProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("no instruction", () => new GuaranteeProvider(null, 1, "X"));
		AssertExceptionThrown<ArgumentException>("no guarantee type", () => new GuaranteeProvider(Factory.New<CusEntryInstruction>(), 1, string.Empty));
	});

	public void TestSequenceNumber()
	{
		AssertEquals(3, provider.SequenceNumber);
	}

	public void TestGuaranteeType()
	{
		AssertEquals("TYP", provider.GuaranteeType);
	}

	public void TestOtherGuaranteeReference()
	{
		AssertNull(provider.OtherGuaranteeReference);
	}

	public void TestGuaranteeReferences()
	{
		var guaranteeTYP = entryInstruction.Guarantees.AddNew();
		guaranteeTYP.PW_BondType = "TYP";
		var guaranteeTYPTwo = entryInstruction.Guarantees.AddNew();
		guaranteeTYPTwo.PW_BondType = "TYP";
		var guaranteeXXX = entryInstruction.Guarantees.AddNew();
		guaranteeXXX.PW_BondType = "XXX";

		AssertEquals(2, provider.GuaranteeReferences.Count);
	}

	protected override GuaranteeProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		entryInstruction = Factory.New<CusEntryInstruction>();
		provider = new GuaranteeProvider(entryInstruction, 3, "TYP");
	}
	CusEntryInstruction entryInstruction;
	GuaranteeProvider provider;
}
