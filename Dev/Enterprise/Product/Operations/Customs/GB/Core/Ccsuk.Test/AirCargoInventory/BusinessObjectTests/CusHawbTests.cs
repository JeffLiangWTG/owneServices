using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusHAWB))]
	class CusHawbTests : EnterpriseBusinessObjectTestCase
	{
		public void TestProcessTaskCollection_InheritFromGeneric()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals("WorkflowItems's type should inherit from ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>", typeof(ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>), ((IWorkflowProvider)hawb).WorkflowItems.GetType().BaseType);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntSetStatus1DateWithoutPieces()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.Profile = "CUKAIR98LHRBAC";
			mawb.AgentBadge = "LXA";
			mawb.NumberOfPiecesExpected = 0;
			hawb.CS_PiecesManifested = 0;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, hawb.Status1Date);
			AssertEquals(ZDateTime.Empty, mawb.Status1Date);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntResetStatus1Date()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.Profile = "CUKAIR98LHRBAC";
			mawb.AgentBadge = "LXA";
			mawb.NumberOfPiecesExpected = 10;
			hawb.CS_PiecesManifested = 10;
			Factory.Save();

			var ot = hawb.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 10;
			Factory.Save();
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 0, 0), hawb.Status1Date);

			var djc = new ZDateTime(1979, 8, 9, 9, 56, 0);
			hawb.Status1Date = djc;
			AssertEquals(djc, hawb.Status1Date);
			Factory.Save();
			AssertEquals(djc, hawb.Status1Date);
		}

		public void TestDeactivate()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();

			hawb2.DeactivateByWtg();
			AssertEquals(false, hawb2.CS_IsActive);
			AssertEquals(true, hawb1.CS_IsActive);

			mawb.DeactivateByWtg();
			AssertEquals(false, hawb2.CS_IsActive);
			AssertEquals(false, hawb1.CS_IsActive);
			AssertEquals(false, mawb.CM_IsActive);
			AssertEquals(false, mawb.MasterLevelHouseHelper.CS_IsActive);

			GlbStaff.CurrentUser.GS_LoginName = "John Locke";
			// Don't do anything when not WTG:
			mawb = Factory.New<CusMAWB>();
			hawb1 = mawb.ChildBills.AddNew();
			hawb2 = mawb.ChildBills.AddNew();
			hawb2.DeactivateByWtg();
			AssertEquals(true, hawb2.CS_IsActive);
			mawb.DeactivateByWtg();
			AssertEquals(true, hawb2.CS_IsActive);
			AssertEquals(true, hawb1.CS_IsActive);
			AssertEquals(true, mawb.CM_IsActive);
			AssertEquals(true, mawb.MasterLevelHouseHelper.CS_IsActive);
		}

		public void TestParameterSourceExceptionOn()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWBForTest>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.NumberOfPiecesExpected = 10;
			Factory.Save();
			var basicPieces = basic.OutTurns.AddNew();
			basicPieces.C5_PackagesOutturned = 10;
			basicPieces.C5_PackagesUnits = "X";
			basicPieces.C5_MarksAndNumbers = "M";
			basicPieces.C5_GoodsDescription = "D";
			basicPieces.C5_CargoReceiptDate = ZDateTime.BrettsBirthday;
			basicPieces.C5_CargoUnpackDate = ZDateTime.BrettsBirthday.AddDays(1);
			basicPieces.C5_ContainerNumber = "C";
			basicPieces.C5_ContainerSeal = "S";
			basicPieces.C5_DamageIndicator = true;
			basicPieces.WarehouseLocationID = ZGuid.NewZGuid();
			Factory.Save();
			Assert(!basic.Status1Date.IsEmpty);
			var newHouse1 = basic.ChildBills.AddNew();

			newHouse1.CS_PiecesManifested = 3;
			basic.OutTurns.RemoveAndDeleteAll();
			basic.OutTurns = null;
			basic.ChildBills.AddNew();
			AssertNoExceptionThrown(() => newHouse1.CS_PiecesManifested = 4);
		}

		public void TestCascadeBasicMawbStatus1ToNewHouses()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.NumberOfPiecesExpected = 10;
			Factory.Save();
			var basicPieces = basic.OutTurns.AddNew();
			basicPieces.C5_PackagesOutturned = 10;
			basicPieces.C5_PackagesUnits = "X";
			basicPieces.C5_MarksAndNumbers = "M";
			basicPieces.C5_GoodsDescription = "D";
			basicPieces.C5_CargoReceiptDate = ZDateTime.BrettsBirthday;
			basicPieces.C5_CargoUnpackDate = ZDateTime.BrettsBirthday.AddDays(1);
			basicPieces.C5_ContainerNumber = "C";
			basicPieces.C5_ContainerSeal = "S";
			basicPieces.C5_DamageIndicator = true;
			basicPieces.WarehouseLocationID = ZGuid.NewZGuid();
			Factory.Save();
			Assert(!basic.Status1Date.IsEmpty);
			var newHouse1 = basic.ChildBills.AddNew();
			newHouse1.CS_PiecesManifested = 3;
			newHouse1.CS_PiecesManifested = 4;
			Assert(!newHouse1.Status1Date.IsEmpty);
			AssertEquals((ZShort)4, newHouse1.CS_PiecesLanded);
			var newHouse2 = basic.ChildBills.AddNew();
			newHouse2.CS_PiecesManifested = 6;
			Assert(!newHouse2.Status1Date.IsEmpty);
			AssertEquals((ZShort)6, newHouse2.CS_PiecesLanded);
			Factory.Save();
			AssertEquals((ZShort)10, basic.NumberOfPiecesReceived);
			var newHouse3 = basic.ChildBills.AddNew();
			newHouse3.CS_PiecesManifested = 10;
			AssertEquals("Not cascaded after basic is saved with houses", (ZShort)0, newHouse3.CS_PiecesLanded);
			var houseOT = newHouse1.OutTurns[0];
			AssertEquals(basicPieces.C5_PackagesUnits, houseOT.C5_PackagesUnits);
			AssertEquals(basicPieces.C5_MarksAndNumbers, houseOT.C5_MarksAndNumbers);
			AssertEquals(basicPieces.C5_GoodsDescription, houseOT.C5_GoodsDescription);
			AssertEquals(basicPieces.C5_CargoReceiptDate, houseOT.C5_CargoReceiptDate);
			AssertEquals(basicPieces.C5_CargoUnpackDate, houseOT.C5_CargoUnpackDate);
			AssertEquals(basicPieces.C5_ContainerNumber, houseOT.C5_ContainerNumber);
			AssertEquals(basicPieces.C5_ContainerSeal, houseOT.C5_ContainerSeal);
			AssertEquals(basicPieces.C5_DamageIndicator, houseOT.C5_DamageIndicator);
			AssertEquals(basicPieces.WarehouseLocationID, houseOT.WarehouseLocationID);

			// Check it doesn't work with multiple outturns
			var basic2 = Factory.New<CusMAWB>();
			basic2.Profile = "CUKAIR98LHRBAC";
			basic2.NumberOfPiecesExpected = 10;
			Factory.Save();
			var basic2Pieces1 = basic2.OutTurns.AddNew();
			basic2Pieces1.C5_PackagesOutturned = 4;
			var basic2Pieces2 = basic2.OutTurns.AddNew();
			basic2Pieces2.C5_PackagesOutturned = 6;
			Factory.Save();
			Assert(!basic2.Status1Date.IsEmpty);
			var newHouse4 = basic2.ChildBills.AddNew();
			newHouse4.CS_PiecesManifested = 4;
			Assert(newHouse4.Status1Date.IsEmpty);

			// Check it doesn't work with agent PIMA once LOCAL shed has set basic's St1
			var basic3 = Factory.New<CusMAWB>();
			basic3.Profile = "CUKAIR98LHRBAC";
			basic3.AgentBadge = "LXA";
			basic3.NumberOfPiecesExpected = 10;
			Factory.Save();
			var basic3Pieces = basic3.OutTurns.AddNew();
			basic3Pieces.C5_PackagesOutturned = 10;
			Factory.Save();
			Assert(!basic3.Status1Date.IsEmpty);
			basic3.Profile = "CUKFFW98000LXA";
			var newHouse5 = basic3.ChildBills.AddNew();
			newHouse5.CS_PiecesManifested = 4;
			Assert(newHouse5.Status1Date.IsEmpty);

			// Check it doesn't work with agent PIMA once LOCAL shed has set basic's St1 - even if we save after changing profile
			var basic3B = Factory.New<CusMAWB>();
			basic3B.Profile = "CUKAIR98LHRBAC";
			basic3B.AgentBadge = "LXA";
			basic3B.NumberOfPiecesExpected = 10;
			Factory.Save();
			var basic3BPieces = basic3B.OutTurns.AddNew();
			basic3BPieces.C5_PackagesOutturned = 10;
			Factory.Save();
			Assert(!basic3B.Status1Date.IsEmpty);
			basic3B.Profile = "CUKFFW98000LXA";
			Factory.Save(); // This is the difference 
			var newHouse5B = basic3B.ChildBills.AddNew();
			newHouse5B.CS_PiecesManifested = 4;
			Assert(newHouse5B.Status1Date.IsEmpty);

			// Check it doesn't work if basic has pieces but not status 1
			var basic4 = Factory.New<CusMAWB>();
			basic4.Profile = "CUKAIR98LHRBAC";
			basic4.NumberOfPiecesExpected = 11;
			Factory.Save();
			var basic4Pieces = basic4.OutTurns.AddNew();
			basic4Pieces.C5_PackagesOutturned = 10;
			Factory.Save();
			Assert(basic4.Status1Date.IsEmpty);
			var newHouse6 = basic4.ChildBills.AddNew();
			newHouse6.CS_PiecesManifested = 11;
			Assert(newHouse6.Status1Date.IsEmpty);
			AssertEquals((ZShort)0, newHouse6.CS_PiecesLanded);
		}

		public void TestIWorkflowTriggerEventSource()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var providersForHouse = ((IWorkflowTriggerEventSource)hawb).ParentWorkflowProviders;
			var providersForWorker = ((IWorkflowTriggerEventSource)mawb.MasterLevelHouseHelper).ParentWorkflowProviders;
			Assert("Worker house shows mawb as a parent workflow helper", providersForWorker.Contains(mawb));
			Assert("Hawb doesn't show mawb as a parent workflow helper", !providersForHouse.Contains(mawb));
			AssertEquals(hawb.Branch.Company, hawb.JobHeaderCompany);
		}

		public void TestWorkflowTriggerForStatus1()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var hawbWorkflow = (IWorkflowProvider)hawb;
			var triggerStatus1 = hawbWorkflow.WorkflowItems.AddNew();
			triggerStatus1.P9_Description = "TestStatus1";
			triggerStatus1.TriggerConditions.TriggerEventCode = Events.StatusChange.Code;
			triggerStatus1.TriggerConditions.TriggerCondition = "REF";
			triggerStatus1.TriggerConditions.TriggerConditionValue = "ST1";
			triggerStatus1.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			var triggerIrrelevant = hawbWorkflow.WorkflowItems.AddNew();
			triggerIrrelevant.P9_Description = "Irrelevant";
			triggerIrrelevant.TriggerConditions.TriggerCondition = "REF";
			triggerIrrelevant.TriggerConditions.TriggerConditionValue = "XXX";
			triggerIrrelevant.TriggerConditions.TriggerEventCode = Events.StatusChange.Code;
			triggerIrrelevant.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			var notificationStatus1 = triggerStatus1.ProcessTaskNotifications.AddNew();
			notificationStatus1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notificationStatus1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notificationStatus1.PQ_EmailAddr = "status1@domain.com";

			hawb.Status1Date = ZDateTime.BrettsBirthday;

			AssertEquals(ZDateTime.BrettsBirthday, triggerStatus1.P9_ActualDate.ToZDateTime());
			AssertEquals(ZDateTime.Empty, triggerIrrelevant.P9_ActualDate.ToZDateTime());
		}

		[ExpectNoExceptions]
		public void TestNoConcurrencyErrors()
		{
			NoConcurrencyErrorsTest(nameof(CusHAWB.PresenceOnNetworkStatus), new object[] { (ZString)"1", (ZString)"2" });
			NoConcurrencyErrorsTest(nameof(CusHAWBSchema.CS_Weight), new object[] { (ZDecimal)1.0, (ZDecimal)2.0 });
			NoConcurrencyErrorsTest(nameof(CusHAWBSchema.CS_PiecesLanded), new object[] { (ZShort)1, (ZShort)2 });
		}

		void NoConcurrencyErrorsTest(ZString propertyName, object[] testvalues)
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			PropertyInfo property = hawb.GetType().GetProperty(propertyName);
			property.SetValue(hawb, testvalues[0]);

			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false; // as per back-endsender
			var backEndHawb = newFactory.Load<CusHAWB>(hawb.PK);
			PropertyInfo property2 = backEndHawb.GetType().GetProperty(propertyName);
			property2.SetValue(backEndHawb, testvalues[1]);

			newFactory.Save();
			Factory.Save();
		}

		public void TestChangingPortsWipesEcStatus()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			CusMawbTests.RunTestChaningPortWipesEcStatus(house);
		}

		public void TestHasSplitsOnMAWBWorksWhetherBasicOrNot()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals(true, mawb.IsBasic);
			var split = mawb.Splits.AddNew();
			AssertEquals(true, mawb.IsBasic);
			AssertEquals(true, mawb.HasSplits);
			AssertEquals(1, mawb.Splits.Count);

			var hawb = mawb.ChildBills.AddNew();
			AssertEquals(false, mawb.IsBasic);
			AssertEquals(true, mawb.HasSplits);
			AssertEquals(1, mawb.Splits.Count);
		}

		public void TestHasSplitsAndSplitReferencesForAllAwbsModuleGrid()
		{
			var mawb = Factory.New<CusMAWB>();
			var realHawb = mawb.ChildBills.AddNew();
			AssertEquals(false, realHawb.HasSplits);
			AssertEquals(false, mawb.HasSplits);
			AssertEquals(false, mawb.MasterLevelHouseHelper.HasSplits);
			var srf01 = realHawb.Splits.AddNew();
			srf01.SplitReference = "01";
			AssertEquals(true, realHawb.HasSplits);
			AssertEquals(false, mawb.HasSplits);
			AssertEquals(false, mawb.MasterLevelHouseHelper.HasSplits);
			AssertEquals("SRF-01 ", realHawb.SplitReferencesForAllAwbsModuleGrid);

			var basic = Factory.New<CusMAWB>();
			AssertEquals(false, basic.HasSplits);
			AssertEquals(false, basic.MasterLevelHouseHelper.HasSplits);
			var srf01b = basic.Splits.AddNew();
			srf01b.SplitReference = "01";
			AssertEquals(true, basic.HasSplits);
			AssertEquals(true, basic.MasterLevelHouseHelper.HasSplits);
			AssertEquals("SRF-01 ", basic.MasterLevelHouseHelper.SplitReferencesForAllAwbsModuleGrid);

			var srf02 = realHawb.Splits.AddNew();
			srf02.SplitReference = "02";
			AssertEquals("SRF-01 ; SRF-02 ", realHawb.SplitReferencesForAllAwbsModuleGrid);
			srf01.LatestCustomsActionText = "OK Transfer POO";
			AssertEquals("SRF-01 OK Transfer POO; SRF-02 ", realHawb.SplitReferencesForAllAwbsModuleGrid);
		}

		public void TestDisplayTextForCustomsCargoStatusColumn()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "DANIEL01";
			hawb.CargoTerminalOperator = "BAC";
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.PresenceOnNetworkStatus = "ABC";
			AssertEquals("Presence ABC @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
			hawb.LatestCustomsActionText = "OK Transfer";
			AssertEquals("OK Transfer @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "DANIEL01";
			hawb.CS_JS = shipment.PK;
			AssertEquals("OK Transfer @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
			shipment.JS_HouseBill = "SOMETHING EXTERNAL";
			AssertEquals("DANIEL01 OK Transfer @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
			hawb.LatestCustomsActionText = "";
			AssertEquals("DANIEL01 Presence ABC @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
			shipment.Delete();
			AssertEquals("Presence ABC @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
			hawb.CS_PiecesLanded = 10;
			hawb.CS_PiecesManifested = 10;
			hawb.Status1Date = ZDateTime.BrettsBirthday;
			AssertEquals("Presence ABC @ LHRBAC St1", hawb.DisplayTextForCustomsCargoStatusColumn);
			hawb.CS_PiecesManifested = 11;
			AssertEquals("Presence ABC @ LHRBAC", hawb.DisplayTextForCustomsCargoStatusColumn);
		}

		void SetUpHawbForMakeDeclarationTest(out CusMAWB mawb, out CusHAWB house)
		{
			mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "12512345678";
			mawb.CM_FlightNo = "BA123";
			house = mawb.ChildBills.AddNew();
			house.CS_HAWB = "HAWB1234";
			house.Profile = "CUKFFW98000XXX";
			house.CS_PiecesManifested = 10;
		}

		static void AssertDeclarationProperties(CusHAWB house, JobDeclaration declaration)
		{
			Assert("Factory was saved as part of create, so that the refrewsh of the DUCR link works", declaration.IsInDatabase);
			AssertEquals("IMP", declaration.JE_MessageType);
			AssertEquals("AIR", declaration.JE_TransportMode);
			AssertEquals("HAWB1234", declaration.JE_HouseBill);
			AssertEquals("12512345678", declaration.JE_MasterBill);
			AssertEquals("HSE", declaration.ZG_ShipmentType);
			AssertEquals("BA123", declaration.JE_VoyageFlightNo);
			AssertEquals(declaration.PK, house.CS_JE_CustomsFormalEntry);
			AssertEquals("Box 6 set from NPX, not NPR", 10, declaration.JE_TotalNoOfPacks);
		}

		public void TestCreateNewStandaloneChiefDeclaration_Arrival()
		{
			CusMAWB mawb;
			CusHAWB house;
			SetUpHawbForMakeDeclarationTest(out mawb, out house);
			mawb.CM_ArrivalDate = new ZDateTime(1986, 3, 12, 4, 27, 0);  //arrived
			house.CS_PiecesLanded = 9;                                  //arrived
			var declaration = ((ICcsukCusAwb)house).CreateNewStandaloneCDSDeclaration();
			AssertDeclarationProperties(house, declaration);
			AssertEquals("Arrived", "A", declaration.JE_EntrySubStyle);
			AssertEquals("CDS declaration", "CDS", declaration.JE_ApplicationCode);
			AssertEquals("Default Declaration Type", "H1", declaration.JE_DeclarationType);
		}

		[ExpectNoExceptions]
		public void TestAddNewHouseToMawbWithDuffBranchAndDuffPimaDoesNotExplode()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_GB = ZGuid.NewZGuid();//invalid
			mawb.Profile = "XX"; // neither an agent or shed style pima, so we look to see if it's a fallback pima
			mawb.AirportOfArrival = "LHR";
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals("SetDefaultsForNewChild() on the mawb's hawbs collection made it past the Pima-setting part without exploding", "LHR", hawb.AirportOfArrival);
		}

		public void TestCreateNewStandaloneChiefDeclaration_PreArrival()
		{
			CusMAWB mawb;
			CusHAWB house;
			SetUpHawbForMakeDeclarationTest(out mawb, out house);
			var declaration = ((ICcsukCusAwb)house).CreateNewStandaloneCDSDeclaration();
			AssertDeclarationProperties(house, declaration);
			AssertEquals("Not arrived", "D", declaration.JE_EntrySubStyle);
		}

		public void TestCacCuIsWipedWhenEditingSomeFields()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRXXX";
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(hawb, delegate
			{ hawb.AirportOfOrigin = "NEW"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(hawb, delegate
			{ hawb.CS_HAWB = "NEW"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(hawb, delegate
			{ hawb.CS_GoodsDescription = "NEW"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(hawb, delegate
			{ hawb.AirportOfDestination = "NEW"; });
			// No changes when not a shed
			hawb.Profile = "CUKFFW98000XXX";
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.AirportOfOrigin = "NEW2"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.CS_HAWB = "NEW2"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.CS_GoodsDescription = "NEW2"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.AirportOfDestination = "NEW2"; });
			// No changes value edited but no actual change
			hawb.Profile = "CUKAIR98LHRXXX";
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.AirportOfOrigin = "NEW2"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.CS_HAWB = "0000NEW2"; });  // the zeros were added during the last set
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.CS_GoodsDescription = "NEW2"; });
			RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(hawb, delegate
			{ hawb.AirportOfDestination = "NEW2"; });
		}

		internal static void RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(ICcsukCusAwb awb, Action setNewValue)
		{
			awb.LatestCustomsActionText = "TEST";
			awb.SetCustomsActionCode("CU", ZDateTime.BrettsBirthday);
			setNewValue();
			AssertEquals("CU", awb.CustomsActionCode);
			AssertEquals("TEST", awb.LatestCustomsActionText);
			AssertEquals(ZDateTime.BrettsBirthday, awb.CustomsActionDate);
		}

		internal static void RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(ICcsukCusAwb awb, Action setNewValue)
		{
			awb.LatestCustomsActionText = "TEST";
			awb.SetCustomsActionCode("CU", ZDateTime.BrettsBirthday);
			setNewValue();
			AssertEquals("", awb.CustomsActionCode);
			AssertNotEquals("TEST", awb.LatestCustomsActionText);
			AssertEquals(ZDateTime.Empty, awb.CustomsActionDate);
		}

		public void TestSetEcStatusRelease()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			RunSetEcStatusReleaseTest(hawb, hawb.ShipmentDescriptionCodeInfo, hawb.OutTurns.AddNew());
		}

		internal static void RunSetEcStatusReleaseTest(ICcsukCusAwb awb, ZPropertyInfo sdcInfo, CusOutTurn ot)
		{
			awb.PresenceOnNetworkStatus = "XYZ";
			awb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport;
			ot.IsReleasedAlready = true;
			AssertEquals("", awb.CustomsActionCode);
			if (sdcInfo != null)
			{
				AssertEquals("SDC should not be locked", false, sdcInfo.ReadOnly);
			}
			awb.SetEcStatusRelease(true);
			if (sdcInfo != null)
			{
				AssertEquals("SDC should be locked", true, sdcInfo.ReadOnly);
			}
			AssertEquals(CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease, awb.CustomsActionCode);
			AssertEquals("Presence unchanged", "XYZ", awb.PresenceOnNetworkStatus);
			AssertEquals("EC Status", awb.LatestCustomsActionText);
			var bizO = awb as EnterpriseBusinessObject;
			AssertEquals(1, bizO.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "EC status release set by CargoWise Support")).Length);
			awb.SetEcStatusRelease(false);
			AssertEquals("", awb.LatestCustomsActionText);
			AssertEquals("", awb.CustomsActionCode);
			AssertEquals(1, bizO.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "EC status release revoked by CargoWise Support")).Length);
			AssertEquals("Unsetting EC status should un-release any out turns", false, ot.IsReleasedAlready);
			if (sdcInfo != null)
			{
				AssertEquals("SDC should not be locked", false, sdcInfo.ReadOnly);
			}
			AssertEquals("Presence unchanged", "XYZ", awb.PresenceOnNetworkStatus);
			awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.CompletedOnCcsUk;
			awb.SetEcStatusRelease(false);
			AssertEquals("When COM, unsetting should change presence to YES", PresenceOnNetworkList.Codes.OnCommDb, awb.PresenceOnNetworkStatus);
		}

		public void TestUnsetEcStatusWipesOldEdocs()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "33333333";
			var eDocEcRra = hawb.DocManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Daniel"), "RRA.txt", "RRA");
			var eDocIrrelevant = hawb.DocManagerInfo.AddFileOrDocument(ZBlob.FromAscii("Clarke"), "Foo.txt", "GRA");
			eDocEcRra.Description = "Comapny - Branch - Release/Removal Authority for 111-22222222-33333333 reprint blah";
			eDocIrrelevant.Description = "Company - Branch - Whatever";
			Factory.Save();
			((ICcsukCusAwb)hawb).SetEcStatusRelease(false);
			Factory.Save();
			var hawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals(1, hawbReloaded.DocManagerInfo.AllEDocs.Count);
		}

		public void TestLoaderFindsCorrectHawb()
		{
			var mawbIrrelevant1 = Factory.New<CusMAWB>();
			mawbIrrelevant1.CM_MAWB = "11111111111";
			var hawbIrrelevant1 = mawbIrrelevant1.ChildBills.AddNew();
			hawbIrrelevant1.CS_HAWB = "DANIEL01";
			var mawbIrrelevant2 = Factory.New<CusMAWB>();
			mawbIrrelevant2.CM_MAWB = "22222222222";
			var hawbIrrelevant2 = mawbIrrelevant2.ChildBills.AddNew();
			hawbIrrelevant2.CS_HAWB = "DANIEL01";
			var irrelevantSplitHawb21 = hawbIrrelevant2.Splits.AddNew();
			var irrelevantSplitHawb22 = hawbIrrelevant2.Splits.AddNew();
			irrelevantSplitHawb21.SplitReference = "01";
			irrelevantSplitHawb22.SplitReference = "02";

			var goodMawb = Factory.New<CusMAWB>();
			goodMawb.CM_MAWB = "33333333333";
			var goodHawb = goodMawb.ChildBills.AddNew();
			goodHawb.CS_HAWB = "DANIEL01";

			Factory.Save();

			AssertEquals("333-33333333-DANIEL01", new CusHAWB.Loader(new BusinessObjectFactory()).FindHawb("DANIEL01", "", "33333333333").ReferenceNumber);
			AssertEquals("222-22222222-DANIEL01", new CusHAWB.Loader(new BusinessObjectFactory()).FindHawb("DANIEL01", "01", "22222222222").ReferenceNumber);

			var goodSplitHawb1 = goodHawb.Splits.AddNew();
			var goodSplitHawb2 = goodHawb.Splits.AddNew();
			goodSplitHawb1.SplitReference = "01";
			goodSplitHawb2.SplitReference = "02";
			Factory.Save();
			AssertEquals("333-33333333-DANIEL01", new CusHAWB.Loader(new BusinessObjectFactory()).FindHawb("DANIEL01", "01", "33333333333").ReferenceNumber);
			AssertEquals("222-22222222-DANIEL01", new CusHAWB.Loader(new BusinessObjectFactory()).FindHawb("DANIEL01", "01", "22222222222").ReferenceNumber);
		}

		[TestDate(1986, 3, 12, 4, 0, 1)]
		public void TestLocalCreationTime()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Factory.Save();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 0, 1), hawb.LocalCreationDate);
		}

		public void TestUpdateStatusToCacIfAllowed()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			var hawb3 = mawb.ChildBills.AddNew();
			RunUpdateStatusToCacIfAllowedTest(hawb1, CustomsStatusCodes.Codes.EntryOrRequestAccepted);
			RunUpdateStatusToCacIfAllowedTest(hawb2, CustomsStatusCodes.Codes.EntryOrRequestCancelled);
			RunUpdateStatusToCacIfAllowedTest(hawb3, CustomsStatusCodes.Codes.CustomsQueriedDetained);
		}

		internal static void RunUpdateStatusToCacIfAllowedTest(ICcsukCusAwb awb, string transientStatusCode)
		{
			var finalStatusCode = CustomsStatusCodes.Codes.ClearedByCustoms;
			awb.UpdateStatusToCacIfAllowed(transientStatusCode, ZDateTime.BrettsBirthday, "POOP", "Update one");
			AssertEquals(transientStatusCode, awb.CustomsActionCode);
			AssertEquals("Update one", awb.LatestCustomsActionText);

			awb.UpdateStatusToCacIfAllowed(finalStatusCode, ZDateTime.BrettsBirthday, "POOP", "Update two");
			AssertEquals(finalStatusCode, awb.CustomsActionCode);
			AssertEquals("Update two", awb.LatestCustomsActionText);

			awb.UpdateStatusToCacIfAllowed(transientStatusCode, ZDateTime.BrettsBirthday, "POOP", "Update three");
			AssertEquals(finalStatusCode, awb.CustomsActionCode);
			AssertEquals("Update two", awb.LatestCustomsActionText);
		}

		public void TestIsPrearrival()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			AssertEquals(true, hawb.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 69;
			AssertEquals(false, hawb.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(false, hawb.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Empty;
			AssertEquals(true, hawb.IsPrearrival);
			hawb.CS_PiecesLanded = 6;
			AssertEquals(false, hawb.IsPrearrival);
		}

		public void TestCompleteOnCcsukAndIsCompleteOnCcsuk()
		{
			var mawb = Factory.New<CusMAWB>();
			ICcsukCusAwb hawb1 = mawb.ChildBills.AddNew();
			ICcsukCusAwb hawb2 = mawb.ChildBills.AddNew();
			hawb1.SetCustomsActionCode("DC", ZDateTime.BrettsBirthday);
			Assert(!hawb1.IsCompleteOnCcsuk);
			hawb1.CompleteOnCcsuk();
			Assert(hawb1.IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			AssertHasStatus3Logs(hawb1, "DC");
			hawb2.CompleteOnCcsuk();
			Assert(hawb2.IsCompleteOnCcsuk);
			Assert("Completing last hawb now conmpletes parent too", ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			hawb2.UncompleteOnCcsuk();
			Assert(!hawb2.IsCompleteOnCcsuk);
			Assert(hawb1.IsCompleteOnCcsuk);
			Assert(!((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			SplitBasicTests.RunSetStatus1AndStatus3TestForCompleteness(hawb2);
		}

		internal static void AssertHasStatus3Logs(ICcsukCusAwb bizO, string cac)
		{
			var filterComplete = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			filterComplete.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.TaskCompletedCode);
			var stmLogComplete = bizO.Factory.Load<StmALog>(filterComplete);
			AssertNotNull(stmLogComplete);

			var filterCAC = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			filterCAC.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatusCode);
			filterCAC.AddToFilter(StmALogSchema.SL_Reference, cac);
			var stmLogCAC = bizO.Factory.Load<StmALog>(filterCAC);
			AssertNotNull(stmLogCAC);
		}

		public void TestArchiveOnCcsukAndIsArchivedOnCcsuk()
		{
			var mawb = Factory.New<CusMAWB>();
			ICcsukCusAwb hawb = mawb.ChildBills.AddNew();
			ICcsukCusAwb hawb2 = mawb.ChildBills.AddNew();
			Assert(!hawb.IsArchivedOnCcsuk);
			hawb.ArchiveOnCcsuk(ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days);
			Assert(hawb.IsArchivedOnCcsuk);
			Assert("First child does not archive parent", !((ICcsukCusAwb)mawb).IsArchivedOnCcsuk);
			hawb2.ArchiveOnCcsuk(ReasonForArchiving.PreArrivalOlderThanFourDays);
			Assert(hawb2.IsArchivedOnCcsuk);
			Assert("Last child now also marks parent as archived", ((ICcsukCusAwb)mawb).IsArchivedOnCcsuk);
		}

		public void TestIsEntryCancelled_CDS()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000XXX";

			var cusHawb = cusMawb.ChildBills.AddNew();
			var awb = (ICcsukCusAwb)cusHawb;

			AssertEquals(false, awb.IsEntryCancelled);

			cusHawb.CS_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			AssertEquals(false, awb.IsEntryCancelled);

			var declaration = awb.CreateNewStandaloneCDSDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, awb.IsEntryCancelled);

			cusHawb.CS_CustomsStatus = CustomsStatusCodes.Codes.ClearedByCustoms;
			AssertEquals(false, awb.IsEntryCancelled);

			cusHawb.CS_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			new List<string> { "ACC", "CLR", "RCV", "REJ" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(false, awb.IsEntryCancelled);
			});

			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals(true, awb.IsEntryCancelled);
		}

		public void TestPorts()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			CusMawbTests.PortsTestRunner(hawb, delegate
			{ hawb.CargoTerminalOperatorAirport = ""; });
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestStatus1()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000ABC";
			var awb = (ICcsukCusAwb)mawb.ChildBills.AddNew();
			RunStatus1Test(awb);
		}

		internal static void RunStatus1Test(ICcsukCusAwb awb)
		{
			// This test needs changing - merely setting NPR diretly should not be enough to assign St1.  Shoudl come only form messages (agent) or OutTurns & Save (shed)
			Assert(awb.Status1Date.IsEmpty);
			awb.NumberOfPiecesExpected = 10;
			Assert(awb.Status1Date.IsEmpty);
			awb.NumberOfPiecesReceived = 10;
			Assert("Settig NPR directly is insuffiocient to set status 1", awb.Status1Date.IsEmpty);

			awb.Status1Date = ZDateTime.Empty;
			Assert(awb.Status1Date.IsEmpty);
			awb.Status1Date = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, awb.Status1Date);
			awb.Status1Date = ZDateTime.BrettsBirthday.AddDays(+1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(+1), awb.Status1Date);
			awb.Status1Date = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-1), awb.Status1Date);

			awb.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			AssertEquals("Setting a final CAC when St1 is set completes the job", PresenceOnNetworkList.Codes.CompletedOnCcsUk, awb.PresenceOnNetworkStatus);
			awb.PresenceOnNetworkStatus = "X";
			awb.Status1Date = ZDateTime.BrettsBirthday;
			AssertEquals("Setting St1 when a final CAC exists completes the job", PresenceOnNetworkList.Codes.CompletedOnCcsUk, awb.PresenceOnNetworkStatus);

			awb.NumberOfPiecesExpected = 69;
			awb.NumberOfPiecesReceived = 70;
			AssertEquals((ZShort)69, awb.NumberOfPiecesExpected);
			AssertEquals((ZShort)70, awb.NumberOfPiecesReceived);
			AssertEquals("Setting the values of NPR and NPX to be unequal still wipes ST1", ZDateTime.Empty, awb.Status1Date);
		}

		public void TestPiecesReleased()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			PiecesReleasedTestRunner<CusHAWB>(hawb, delegate (string p)
			{ hawb.Profile = p; });
		}

		internal static void PiecesReleasedTestRunner<T>(ICcsukCusAwb awb, Action<string> setPima) where T : BusinessObject, ICcsukCusAwb
		{
			setPima("CUKFFW98000XXX"); // agent
			awb.NumberOfPiecesReceived = 100;
			AssertEquals(0, awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			awb.ReleaseThisNumberOfPieces(10, Events.Released);
			AssertEquals(10, awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			awb.ReleaseThisNumberOfPieces(50, Events.Released);
			AssertEquals(60, awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			setPima("CUKAIR98LHRYYY"); // shed
			AssertEquals(0, awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			awb.ReleaseThisNumberOfPieces(69, Events.DeliveryOrderReceived);
			AssertEquals(69, awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent));
			setPima("CUKFFW98000XXX"); // agent
			AssertEquals(60, awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
			awb.Factory.Save();
			ICcsukCusAwb newAwb = awb.Factory.Load<T>(awb.PK);
			AssertEquals(60, newAwb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event));
		}

		public void TestCS_JS()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			AssertEquals(ZGuid.Empty, hawb.CS_JE_CustomsFormalEntry);

			var shipmentWithDec = Factory.New<ForwardingShipment>();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_JS = shipmentWithDec.PK;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_JS = shipmentWithDec.PK;
			AssertEquals(dec.PK, hawb2.CS_JE_CustomsFormalEntry);
		}

		public void TestCS_JE_CustomsFormalEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(ZGuid.Empty, hawb.CS_JS);

			var declarationWithShipment = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declarationWithShipment.JE_JS = shipment.PK;
			var hawb2 = Factory.New<CusHAWB>();
			hawb2.CS_JE_CustomsFormalEntry = declarationWithShipment.PK;
			AssertEquals(shipment.PK, hawb2.CS_JS);
		}

		public void TestReadOnlyOnIndividualPropertiesBasedOnSplits_Agent()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKFFW98000LXA";
			AssertPropertiesReadOnly(hawb, false);
			AssertEquals(true, hawb.CS_PiecesLandedInfo.ReadOnly);
			AssertEquals(true, hawb.AgentBadgeInfo.ReadOnly);
			AssertEquals(false, hawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			hawb.Splits.AddNew();
			AssertPropertiesReadOnly(hawb, true);
			AssertEquals(true, hawb.CS_PiecesLandedInfo.ReadOnly);
		}

		public void TestReadOnlyOnIndividualPropertiesBasedOnSplits_Shed()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRBAC";
			AssertPropertiesReadOnly(hawb, false);
			AssertEquals(false, hawb.CS_PiecesLandedInfo.ReadOnly);
			AssertEquals(false, hawb.AgentBadgeInfo.ReadOnly);
			AssertEquals(true, hawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			hawb.Splits.AddNew();
			AssertPropertiesReadOnly(hawb, true);
			AssertEquals(false, hawb.CS_PiecesLandedInfo.ReadOnly);
		}

		public void TestReadOnlyOnIdentifyingFieldsBasedOnPresence()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			CusMawbTests.RunReadOnlyOnIdentifyingFieldsBasedOnPresenceTest(hawb, hawb.CS_HAWBInfo, hawb.CargoTerminalOperatorAirportAndShedInfo, hawb.AgentBadgeInfo, hawb.ProfileInfo);
		}

		void AssertPropertiesReadOnly(CusHAWB hawb, bool shouldBeReadOnly)
		{
			var properties = new List<ZPropertyInfo>()
			{
				hawb.ShipmentDescriptionCodeInfo,
				hawb.CS_PiecesManifestedInfo,
				hawb.CS_WeightUQInfo,
				hawb.CS_WeightInfo,
				hawb.CS_GoodsDescriptionInfo,
				hawb.AirportOfOriginInfo,
				hawb.AirportOfArrivalInfo,
				hawb.AirportOfDestinationInfo,
				hawb.CS_HAWBInfo
			};
			foreach (var zpi in properties)
			{
				AssertEquals("Readonly status of " + zpi.Name, shouldBeReadOnly, zpi.ReadOnly);
			}
		}

		public void TestReadOnly()
		{
			try
			{
				var mawb = Factory.New<CusMAWB>();
				AssertEquals(false, mawb.ProfileInfo.ReadOnly);
				AssertEquals(true, mawb.ChildBills.AllowNew);
				AssertEquals(true, mawb.ChildBills.AllowRemove);
				AssertEquals(false, mawb.ChildBills.ReadOnly);
				var hawb = mawb.ChildBills.AddNew();
				AssertEquals(false, hawb.ReadOnly);
				Env.Security.AirCcsukHouse.IsAllowed = false;
				mawb = Factory.New<CusMAWB>();
				AssertEquals(false, mawb.ProfileInfo.ReadOnly);
				AssertEquals(false, mawb.ChildBills.AllowNew);
				AssertEquals(false, mawb.ChildBills.AllowRemove);
				AssertEquals(true, hawb.ReadOnly);
				AssertEquals(true, mawb.ChildBills.ReadOnly);
			}
			finally
			{
				Env.Security.AirCcsukHouse.IsAllowed = true;
			}
		}

		public void TestCanDelete()
		{
			var mawb = Factory.New<CusMAWB>();
			var awbOkToDelete = mawb.ChildBills.AddNew();
			var awbHasCACLocked = mawb.ChildBills.AddNew();
			var awbHasCACUnLocked = mawb.ChildBills.AddNew();
			var awbIsLodged = mawb.ChildBills.AddNew();
			var awbHasSplits = mawb.ChildBills.AddNew();
			var awbPendingSendWithMessage = mawb.ChildBills.AddNew();
			var awbPendingSendWithNoMessage = mawb.ChildBills.AddNew();
			var awbWithMessage = mawb.ChildBills.AddNew();
			RunCanDeleteTest(awbOkToDelete, awbHasCACLocked, awbHasCACUnLocked, awbIsLodged, awbHasSplits, awbPendingSendWithMessage, awbPendingSendWithNoMessage, awbWithMessage);
		}

		internal static void RunCanDeleteTest<T>(T awbOkToDelete, T awbHasCACLocked, T awbHasCACUnLocked, T awbIsLodged, T awbHasSplits, T awbPendingSendWithMessage, T awbPendingSendWithNoMessage, T awbWithMessageNotOnCommDb)
			where T : BusinessObject, ICcsukCusAwb
		{
			awbHasCACUnLocked.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
			awbOkToDelete.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
			awbHasCACLocked.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterShedRemoval, ZDateTime.BrettsBirthday);
			awbHasCACUnLocked.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.BrettsBirthday);
			awbIsLodged.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			awbHasSplits.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			awbHasSplits.Splits.AddNew();
			awbPendingSendWithMessage.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask;
			var message = awbPendingSendWithMessage.Messages.AddNew();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			awbWithMessageNotOnCommDb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
			message = awbWithMessageNotOnCommDb.Messages.AddNew();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			awbPendingSendWithNoMessage.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask;
			awbOkToDelete.Factory.Save();
			var awbNotInDatabaseBrandNew = awbOkToDelete.Factory.New<T>();

			AssertEquals(true, awbOkToDelete.CanDelete);
			AssertEquals(false, awbHasCACLocked.CanDelete);
			AssertEquals(true, awbHasCACUnLocked.CanDelete);
			AssertEquals(false, awbIsLodged.CanDelete);
			AssertEquals(false, awbHasSplits.CanDelete);
			AssertEquals(false, awbPendingSendWithMessage.CanDelete);
			AssertEquals(true, awbPendingSendWithNoMessage.CanDelete);
			AssertEquals(true, awbWithMessageNotOnCommDb.CanDelete);
			AssertEquals(true, awbNotInDatabaseBrandNew.CanDelete);

			Assert(awbHasCACLocked.ReasonForNotAbleToDelete.ToString().Contains("locked"));
			Assert(awbIsLodged.ReasonForNotAbleToDelete.ToString().Contains("registered on the national network"));
			Assert(awbHasSplits.ReasonForNotAbleToDelete.ToString().Contains("has splits"));
			Assert(awbPendingSendWithMessage.ReasonForNotAbleToDelete.ToString().Contains("certain"));
		}

		public void TestIsLodgedAtCcsuk()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals(false, mawb.IsLodgedAtCcsuk);
			AssertEquals(false, hawb.IsLodgedAtCcsuk);
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			AssertEquals(true, hawb.IsLodgedAtCcsuk);
			AssertEquals("Putting hawb onto network puts mawb on too", true, mawb.IsLodgedAtCcsuk);
		}

		public void TestCS_TranshipmentEntryNum()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			var existingTsrNum = shipment.CusEntryNumbers.AddNew();
			existingTsrNum.CE_EntryType = "TSN";
			existingTsrNum.CE_EntryNum = "Foo";

			hawb.CS_TranshipmentEntryNum = "Bar";
			AssertEquals("Bar", hawb.CS_TranshipmentEntryNum);
			AssertEquals("Shipment's entry number updated when TRN already exists", "Bar", shipment.CustomsEntryNumber);

			var shipment2 = Factory.New<ForwardingShipment>();
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_JS = shipment2.PK;
			hawb2.CS_TranshipmentEntryNum = "Bar";
			AssertEquals("Bar", hawb2.CS_TranshipmentEntryNum);
			AssertEquals("Shipment's entry number set when none exists", "Bar", shipment2.CustomsEntryNumber);
			AssertEquals("TSN", shipment2.CustomsEntryNumberType);
			AssertHasTranshipmentLog(shipment2, "Bar");
			AssertHasTranshipmentLog(hawb2, "Bar");

			var shipment3 = Factory.New<ForwardingShipment>();
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_JS = shipment3.PK;
			var existingTsrNum3 = shipment2.CusEntryNumbers.AddNew();
			existingTsrNum3.CE_EntryType = "XXX";
			existingTsrNum3.CE_EntryNum = "Ram";
			hawb3.CS_TranshipmentEntryNum = "Bar";
			AssertEquals("Bar", hawb3.CS_TranshipmentEntryNum);
			AssertEquals("Shipment's entry number updated when one exists but of wrong flavour", "Bar", shipment3.CustomsEntryNumber);
			AssertEquals("TSN", shipment3.CustomsEntryNumberType);
		}

		void AssertHasTranshipmentLog(BusinessObject bizO, string reference)
		{
			var filter = new ZQuery(StmALogSchema.SL_Parent, bizO.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.IntermediateTranshipmentArrivalCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, reference);
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
		}

		public void TestAllCusUnderbonds()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(0, cusHawb.AllCusUnderbonds.Count);
			var iar = cusHawb.IARs.AddNew();
			cusHawb.AllCusUnderbonds.Load();
			AssertEquals(1, cusHawb.AllCusUnderbonds.Count);
			AssertCollectionContains(iar, cusHawb.AllCusUnderbonds);
			var tsr = cusHawb.TSRs.AddNew();
			cusHawb.AllCusUnderbonds.Load();
			AssertEquals(2, cusHawb.AllCusUnderbonds.Count);
			AssertCollectionContains(tsr, cusHawb.AllCusUnderbonds);
			var isr = cusHawb.ISRs.AddNew();
			cusHawb.AllCusUnderbonds.Load();
			AssertEquals(3, cusHawb.AllCusUnderbonds.Count);
			AssertCollectionContains(isr, cusHawb.AllCusUnderbonds);
			var fbk = cusHawb.FBKs.AddNew();
			cusHawb.AllCusUnderbonds.Load();
			AssertEquals(4, cusHawb.AllCusUnderbonds.Count);
			AssertCollectionContains(fbk, cusHawb.AllCusUnderbonds);
		}
		public void TestCustomsActions()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();

			house.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			house.LatestCustomsActionText = "You suck";
			AssertEquals("CT", house.CustomsActionCode);
			AssertEquals("You suck", house.LatestCustomsActionText);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CT");
			filter.AddToFilter(StmALogSchema.SL_Parent, house.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
			Factory.Save();
			house = new BusinessObjectFactory().Load<CusHAWB>(house.PK);
			AssertEquals("CAC visible upon reload", "CT", house.CustomsActionCode);
			AssertEquals("CAT visible on reload", "You suck", house.LatestCustomsActionText);
			AssertEquals("CAC date recorded", ZDateTime.BrettsBirthday, house.CustomsActionDate);
			house.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday.AddDays(1));
			AssertEquals("CAC date updated when CAC set again to same value (allows CAC to go from blank->CT->CX->CT again)", ZDateTime.BrettsBirthday.AddDays(1), house.CustomsActionDate);
			AssertEquals("CAC visible upon reload", "CT", house.CustomsActionCode);
			house.SetCustomsActionCode("BB", ZDateTime.BrettsBirthday.AddYears(1));
			house.LatestCustomsActionText = "You still suck";
			AssertEquals("BB", house.CustomsActionCode);
			AssertEquals("You still suck", house.LatestCustomsActionText);
			AssertEquals("CAC date now updated because code also changed", ZDateTime.BrettsBirthday.AddYears(1), house.CustomsActionDate);
		}

		public void TestOutturns()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.Profile = "CUKFFW98000LXA";
			var outturn1 = hawb1.OutTurns.AddNew();
			Factory.Save();
			var hawb1Reloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb1.PK);
			AssertEquals(1, hawb1Reloaded.OutTurns.Count);
			AssertEquals(outturn1.PK, hawb1Reloaded.OutTurns[0].PK);

			var hawb2 = mawb.ChildBills.AddNew();
			var outturn2 = hawb2.OutTurns.AddNew();
			Factory.Save();
			var hawb2Reloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb2.PK);
			AssertEquals(1, hawb2Reloaded.OutTurns.Count);
			AssertEquals(outturn2.PK, hawb2Reloaded.OutTurns[0].PK);

			AssertEquals(true, outturn1.C5_PackagesOutturnedInfo.ReadOnly);
			AssertEquals(true, hawb1.OutTurns.ReadOnly);
			AssertEquals(true, outturn1.ReadOnly);
			AssertEquals(true, hawb1.OutTurns.ReadOnly);
			hawb1.Profile = "CUKAIR98LHRBAC";
			AssertEquals(false, outturn1.ReadOnly);
			AssertEquals(false, hawb1.OutTurns.ReadOnly);
			hawb1.Profile = "CUKAIR98XXXXXX";  // invalid
			AssertEquals(true, outturn1.ReadOnly);
			AssertEquals(true, hawb1.OutTurns.ReadOnly);
		}

		public void TestModuleControllerId()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(ControllerIDs.Customs.GB.CcsukAirInventoryHouse, cusHawb.ModuleControllerId);
		}

		public void TestDocManagerInfoAndDocumentSupporter()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertType(typeof(CcsukDocumentSupporter), cusHawb.DocumentSupporter);
			AssertType(typeof(CusHawbDocManagerInfo), ((IDocManagerSupport)cusHawb).DocManagerInfo);
		}

		[TestDate(2015, 08, 22, 14, 00, 00)]
		public void TestNumbersOfPiecesIncStatus1()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			hawb.Profile = "CUKFFW98000LXA";
			var awb = (ICcsukCusAwb)hawb;
			awb.NumberOfPiecesExpected = 69;
			awb.NumberOfPiecesReceived = 70;
			hawb.Factory.Save();
			ICcsukCusAwb hawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals((ZShort)69, awb.NumberOfPiecesExpected);
			AssertEquals((ZShort)69, hawbReloaded.NumberOfPiecesExpected);
			AssertEquals("For agents, NPR is stored", (ZShort)70, awb.NumberOfPiecesReceived);
			AssertEquals("For agents, NPR is stored", (ZShort)70, hawbReloaded.NumberOfPiecesReceived);
			AssertEquals(ZDateTime.Empty, awb.Status1Date);

			hawb.Profile = "CUKAIR98LHRBAC";
			var ot1 = hawb.OutTurns.AddNew();
			ot1.C5_PackagesOutturned = 50;
			var ot2 = hawb.OutTurns.AddNew();
			ot2.C5_PackagesOutturned = 10;
			hawb.Factory.Save();
			AssertEquals((ZShort)69, hawb.CS_PiecesManifested);
			AssertEquals("For sheds, NPR is calculated from the outTurns", (ZShort)60, hawb.CS_PiecesLanded);
			AssertEquals(ZDateTime.Empty, hawb.Status1Date);
			var ot3 = hawb.OutTurns.AddNew();
			ot3.C5_PackagesOutturned = 9;
			hawb.Factory.Save();
			AssertEquals((ZShort)69, hawb.CS_PiecesLanded);
			AssertEquals(new ZDateTime(2015, 08, 22, 14, 00, 00), hawb.Status1Date);

			hawb.Profile = "CUKAIRLHRXXX";  //duff
			var ot4 = hawb.OutTurns.AddNew();
			ot4.C5_PackagesOutturned = 100;
			hawb.Factory.Save();
			AssertEquals("If the PIMA is a legitmate shed invalid then no update occurs", (ZShort)69, hawb.CS_PiecesLanded);
		}

		public void TestIsThroughAwb()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			AssertEquals(false, hawb.IsThroughAwb);
			hawb.AirportOfArrival = "LHR";
			hawb.AirportOfDestination = "MAN";
			AssertEquals(true, hawb.IsThroughAwb);
		}

		public void TestPropertiesReadOnlyFromCAC()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_ArrivalDate = ZDateTime.Now; //  stops it being a prearrival
			mawb.NumberOfPiecesReceived = 69; //  stops it being a prearrival
			var h = mawb.ChildBills.AddNew();
			var columnOneReadOnly = Array.Empty<ZPropertyInfo>();
			var allPossibleProperties = new ZPropertyInfo[] { h.AirportOfOriginInfo, h.AirportOfDestinationInfo, h.ShipmentDescriptionCodeInfo, h.CS_PiecesManifestedInfo, h.CS_WeightInfo, h.CS_WeightUQInfo, h.CS_GoodsDescriptionInfo, h.AgentBadgeInfo, h.CS_HAWBInfo };
			var columnTwoReadOnly = new ZPropertyInfo[] { h.AirportOfOriginInfo, h.AirportOfDestinationInfo, h.ShipmentDescriptionCodeInfo, h.CS_PiecesManifestedInfo, h.CS_WeightInfo, h.CS_WeightUQInfo, h.CS_GoodsDescriptionInfo, h.AgentBadgeInfo, h.CS_HAWBInfo };
			var columnThreeReadOnly = new ZPropertyInfo[] { h.ShipmentDescriptionCodeInfo, h.CS_PiecesManifestedInfo, h.AgentBadgeInfo, };

			h.SetCustomsActionCode("", ZDateTime.BrettsBirthday);
			AssertThesePropertiesReadOnly(columnOneReadOnly, allPossibleProperties, "CAC=blank");
			h.SetCustomsActionCode("CX", ZDateTime.BrettsBirthday);
			AssertThesePropertiesReadOnly(columnOneReadOnly, allPossibleProperties, "CAC=CX");
			h.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			AssertThesePropertiesReadOnly(columnTwoReadOnly, allPossibleProperties, "CAC=CT");
			h.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			AssertThesePropertiesReadOnly(columnTwoReadOnly, allPossibleProperties, "CAC=CC");
			h.SetCustomsActionCode("CU", ZDateTime.BrettsBirthday);
			AssertThesePropertiesReadOnly(columnThreeReadOnly, allPossibleProperties, "CAC=CU");
		}

		internal static void AssertThesePropertiesReadOnly(IEnumerable<ZPropertyInfo> readOnlyProperties, IEnumerable<ZPropertyInfo> allPossibleProperties, string msg)
		{
			foreach (var ro in readOnlyProperties)
			{
				AssertEquals(ro.Name + " should be readonly when " + msg, true, ro.ReadOnly);
			}
			foreach (var p in allPossibleProperties)
			{
				if (!readOnlyProperties.Contains(p))
				{
					AssertEquals(p.Name + " should be writable when " + msg, false, p.ReadOnly);
				}
			}
		}

		public void TestShipmentDescriptionCodeAndConsignmentOrEntryType()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			hawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport;
			AssertEquals(ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport, hawb.ShipmentDescriptionCode);
			hawb.ConsignmentOrEntryType = ConsignmentOrEntryTypes.Codes.Import;
			AssertEquals(ConsignmentOrEntryTypes.Codes.Import, hawb.ConsignmentOrEntryType);
			Factory.Save();
			var hawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals(ConsignmentOrEntryTypes.Codes.Import, hawbReloaded.ConsignmentOrEntryType);
		}

		public void TestSettingAndValidatationOfSDCPortCombination()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			cusMawb.NumberOfPiecesReceived = 60;
			var cushawb = cusMawb.ChildBills.AddNew();
			cushawb.CS_PiecesLanded = 60;
			cushawb.AirportOfOrigin = "USJFK";
			cushawb.AirportOfDestination = "GBBEL";
			AssertEquals("T", cushawb.ShipmentDescriptionCode);
			AssertNoMessageErrors(cushawb.ShipmentDescriptionCodeInfo);
			cushawb.ShipmentDescriptionCode = "C";
			AssertHasErrorContaining(cushawb.ShipmentDescriptionCodeInfo, "Origin outside EU requires SDC T or M");

			cushawb.ShipmentDescriptionCode = "";
			cushawb.AirportOfOrigin = "ATL";
			cushawb.AirportOfDestination = "GBBEL";
			AssertEquals("T", cushawb.ShipmentDescriptionCode);
			AssertNoErrorContaining(cushawb.ShipmentDescriptionCodeInfo, "Origin outside EU requires SDC T or M");

			cushawb.ShipmentDescriptionCode = "";
			cushawb.AirportOfOrigin = "FRPAR";
			cushawb.AirportOfDestination = "BFS";
			AssertEquals("C", cushawb.ShipmentDescriptionCode);
			AssertEquals(false, cushawb.ShipmentDescriptionCodeInfo.HasErrors());
			cushawb.ShipmentDescriptionCode = "E";
			AssertEquals(false, cushawb.ShipmentDescriptionCodeInfo.HasErrors());
			cushawb.ShipmentDescriptionCode = "C";
			AssertEquals(false, cushawb.ShipmentDescriptionCodeInfo.HasErrors());
			cushawb.ShipmentDescriptionCode = "T";
			AssertEquals(false, cushawb.ShipmentDescriptionCodeInfo.HasErrors());

			cushawb.ShipmentDescriptionCode = "";
			cushawb.AirportOfOrigin = "ATL";
			cushawb.AirportOfDestination = "CDG";
			AssertEquals("T", cushawb.ShipmentDescriptionCode);

			cushawb.ShipmentDescriptionCode = "";
			cushawb.AirportOfDestination = "CDG";
			cushawb.AirportOfOrigin = "ATL";
			AssertEquals("T", cushawb.ShipmentDescriptionCode);
			cushawb.AirportOfOrigin = "FRA";
			AssertEquals("C", cushawb.ShipmentDescriptionCode);
		}

		public void TestProfile()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			hawb.Profile = "CUKFFW98000DAN";
			AssertEquals("CUKFFW98000DAN", hawb.Profile);
			Factory.Save();
			var cusHawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(hawb.PK);
			AssertEquals("CUKFFW98000DAN", cusHawbReloaded.Profile);
		}

		public void TestProfileReadOnlyAndPropagation()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var cusMawb = Factory.New<CusMAWB>();
			var hawb = cusMawb.ChildBills.AddNew();
			hawb.Profile = "CUKFFW98000LXA";
			AssertEquals("CUKFFW98000LXA", hawb.Profile);
			AssertEquals(true, hawb.IsProfileAnAgent);
			AssertEquals(false, hawb.IsProfileAShed);
			AssertEquals("When selecting an agent profile, Shed field is not readonly", false, hawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			AssertEquals("When selecting an agent profile, Agent field is readonly", true, hawb.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting an agent profile, Agent field is set from PIMA", "LXA", hawb.AgentBadge);
			AssertEquals("When selecting an agent profile, NPR field is readonly", true, hawb.CS_PiecesLandedInfo.ReadOnly);
			hawb.Profile = "CUKAIR98LHRBAC";
			AssertEquals(false, hawb.IsProfileAnAgent);
			AssertEquals(true, hawb.IsProfileAShed);
			AssertEquals("When selecting a shed profile, Agent field is not readonly", false, hawb.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting a shed profile, Shed field is readonly", true, hawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			AssertEquals("When selecting a shed profile, Shed field is updated", "BAC", hawb.CargoTerminalOperator);
			AssertEquals("When selecting a shed profile, Airport field is updated", "LHR", hawb.CargoTerminalOperatorAirport);

			var cusMawbNewFactoryToAvoidCache = new BusinessObjectFactory().New<CusMAWB>();
			var hawbNewFactoryToAvoidCache = cusMawbNewFactoryToAvoidCache.ChildBills.AddNew();
			hawbNewFactoryToAvoidCache.Profile = "CUKAIR98LHRLXA";
			AssertEquals("When selecting a fallback profile, Agent field is not readonly", false, hawbNewFactoryToAvoidCache.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting a fallback profile, Shed field is not readonly", false, hawbNewFactoryToAvoidCache.CargoTerminalOperatorInfo.ReadOnly);
			AssertEquals("When selecting a fallback profile, NPR field is not readonly", false, hawbNewFactoryToAvoidCache.CS_PiecesLandedInfo.ReadOnly);
			hawb.Profile = "CUKAIR98LHRBAC";
			AssertEquals("When selecting a FULL shed profile, NPR field is not readonly", false, hawbNewFactoryToAvoidCache.CS_PiecesLandedInfo.ReadOnly);

			var hawb2 = cusMawb.ChildBills.AddNew();
			hawb2.Profile = "CUKAIR98LHRAAA";
			AssertEquals("Shed updated from BAC (fallback test)", "LHRAAA", hawb2.CS_WarehouseLocation);
			hawb2.Profile = "CUKFFW98000DAN";
			AssertEquals("DAN", hawb2.AgentBadge);
			hawb2.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			hawb2.Profile = "CUKFFW98000XXX";
			AssertEquals("Agent not updated when status3", "DAN", hawb2.AgentBadge);
			hawb2.Profile = "CUKAIR98LHRYYY";
			AssertEquals("Shed not updated when status3", "LHRAAA", hawb2.CS_WarehouseLocation);
		}

		public void TestPreferredAgentSetForSheds()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			AssertEquals("The badge should be blank when choosing a shed pima without a preferred agent. If this is not blank then it could be that when CusHAWB.SetDefaultValues() looks at credentials to see if there is exactly one it finds exactly one (an agent PIMA) because shed not enabled. Check licencing. Break on CusHAWBLookups.GetProfilesList().", "", basic.AgentBadge);

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, true);
			var basic2 = Factory.New<CusMAWB>();
			basic2.Profile = "CUKAIR98LHRBAC";
			AssertEquals("DJC", basic2.AgentBadge);

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false, true);
			var basic3 = Factory.New<CusMAWB>();
			basic3.AgentBadge = "ABC";
			basic3.Profile = "CUKAIR98LHRBAC";
			AssertEquals("Agent not clobbered if already set", "ABC", basic3.AgentBadge);
		}

		public void TestSimpleAliasedProperties()
		{
			var mawb = Factory.New<CusMAWB>();
			var cusHawb = mawb.ChildBills.AddNew();
			cusHawb.CS_GoodsDescription = "Poop:Dan$X1";
			AssertEquals("UNOA chars are kept", "POOP:DANX1", cusHawb.DescriptionOfGoods);
		}

		public void TestRemovalAndFallbackCollections()
		{
			var mawb = Factory.New<CusMAWB>();
			var cusHawb = mawb.ChildBills.AddNew();
			var iar = cusHawb.IARs.AddNew();
			AssertEquals(iar, cusHawb.IARs[0]);
			var isr = cusHawb.ISRs.AddNew();
			AssertEquals(isr, cusHawb.ISRs[0]);
			var tsr = cusHawb.TSRs.AddNew();
			AssertEquals(tsr, cusHawb.TSRs[0]);
			var fbk = cusHawb.FBKs.AddNew();
			AssertEquals(fbk, cusHawb.FBKs[0]);
			AssertEquals(fbk, (cusHawb as ICcsukCusAwb).FBKs[0]);
		}

		public void TestHasEntryWithLodgedOrPrelodgedWithCustoms_Chief()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			cusHawb.CS_JE_CustomsFormalEntry = Guid.NewGuid();
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			entry.EntryNumber = "123";
			AssertEquals(true, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
		}

		public void TestHasEntryWithLodgedOrPrelodgedWithCustoms_CDS()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			cusHawb.CS_JE_CustomsFormalEntry = Guid.NewGuid();
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.EntryNumber = "123";
			entry.CH_EntryStatus = ZString.Empty;
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.CH_EntryStatus = "REJ";
			AssertEquals(false, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			new List<string> { "ACC", "RCV", "CLR", "CAN" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(true, cusHawb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			});
		}

		public void TestHasDeclaration()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(false, cusHawb.HasDeclaration);
			cusHawb.CS_JE_CustomsFormalEntry = Guid.NewGuid();
			AssertEquals(false, cusHawb.HasDeclaration);
			var declaration = Factory.New<JobDeclaration>();
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(true, cusHawb.HasDeclaration);
		}

		public void TestReasonForNotAllowSplit_Chief()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(ZString.Empty, cusHawb.ReasonForNotAllowSplit);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.CustomsEntryHeaders.AddNew().EntryNumber = "123";
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(ZString.Empty, cusHawb.ReasonForNotAllowSplit);
		}

		public void TestReasonForNotAllowSplit_CDS()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertEquals(ZString.Empty, cusHawb.ReasonForNotAllowSplit);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123";
			cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
			AssertEquals(ZString.Empty, cusHawb.ReasonForNotAllowSplit);
			entry.CH_EntryStatus = "REJ";
			AssertEquals(ZString.Empty, cusHawb.ReasonForNotAllowSplit);

			new List<string> { "ACC", "RCV", "CLR", "CAN" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(ZString.Empty, cusHawb.ReasonForNotAllowSplit);
			});
		}

		public void TestTypes()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			AssertType(typeof(CusMAWB), cusHawb.MAWB);
			AssertType(typeof(CusHAWB), cusHawb);
			AssertType(typeof(CusHAWBValidation), cusHawb.Validation);
		}

		public void TestCustomsActionText()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			var sirMixalot = "I LIKE SMALL BUTTS AND I CANNOT LIE";
			cusHawb.LatestCustomsActionText = sirMixalot;
			AssertEquals(sirMixalot, cusHawb.LatestCustomsActionText);
			Factory.Save();
			var cusHawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(cusHawb.PK);
			AssertEquals(sirMixalot, cusHawbReloaded.LatestCustomsActionText);
		}

		public void TestCustomsActionCode()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.LatestCustomsActionText = "CC";
			AssertEquals("CC", cusHawb.LatestCustomsActionText);
			Factory.Save();
			var cusHawbReloaded = new BusinessObjectFactory().Load<CusHAWB>(cusHawb.PK);
			AssertEquals("CC", cusHawbReloaded.LatestCustomsActionText);
			var filter = new ZQuery(StmALogSchema.SL_Reference, "CC");
			filter.AddToFilter(StmALogSchema.SL_Parent, cusHawb.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			var stmLog = Factory.Load<StmALog>(filter);
			AssertNotNull(stmLog);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusHawb = factory.New<CusHAWB>();
			var cusMawb = factory.New<CusMAWB>();
			cusHawb.CS_CM = cusMawb.PK;
			return cusHawb;
		}

		public void TestBoringLookupsCode()
		{
			AssertEquals("Weights lookup should contain Kilos", true, Factory.New<CusHAWB>().Lookups.UnitOfWeightList.ContainsCode("kg"));
		}

		public void TestDeclaration()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertNull(hawb.Declaration);
			var dec = Factory.New<JobDeclaration>();
			hawb.CS_JE_CustomsFormalEntry = dec.PK;
			dec.JE_GoodsDescription = "DANIEL";
			AssertNotNull(hawb.Declaration);
			AssertEquals("DANIEL", hawb.Declaration.JE_GoodsDescription);
			AssertType(typeof(JobDeclaration), hawb.Declaration);
		}
		public void TestUserInChargeOfJob()
		{
			var userSystemsAccount = Factory.New<GlbStaff>();
			userSystemsAccount.GS_Code = "AAA";
			userSystemsAccount.GS_LoginName = "AAA";
			userSystemsAccount.GS_IsSystemAccount = true;
			Factory.Save();

			var userNotSystemsAccount = Factory.New<GlbStaff>();
			userNotSystemsAccount.GS_Code = "DJC";
			userNotSystemsAccount.GS_LoginName = "DJC";
			userNotSystemsAccount.GS_IsSystemAccount = false;
			Factory.Save();

			var hawb = Factory.New<CusHAWB>();
			AssertNull(hawb.UserInChargeOfJob);

			var dec = Factory.New<JobDeclaration>();
			hawb.CS_JE_CustomsFormalEntry = dec.PK;
			dec.JE_GS_NKCusAgent = "DJC";
			hawb.CS_JE_CustomsFormalEntry = dec.PK;
			AssertEquals(userNotSystemsAccount.PK, hawb.UserInChargeOfJob.PK);

			hawb = Factory.New<CusHAWB>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_SystemCreateUser = "DJC";
			hawb.CS_JS = shipment.PK;
			AssertEquals(null, hawb.UserInChargeOfJob);

			hawb = Factory.New<CusHAWB>();
			var message = hawb.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_SystemCreateUser = "DJC";
			AssertEquals(userNotSystemsAccount.PK, hawb.UserInChargeOfJob.PK);
			Thread.Sleep(100);

			var message2 = hawb.Messages.AddNew();
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_SystemCreateUser = "AAA";
			AssertEquals(userNotSystemsAccount.PK, hawb.UserInChargeOfJob.PK);
		}

		public void TestReferenceNumbersAndHumanReadableName()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertEquals("", hawb.ReferenceNumber);
			hawb.CS_HAWB = "12345678";
			AssertEquals("12345678", hawb.ReferenceNumber);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "12387654321";
			mawb.ChildBills.Add(hawb);
			AssertEquals("123-87654321-12345678", hawb.ReferenceNumber);
			AssertEquals("CCS-UK House Bill 123-87654321-12345678", hawb.HumanReadableName);
			AssertEquals("1238765432112345678", ((ICcsukCusAwb)hawb).ChiefMasterUCRReferenceSuffix);
			AssertEquals("123-87654321", mawb.MasterLevelHouseHelper.ReferenceNumber);
			AssertEquals("12387654321", ((ICcsukCusAwb)mawb).ChiefMasterUCRReferenceSuffix);
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CargoTerminalOperator = "CAX";
			AssertEquals("LHRCAX-123-87654321-12345678", hawb.ReferenceNumberWithShed);
		}

		public void TestLoad()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertNull("No result, hawb not linked to shipment", new CusHAWB.Loader(Factory).LoadHawbFromShipmentDbOnly(shipment));
			hawb.CS_JS = shipment.PK;
			AssertNull("Find no hawb in DB", new CusHAWB.Loader(Factory).LoadHawbFromShipmentDbOnly(shipment));
			Factory.Save();
			AssertEquals("Find hawb in DB after saving", hawb.PK, new CusHAWB.Loader(Factory).LoadHawbFromShipmentDbOnly(shipment).PK);
		}

		public void TestShipmentAndIsLinkedToJobShipmentAndIsLinkedToForwardingJob()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals(false, mawb.MasterLevelHouseHelper.IsLinkedToForwardingJob);
			AssertNull(hawb.Shipment);
			AssertEquals(false, hawb.IsLinkedToJobShipment);
			AssertEquals(false, hawb.IsLinkedToForwardingJob);
			hawb.CS_JS = shipment.PK;
			AssertEquals(shipment.PK, hawb.Shipment.PK);
			AssertEquals(true, hawb.IsLinkedToJobShipment);
			AssertEquals(true, hawb.IsLinkedToForwardingJob);
			AssertEquals(false, mawb.MasterLevelHouseHelper.IsLinkedToForwardingJob);
			var consol = Factory.New<ForwardingConsol>();
			mawb.CM_JK = mawb.PK;
			AssertEquals(true, mawb.MasterLevelHouseHelper.IsLinkedToForwardingJob);
		}

		public void TestCreateNewOnMawbLinkedToShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = new CusHAWB.Loader(Factory).CreateNewOnMawbLinkedToShipment(mawb, shipment);
			AssertEquals(mawb.PK, hawb.CS_CM);
			AssertEquals(shipment.PK, hawb.CS_JS);
		}

		public void TestDefaultValues()
		{
			var hawb = Factory.New<CusHAWB>();
			AssertEquals(PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck, hawb.PresenceOnNetworkStatus);
			AssertEquals("CUK", hawb.CS_ApplicationCode);
		}

		public void TestPresenceOnNetworkStatus()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.PresenceOnNetworkStatus = "X";
			AssertEquals("X", hawb.PresenceOnNetworkStatus);
			Factory.Save();
			var hawbReloaded = Factory.Load<CusHAWB>(hawb.PK);
			AssertEquals("X", hawbReloaded.PresenceOnNetworkStatus);
			hawbReloaded.SetPresenceOnNetworkStatusAfterMessageUpload();
			AssertEquals("X is unchanged", "X", hawbReloaded.PresenceOnNetworkStatus);
			hawbReloaded.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
			hawb.SetPresenceOnNetworkStatusAfterMessageUpload();
			AssertEquals("UNK becomes ASS", PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection, hawbReloaded.PresenceOnNetworkStatus);
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask;
			hawbReloaded.SetPresenceOnNetworkStatusAfterMessageUpload();
			AssertEquals("PND becomes ASS", PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection, hawbReloaded.PresenceOnNetworkStatus);
			hawbReloaded.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			hawbReloaded.SetPresenceOnNetworkStatusAfterMessageUpload();
			AssertEquals("A firm status of YES is not clobbered", PresenceOnNetworkList.Codes.OnCommDb, hawbReloaded.PresenceOnNetworkStatus);
		}

		public void TestIsLodgedOrAssumedAtCcsuk()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.CS_HAWB = "12345678";
			cusHawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			Factory.Save();
			cusHawb.CS_HAWB = "23456789";

			Assert("CusHawb_Assumed CusMawb_Assumed", cusHawb.IsLodgedOrAssumedAtCcsuk);

			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Assert("CusHawb_Assumed CusMawb_Lodged", cusHawb.IsLodgedOrAssumedAtCcsuk);

			cusHawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Assert("CusHawb_Lodged CusMawb_Lodged", cusHawb.IsLodgedOrAssumedAtCcsuk);

			cusHawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			Assert("CusHawb_Lodged CusMawb_Assumed", cusHawb.IsLodgedOrAssumedAtCcsuk);
		}

		public void TestTemporaryStorageEndDate()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.TemporaryStorageEndDate = ZDateTime.Empty;
			Assert(hawb.TemporaryStorageEndDate.IsEmpty);
			hawb.TemporaryStorageEndDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, hawb.TemporaryStorageEndDate);
		}
	}

	public class CusMAWBForTest : CusMAWB
	{
		public CusMAWBForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		public new CusOutTurnCollection OutTurns
		{
			get
			{
				if (IsBasic && outTurns == null)
				{
					outTurns = new CusOutTurnCollection(MasterLevelHouseHelper);
					RegisterEditableChildObject(outTurns);
					outTurns.Load();
				}
				return outTurns;
			}

			set { outTurns = value; }
		}
	}

	[TestedType(typeof(CusHAWBDependentCollection))]
	class CusHAWBDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusHAWBDependentCollection(Mawb);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusHAWB>();
		}

		public void TestHawbMawbWorker()
		{
			var col = GetCollectionToTest();
			col.Load();
			AssertCollectionNotContains("House collection should not see the worker house", Mawb.MasterLevelHouseHelper, col);
			var hawb = Mawb.ChildBills.AddNew();
			col.Load();
			AssertCollectionContains("House collection should see the regular house", hawb, col);
		}

		public void TestAllowNew()
		{
			AssertEquals(true, Mawb.ChildBills.AllowNew);
			var consol = Factory.New<ForwardingConsol>();
			Mawb.CM_JK = consol.PK;
			AssertEquals("Pre-req: consol is not direct", false, consol.IsDirect);
			AssertEquals("Allow new child by default", true, Mawb.ChildBills.AllowNew);
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			AssertEquals("Prevent allow new for direct consol", false, Mawb.ChildBills.AllowNew);

			consol.Delete();
			Mawb.CM_JK = ZGuid.Empty;
			Mawb.SetCustomsActionCode("CX", ZDateTime.BrettsBirthday);
			AssertEquals("Allow new child when CAC=CX", true, Mawb.ChildBills.AllowNew);
			Mawb.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			AssertEquals("No new child when CAC=CT", false, Mawb.ChildBills.AllowNew);
			Mawb.SetCustomsActionCode("CU", ZDateTime.BrettsBirthday);
			AssertEquals("No new child when CAC=CU", false, Mawb.ChildBills.AllowNew);
			Mawb.SetCustomsActionCode("", ZDateTime.BrettsBirthday);
			Mawb.Splits.AddNew();
			AssertEquals("No new child when splits exist", false, Mawb.ChildBills.AllowNew);

			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000XXX";
			Assert(basic.ChildBills.AllowNew);
			var dec = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			AssertNotNull("Pre-req - a dec was created", dec);
			Assert("Cannot add a house to the basic to make it a consol when the basic has a declaration", !basic.ChildBills.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(true, Mawb.ChildBills.AllowNew);
			AssertEquals("Allow remove child by default", true, Mawb.ChildBills.AllowRemove);
			Mawb.SetCustomsActionCode("CX", ZDateTime.BrettsBirthday);
			AssertEquals("Allow remove child when CAC=CX", true, Mawb.ChildBills.AllowRemove);
			Mawb.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			AssertEquals("No remove child when CAC=CT", false, Mawb.ChildBills.AllowRemove);
			Mawb.SetCustomsActionCode("CU", ZDateTime.BrettsBirthday);
			AssertEquals("No remove child when CAC=CU", false, Mawb.ChildBills.AllowRemove);
			Mawb.SetCustomsActionCode("", ZDateTime.BrettsBirthday);
			Mawb.Splits.AddNew();
			AssertEquals("No remove child when splits exist", false, Mawb.ChildBills.AllowRemove);
		}

		public void TestSetDefaultsForNewChild()
		{
			Mawb.CargoTerminalOperator = "BAC";
			Mawb.CargoTerminalOperatorAirport = "LGR";
			Mawb.AgentBadge = "DAN";
			Mawb.ShipmentDescriptionCode = "T";
			Mawb.WeightCode = "X";
			Mawb.Profile = "Y";
			Mawb.AirportOfArrival = "AOA";
			Mawb.AirportOfDestination = "AOD";
			Mawb.AirportOfOrigin = "AOO";
			var hawb = Mawb.ChildBills.AddNew();
			AssertEquals("BAC", hawb.CargoTerminalOperator);
			AssertEquals("LGR", hawb.CargoTerminalOperatorAirport);
			AssertEquals("DAN", hawb.AgentBadge);
			AssertEquals("T", hawb.ShipmentDescriptionCode);
			AssertEquals("X", hawb.CS_WeightUQ);
			AssertEquals("Y", hawb.Profile);
			AssertEquals("AOA", hawb.AirportOfArrival);
			AssertEquals("AOD", hawb.AirportOfDestination);
			AssertEquals("", hawb.AirportOfOrigin);
		}

		CusMAWB Mawb
		{
			get
			{
				if (mawb == null)
				{
					mawb = Factory.New<CusMAWB>();
				}
				return mawb;
			}
		}

		CusMAWB mawb;
	}

	[TestedType(typeof(CusHAWBCollectionNonDependentShowMastersToo))]
	class CusHAWBCollectionNonDependentShowMastersTooTest : BusinessObjectCollectionTestCase
	{
		public void TestWorkersShown()
		{
			var trueHouse = Factory.New<CusHAWB>();
			var workerHouse = Factory.New<CusHAWB>();
			workerHouse.CS_IsMasterHouse = true;
			var coll = GetCollectionToTest();
			coll.Load();
			AssertEquals(2, coll.Count);
			AssertEquals(true, coll.Contains(trueHouse));
			AssertEquals(true, coll.Contains(workerHouse));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusHAWBCollectionNonDependentShowMastersToo(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusHAWB>();
		}
	}

	[TestedType(typeof(CusHAWBCollectionNonDependent))]
	class CusHAWBCollectionNonDependentTest : BusinessObjectCollectionTestCase
	{
		public void TestWorkersHidden()
		{
			var trueHouse = Factory.New<CusHAWB>();
			var workerHouse = Factory.New<CusHAWB>();
			workerHouse.CS_IsMasterHouse = true;
			var coll = GetCollectionToTest();
			coll.Load();
			AssertEquals(1, coll.Count);
			AssertEquals(true, coll.Contains(trueHouse));
			AssertEquals(false, coll.Contains(workerHouse));
		}
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusHAWBCollectionNonDependent(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusHAWB>();
		}
	}

	class CusHawbLoaderTests : TestCaseWithFactory
	{
		public void TestLoadHawbFromShipmentDbOnly()
		{
			var writingFactory = new BusinessObjectFactory();
			var shipment = writingFactory.New<ForwardingShipment>();
			var aussieMawb = (Customs.Business.CusMAWB)writingFactory.New<Integration.Customs.AU.ICusMAWB>();
			var aussieHawb = (Customs.Business.CusHAWB)writingFactory.New<Integration.Customs.AU.ICusHAWB>();
			aussieHawb.CS_CM = aussieMawb.PK;
			aussieHawb.CS_JS = shipment.PK;
			writingFactory.Save();

			var loader = new CusHAWB.Loader(Factory);
			var gbHawb = loader.LoadHawbFromShipmentDbOnly(shipment);
			AssertNull("No hawb found", gbHawb);

			shipment = writingFactory.New<ForwardingShipment>();
			var gbMawb = writingFactory.New<CusMAWB>();
			var gbHawbWrite = writingFactory.New<CusHAWB>();
			gbHawbWrite.CS_CM = gbMawb.PK;
			gbHawbWrite.CS_JS = shipment.PK;
			writingFactory.Save();

			loader = new CusHAWB.Loader(Factory);
			gbHawb = loader.LoadHawbFromShipmentDbOnly(shipment);
			AssertEquals("GB hawb found", gbHawbWrite, gbHawb);
		}

		public void TestCreateNewOnMawbLinkedToShipmentAndSynchronise()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GoodsValue = 69m;
			shipment.JS_GoodsDescription = "STUFF";
			shipment.JS_ActualWeight = 169m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_OuterPacks = 100;
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			shipment.JS_HouseBill = "12345678";

			var mawb = Factory.New<CusMAWB>();
			var loader = new CusHAWB.Loader(Factory);
			var hawb = loader.CreateNewOnMawbLinkedToShipment(mawb, shipment);
			AssertEquals(shipment.PK, hawb.CS_JS);
			AssertEquals("12345678", hawb.CS_HAWB);
			AssertEquals(69m, hawb.CS_GoodsValue);
			AssertEquals("STUFF", hawb.CS_GoodsDescription);
			AssertEquals(169m, hawb.CS_Weight);
			AssertEquals("KG", hawb.CS_WeightUQ);
			AssertEquals(100, (int)hawb.CS_PiecesManifested);
			AssertEquals("USD", hawb.CS_RX_NKGoodsCurrency);
		}

		public void TestLoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var databaseHawb = mawb.ChildBills.AddNew();
			databaseHawb.CS_JS = shipment.PK;
			Factory.Save();
			var loader = new CusHAWB.Loader(Factory);
			var result = loader.LoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill(shipment, null);
			AssertEquals("Matches on CS_JS", databaseHawb.PK, result.PK);

			shipment.JS_HouseBill = "12345678XX";
			databaseHawb.CS_HAWB = "12345678";
			databaseHawb.CS_JS = Guid.Empty;
			Factory.Save();
			result = loader.LoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill(shipment, null);
			AssertNull("No match because the HAWB numbers are not exactly the same", result);

			shipment.JS_HouseBill = "1234567X";
			databaseHawb.CS_HAWB = "1234567X";
			Factory.Save();
			result = loader.LoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill(shipment, null);
			AssertEquals("Matches on CS_HAWB", databaseHawb.PK, result.PK);

			var unrelatedMawb = Factory.New<CusMAWB>();
			var unrelatedHawbWithSameNumber = unrelatedMawb.ChildBills.AddNew();
			unrelatedHawbWithSameNumber.CS_HAWB = "12345678";
			var consolAsParentOfMawb = shipment.Consols.AddNew();
			mawb.CM_JK = consolAsParentOfMawb.PK;
			Factory.Save();
			result = loader.LoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill(shipment, consolAsParentOfMawb);
			AssertEquals("Matches on CM_JK", databaseHawb.PK, result.PK);

			mawb.CM_JK = Guid.Empty;
			mawb.CM_MAWB = "12512345678";
			consolAsParentOfMawb.JK_MasterBillNum = "125-12345678";
			result = loader.LoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill(shipment, consolAsParentOfMawb);
			AssertEquals("Matches on CM_MAWB", databaseHawb.PK, result.PK);
		}

		public void TestFindAllHawbsWithoutShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawbGood = mawb.ChildBills.AddNew();
			var hawbBad = mawb.ChildBills.AddNew();
			hawbBad.CS_HAWB = "0000000X";
			hawbGood.CS_HAWB = "0000000X";
			hawbBad.CS_JS = shipment.PK;
			Factory.Save();
			var hawbsFound = new CusHAWB.Loader(Factory).FindAllHawbsWithoutShipment("0000000X");
			AssertEquals(hawbGood.PK, hawbsFound[0].PK);
		}

		public void TestFindHawbOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawbGood = mawb.ChildBills.AddNew();
			var hawbBad = mawb.ChildBills.AddNew();
			hawbGood.CS_JS = shipment.PK;
			Factory.Save();
			var hawbFound = new CusHAWB.Loader(Factory).FindHawbOnShipment(shipment);
			AssertEquals(hawbGood.PK, hawbFound.PK);
		}

		public void TestFindExistingHawbOnMawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawbGood = mawb.ChildBills.AddNew();
			var hawbBad = mawb.ChildBills.AddNew();
			hawbBad.CS_HAWB = "0000000Y";
			hawbGood.CS_HAWB = "0000000X";
			var hawbFound = new CusHAWB.Loader(Factory).FindExistingHawbOnMawb(mawb, "0000000X");
			AssertEquals(hawbGood.PK, hawbFound.PK);

			var mawb2 = Factory.New<CusMAWB>();
			var hawbUC1 = mawb2.ChildBills.AddNew();
			hawbUC1.CS_HAWB = "HB001LON";
			hawbFound = new CusHAWB.Loader(Factory).FindExistingHawbOnMawb(mawb2, "hb001lon");
			AssertEquals("Case should be ignored in HAWB search", hawbUC1.PK, hawbFound.PK);

			var hawbLC2 = mawb2.ChildBills.AddNew();
			hawbLC2.CS_HAWB = "hb001man";
			AssertEquals("Pre-condition: Hawb value should be stored in Upper Case in db", "HB001MAN", hawbLC2.CS_HAWB);
			hawbFound = new CusHAWB.Loader(Factory).FindExistingHawbOnMawb(mawb2, "HB001MAN");
			AssertEquals("Case should be ignored in HAWB search", hawbLC2.PK, hawbFound.PK);
		}

		public void TestFindHawb_HawbNoSplitMawbNo()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "12345678";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "87654321";
			var split = hawb1.Splits.AddNew();
			split.SplitReference = "69";
			Factory.Save();
			var loader = new CusHAWB.Loader(Factory);
			AssertEquals(hawb1.PK, loader.FindHawb("12345678", "69", "").PK);
			AssertEquals(hawb1.PK, loader.FindHawb("12345678", "", "").PK);
			AssertEquals(hawb2.PK, loader.FindHawb("87654321", "", "").PK);
			AssertNull(loader.FindHawb("87654321", "69", ""));
		}

		public void TestIsPreArrival()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.NumberOfPiecesReceived = 1;
			AssertEquals(false, hawb.IsPrearrival);
			mawb.NumberOfPiecesReceived = 0;
			AssertEquals(true, hawb.IsPrearrival);
			mawb.CM_ArrivalDate = ZDate.BrettsBirthday;
			AssertEquals(false, hawb.IsPrearrival);
		}
	}
}
