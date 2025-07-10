using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCIPEDMessageHeaderProvider))]
	class SCIPEDMessageHeaderProviderTest : MonthlyClosingMessageHeaderProviderTest<SCIPEDMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCIPEDMessageHeaderProvider(null, string.Empty));
		}

		public void TestHeader()
		{
			AssertType<SCIPEDHeaderProvider>(Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals(MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingInwardProcessing, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<SCIPEDMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override SCIPEDMessageHeaderProvider GetProvider() => new SCIPEDMessageHeaderProvider(declaration, string.Empty);
	}
}
