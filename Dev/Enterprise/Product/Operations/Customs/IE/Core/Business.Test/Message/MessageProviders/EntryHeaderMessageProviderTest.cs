using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class EntryHeaderMessageProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusEntryHeader missing", () => new EntryHeaderMessageProviderForTest(null));

				var entryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown<ArgumentException>("JobDeclaration missing", () => new EntryHeaderMessageProviderForTest(entryHeader));

				var declaration = Factory.New<JobDeclaration>();
				entryHeader.CH_JE = declaration.PK;
				AssertExceptionThrown<ArgumentException>("CusEntryInstruction missing", () => new EntryHeaderMessageProviderForTest(entryHeader));

				var instruction = Factory.New<CusEntryInstruction>();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				AssertNoExceptionThrown("No missing", () => new EntryHeaderMessageProviderForTest(entryHeader));
			});
		}

		class EntryHeaderMessageProviderForTest : EntryHeaderMessageProvider
		{
			public EntryHeaderMessageProviderForTest(CusEntryHeader entryHeader)
				: base(entryHeader)
			{ }
		}
	}
}
