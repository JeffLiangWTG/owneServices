using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWPEDMessageHeaderProvider))]
	class SCWPEDMessageHeaderProviderTest : MonthlyClosingMessageHeaderProviderTest<SCWPEDMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWPEDMessageHeaderProvider(null, string.Empty));
		}

		public void TestHeader()
		{
			AssertType<SCWPEDHeaderProvider>(Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals(MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingBondedWarehouse, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<SCWPEDMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override SCWPEDMessageHeaderProvider GetProvider() => new SCWPEDMessageHeaderProvider(declaration, string.Empty);
	}
}
