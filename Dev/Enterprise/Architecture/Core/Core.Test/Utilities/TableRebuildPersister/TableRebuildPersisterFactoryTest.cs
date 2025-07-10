using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TableRebuildPersisterFactoryTest : TestCase
	{
		public void TestCreatesTableRebuildPersister()
		{
			var factory = new TableRebuildPersisterFactory();
			var persister = factory.Get(Db.Connection);

			AssertType<TableRebuildPersister>(persister);
		}
	}
}
