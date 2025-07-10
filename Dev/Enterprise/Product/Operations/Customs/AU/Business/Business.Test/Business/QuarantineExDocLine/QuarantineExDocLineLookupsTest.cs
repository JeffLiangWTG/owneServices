using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestException()
		{
			AssertExceptionThrown<NotImplementedException>(() => new QuarantineExDocLineLookups(Factory.New<QuarantineExDocLine>()));
		}
	}
}
