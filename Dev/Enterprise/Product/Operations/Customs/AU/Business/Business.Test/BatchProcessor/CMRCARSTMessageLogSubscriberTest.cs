using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCARSTMessageLogSubscriber))]
	sealed class CMRCARSTMessageLogSubscriberTest : LogSubscriberTest<CMRCARSTMessageLogSubscriber>
	{
		public void TestSendingMultipleDLCMovements()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_RL_NKDestination = "AUSYD";
			hawb.CS_PiecesManifested = 10;

			var carst = CreateCARST("RCV", "", "SUBUBMOV", "NO", true);
			carst.EM_LinkTable = mawb.TableName;
			carst.EM_LinkUniqueID = mawb.PK;

			SetupDefaultPremiseIDs(carst.FlightNumber, hawb.CS_RL_NKDestination, carst.PortOfDischarge);
			carst.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("1 underbond movement should be created", 1, mawb.AllUnderbonds.Count);

			carst = CreateCARST("RCV", "", "SUBUBMOV", "NO", true, "9922W");
			carst.EM_LinkTable = mawb.TableName;
			carst.EM_LinkUniqueID = mawb.PK;

			carst.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();

			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertEquals("another underbond movement should be created", 2, mawb.AllUnderbonds.Count);
			var underbond = mawb.AllUnderbonds.Cast<CusUnderbond>().FirstOrDefault(x => x.C4_OriginPremiseID == "9922W");
			AssertEquals("Movement Reason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbond.C4_MovementReason);
			AssertEquals("Mode Of Movement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			AssertEquals("Origin Premise ID", "9922W", underbond.C4_OriginPremiseID);
			AssertEquals("Destination Premise ID", "1111A", underbond.C4_DestinationPremiseID);
			AssertEquals("Is Move From Discharge", true, underbond.C4_IsMoveFromDischarge);
			AssertEquals("Discharge Premise ID", "9922W", underbond.C4_DischargePremiseID);
			AssertEquals("Flight No", "QF33", underbond.C4_FlightNo);
			AssertEquals("Arrival Date", new ZDateTime(2006, 6, 30), underbond.C4_ArrivalDate);
			AssertEquals("Pieces Manifested", (ZShort)10, underbond.C4_PiecesManifested);

			AssertEquals("1 message should be generated", 1, underbond.Messages.Count);
			Assert("Message type", underbond.Messages[0].EM_MessageText.Contains(":UBMREQ+"));

			carst = CreateCARST("RCV", "", "SUBUBMOV", "NO", true, "9922W");
			carst.EM_LinkTable = mawb.TablePrefix;
			carst.EM_LinkUniqueID = mawb.PK;

			carst.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals("no more underbonds are created", 2, mawb.AllUnderbonds.Count);
		}

		public void TestGetDefaultDestinationPremiseIDUsingDischargePort()
		{
			var mawb = GetMAWB();
			Factory.Save();
			RunLogWalkerCycleForTest();
			var underbond = mawb.AllUnderbonds[0];
			AssertEquals("Destination Premise ID", "2222B", underbond.C4_DestinationPremiseID);
		}

		public void TestGetDefaultDestinationPremiseIDUsingDepotAddress()
		{
			var mawb = GetMAWB();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3333C", Core.Constants.CountryCodes.Australia);
			mawb.CM_OA_UnpackDepotAddress = org.MainAddress.PK;
			Factory.Save();
			RunLogWalkerCycleForTest();
			var underbond = mawb.AllUnderbonds[0];
			AssertEquals("Destination Premise ID", "3333C", underbond.C4_DestinationPremiseID);
		}

		public void TestProcessLogQueueItemWithPremiseAndNoCM_GB()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYK";

			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_GB = ZGuid.Empty;

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "YES", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("1 underbond movement should be created", 1, mAWB.AllUnderbonds.Count);

			var underbond = mAWB.AllUnderbonds[0];
			AssertEquals("Movement Reason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbond.C4_MovementReason);
			AssertEquals("Mode Of Movement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			AssertEquals("Origin Premise ID", cARST.PremiseID, underbond.C4_OriginPremiseID);
			AssertEquals("Destination Premise ID", "1111A", underbond.C4_DestinationPremiseID);
			AssertEquals("Is Move From Discharge", true, underbond.C4_IsMoveFromDischarge);
			AssertEquals("Discharge Premise ID", underbond.C4_OriginPremiseID, underbond.C4_DischargePremiseID);
			AssertEquals("Flight No", "QF33", underbond.C4_FlightNo);
			AssertEquals("Arrival Date", new ZDateTime(2006, 6, 30), underbond.C4_ArrivalDate);
			AssertEquals("Pieces Manifested", (ZShort)25, underbond.C4_PiecesManifested);
			AssertEquals("1 message should be generated", 1, underbond.Messages.Count);
			Assert("Message type", underbond.Messages[0].EM_MessageText.Contains(":UBMREQ+"));
			AssertEquals("Message Branch", newBranch.PK, underbond.Messages[0].EM_GB);

			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("No more underbond movement should be created", 1, mAWB.AllUnderbonds.Count);
		}

		public void TestProcessLogQueueItemWithPremiseWithCM_GBSet()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYK";

			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_GB = newBranch.PK;

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "YES", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("1 underbond movement should be created", 1, mAWB.AllUnderbonds.Count);

			var underbond = mAWB.AllUnderbonds[0];
			AssertEquals("Movement Reason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbond.C4_MovementReason);
			AssertEquals("Mode Of Movement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			AssertEquals("Origin Premise ID", cARST.PremiseID, underbond.C4_OriginPremiseID);
			AssertEquals("Destination Premise ID", "1111A", underbond.C4_DestinationPremiseID);
			AssertEquals("Is Move From Discharge", true, underbond.C4_IsMoveFromDischarge);
			AssertEquals("Discharge Premise ID", underbond.C4_OriginPremiseID, underbond.C4_DischargePremiseID);
			AssertEquals("Flight No", "QF33", underbond.C4_FlightNo);
			AssertEquals("Arrival Date", new ZDateTime(2006, 6, 30), underbond.C4_ArrivalDate);
			AssertEquals("Pieces Manifested", (ZShort)25, underbond.C4_PiecesManifested);
			AssertEquals("1 message should be generated", 1, underbond.Messages.Count);
			Assert("Message type", underbond.Messages[0].EM_MessageText.Contains(":UBMREQ+"));
			AssertEquals("Message Branch", newBranch.PK, underbond.Messages[0].EM_GB);

			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("No more underbond movement should be created", 1, mAWB.AllUnderbonds.Count);
		}

		public void TestProcessLogQueueItemWithDifferentDischarge()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "NO", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("1 underbond movement should be created", 1, mAWB.AllUnderbonds.Count);

			var underbond = mAWB.AllUnderbonds[0];
			AssertEquals("Movement Reason", CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination, underbond.C4_MovementReason);
			AssertEquals("Mode Of Movement", CMRUnderbondModeOfMovement.Codes.Road, underbond.C4_ModeOfMovement);
			AssertEquals("Origin Premise ID", cARST.PremiseID, underbond.C4_OriginPremiseID);
			AssertEquals("Destination Premise ID", "1111A", underbond.C4_DestinationPremiseID);
			AssertEquals("Is Move From Discharge", false, underbond.C4_IsMoveFromDischarge);
			AssertEquals("Discharge Premise ID", "2222B", underbond.C4_DischargePremiseID);
			AssertEquals("Flight No", "QF33", underbond.C4_FlightNo);
			AssertEquals("Arrival Date", new ZDateTime(2006, 6, 30), underbond.C4_ArrivalDate);
			AssertEquals("Pieces Manifested", (ZShort)25, underbond.C4_PiecesManifested);

			AssertEquals("1 message should be generated", 1, underbond.Messages.Count);
			Assert("Message type", underbond.Messages[0].EM_MessageText.Contains(":UBMREQ+"));

			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("No more underbond movement should be created", 1, mAWB.AllUnderbonds.Count);
		}

		public void TestProcessLogQueueItemColoadConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "NO", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("0 underbond movement should be created for a co-load", 0, mAWB.AllUnderbonds.Count);
		}

		public void TestProcessLogQueueItemGatewayColoadConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "NO", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("0 underbond movement should be created for a gateway co-load", 0, mAWB.AllUnderbonds.Count);
		}

		public void TestSendingUnderbondCreatesEventOnConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_JK = consol.PK;

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "NO", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;
			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);
			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			Factory.Save();
			var consolLogCollection = new StmALogDependentCollection(consol);
			consolLogCollection.Load(new ZQuery(StmALogSchema.SL_IsCancelled, "N"));
			Assert("Process failed to add an underbond submitted event to the Consol", CollectionContainsEvent(consolLogCollection, Events.UnderbondRequest));
		}

		public void TestProcessLogQueueItemWithoutPremise()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "SUBUBMOV", "YES", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("0 underbond movement should be created", 0, mAWB.AllUnderbonds.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Error in processing CARST message"));
			AssertNotNull("Failure email sent", email);
		}

		public void TestLogWalkerWhenIsMessageLinkedToSubUnderbondMovementMAWBFalse()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_RL_NKDestination = hAWB1.CS_RL_NKDestination;
			hAWB2.CS_PiecesManifested = 15;

			var cARST = CreateCARST("RCV", "", "", "YES", true);
			cARST.EM_LinkTable = CusMAWB.Schema.TableName;
			cARST.EM_LinkUniqueID = mAWB.PK;

			SetupDefaultPremiseIDs(cARST.FlightNumber, hAWB1.CS_RL_NKDestination, cARST.PortOfDischarge);

			cARST.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals("0 underbond movement should be created", 0, mAWB.AllUnderbonds.Count);
		}

		[ExpectNoExceptions]
		public void TestNonCMRCusresMessageDoesNotCauseException()
		{
			var invalidMessage = Factory.New<EDIMessage>();
			invalidMessage.EM_Status = EDIMessage.Status.Received;
			invalidMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			invalidMessage.EM_MessageText = "UNH+000001+CUSRES:D:96B:UN'UNT+2+000001'";
			invalidMessage.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();
			RunLogWalkerCycleForTest();
		}

		[ExpectNoExceptions]
		public void TestNonAUBranchCusresMessageDoesNotCauseException()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYK";
			string lb = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var invalidMessage = Factory.New<EDIMessage>();
			invalidMessage.EM_Status = EDIMessage.Status.Received;
			invalidMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			invalidMessage.EM_MessageText = "UNH+000001+CUSRES:D:96B:UN'UNT+2+000001'";
			invalidMessage.Logs.AddNew(Events.SubjectToUnderbondMovement);
			Factory.Save();
			RunLogWalkerCycleForTest();
		}

		public override void TestIsRequired()
		{
			var logSubscriber = new CMRCARSTMessageLogSubscriber();
			logSubscriber.isAlwaysRequiredForTest = false;

			var collection = new DefaultPremiseIDCollection();
			AUCustomsDataRegistry.Instance.DefaultDischargePremiseIDs = collection;
			Assert(!logSubscriber.IsRequired);

			var premiseID = collection.AddNew();
			premiseID.AirlineCode = "QF";
			premiseID.PortOfDischarge = "AUSYD";
			premiseID.PremiseID = "MEH";
			AUCustomsDataRegistry.Instance.DefaultDischargePremiseIDs = collection;
			Assert(logSubscriber.IsRequired);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";

			newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "XXX";
			newBranch.GB_RL_NKHomePort = "AUXXX";
			AUCustomsDataRegistry.Instance.BranchForAutomaticUnderbonds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXX");

			Factory.Save();
		}
		GlbBranch newBranch;

		CMRCARSTMessage CreateCARST(string status, string linkTable, string consolidatedStatus, string releasePremiseInDestination, bool isAir, string premiseID = "8553P")
		{
			var result = Factory.New<CMRCARSTMessage>();

			result.EM_Status = status;
			result.EM_LinkTable = linkTable;
			result.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+CD33 66D6 9AG:1+8'
DTM+9:20060630133540717372:ZZZ'
DTM+132:20060630:102'
FTX+AHN+++CONSOLIDATED STATUS:" + consolidatedStatus + @"'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:NO'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:" + releasePremiseInDestination + @"'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:YES'" +
(isAir ? @"TDT+20+33++6+QF::3'" : @"TDT+20+123++11++++BLAH::11'") + @"
LOC+12+AUCNS::6'
LOC+4+" + premiseID + @"::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:M00000075/CMT1::1'
RFF+MWB:08191919192'
UNT+33+000001'";

			result.EM_MessageText = result.EM_MessageText.Replace("\r\n", "");

			return result;
		}

		void SetupDefaultPremiseIDs(string flightNumber, string portOfDestination, string portOfDischarge)
		{
			SetupDefaultDestinationPremiseID(flightNumber, portOfDestination);
			SetupDefaultDischargePremiseID(flightNumber, portOfDischarge);
		}

		void SetupDefaultDestinationPremiseID(string flightNumber, string portOfDestination)
		{
			DefaultPremiseID item = DestinationCollection.AddNew();
			item.AirlineCode = flightNumber.Substring(0, 2);
			item.PortOfDischarge = portOfDestination;
			item.PremiseID = "1111A";

			AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs = DestinationCollection;
		}

		void SetupDefaultDischargePremiseID(string flightNumber, string portOfDischarge)
		{
			var item = DischargeCollection.AddNew();
			item.AirlineCode = flightNumber.Substring(0, 2);
			item.PortOfDischarge = portOfDischarge;
			item.PremiseID = "2222B";

			AUCustomsDataRegistry.Instance.DefaultDischargePremiseIDs = DischargeCollection;
		}

		DefaultPremiseIDCollection DischargeCollection
		{
			get
			{
				if (dischargeCollection == null)
				{
					dischargeCollection = new DefaultPremiseIDCollection();
				}

				return dischargeCollection;
			}
		}
		DefaultPremiseIDCollection dischargeCollection;

		DefaultDestinationPremiseIDCollection DestinationCollection
		{
			get
			{
				if (destinationCollection == null)
				{
					destinationCollection = new DefaultDestinationPremiseIDCollection();
				}

				return destinationCollection;
			}
		}
		DefaultDestinationPremiseIDCollection destinationCollection;

		CusMAWB GetMAWB()
		{
			var result = Factory.NewWithValidTestData<CusMAWB>();
			result.CM_GB = ZGuid.Empty;
			result.CM_FlightNo = "QF100";
			var hawb1 = result.ChildBills.AddNew();
			hawb1.FillWithValidTestData();
			hawb1.CS_RL_NKDestination = "AUSYD";
			var hawb2 = result.ChildBills.AddNew();
			hawb2.FillWithValidTestData();
			hawb2.CS_RL_NKDestination = "AUMEL";
			var carst = CreateCARST("RCV", "", "SUBUBMOV", "YES", true);
			carst.EM_LinkTable = CusMAWB.Schema.TableName;
			carst.EM_LinkUniqueID = result.PK;

			var premiseIDs = new DefaultDestinationPremiseIDCollection(Factory);
			var item1 = premiseIDs.AddNew();
			item1.AirlineCode = carst.FlightNumber.Substring(0, 2);
			item1.PortOfDischarge = hawb1.CS_RL_NKDestination;
			item1.PremiseID = "1111A";

			var item2 = premiseIDs.AddNew();
			item2.AirlineCode = carst.FlightNumber.Substring(0, 2);
			item2.PortOfDischarge = carst.PortOfDischarge;
			item2.PremiseID = "2222B";
			item2.UseDischargePort = true;
			AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs = premiseIDs;

			carst.Logs.AddNew(Events.SubjectToUnderbondMovement);

			return result;
		}

		bool CollectionContainsEvent(StmALogDependentCollection logCollection, Event cargoEvent)
		{
			var result = false;
			foreach (StmALog aLog in logCollection)
			{
				if (aLog.SL_SE_NKEvent == cargoEvent.Code)
				{
					result = true;
					break;
				}
			}

			return result;
		}
	}
}
