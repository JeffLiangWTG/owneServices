using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CUSWATMessageHeaderProvider))]
	class CUSWATMessageHeaderProviderTest : ImportMessageHeaderProviderTest<CUSWATMessageHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSWATMessageHeaderProvider(null));
		}

		public void TestHeader()
		{
			AssertType<CUSWATHeaderProvider>("Type", Provider.Header);
		}

		public void TestMessageGroup()
		{
			AssertEquals(Messaging.ImportMessageSubTypeList.Codes.WarehouseStockTransfer, Provider.MessageGroup);
		}

		protected override IEnumerable<Expression<Func<CUSWATMessageHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Header;
		}

		protected override CUSWATMessageHeaderProvider GetProvider() => new CUSWATMessageHeaderProvider(entryHeader);
	}
}
