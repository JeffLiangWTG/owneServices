using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWDECMessageHeaderProvider))]
	class SCWDECMessageHeaderProviderTest : ImportMessageHeaderProviderTest<SCWDECMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusEntryHeader null", () => new SCWDECMessageHeaderProvider(null));

				AssertExceptionThrown<ArgumentException>("CusEntryHeader.Declaration null", () => new SCWDECMessageHeaderProvider(Factory.New<CusEntryHeader>()));

				entryHeader.CH_CEI_Instruction = ZGuid.Empty;
				AssertExceptionThrown<ArgumentException>("CusEntryHeader.EntryInstruction null", () => new SCWDECMessageHeaderProvider(entryHeader));
			});
		}

		public void TestHeader()
		{
			AssertType<SCWDECHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup_LAE_A()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals("LAE", Provider.MessageGroup);
		}

		public void TestMessageGroup_LVE_D()
		{
			entryInstruction.CEI_SubStyle = ImportSubStyleList.Codes.D;
			AssertEquals("LVE", Provider.MessageGroup);
		}

		public void TestMessageGroup_LVE_Invalid()
		{
			entryInstruction.CEI_SubStyle = "X";
			AssertEquals(ZString.Empty, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<SCWDECMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override SCWDECMessageHeaderProvider GetProvider() => new SCWDECMessageHeaderProvider(entryHeader);
	}
}
