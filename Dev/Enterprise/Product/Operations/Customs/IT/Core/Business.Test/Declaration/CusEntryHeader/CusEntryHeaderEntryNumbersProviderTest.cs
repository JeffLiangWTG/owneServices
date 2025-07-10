using Enterprise.Customs.IT.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeaderEntryNumbersProvider))]
sealed class CusEntryHeaderEntryNumbersProviderTest : CusEntryNumbersProviderTest<CusEntryHeader, CusEntryHeaderEntryNumbersProvider>
{
	protected override CusEntryHeader GetParentBizObj() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

	protected override CusEntryHeaderEntryNumbersProvider GetCusEntryNumbersProvider(CusEntryHeader parentBizObj) => new CusEntryHeaderEntryNumbersProvider(parentBizObj);
}
