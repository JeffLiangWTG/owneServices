using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCDECMessageHeaderProvider))]
	class CFCDECMessageHeaderProviderTest : ImportMessageHeaderProviderTest<CFCDECMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCDECMessageHeaderProvider(null));
		}

		public void TestHeader()
		{
			AssertType<CFCDECHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup_ZBE_A()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals("ZBE", Provider.MessageGroup);
		}

		public void TestMessageGroup_ZBE_B()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.B;
			AssertEquals("ZBE", Provider.MessageGroup);
		}

		public void TestMessageGroup_ZBV_C()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.C;
			AssertEquals("ZBV", Provider.MessageGroup);
		}

		public void TestMessageGroup_ZBV_D()
		{
			entryHeader.EntryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.D;
			AssertEquals("ZBV", Provider.MessageGroup);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}

		protected override IEnumerable<Expression<Func<CFCDECMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override CFCDECMessageHeaderProvider GetProvider() => new CFCDECMessageHeaderProvider(entryHeader);
	}
}
