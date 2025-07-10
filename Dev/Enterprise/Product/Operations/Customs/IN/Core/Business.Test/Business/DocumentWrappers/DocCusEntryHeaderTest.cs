using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DocCusEntryHeader))]
sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
{
	protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader) => DocCusEntryHeader.New(entryHeader, Factory);

	protected override string TestingCountry => Core.Constants.CountryCodes.India;

		public void TestNew()
		{
			AssertNull("Created with null", DocCusEntryHeader.New(null, Factory));
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertNotNull("Created with a valild object", DocCusEntryHeader.New(entryHeader, Factory));
		}
}
