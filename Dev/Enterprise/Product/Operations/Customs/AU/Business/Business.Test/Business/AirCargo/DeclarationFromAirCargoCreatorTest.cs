using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DeclarationFromAirCargoCreatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateAndSaveToDbWithNoData()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_GoodsDescription = "Goods description";
			Assert("Pre-condition", cusHAWB.CS_JE_CustomsFormalEntry.IsEmpty);

			var creator = new DeclarationFromAirCargoCreator(cusHAWB);
			var declaration = creator.CreateIgnoreWarnings();
			Factory.Save();
			AssertEquals("Should now be linked to the Declaration", declaration.PK, cusHAWB.CS_JE_CustomsFormalEntry);
		}

		public void TestCreateIgnoreWarnings()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			CusHAWB cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_GoodsDescription = "Goods description";
			Assert("Pre-condition", cusHAWB.CS_JE_CustomsFormalEntry.IsEmpty);

			var creator = new DeclarationFromAirCargoCreator(cusHAWB);
			var declaration = creator.CreateIgnoreWarnings();
			AssertEquals("Should now be linked to the Declaration", declaration.PK, cusHAWB.CS_JE_CustomsFormalEntry);
			AssertEquals("Declaration details should be set from the air cargo record, just that no notifications are returned", cusHAWB.CS_GoodsDescription, declaration.JE_GoodsDescription);
		}

		public void TestCreate()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			cusMAWB.CM_MAWB = "08133333333";
			cusMAWB.CM_FlightNo = "QF1234";
			cusMAWB.CM_Folio = "FOLIO";
			cusMAWB.CM_RL_NKLoadPort = "USLAX";
			cusMAWB.CM_DateOfFirstArrival = new ZDateTime(2005, 2, 2);
			cusMAWB.CM_RL_NKFirstArrivalPort = "AUBNE";
			cusMAWB.CM_RL_NKDischargePort = "AUMEL";
			cusMAWB.CM_ArrivalDate = new ZDateTime(2005, 1, 1);

			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = "HAWB";
			cusHAWB.CS_RL_NKOrigin = "MYPKG";
			cusHAWB.CS_RL_NKDestination = "AUADL";
			cusHAWB.CS_Weight = 5;
			cusHAWB.CS_WeightUQ = Core.Constants.Weight.Grams;
			cusHAWB.CS_GoodsValue = 10;
			cusHAWB.CS_RX_NKGoodsCurrency = "USD";
			cusHAWB.CS_GoodsDescription = "GoodsDescription";
			cusHAWB.CS_PiecesManifested = 15;
			cusHAWB.CS_FreightPrepaidCollect = Core.Constants.PaymentType.Prepaid;
			Assert("Pre-condition", cusHAWB.CS_JE_CustomsFormalEntry.IsEmpty);

			var creator = new DeclarationFromAirCargoCreator(cusHAWB);
			var declaration = creator.Create(new NotificationBuffer());

			AssertEquals("Should now be linked to the Declaration", declaration.PK, cusHAWB.CS_JE_CustomsFormalEntry);
			AssertEquals("Transport mode should be air", Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
			AssertEquals("Should be an import declaration", Customs.Business.JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("IncoTerm mode should be FOB", Core.Constants.IncoTerms.FreeOnBoard, declaration.JE_ShipmentIncoTerm);

			AssertEquals("Master Bill", cusMAWB.CM_MAWB, declaration.JE_MasterBill);
			AssertEquals("Flight", cusMAWB.CM_FlightNo, declaration.JE_VoyageFlightNo);
			AssertEquals("Folio", cusMAWB.CM_Folio, declaration.JE_Folio);
			AssertEquals("Load port", "USLAX", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of first arrival", "AUBNE", declaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals("Date of first arrival", new ZDateTime(2005, 2, 2), declaration.JE_DateOfFirstArrival);
			AssertEquals("Discharge port", "AUMEL", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Date of arrival", new ZDateTime(2005, 1, 1), declaration.JE_DateOfArrival);
			AssertEquals("MasterBill", "08133333333", declaration.JE_MasterBill);
			AssertEquals(Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, declaration.JE_ApplicationCode);

			AssertEquals("House bill", "HAWB", declaration.JE_HouseBill);
			AssertEquals("Origin port", "MYPKG", declaration.JE_RL_NKOrigin);
			AssertEquals("Destination port", "AUADL", declaration.JE_RL_NKFinalDestination);
			AssertEquals("Total Weight on dec", 5m, declaration.JE_TotalWeight);
			AssertEquals("Total Weight UQ on dec", Core.Constants.Weight.Grams, declaration.JE_TotalWeightUnit);
			AssertEquals("Goods Description", "GoodsDescription", declaration.JE_GoodsDescription);
			AssertEquals("Total Units", 15, declaration.JE_TotalNoOfPieces);
			AssertEquals("Total Packs", 15, declaration.JE_TotalNoOfPacks);
			AssertEquals("Total Packs Unit", Core.Constants.PkgUnit.Piece, declaration.JE_TotalNoOfPacksPackType);
		}

		public void TestCusHAWBForeignKeySetInDeclarationFactory()
		{
			var declarationFactory = new BusinessObjectFactory();
			var declaration = declarationFactory.New<BaseJobDeclaration>();

			var cusHAWBFactory = new BusinessObjectFactory();
			var mAWB = cusHAWBFactory.New<CusMAWB>();
			var hAWB = mAWB.ChildBills.AddNew();
			cusHAWBFactory.Save();

			var creator = new DeclarationFromAirCargoCreator(hAWB);
			creator.Create(declaration, new NotificationBuffer());
			var hAWBInDeclarationFactory = declarationFactory.Load<CusHAWB>(hAWB.PK);
			AssertEquals("FK should be in the declaration factory so that it gets saved with the declaration", declaration.PK, hAWBInDeclarationFactory.CS_JE_CustomsFormalEntry);
		}
	}
}
