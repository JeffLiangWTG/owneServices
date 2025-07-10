using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocPassarCusEntryHeader))]
public class DocPassarCusEntryHeaderTest : DocBaseCusEntryHeaderTest<CusEntryHeader, DocPassarCusEntryHeader>
{
	protected override DocPassarCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
	{
		return DocPassarCusEntryHeader.New(entryHeader, Factory);
	}
}
