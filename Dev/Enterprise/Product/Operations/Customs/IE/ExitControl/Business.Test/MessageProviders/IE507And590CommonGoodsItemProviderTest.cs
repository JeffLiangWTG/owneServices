using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	sealed class IE507And590CommonGoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<IE507And590CommonGoodsItemProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("consignmentItem could not be null", () => new IE507And590CommonGoodsItemProvider(null, 100m, 10m, Enumerable.Empty<(ZString packageType, int? packageQuantity, ZString shippingMarks)>()));
			AssertExceptionThrown<ArgumentNullException>("consignmentItem could not be null", () => new IE507And590CommonGoodsItemProvider(Factory.New<CusExitConsignmentItem>(), 100m, 10m, null));
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("GoodsItemNumber", (short)10, Provider.GoodsItemNumber);
		}

		public void TestGrossMass()
		{
			AssertEquals("GrossMass", 100m, Provider.GrossMass);
		}

		public void TestNetMass()
		{
			AssertEquals("NetMass", 10m, Provider.NetMass);
		}

		public void TestPackages()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();

			CombineAssertions("Empty Packages", () =>
			{
				consignmentItem1.CCI_LineNumber = 10;
				var provider = new IE507And590CommonGoodsItemProvider(consignmentItem1, 100m, 10m, Enumerable.Empty<(ZString packageType, int? packageQuantity, ZString shippingMarks)>());
				AssertEquals("No Packages", false, provider.Packages.Any());

				consignmentItem1.CCI_LineNumber = 6;
				provider = new IE507And590CommonGoodsItemProvider(consignmentItem1, 150m, 15m, new[] { (new ZString("P1"), 50, new ZString("MARK1")), (new ZString("P2"), 60, new ZString("MARK2")), (new ZString("P3"), (int?)0, new ZString("MARK3")) });
				var packages = provider.Packages.ToArray();
				AssertEquals("Length", 3, packages.Length);
				AssertPackage(packages[0], "P1", 50, "MARK1");
				AssertPackage(packages[1], "P2", 60, "MARK2");
				AssertPackage(packages[2], "P3", 0, "MARK3");
			});
		}

		public void TestPackages_BulkType()
		{
			EU.ExitControl.Business.Testing.CusExitConsignmentPackageTestHelper.SetupBulkCusCode(Factory);
			var exitHeader = Factory.New<CusExitHeader>();
			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();

			CombineAssertions("Bulk Type Packages", () =>
			{
				consignmentItem1.CCI_LineNumber = 88;
				var provider = new IE507And590CommonGoodsItemProvider(consignmentItem1, 150m, 15m, new[] { (new ZString("VG"), null, new ZString("MARK1")), (new ZString("NE"), (int?)0, new ZString("MARK2")) });
				var packages = provider.Packages.ToArray();
				AssertEquals("Length", 2, packages.Length);
				AssertPackage(packages[0], "VG", 0, "MARK1");
				AssertPackage(packages[1], "NE", 0, "MARK2");
			});
		}

		void AssertPackage(IPackaging packaging, string packageType, int packageQuantity, string shippingMarks)
		{
			AssertEquals("PackageType", packageType, packaging.PackageType);
			AssertEquals("PackageQuantity", packageQuantity, packaging.PackageQuantity);
			AssertEquals("ShippingMarks", shippingMarks, packaging.ShippingMarks);
		}

		protected override IE507And590CommonGoodsItemProvider GetProvider()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();
			consignmentItem1.CCI_LineNumber = 10;
			return new IE507And590CommonGoodsItemProvider(consignmentItem1, 100m, 10m, Enumerable.Empty<(ZString packageType, int? packageQuantity, ZString shippingMarks)>());
		}
	}
}
