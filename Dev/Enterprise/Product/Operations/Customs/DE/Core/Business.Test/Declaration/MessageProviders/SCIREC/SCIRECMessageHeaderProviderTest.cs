using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIRECMessageHeaderProvider))]
	sealed class SCIRECMessageHeaderProviderTest : ImportMessageHeaderProviderTest<SCIRECMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusEntryHeader null", () => new SCIRECMessageHeaderProvider(null));

				AssertExceptionThrown<ArgumentException>("CusEntryHeader.Declaration null", () => new SCIRECMessageHeaderProvider(Factory.New<CusEntryHeader>()));

				entryHeader.CH_CEI_Instruction = Guid.Empty;
				AssertExceptionThrown<ArgumentException>("CusEntryHeader.EntryInstruction null", () => new SCIRECMessageHeaderProvider(entryHeader));
			});
		}

		public void TestHeader()
		{
			AssertType<SCIRECHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup_AAV_C()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.C;
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_AVV_F()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.F;
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedPrematureDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_InvalidCEI_SubStyle()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals(string.Empty, Provider.MessageGroup);
		}

		public void TestMessageGroup_EmptyCEI_SubStyle()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = string.Empty;
			AssertEquals(string.Empty, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<SCIRECMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override SCIRECMessageHeaderProvider GetProvider() => new SCIRECMessageHeaderProvider(entryHeader);
	}
}
