using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader) => DocCusEntryHeader.New(entryHeader, Factory);

		protected override string TestingCountry => Core.Constants.CountryCodes.Mexico;
	}
}
