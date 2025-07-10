using NUnit.Framework;

namespace Enterprise.Customs.Common.CA.Testing
{
	sealed class CADEntryTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetCADWarehouseEntryType()
		{
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.Confirming), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.NonWarehouse));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.Warehouse101), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Inward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.Warehouse102), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Inward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ReWarehouse131), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Inward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ReWarehouse132), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Inward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse201), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse211), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse212), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse213), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse214), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse215), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse216), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse22), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.TransferOfGoods301), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));
			NUnit.Framework.Assert.That(CADEntryTypeList.GetWarehouseEntryType(CADEntryTypeList.Codes.TransferOfGoods302), Is.EqualTo(B3EntryTypeList.WarehouseEntryType.Outward));

			NUnit.Framework.Assert.That(!CADEntryTypeList.IsInwardWarehouseEntryType(CADEntryTypeList.Codes.Confirming));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsInwardWarehouseEntryType(CADEntryTypeList.Codes.Warehouse101));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsInwardWarehouseEntryType(CADEntryTypeList.Codes.Warehouse102));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsInwardWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse201));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsInwardWarehouseEntryType(CADEntryTypeList.Codes.TransferOfGoods301));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsInwardWarehouseEntryType(CADEntryTypeList.Codes.TransferOfGoods302));

			NUnit.Framework.Assert.That(!CADEntryTypeList.IsExWarehouseEntryType(CADEntryTypeList.Codes.Confirming));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsExWarehouseEntryType(CADEntryTypeList.Codes.Warehouse101));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsExWarehouseEntryType(CADEntryTypeList.Codes.Warehouse102));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsExWarehouseEntryType(CADEntryTypeList.Codes.ExWarehouse201));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsExWarehouseEntryType(CADEntryTypeList.Codes.TransferOfGoods301));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsExWarehouseEntryType(CADEntryTypeList.Codes.TransferOfGoods302));
		}

		[ExpectNoExceptions]
		public void TestCSAImporter()
		{
			var cadEntryTypeList = new CADEntryTypeList();
			NUnit.Framework.Assert.That(cadEntryTypeList.ContainsCode("TT"), Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestIsWarehouseEntryTypeForReleaseLocationST()
		{
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.Warehouse101));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.Warehouse102));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.ReWarehouse131));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.ReWarehouse132));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.TransferOfGoods301));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.TransferOfGoods301));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationST(CADEntryTypeList.Codes.ExWarehouse22));
		}

		[ExpectNoExceptions]
		public void TestIsWarehouseEntryTypeForReleaseLocationSF()
		{
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ReWarehouse131));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ReWarehouse132));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse201));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse211));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse212));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse213));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse214));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse215));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse216));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.TransferOfGoods301));
			NUnit.Framework.Assert.That(CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.TransferOfGoods301));
			NUnit.Framework.Assert.That(!CADEntryTypeList.IsWarehouseEntryTypeForReleaseLocationSF(CADEntryTypeList.Codes.ExWarehouse22));
		}
	}
}
