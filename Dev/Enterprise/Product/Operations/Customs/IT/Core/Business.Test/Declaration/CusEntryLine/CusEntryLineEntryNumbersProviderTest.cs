using Enterprise.Customs.IT.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineEntryNumbersProvider))]
sealed class CusEntryLineEntryNumbersProviderTest : CusEntryNumbersProviderTest<CusEntryLine, CusEntryLineEntryNumbersProvider>
{
	protected override CusEntryLineEntryNumbersProvider GetCusEntryNumbersProvider(CusEntryLine parentBizObj) => new CusEntryLineEntryNumbersProvider(parentBizObj);

	protected override CusEntryLine GetParentBizObj() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().MergedLines.AddNew();
}
