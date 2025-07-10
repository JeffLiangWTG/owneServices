using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderGuaranteeTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When guarantee is null", () => new TransitHeaderGuarantee(null));
		AssertNoExceptionThrown(() => new TransitHeaderGuarantee(Factory.New<NctsGuarantee>()));
	}

	public void TestProperties()
	{
		var guarantee = Factory.NewDepartureNctsHeader().Guarantees.AddNew();
		guarantee.PW_BondType = "TYPE";
		guarantee.PW_BondNumber = "BOND1";
		guarantee.PW_BondNumber2 = "BOND2";
		guarantee.PW_Password = "PWD";
		guarantee.PW_BondAmount = 1m;

		CombineAssertions(() =>
		{
			var wrapper = new TransitHeaderGuarantee(guarantee);
			AssertEquals(nameof(wrapper.AccessCode), "PWD", wrapper.AccessCode);
			AssertNull(nameof(wrapper.Amount), wrapper.Amount);
			AssertEquals(nameof(wrapper.Grn), "BOND1", wrapper.Grn);
			AssertEquals(nameof(wrapper.NotValidForEC), ZString.Empty, wrapper.NotValidForEC);
			AssertEquals(nameof(wrapper.NotValidForOtherContractingParties), ZString.Empty, wrapper.NotValidForOtherContractingParties);
			AssertEquals(nameof(wrapper.OtherReference), "BOND2", wrapper.OtherReference);
			AssertEquals(nameof(wrapper.Type), "TYPE", wrapper.Type);
		});
	}

	public void TestAmount()
	{
		var guarantee = Factory.NewDepartureNctsHeader().Guarantees.AddNew();
		guarantee.PW_BondNumber = "BOND1";
		guarantee.PW_BondAmount = 1m;

		var wrapper = new TransitHeaderGuarantee(guarantee);
		AssertNotNullOrEmpty("[PRE-CONDITION] Grn", wrapper.Grn);
		AssertNull("When Grn is not empty, Amount", wrapper.Amount);

		guarantee.PW_BondNumber = "";
		wrapper = new TransitHeaderGuarantee(guarantee);
		AssertNullOrEmpty("[PRE-CONDITION] Grn", wrapper.Grn);
		AssertEquals("When Grn is empty, Amount", 1m, wrapper.Amount);
	}

	public void TestNullWrapperAmountWhenGuaranteeAmountIsZero()
	{
		var guarantee = Factory.NewDepartureNctsHeader().Guarantees.AddNew();
		guarantee.PW_BondAmount = 0m;
		var wrapper = new TransitHeaderGuarantee(guarantee);
		wrapper = new TransitHeaderGuarantee(guarantee);
		AssertNull(nameof(wrapper.Amount), wrapper.Amount);
	}
}
