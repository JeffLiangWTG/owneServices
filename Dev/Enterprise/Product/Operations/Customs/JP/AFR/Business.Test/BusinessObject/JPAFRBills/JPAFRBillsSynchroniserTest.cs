using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRBillsSynchroniserTest : AFRSynchroniserTestCase
	{
		public void TestSynchroniseJPB_BillNumber()
		{
			shipment.JS_HouseBill = "ABCD1234";
			AssertEquals("ABCD1234", bill.JPB_BillNumber);

			shipment.JS_HouseBill = "JDSD4343";
			AssertEquals("JDSD4343", bill.JPB_BillNumber);

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			shipment.JS_HouseBill = "OTT16789";
			AssertEquals("JDSD4343", bill.JPB_BillNumber);

			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			AssertEquals("OTT16789", bill.JPB_BillNumber);

			synchroniser.SetEnabled(false, false);
			shipment.JS_HouseBill = "OTT16712";
			AssertEquals("OTT16789", bill.JPB_BillNumber);
		}

		public void TestSynchroniseJPB_BillNumber_AFRNVOCCIDtoAddtoBillDuringSync()
		{
			synchroniser.SetEnabled(true, false);
			using (JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetTemporaryValue(bill.Header.RegistryCompanyPK, Guid.Empty, Guid.Empty, "J07J"))
			{
				shipment.JS_HouseBill = "12345678901234567890";
				AssertEquals("J07J12345678901234567890", bill.JPB_BillNumber);

				shipment.JS_HouseBill = "J07J1234567890123456";
				AssertEquals("J07J1234567890123456", bill.JPB_BillNumber);
			}

			using (JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetTemporaryValue(bill.Header.RegistryCompanyPK, Guid.Empty, Guid.Empty, "J07"))
			{
				shipment.JS_HouseBill = "12345678901234567890";
				AssertEquals("J07-12345678901234567890", bill.JPB_BillNumber);
			}
		}

		public void TestSynchroniseJPB_RL_NKOrigin()
		{
			shipment.JS_RL_NKOrigin = AUSYD.RL_Code;
			AssertEquals(AUSYD.RL_Code, bill.JPB_RL_NKOrigin);

			shipment.JS_RL_NKOrigin = SGSIN.RL_Code;
			AssertEquals(SGSIN.RL_Code, bill.JPB_RL_NKOrigin);

			synchroniser.SetEnabled(false, false);
			shipment.JS_RL_NKOrigin = AUMEL.RL_Code;
			AssertEquals(SGSIN.RL_Code, bill.JPB_RL_NKOrigin);
		}

		public void TestSynchroniseJPB_RL_NKDelivery()
		{
			shipment.JS_RL_NKDestination = JPTKY.RL_Code;
			AssertEquals(JPTKY.RL_Code, bill.JPB_RL_NKDelivery);

			shipment.JS_RL_NKDestination = JPHAO.RL_Code;
			AssertEquals(JPHAO.RL_Code, bill.JPB_RL_NKDelivery);

			synchroniser.SetEnabled(false, false);
			shipment.JS_RL_NKDestination = JPTKY.RL_Code;
			AssertEquals(JPHAO.RL_Code, bill.JPB_RL_NKDelivery);
		}

		public void TestSynchroniseJPB_RL_NKFinalDestination()
		{
			shipment.JS_RL_NKDestination = JPTKY.RL_Code;
			AssertEquals(JPTKY.RL_Code, bill.JPB_RL_NKFinalDestination);

			shipment.JS_RL_NKDestination = JPHAO.RL_Code;
			AssertEquals(JPHAO.RL_Code, bill.JPB_RL_NKFinalDestination);

			synchroniser.SetEnabled(false, false);
			shipment.JS_RL_NKDestination = JPTKY.RL_Code;
			AssertEquals(JPHAO.RL_Code, bill.JPB_RL_NKFinalDestination);
		}

		public void TestSynchronisePacklineData()
		{
			AssertEquals(ZString.Empty, bill.JPB_Tariff);
			AssertEquals(ZString.Empty, bill.JPB_RN_NKGoodsOrigin);

			var packline1 = shipment.OuterPackLines.AddNew();
			AssertEquals(ZString.Empty, bill.JPB_Tariff);

			packline1.JL_HarmonisedCode = "1020.3040";
			AssertEquals("102030", bill.JPB_Tariff);

			packline1.JL_LinePrice = 100m;
			packline1.JL_ActualWeight = 100m;
			packline1.JL_PackageCount = 100;
			AssertEquals("102030", bill.JPB_Tariff);

			var packline2 = shipment.OuterPackLines.AddNew();
			AssertEquals("102030", bill.JPB_Tariff);

			packline2.JL_LinePrice = 200m;
			packline2.JL_ActualWeight = 200m;
			packline2.JL_PackageCount = 200;
			AssertEquals("102030", bill.JPB_Tariff);

			packline2.JL_HarmonisedCode = "20.304050";
			AssertEquals("203040", bill.JPB_Tariff);

			packline2.JL_LinePrice = 50m;
			AssertEquals("102030", bill.JPB_Tariff);

			packline2.JL_LinePrice = 100m;
			AssertEquals("203040", bill.JPB_Tariff);

			packline2.JL_ActualWeight = 50m;
			AssertEquals("102030", bill.JPB_Tariff);

			packline2.JL_ActualWeight = 100m;
			AssertEquals("203040", bill.JPB_Tariff);

			packline2.JL_PackageCount = 50;
			AssertEquals("102030", bill.JPB_Tariff);

			packline2.JL_PackageCount = 100;
			AssertEquals("203040", bill.JPB_Tariff);

			packline2.JL_HarmonisedCode = "1010.3040";
			AssertEquals("102030", bill.JPB_Tariff);

			packline1.Delete();
			AssertEquals("101030", bill.JPB_Tariff);

			synchroniser.SetEnabled(false, false);
			packline2.JL_HarmonisedCode = "1020.3040";
			AssertEquals("101030", bill.JPB_Tariff);
		}

		public void TestSynchroniseGrossWeight()
		{
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			shipment.JS_ActualWeight = 110m;
			AssertEquals(110m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, bill.JPB_GrossWeightUQ);

			shipment.JS_ActualWeight = 0m;
			AssertEquals(0m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, bill.JPB_GrossWeightUQ);

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_ActualWeight = 110m;
			AssertEquals(110m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.JPB_GrossWeightUQ);

			shipment.JS_ActualWeight = 0m;
			AssertEquals(0m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.JPB_GrossWeightUQ);

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 110m;
			AssertEquals(110m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Pounds, bill.JPB_GrossWeightUQ);

			shipment.JS_ActualWeight = 0m;
			AssertEquals(0m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Pounds, bill.JPB_GrossWeightUQ);

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Grams;
			AssertEquals(0m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.JPB_GrossWeightUQ);

			shipment.JS_ActualWeight = 110m;
			AssertEquals(0.11m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.JPB_GrossWeightUQ);

			synchroniser.SetEnabled(false, false);
			shipment.JS_ActualWeight = 0m;
			AssertEquals(0.11m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.JPB_GrossWeightUQ);

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals(0.11m, bill.JPB_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.JPB_GrossWeightUQ);
		}

		public void TestSynchroniseJVolume()
		{
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment.JS_ActualVolume = 110m;
			AssertEquals(110m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, bill.JPB_VolumeUQ);

			shipment.JS_ActualVolume = 0m;
			AssertEquals(0m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, bill.JPB_VolumeUQ);

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals(0m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, bill.JPB_VolumeUQ);

			shipment.JS_ActualVolume = 110m;
			AssertEquals(110m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, bill.JPB_VolumeUQ);

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			AssertEquals(0.11m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, bill.JPB_VolumeUQ);

			shipment.JS_ActualVolume = 100m;
			AssertEquals(0.1m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, bill.JPB_VolumeUQ);

			synchroniser.SetEnabled(false, false);
			shipment.JS_ActualVolume = 0m;
			AssertEquals(0.1m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, bill.JPB_VolumeUQ);

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals(0.1m, bill.JPB_Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, bill.JPB_VolumeUQ);
		}

		public void TestSynchroniseJPB_ManifestQty()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "AFR";
			pack1.RP_CustomsCountry = "JP";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";
			Factory.Save();

			shipment.JS_F3_NKPackType = "DIZ";
			shipment.JS_OuterPacks = 3;
			AssertEquals(30, bill.JPB_ManifestQty);

			shipment.JS_F3_NKPackType = "BAG";
			AssertEquals(3, bill.JPB_ManifestQty);

			shipment.JS_OuterPacks = 0;
			AssertEquals(0, bill.JPB_ManifestQty);

			shipment.JS_OuterPacks = 10;
			AssertEquals(10, bill.JPB_ManifestQty);

			synchroniser.SetEnabled(false, false);
			shipment.JS_OuterPacks = 1;
			AssertEquals(10, bill.JPB_ManifestQty);
		}

		public void TestSynchroniseJPB_ManifestUQ()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "AFR";
			pack1.RP_CustomsCountry = "JP";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";
			Factory.Save();

			SynchroniseJPB_ManifestUQSingleTest("DIZ", "NO");

			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Bag, PackageTypeList.Codes.Bag);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BaleCompressed, PackageTypeList.Codes.BaleCompressed);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BaleUncompressed, PackageTypeList.Codes.BaleNonCompressed);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Basket, PackageTypeList.Codes.Basket);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Box, PackageTypeList.Codes.Box);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Bundle, PackageTypeList.Codes.Bundle);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Carton, PackageTypeList.Codes.Carton);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Case, PackageTypeList.Codes.Case);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Coil, PackageTypeList.Codes.Coil);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Container, PackageTypeList.Codes.Container);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Crate, PackageTypeList.Codes.Crate);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Cylinder, PackageTypeList.Codes.Cylinder);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Drum, PackageTypeList.Codes.Drum);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Keg, PackageTypeList.Codes.Keg);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Package, PackageTypeList.Codes.Package);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Pail, PackageTypeList.Codes.Pail);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Pallet, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Piece, PackageTypeList.Codes.Piece);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Reel, PackageTypeList.Codes.Reel);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Roll, PackageTypeList.Codes.Roll);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Sheet, PackageTypeList.Codes.Sheet);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Skid, PackageTypeList.Codes.Skid);

			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Bottle, Core.Constants.PkgUnit.Bottle);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BreakBulk, Core.Constants.PkgUnit.BreakBulk);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BulkBag, Core.Constants.PkgUnit.BulkBag);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Cradle, Core.Constants.PkgUnit.Cradle);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Dozen, Core.Constants.PkgUnit.Dozen);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Envelope, Core.Constants.PkgUnit.Envelope);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Gross, Core.Constants.PkgUnit.Gross);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Mix, Core.Constants.PkgUnit.Mix);

			SynchroniseJPB_ManifestUQSingleTest(JPPkgUnit.Pieces, PackageTypeList.Codes.Piece);
			SynchroniseJPB_ManifestUQSingleTest(JPPkgUnit.Tin, PackageTypeList.Codes.Tin);

			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Pallet, PackageTypeList.Codes.PalletAndPackage);
			synchroniser.SetEnabled(false, false);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Bag, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BaleCompressed, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BaleUncompressed, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Basket, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Box, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Bundle, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Carton, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Case, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Coil, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Container, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Crate, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Cylinder, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Drum, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Keg, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Package, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Pail, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Pallet, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Piece, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Reel, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Roll, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Sheet, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Skid, PackageTypeList.Codes.PalletAndPackage);

			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Bottle, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BreakBulk, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.BulkBag, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Cradle, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Dozen, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Envelope, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Gross, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(Core.Constants.PkgUnit.Mix, PackageTypeList.Codes.PalletAndPackage);

			SynchroniseJPB_ManifestUQSingleTest(JPPkgUnit.Pieces, PackageTypeList.Codes.PalletAndPackage);
			SynchroniseJPB_ManifestUQSingleTest(JPPkgUnit.Tin, PackageTypeList.Codes.PalletAndPackage);
		}

		void SynchroniseJPB_ManifestUQSingleTest(string input, string expected)
		{
			shipment.JS_F3_NKPackType = input;
			AssertEquals(expected, bill.JPB_ManifestUQ);
		}

		public void TestSynchroniseJPB_RN_NKGoodsOrigin()
		{
			AssertEquals(ZString.Empty, bill.JPB_RN_NKGoodsOrigin);

			var packline1 = shipment.OuterPackLines.AddNew();
			AssertEquals(ZString.Empty, bill.JPB_RN_NKGoodsOrigin);

			packline1.JL_RN_NKOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline1.JL_HarmonisedCode = "101020";
			packline1.JL_LinePrice = 100m;
			packline1.JL_ActualWeight = 100m;
			packline1.JL_PackageCount = 100;
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			var packline2 = shipment.OuterPackLines.AddNew();
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_LinePrice = 200m;
			packline2.JL_ActualWeight = 200m;
			packline2.JL_PackageCount = 200;
			packline2.JL_RN_NKOrigin = Core.Constants.CountryCodes.NewZealand;
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_HarmonisedCode = "203040";
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_LinePrice = 50m;
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_LinePrice = 100m;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_ActualWeight = 50m;
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_ActualWeight = 100m;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_PackageCount = 50;
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_PackageCount = 100;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.JPB_RN_NKGoodsOrigin);

			packline2.JL_HarmonisedCode = "101010";
			AssertEquals(Core.Constants.CountryCodes.Australia, bill.JPB_RN_NKGoodsOrigin);

			packline1.Delete();
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.JPB_RN_NKGoodsOrigin);

			synchroniser.SetEnabled(false, false);
			packline2.JL_RN_NKOrigin = Core.Constants.CountryCodes.Singapore;
			AssertEquals(Core.Constants.CountryCodes.NewZealand, bill.JPB_RN_NKGoodsOrigin);
		}

		public void TestSynchroniseJPB_MarksAndNumbers()
		{
			AssertEquals(ZString.Empty, bill.JPB_MarksAndNumbers);

			shipment.JS_MarksAndNumbers = "MARKS AND NUMBERS";
			AssertEquals("MARKS AND NUMBERS", bill.JPB_MarksAndNumbers);

			synchroniser.SetEnabled(false, false);
			shipment.JS_MarksAndNumbers = "DIFF MARKS AND NUMBERS";
			AssertEquals("MARKS AND NUMBERS", bill.JPB_MarksAndNumbers);
		}

		public void TestSynchroniseJPB_GoodsDescription()
		{
			AssertEquals(ZString.Empty, bill.JPB_GoodsDescription);

			shipment.JS_GoodsDescription = "GOODS AND MORE GOODS";
			AssertEquals("GOODS AND MORE GOODS", bill.JPB_GoodsDescription);

			shipment.DetailedGoodsDescriptionNoteText = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			AssertEquals("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890", bill.JPB_GoodsDescription);

			synchroniser.SetEnabled(false, false);
			shipment.DetailedGoodsDescriptionNoteText = "DIFF GOODS AND MORE GOODS";
			AssertEquals("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890", bill.JPB_GoodsDescription);
		}

		public void TestSynchroniseUNDG()
		{
			CombineAssertions(() =>
			{
				AssertEquals("", bill.JPB_DG_NKSubstance);
				AssertNull(bill.Substance);

				var packline1 = shipment.OuterPackLines.AddNew();
				AssertEquals("", bill.JPB_DG_NKSubstance);
				AssertNull(bill.Substance);

				packline1.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0014", "a", "IMO").First().PK;
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0014a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline1.JL_HarmonisedCode = "102030";
				packline1.JL_LinePrice = 100m;
				packline1.JL_ActualWeight = 100m;
				packline1.JL_PackageCount = 100;
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);

				var packline2 = shipment.OuterPackLines.AddNew();
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);

				packline2.JL_HarmonisedCode = "203040";
				packline2.JL_LinePrice = 200m;
				packline2.JL_ActualWeight = 200m;
				packline2.JL_PackageCount = 200;
				packline2.JL_RN_NKOrigin = Core.Constants.CountryCodes.NewZealand;
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);

				packline2.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				AssertEquals("0004a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0004a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_LinePrice = 50m;
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0014a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_LinePrice = 100m;
				AssertEquals("0004a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0004a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_ActualWeight = 50m;
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0014a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_ActualWeight = 100m;
				AssertEquals("0004a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0004a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_PackageCount = 50;
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0014a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_PackageCount = 100;
				AssertEquals("0004a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0004a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline2.JL_HarmonisedCode = "102010";
				AssertEquals("0014a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0014a", bill.UNDGSubstance.PK, bill.JPB_DG);

				packline1.Delete();
				AssertEquals("0004a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0004a", bill.UNDGSubstance.PK, bill.JPB_DG);

				synchroniser.SetEnabled(false, false);
				packline2.JL_RN_NKOrigin = Core.Constants.CountryCodes.Singapore;
				AssertEquals("0004a", bill.UNDGSubstance.DG_Code);
				AssertEquals("JPB_DG for 0004a", bill.UNDGSubstance.PK, bill.JPB_DG);
			});
		}

		public void TestSynchroniseNotificationForwardingParty()
		{
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals(0, bill.NotificationForwardingParties.Count);

			var oldConsigneeAddress = shipment.ConsigneeDocumentaryAddress.E2_OA_Address;
			var newConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg.SetCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Japan), "VICTA");
			newConsigneeOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery);
			Factory.Save();
			AssertEquals(0, bill.NotificationForwardingParties.Count);
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = newConsigneeOrg.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals(1, bill.NotificationForwardingParties.Count);
				AssertEquals((short)1, bill.NotificationForwardingParties.FirstOrDefault().CY_Order);
				AssertEquals("VICTA", bill.NotificationForwardingParties.FirstOrDefault().CY_Data);
			});

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(0, bill.NotificationForwardingParties.Count);

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = newConsigneeOrg.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals(1, bill.NotificationForwardingParties.Count);
				AssertEquals((short)1, bill.NotificationForwardingParties.FirstOrDefault().CY_Order);
				AssertEquals("VICTA", bill.NotificationForwardingParties.FirstOrDefault().CY_Data);
			});

			newConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			relatedOrg = Factory.NewWithValidTestData<OrgHeader>();
			newConsigneeOrg.SetRelatedParty(relatedOrg, RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyDirectionList.Codes.Delivery);
			Factory.Save();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = newConsigneeOrg.MainAddress.PK;
			AssertEquals(0, bill.NotificationForwardingParties.Count);
		}

		public void TestSynchroniseConsignee()
		{
			AssertSynchroniseJobDocAddress(shipment.ConsigneeDocumentaryAddress, bill.Consignee);
		}

		public void TestSynchroniseConsignor()
		{
			AssertSynchroniseJobDocAddress(shipment.ConsignorDocumentaryAddress, bill.Consignor);
		}

		public void TestSynchroniseNotifyParty1()
		{
			AssertSynchroniseJobDocAddress(shipment.ConsigneeDocumentaryAddress, bill.NotifyParty1);
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			synchroniser.Synchronise(true);
			AssertSynchroniseJobDocAddress(shipment.NotifyPartyDocumentaryAddress, bill.NotifyParty1);
		}

		public void TestSynchroniseNotifyParty2()
		{
			AssertSynchroniseJobDocAddress(shipment.NotifyParty2DocumentaryAddress, bill.NotifyParty2);
		}

		public void TestSynchroniseContainers()
		{
			AssertEquals(0, bill.Containers.Count);
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container1);

			AssertEquals(1, bill.Containers.Count);
			var billContainer1 = bill.Containers["CONT1"];
			AssertNotNull(billContainer1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(consol, container2);

			AssertEquals(2, bill.Containers.Count);
			AssertEquals(billContainer1, bill.Containers["CONT1"]);
			var billContainer2 = bill.Containers["CONT2"];
			AssertNotNull(billContainer2);

			container1.Delete();
			AssertEquals(1, bill.Containers.Count);
			AssertEquals(billContainer2, bill.Containers["CONT2"]);
			AssertEquals(true, billContainer1.IsDeleted);
		}

		void AssertSynchroniseJobDocAddress(JobDocAddress source, JobDocAddress destination)
		{
			AssertNotEquals(source, destination);
			source.E2_OA_Address = org1.MainAddress.PK;
			AssertEquals(org1.MainAddress.PK, destination.E2_OA_Address);
			source.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, destination.E2_OA_Address);
			source.E2_AddressOverride = true;
			source.E2_CompanyName = "BOB THE BUILDER";
			AssertEquals(true, destination.E2_AddressOverride);
			AssertEquals("BOB THE BUILDER", destination.E2_CompanyName);
		}

		ForwardingShipment shipment;
		JPAFRBills bill;
		JPAFRBillsynchroniser synchroniser;

		protected override void SetUp()
		{
			base.SetUp();
			shipment = consol.Shipments.AddNew();
			bill = header.Bills.AddNew();
			synchroniser = new JPAFRBillsynchroniser(bill, shipment);
			synchroniser.Synchronise(true);
		}

		protected override void TearDown()
		{
			base.TearDown();
		}
	}
}
