using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(LoadListPackLine))]
	internal class LoadListPackLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			AssertEquals(9, PackLine.PackageCount);
			AssertEquals("Pallet", PackLine.PackType);
			AssertEquals(10M, PackLine.Volume);
			AssertEquals(30M, PackLine.Weight);
			AssertEquals(0M, PackLine.TotalHazVolume);
			AssertEquals(0M, PackLine.TotalHazWeight);
			AssertEquals("M3", PackLine.VolumeUQ);
			AssertEquals("KG", PackLine.WeightUQ);
			AssertEquals("(L): 0.23  (W): 0.029  (H): 2.34 M", PackLine.Dimensions);
			AssertEquals("WAREHOUSE1,  Packs: 5 PLT\nWAREHOUSE2,  Packs: 4 PLT", PackLine.CargoLocationAndPacks);
			AssertNotNull(PackLine.Shipment);
			AssertNotNull(PackLine.Container);
			AssertEquals("N", PackLine.IsGroupPackLine.ToString());
			AssertEquals(4, PackLine.PackLocations.Count);
			AssertEquals(4, PackLine.PackingOrder);
		}

		public void TestPackageDetails()
		{
			AssertEquals("9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M", PackLine.PackageDetails);
			AssertEquals("", PackLine.PackageDetailsWithHazCat);
			AssertEquals(0M, PackLine.TotalHazVolume);
			AssertEquals(0M, PackLine.TotalHazWeight);

			Pack.JL_RH_NKCommodityCode = "GEN";
			PackLine = new LoadListPackLine(DocPackLines.New(Pack, Factory), DocContainer);
			AssertEquals("9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M", PackLine.PackageDetails);
			AssertEquals("", PackLine.PackageDetailsWithHazCat);
			AssertEquals(0M, PackLine.TotalHazVolume);
			AssertEquals(0M, PackLine.TotalHazWeight);

			Pack.JL_RH_NKCommodityCode = "HAZ";
			UNDGSubstance uNDG = Factory.New<UNDGSubstance>();
			uNDG.DG_UNNO = "9988";
			uNDG.DG_PSN = "SOME SHIPPING NAME";
			uNDG.DG_Class = "3";
			uNDG.DG_PG = "II";
			uNDG.DG_MP = "X";

			UNDGDataItem dGDataItem = Pack.UNDGs.AddNew();
			dGDataItem.DI_DG = uNDG.PK;

			PackLine = new LoadListPackLine(DocPackLines.New(Pack, Factory), DocContainer);
			AssertEquals("9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M", PackLine.PackageDetails);
			ZString expected = "9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M" +
								 "\nUN9988, SOME SHIPPING NAME, class 3, PG II, MARINE POLLUTANT";
			AssertEquals(expected, PackLine.PackageDetailsWithHazCat);
			AssertEquals(30M, PackLine.TotalHazWeight);
			AssertEquals(10M, PackLine.TotalHazVolume);

			PackLine newPack = Factory.New<PackLine>();
			newPack.JL_F3_NKPackType = "BOX";
			newPack.JL_PackageCount = 1;
			newPack.JL_ActualVolume = 1000M;
			newPack.JL_ActualVolumeUQ = "L";
			newPack.JL_ActualWeight = 500M;
			newPack.JL_ActualWeightUQ = "G";
			DocPackLines lineToAdd = DocPackLines.New(newPack, Factory);
			PackLine.AmendExistingPackLine(lineToAdd);

			AssertEquals("9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M" + "\n" +
						 "1 Box 500 G 1000 L", PackLine.PackageDetails);
			AssertEquals(expected, PackLine.PackageDetailsWithHazCat);
			AssertEquals(30M, PackLine.TotalHazWeight);
			AssertEquals(10M, PackLine.TotalHazVolume);

			newPack.JL_RH_NKCommodityCode = "HAZ";
			UNDGSubstance uNDG2 = Factory.New<UNDGSubstance>();
			uNDG2.DG_UNNO = "9900";
			uNDG2.DG_PSN = "OTHER NAME";
			uNDG2.DG_Class = "1";
			uNDG2.DG_PG = "I";
			uNDG2.DG_MP = "";

			UNDGDataItem newDGDataItem = newPack.UNDGs.AddNew();
			newDGDataItem.DI_DG = uNDG2.PK;

			PackLine = new LoadListPackLine(DocPackLines.New(Pack, Factory), DocContainer);
			lineToAdd = DocPackLines.New(newPack, Factory);
			PackLine.AmendExistingPackLine(lineToAdd);

			expected += "\n" + "1 Box 500 G 1000 L";
			expected += "\nUN9900, OTHER NAME, class 1, PG I";
			AssertEquals(expected, PackLine.PackageDetailsWithHazCat);
			AssertEquals(30.5M, PackLine.TotalHazWeight);
			AssertEquals(11M, PackLine.TotalHazVolume);
		}

		public void TestAmendExistingPackLine()
		{
			DocPackLines lineToAdd = CreateNewDocPackLine();
			PackLine.AmendExistingPackLine(lineToAdd);

			AssertEquals(18, PackLine.PackageCount);
			AssertEquals("Pallet", PackLine.PackType);
			AssertEquals(20M, PackLine.Volume);
			AssertEquals(60M, PackLine.Weight);
			AssertEquals("M3", PackLine.VolumeUQ);
			AssertEquals("KG", PackLine.WeightUQ);
			AssertEquals("WAREHOUSE1,  Packs: 5 PLT\nWAREHOUSE2,  Packs: 4 PLT\nWAREHOUSE1,  Packs: 5 PLT\nWAREHOUSE2,  Packs: 4 PLT", PackLine.CargoLocationAndPacks);
			AssertEquals("Y", PackLine.IsGroupPackLine.ToString());
			AssertEquals("9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M\n9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M", PackLine.PackageDetails);
		}

		public void TestAmendExistingPackLineWithDifferentDimensions()
		{
			PackLine newPack = Factory.New<PackLine>();
			newPack.JL_F3_NKPackType = "BOX";
			newPack.JL_PackageCount = 1;
			newPack.JL_ActualVolume = 1000M;
			newPack.JL_ActualVolumeUQ = "L";
			newPack.JL_ActualWeight = 500M;
			newPack.JL_ActualWeightUQ = "G";
			DocPackLines lineToAdd = DocPackLines.New(newPack, Factory);

			PackLine.AmendExistingPackLine(lineToAdd);

			AssertEquals("Packages", PackLine.PackType);
			AssertEquals(11M, PackLine.Volume);
			AssertEquals(30.5M, PackLine.Weight);
			AssertEquals("M3", PackLine.VolumeUQ);
			AssertEquals("KG", PackLine.WeightUQ);
			AssertEquals("WAREHOUSE1,  Packs: 5 PLT\nWAREHOUSE2,  Packs: 4 PLT", PackLine.CargoLocationAndPacks);
			AssertEquals("Y", PackLine.IsGroupPackLine.ToString());
			AssertEquals("9 Pallet 30 KG 10 M3\n   (L): 0.23  (W): 0.029  (H): 2.34 M\n1 Box 500 G 1000 L", PackLine.PackageDetails);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return PackLine;
		}

		protected override void SetUp()
		{
			DocPackLines docPackLine = CreateNewDocPackLine();

			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "CONT1111111";
			DocContainer = DocContainer.New(container, Factory);

			PackLine = new LoadListPackLine(docPackLine, DocContainer);
			base.SetUp();
		}

		protected DocPackLines CreateNewDocPackLine()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			Pack = shipment.OuterPackLines.AddNew();
			Pack.JL_PackageCount = 9;
			Pack.JL_F3_NKPackType = "PLT";
			Pack.JL_Length = 0.23M;
			Pack.JL_Width = 0.029M;
			Pack.JL_Height = 2.34M;
			Pack.JL_UnitOfDimension = "M";
			Pack.JL_ActualVolumeUQ = "M3";
			Pack.JL_ActualWeightUQ = "KG";
			Pack.JL_ActualVolume = 10M;
			Pack.JL_ActualWeight = 30M;
			Pack.JL_ContainerPackingOrder = 4;

			var location1 = Pack.PackLocations.AddNew();
			location1.JQ_NoPackages = 5;
			location1.JQ_WarehouseLocation = "WAREHOUSE1";

			var location2 = Pack.PackLocations.AddNew();
			location2.JQ_NoPackages = 4;
			location2.JQ_WarehouseLocation = "WAREHOUSE2";

			Pack.PackLocations.AddNew();
			Pack.PackLocations.AddNew();

			return DocPackLines.New(Pack, Factory);
		}

		protected LoadListPackLine PackLine;
		protected PackLine Pack;
		protected DocContainer DocContainer;

		#endregion
	}
}
