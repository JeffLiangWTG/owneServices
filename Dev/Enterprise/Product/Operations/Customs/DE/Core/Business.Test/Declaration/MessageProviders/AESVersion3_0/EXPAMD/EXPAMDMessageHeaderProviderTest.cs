using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPAMDMessageHeaderProvider))]
	class EXPAMDMessageHeaderProviderTest : AESMessageHeaderProviderAbstractTest<EXPAMDMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPAMDMessageHeaderProvider(null));
		}

		public void TestAESHeader()
		{
			AssertType<EXPAMDHeaderProvider>("Type", Provider.AESHeader);
		}

		protected override IEnumerable<Expression<Func<EXPAMDMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.AESHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		protected ExportEntryMessageSendingAction action;

		protected override EXPAMDMessageHeaderProvider GetProvider() => new EXPAMDMessageHeaderProvider(action);
	}
}
