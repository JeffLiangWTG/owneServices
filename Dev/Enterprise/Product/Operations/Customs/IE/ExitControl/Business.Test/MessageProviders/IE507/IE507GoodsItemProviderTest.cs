using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	public class IE507GoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<IE507GoodsItemProvider, IIE507GoodsItem>
	{
		public void TestAuthorisations()
		{
			var usage1 = consignmentItem.CusAuthorizationUsages.AddNew();
			usage1.AGC_Number = "AC1";
			var usage2 = consignmentItem.CusAuthorizationUsages.AddNew();
			usage2.AGC_Number = "AC2";
			var authorisations = IProvider.Authorisations.ToArray();
			AssertEquals("Authorisations", 2, authorisations.Length);
			AssertEquals("authorisations[0].UCR", "AC1", authorisations[0].UCR);
			AssertEquals("authorisations[1].UCR", "AC2", authorisations[1].UCR);
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("GoodsItemNumber", (short)10, IProvider.GoodsItemNumber);
		}

		public void TestGrossMass()
		{
			AssertEquals("GrossMass", 100m, IProvider.GrossMass);
		}

		public void TestNetMass()
		{
			AssertEquals("NetMass", 90m, IProvider.NetMass);
		}

		public void TestPackages()
		{
			var packages = Provider.Packages.ToArray();
			AssertEquals("Length", 2, packages.Length);
			AssertPackage(packages[0], "P1", 50, "MARK1");
			AssertPackage(packages[1], "P2", 60, "MARK2");
		}

		void AssertPackage(IPackaging packaging, string packageType, int? packageQuantity, string shippingMarks)
		{
			AssertEquals("PackageType", packageType, packaging.PackageType);
			AssertEquals("PackageQuantity", packageQuantity, packaging.PackageQuantity);
			AssertEquals("ShippingMarks", shippingMarks, packaging.ShippingMarks);
		}

		protected override IE507GoodsItemProvider GetProvider() => new IE507GoodsItemProvider(consignmentItem, 100m, 90m, new[] { (new ZString("P1"), 50, new ZString("MARK1")), (new ZString("P2"), (int?)60, new ZString("MARK2")) });

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 10;
		}
		CusExitHeader header;
		CusExitConsignment consignment;
		CusExitConsignmentItem consignmentItem;
	}
}
