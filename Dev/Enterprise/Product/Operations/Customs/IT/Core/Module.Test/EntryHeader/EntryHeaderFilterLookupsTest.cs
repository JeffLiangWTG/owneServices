using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Module.Testing;

sealed class EntryHeaderFilterLookupsTest : TestCaseWithFactory
{
	public void TestControlChannelList()
	{
		AssertType<CustomsChannelCodeList>("ControlChannelList Type", filterLookups.ControlChannelList);
	}

	public void TestEntryInstructionStyleList()
	{
		AssertEquals("EntryInstructionStyleList CodesAsString", "DSE, COD, COL, H1, H2, H3, H4, H5, I1, I2, B1, B2, B4, C1, C2", filterLookups.EntryInstructionStyleList.CodesAsString);
	}

	public void TestCurrencyList()
	{
		AssertType<RefCurrencyCollection>("CurrencyList Type", filterLookups.CurrencyList);
	}

	public void TestExitStatusList()
	{
		AssertType<ExitStatusList>("ExitStatusList type", filterLookups.ExitStatusList);
	}

	public void TestArrivalStatusList()
	{
		AssertType<ITGuaranteeReleaseResultList>("ArrivalStatusList type", filterLookups.ArrivalStatusList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		filterBusinessObject = new EntryHeaderFilterBusinessObject();
		filterLookups = new EntryHeaderFilterLookups(filterBusinessObject);
	}

	EntryHeaderFilterBusinessObject filterBusinessObject;
	EntryHeaderFilterLookups filterLookups;
}
