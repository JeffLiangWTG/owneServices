using CargoWise.Common.Collections;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class EoriOrTcuTraderWrapperTest : TestCaseWithFactory
{
	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			var exportTrader = GetNewTrader(orgAddress);
			AssertEquals("When organisation does not have EOR or TCU, IdentificationNumber", "", exportTrader.IdentificationNumber);

			var eoriCode = orgAddress.CustomsCodes.AddNew("EOR", "385040449", "US");
			orgAddress.CustomsCodes.AddNew("TCU", "ABB3332", "US");

			exportTrader = GetNewTrader(orgAddress);
			AssertEquals("When organisation has both EOR and TCU, IdentificationNumber", "US385040449", exportTrader.IdentificationNumber);

			orgAddress.CustomsCodes.RemoveAll(x => x.OK_CodeType == "EOR");
			exportTrader = GetNewTrader(orgAddress);
			AssertEquals("When organisation has only TCU, IdentificationNumber", "USABB3332", exportTrader.IdentificationNumber);
		});
	}

	public void TestAddress()
	{
		var exportTrader = GetNewTrader(orgAddress);
		AssertEquals("[PRE-CONDITION] IdentificationNumber", "", exportTrader.IdentificationNumber);
		CombineAssertions("When IdentificationNumber is Empty", () =>
		{
			AssertNotNull(nameof(ITrader.Address), exportTrader.Address);
			AssertType<AddressWrapper>(nameof(ITrader.Address), exportTrader.Address);
		});

		orgAddress.CustomsCodes.AddNew("TCU", "ABB3332", "US");
		exportTrader = GetNewTrader(orgAddress);
		AssertNotEquals("[PRE-CONDITION] IdentificationNumber", "", exportTrader.IdentificationNumber);
		AssertNull("When IdentificationNumber is populated", exportTrader.Address);
	}

	public void TestEoriNumber()
	{
		var exportTrader = GetNewTrader(orgAddress);
		AssertEquals(nameof(IEoriTrader.EoriNumber), "", exportTrader.EoriNumber);

		orgAddress.CustomsCodes.AddNew("EOR", "385040449", "US");
		exportTrader = GetNewTrader(orgAddress);
		AssertEquals("When organisation has EOR, EoriNumber", "US385040449", exportTrader.IdentificationNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgAddress = Factory.New<OrgHeader>().MainAddress;
	}

	OrgAddress orgAddress;

	IEoriTrader GetNewTrader(OrgAddress orgAddress) => new EoriOrTcuTraderWrapper(orgAddress);
}
