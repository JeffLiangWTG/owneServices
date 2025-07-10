using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCRECMessageHeaderProvider))]
	sealed class CFCRECMessageHeaderProviderTest : ImportMessageHeaderProviderTest<CFCRECMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCRECMessageHeaderProvider(null));
		}

		public void TestHeader()
		{
			AssertType<CFCRECHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup_ZAV_C()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.C;
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_ZVV_F()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.F;
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedPrematureDeclaration, Provider.MessageGroup);
		}

		public void TestMessageGroup_InvalidCEI_SubStyle()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals(string.Empty, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<CFCRECMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override CFCRECMessageHeaderProvider GetProvider() => new CFCRECMessageHeaderProvider(entryHeader);
	}
}
