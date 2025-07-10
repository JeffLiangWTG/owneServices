using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class TradeTermsWrapperTest : DataProviderTestCase<TradeTermsWrapper>
{
	public void TestConditionCode()
	{
		entryHeader.RandomHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
		AssertEquals(Core.Constants.IncoTerms.CostInsuranceAndFreight, wrapper.ConditionCode);
	}

	public void TestDescription()
	{
		AssertNullOrEmpty(wrapper.Description);
	}

	public void TestLocationID()
	{
		entryHeader.RandomHeader.ZG_AgreedPlaceCode = "NLRTM";
		AssertEquals("NLRTM", wrapper.LocationID);
	}

	public void TestLocationName()
	{
		entryHeader.RandomHeader.ZG_AgreedPlaceCode = "NL";
		entryHeader.RandomHeader.JZ_IncoTermPlace = "NLRTM";
		AssertEquals("NLRTM", wrapper.LocationName);
	}

	public void TestLocationName_Empty()
	{
		entryHeader.RandomHeader.JZ_IncoTermPlace = "NLRTM";
		AssertNullOrEmpty(wrapper.LocationName);
	}

	public void TestCountryCode()
	{
		entryHeader.RandomHeader.ZG_AgreedPlaceCode = "NL";
		AssertEquals("NL", wrapper.CountryCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<CusEntryHeader>();
		wrapper = new TradeTermsWrapper(entryHeader);
	}
	CusEntryHeader entryHeader;
	TradeTermsWrapper wrapper;

	protected override TradeTermsWrapper GetProvider() => wrapper;
}
