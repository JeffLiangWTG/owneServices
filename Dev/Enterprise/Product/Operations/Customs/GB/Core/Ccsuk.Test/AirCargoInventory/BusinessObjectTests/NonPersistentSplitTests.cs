using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class NonPersistentSplitLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSplitNumberingIsSequential()
		{
			var col = new NonPersistentSplitLineCollection(Factory);
			var srf01 = col.AddNew();
			var srf02 = col.AddNew();
			var srf03 = col.AddNew();
			col.RemoveAndDelete(srf02);
			AssertHasErrorContaining(srf01.SplitNumberInfo, "02 is missing");
			srf02 = col.AddNew();
			srf02.SplitNumber = "02";
			AssertNoErrorContaining(srf01.SplitNumberInfo, "02 is missing");
			AssertNoErrorContaining(srf02.SplitNumberInfo, "02 is missing");
			AssertNoErrorContaining(srf03.SplitNumberInfo, "02 is missing");

			var anotherCol = new NonPersistentSplitLineCollection(Factory);
			var newSrf01 = anotherCol.AddNew();
			AssertNoErrorContaining(newSrf01.SplitNumberInfo, "01 is missing");
			newSrf01.SplitNumber = "02";
			AssertHasErrorContaining(newSrf01.SplitNumberInfo, "01 is missing");
		}

		public void TestCheckSplitNumberCheckWeightCheckWeightUQCheckNumberOfPieces()
		{
			var line = new NonPersistentSplitLine("", "", 0, 0m, "", "", Factory);
			AssertHasErrorContaining(line.SplitNumberInfo, "enter a");
			AssertNoErrorContaining(line.WeightInfo, "enter a");
			AssertHasWarningContaining(line.NumberOfPiecesInfo, "number of pieces");
			line.WeightUQ = "LB";
			AssertHasErrorContaining(line.WeightUQInfo, "kilo");
			line.WeightUQ = "KG";
			AssertNoErrorContaining(line.WeightUQInfo, "kilo");
			line.NumberOfPieces = 69;
			AssertHasErrorContaining(line.WeightInfo, "enter a");
			line.Weight = 69m;
			AssertNoErrorContaining(line.WeightInfo, "enter a");
		}

		public void TestCheckHandlingDetail()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000AAA";
			basic.NumberOfPiecesExpected = 100;
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "Stuff", "", basic);
			line.HandlingDetail = "";
			AssertNoErrorContaining(line.HandlingDetailInfo, "marks");
			line.HandlingDetail = "X";
			AssertNoErrorContaining(line.HandlingDetailInfo, "marks");
			basic.Profile = "CUKAIR98LHRBAC";
			line.HandlingDetail = "Y";
			AssertNoErrorContaining(line.HandlingDetailInfo, "marks");
			line.HandlingDetail = "";
			AssertHasErrorContaining(line.HandlingDetailInfo, "marks");
		}

		public void TestCheckNumberOfPiecesReceived()
		{
			var basic = Factory.New<CusMAWB>();
			basic.NumberOfPiecesExpected = 100;
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "Stuff", "", basic);
			line.WarehouseLocationID = ZGuid.NewZGuid();
			AssertHasErrorContaining(line.NumberOfPiecesReceivedInfo, "SSL");
			line.NumberOfPiecesReceived = 10;
			AssertNoErrorContaining(line.NumberOfPiecesReceivedInfo, "SSL");
		}
	}

	[TestedType(typeof(NonPersistentSplitLine))]
	class NonPersistentSplitLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var line = new NonPersistentSplitLine("03", "KGM", 69, 70m, "Handle me", "CA", Factory);
			AssertEquals("Split", line.HumanReadableName);
		}

		public void TestCtor1()
		{
			var line = new NonPersistentSplitLine("03", "KGM", 69, 70m, "Handle me", "CA", Factory);
			AssertEquals("03", line.SplitNumber);
			AssertEquals("KG", line.WeightUQ);
			AssertEquals(70m, line.Weight);
			AssertEquals(69, line.NumberOfPieces);
			AssertEquals("Handle me", line.HandlingDetail);
			AssertEquals("CA", line.CustomsActionCode);
		}

		public void TestCtor2()
		{
			var line = new NonPersistentSplitLine(4, Factory);
			AssertEquals("04", line.SplitNumber);
		}

		public void TestCtor3()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000ABC";
			var line = new NonPersistentSplitLine(4, basic);
			AssertEquals(false, line.HandlingDetailInfo.ReadOnly);
			basic.Profile = "CUKAIR98LHRBAC";
			line = new NonPersistentSplitLine(4, basic);
			AssertEquals(false, line.HandlingDetailInfo.ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			var line = new NonPersistentSplitLine();
			AssertEquals("KG", line.WeightUQ);
		}

		public void TestFormatForGenral()
		{
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "", "", Factory);
			AssertEquals("  SRF 3, 69 Piece(s), Wgt 70KG", line.FormatForGenral());
		}

		public void TestReadOnlyAndDelete()
		{
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "", "", Factory);
			AssertEquals("Not read only, no CAC", false, line.ReadOnly);
			AssertEquals("Not read only, can delete", true, line.CanDelete);
			line.CustomsActionCode = "CA";
			AssertEquals("Read only, locking CAC", true, line.ReadOnly);
			AssertEquals("Read only, cannot delete", false, line.CanDelete);
			line.CustomsActionCode = "CC";
			AssertEquals("Read only, final CAC", true, line.ReadOnly);
			AssertEquals("Read only, cannot delete", false, line.CanDelete);
			AssertContains("locking customs action code", line.ReasonForNotAbleToDelete);
			line.CustomsActionCode = "CX";
			AssertEquals("Not read only, deleted CAC", false, line.ReadOnly);
			AssertEquals("Not read only, can delete", true, line.CanDelete);
			var collection = new NonPersistentSplitLineCollection(Factory);
			collection.Add(line);
			AssertEquals("Collection is not allocating, can delete", true, line.CanDelete);
			AssertEquals("Collection is not allocating, can edit", false, line.SplitNumberInfo.ReadOnly);
			AssertEquals("Collection is not allocating, can edit", false, line.WeightInfo.ReadOnly);
			AssertEquals("Collection is not allocating, can edit", false, line.WeightUQInfo.ReadOnly);
			collection.IsAllocatingNprWithFlightData = true;
			AssertEquals("Collection is allocating, cannot delete", false, line.CanDelete);
			AssertEquals("Collection is allocating, cannot edit", true, line.SplitNumberInfo.ReadOnly);
			AssertEquals("Collection is allocating, cannot edit", true, line.WeightInfo.ReadOnly);
			AssertEquals("Collection is allocating, cannot edit", true, line.WeightUQInfo.ReadOnly);
		}

		public void TestSplitNumber()
		{
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "", "", Factory);
			line.SplitNumber = "XX";
			AssertEquals("", line.SplitNumber);
			line.SplitNumber = "X2";
			AssertEquals("2", line.SplitNumber);
			line.SplitNumber = "12";
			AssertEquals("12", line.SplitNumber);
		}

		public void TestNumberOfPieces()
		{
			var basic = Factory.New<CusMAWB>();
			basic.NumberOfPiecesExpected = 100;
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "Stuff", "", basic);
			line.NumberOfPieces = 10;
			AssertEquals(10, line.NumberOfPieces);
			AssertEquals(10, (ZInt)line.NumberOfPiecesExpected);
			AssertEquals(0, line.NumberOfPiecesReceived);
			basic.NumberOfPiecesReceived = 100;
			basic.Status1Date = ZDateTime.BrettsBirthday;
			line.NumberOfPieces = 11;
			AssertEquals(11, line.NumberOfPieces);
			AssertEquals(11, (ZInt)line.NumberOfPiecesExpected);
			AssertEquals("With St1, NPR is also set", 11, line.NumberOfPiecesReceived);
			AssertEquals(true, line.NumberOfPiecesReceivedInfo.ReadOnly);
			basic.Profile = "CUKAIR98LHRBAC";
			AssertEquals(false, line.NumberOfPiecesReceivedInfo.ReadOnly);
		}

		public void TestWarehouseLocationID()
		{
			var basic = Factory.New<CusMAWB>();
			var line = new NonPersistentSplitLine("3", "KGM", 69, 70m, "Stuff", "", basic);
			var id = ZGuid.NewZGuid();
			line.WarehouseLocationID = id;
			AssertEquals(id, line.WarehouseLocationID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentSplitLine();
		}
	}

	[TestedType(typeof(NonPersistentSplitLineCollection))]
	class NonPersistentSplitLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentSplitLineCollection>
	{
		protected override NonPersistentSplitLineCollection GetCollectionToTest()
		{
			return new NonPersistentSplitLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NonPersistentSplitLine();
		}

		public void TestDefaultSplitNumberDuringAddAndRemove()
		{
			var col = new NonPersistentSplitLineCollection(Factory);
			var item1 = col.AddNew();
			AssertEquals("01", item1.SplitNumber);
			var item2 = col.AddNew();
			AssertEquals("02", item2.SplitNumber);
			col.Remove(item2);
			AssertEquals(1, col.Count);
			var split3 = col.AddNew();
			AssertEquals("Third split's number is 2, not 3, because index was knocked down one when number 3 was removed", "02", split3.SplitNumber);
		}

		public void TestFormat()
		{
			var col = new NonPersistentSplitLineCollection(Factory);
			var line1 = new NonPersistentSplitLine("01", "KGM", 69, 70m, "", "", Factory);
			var line2 = new NonPersistentSplitLine("01", "LB", 169, 170m, "", "", Factory);
			col.Add(line1);
			col.Add(line2);
			AssertEquals(line1.FormatForGenral() + System.Environment.NewLine + line2.FormatForGenral(), col.FormatForGenral());
		}

		public void TestMaximumRows()
		{
			var col = new NonPersistentSplitLineCollectionForTest(Factory);
			AssertEquals(true, col.AllowNew);
			AssertEquals(true, col.AllowRemove);
			col.IsAllocatingNprWithFlightData = true;
			AssertEquals(false, col.AllowNew);
			AssertEquals(false, col.AllowRemove);
			col.IsAllocatingNprWithFlightData = false;

			for (int i = 1; i < 5; i++)
			{
				col.AddNew();
			}
			AssertEquals(4, col.Count);
			AssertEquals(true, col.AllowNew);
			col.AddNew();
			AssertEquals(5, col.Count);
			AssertEquals("When maxRows (99) rows exist, no more are allowed.  NB there is a bug in ZGrid or BusinessObkectCollection that allows you to add a new row to a grid event when AllowNew is false, so long as you use DownArrow or Enter to commit the last and the excess rows. But that is beyond the scope of this unit test.",
							false, col.AllowNew);
			var fifthSplit = col.AddNew();
			Assert(fifthSplit.Notifications.ContainsNotificationContaining("maximum"));
		}

		class NonPersistentSplitLineCollectionForTest : NonPersistentSplitLineCollection
		{
			public NonPersistentSplitLineCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{ }

			protected override int maxRows
			{
				get { return 5; }
			}
		}
	}

	[TestedType(typeof(NonPersistentSplitsAndFlightData))]
	class NonPersistentSplitsAndFlightDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCtor1()
		{
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData(Factory);
			AssertEquals("SplitLines collection should be created", 0, npSplitsAndFlightData.SplitLines.Count);
		}

		public void TestCtor2()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var splits = new NonPersistentSplitLineCollection(Factory);
			var splitLine = splits.AddNew();
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData("BA123", ZDateTime.BrettsBirthday, splits, mawb);
			AssertEquals("SplitLines collection should be created", 1, npSplitsAndFlightData.SplitLines.Count);
			AssertEquals("Flight Number should be set", "BA123", npSplitsAndFlightData.FlightNumber);
			AssertEquals("Flight Date should be set", ZDateTime.BrettsBirthday.ToString(), npSplitsAndFlightData.FlightArrivalDate.ToString());
		}

		public void TestReadOnlyFlightDetails()
		{
			var mawb = Factory.New<CusMAWB>();
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData("BA123", ZDateTime.BrettsBirthday, new NonPersistentSplitLineCollection(Factory), mawb);
			AssertEquals("Sending flight details disabled because the flight has not arrived, there are no splits and  no pieces received", true, npSplitsAndFlightData.ReadOnlyFlightDetails);
			mawb.CM_FlightNo = "VV121";
			mawb.CM_ArrivalDate = ZDateTime.Today;
			AssertEquals("Sending flight details disabled because there are no splits and  no pieces received", true, npSplitsAndFlightData.ReadOnlyFlightDetails);
			var splitline = mawb.Splits.AddNew();
			AssertEquals("Sending flight details disabled because no pieces received", true, npSplitsAndFlightData.ReadOnlyFlightDetails);
			mawb.NumberOfPiecesReceived = 10;
			AssertEquals("Sending flight details enabled", false, npSplitsAndFlightData.ReadOnlyFlightDetails);
		}

		public void TestReadOnly()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var splits = new NonPersistentSplitLineCollection(Factory);
			var splitLine = splits.AddNew();
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData("BA123", ZDateTime.BrettsBirthday, splits, mawb);
			AssertEquals(false, npSplitsAndFlightData.SplitLines.ReadOnly);
			AssertEquals(true, npSplitsAndFlightData.SplitLines.AllowNew);
			AssertEquals(true, npSplitsAndFlightData.SplitLines.AllowRemove);
			npSplitsAndFlightData.SendFlightInfoToo = true;
			AssertEquals(false, npSplitsAndFlightData.SplitLines.ReadOnly);
			AssertEquals(false, npSplitsAndFlightData.SplitLines.AllowNew);
			AssertEquals(false, npSplitsAndFlightData.SplitLines.AllowRemove);

			mawb.Profile = "CUKFFW98000AAA";
			AssertEquals(false, npSplitsAndFlightData.SendFlightInfoTooInfo.ReadOnly);
			mawb.Profile = "CUKAIR98LHRXXX";
			AssertEquals(true, npSplitsAndFlightData.SendFlightInfoTooInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentSplitsAndFlightData(Factory);
		}

		public void TestCheckTotalPieces()
		{
			var basic = Factory.New<CusMAWB>();
			basic.NumberOfPiecesExpected = 10;
			var controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.GetNonPersistentSplitsAndFlightData();
			var bizO = controller.SplitsAndFlightData;
			var line1 = bizO.SplitLines.AddNew();
			line1.NumberOfPieces = 9;
			bizO.UpdateTotal();
			AssertHasErrorContaining(bizO.TotalPiecesInfo, "NPX");
			line1.NumberOfPieces = 10;
			bizO.UpdateTotal();
			AssertNoErrorContaining(bizO.TotalPiecesInfo, "NPX");
			var line2 = bizO.SplitLines.AddNew();
			line2.NumberOfPieces = 1;
			bizO.UpdateTotal();
			AssertHasErrorContaining(bizO.TotalPiecesInfo, "NPX");
			line1.NumberOfPieces = 9;
			bizO.UpdateTotal();
			AssertNoErrorContaining(bizO.TotalPiecesInfo, "NPX");
		}
	}

	internal class NonPersistentSplitLineOrchestratorTest : TestCaseWithFactory
	{
		public void TestLoadFromFRD()
		{
			var basic = Factory.New<CusMAWB>();
			var orchestrator = new NonPersistentSplitLineOrchestrator(basic);
			var result = orchestrator.PerformLoadFromLastFrd();
			AssertContains("No inbound FRD", result);
			var frdOld = basic.Messages.AddNew();
			frdOld.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			frdOld.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			frdOld.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code;
			frdOld.EM_MessageSubType = CcsukTransmissionMessageFunction.CIM.FRD.SubCode;
			frdOld.EM_MessageText = "CRAP, ignore, too old";
			var frd = basic.Messages.AddNew();
			frd.EM_SystemCreateTimeUtc = ZDateTime.Now;  // fresh
			frd.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			frd.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code;
			frd.EM_MessageSubType = CcsukTransmissionMessageFunction.CIM.FRD.SubCode;
			frd.EM_MessageText = CargoFactMessageGenerationTests.expectedFrdWithoutFlightData;

			// No flight number
			var throwAway = orchestrator.SplitsAndFlightData.SplitLines.AddNew();
			result = orchestrator.PerformLoadFromLastFrd();
			AssertEquals("", result);
			AssertCollectionNotContains("Existing NPBOs clobbered", throwAway, orchestrator.SplitsAndFlightData.SplitLines);
			AssertEquals(3, orchestrator.SplitsAndFlightData.SplitLines.Count);
			CheckSplitLine(0, "01", 33m, 2, orchestrator, "", 0);
			CheckSplitLine(1, "02", 12.67m, 2, orchestrator, "HANDLE WITH CARE", 0);
			CheckSplitLine(2, "03", 10m, 1, orchestrator, "DANIEL", 0);

			// With flight number
			orchestrator.SplitsAndFlightData.SplitLines[0].NumberOfPieces = 111;
			orchestrator.SplitsAndFlightData.SplitLines[1].NumberOfPieces = 222;
			orchestrator.SplitsAndFlightData.SplitLines[2].NumberOfPieces = 333;
			orchestrator.SplitsAndFlightData.SplitLines[0].HandlingDetail = "x";
			orchestrator.SplitsAndFlightData.SplitLines[1].HandlingDetail = "y";
			orchestrator.SplitsAndFlightData.SplitLines[2].HandlingDetail = "z";
			var frdNewest = basic.Messages.AddNew();
			frdNewest.EM_SystemCreateTimeUtc = ZDateTime.Now;  // fresh
			frdNewest.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			frdNewest.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code;
			frdNewest.EM_MessageSubType = CcsukTransmissionMessageFunction.CIM.FRD.SubCode;
			frdNewest.EM_MessageText = CargoFactMessageGenerationTests.expectedFrdWithFlightData;
			result = orchestrator.PerformLoadFromLastFrd();
			AssertContains("number of splits in the messages does not match those which already exist, or none already exist", result);
			var s1 = basic.Splits.AddNew();
			s1.SplitReference = "01";
			var s2 = basic.Splits.AddNew();
			s2.SplitReference = "02";
			result = orchestrator.PerformLoadFromLastFrd();
			AssertContains("number of splits in the messages does not match those which already exist, or none already exist", result);
			var s3 = basic.Splits.AddNew();
			s3.SplitReference = "03";
			result = orchestrator.PerformLoadFromLastFrd();  // OK
			AssertEquals("", result);
			CheckSplitLine(0, "01", 33m, 111, orchestrator, "", 2);
			CheckSplitLine(1, "02", 12.67m, 222, orchestrator, "HANDLE WITH CARE", 2);
			CheckSplitLine(2, "03", 10m, 333, orchestrator, "DANIEL", 1);
			var s4 = basic.Splits.AddNew();
			s4.SplitReference = "04";
			result = orchestrator.PerformLoadFromLastFrd();
			AssertContains("number of splits in the messages does not match those which already exist, or none already exist", result);
		}

		public void TestPerformRemoveAllSplitsAndManagePiecesForShed()
		{
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var basic = Factory.New<CusMAWB>();
			basic.NumberOfPiecesExpected = 69;
			basic.Profile = "CUKAIR98LHRBAC";
			basic.CM_MAWB = "55512345678";
			basic.AirportOfArrival = "LHR";
			var warehouseId1 = ZGuid.NewZGuid();
			var warehouseId2 = ZGuid.NewZGuid();
			var ot1 = basic.OutTurns.AddNew();
			ot1.WarehouseLocationID = warehouseId1;
			ot1.C5_MarksAndNumbers = "Red";
			ot1.C5_PackagesOutturned = 11;
			ot1.SplitReferenceToWhichThisPertains = "01";
			ot1.C5_GoodsDescription = "Keep me";
			var ot2 = basic.OutTurns.AddNew();
			ot2.WarehouseLocationID = warehouseId1;
			ot2.C5_MarksAndNumbers = "Blue";
			ot2.C5_PackagesOutturned = 22;
			ot2.SplitReferenceToWhichThisPertains = "02";
			ot2.C5_GoodsDescription = "Chuck me";
			var ot3 = basic.OutTurns.AddNew();
			ot3.WarehouseLocationID = warehouseId2;
			ot3.C5_MarksAndNumbers = "Green";
			ot3.C5_PackagesOutturned = 333;
			ot3.SplitReferenceToWhichThisPertains = "03";
			ot3.C5_GoodsDescription = "Keep me too";
			var split1 = basic.Splits.AddNew();
			var split2 = basic.Splits.AddNew();
			var split3 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split3.SplitReference = "03";

			var orchestrator = new NonPersistentSplitLineOrchestrator(basic);
			var result = orchestrator.PerformRemoveAllSplitsAndManagePiecesForShed(shutUp);

			AssertEquals(2, basic.OutTurns.Count);
			AssertEquals(0, basic.Splits.Count);
			AssertEquals(33, basic.OutTurns[0].C5_PackagesOutturned);
			AssertEquals("Red, Blue", basic.OutTurns[0].C5_MarksAndNumbers);
			AssertEquals(warehouseId1, basic.OutTurns[0].WarehouseLocationID);
			AssertEquals("Keep me", basic.OutTurns[0].C5_GoodsDescription);
			AssertEquals(333, basic.OutTurns[1].C5_PackagesOutturned);
			AssertEquals("Green", basic.OutTurns[1].C5_MarksAndNumbers);
			AssertEquals(warehouseId2, basic.OutTurns[1].WarehouseLocationID);
			AssertEquals("Keep me too", basic.OutTurns[1].C5_GoodsDescription);
			AssertEquals(2, basic.Messages.Count);
			AssertEquals("FCS", basic.Messages[0].EM_MessageSubType);
			AssertEquals("FAU", basic.Messages[1].EM_MessageSubType);
			AssertEquals("Auto-saved", true, basic.IsInDatabase);
			AssertContains("GID+01'QTY+118:69'", basic.Messages[0].EM_MessageText);
			AssertContains("GID+02'QTY+118:0'", basic.Messages[0].EM_MessageText);
			AssertContains("GID+03'QTY+118:0'", basic.Messages[0].EM_MessageText);
		}

		public void TestContructorForAwbWithSplitsAlready()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];
			AddSplit(hawb, "01", 1m, 2);
			AddSplit(hawb, "02", 4m, 5);
			var controller = new NonPersistentSplitLineOrchestrator(hawb);
			AssertEquals(2, controller.SplitsAndFlightData.SplitLines.Count);
			var nonPersistentSplit3 = controller.SplitsAndFlightData.SplitLines.AddNew();
			AssertEquals("03", nonPersistentSplit3.SplitNumber);
		}

		static void AddSplit(ICcsukCusAwb awb, string splitNumber, decimal weight, short pieces)
		{
			var split = awb.Splits.AddNew();
			split.CG_PiecesManifested = pieces;
			split.Weight = weight;
			split.SplitReference = splitNumber;
		}

		public void TestMakeUsingGenral_HugeRequestSplitOverMultipleMessages()
		{
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Profile = "CUKFFW98000DEF";  // agent
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			var collection = new NonPersistentSplitLineCollection(Factory);
			for (int i = 1; i < 50; i++)
			{
				collection.Add(new NonPersistentSplitLine(i.ToString("D2"), "KG", i, i, "", "", Factory));
			}
			controller.SplitsAndFlightData.SplitLines = collection;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			controller.PerformSplit_Genral(shutUp);
			AssertEquals(4, awb.Messages.Count);

			AssertContains("1 OF 4", awb.Messages[0].EM_MessageText);
			AssertContains("2 OF 4", awb.Messages[1].EM_MessageText);
			AssertContains("3 OF 4", awb.Messages[2].EM_MessageText);
			AssertContains("4 OF 4", awb.Messages[3].EM_MessageText);

			AssertContains("SRF 01", awb.Messages[0].EM_MessageText);
			AssertContains("SRF 14", awb.Messages[0].EM_MessageText);
			AssertContains("SRF 15", awb.Messages[1].EM_MessageText);
			AssertContains("SRF 32", awb.Messages[1].EM_MessageText);
			AssertContains("SRF 33", awb.Messages[2].EM_MessageText);
			AssertContains("SRF 49", awb.Messages[2].EM_MessageText);
			AssertContains("CARGOWISE SUPPORT", awb.Messages[3].EM_MessageText);

			AssertContains("CONTINUES IN NEXT MESSAGE", awb.Messages[0].EM_MessageText);
			AssertContains("CONTINUES IN NEXT MESSAGE", awb.Messages[1].EM_MessageText);
			AssertContains("CONTINUES IN NEXT MESSAGE", awb.Messages[2].EM_MessageText);
			AssertNotContains("CONTINUES IN NEXT MESSAGE", awb.Messages[3].EM_MessageText);

			AssertNotContains("END OF MESSAGE SET", awb.Messages[0].EM_MessageText);
			AssertNotContains("END OF MESSAGE SET", awb.Messages[1].EM_MessageText);
			AssertNotContains("END OF MESSAGE SET", awb.Messages[2].EM_MessageText);
			AssertContains("END OF MESSAGE SET", awb.Messages[3].EM_MessageText);
		}

		public void TestMakeUsingGenral_Mawb()
		{
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Profile = "CUKFFW98000DEF";  // agent
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			controller.SplitsAndFlightData.SplitLines = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory).SplitLines;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			controller.PerformSplit_Genral(shutUp);
			AssertEquals(1, awb.Messages.Count);
			var msg = awb.Messages[0];
			AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " is shed's PIMA", "CUKAIR98LHRKLM", msg.EM_ApplicationReference);
			AssertEquals(EDIMessage.Schema.EM_ApplicationCode, "CUK", msg.EM_ApplicationCode);
			AssertContains(EDIMessage.Schema.EM_MessageText, "GENRAL:0:912:UN", msg.EM_MessageText);
			AssertContains(EDIMessage.Schema.EM_MessageText, "BGM+TXT:ZZZ", msg.EM_MessageText);
			AssertContains(EDIMessage.Schema.EM_MessageInterpretation + " contains DEF (real badge) not ABC (mnemonic)", @"<pre><i>-- Split Request --
Request from agent DEF to split consignment at shed LHR/KLM
Consignment identifier: 801-12345678
Requested division of consignment:
  SRF 01, 2 Piece(s), Wgt 33KG
  SRF 02, 2 Piece(s), Wgt 12.67KG
   Handle with care
  SRF 03, 1 Piece(s), Wgt 10KG
   Daniel
Agent details:
  Operator company: Eagle Datamation International
  Operator name: CargoWise Support
  Operator phone: PH 07 3268 2903  FAX</i></pre>", msg.EM_MessageInterpretation);
		}

		public void TestMakeFRD()
		{
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Profile = "CUKFFW98000DEF";
			awb.NumberOfPiecesReceived = 1;
			AddSplit(awb, "01", 1m, 5);
			AddSplit(awb, "02", 4m, 3);
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			var npSplits = new NonPersistentSplitLineCollection(Factory);
			var npSplitLine1 = npSplits.AddNew();
			npSplitLine1.SplitNumber = "01";
			npSplitLine1.NumberOfPieces = 0;
			npSplitLine1.Weight = 1;
			var npSplitLine2 = npSplits.AddNew();
			npSplitLine2.SplitNumber = "02";
			npSplitLine2.Weight = 2;
			npSplitLine2.NumberOfPieces = 1;
			controller.SplitsAndFlightData.SplitLines = npSplits;
			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals("No pieces have been received", (short)0, awb.Splits[0].NumberOfPiecesReceived);
			AssertEquals("No pieces have been received", (short)0, awb.Splits[1].NumberOfPiecesReceived);
			AssertEquals("5 pieces expected", (short)5, awb.Splits[0].NumberOfPiecesExpected);
			AssertEquals("3 pieces expected", (short)3, awb.Splits[1].NumberOfPiecesExpected);
			awb.NumberOfPiecesReceived = 10;
			controller.SplitsAndFlightData.SendFlightInfoToo = true;
			controller.PerformSplit_FRD(shutUp);
			awb.Reload();

			AssertEquals("An agent sending an FRD has no effect on NPR (that is achieved via FRC from shed)", (short)0, awb.Splits[0].NumberOfPiecesReceived);
			AssertEquals("An agent sending an FRD has no effect on NPR (that is achieved via FRC from shed)", (short)0, awb.Splits[1].NumberOfPiecesReceived);
			AssertEquals("5 pieces expected", (short)5, awb.Splits[0].NumberOfPiecesExpected);
			AssertEquals("3 pieces expected", (short)3, awb.Splits[1].NumberOfPiecesExpected);
			AssertEquals(1, awb.Messages.Count);
			var msg = awb.Messages[0];
			AssertEquals(EDIMessage.Schema.EM_MessageOwner + " is agent PIMA", "CUKFFW98000DEF", msg.EM_MessageOwner);
			AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " = shed", "CUKAIR98LHRKLM", msg.EM_ApplicationReference);
			AssertEquals(EDIMessage.Schema.EM_ApplicationCode, "CUK", msg.EM_ApplicationCode);
			AssertContains(EDIMessage.Schema.EM_MessageText + " is CIM", "CIMFRD:0:0:IA+", msg.EM_MessageText);
		}

		public void TestMakeUsingFcs_AgentDisallowed()
		{
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Profile = "CUKFFW98000YYY";  //agent
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			controller.SplitsAndFlightData.SplitLines = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory).SplitLines;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var result = controller.PerformSplit_FCS(shutUp);
			AssertContains("Agent roles may not perform this function", result);
			AssertEquals(0, awb.Messages.Count);
		}

		public void TestMakeUsingFcs_ValidationFailures()
		{
			var awb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			awb.Profile = "CUKAIR98000XXX"; // Shed
			var controller = new NonPersistentSplitLineOrchestrator(awb);
			controller.SplitsAndFlightData.SplitLines = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory).SplitLines;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);

			awb.AirportOfArrival = "";
			controller.PerformSplit_FCS(shutUp);
			AssertEquals(0, awb.Messages.Count);
			AssertContains("Missing data for mandatory field: Airport", shutUp.LastErrorsAsString);
		}

		public void TestRemovingAllSplitsOnHawb()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesManifested = 6;
			hawb.CS_Weight = 20;
			AddSplit(hawb, "01", 2m, 1);
			AddSplit(hawb, "02", 4m, 2);
			AddSplit(hawb, "03", 6m, 3);
			var controller = new NonPersistentSplitLineOrchestrator(hawb);
			controller.PerformRemoveAllSplitstNoMessage();
			AssertSplitsMarkedForRemoval(controller);
		}

		void AssertSplitsMarkedForRemoval(NonPersistentSplitLineOrchestrator controller)
		{
			CheckSplitLine(0, "01", 20, 6, controller, "", -1);
			CheckSplitLine(1, "02", 0, 0, controller, "", -1);
			CheckSplitLine(2, "03", 0, 0, controller, "", -1);
		}

		public void TestRemovingAllSplitsOnBasic()
		{
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.NumberOfPiecesExpected = 6;
			basic.Weight = 20;
			AddSplit(basic, "01", 2m, 1);
			AddSplit(basic, "02", 4m, 2);
			AddSplit(basic, "03", 6m, 3);
			var controller = new NonPersistentSplitLineOrchestrator(basic);
			controller.PerformRemoveAllSplitstNoMessage();
			AssertSplitsMarkedForRemoval(controller);
		}

		public void TestRemovingAllSplitWhenOneSplitIsLocked()
		{
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.NumberOfPiecesExpected = 6;
			basic.Weight = 20;
			AddSplit(basic, "01", 2m, 1);
			AddSplit(basic, "02", 4m, 2);
			AddSplit(basic, "03", 6m, 3);
			var split = basic.Splits[0];
			split.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			var controller = new NonPersistentSplitLineOrchestrator(basic);
			var cannotRemoveReason = controller.PerformRemoveAllSplitstNoMessage();
			AssertContains("Cannot delete all splits, at least one has a locking customs action code", cannotRemoveReason);
			CheckSplitLine(0, "01", 2m, 1, controller, "", -1);
			CheckSplitLine(1, "02", 4m, 2, controller, "", -1);
			CheckSplitLine(2, "03", 6m, 3, controller, "", -1);

			split.SetCustomsActionCode("CX", ZDateTime.BrettsBirthday);
			controller = new NonPersistentSplitLineOrchestrator(basic);
			cannotRemoveReason = controller.PerformRemoveAllSplitstNoMessage();
			AssertEquals("", cannotRemoveReason);
			AssertSplitsMarkedForRemoval(controller);
		}

		void CheckSplitLine(int collectionIndex, string splitReferenceExpected, decimal weightExpected, int piecesExpected, NonPersistentSplitLineOrchestrator controller, ZString handingDetail, int numberOfPiecesReceived)
		{
			var npbo = controller.SplitsAndFlightData.SplitLines[collectionIndex];
			AssertEquals("Wrong split reference number", splitReferenceExpected, npbo.SplitNumber);
			AssertEquals("Wrong weight", weightExpected, npbo.Weight);
			AssertEquals("Wrong NPX", piecesExpected, npbo.NumberOfPieces);
			AssertEquals("Wrong handing", handingDetail, npbo.HandlingDetail);
			if (numberOfPiecesReceived > -1)
			{
				AssertEquals("Wrong NPR", numberOfPiecesReceived, npbo.NumberOfPiecesReceived);
			}
		}
	}
}
