using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPEXTMessageHeaderProvider))]
	class EXPEXTMessageHeaderProviderTest : AESMessageHeaderProviderAbstractTest<EXPEXTMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPEXTMessageHeaderProvider(null));
		}

		public void TestAESHeader()
		{
			AssertType<EXPEXTHeaderProvider>(Provider.AESHeader);
		}

		protected override IEnumerable<Expression<Func<EXPEXTMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.AESHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		ExportEntryMessageSendingAction action;

		protected override EXPEXTMessageHeaderProvider GetProvider() => new EXPEXTMessageHeaderProvider(action);
	}
}
