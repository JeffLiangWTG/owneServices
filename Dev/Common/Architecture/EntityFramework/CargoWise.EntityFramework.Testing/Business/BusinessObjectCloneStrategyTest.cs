using System.Collections.Generic;
using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCloneStrategyTest : TestCaseWithDummy
	{
		public void TestCloneInternal()
		{
			BusinessObjectCloneStrategy strategy = new BusinessObjectCloneStrategy(Dummy);
			List<string> columnsToExclude = new List<string>();
			columnsToExclude.Add(DummyBusinessObject.Schema.Z0_AnotherDate);

			Dummy.Z0_AnotherDate = ZDateTime.BrettsBirthday;

			BusinessObject cloned = strategy.Clone(new BusinessObjectCloneArgs(columnsToExclude, typeof(TestDummyClass)));
			AssertEquals("Z0_AnotherDate excluded", ZDateTime.Empty, cloned[DummyBusinessObject.Schema.Z0_AnotherDate]);
			AssertEquals("Type of cloned", typeof(TestDummyClass), cloned.GetType());
		}

		class TestDummyClass : DummyBusinessObject
		{
			public TestDummyClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
