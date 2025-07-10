using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ECWCCMMessageHeaderProvider))]
	sealed class ECWCCMMessageHeaderProviderTest : ImportMessageHeaderProviderTest<ECWCCMMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ECWCCMMessageHeaderProvider(null));
		}

		public void TestHeader()
		{
			AssertType<ECWCCMHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.WarehouseStockTransfer, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<ECWCCMMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override ECWCCMMessageHeaderProvider GetProvider() => new ECWCCMMessageHeaderProvider(entryHeader);
	}
}
