using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	abstract class NonPersistentReleaseOrchestratorTests : TestCaseWithFactory
	{
		public void TestPrintRelease_Basic()
		{
			var basic = MakeMawbOrBasicWithProfile();
			var mock = MakeNewMessageAndAddToAwb(basic, "");
			PrintTestRunnerIncludingMakeMessageForAwb(basic, basic.OutTurns, "");
			mock.VerifyAll();
		}

		protected abstract CusMAWB MakeMawbOrBasicWithProfile();

		public void TestPrintRelease_House()
		{
			var mawb = MakeMawbOrBasicWithProfile();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			var mock = MakeNewMessageAndAddToAwb(hawb, "");
			PrintTestRunnerIncludingMakeMessageForAwb(hawb, hawb.OutTurns, "");
			mock.VerifyAll();
		}

		public void TestPrintRelease_BasicSplit()
		{
			var basic = MakeMawbOrBasicWithProfile();
			var split1 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			var mock = MakeNewMessageAndAddToAwb(split1, "01");
			var split2 = basic.Splits.AddNew();
			split2.SplitReference = "02";
			PrintTestRunnerIncludingMakeMessageForAwb(split2, basic.OutTurns, "02");
			mock.VerifyAll();
		}

		public void TestPrintRelease_HouseSplit()
		{
			var mawb = MakeMawbOrBasicWithProfile();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			var split1 = hawb.Splits.AddNew();
			split1.SplitReference = "01";
			var mock1 = MakeNewMessageAndAddToAwb(split1, "01");
			var mock2 = MakeNewMessageAndAddToAwb(split1, "01");
			var split2 = hawb.Splits.AddNew();
			split2.SplitReference = "02";
			var split3 = hawb.Splits.AddNew();
			split3.SplitReference = "03";
			var mock3 = MakeNewMessageAndAddToAwb(split3, "03");
			PrintTestRunnerIncludingMakeMessageForAwb(split2, hawb.OutTurns, "02");
			mock1.VerifyAll();
			mock2.VerifyAll();
			mock3.VerifyAll();
		}

		protected void PrintTestRunnerIncludingMakeMessageForAwb(ICcsukCusAwb awb, CusOutTurnCollection outTurns, string splitReferenceForMessageApplicationReference, bool alsoMawbInboundFsnMessage = true)
		{
			GBCustomsDataRegistry.Instance.PrinterCcsuk.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, Guid.Empty); // No paper... means we'll only see eDoc version			
			awb.NumberOfPiecesExpected = 10;
			SetNprForTest(awb, 10, outTurns);
			if (alsoMawbInboundFsnMessage)
			{
				MakeNewMessageAndAddToAwb(awb, splitReferenceForMessageApplicationReference);
			}
			Factory.Save();

			ReleaseAndPrint(awb);
			AssertEquals(0, awb.NumberOfPiecesReleasedSoFarCumulative(EventCode));
			AssertReleasePrintQueued(awb, false);

			SetNumberOfPieces(awb, 2);
			ReleaseAndPrint(awb);
			AssertEquals("AWB not CW/CT status, cannot do anything, pieces not released", 0, awb.NumberOfPiecesReleasedSoFarCumulative(EventCode));
			AssertReleasePrintQueued(awb, false);

			awb.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestAccepted, ZDateTime.BrettsBirthday);
			ReleaseAndPrint(awb);
			AssertEquals("AWB has CAC but not CW/CT status, cannot do anything, pieces not released", 0, awb.NumberOfPiecesReleasedSoFarCumulative(EventCode));
			AssertReleasePrintQueued(awb, false);

			awb.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			ReleaseAndPrint(awb);
			AssertEquals(2, awb.NumberOfPiecesReleasedSoFarCumulative(EventCode));
			AssertReleasePrintQueued(awb, true, "2 OF 10");

			SetNumberOfPieces(awb, 8);
			ReleaseAndPrint(awb);
			AssertEquals("Cumulative total - 10 pieces released", 10, awb.NumberOfPiecesReleasedSoFarCumulative(EventCode));
			AssertReleasePrintQueued(awb, true, "8 OF 10");
		}

		protected abstract void SetNumberOfPieces(ICcsukCusAwb awb, ZInt value);

		protected abstract void ReleaseAndPrint(ICcsukCusAwb awb);

		protected abstract void SetNprForTest(ICcsukCusAwb awb, ZShort n, CusOutTurnCollection outTurns);

		protected Mock<EDIMessage> MakeNewMessageAndAddToAwb(ICcsukCusAwb awb, string splitReferenceForMessageApplicationReference)
		{
			var mocker = Factory.NewMoq<EDIMessage>();
			var inboundFsn = mocker.Object;
			awb.Messages.Add(inboundFsn);
			var maybeSplitSuffix = string.IsNullOrEmpty(splitReferenceForMessageApplicationReference) ? "" : "-" + splitReferenceForMessageApplicationReference;
			inboundFsn.EM_MessageText = "UNH+MSGREF+CIMFSN:0:0:IA+07412345675'FTX+CIM+++FSN:LHRKLM:074-12345675:CSN/CT" + maybeSplitSuffix + "/10/12SEP1200/ABC12532/POOP ME'UNT+3+MSGREF'";
			inboundFsn.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundFsn.EM_MessageType = CcsukTransmissionMessageFunction.CIM.Code;
			inboundFsn.EM_MessageSubType = CcsukTransmissionMessageFunction.CIM.CUKFSR.FSN.Subcode;
			inboundFsn.EM_ApplicationReference = splitReferenceForMessageApplicationReference;

			var interchange = Factory.New<EDIInterchange>();
			inboundFsn.EM_EI = interchange.PK;
			interchange.EI_To = awb.Profile;
			interchange.EI_From = "Anyone";
			interchange.EI_BodyText = inboundFsn.EM_MessageText;
			interchange.EI_ReceiveTransmit = "RCV";
			return mocker;
		}

		protected void AssertReleasePrintQueued(ICcsukCusAwb awb, bool expectAPrint, string pieceCountSuffix = "")
		{
			var query = new ZQuery();
			query.OrderBy = StmPrintJobSchema.SP_Sequence.Name + " DESC";
			var mostRecentPrint = Factory.LoadTop1<StmPrintJob>(query);
			if (expectAPrint)
			{
				AssertContains(ReleaseDocumentDescription, mostRecentPrint.SP_EmailSubjectLine);
				AssertContains(awb.ReferenceNumber, mostRecentPrint.SP_EmailSubjectLine);
				AssertContains(pieceCountSuffix, mostRecentPrint.SP_EmailSubjectLine);
			}
			else
			{
				AssertNull("Should not expect a print", mostRecentPrint);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
		}

		protected abstract Event EventCode { get; }

		protected abstract string ReleaseDocumentDescription { get; }
	}

	class NonPersistentC1ReleaseOrchestratorTests : NonPersistentReleaseOrchestratorTests
	{
		protected override Event EventCode => NumberOfPiecesReleasedHelper.AgentC1Event;

		protected override string ReleaseDocumentDescription
		{
			get { return "C1"; }
		}

		protected override CusMAWB MakeMawbOrBasicWithProfile()
		{
			var mawbOrBasic = Factory.New<CusMAWB>();
			mawbOrBasic.CM_MAWB = "125-87654321";
			mawbOrBasic.Profile = "CUKFFW98000LXA"; // agent
			return mawbOrBasic;
		}

		protected override void SetNprForTest(ICcsukCusAwb awb, ZShort n, CusOutTurnCollection outTurnsIgnore)
		{
			awb.NumberOfPiecesReceived = n;
		}

		NonPersistentC1ReleaseOrchestrator Orchestrator(ICcsukCusAwb awb)
		{
			if (orchestrator == null)
			{
				orchestrator = new NonPersistentC1ReleaseOrchestrator(awb);
			}
			return orchestrator;
		}
		NonPersistentC1ReleaseOrchestrator orchestrator;

		protected override void ReleaseAndPrint(ICcsukCusAwb awb)
		{
			Orchestrator(awb).ReleaseAndPrintC1OnFsn();
		}

		protected override void SetNumberOfPieces(ICcsukCusAwb awb, ZInt value)
		{
			Orchestrator(awb).C1ReleaseHelper.NumberOfPieces = value;
		}
	}

	class NonPersistentErtsReleaseOrchestratorTests : NonPersistentReleaseOrchestratorTests
	{
		protected override Event EventCode => NumberOfPiecesReleasedHelper.ShedEvent;

		protected override void SetNprForTest(ICcsukCusAwb awb, ZShort n, CusOutTurnCollection outTurns)
		{
			var ot = outTurns.AddNew();
			ot.C5_PackagesOutturned = n;
			ot.SplitReferenceToWhichThisPertains = awb.SplitReference;
		}

		protected override CusMAWB MakeMawbOrBasicWithProfile()
		{
			var mawbOrBasic = Factory.New<CusMAWB>();
			mawbOrBasic.CM_MAWB = "125-87654321";
			mawbOrBasic.Profile = "CUKAIR98LHRBAC";  // shed
			return mawbOrBasic;
		}

		NonPersistentErtsReleaseOrchestrator Orchestrator(ICcsukCusAwb awb)
		{
			if (orchestrator == null)
			{
				orchestrator = new NonPersistentErtsReleaseOrchestrator(awb);
			}
			return orchestrator;
		}
		NonPersistentErtsReleaseOrchestrator orchestrator;

		protected override void ReleaseAndPrint(ICcsukCusAwb awb)
		{
			Orchestrator(awb).ReleaseAndPrintRRAOnFsnOrAwb(null);
		}

		protected override void SetNumberOfPieces(ICcsukCusAwb awb, ZInt value)
		{
			Orchestrator(awb).ErtsReleaseHelper.NumberOfPieces = value;
		}

		public void TestPrintRelease_SplitBasicEUCstatusWithoutFsn()
		{
			var basic = MakeMawbOrBasicWithProfile();
			basic.ShipmentDescriptionCode = "C";
			var split = basic.Splits.AddNew();
			split.SplitReference = "02";
			PrintTestRunnerIncludingMakeMessageForAwb(split, basic.OutTurns, "", false);
			var stmPrintJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertEquals("eDocs is tied to AWB, not a message, for ERTS release of EC status job", split.PK, stmPrintJob.SP_ParentGuid);
		}

		public void TestPrintRelease_BasicEUCstatusWithoutFsn()
		{
			var basic = MakeMawbOrBasicWithProfile();
			basic.ShipmentDescriptionCode = "C";
			PrintTestRunnerIncludingMakeMessageForAwb(basic, basic.OutTurns, "", false);
			var stmPrintJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertEquals("eDocs is tied to AWB, not a message, for ERTS release of EC status job", basic.PK, stmPrintJob.SP_ParentGuid);
		}

		public void TestPrintRelease_HouseEUCstatusWithoutFsn()
		{
			var mawb = MakeMawbOrBasicWithProfile();
			mawb.ShipmentDescriptionCode = "C";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			PrintTestRunnerIncludingMakeMessageForAwb(hawb, hawb.OutTurns, "", false);
			var stmPrintJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertEquals("eDocs is tied to AWB, not a message, for ERTS release of EC status job", hawb.PK, stmPrintJob.SP_ParentGuid);
		}

		public void TestPrintRelease_SplitHouseEUCstatusWithoutFsn()
		{
			var mawb = MakeMawbOrBasicWithProfile();
			mawb.ShipmentDescriptionCode = "C";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			var split = hawb.Splits.AddNew();
			split.SplitReference = "02";
			PrintTestRunnerIncludingMakeMessageForAwb(split, hawb.OutTurns, "", false);
			var stmPrintJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertEquals("eDocs is tied to AWB, not a message, for ERTS release of EC status job", split.PK, stmPrintJob.SP_ParentGuid);
		}

		protected override string ReleaseDocumentDescription
		{
			get { return "Release/Removal Authority"; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.Company.GC_Name = "Short 4 eDoc subj.";
		}
	}
}
