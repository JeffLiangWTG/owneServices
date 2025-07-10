using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(WCBDataRegistry))]
	public class WCBDataRegistryTest : RegistryItemSetTestCase<WCBDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Count", 6, AllItems.Count);
			AssertVisible(ItemSet.MercedesImporterItem);
			AssertVisible(ItemSet.ChryslerImporterItem);
			AssertVisible(ItemSet.MercedesSupplierItem);
			AssertVisible(ItemSet.ChryslerSupplierItem);
			AssertVisible(ItemSet.MercedesImportFileNamePrefixItem);
			AssertVisible(ItemSet.ChryslerImportFileNamePrefixItem);
		}

		public void TestMercedesImporter()
		{
			AssertEquals("DaimlerImporter", ZGuid.Empty, ItemSet.MercedesImporter);
			ZGuid mercedesImporter = ZGuid.NewZGuid();
			ItemSet.MercedesImporterItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mercedesImporter.ToGuid());
			AssertEquals("DaimlerImporter", mercedesImporter, ItemSet.MercedesImporter);
		}

		public void TestChryslerImporter()
		{
			AssertEquals("ChryslerImporter", ZGuid.Empty, ItemSet.ChryslerImporter);
			ZGuid chryslerImporter = ZGuid.NewZGuid();
			ItemSet.ChryslerImporterItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chryslerImporter.ToGuid());
			AssertEquals("ChryslerImporter", chryslerImporter, ItemSet.ChryslerImporter);
		}

		public void TestMercedesSupplier()
		{
			AssertEquals("DaimlerSupplier", ZGuid.Empty, ItemSet.MercedesSupplier);
			ZGuid mercedesSupplier = ZGuid.NewZGuid();
			ItemSet.MercedesSupplierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mercedesSupplier.ToGuid());
			AssertEquals("DaimlerSupplier", mercedesSupplier, ItemSet.MercedesSupplier);
		}

		public void TestChryslerSupplier()
		{
			AssertEquals("ChryslerSupplier", ZGuid.Empty, ItemSet.ChryslerSupplier);
			ZGuid supplier = ZGuid.NewZGuid();
			ItemSet.ChryslerSupplierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, supplier.ToGuid());
			AssertEquals("ChryslerSupplier", supplier.ToGuid(), ItemSet.ChryslerSupplier);
		}

		public void TestMercedesFileNamePrefix()
		{
			AssertEquals("DaimlerFileNamePrefix", "SWTTOWCB", ItemSet.MercedesImportFileNamePrefix);
			ItemSet.MercedesImportFileNamePrefixItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");
			AssertEquals("DaimlerFileNamePrefix", "XXX", ItemSet.MercedesImportFileNamePrefix);
		}

		public void TestChryslerFileNamePrefix()
		{
			AssertEquals("ChryslerFileNamePrefix", "SWTTOCRD", ItemSet.ChryslerImportFileNamePrefix);
			ItemSet.ChryslerImportFileNamePrefixItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ZZZ");
			AssertEquals("ChryslerFileNamePrefix", "ZZZ", ItemSet.ChryslerImportFileNamePrefix);
		}
	}
}
