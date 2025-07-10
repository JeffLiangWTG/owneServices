using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPINVMessageHeaderProvider))]
	class EXPINVMessageHeaderProviderTest : AESMessageHeaderProviderAbstractTest<EXPINVMessageHeaderProvider>
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPINVMessageHeaderProvider(null));
		}

		public void TestAESHeader()
		{
			AssertType<EXPINVHeaderProvider>(Provider.AESHeader);
		}

		protected override IEnumerable<Expression<Func<EXPINVMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.AESHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}
		protected ExportEntryMessageSendingAction action;

		protected override EXPINVMessageHeaderProvider GetProvider() => new EXPINVMessageHeaderProvider(action);
	}
}
