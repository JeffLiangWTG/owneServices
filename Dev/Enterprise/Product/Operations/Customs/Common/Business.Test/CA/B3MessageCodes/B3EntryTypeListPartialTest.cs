using NUnit.Framework;

namespace Enterprise.Customs.Common.CA.Testing
{
	class B3EntryTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetWarehouseEntryType()
		{
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.Confirming), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.NonWarehouse));
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.Warehouse10), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Inward));
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.ReWarehouse13), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Inward));
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.ExWarehouse20), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.ExWarehouse21), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.ExWarehouse22), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(B3EntryTypeList.GetWarehouseEntryType(B3EntryTypeList.Codes.TransferOfGoods30), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));

			NUnit.Framework.Assert.That(!B3EntryTypeList.IsInwardWarehouseEntryType(B3EntryTypeList.Codes.Confirming));
			NUnit.Framework.Assert.That(B3EntryTypeList.IsInwardWarehouseEntryType(B3EntryTypeList.Codes.Warehouse10));
			NUnit.Framework.Assert.That(!B3EntryTypeList.IsInwardWarehouseEntryType(B3EntryTypeList.Codes.ExWarehouse20));
			NUnit.Framework.Assert.That(!B3EntryTypeList.IsInwardWarehouseEntryType(B3EntryTypeList.Codes.TransferOfGoods30));

			NUnit.Framework.Assert.That(!B3EntryTypeList.IsExWarehouseEntryType(B3EntryTypeList.Codes.Confirming));
			NUnit.Framework.Assert.That(!B3EntryTypeList.IsExWarehouseEntryType(B3EntryTypeList.Codes.Warehouse10));
			NUnit.Framework.Assert.That(B3EntryTypeList.IsExWarehouseEntryType(B3EntryTypeList.Codes.ExWarehouse20));
			NUnit.Framework.Assert.That(B3EntryTypeList.IsExWarehouseEntryType(B3EntryTypeList.Codes.TransferOfGoods30));
		}

		[ExpectNoExceptions]
		public void TestCSAImporter()
		{
			var b3EntryTypeList = new B3EntryTypeList();
			NUnit.Framework.Assert.That(b3EntryTypeList.ContainsCode("TT"), Is.EqualTo(true));
		}
	}
}
