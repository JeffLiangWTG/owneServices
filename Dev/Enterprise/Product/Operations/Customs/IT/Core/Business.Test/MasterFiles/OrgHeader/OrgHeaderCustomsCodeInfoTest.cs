using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OrgHeaderCustomsCodeInfoTest : TestCase
{
	public void TestEmpty()
	{
		var customsCodeInfo = new OrgHeaderCustomsCodeInfo(ZString.Empty, ZString.Empty);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(customsCodeInfo.FullId), ZString.Empty, customsCodeInfo.FullId);
			AssertEquals(nameof(customsCodeInfo.Id), ZString.Empty, customsCodeInfo.Id);
			AssertEquals(nameof(customsCodeInfo.IdCountryCode), ZString.Empty, customsCodeInfo.IdCountryCode);
		});
	}

	public void TestFilled()
	{
		var customsCodeInfo = new OrgHeaderCustomsCodeInfo("IT", "000000");
		CombineAssertions(() =>
		{
			AssertEquals(nameof(customsCodeInfo.FullId), "IT000000", customsCodeInfo.FullId);
			AssertEquals(nameof(customsCodeInfo.Id), "000000", customsCodeInfo.Id);
			AssertEquals(nameof(customsCodeInfo.IdCountryCode), "IT", customsCodeInfo.IdCountryCode);
		});
	}
}
