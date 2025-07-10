using System;
using NUnit.Framework;

namespace Enterprise.Accounting.RulesEngine.Facts.Testing
{
	public class WarehouseFactTest : TestCase
	{
		public void TestConstructor()
		{
			var pk = Guid.NewGuid();
			var code = "CODE";
			var organisation = new WarehouseFact(pk, code);
			AssertEquals(pk, organisation.PK);
			AssertEquals(code, organisation.Code);
		}

		public void TestConstructor_Throws_WhenCreatedWithNullCode()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WarehouseFact(Guid.NewGuid(), null));
		}
	}
}
