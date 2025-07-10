using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPDATMessageHeaderProvider))]
	class EXPDATMessageHeaderProviderTest : AESMessageHeaderProviderAbstractTest<EXPDATMessageHeaderProvider>
	{
		public void TestConsturctor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPDATMessageHeaderProvider(null));
		}

		public void TestAESHeader()
		{
			AssertType<EXPDATHeaderProvider>(Provider.AESHeader);
		}

		protected override IEnumerable<Expression<Func<EXPDATMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
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

		protected override EXPDATMessageHeaderProvider GetProvider() => new EXPDATMessageHeaderProvider(action);
	}
}
