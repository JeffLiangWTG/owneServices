using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAPivotSynchroniserTest : SynchroniserTestCase
	{
		// much of this is tested in the end 2 end test in CusSCAHouseSynchroniserTest
		public void TestCusSCAPivotSynchroniser()
		{
			var helper = new CusSCATestHelper();
			helper.Shipment.JS_GoodsDescription = "SHIPMENT DESC";

			var source = helper.Shipment.OuterPackLines[0];
			source.JL_PackageCount = new ZInt(5);
			source.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;
			source.JL_ActualWeight = 250m;
			source.JL_ActualWeightUQ = "KG";
			source.JL_ActualVolume = 2.5m;
			source.JL_ActualVolumeUQ = "M3";
			source.JL_HarmonisedCode = "123456";
			source.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			source.JL_DetailedDescription = "DETAILED DESCRIPTION";
			source.JL_MarksAndNumbers = "MARKS";

			var product = source.Products.AddNew();
			product.D2_ProductQuantity = 100;
			product.D2_ProductUnitOfQty = Core.Constants.PkgUnit.Bag;

			var destination = Factory.New<CusSCAPivot>();
			var synchroniser = new CusSCAPivotSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();
			AssertEquals("packs", new ZInt(5), destination.CV_PackageCount);
			AssertEquals("UQ", ACROSSPackageTypes.Codes.BALEBLE, destination.CV_PackageType);
			AssertEquals("weight", 250m, destination.CV_Weight);
			AssertEquals("weightUQ", "KG", destination.CV_WeightUQ);
			AssertEquals("volume", 2.5m, destination.CV_Volume);
			AssertEquals("volume UQ", "M3", destination.CV_VolumeUQ);
			AssertEquals("tariff", "123456", destination.CV_HarmonisedTariffNums);
			AssertEquals("description", "DETAILED DESCRIPTION", destination.CV_GoodsDescription);
			AssertEquals("marks", "MARKS", destination.CV_MarksAndNumbers);
			AssertEquals("DG count", 1, destination.UNDGs.Count);
			AssertEquals("DG substance", "0004a", destination.UNDGs[0].UNDGSubstance.DG_Code);

			source.JL_PackageCount = new ZInt(6);
			AssertEquals("packs", new ZInt(6), destination.CV_PackageCount);
			source.JL_DetailedDescription = new string('A', destination.CV_GoodsDescriptionInfo.MaxLength + 10);
			AssertEquals("long description", new string('A', destination.CV_GoodsDescriptionInfo.MaxLength), destination.CV_GoodsDescription);
			source.JL_MarksAndNumbers = new string('A', destination.CV_MarksAndNumbersInfo.MaxLength + 10);
			AssertEquals("long description", new string('A', destination.CV_MarksAndNumbersInfo.MaxLength), destination.CV_MarksAndNumbers);

			source.JL_PackageCount = 0;
			helper.Shipment.JS_TotalPackageCount = 105;
			helper.Shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Carton;
			synchroniser.Synchronise();
			AssertEquals("packs", new ZInt(105), destination.CV_PackageCount);
			AssertEquals("UQ", Core.Constants.PkgUnit.Carton, destination.CV_PackageType);
		}

		public void TestCusSCAPivotCollectionSynchroniser_FromInnerPackLineAndOuterPackLine()
		{
			var helper = new CusSCATestHelper();
			var shipment = helper.Shipment;
			var outerPackLine = shipment.OuterPackLines.AddNew();
			outerPackLine.JL_PackageCount = 100;
			var innerPackLine = shipment.InnerPackLines.AddNew();
			innerPackLine.JL_JL_OuterPackLine = outerPackLine.PK;
			innerPackLine.JL_PackageCount = 200;
			var item = helper.House.PackLines.AddNew();
			var synchroniser = new CusSCAPivotSynchroniser(item, innerPackLine, outerPackLine);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("quantity from inner", 200, item.CV_PackageCount);
			innerPackLine.JL_PackageCount = 201;
			AssertEquals("quantity update from inner", 201, item.CV_PackageCount);

			outerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			innerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Coil;
			AssertEquals("unit from inner", ACROSSPackageTypes.Codes.COIL, item.CV_PackageType);
			innerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Drum;
			AssertEquals("unit update from inner", ACROSSPackageTypes.Codes.DRUM, item.CV_PackageType);

			outerPackLine.JL_ActualWeight = 10;
			outerPackLine.JL_ActualWeightUQ = "K";
			innerPackLine.JL_ActualWeight = 20;
			innerPackLine.JL_ActualWeightUQ = "KG";
			AssertEquals("weight from inner", 20m, item.CV_Weight);
			AssertEquals("weight uq from inner", "KG", item.CV_WeightUQ);
			innerPackLine.JL_ActualWeight = 21;
			innerPackLine.JL_ActualWeightUQ = "MG";
			AssertEquals("weight update from inner", 21m, item.CV_Weight);
			AssertEquals("weight uq update from inner", "MG", item.CV_WeightUQ);

			outerPackLine.JL_ActualVolume = 30;
			outerPackLine.JL_ActualVolumeUQ = "M1";
			innerPackLine.JL_ActualVolume = 40;
			innerPackLine.JL_ActualVolumeUQ = "M2";
			AssertEquals("volume from inner", 40m, item.CV_Volume);
			AssertEquals("volume uq from inner", "M2", item.CV_VolumeUQ);
			innerPackLine.JL_ActualVolume = 41;
			innerPackLine.JL_ActualVolumeUQ = "M3";
			AssertEquals("volume update from inner", 41m, item.CV_Volume);
			AssertEquals("volume uq update from inner", "M3", item.CV_VolumeUQ);

			outerPackLine.JL_HarmonisedCode = "111111";
			innerPackLine.JL_HarmonisedCode = "222222";
			AssertEquals("hs code from outer", "111111", item.CV_HarmonisedTariffNums);
			outerPackLine.JL_HarmonisedCode = "333333";
			AssertEquals("hs code update from outer", "333333", item.CV_HarmonisedTariffNums);

			shipment.JS_GoodsDescription = "SHIPMENT GOODS DESC";
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT DETAIL DESC";
			innerPackLine.JL_Description = "INNER DESC";
			outerPackLine.JL_Description = "OUTER DESC";
			AssertEquals("INNER DESC", item.CV_GoodsDescription);
			innerPackLine.JL_Description = ZString.Empty;
			AssertEquals("OUTER DESC", item.CV_GoodsDescription);

			shipment.JS_MarksAndNumbers = "SHIPMENT MARKS";
			outerPackLine.JL_MarksAndNumbers = "OUTER PACK MARKS";
			innerPackLine.JL_MarksAndNumbers = "INNER PACK MARKS";
			AssertEquals("OUTER PACK MARKS", item.CV_MarksAndNumbers);
			outerPackLine.JL_MarksAndNumbers = ZString.Empty;
			AssertEquals("SHIPMENT MARKS", item.CV_MarksAndNumbers);
			shipment.JS_MarksAndNumbers = ZString.Empty;
			AssertEquals("marks from outer", ZString.Empty, item.CV_MarksAndNumbers);

			outerPackLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("DG from outer", 1, item.UNDGs.Count);
			AssertEquals("DG substance from outer", "0004a", item.UNDGs[0].UNDGSubstance.DG_Code);
		}

		public void TestSynchroniseContainerNumFromOceanBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			var helper = new CusSCATestHelper();
			var packLine = helper.Shipment.OuterPackLines.AddNew();
			var packLine1 = helper.House.PackLines.AddNew();
			var container = packLine.Containers.AddNew();
			container.JC_ContainerNum = "TESTCONT1";
			var oceanBill = packLine1.OceanBill;
			oceanBill.CB_ParentTableCode = "JK";
			oceanBill.CB_ParentId = consol.PK;
			container.JC_JK = consol.PK;
			var container1 = packLine1.OceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "TESTCONT1";
			var synchroniser = new CusSCAPivotSynchroniser(packLine1, packLine);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals(container1.PK, packLine1.CV_CN);
		}

		public void TestSynchroniseContainerNumFromOceanBill_FromInnerPackLineAndOuterPackLine()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			var helper = new CusSCATestHelper();
			var shipment = helper.Shipment;
			var outerPackLine = shipment.OuterPackLines.AddNew();
			outerPackLine.JL_PackageCount = 100;

			var innerPackLine = shipment.InnerPackLines.AddNew();
			innerPackLine.JL_JL_OuterPackLine = outerPackLine.PK;
			innerPackLine.JL_PackageCount = 200;

			var item = helper.House.PackLines.AddNew();
			var container = outerPackLine.Containers.AddNew();
			container.JC_ContainerNum = "TESTCONT1";
			container.JC_JK = consol.PK;

			var oceanBill = item.OceanBill;
			oceanBill.CB_ParentTableCode = "JK";
			oceanBill.CB_ParentId = consol.PK;

			var container1 = item.OceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "TESTCONT1";

			var synchroniser = new CusSCAPivotSynchroniser(item, innerPackLine, outerPackLine);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals(container1.PK, item.CV_CN);
		}

		public void TestSynchroniseWhenContainerNumInfoChanged()
		{
			var helper = new CusSCATestHelper();
			var shipment = helper.Shipment;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = new ZInt(5);
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;
			var container1 = packLine1.Containers.AddNew();
			container1.JC_ContainerNum = "TESTCONT1";
			var item = helper.House.PackLines.AddNew();
			var synchroniser1 = new CusSCAPivotSynchroniser(item, packLine1);
			synchroniser1.SetEnabled(true, false);
			synchroniser1.Synchronise();
			AssertEquals(new ZInt(5), item.CV_PackageCount);
			AssertEquals(ACROSSPackageTypes.Codes.BALEBLE, item.CV_PackageType);

			synchroniser1.SetEnabled(false, false);
			packLine1.JL_PackageCount = new ZInt(6);
			AssertEquals(new ZInt(5), item.CV_PackageCount);

			synchroniser1.SetEnabled(true, false);
			AssertEquals(new ZInt(5), item.CV_PackageCount);

			container1.JC_ContainerNum = "TESTCONT2";
			AssertEquals(new ZInt(6), item.CV_PackageCount);

			synchroniser1.SetEnabled(false, false);
			packLine1.JL_PackageCount = new ZInt(7);
			var container2 = packLine1.Containers.AddNew();
			container2.JC_ContainerNum = "TESTCONT3";
			AssertEquals(new ZInt(6), item.CV_PackageCount);

			synchroniser1.SetEnabled(true, false);
			container2.JC_ContainerNum = "TESTCONT4";
			AssertEquals(new ZInt(7), item.CV_PackageCount);
		}

		public void TestSynchroniseWhenContainerNumInfoChanged_FromInnerPackLineAndOuterPackLine()
		{
			var helper = new CusSCATestHelper();
			var shipment = helper.Shipment;
			var outerPackLine = shipment.OuterPackLines.AddNew();
			outerPackLine.JL_PackageCount = 1;

			var innerPackLine = shipment.InnerPackLines.AddNew();
			innerPackLine.JL_JL_OuterPackLine = outerPackLine.PK;
			innerPackLine.JL_PackageCount = 5;
			innerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;

			var container1 = outerPackLine.Containers.AddNew();
			container1.JC_ContainerNum = "TESTCONT1";

			var item = helper.House.PackLines.AddNew();
			var synchroniser1 = new CusSCAPivotSynchroniser(item, innerPackLine, outerPackLine);
			synchroniser1.SetEnabled(true, false);
			synchroniser1.Synchronise();
			AssertEquals(new ZInt(5), item.CV_PackageCount);
			AssertEquals(ACROSSPackageTypes.Codes.BALEBLE, item.CV_PackageType);

			synchroniser1.SetEnabled(false, false);
			innerPackLine.JL_PackageCount = new ZInt(6);
			AssertEquals(new ZInt(5), item.CV_PackageCount);

			synchroniser1.SetEnabled(true, false);
			AssertEquals(new ZInt(5), item.CV_PackageCount);

			container1.JC_ContainerNum = "TESTCONT2";
			AssertEquals(new ZInt(6), item.CV_PackageCount);

			synchroniser1.SetEnabled(false, false);
			innerPackLine.JL_PackageCount = new ZInt(7);
			var container2 = outerPackLine.Containers.AddNew();
			container2.JC_ContainerNum = "TESTCONT3";
			AssertEquals(new ZInt(6), item.CV_PackageCount);

			synchroniser1.SetEnabled(true, false);
			container2.JC_ContainerNum = "TESTCONT4";
			AssertEquals(new ZInt(7), item.CV_PackageCount);
		}
	}
}
