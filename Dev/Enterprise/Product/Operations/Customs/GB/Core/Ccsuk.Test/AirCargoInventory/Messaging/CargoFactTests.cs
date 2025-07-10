using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;
using biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing
{
	class CargoFactCimEndToEndTests : TestCaseWithFactory
	{
		public void TestCimErrorCollection()
		{
			var ec = new ErrorCollector();
			var collection = new NonPersistentSplitLineCollection(Factory);
			var fcsMaker = new CIMFCS("", "", "", "", collection, ec);
			var constructorErrorsList = ec.GetErrorsAsString();
			AssertContains("Airport", constructorErrorsList);
			AssertContains("AWB number", constructorErrorsList);
			AssertContains("Shed code", constructorErrorsList);
			AssertContains("number of splits", constructorErrorsList);
			var line = new NonPersistentSplitLine(0, Factory);
			line.SplitNumber = "";
			line.WeightUQ = "";
			line.Weight = 0;
			collection.Add(line);
			ec = new ErrorCollector();
			fcsMaker = new CIMFCS("X", "X", "X", "X", collection, ec);
			var throwAway = fcsMaker.AllCargoImpLinesIncludingType;
			var secondSetOfErrors = ec.GetErrorsAsString();
			AssertContains("Error - SplitNumber", secondSetOfErrors);
			AssertContains("Error - WeightUQ", secondSetOfErrors);
		}

		public void TestSendCIMFRN()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.CargoTerminalOperatorAirport = "ABC";
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			hawb.CS_PiecesLanded = 0;
			hawb.CS_PiecesManifested = 15;
			CcsukInventoryICusAwbMessageSender sender = new CcsukInventoryICusAwbMessageSender();
			foreach (ICcsukCusAwb awb in new ICcsukCusAwb[] { mawb, hawb })
			{
				sender.Send(awb, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FRN("DANiel long agent but only interested in DAN"));
				AssertEquals(1, awb.Messages.Count);
				AssertContains("Generated message should contain CARGOFACT data, but its exact content is not part of this test.  See CargoFactMessageGenerationTests for that",
								"+CIMFRN", awb.Messages[0].EM_MessageText);
				AssertContains(EDIMessage.Schema.EM_MessageInterpretation, "Request renomination to another agent, DAN", awb.Messages[0].EM_MessageInterpretation);
				AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " - goes to shed", "CUKAIR98ABCKLM", awb.Messages[0].EM_ApplicationReference);
				AssertEquals(EDIMessage.Schema.EM_MessageType, "CIM", awb.Messages[0].EM_MessageType);
				AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FRN", awb.Messages[0].EM_MessageSubType);
			}
		}

		public void TestRecipientPimaForFrnAndFrdForSomeExceptionalSheds()
		{
			// Some sheds want FRNs and FRDs delivered to a special PIMA that is not in the standard format. 

			var defaultPimaMatrix = GBCustomsDataRegistry.Instance.CcsukNonStandardPimas.Value;
			defaultPimaMatrix.Add(new CcsukNonstandardPimaSetting("AAABBB", "FRD", "FAKE PIMA", Factory));
			GBCustomsDataRegistry.Instance.CcsukNonStandardPimas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultPimaMatrix);

			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			basic.NumberOfPiecesReceived = 0;
			basic.CM_ArrivalDate = ZDateTime.Empty;
			var dictForFrn = new Dictionary<ZString, ZString>();
			dictForFrn.Add("LHRBAC", "CUKAIR01LONFMBA");
			dictForFrn.Add("LHRJAS", "CUKAIR01TYOFMJL");
			dictForFrn.Add("MANHCS", "CUKAIR01MANCS8X");
			dictForFrn.Add("LHRSLS", "CUKAIR01LHRSP8X");
			dictForFrn.Add("LHRXXX", "CUKAIR98LHRXXX"); // standard
			dictForFrn.Add("AAABBB", "CUKAIR98AAABBB"); // standard, despite user map for FRD

			foreach (var pair in dictForFrn)
			{
				var expectedRecipientPima = pair.Value;
				basic.MasterLevelHouseHelper.CS_WarehouseLocation = pair.Key;
				var sender = new CcsukInventoryICusAwbMessageSender();
				sender.Send(basic, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FRN("DAN"));
				var latestMessage = basic.Messages.LastOutgoingMessage;
				AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " - goes to expected PIMA", expectedRecipientPima, latestMessage.EM_ApplicationReference);
				AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FRN", latestMessage.EM_MessageSubType);
			}

			var nonPersistentSplits = CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory);
			var dictForFrd = new Dictionary<ZString, ZString>();
			dictForFrd.Add("LHRBAC", "CUKAIR01LONFMBA");
			dictForFrd.Add("LHRJAS", "CUKAIR98LHRJAS");  // standard - c.f. FRN above
			dictForFrd.Add("MANHCS", "CUKAIR01MANCS8X");
			dictForFrd.Add("LHRSLS", "CUKAIR01LHRSP8X");
			dictForFrd.Add("LHRXXX", "CUKAIR98LHRXXX"); // standard
			dictForFrd.Add("AAABBB", "FAKE PIMA");  // user mapped
			foreach (var pair in dictForFrd)
			{
				var expectedRecipientPima = pair.Value;
				basic.MasterLevelHouseHelper.CS_WarehouseLocation = pair.Key;
				var sender = new CcsukInventoryICusAwbMessageSender();
				sender.Send(basic, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FRD(nonPersistentSplits));
				var latestMessage = basic.Messages.LastOutgoingMessage;
				AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " - goes to expected PIMA", expectedRecipientPima, latestMessage.EM_ApplicationReference);
				AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FRD", latestMessage.EM_MessageSubType);
			}
		}

		public void TestSendCIMFCS()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.CargoTerminalOperatorAirport = "ABC";
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			hawb.CS_PiecesManifested = 15;
			CcsukInventoryICusAwbMessageSender sender = new CcsukInventoryICusAwbMessageSender();
			foreach (ICcsukCusAwb awb in new ICcsukCusAwb[] { mawb, hawb })
			{
				sender.Send(awb, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FCS(CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory).SplitLines));
				AssertEquals(1, awb.Messages.Count);
				AssertContains("Generated message should contain CARGOFACT data, but its exact content is not part of this test.  See CargoFactMessageGenerationTests for that",
								"+CIMFCS", awb.Messages[0].EM_MessageText);
				AssertContains(EDIMessage.Schema.EM_MessageInterpretation, "Request splitting", awb.Messages[0].EM_MessageInterpretation);
				AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " - goes to CommDb", "CUKSYS98COMMDB", awb.Messages[0].EM_ApplicationReference);
				AssertEquals(EDIMessage.Schema.EM_MessageType, "CIM", awb.Messages[0].EM_MessageType);
				AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FCS", awb.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendCIMFRD()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			mawb.CargoTerminalOperatorAirport = "ABC";
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_PiecesLanded = 0;
			hawb.CS_HAWB = "12345678";
			hawb.CS_PiecesManifested = 15;
			CcsukInventoryICusAwbMessageSender sender = new CcsukInventoryICusAwbMessageSender();
			foreach (ICcsukCusAwb awb in new ICcsukCusAwb[] { mawb, hawb })
			{
				sender.Send(awb, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FRD(CargoFactMessageGenerationTests.GetSplitsAndFlightDataForTest(Factory)));
				AssertEquals(1, awb.Messages.Count);
				AssertContains("Generated message should contain CARGOFACT data, but its exact content is not part of this test.  See CargoFactMessageGenerationTests for that",
								"+CIMFRD", awb.Messages[0].EM_MessageText);
				AssertContains(EDIMessage.Schema.EM_MessageInterpretation, "Split request", awb.Messages[0].EM_MessageInterpretation);
				AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " - goes to shed", "CUKAIR98ABCKLM", awb.Messages[0].EM_ApplicationReference);
				AssertEquals(EDIMessage.Schema.EM_MessageType, "CIM", awb.Messages[0].EM_MessageType);
				AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FRD", awb.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendCIMFSR()
		{
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			mawb.CargoTerminalOperatorAirport = "ABC";
			var hawb = mawb.ChildBills[0];
			hawb.CS_PiecesManifested = 15;
			CcsukInventoryICusAwbMessageSender sender = new CcsukInventoryICusAwbMessageSender();
			sender.Send(mawb, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FSR());
			AssertEquals(1, mawb.Messages.Count);
			AssertContains("Generated message should contain CARGOFACT data, but its exact content is not part of this test.  See CargoFactMessageGenerationTests for that",
							"+CIMFSR", mawb.Messages[0].EM_MessageText);
			AssertContains("Mawb number is hyphenated", "801-12345678", mawb.Messages[0].EM_MessageText);
			AssertEquals(EDIMessage.Schema.EM_ApplicationReference + " - goes to shed", "CUKAIR98ABCKLM", mawb.Messages[0].EM_ApplicationReference);
			AssertEquals(EDIMessage.Schema.EM_MessageType, "CIM", mawb.Messages[0].EM_MessageType);
			AssertEquals(EDIMessage.Schema.EM_MessageSubType, "FSR", mawb.Messages[0].EM_MessageSubType);

			sender.Send(hawb, new biz.SendsMessagesToCustomsShutterUpperer(), new CcsukTransmissionMessageFunction.CIM.FSR());
			AssertEquals(1, mawb.Messages.Count);
			AssertEquals(1, hawb.Messages.Count);
			AssertContains("Mawb number is hyphenated", "801-12345678", hawb.Messages[0].EM_MessageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential();
		}
	}

	public class CargoFactMessageGenerationTests : TestCaseWithFactory
	{
		public void TestMakeCimFrn()
		{
			var ec = new ErrorCollector();
			var expectedFrnWithHouseAndSplit = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFRN:0:0:IA+<<SYSCAR>>'FTX+CIM+++FRN:LHRTWA:015-12345675-09124361:SPT/02:	AGT/ABC'UNT+3+<<MSGNO PLACEHOLDER>>'";
			var expectedFrnWithHouse = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFRN:0:0:IA+<<SYSCAR>>'FTX+CIM+++FRN:LHRTWA:015-12345675-09124361:			AGT/ABC'UNT+3+<<MSGNO PLACEHOLDER>>'";
			var expectedFrnMasterOnly = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFRN:0:0:IA+<<SYSCAR>>'FTX+CIM+++FRN:LHRTWA:015-12345675:					AGT/ABC'UNT+3+<<MSGNO PLACEHOLDER>>'";

			var frnWrapper = new CIMFRN("LHR", "TWA", "015-12345675", "09124361", "02", "ABC", ec);
			var cargoFact = new CargoFACTWrapper(frnWrapper).Wrap();
			AssertEquals(expectedFrnWithHouseAndSplit.Replace("\t", ""), cargoFact);

			frnWrapper = new CIMFRN("LHR", "TWA", "015-12345675", "", "", "ABC", ec);
			cargoFact = new CargoFACTWrapper(frnWrapper).Wrap();
			AssertEquals(expectedFrnMasterOnly.Replace("\t", ""), cargoFact);

			frnWrapper = new CIMFRN("LHR", "TWA", "015-12345675", "09124361", "", "ABC", ec);
			cargoFact = new CargoFACTWrapper(frnWrapper).Wrap();
			AssertEquals(expectedFrnWithHouse.Replace("\t", ""), cargoFact);
		}

		public void TestMakeCimFcs()
		{
			var ec = new ErrorCollector();
			var expectedFrnWithHouse = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFCS:0:0:IA+<<SYSCAR>>'FTX+CIM+++FCS:LHRTWA:015-12345675-87654321:SPT/01P2K33:SPT/02P2K12.67'FTX+CIM+++SPT/03P1K10'UNT+4+<<MSGNO PLACEHOLDER>>'";
			var expectedFrnMasterOnly = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFCS:0:0:IA+<<SYSCAR>>'FTX+CIM+++FCS:LHRTWA:015-12345675:			SPT/01P2K33:SPT/02P2K12.67'FTX+CIM+++SPT/03P1K10'UNT+4+<<MSGNO PLACEHOLDER>>'";

			var splits = GetSplitsAndFlightDataForTest(Factory);

			var fcsWrapper = new CIMFCS("LHR", "TWA", "015-12345675", "", splits.SplitLines, ec);
			var cargoFact = new CargoFACTWrapper(fcsWrapper).Wrap();
			AssertEquals(expectedFrnMasterOnly.Replace("\t", ""), cargoFact);

			fcsWrapper = new CIMFCS("LHR", "TWA", "015-12345675", "87654321", splits.SplitLines, ec);
			cargoFact = new CargoFACTWrapper(fcsWrapper).Wrap();
			AssertEquals(expectedFrnWithHouse.Replace("\t", ""), cargoFact);

			splits.SplitLines[0].Weight = 0;
			splits.SplitLines[0].WeightUQ = "X";
			splits.SplitLines[0].SplitNumber = "";

			ec = new ErrorCollector();
			fcsWrapper = new CIMFCS("LHR", "TWA", "015-12345675", "87654321", splits.SplitLines, ec);
			cargoFact = new CargoFACTWrapper(fcsWrapper).Wrap();
			var errorsReported = ec.GetErrorsAsString();
			AssertContains("Error - Weight", errorsReported);
			AssertContains("Error - WeightUQ", errorsReported);
			AssertContains("Error - Split", errorsReported);
		}

		public void TestMakeCimFrd()
		{
			var ec = new ErrorCollector();
			var expectedFrdMasterOnly = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFRD:0:0:IA+<<SYSCAR>>'FTX+CIM+++FRD:LHRTWA:015-12345675:ARR/BA123/18SEP:AGT/XYZ'FTX+CIM+++SPT/01P2K33:SPT/02P2K12.67/HANDLE WITH CARE:SPT/03P1K10/DANIEL'UNT+4+<<MSGNO PLACEHOLDER>>'";
			var splits = GetSplitsAndFlightDataForTest(Factory);
			splits.SendFlightInfoToo = true;
			var frdWrapper = new CIMFRD("LHR", "TWA", "015-12345675", "", splits, ec, "XYZ");
			var cargoFact = new CargoFACTWrapper(frdWrapper).Wrap();
			AssertEquals(expectedFrdMasterOnly.Replace("\t", ""), cargoFact);
		}

		public void TestSendCIMFRDToRemoveAllSplits()
		{
			var ec = new ErrorCollector();
			var frdMaker = new CIMFRD("X", "X", "X", "X", CargoFactMessageGenerationTests.GetSplitsAndFlightDataForRemoveAllTest(Factory), ec, "X");
			var throwAway = frdMaker.AllCargoImpLinesIncludingType;
			var secondSetOfErrors = ec.GetErrorsAsString();
			AssertNotContains("weight details", secondSetOfErrors);
		}

		public void TestCIMFRDSendingOfFlightData()
		{
			var ec = new ErrorCollector();
			var splits = GetSplitsAndFlightDataForTest(Factory);
			splits.SendFlightInfoToo = true;
			var frdWrapper = new CIMFRD("LHR", "TWA", "015-12345675", "", splits, ec, "XYZ");
			var cargoFact = new CargoFACTWrapper(frdWrapper).Wrap();
			AssertEquals("Flight Data should be sent in message as the send Flight data check box was ticked", expectedFrdWithFlightData.Replace("\t", ""), cargoFact);
			splits.SendFlightInfoToo = false;
			frdWrapper = new CIMFRD("LHR", "TWA", "015-12345675", "", splits, ec, "XYZ");
			cargoFact = new CargoFACTWrapper(frdWrapper).Wrap();
			AssertEquals("Flight Data should not be sent in message as the send Flight data check box was not ticked", expectedFrdWithoutFlightData.Replace("\t", ""), cargoFact);
		}

		public void TestMakeCimFsr()
		{
			var ec = new ErrorCollector();
			var expectedFsr = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFSR:0:0:IA+<<SYSCAR>>'FTX+CIM+++FSR:015-12345675'UNT+3+<<MSGNO PLACEHOLDER>>'";
			var wrapper = new CIMFSR("015-12345675", ec);
			var cargoFact = new CargoFACTWrapper(wrapper).Wrap();
			AssertEquals(expectedFsr, cargoFact);
		}

		public static NonPersistentSplitsAndFlightData GetSplitsAndFlightDataForTest(BusinessObjectFactory factory)
		{
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData(factory);
			var splits = npSplitsAndFlightData.SplitLines;
			AddSplit(splits, "01", 2, 33m, "K", "", factory);
			AddSplit(splits, "02", 2, 12.67m, "K", "Handle with care", factory);
			AddSplit(splits, "03", 1, 10m, "K", "Daniel", factory);
			npSplitsAndFlightData.FlightNumber = "BA123";
			npSplitsAndFlightData.FlightArrivalDate = ZDateTime.BrettsBirthday;
			return npSplitsAndFlightData;
		}

		internal static NonPersistentSplitsAndFlightData GetSplitsAndFlightDataForRemoveAllTest(BusinessObjectFactory factory)
		{
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData(factory);
			var splits = npSplitsAndFlightData.SplitLines;
			AddSplit(splits, "01", 5, 12388.67m, "K", "", factory);
			AddSplit(splits, "02", 0, 0m, "K", "To delete", factory);
			AddSplit(splits, "03", 0, 0m, "K", "To delete", factory);
			return npSplitsAndFlightData;
		}

		static void AddSplit(NonPersistentSplitLineCollection splitCollection, string splitNumber, short pieces, decimal weight, string weightUnits, string handlingDetail, BusinessObjectFactory factory)
		{
			splitCollection.Add(new NonPersistentSplitLine(splitNumber, weightUnits, pieces, weight, handlingDetail, "", factory));
		}

		internal const string expectedFrdWithFlightData = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFRD:0:0:IA+<<SYSCAR>>'FTX+CIM+++FRD:LHRTWA:015-12345675:ARR/BA123/18SEP:AGT/XYZ'FTX+CIM+++SPT/01P2K33:SPT/02P2K12.67/HANDLE WITH CARE:SPT/03P1K10/DANIEL'UNT+4+<<MSGNO PLACEHOLDER>>'";
		internal const string expectedFrdWithoutFlightData = @"UNH+<<MSGNO PLACEHOLDER>>+CIMFRD:0:0:IA+<<SYSCAR>>'FTX+CIM+++FRD:LHRTWA:015-12345675:AGT/XYZ:SPT/01P2K33'FTX+CIM+++SPT/02P2K12.67/HANDLE WITH CARE:SPT/03P1K10/DANIEL'UNT+4+<<MSGNO PLACEHOLDER>>'";
	}
}
