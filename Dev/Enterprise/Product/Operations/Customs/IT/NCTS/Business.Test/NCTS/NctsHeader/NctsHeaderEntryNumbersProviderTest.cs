using Enterprise.Customs.IT.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderEntryNumbersProvider))]
sealed class NctsHeaderEntryNumbersProviderTest : CusEntryNumbersProviderTest<NctsHeader, NctsHeaderEntryNumbersProvider>
{
	protected override NctsHeader GetParentBizObj() => Factory.New<NctsHeader>();

	protected override NctsHeaderEntryNumbersProvider GetCusEntryNumbersProvider(NctsHeader parentBizObj) => new NctsHeaderEntryNumbersProvider(parentBizObj);
}
