using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAPivotCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestCusSCAPivotCollectionSynchroniser()
		{
			var helper = new CusSCATestHelper();
			helper.Shipment.JS_GoodsDescription = "SHIPMENT DESC";
			var packLine1 = helper.Shipment.OuterPackLines[0];
			packLine1.JL_PackageCount = new ZInt(1);
			packLine1.JL_DetailedDescription = "LINE 1";
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;

			var product = packLine1.Products.AddNew();
			product.D2_ProductQuantity = 100;
			product.D2_ProductUnitOfQty = Core.Constants.PkgUnit.Bag;

			var undg = packLine1.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			undg.DI_DGFlashPoint = 1m;
			var packLine2 = helper.Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = new ZInt(2);
			packLine2.JL_DetailedDescription = "LINE 2";
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			var packLine3 = helper.Shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = new ZInt(3);
			packLine3.JL_DetailedDescription = "LINE 3";
			packLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;

			var destination = helper.OceanBill.HouseBills.AddNew();
			destination.CA_JS = helper.Shipment.PK;
			var c1 = helper.OceanBill.Containers.AddNew();
			c1.CN_ContainerNumber = "NCT";
			var destPackLine1 = destination.PackLines.AddNew();
			destPackLine1.CV_PackageCount = new ZInt(1);
			destPackLine1.CV_GoodsDescription = "LINE 1";
			destPackLine1.CV_PackageType = ACROSSPackageTypes.Codes.BALEBLE;
			destPackLine1.CV_CN = c1.PK;
			var destUndg = destPackLine1.UNDGs.AddNew();
			destUndg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			destUndg.DI_DGFlashPoint = 1m;
			var destPackLine2 = destination.PackLines.AddNew();
			destPackLine2.CV_PackageCount = new ZInt(2);
			destPackLine2.CV_GoodsDescription = "LINE 2";
			destPackLine2.CV_PackageType = ACROSSPackageTypes.Codes.BAG;
			destPackLine2.CV_CN = c1.PK;

			var synchroniser = new CusSCAPivotCollectionSynchroniser(destination, helper.Shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("should be 3 synchronised lines", 3, destination.PackLines.Count);
			Assert(!destPackLine1.IsDeleted);
			Assert(destPackLine2.IsDeleted);
			Assert(DestinationContains(destination.PackLines, "LINE 1"));
			Assert(DestinationContains(destination.PackLines, "LINE 2"));
			Assert(DestinationContains(destination.PackLines, "LINE 3"));

			var packLine4 = helper.Shipment.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = new ZInt(4);
			packLine4.JL_DetailedDescription = "LINE 4";
			packLine4.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			AssertEquals("now 4 synchronised lines", 4, destination.PackLines.Count);
			var extraLine = FindItemInDestination(destination.PackLines, "LINE 4");
			AssertNotNull(extraLine);
			packLine4.JL_PackageCount = new ZInt(5);
			AssertEquals(5, extraLine.CV_PackageCount);
			packLine4.Delete();
			AssertEquals("now 3 synchronised lines", 3, destination.PackLines.Count);
			Assert(extraLine.IsDeleted);

			packLine1.JL_PackageCount = 0;
			var shipment = helper.Shipment;
			shipment.JS_TotalPackageCount = 105;
			shipment.JS_F3_NKTotalCountPackType = "CTN";
			packLine2.Delete();
			packLine3.Delete();
			packLine4.Delete();
			AssertEquals(1, destination.PackLines.Count);
			AssertEquals(105, destination.PackLines[0].CV_PackageCount);
			AssertEquals("CTN", destination.PackLines[0].CV_PackageType);
		}

		public void TestCusSCAPivotCollectionSynchroniser_FromInnerPackLineAndOuterPackLine()
		{
			var helper = new CusSCATestHelper();
			var shipment = helper.Shipment;
			var outerPackLine1 = shipment.OuterPackLines.AddNew();
			outerPackLine1.JL_PackageCount = 101;
			outerPackLine1.JL_DetailedDescription = "LINE 11";
			var outerPackLine2 = shipment.OuterPackLines.AddNew();
			outerPackLine2.JL_PackageCount = 102;
			outerPackLine2.JL_DetailedDescription = "LINE 12";
			var innerPackLine1 = shipment.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 201;
			innerPackLine1.JL_DetailedDescription = "LINE 21";
			innerPackLine1.JL_JL_OuterPackLine = outerPackLine1.PK;
			var innerPackLine2 = shipment.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 202;
			innerPackLine2.JL_DetailedDescription = "LINE 22";
			innerPackLine2.JL_JL_OuterPackLine = outerPackLine1.PK;

			var destination = helper.OceanBill.HouseBills.AddNew();
			var synchroniser = new CusSCAPivotCollectionSynchroniser(destination, helper.Shipment);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("2 inner 1 outer, total 3", 3, destination.PackLines.Count);
			AssertNotNull("21 from inner pack line", DestinationContains(destination.PackLines, "LINE 21"));
			AssertNotNull("22 from inner pack line", DestinationContains(destination.PackLines, "LINE 22"));
			AssertNotNull("12 from outer pack line", DestinationContains(destination.PackLines, "LINE 12"));

			innerPackLine2.JL_JL_OuterPackLine = outerPackLine2.PK;
			synchroniser.Synchronise();
			AssertEquals("2 inner, total 2", 2, destination.PackLines.Count);
			AssertNotNull("21 from inner pack line", DestinationContains(destination.PackLines, "LINE 21"));
			AssertNotNull("22 from inner pack line", DestinationContains(destination.PackLines, "LINE 22"));

			shipment.InnerPackLines.Remove(innerPackLine1);
			synchroniser.Synchronise();
			AssertEquals("1 inner 1 outer, total 2", 2, destination.PackLines.Count);
			AssertNotNull("22 from inner pack line", DestinationContains(destination.PackLines, "LINE 22"));
			AssertNotNull("11 from outer pack line", DestinationContains(destination.PackLines, "LINE 11"));

			shipment.InnerPackLines.Remove(innerPackLine2);
			synchroniser.Synchronise();
			AssertEquals("2 outer, total 2", 2, destination.PackLines.Count);
			AssertNotNull("11 from outer pack line", DestinationContains(destination.PackLines, "LINE 11"));
			AssertNotNull("12 from outer pack line", DestinationContains(destination.PackLines, "LINE 12"));
		}

		bool DestinationContains(CusSCAPivotCollectionForHouse destinationCollection, ZString description)
		{
			return FindItemInDestination(destinationCollection, description) != null;
		}

		CusSCAPivot FindItemInDestination(CusSCAPivotCollectionForHouse destinationCollection, ZString description)
		{
			return destinationCollection.Find(new ZQuery(CusSCAPivotSchema.CV_GoodsDescription, description)).FirstOrDefault();
		}
	}
}
