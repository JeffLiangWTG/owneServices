using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageLinkPackage))]
	public class TemporaryStorageLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.TemporaryStorageLinkPackages.AddNew();
			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";
			linkPackage.Package = pack;

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = linkPackage.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		public void TestTariff()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory);
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();

			CombineAssertions(() =>
			{
				packedItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				var tariff = packedItem.Tariff;
				AssertEquals("Tariff is loaded", TemporaryStorageTestHelper.TestTariffCode, tariff.ZZ1_TariffCode);

				packedItem.API_Tariff = "ABC";
				AssertNull("Invalid tariff", packedItem.Tariff);

				packedItem.API_Tariff = "030479";
				var tariffFromShortTariffCode = packedItem.Tariff;
				AssertEquals("Tariff is the same for short code", tariff.ZZ1_TariffCode, tariffFromShortTariffCode.ZZ1_TariffCode);
			});
		}

		public void TestPackage()
		{
			var linkPackage = (TemporaryStorageLinkPackage)GetNewBusinessObject();
			AssertEquals("Package should equal Pack entered via the setter.", pack, linkPackage.Package);
		}

		public void TestPackageNumber()
		{
			var linkPackage = (TemporaryStorageLinkPackage)GetNewBusinessObject();

			var package = linkPackage.Package;
			package.APA_PackUQ = "BX";
			CombineAssertions(() =>
			{
				AssertEquals("PackageNumber should be APA_PackUQ + marked APA_MarksAndNumbers if marks but no container.", "BX marked mark", linkPackage.PackageNumber);

				var container = Factory.New<TemporaryStorageContainer>();
				container.ACN_ContainerNumber = "CONTAINER";
				package.ContainerPK = container.PK;
				AssertEquals("PackageNumber should be APA_PackUQ + in ACN_ContainerNumber + marked APA_MarksAndNumbers if marks and container.", "BX in CONTAINER marked mark", linkPackage.PackageNumber);

				package.APA_MarksAndNumbers = ZString.Empty;
				AssertEquals("PackageNumber should be APA_PackUQ + in ACN_ContainerNumber if container and no marks.", "BX in CONTAINER", linkPackage.PackageNumber);

				container.ACN_ContainerNumber = ZString.Empty;
				AssertEquals("PackageNumber should be APA_PackUQ if no marks and no container.", "BX", linkPackage.PackageNumber);
			});
		}

		public void TestPackQty()
		{
			var linkPackage = (TemporaryStorageLinkPackage)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("PackQty should be 0 if no linked.", ZInt.Zero, linkPackage.PackQty);

				linkPackage.IsLinked = ZBool.True;
				AssertEquals("PackQty should be equal pack APA_PackQty if linked.", 10, linkPackage.PackQty);

				packedItem.API_LineNo = 1;
				var bill = packedItem.Bill;
				var newPackedItem = bill.PackedItems.AddNew();
				newPackedItem.API_LineNo = 2;
				var linkPackage2 = newPackedItem.TemporaryStorageLinkPackages.AddNew();
				linkPackage2.Package = pack;
				linkPackage2.IsLinked = ZBool.True;
				AssertEquals("PackQty should be equal pack APA_PackQty if is linked to a after item", 10, linkPackage.PackQty);
				AssertEquals("PackQty should be 0 if is linked to a previous item", ZInt.Zero, linkPackage2.PackQty);
			});
		}

		public void TestIsLinked()
		{
			var linkPackage = (TemporaryStorageLinkPackage)GetNewBusinessObject();
			AssertEquals("Prerequisite:", 0, packedItem.PackagesPivot.Count);
			Assert(!linkPackage.IsLinked);
			linkPackage.IsLinked = true;
			AssertEquals("A pivot should have been created.", 1, packedItem.PackagesPivot.Count);
			Assert(linkPackage.IsLinked);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var temporaryStorage = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = temporaryStorage.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
			var linkPackage = packedItem.TemporaryStorageLinkPackages.AddNew();
			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";
			linkPackage.Package = pack;
			return linkPackage;
		}

		TemporaryStoragePack pack;
		TemporaryStoragePackedItem packedItem;
	}
}
