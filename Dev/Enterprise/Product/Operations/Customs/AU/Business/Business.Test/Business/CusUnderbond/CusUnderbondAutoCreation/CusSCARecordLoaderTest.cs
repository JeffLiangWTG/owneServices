// using Enterprise.Customs.AU.SeaCargo.Business;
// using CargoWise.EntityFramework.Testing;
// using CargoWise.Types;
// using NUnit.Framework;

// namespace Enterprise.Customs.AU.Declaration.Business.Testing
// {
// 	class CusSCARecordLoaderTest : TestCaseWithFactory
// 	{
// 		const string LloydsNum = "1234567";
// 		const string Voyage = "0039S";
// 		const string HouseBill1 = "HB9039";
// 		const string OceanBill1 = "OBL3939200";
// 		const string Container1 = "TRFU3939302";

// 		[ExpectNoExceptions()]
// 		public void TestLoadSCARecordNotCheckingNullOnContainer()
// 		{
// 			CusSCAContainer Container = (CusSCAContainer)Factory.New(typeof(CusSCAContainer));
// 			Container.CN_ContainerNumber = Container1;
// 			Loader.LoadSCARecord(LloydsNum, Voyage, OceanBill1, HouseBill1, Container1, CMRImportCargoTypes.Codes.FullContainerLoad);
// 		}

// 		public void TestLoadHouseBill()
// 		{
// 			AssertNull(Loader.LoadSCARecord(LloydsNum, Voyage, OceanBill1, HouseBill1, Container1, CMRImportCargoTypes.Codes.LessThanContainerLoad));
// 			AssertNull(Loader.LoadSCARecord(LloydsNum, Voyage, ZString.Empty, HouseBill1, Container1, CMRImportCargoTypes.Codes.LessThanContainerLoad));
// 			CMRCusSCAHouse House = OceanBill.HouseBills.AddNew();
// 			House.CA_HouseBill = HouseBill1;
// 			CMRCusSCAContainer Container = OceanBill.Containers.AddNew();
// 			Container.CN_ContainerNumber = Container1;
// 			Container.Pivots.Add(House.Pivot.AddNew());
// 			AssertNotNull(Loader.LoadSCARecord(LloydsNum, Voyage, OceanBill1, HouseBill1, Container1, CMRImportCargoTypes.Codes.LessThanContainerLoad));
// 			AssertNotNull(Loader.LoadSCARecord(LloydsNum, Voyage, ZString.Empty, HouseBill1, Container1, CMRImportCargoTypes.Codes.LessThanContainerLoad));
// 		}

// 		public void TestLoadContainer()
// 		{
// 			AssertNull(Loader.LoadSCARecord(LloydsNum, Voyage, ZString.Empty, ZString.Empty, Container1, CMRImportCargoTypes.Codes.FullContainerLoad));
// 			AssertNull(Loader.LoadSCARecord(LloydsNum, Voyage, OceanBill1, ZString.Empty, Container1, CMRImportCargoTypes.Codes.FullContainerLoad));
// 			CMRCusSCAHouse House = OceanBill.HouseBills.AddNew();
// 			House.CA_HouseBill = HouseBill1;
// 			CMRCusSCAContainer Container = OceanBill.Containers.AddNew();
// 			Container.CN_ContainerNumber = Container1;
// 			AssertNotNull(Loader.LoadSCARecord(LloydsNum, Voyage, ZString.Empty, ZString.Empty, Container1, CMRImportCargoTypes.Codes.FullContainerLoad));
// 			AssertNotNull(Loader.LoadSCARecord(LloydsNum, Voyage, OceanBill1, ZString.Empty, Container1, CMRImportCargoTypes.Codes.FullContainerLoad));
// 		}

// 		CMRCusSCAOceanBill OceanBill
// 		{
// 			get
// 			{
// 				if (fOceanBill == null)
// 				{
// 					fOceanBill = Factory.New<CMRCusSCAOceanBill>();
// 					fOceanBill.CB_OceanBill = OceanBill1;
// 					fOceanBill.CB_LloydsIMO = LloydsNum;
// 					fOceanBill.CB_Voyage = Voyage;
// 				}
// 				return fOceanBill;
// 			}
// 		}
// 		CMRCusSCAOceanBill fOceanBill;

// 		CusSCARecordLoader fLoader;
// 		CusSCARecordLoader Loader
// 		{
// 			get
// 			{
// 				if (fLoader == null)
// 				{
// 					fLoader = new CusSCARecordLoader(Factory);
// 				}
// 				return fLoader;
// 			}
// 		}

// 	}
// }
