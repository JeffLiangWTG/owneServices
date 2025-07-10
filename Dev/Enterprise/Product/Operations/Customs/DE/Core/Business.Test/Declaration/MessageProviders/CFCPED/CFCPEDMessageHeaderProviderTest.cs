using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCPEDMessageHeaderProvider))]
	class CFCPEDMessageHeaderProviderTest : MonthlyClosingMessageHeaderProviderTest<CFCPEDMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCPEDMessageHeaderProvider(null, string.Empty));
		}

		public void TestHeader()
		{
			AssertType<CFCPEDHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals(MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<CFCPEDMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override CFCPEDMessageHeaderProvider GetProvider() => new CFCPEDMessageHeaderProvider(declaration, string.Empty);
	}
}
