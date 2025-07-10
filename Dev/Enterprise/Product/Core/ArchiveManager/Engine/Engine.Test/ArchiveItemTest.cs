using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.Test
{
	class ArchiveItemTest : TestCaseWithFactory
	{
		public void TestArchiveItemToString()
		{
			var pkColumn = DummyBizoSchema.PK;
			var pk = Guid.NewGuid();
			var archiveItem = new ArchiveItem(pkColumn, pk, pkColumn, pk, false, pkColumn.ColumnPrefix);

			AssertEquals($"ArchiveItem(tableName=DummyBizo, pk={pk}, parentTableName=DummyBizo, parentPK={pk}, isReversed=False)", archiveItem.ToString());
		}

		public void TestArchiveItemToString_WithoutParent()
		{
			var pkColumn = DummyBizoSchema.PK;
			var pk = Guid.NewGuid();
			var archiveItem = new ArchiveItem(pkColumn, pk, null, Guid.Empty, false, pkColumn.ColumnPrefix);

			AssertEquals($"ArchiveItem(tableName=DummyBizo, pk={pk}, parentTableName=, parentPK={Guid.Empty}, isReversed=False)", archiveItem.ToString());
		}
	}
}
