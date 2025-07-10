using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusMAWB))]
	class CusMawbTests : EnterpriseBusinessObjectTestCase
	{
		public void TestCheckInAllChildPieces_NoSplits()
		{
			var mawb1 = Factory.New<CusMAWB>();
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "HOUSE1";
			hawb1.CS_PiecesManifested = 3;
			var outturn1 = hawb1.OutTurns.AddNew();
			outturn1.C5_PackagesOutturned = 0;
			var outturn2 = hawb1.OutTurns.AddNew();
			outturn2.C5_PackagesOutturned = 0;
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "HOUSE2";
			hawb2.CS_PiecesManifested = 4;
			hawb2.Status1Date = ZDateTime.BrettsBirthday;
			var outturn3 = hawb2.OutTurns.AddNew();
			outturn3.C5_PackagesOutturned = 3;
			var outturn4 = hawb2.OutTurns.AddNew();
			outturn4.C5_PackagesOutturned = 0;

			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
			mawb1.CargoTerminalOperator = "CAX";

			var nonPersistentCheckInAllChildPieces = new NonPersistentCheckInAllChildPieces(mawb1);

			nonPersistentCheckInAllChildPieces.PackagesUnits = "PK";
			nonPersistentCheckInAllChildPieces.ReceivedDate = ZDateTime.BrettsBirthday;
			nonPersistentCheckInAllChildPieces.MarksAndNumbers = "Marks&Numbers";
			nonPersistentCheckInAllChildPieces.ShedStorageLocationId = locationCax.PK;
			nonPersistentCheckInAllChildPieces.IsBeingReleasedNow = true;
			nonPersistentCheckInAllChildPieces.GoodsDescription = "Good";
			nonPersistentCheckInAllChildPieces.ContainerNumber = "ContainerNumber";
			nonPersistentCheckInAllChildPieces.ContainerSeal = "ContainerSeal";
			nonPersistentCheckInAllChildPieces.IsDamaged = true;

			var result = mawb1.CheckInAllChildPieces(nonPersistentCheckInAllChildPieces);

			AssertEquals("Make single new outturn for Hawb", 1, hawb1.OutTurns.Count);
			var newOutturn = hawb1.OutTurns[0];
			AssertEquals("PK", newOutturn.C5_PackagesUnits);
			AssertEquals(ZDateTime.BrettsBirthday, newOutturn.C5_CargoReceiptDate);
			AssertEquals("Marks&Numbers", newOutturn.C5_MarksAndNumbers);
			AssertEquals(locationCax.PK, newOutturn.WarehouseLocationID);
			AssertEquals(true, newOutturn.IsBeingReleasedNow);
			AssertEquals("Good", newOutturn.C5_GoodsDescription);
			AssertEquals("ContainerNumber", newOutturn.C5_ContainerNumber);
			AssertEquals("ContainerSeal", newOutturn.C5_ContainerSeal);
			AssertEquals(true, newOutturn.C5_DamageIndicator);
			AssertEquals(3, newOutturn.C5_PackagesOutturned);

			AssertContains($"HAWB {hawb2.CS_HAWB}: status 1 already set; skipping", result);
			AssertEquals("Skipped - no changes", 2, hawb2.OutTurns.Count);
		}

		public void TestCheckInAllChildPieces_WithSplits()
		{
			var mawb1 = Factory.New<CusMAWB>();
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "HOUSE1";
			var split1 = hawb1.Splits.AddNew();
			split1.NumberOfPiecesReceived = 0;
			split1.NumberOfPiecesExpected = 1;
			split1.SplitReference = "01";
			var split2 = hawb1.Splits.AddNew();
			split2.NumberOfPiecesReceived = 0;
			split2.NumberOfPiecesExpected = 2;
			split2.SplitReference = "02";
			var outturn1 = hawb1.OutTurns.AddNew();
			outturn1.C5_PackagesOutturned = 1;
			var outturn2 = hawb1.OutTurns.AddNew();
			outturn2.C5_PackagesOutturned = 2;
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "HOUSE2";
			var split3 = hawb2.Splits.AddNew();
			split3.NumberOfPiecesReceived = 0;
			split3.NumberOfPiecesExpected = 1;
			split3.SplitReference = "01";
			var split4 = hawb2.Splits.AddNew();
			split4.NumberOfPiecesReceived = 1;
			split4.NumberOfPiecesExpected = 2;
			split4.SplitReference = "02";
			var outturn3 = hawb2.OutTurns.AddNew();
			var outturn4 = hawb2.OutTurns.AddNew();

			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);
			mawb1.CargoTerminalOperator = "CAX";

			var nonPersistentCheckInAllChildPieces = new NonPersistentCheckInAllChildPieces(mawb1);

			nonPersistentCheckInAllChildPieces.PackagesUnits = "PK";
			nonPersistentCheckInAllChildPieces.ReceivedDate = ZDateTime.BrettsBirthday;
			nonPersistentCheckInAllChildPieces.MarksAndNumbers = "Marks&Numbers";
			nonPersistentCheckInAllChildPieces.ShedStorageLocationId = locationCax.PK;
			nonPersistentCheckInAllChildPieces.IsBeingReleasedNow = true;
			nonPersistentCheckInAllChildPieces.GoodsDescription = "Good";
			nonPersistentCheckInAllChildPieces.ContainerNumber = "ContainerNumber";
			nonPersistentCheckInAllChildPieces.ContainerSeal = "ContainerSeal";
			nonPersistentCheckInAllChildPieces.IsDamaged = true;

			var result = mawb1.CheckInAllChildPieces(nonPersistentCheckInAllChildPieces);

			AssertEquals("Make new outturn for each Hawb split", 2, hawb1.OutTurns.Count);
			var newOutturn1 = hawb1.OutTurns[0];
			AssertEquals("PK", newOutturn1.C5_PackagesUnits);
			AssertEquals(ZDateTime.BrettsBirthday, newOutturn1.C5_CargoReceiptDate);
			AssertEquals("Marks&Numbers", newOutturn1.C5_MarksAndNumbers);
			AssertEquals(locationCax.PK, newOutturn1.WarehouseLocationID);
			AssertEquals(true, newOutturn1.IsBeingReleasedNow);
			AssertEquals("Good", newOutturn1.C5_GoodsDescription);
			AssertEquals("ContainerNumber", newOutturn1.C5_ContainerNumber);
			AssertEquals("ContainerSeal", newOutturn1.C5_ContainerSeal);
			AssertEquals(true, newOutturn1.C5_DamageIndicator);
			AssertEquals(1, newOutturn1.C5_PackagesOutturned);
			AssertEquals("01", newOutturn1.SplitReferenceToWhichThisPertains);
			var newOutturn2 = hawb1.OutTurns[1];
			AssertEquals("PK", newOutturn2.C5_PackagesUnits);
			AssertEquals(ZDateTime.BrettsBirthday, newOutturn2.C5_CargoReceiptDate);
			AssertEquals("Marks&Numbers", newOutturn2.C5_MarksAndNumbers);
			AssertEquals(locationCax.PK, newOutturn2.WarehouseLocationID);
			AssertEquals(true, newOutturn2.IsBeingReleasedNow);
			AssertEquals("Good", newOutturn2.C5_GoodsDescription);
			AssertEquals("ContainerNumber", newOutturn2.C5_ContainerNumber);
			AssertEquals("ContainerSeal", newOutturn2.C5_ContainerSeal);
			AssertEquals(true, newOutturn2.C5_DamageIndicator);
			AssertEquals(2, newOutturn2.C5_PackagesOutturned);
			AssertEquals("02", newOutturn2.SplitReferenceToWhichThisPertains);

			AssertContains($"HAWB {hawb2.CS_HAWB}: checked-in splits exist already set; skipping", result);
			AssertEquals("Skipped - no changes", 2, hawb2.OutTurns.Count);
		}

		public void TestProcessTaskCollection_InheritFromGeneric()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("WorkflowItems's type should inherit from ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>", typeof(ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>), ((IWorkflowProvider)mawb).WorkflowItems.GetType().BaseType);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntSetStatus1DateWithoutPieces()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.AgentBadge = "LXA";
			basic.NumberOfPiecesExpected = 0;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, basic.Status1Date);
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSaveDoesntResetStatus1Date()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			basic.AgentBadge = "LXA";
			basic.NumberOfPiecesExpected = 10;
			Factory.Save();

			var ot = basic.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 10;
			Factory.Save();
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 0, 0), basic.Status1Date);

			var djc = new ZDateTime(1979, 8, 9, 9, 56, 0);
			basic.Status1Date = djc;
			AssertEquals(djc, basic.Status1Date);
			Factory.Save();
			AssertEquals(djc, basic.Status1Date);
		}

		public void TestWhenMawbIsBeingDeletedWeDoNotCreateANewWorkerHouseHelper()
		{
			var basic = Factory.New<CusMAWB>();
			var helperPK = basic.MasterLevelHouseHelper.PK;
			basic.Delete();
			var helperAgain = basic.MasterLevelHouseHelper.PK;
			AssertEquals("Should not create a new helper while in the process of deletion", helperPK, helperAgain);
		}

		public void TestHasEntryWithLodgedOrPrelodgedWithCustoms_CDS()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000CAR";
			ICcsukCusAwb awb = basic;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var dec = awb.CreateNewStandaloneCDSDeclaration();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.EntryNumber = "123";
			entry.CH_EntryStatus = ZString.Empty;
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			entry.CH_EntryStatus = "REJ";
			AssertEquals(false, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);

			new List<string> { "ACC", "RCV", "CLR", "CAN" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(true, awb.HasEntryWithLodgedOrPrelodgedWithCustoms);
			});
		}

		public void TestWorkflowTriggerForStatus1()
		{
			var basic = Factory.New<CusMAWB>();
			var basicWorkflow = (IWorkflowProvider)basic;
			var triggerStatus1 = basicWorkflow.WorkflowItems.AddNew();
			triggerStatus1.P9_Description = "TestStatus1";
			triggerStatus1.TriggerConditions.TriggerEventCode = Events.StatusChange.Code;
			triggerStatus1.TriggerConditions.TriggerCondition = "REF";
			triggerStatus1.TriggerConditions.TriggerConditionValue = "ST1";
			triggerStatus1.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			var triggerIrrelevant = basicWorkflow.WorkflowItems.AddNew();
			triggerIrrelevant.P9_Description = "Irrelevant";
			triggerIrrelevant.TriggerConditions.TriggerCondition = "REF";
			triggerIrrelevant.TriggerConditions.TriggerConditionValue = "XXX";
			triggerIrrelevant.TriggerConditions.TriggerEventCode = Events.StatusChange.Code;
			triggerIrrelevant.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			var notificationStatus1 = triggerStatus1.ProcessTaskNotifications.AddNew();
			notificationStatus1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notificationStatus1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notificationStatus1.PQ_EmailAddr = "status1@domain.com";

			basic.Status1Date = ZDateTime.BrettsBirthday;

			AssertEquals(ZDateTime.BrettsBirthday, triggerStatus1.P9_ActualDate.ToZDateTime());
			AssertEquals(ZDateTime.Empty, triggerIrrelevant.P9_ActualDate.ToZDateTime());
		}

		public void TestCreateNewStandaloneChiefDeclaration()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "12512345678";
			basic.CM_FlightNo = "BA123";
			basic.Profile = "CUKFFW98000XXX";
			var declaration = ((ICcsukCusAwb)basic).CreateNewStandaloneCDSDeclaration();
			Assert("Factory was saved as part of create, so that the refresh of the DUCR link works", declaration.IsInDatabase);
			AssertEquals("IMP", declaration.JE_MessageType);
			AssertEquals("AIR", declaration.JE_TransportMode);
			AssertEquals("", declaration.JE_HouseBill);
			AssertEquals("12512345678", declaration.JE_MasterBill);
			AssertEquals("BAS", declaration.ZG_ShipmentType);
			AssertEquals("D", declaration.JE_EntrySubStyle);
			AssertEquals("BA123", declaration.JE_VoyageFlightNo);
			AssertEquals(declaration.PK, basic.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry);
			AssertEquals("CDS declaration", "CDS", declaration.JE_ApplicationCode);
			AssertEquals("Default Declaration Type", "H1", declaration.JE_DeclarationType);
		}

		public void TestAirportNameInWarningForDuplicateMAWBNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Facilities, "Test Facility Code");
			var shedXXX_P = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.Facilities, "XXXABC", "Some Description1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var shedYYY_Q = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.Facilities, "YYYABC", "Some Description2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			shedXXX_P.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shedXXX_P.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.ACPCode, "P"));
			shedYYY_Q.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shedYYY_Q.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.ACPCode, "Q"));
			shedXXX_P.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shedXXX_P.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.ETSF, string.Empty));
			shedYYY_Q.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shedYYY_Q.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.ETSF, string.Empty));

			Factory.Save();

			var cusMAWB1_AtXXX = Factory.New<CusMAWB>();
			var cusMAWB2_AtYYY = Factory.New<CusMAWB>();
			cusMAWB1_AtXXX.CM_MAWB = "11133333333";
			cusMAWB2_AtYYY.CM_MAWB = "11133333333";

			cusMAWB1_AtXXX.CargoTerminalOperatorAirportAndShed = shedXXX_P.ZZD_Code;
			cusMAWB2_AtYYY.CargoTerminalOperatorAirportAndShed = shedYYY_Q.ZZD_Code;
			cusMAWB1_AtXXX.NumberOfPiecesExpected = 500;
			cusMAWB2_AtYYY.NumberOfPiecesExpected = 501;
			cusMAWB1_AtXXX.ShipmentDescriptionCode = "T";
			cusMAWB2_AtYYY.ShipmentDescriptionCode = "T";

			cusMAWB1_AtXXX.AgentBadge = "MMM";
			cusMAWB2_AtYYY.AgentBadge = "NNN";

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_CustomsProfile = "LXA";
			declaration.JE_TotalNoOfPacks = 8;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;

			declaration.JE_MasterUCR = "PABC11133333333";   // at airport XXX
			Factory.Save();

			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, string.Format("The AWB mentioned in the MUCR, {0}", cusMAWB1_AtXXX.CargoTerminalOperatorAirportAndShed));
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, string.Format("Badge does not match this declaration's. AWB:MMM"));

			declaration.JE_MasterUCR = "QABC11133333333"; // at airport YYY
			Factory.Save();

			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, string.Format("The AWB mentioned in the MUCR, {0}", cusMAWB2_AtYYY.CargoTerminalOperatorAirportAndShed));
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, string.Format("Badge does not match this declaration's. AWB:NNN"));
		}

		public void TestCommunityHandlingCodes()
		{
			var basic = Factory.New<CusMAWB>();
			var chc = basic.CommunityHandlingCodes.AddNew();
			chc.Data.C4_CommunityHandlingCode = "DJC";
			AssertEquals("DJC", chc.Data.C4_CommunityHandlingCode);
			Factory.Save();
			basic = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("DJC", basic.CommunityHandlingCodes[0].Data.C4_CommunityHandlingCode);
		}

		public void TestCacCuIsWipedWhenEditingSomeFields()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRXXX";
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(basic, delegate
			{ basic.AirportOfOrigin = "NEW"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(basic, delegate
			{ basic.CM_MAWB = "NEW"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(basic, delegate
			{ basic.DescriptionOfGoods = "NEW"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertChange(basic, delegate
			{ basic.AirportOfDestination = "NEW"; });
			// No changes when not a shed
			basic.Profile = "CUKFFW98000XXX";
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.AirportOfOrigin = "NEW2"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.CM_MAWB = "NEW2"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.DescriptionOfGoods = "NEW2"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.AirportOfDestination = "NEW2"; });
			// No changes value edited but no actual change
			basic.Profile = "CUKAIR98LHRXXX";
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.AirportOfOrigin = "NEW2"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.CM_MAWB = "NEW2"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.DescriptionOfGoods = "NEW2"; });
			CusHawbTests.RunTestCacCuIsWipedWhenEditingSomeFields_AssertNoChange(basic, delegate
			{ basic.AirportOfDestination = "NEW2"; });
		}

		public void TestSetEcStatusRelease()
		{
			var basic = Factory.New<CusMAWB>();
			CusHawbTests.RunSetEcStatusReleaseTest(basic, basic.ShipmentDescriptionCodeInfo, basic.OutTurns.AddNew());
		}

		public void TestUnsetEcStatusWipesOldEdocs()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "11122222222";
			var eDocEcRra = basic.DocManagerInfo().AddFileOrDocument(ZBlob.FromAscii("Daniel"), "RRA.txt", "RRA");
			var eDocIrrelevant = basic.DocManagerInfo().AddFileOrDocument(ZBlob.FromAscii("Clarke"), "Foo.txt", "GRA");
			eDocEcRra.Description = "Comapny - Branch - Release/Removal Authority for 111-22222222 reprint blah";
			eDocIrrelevant.Description = "Company - Branch - Whatever";
			Factory.Save();
			((ICcsukCusAwb)basic).SetEcStatusRelease(false);
			Factory.Save();
			var basicReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(1, basicReloaded.DocManagerInfo().AllEDocs.Count);
		}

		public void TestLogsAndWorkflowShowsChildObjects()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			Assert("Mawb's child related objects should NOT include its real hawbs", !((IList)mawb.BusinessObjectsWithRelatedEvents).Contains(hawb));
			Assert("Mawb's child related objects should include its worker helper", ((IList)mawb.BusinessObjectsWithRelatedEvents).Contains(mawb.MasterLevelHouseHelper));
			var basic = Factory.New<CusMAWB>();
			Assert("Basic's child related objects should include its worker helper", ((IList)basic.BusinessObjectsWithRelatedEvents).Contains(basic.MasterLevelHouseHelper));
		}

		[TestDate(1986, 3, 12, 4, 0, 1)]
		public void TestLocalCreationTime()
		{
			var awb = (ICcsukCusAwb)GetNewBusinessObject();
			Factory.Save();
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 0, 1), awb.LocalCreationDate);
		}

		public void TestUpdateStatusToCacIfAllowed()
		{
			var basic1 = Factory.New<CusMAWB>();
			var basic2 = Factory.New<CusMAWB>();
			var basic3 = Factory.New<CusMAWB>();
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(basic1, CustomsStatusCodes.Codes.EntryOrRequestAccepted);
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(basic2, CustomsStatusCodes.Codes.EntryOrRequestCancelled);
			CusHawbTests.RunUpdateStatusToCacIfAllowedTest(basic3, CustomsStatusCodes.Codes.CustomsQueriedDetained);
		}

		public void TestCompleteOnCcsukAndIsCompleteOnCcsuk()
		{
			ICcsukCusAwb basic = Factory.New<CusMAWB>();
			basic.SetCustomsActionCode("DC", ZDateTime.BrettsBirthday);
			Assert(!basic.IsCompleteOnCcsuk);
			basic.CompleteOnCcsuk();
			Assert(basic.IsCompleteOnCcsuk);
			basic.UncompleteOnCcsuk();
			Assert(!basic.IsCompleteOnCcsuk);
			CusHawbTests.AssertHasStatus3Logs(basic, "DC");
			SplitBasicTests.RunSetStatus1AndStatus3TestForCompleteness(basic);
		}

		public void TestArchiveOnCcsukAndIsArchivedOnCcsuk()
		{
			ICcsukCusAwb awb = Factory.New<CusMAWB>();
			Assert(!awb.IsArchivedOnCcsuk);
			awb.ArchiveOnCcsuk(ReasonForArchiving.NprFewerThanNpxAndFinalCustomsActionDateOlderThan180Days);
			Assert(awb.IsArchivedOnCcsuk);
		}

		public void TestIsEntryCancelled_CDS()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000CAR";

			var awb = (ICcsukCusAwb)cusMawb;

			AssertEquals(false, awb.IsEntryCancelled);

			cusMawb.MasterLevelHouseHelper.CS_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			AssertEquals(false, awb.IsEntryCancelled);

			var declaration = awb.CreateNewStandaloneCDSDeclaration();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, awb.IsEntryCancelled);

			cusMawb.MasterLevelHouseHelper.CS_CustomsStatus = CustomsStatusCodes.Codes.ClearedByCustoms;
			AssertEquals(false, awb.IsEntryCancelled);

			cusMawb.MasterLevelHouseHelper.CS_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
			new List<string> { "ACC", "CLR", "RCV", "REJ" }.ForEach(x =>
			{
				entry.CH_EntryStatus = x;
				AssertEquals(false, awb.IsEntryCancelled);
			});

			entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals(true, awb.IsEntryCancelled);
		}

		[TestDate(1986, 3, 12, 4, 27, 3)]
		public void TestUFOProperties()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow");
			Factory.Save();

			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);

			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperator = "BAC";
			Assert(!mawb.IsUFO);
			mawb.CM_MAWB = "00012345678";
			Assert(!mawb.IsUFO);
			mawb.InitialiseUFO();
			AssertEquals("BAC03120427", mawb.CM_MAWB);
			Assert(mawb.IsUFO);
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 27, 3), mawb.CM_ArrivalDate);
			mawb.UfoShedStorageLocation = locationBac1.PK;
			AssertEquals(locationBac1.PK, mawb.UfoShedStorageLocation);
			mawb.UfoPiecesReceived = 69;
			AssertEquals(69, mawb.UfoPiecesReceived);
			AssertEquals((ZShort)69, mawb.NumberOfPiecesReceived);

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			var ufo = Factory.New<CusMAWB>();
			ufo.InitialiseUFO();
			AssertEquals("Two shed profiles in rego, so do not default", string.Empty, ufo.Profile);
			AssertEquals("Two shed profiles in rego, do not know which to use", "03120427", ufo.CM_MAWB);
			ufo.Profile = "CUKAIR98LHRBAC";
			AssertEquals("Setting profile updates dummy MAWB number", "BAC03120427", ufo.CM_MAWB);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestStatus1()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000ABC";
			CusHawbTests.RunStatus1Test(basic);
		}

		public void TestPiecesReleased()
		{
			var mawb = Factory.New<CusMAWB>();
			CusHawbTests.PiecesReleasedTestRunner<CusMAWB>(mawb, (string p) =>
			{ mawb.Profile = p; });
		}

		public void TestCanDelete()
		{
			var awbOkToDelete = Factory.New<CusMAWB>();
			var awbHasCACLocked = Factory.New<CusMAWB>();
			var awbHasCACUnLocked = Factory.New<CusMAWB>();
			var awbIsLodged = Factory.New<CusMAWB>();
			var awbHasSplits = Factory.New<CusMAWB>();
			var awbPendingSendWithMessage = Factory.New<CusMAWB>();
			var awbPendingSendWithNoMessage = Factory.New<CusMAWB>();
			var awbWithMessage = Factory.New<CusMAWB>();
			CusHawbTests.RunCanDeleteTest(awbOkToDelete, awbHasCACLocked, awbHasCACUnLocked, awbIsLodged, awbHasSplits, awbPendingSendWithMessage, awbPendingSendWithNoMessage, awbWithMessage);

			var mawbWithHawbs = Factory.New<CusMAWB>();
			mawbWithHawbs.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			var hawb1 = mawbWithHawbs.ChildBills.AddNew();
			var hawb2 = mawbWithHawbs.ChildBills.AddNew();
			Factory.Save(); // Put 'em in DB otherwise unsaved records can always vbe deleted
			AssertEquals("Can delete while hawbs are not cleared", true, mawbWithHawbs.CanDelete);
			hawb1.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.Now);
			AssertEquals("Pre-req: cannot delete hawb (due to CAC)", false, hawb1.CanDelete);
			AssertEquals("Can't delete while any awb forbids delete", false, mawbWithHawbs.CanDelete);
			AssertContains("The MAWB has at least one HAWB which cannot be deleted.", mawbWithHawbs.ReasonForNotAbleToDelete);
			AssertContains("Customs Action Code", mawbWithHawbs.ReasonForNotAbleToDelete);
		}

		public void TestOutturnsBasic()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA";
			var outturn1 = basic.OutTurns.AddNew();
			Factory.Save();
			var basicReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals(1, basicReloaded.OutTurns.Count);
			AssertEquals(outturn1.PK, basicReloaded.OutTurns[0].PK);
			var outturn2 = basic.OutTurns.AddNew();
			Factory.Save();

			AssertEquals(true, outturn1.C5_PackagesOutturnedInfo.ReadOnly);
			AssertEquals(true, basic.OutTurns.ReadOnly);
			AssertEquals(true, outturn1.ReadOnly);
			AssertEquals(true, basic.OutTurns.ReadOnly);
			basic.Profile = "CUKAIR98LHRBAC";
			AssertEquals(false, outturn1.ReadOnly);
			AssertEquals(false, basic.OutTurns.ReadOnly);
			basic.Profile = "CUKAIR98XXXXXX";  // invalid
			AssertEquals(true, outturn1.ReadOnly);
			AssertEquals(true, basic.OutTurns.ReadOnly);
		}

		public void TestOutturnsDontThrowNullException()
		{
			var basic = Factory.New<CusMAWBReturnsNullOutturnsForTest>();
			AssertNoExceptionThrown(() => { var x = basic.UfoShedStorageLocation; });
			AssertNoExceptionThrown(() => { basic.UfoShedStorageLocation = ZGuid.NewZGuid(); });
			AssertNoExceptionThrown(() => { var x = basic.UfoPiecesReceived; });
			AssertNoExceptionThrown(() => { basic.UfoPiecesReceived = 1; });
		}

		public void TestPresenceOnNetworkStatus()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.PresenceOnNetworkStatus = "X";
			AssertEquals("X", mawb.PresenceOnNetworkStatus);
			Factory.Save();
			var mawbReloaded = Factory.Load<CusMAWB>(mawb.PK);
			AssertEquals("X", mawbReloaded.PresenceOnNetworkStatus);
		}

		public void TestPropertiesReadOnlyFromCAC()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var b = Factory.New<CusMAWB>();
			b.Profile = "CUKAIR98LHRBAC";
			b.CM_ArrivalDate = ZDateTime.Now; //  stops it being a prearrival
			b.NumberOfPiecesReceived = 69; //  stops it being a prearrival
			var columnOneReadOnly = Array.Empty<ZPropertyInfo>();
			var allPossibleProperty = new ZPropertyInfo[] { b.CM_FlightNoInfo, b.CM_ArrivalDateInfo, b.AirportOfOriginInfo, b.AirportOfDestinationInfo, b.ShipmentDescriptionCodeInfo, b.NumberOfPiecesExpectedInfo, b.WeightInfo, b.WeightCodeInfo, b.NumberOfPiecesReceivedInfo, b.DescriptionOfGoodsInfo, b.AgentBadgeInfo, b.CM_MAWBInfo };
			var columnTwoReadOnlyPs = new ZPropertyInfo[] { b.CM_FlightNoInfo, b.CM_ArrivalDateInfo, b.AirportOfOriginInfo, b.AirportOfDestinationInfo, b.ShipmentDescriptionCodeInfo, b.NumberOfPiecesExpectedInfo, b.WeightInfo, b.WeightCodeInfo,/*NumberOfPiecesReceivedInfo*/ b.DescriptionOfGoodsInfo, b.AgentBadgeInfo, b.CM_MAWBInfo };
			var columnThreeReadOnly = new ZPropertyInfo[] { b.CM_FlightNoInfo, b.CM_ArrivalDateInfo, /*AirportOfOriginInfo, b.AirportOfDestinationInfo*/b.ShipmentDescriptionCodeInfo, b.NumberOfPiecesExpectedInfo, /*WeightInfo, b.WeightCodeInfo,  NumberOfPiecesReceivedInfo,  b.DescriptionOfGoodsInfo*/b.AgentBadgeInfo, /*CM_MAWBInfo*/ };

			b.SetCustomsActionCode("", ZDateTime.BrettsBirthday);
			CusHawbTests.AssertThesePropertiesReadOnly(columnOneReadOnly, allPossibleProperty, "CAC=blank");
			b.SetCustomsActionCode("CX", ZDateTime.BrettsBirthday);
			CusHawbTests.AssertThesePropertiesReadOnly(columnOneReadOnly, allPossibleProperty, "CAC=CX");
			b.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			CusHawbTests.AssertThesePropertiesReadOnly(columnTwoReadOnlyPs, allPossibleProperty, "CAC=CT");
			b.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			CusHawbTests.AssertThesePropertiesReadOnly(columnTwoReadOnlyPs, allPossibleProperty, "CAC=CC");
			b.SetCustomsActionCode("CU", ZDateTime.BrettsBirthday);
			CusHawbTests.AssertThesePropertiesReadOnly(columnThreeReadOnly, allPossibleProperty, "CAC=CU");
		}

		public void TestModuleControllerId()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertEquals(ControllerIDs.Customs.GB.CcsukAirInventory, cusMawb.ModuleControllerId);
		}

		public void TestDocManagerInfoAndDocumentSupporter()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertType(typeof(CcsukDocumentSupporter), cusMawb.DocumentSupporter);
			AssertType(typeof(CusMawbDocManagerInfo), ((IDocManagerSupport)cusMawb).DocManagerInfo);
		}

		public void TestProfileSetFromRegistry_HasShedLicence()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("With shed enabled there are several PIMAs available so none is selected", "", mawb.Profile);
		}

		public void TestProfileSetFromRegistry_Multiple()
		{
			MawbTestHelper.MakeBadge("ONE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false);
			MawbTestHelper.MakeBadge("TWO", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false);
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("Several profiles in registry, none set by default", "", mawb.Profile);
		}

		public void TestProfileSetFromRegistry_MultipleWithPrimary()
		{
			MawbTestHelper.MakeBadge("ONE", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: false);
			MawbTestHelper.MakeBadge("TWO", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", isPrimaryBadge: true);
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("Several profiles in registry, primary one is set by default", "CUKFFW98000TWO", mawb.Profile);
		}

		public void TestReadOnlyOnIndividualPropertiesBasedOnSplits_Agent()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000XXX";
			AssertPropertiesReadOnly(basic, false);
			AssertEquals(true, basic.NumberOfPiecesReceivedInfo.ReadOnly);
			AssertEquals(true, basic.AgentBadgeInfo.ReadOnly);
			AssertEquals(false, basic.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			basic.Splits.AddNew();
			AssertPropertiesReadOnly(basic, true);
			AssertEquals(true, basic.NumberOfPiecesReceivedInfo.ReadOnly);
		}

		public void TestReadOnlyOnIndividualPropertiesBasedOnSplits_Shed()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRBAC";
			AssertPropertiesReadOnly(basic, false);
			AssertEquals(false, basic.NumberOfPiecesReceivedInfo.ReadOnly);
			AssertEquals(false, basic.AgentBadgeInfo.ReadOnly);
			AssertEquals(true, basic.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			basic.Splits.AddNew();
			AssertPropertiesReadOnly(basic, true);
			AssertEquals(false, basic.NumberOfPiecesReceivedInfo.ReadOnly);
		}

		public void TestReadOnlyOnIdentifyingFieldsBasedOnPresence()
		{
			var basic = Factory.New<CusMAWB>();
			RunReadOnlyOnIdentifyingFieldsBasedOnPresenceTest(basic, basic.CM_MAWBInfo, basic.CargoTerminalOperatorAirportAndShedInfo, basic.AgentBadgeInfo, basic.ProfileInfo);
		}

		internal static void RunReadOnlyOnIdentifyingFieldsBasedOnPresenceTest(ICcsukCusAwb awb, ZPropertyInfo awbNumberInfo, ZPropertyInfo shedInfo, ZPropertyInfo agentBadgeInfo, ZPropertyInfo pimaInfo)
		{
			var originalLogin = GlbStaff.CurrentUser.GS_LoginName;
			var originalController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "DJC";
				GlbStaff.CurrentUser.GS_IsController = false;
				var agentPima = new ZString("CUKFFW98000XXX");
				var shedPima = new ZString("CUKAIR98LHRYYY");

				pimaInfo.Value = agentPima;
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
				AssertEquals("Not yet readonly: unknown presence - awb#", false, awbNumberInfo.ReadOnly);
				AssertEquals("Not yet readonly: unknown presence - shed", false, shedInfo.ReadOnly);

				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
				AssertEquals("Readonly - awb#", true, awbNumberInfo.ReadOnly);
				AssertEquals("Readonly - shed", true, shedInfo.ReadOnly);

				pimaInfo.Value = shedPima;
				AssertEquals("Readonly: present on network but shed", true, awbNumberInfo.ReadOnly);
				AssertEquals("The Agent Code should be editable for the Shed", false, agentBadgeInfo.ReadOnly);
				pimaInfo.Value = agentPima;
				AssertEquals("Readonly: present on network but shed", true, awbNumberInfo.ReadOnly);
				AssertEquals("The Agent Code should be Readonly for the Agent", true, agentBadgeInfo.ReadOnly);

				pimaInfo.Value = agentPima;
				GBCustomsDataRegistry.Instance.CcsukMakeAwbNumberAndNamedPartyFieldsReadOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertEquals("Not readonly: present but feature disabled - awb#", false, awbNumberInfo.ReadOnly);
				AssertEquals("Not readonly: present but feature disabled - shed", false, shedInfo.ReadOnly);

				pimaInfo.Value = agentPima;
				GBCustomsDataRegistry.Instance.CcsukMakeAwbNumberAndNamedPartyFieldsReadOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals("Not readonly: present, enabled but admin - awb#", false, awbNumberInfo.ReadOnly);
				AssertEquals("Not readonly: present, enabled but admin - shed", false, shedInfo.ReadOnly);

				GlbStaff.CurrentUser.GS_IsController = false;
				pimaInfo.Value = new ZString("nonsense");
				AssertEquals("Readonly: present, enabled, not admin, but profile is not defo a shed- awb#", true, awbNumberInfo.ReadOnly);
				AssertEquals("Readonly: present, enabled, not admin, but profile is not defo a shed - shed", true, shedInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalLogin;
				GlbStaff.CurrentUser.GS_IsController = originalController;
			}
		}

		void AssertPropertiesReadOnly(CusMAWB basic, bool shouldBeReadOnly)
		{
			var properties = new List<ZPropertyInfo>()
			{
				basic.CM_FlightNoInfo,
				basic.CM_ArrivalDateInfo,
				basic.CM_MAWBInfo,
				basic.ShipmentDescriptionCodeInfo,
				basic.NumberOfPiecesExpectedInfo,
				basic.WeightCodeInfo,
				basic.WeightInfo,
				basic.DescriptionOfGoodsInfo,
				basic.AirportOfOriginInfo,
				basic.AirportOfArrivalInfo,
				basic.AirportOfDestinationInfo,
			};
			foreach (var zpi in properties)
			{
				AssertEquals("Readonly status of " + zpi.Name, shouldBeReadOnly, zpi.ReadOnly);
			}
		}

		public void TestProfile()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000LXA";
			AssertEquals("CUKFFW98000LXA", cusMawb.Profile);
			Factory.Save();
			var cusMawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(cusMawb.PK);
			AssertEquals("CUKFFW98000LXA", cusMawbReloaded.Profile);

			AssertEquals("When selecting an agent profile, Agent field is readonly", true, cusMawb.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting an agent profile, Agent field is set", "LXA", cusMawb.AgentBadge);
			AssertEquals("When selecting an agent profile, Airport and Shed field is not readonly", false, cusMawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			AssertEquals("When selecting an agent profile, Number of pieces received field is readonly", true, cusMawb.NumberOfPiecesReceivedInfo.ReadOnly);
			cusMawb.Profile = "CUKAIR98LHRBAC";
			AssertEquals("When selecting a shed profile, Agent field is not readonly", false, cusMawb.AgentBadgeInfo.ReadOnly);
			AssertEquals("When selecting a shed profile, Shed field is readonly", true, cusMawb.CargoTerminalOperatorAirportAndShedInfo.ReadOnly);
			AssertEquals("When selecting a shed profile, Shed field is updated", "BAC", cusMawb.CargoTerminalOperator);
			AssertEquals("When selecting a shed profile, Airport field is updated", "LHR", cusMawb.CargoTerminalOperatorAirport);
			AssertEquals("When selecting a shed profile, Number of pieces received field is not readonly", false, cusMawb.NumberOfPiecesReceivedInfo.ReadOnly);
		}

		public void TestIsBasic()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertEquals(true, cusMawb.IsBasic);
			var hawb = cusMawb.ChildBills.AddNew();
			AssertEquals(false, cusMawb.IsBasic);
			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			AssertEquals("Hawbs in DEL status are not considered in the test for IsBasic", true, cusMawb.IsBasic);
			var splits = cusMawb.Splits;
			AssertEquals("When IsBasic is true, Splits should return a non-null, but empty, collection", 0, splits.Count);

			hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
			AssertEquals("Hawbs in NO status are considered in the test for IsBasic if there are no splits", false, cusMawb.IsBasic);
			splits.AddNew();
			AssertEquals("Hawbs in NO status are not considered in the test for IsBasic if there are splits", true, cusMawb.IsBasic);
		}

		public void TestIsLodgedAtCcsuk()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertEquals(false, cusMawb.IsLodgedAtCcsuk);
			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			AssertEquals(true, cusMawb.IsLodgedAtCcsuk);
		}

		public void TestIsPrearrival()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertEquals(true, cusMawb.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 69;
			AssertEquals(false, cusMawb.IsPrearrival);
			cusMawb.NumberOfPiecesReceived = 0;
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			AssertEquals(false, cusMawb.IsPrearrival);
		}

		public void TestPreArrivalFlightDetailsEditableWhenCaStatus()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKAIR98LHRBAC";
			cusMawb.AgentBadge = "LXA";
			AssertEquals(true, cusMawb.IsPrearrival);
			AssertEquals(false, cusMawb.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals(false, cusMawb.CM_FlightNoInfo.ReadOnly);
			cusMawb.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestAccepted, ZDateTime.Now);
			AssertEquals("Editable because it meets all criteria", false, cusMawb.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals("Editable because it meets all criteria", false, cusMawb.CM_FlightNoInfo.ReadOnly);
			cusMawb.Profile = "CUKAIR98LHRXXX";  // PIMA is a shed but not valid
			AssertEquals(true, cusMawb.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals(true, cusMawb.CM_FlightNoInfo.ReadOnly);
			cusMawb.Profile = "CUKFFW98000LXA";  // PIMA is valid but not shed
			AssertEquals(true, cusMawb.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals(true, cusMawb.CM_FlightNoInfo.ReadOnly);
			cusMawb.Profile = "CUKAIR98LHRBAC";
			cusMawb.AgentBadge = "XXX"; // no longer our own agent
			AssertEquals(true, cusMawb.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals(true, cusMawb.CM_FlightNoInfo.ReadOnly);
			cusMawb.AgentBadge = "LXA";
			cusMawb.CM_ArrivalDate = ZDateTime.Now;// no longer a prearrival
			AssertEquals(true, cusMawb.CM_ArrivalDateInfo.ReadOnly);
			AssertEquals(true, cusMawb.CM_FlightNoInfo.ReadOnly);
		}

		public void TestCustomsActionText()
		{
			var cusMawb = Factory.New<CusMAWB>();
			var sirMixalot = "I LIKE SMALL BUTTS AND I CANNOT LIE";
			cusMawb.LatestCustomsActionText = sirMixalot;
			AssertEquals(sirMixalot, cusMawb.LatestCustomsActionText);
			Factory.Save();
			var cusMawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(cusMawb.PK);
			AssertEquals(sirMixalot, cusMawbReloaded.LatestCustomsActionText);
		}

		public void TestRemovalCollections()
		{
			var basicMawb = Factory.New<CusMAWB>();
			var iar = basicMawb.IARs.AddNew();
			AssertEquals(iar, basicMawb.IARs[0]);
			var isr = basicMawb.ISRs.AddNew();
			AssertEquals(isr, basicMawb.ISRs[0]);
			var tsr = basicMawb.TSRs.AddNew();
			AssertEquals(tsr, basicMawb.TSRs[0]);
		}

		public void TestFallbackCollectionForConsolAndBasic()
		{
			var basicMawb = Factory.New<CusMAWB>();
			var fbk = basicMawb.FBKs.AddNew();
			AssertEquals(fbk, basicMawb.FBKs[0]);
			AssertEquals(1, basicMawb.FBKs.Count);
			AssertEquals(fbk, basicMawb.FBKs[0]);

			var consol = Factory.New<CusMAWB>();
			var house1 = consol.ChildBills.AddNew();
			var fallback1 = house1.FBKs.AddNew();
			var house2 = consol.ChildBills.AddNew();
			var fallback2 = house2.FBKs.AddNew();
			AssertEquals(0, consol.FBKs.Count);
			AssertEquals(house1.PK, fallback1.WholeAwb.PK);
			AssertEquals(house2.PK, fallback2.WholeAwb.PK);
		}

		public void TestCustomsActionCodeAndDate()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday);
			AssertEquals("CT", mawb.CustomsActionCode);
			AssertEquals(ZDateTime.BrettsBirthday, mawb.CustomsActionDate);
			Factory.Save();
			mawb = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertEquals("CAC visible upon reload", "CT", mawb.CustomsActionCode);
			mawb.SetCustomsActionCode("CT", ZDateTime.BrettsBirthday.AddDays(1));
			AssertEquals("CAC visible upon reload", "CT", mawb.CustomsActionCode);
			AssertEquals("CAC date updated when CAC set again to same value (allows CAC to go from blank->CT->CX->CT again)", ZDateTime.BrettsBirthday.AddDays(1), mawb.CustomsActionDate);
			mawb.SetCustomsActionCode("BB", ZDateTime.BrettsBirthday.AddYears(1));
			mawb.LatestCustomsActionText = "You still suck";
			AssertEquals("BB", mawb.CustomsActionCode);
			AssertEquals("Log date now shows date of new code", ZDateTime.BrettsBirthday.AddYears(1), mawb.CustomsActionDate);
		}

		public void TestConsolAndIsLinkedToJobConsol()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertEquals(false, cusMawb.IsLinkedToJobConsol);
			var consol = Factory.New<ForwardingConsol>();
			cusMawb.CM_JK = consol.PK;
			AssertEquals(consol, cusMawb.Consol);
			AssertEquals(true, cusMawb.IsLinkedToJobConsol);
		}

		public void TestCreateNewAndSynchroniseData()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			consol.JK_RL_NKPortOfFirstArrival = "GBMAN";
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "125-12345678";
			consol.JK_RL_NKDischargePort = "GBMNC";
			consol.JK_RL_NKLoadPort = "AUSYD";
			shipment.JS_OuterPacks = 60;
			shipment.JS_UnitOfWeight = "KG";
			consol.Transports.AddNew();
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ATA = ZDateTime.BrettsBirthday;
			transport.JW_VoyageFlight = "BA123";
			shipment.JS_ActualWeight = 50m;

			var mawb = CusMAWB.CreateNew(consol);
			AssertEquals(mawb.CM_JK, consol.PK);
			AssertEquals(ZDateTime.BrettsBirthday, mawb.CM_ArrivalDate);
			AssertEquals("BA123", mawb.CM_FlightNo);
			AssertEquals("12512345678", mawb.CM_MAWB);
			AssertEquals("MAN", mawb.AirportOfDestination);
			AssertEquals("AUSYD", mawb.AirportOfOrigin);
			AssertEquals(50m, mawb.Weight);
			AssertEquals("KG", mawb.WeightCode);
			AssertEquals((ZShort)60, mawb.NumberOfPiecesExpected);
		}

		public void TestTypes()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertType(typeof(CusMAWBValidation), cusMawb.Validation);
			AssertType(typeof(CusMAWBLookups), cusMawb.Lookups);
			var col = cusMawb.ChildBills;
			AssertType(typeof(CusHAWBDependentCollection), col);
		}

		public void TestReferenceNumberAndMawbNumbersAndHumanReadableName()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CargoTerminalOperator = "CAX";
			AssertEquals("", mawb.ReferenceNumber);
			mawb.CM_MAWB = "12387654321";
			AssertEquals("123-87654321", mawb.CM_MAWB_Formatted);
			AssertEquals("123-87654321", mawb.ReferenceNumber);
			AssertEquals("CCS-UK Basic Air Waybill 123-87654321", mawb.HumanReadableName);
			mawb.ChildBills.AddNew();
			AssertEquals("CCS-UK Master Air Waybill 123-87654321", mawb.HumanReadableName);
			AssertEquals("LHRCAX-123-87654321", mawb.ReferenceNumberWithShed);
		}

		public void TestUserInChargeOfJob()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "DJC";
			var mawb = Factory.New<CusMAWB>();
			AssertNull(mawb.UserInChargeOfJob);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_SystemCreateUser = "DJC";
			mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			AssertEquals(null, mawb.UserInChargeOfJob);

			mawb = Factory.New<CusMAWB>();
			var message = mawb.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_SystemCreateUser = "DJC";
			AssertEquals(user.PK, mawb.UserInChargeOfJob.PK);
		}

		public void TestPorts()
		{
			var mawb = Factory.New<CusMAWB>();
			PortsTestRunner(mawb, delegate
			{ mawb.CargoTerminalOperatorAirport = ""; });
		}

		internal static void PortsTestRunner(ICcsukCusAwb awb, Action resetCargoTerminalOperatorAirport)
		{
			AssertEquals("", awb.CargoTerminalOperatorAirport);
			AssertEquals("", awb.AirportOfDestination);
			AssertEquals("", awb.AirportOfArrival);
			AssertEquals("", awb.AirportOfOrigin);

			awb.AirportOfArrival = "LHR";
			AssertEquals("LHR", awb.AirportOfArrival);
			AssertEquals("LHR", awb.AirportOfDestination);
			AssertEquals("LHR", awb.CargoTerminalOperatorAirport);
			AssertEquals("", awb.AirportOfOrigin);

			awb.AirportOfArrival = "";
			awb.AirportOfDestination = "";
			resetCargoTerminalOperatorAirport();

			awb.AirportOfDestination = "LGW";
			AssertEquals("LGW", awb.CargoTerminalOperatorAirport);
			AssertEquals("LGW", awb.AirportOfDestination);
			AssertEquals("LGW", awb.AirportOfArrival);
			AssertEquals("", awb.AirportOfOrigin);
		}

		public void TestShipmentDescriptionCodeAndConsignmentOrEntryType()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport;
			AssertEquals(ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport, cusMawb.ShipmentDescriptionCode);
			cusMawb.ConsignmentOrEntryType = ConsignmentOrEntryTypes.Codes.Import;
			AssertEquals(ConsignmentOrEntryTypes.Codes.Import, cusMawb.ConsignmentOrEntryType);
			Factory.Save();
			var mawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(cusMawb.PK);
			AssertEquals(ConsignmentOrEntryTypes.Codes.Import, mawbReloaded.ConsignmentOrEntryType);
		}

		public void TestSettingAndValidatationOfSDCPortCombination()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			cusMawb.AirportOfOrigin = "USJFK";
			cusMawb.AirportOfDestination = "GBLHR";
			AssertEquals("T", cusMawb.ShipmentDescriptionCode);
			AssertNoMessageErrors(cusMawb.AirportOfOriginInfo);
			cusMawb.ShipmentDescriptionCode = "C";
			AssertHasErrorContaining(cusMawb.ShipmentDescriptionCodeInfo, "Origin outside EU requires SDC T or M");

			cusMawb.ShipmentDescriptionCode = "";
			cusMawb.AirportOfOrigin = "ATL";
			cusMawb.AirportOfDestination = "GBLHR";
			AssertEquals("T", cusMawb.ShipmentDescriptionCode);

			cusMawb.ShipmentDescriptionCode = "";
			cusMawb.AirportOfOrigin = "FRPAR";
			cusMawb.AirportOfDestination = "LHR";
			AssertEquals("C", cusMawb.ShipmentDescriptionCode);
			AssertEquals(false, cusMawb.ShipmentDescriptionCodeInfo.HasErrors());
			cusMawb.ShipmentDescriptionCode = "E";
			AssertEquals(false, cusMawb.ShipmentDescriptionCodeInfo.HasErrors());
			cusMawb.ShipmentDescriptionCode = "C";
			AssertEquals(false, cusMawb.ShipmentDescriptionCodeInfo.HasErrors());
			cusMawb.ShipmentDescriptionCode = "T";
			AssertEquals(false, cusMawb.ShipmentDescriptionCodeInfo.HasErrors());

			cusMawb.ShipmentDescriptionCode = "";
			cusMawb.AirportOfOrigin = "ATL";
			cusMawb.AirportOfDestination = "CDG";
			AssertEquals("T", cusMawb.ShipmentDescriptionCode);

			cusMawb.ShipmentDescriptionCode = "";
			cusMawb.AirportOfDestination = "CDG";
			cusMawb.AirportOfOrigin = "ATL";
			AssertEquals("T", cusMawb.ShipmentDescriptionCode);
			cusMawb.AirportOfOrigin = "FRA";
			AssertEquals("C", cusMawb.ShipmentDescriptionCode);
		}

		public void TestChangingPortsWipesEcStatus()
		{
			var basic = Factory.New<CusMAWB>();
			RunTestChaningPortWipesEcStatus(basic);
		}

		internal static void RunTestChaningPortWipesEcStatus(ICcsukCusAwb awb)
		{
			awb.AirportOfOrigin = "DEFRA";
			awb.AirportOfArrival = "LHR";
			awb.AirportOfDestination = "LHR";
			AssertEquals("C", awb.ShipmentDescriptionCode);
			awb.NumberOfPiecesExpected = 10;
			awb.NumberOfPiecesReceived = 10;
			awb.Status1Date = ZDateTime.BrettsBirthday;
			awb.SetEcStatusRelease(true);
			awb.AirportOfOrigin = "USATL";
			AssertEquals("T", awb.ShipmentDescriptionCode);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, awb.PresenceOnNetworkStatus);
			AssertEquals("", awb.CustomsActionCode);
			AssertEquals("", awb.LatestCustomsActionText);

			awb.AirportOfOrigin = "DEFRA";
			awb.AirportOfArrival = "LHR";
			awb.AirportOfDestination = "LHR";
			AssertEquals("C", awb.ShipmentDescriptionCode);
			awb.SetEcStatusRelease(true);
			awb.AirportOfDestination = "SYD";
			AssertEquals("T", awb.ShipmentDescriptionCode);
			AssertEquals(PresenceOnNetworkList.Codes.OnCommDb, awb.PresenceOnNetworkStatus);
			AssertEquals("", awb.CustomsActionCode);
			AssertEquals("", awb.LatestCustomsActionText);
		}

		public void TestNumberOfPiecesExpected()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.NumberOfPiecesExpected = 69;
			AssertEquals((ZShort)69, mawb.NumberOfPiecesExpected);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals((ZShort)69, mawbReloaded.NumberOfPiecesExpected);
		}

		[TestDate(2015, 8, 22, 14, 00, 00)]
		public void TestNumberOfPiecesReceived_Basic()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA";
			basic.NumberOfPiecesExpected = 69;
			basic.NumberOfPiecesReceived = 70;
			basic.Factory.Save();
			var mawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(basic.PK);
			AssertEquals("For agents, NPR is stored", (ZShort)70, mawbReloaded.NumberOfPiecesReceived);
			basic.Profile = "CUKAIR98LHRBAC";
			basic.NumberOfPiecesReceived = 70;
			var ot1 = basic.OutTurns.AddNew();
			ot1.C5_PackagesOutturned = 50;
			var ot2 = basic.OutTurns.AddNew();
			ot2.C5_PackagesOutturned = 19;
			basic.Factory.Save();
			AssertEquals((ZShort)69, basic.NumberOfPiecesExpected);
			AssertEquals("For sheds, NPR is calculated from the outTurns", (ZShort)69, basic.NumberOfPiecesReceived);
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 00, 00), basic.Status1Date);
		}

		public void TestCompleteStatusProcessingForConsolidationOfHouses()
		{
			// if you have a consol whose houses are all complete and you change NPR or NPX on the consol, it shoudl update to COM regardless of its own status 1
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKAIR98LHRBAC";
			mawb.NumberOfPiecesExpected = 10;
			ICcsukCusAwb hawb1 = mawb.ChildBills.AddNew();
			ICcsukCusAwb hawb2 = mawb.ChildBills.AddNew();
			hawb1.NumberOfPiecesExpected = 6;
			hawb1.NumberOfPiecesReceived = 6;
			hawb2.NumberOfPiecesExpected = 4;
			hawb2.NumberOfPiecesReceived = 4;
			hawb1.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.Now);
			AssertEquals(false, ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			hawb2.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.Now);
			AssertEquals("Setting last hawb as complete marks mawb as complete", true, ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			mawb.NumberOfPiecesExpected = 11;
			AssertEquals("Changing NPX does not clobber COM if all houses are COM", true, ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			mawb.NumberOfPiecesExpected = 10;
			hawb1.NumberOfPiecesReceived = 5;
			AssertEquals("Revoking st1 on hawb revokes COM", false, hawb1.IsCompleteOnCcsuk);
			AssertEquals("Revokation propogates", false, ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			hawb1.NumberOfPiecesExpected = 5;
			AssertEquals(true, hawb1.IsCompleteOnCcsuk);
			AssertEquals("MAWB marked as COM even without status 1", true, ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
			var hawb3 = mawb.ChildBills.AddNew();
			AssertEquals("New house wipes COM from mawb", false, ((ICcsukCusAwb)mawb).IsCompleteOnCcsuk);
		}

		public void TestNumberOfPiecesReceived_Consolidation()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000LXA";
			var hawb1 = mawb.ChildBills.AddNew();
			var hawb2 = mawb.ChildBills.AddNew();
			mawb.NumberOfPiecesReceived = 70;
			mawb.Factory.Save();
			var mawbReloaded = new BusinessObjectFactory().Load<CusMAWB>(mawb.PK);
			AssertEquals("For agents, NPR is stored", (ZShort)70, mawbReloaded.NumberOfPiecesReceived);
			mawb.Profile = "CUKAIR98LHRBAC";
			hawb1.Profile = "CUKAIR98LHRBAC";
			hawb2.Profile = "CUKAIR98LHRBAC";
			var ot1 = hawb1.OutTurns.AddNew();
			ot1.C5_PackagesOutturned = 50;
			var ot2 = hawb2.OutTurns.AddNew();
			ot2.C5_PackagesOutturned = 10;
			Factory.Save();
			AssertEquals("For sheds, NPR is calculated from the houses' outTurns", (ZShort)50, hawb1.CS_PiecesLanded);
			AssertEquals("For sheds, NPR is calculated from the houses' outTurns", (ZShort)10, hawb2.CS_PiecesLanded);
			AssertEquals("For sheds, NPR is calculated from the houses' outTurns", (ZShort)60, mawb.NumberOfPiecesReceived);
		}

		public void TestWeightAndUnit()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.Weight = 69m;
			AssertEquals(69m, mawb.Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, mawb.WeightCode);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals(69m, mawbReloaded.Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, mawbReloaded.WeightCode);
		}

		public void TestCargoTerminalOperator()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperator = "BAC";
			AssertEquals("BAC", mawb.CargoTerminalOperator);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals("BAC", mawbReloaded.CargoTerminalOperator);

			mawb.CargoTerminalOperatorAirport = "LHR";
			AssertEquals("BAC", mawb.CargoTerminalOperator);
			Factory.Save();
			mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals("BAC", mawbReloaded.CargoTerminalOperator);
		}

		public void TestCargoTerminalOperatorAirport()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperatorAirport = "LHR";
			AssertEquals("LHR", mawb.CargoTerminalOperatorAirport);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals("LHR", mawbReloaded.CargoTerminalOperatorAirport);

			mawb.CargoTerminalOperator = "BAC";
			AssertEquals("LHR", mawb.CargoTerminalOperatorAirport);
			Factory.Save();
			mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals("LHR", mawbReloaded.CargoTerminalOperatorAirport);
		}

		public void TestAgentBadge()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.AgentBadge = "ZPE";
			AssertEquals("ZPE", mawb.AgentBadge);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals("ZPE", mawbReloaded.AgentBadge);
		}

		public void TestDescriptionOfGoods()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("", mawb.DescriptionOfGoods);
			mawb.DescriptionOfGoods = "Some other stuf$";
			AssertEquals("SOME OTHER STUF", mawb.DescriptionOfGoods);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals("SOME OTHER STUF", mawbReloaded.DescriptionOfGoods);

			mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertEquals("CONSOLIDATION", mawb.DescriptionOfGoods);
			var hawb2 = mawb.ChildBills.AddNew();
			AssertEquals("CONSOLIDATION", mawb.DescriptionOfGoods);
			mawb.ChildBills.Remove(hawb2);
			AssertEquals("CONSOLIDATION", mawb.DescriptionOfGoods);
			mawb.ChildBills.Remove(hawb);
			AssertEquals("", mawb.DescriptionOfGoods);
			mawb.DescriptionOfGoods = "Stuff";
			var hawb3 = mawb.ChildBills.AddNew();
			AssertEquals("STUFF", mawb.DescriptionOfGoods);
			mawb.ChildBills.Remove(hawb3);
			AssertEquals("STUFF", mawb.DescriptionOfGoods);
		}

		public void TestChildrenDoesNotContainBastardHouse()
		{
			var mawb = Factory.New<CusMAWB>();
			var houses = mawb.ChildBills;
			houses.Load();
			AssertEquals("No kids, not even that little BastardHouse", 0, houses.Count);
			mawb.NumberOfPiecesExpected = 69;
			houses.Load();
			AssertEquals("Still no kids, especially not that little BastardHouse", 0, houses.Count);
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_CM = mawb.PK;
			houses.Load();
			AssertEquals("Now one child, but not BastardHouse", 1, houses.Count);
			AssertEquals(houses[0].PK, hawb.PK);
		}

		[TestDate(1987, 12, 11, 1, 2, 3)]
		public void TestStatus1Date()
		{
			// Maybe change this test to run as a shed. Setting NPP directly on an agent job isa not realistic - it should come via message, when the status 1 date will be present.
			var mawb = Factory.New<CusMAWB>();
			mawb.Profile = "CUKFFW98000ABC";
			Assert(mawb.Status1Date.IsEmpty);
			mawb.NumberOfPiecesExpected = 69;
			Assert(mawb.Status1Date.IsEmpty);
			mawb.NumberOfPiecesReceived = 40;
			Assert(mawb.Status1Date.IsEmpty);
			mawb.NumberOfPiecesReceived = 69;
			AssertEquals(true, mawb.Status1Date.IsEmpty);
			mawb.Status1Date = new ZDateTime(1987, 12, 11, 1, 2, 3);
			AssertEquals(new ZDateTime(1987, 12, 11, 1, 2, 3), mawb.Status1Date);
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			AssertEquals(false, mawbReloaded.Status1Date.IsEmpty);
			AssertEquals(new ZDateTime(1987, 12, 11, 1, 2, 3), mawbReloaded.Status1Date);
		}

		public void TestStatus2Granted()
		{
			var mawb = Factory.New<CusMAWB>();
			Assert("Detaulf value", mawb.Status2Granted);
			mawb.Status2Granted = false;
			Assert("Value set", !mawb.Status2Granted);
			mawb.Status2Granted = false;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var mawbReloaded = factory2.Load<CusMAWB>(mawb.PK);
			Assert("Value persisted and reloaded", !mawbReloaded.Status2Granted);

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.Status2Granted = false;
			var hawb1 = mawb2.ChildBills.AddNew();
			AssertEquals("Value inherited by new house", false, hawb1.Status2Granted);
			mawb2.Status2Granted = true;
			var hawb2 = mawb2.ChildBills.AddNew();
			AssertEquals("Value inherited by new house", true, hawb2.Status2Granted);
			AssertEquals("Updated value propgated to new house", true, hawb1.Status2Granted);
			hawb1.Status2Granted = false;
			AssertEquals("Value set on house", false, hawb1.Status2Granted);
			AssertEquals("House value doe snot affect mawb", true, mawb2.Status2Granted);
			Factory.Save();
			var mawb2Reloaded = new BusinessObjectFactory().Load<CusMAWB>(mawb2.PK);
			var hawb1Reloaded = mawb2Reloaded.Factory.Load<CusHAWB>(hawb1.PK);
			var hawb2Reloaded = mawb2Reloaded.Factory.Load<CusHAWB>(hawb2.PK);
			AssertEquals(true, mawb2Reloaded.Status2Granted);
			AssertEquals("House value persisted and reloaded", false, hawb1Reloaded.Status2Granted);
			AssertEquals("House value persisted and reloaded", true, hawb2Reloaded.Status2Granted);
		}

		public void TestMessagesProxiedFromHouse()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals(0, mawb.Messages.Count);
			mawb.Messages.AddNew();
			AssertEquals(1, mawb.Messages.Count);
			AssertEquals(1, mawb.MasterLevelHouseHelper.Messages.Count);
			AssertEquals(mawb.MasterLevelHouseHelper.Messages[0], mawb.Messages[0]);
		}

		public void TestIsLodgedOrAssumedAtCcsuk()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			Factory.Save();

			Assert("CusMawb_Assumed", cusMawb.IsLodgedOrAssumedAtCcsuk);

			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Assert("CusMawb_Lodged", cusMawb.IsLodgedOrAssumedAtCcsuk);
		}

		public void TestBrexitCcsukSdcShipmentDescriptionCode()
		{
			var ukPort = Core.Constants.CountryCodes.UnitedKingdom + "LHR";
			var euPort = Core.Constants.CountryCodes.France + "PAR";
			var xiPort = GetNorthernIrelandPort(Factory);

			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = "";
			var frCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			frCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			AssertEquals("SDC Empty", "", mawb.ShipmentDescriptionCode);

			mawb.CM_RL_NKLoadPort = euPort;
			mawb.CM_RL_NKDischargePort = xiPort;
			AssertEquals("SDC Value", "C", mawb.ShipmentDescriptionCode);

			mawb.CM_RL_NKDischargePort = ukPort;
			AssertEquals("SDC Value", "T", mawb.ShipmentDescriptionCode);
		}

		internal static string GetNorthernIrelandPort(BusinessObjectFactory factory)
		{
			var belfast = new RefUNLOCO.Loader(factory).Load(Core.Constants.CountryCodes.UnitedKingdom + "BEL");
			if (!belfast.IsInNorthernIreland)
			{
				var ni = factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;
				belfast.RL_RW = ni.PK;
			}
			Assert("Pre-requisite: GBBEL must be InNorthernIreland!", belfast.IsInNorthernIreland);
			return belfast.Code;
		}

		public void TestCM_MAWBMaxLength()
		{
			AssertEquals("Pre Req", 12, CusMAWB.Schema.CM_MAWBMaxLength);

			var cusMawb = Factory.New<CusMAWB>();
			var value = "123456789012";
			cusMawb.CM_MAWB = value;
			AssertEquals(value, cusMawb.CM_MAWB);

			cusMawb.CM_MAWB = $"{value}3456";
			AssertEquals(value, cusMawb.CM_MAWB);
		}
	}

	[TestedType(typeof(CusMAWB.Loader))]
	class CusMAWBLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusMAWB.Loader(Factory);
		}

		public void TestLoadFromConsolPkOrMasterNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var loader = new CusMAWB.Loader(Factory);
			var result = loader.LoadFromConsolPkOrMasterNumber(consol);
			AssertNull("Find no mawb yet", result);
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_JK = consol.PK;
			result = loader.LoadFromConsolPkOrMasterNumber(consol);
			AssertEquals("Find mawb by consol PK", cusMawb.PK, result.PK);
			cusMawb.CM_JK = Guid.Empty;
			cusMawb.CM_MAWB = "123456789001";
			consol.JK_MasterBillNum = "123-456789001";
			result = loader.LoadFromConsolPkOrMasterNumber(consol);
			AssertEquals("Find mawb by mawb number", cusMawb.PK, result.PK);
		}

		// Test removed - It's not valid to have two MAWBs with same mawb number, shed and date separated only by branch. 

		public void TestFindFromMawbNumberAndLocation()
		{
			var basicAtManchester = Factory.New<CusMAWB>();
			basicAtManchester.CM_MAWB = "Daniel";
			basicAtManchester.CargoTerminalOperator = "DAN";
			basicAtManchester.CargoTerminalOperatorAirport = "MAN";
			var basicAtHeathrow = Factory.New<CusMAWB>();
			basicAtHeathrow.CM_MAWB = "Daniel";
			basicAtHeathrow.CargoTerminalOperator = "DAN";
			basicAtHeathrow.CargoTerminalOperatorAirport = "LHR";
			Factory.Save();

			var loader = new CusMAWB.Loader(Factory);
			var mawbFound = loader.FindFromMawbNumber("Daniel", "");
			Assert(mawbFound.PK == basicAtHeathrow.PK || mawbFound.PK == basicAtManchester.PK);
			mawbFound = loader.FindFromMawbNumber("Daniel", "LHRDAN");
			AssertEquals(basicAtHeathrow.PK, mawbFound.PK);
			mawbFound = loader.FindFromMawbNumber("Daniel", "MANDAN");
			AssertEquals(basicAtManchester.PK, mawbFound.PK);
			mawbFound = loader.FindFromMawbNumber("Daniel", "POOP");
			AssertEquals(null, mawbFound);
			mawbFound = loader.FindFromMawbNumber("POOP", "");
			AssertEquals(null, mawbFound);
		}

		public void TestFindFromMawbNumberAndAgentBadge()
		{
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "12378776655";
			mawb1.AgentBadge = "WIS";
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "12378776655";
			mawb2.AgentBadge = "DAN";
			Factory.Save();

			var loader = new CusMAWB.Loader(Factory);
			var mawbFound = loader.FindFromMawbNumberAndAgentBadge("12378776655", "");
			Assert(mawbFound.PK == mawb1.PK || mawbFound.PK == mawb2.PK);

			mawbFound = loader.FindFromMawbNumberAndAgentBadge("12378776655", "WIS");
			AssertEquals(mawb1.PK, mawbFound.PK);

			mawbFound = loader.FindFromMawbNumberAndAgentBadge("12378776655", "DAN");
			AssertEquals(mawb2.PK, mawbFound.PK);

			mawbFound = loader.FindFromMawbNumberAndAgentBadge("12378776655", "AMY");
			AssertEquals(null, mawbFound);

			mawbFound = loader.FindFromMawbNumberAndAgentBadge("12345678911", "");
			AssertEquals(null, mawbFound);
		}

		public void TestLoad()
		{
			var cusMawbBase = Factory.New<Customs.Business.CusMAWB>();
			var cusMawbUk = Factory.New<CusMAWB>();
			var consol = Factory.New<ForwardingConsol>();
			cusMawbUk.CM_JK = consol.PK;
			var loader = new CusMAWB.Loader(Factory);
			var cusMawbReloaded = loader.Load(consol);
			AssertEquals(cusMawbUk.PK, cusMawbReloaded.PK);
			cusMawbUk.CM_JK = ZGuid.Empty;
			cusMawbBase.CM_JK = consol.PK;
			cusMawbReloaded = loader.Load(consol);
			AssertNull(cusMawbReloaded);
		}
	}

	class CusMawbFormatValidationTests : Customs.Business.Testing.CusMAWBValidationTest
	{
		const string EmptyMawbValidationMessageSubstring = "Master";
		const string LengthValidationMessageSubstring = "11 characters";
		const string FormatValidationMessageSubstring = "letters or digits";
		const string CheckDigitValidationMessageSubstring = "check digit";

		public void TestEmptyMAWB_ShouldHaveError()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Validation.ValidateCM_MAWB();
			AssertHasErrorContaining(cusMawb.CM_MAWBInfo, EmptyMawbValidationMessageSubstring);
		}

		public void TestValidMAWB_ShouldHaveNoFormatErrorsOrWarnings()
		{
			var cusMawb = Factory.New<CusMAWB>();

			// Workaround against TestCaseWithFactory.FailIfSomeoneCalledAssertNoWithoutCallingAssertHas 
			cusMawb.CM_MAWB = "12312345670";
			AssertHasWarningContaining(cusMawb.CM_MAWBInfo, CheckDigitValidationMessageSubstring);

			cusMawb.CM_MAWB = "12312345675";
			AssertNoFormatWarningsOrErrors();

			cusMawb.CM_MAWB = "LHR12345675";
			AssertNoFormatWarningsOrErrors();

			void AssertNoFormatWarningsOrErrors() =>
				CombineAssertions(() =>
				{
					AssertNoErrors(cusMawb.CM_MAWBInfo);
					AssertNoMessageErrors(cusMawb.CM_MAWBInfo);
					AssertNoWarningContaining(cusMawb.CM_MAWBInfo, CheckDigitValidationMessageSubstring); // We expect one warning "You are creating a BASIC record..." not related to format validations, so it is not possible to use AssertNoWarnings
				});
		}

		public void TestMAWBWithLettersInDigitsPart_ShouldHaveMessageError()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "LHR1234567X";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, FormatValidationMessageSubstring);
			cusMawb.CM_MAWB = "1231234567X";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, FormatValidationMessageSubstring);
		}

		public void TestTooShortMAWB_ShouldHaveMessageError()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "LHR123";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, LengthValidationMessageSubstring);
			cusMawb.CM_MAWB = "123123";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, LengthValidationMessageSubstring);
		}

		public void TestTooLongMAWB_ShouldHaveMessageError()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "LHR123456789";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, LengthValidationMessageSubstring);
			cusMawb.CM_MAWB = "123123456789";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, LengthValidationMessageSubstring);
		}

		public void TestMAWBWithIncorrectCheckDigit_ShouldHaveWarning()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "LHR12345678";
			AssertHasWarningContaining(cusMawb.CM_MAWBInfo, CheckDigitValidationMessageSubstring);
			cusMawb.CM_MAWB = "12312345678";
			AssertHasWarningContaining(cusMawb.CM_MAWBInfo, CheckDigitValidationMessageSubstring);
		}

		public void TestMAWBWithNonDigitCharacterInDigitsPart_ShouldHaveMessageError()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "LHR1234567X";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, FormatValidationMessageSubstring);
			cusMawb.CM_MAWB = "1231234567X";
			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, FormatValidationMessageSubstring);
		}
	}

	class CusMawbValidationTests : Customs.Business.Testing.CusMAWBValidationTest
	{
		public void TestValidateCM_GBForPima()
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(aaaBranch.PK.ToGuid()))
			{
				AddBadgeAndCredToExisting("AAA");
			}
			AddBadgeAndCredToExisting("BBB");
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000BBB";
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "current PIMA");
			cusMawb.CM_GB = aaaBranch.PK;
			AssertHasErrorContaining(cusMawb.CM_GBInfo, "current PIMA");
			AssertHasErrorContaining(cusMawb.ProfileInfo, "valid profile");
			cusMawb.Profile = "CUKFFW98000AAA";
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "current PIMA");
			AssertNoErrorContaining(cusMawb.ProfileInfo, "valid profile");
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "Please enter");
			cusMawb.CM_GB = ZGuid.Empty;
			AssertNoErrorContaining(cusMawb.CM_GBInfo, "current PIMA");
			AssertHasErrorContaining(cusMawb.CM_GBInfo, "Please enter");
		}

		void AddBadgeAndCredToExisting(string badgeCode)
		{
			var badge = new BadgeCodeSetting();
			badge.BadgeCode = badgeCode;
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badgeCode;
			credential.PIMA = "CUKFFW98000" + badgeCode;
			var creds = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			creds.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);
		}

		public void TestShipmentDescriptionCodeSDCValidation()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Validation.ValidateAll();
			AssertHasErrorContaining(basic.ShipmentDescriptionCodeInfo, "Shipment Description Code");
			AssertHasErrorContaining(basic.ShipmentDescriptionCodeInfo, "Please enter a Shipment Description Code");
			basic.ShipmentDescriptionCode = "X";
			AssertHasErrorContaining(basic.ShipmentDescriptionCodeInfo, "Please select a valid Shipment Description Code");
			basic.ShipmentDescriptionCode = "T";
			AssertNoErrorContaining(basic.ShipmentDescriptionCodeInfo, "Please select a valid Shipment Description Code");
			basic.InitialiseUFO();
			basic.ShipmentDescriptionCode = "";
			AssertNoErrorContaining(basic.ShipmentDescriptionCodeInfo, "Shipment Description Code");
			AssertNoErrorContaining(basic.ShipmentDescriptionCodeInfo, "SDC");
			basic.CM_ArrivalDate = ZDateTime.Empty;
			basic.NumberOfPiecesReceived = 0;
			Assert(basic.IsPrearrival);
			basic.ShipmentDescriptionCode = "M";
			AssertHasErrorContaining(basic.ShipmentDescriptionCodeInfo, "SDC");
		}

		public void TestValidationForBasic()
		{
			var cusMawb = Factory.New<CusMAWB>();
			AssertEquals("Pre-req - IsBasic", true, cusMawb.IsBasic);
			cusMawb.CM_MAWB = "x";
			AssertHasWarningContaining(cusMawb.CM_MAWBInfo, "BASIC");
			cusMawb.Profile = "CUKAIR98LHRBAC";
			cusMawb.CM_MAWB = "y";
			AssertNoWarningContaining(cusMawb.CM_MAWBInfo, "add houses");
			cusMawb.Profile = "CUKFFW98000LXA";
			cusMawb.CM_MAWB = "x";
			AssertHasWarningContaining(cusMawb.CM_MAWBInfo, "add houses");
			cusMawb.ChildBills.AddNew();
			AssertEquals("Pre-req - IsBasic", false, cusMawb.IsBasic);
			cusMawb.CM_MAWB = "y";
			AssertNoWarningContaining(cusMawb.CM_MAWBInfo, "BASIC");
		}

		public void TestValidateConsignmentOrEntryType()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.ConsignmentOrEntryType = "X";
			AssertHasWarningContaining(cusMawb.ConsignmentOrEntryTypeInfo, "valid code");
			cusMawb.ConsignmentOrEntryType = ConsignmentOrEntryTypes.Codes.Import;
			AssertNoWarningContaining(cusMawb.ConsignmentOrEntryTypeInfo, "valid code");
		}

		public void TestProfilePimaFolioValidation()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.Validation.ValidateAll();
			AssertHasErrorContaining(mawb.ProfileInfo, "PIMA");
			mawb.Profile = "PROFILE";
			AssertHasErrorContaining(mawb.ProfileInfo, "PIMA");
			mawb.Profile = "CUKFFW98000LXA";
			AssertNoErrorContaining(mawb.ProfileInfo, "PIMA");
		}

		public void TestCheckCM_FlightNo()
		{
			var etsfMawb = Factory.New<CusMAWB>();
			etsfMawb.Profile = "CUKAIR98ABC123"; 
			etsfMawb.Validation.ValidateCM_FlightNo();

			AssertHasMessageErrorContaining(etsfMawb.CM_FlightNoInfo, "mandatory");

			etsfMawb.CM_FlightNo = "BA3";
			etsfMawb.Validation.ValidateCM_FlightNo();
			AssertHasMessageErrorContaining(etsfMawb.CM_FlightNoInfo, "5 characters");

			etsfMawb.CM_FlightNo = "BA003";
			etsfMawb.Validation.ValidateCM_FlightNo();
			AssertNoMessageErrorContaining(etsfMawb.CM_FlightNoInfo, "mandatory");
			AssertNoMessageErrorContaining(etsfMawb.CM_FlightNoInfo, "5 characters");

			var agentMawb = Factory.New<CusMAWB>();
			agentMawb.Profile = "CUKFFW98XYZ";
			agentMawb.Validation.ValidateCM_FlightNo();

			AssertNoMessageErrorContaining(agentMawb.CM_FlightNoInfo, "mandatory");

			agentMawb.CM_FlightNo = "BA3";
			agentMawb.Validation.ValidateCM_FlightNo();
			AssertNoMessageErrorContaining(agentMawb.CM_FlightNoInfo, "5 characters");

			agentMawb.CM_FlightNo = "BA123";
			agentMawb.Validation.ValidateCM_FlightNo();
			AssertNoMessageErrorContaining(agentMawb.CM_FlightNoInfo, "mandatory");
			AssertNoMessageErrorContaining(agentMawb.CM_FlightNoInfo, "5 characters");
		}

		public void TestCheckCM_MAWBWhenChangingSerialNumber()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "12345678901";
			cusMawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			Factory.Save();
			cusMawb.CM_MAWB = "12345678902";

			AssertHasMessageErrorContaining(cusMawb.CM_MAWBInfo, "You should first check whether you need to delete the record from CCS-UK and recreate it with the new serial number. As a controller, you may proceed with caution.");  // blue for CW1 support etc
			var originalLogin = GlbStaff.CurrentUser.GS_LoginName;
			var originalController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "DJC";
				GlbStaff.CurrentUser.GS_IsController = false;
				cusMawb.Validation.ValidateCM_MAWB();
				AssertHasErrorContaining(cusMawb.CM_MAWBInfo, "You should first check whether you need to delete the record from CCS-UK and recreate it with the new serial number. Only your administrator can save this change."); // red error for Joe User
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = originalLogin;
				GlbStaff.CurrentUser.GS_IsController = originalController;
			}
		}

		public void TestWeight()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Weight = 69m;
			AssertNoMessageErrorContaining(cusMawb.WeightInfo, "Weight");
			cusMawb.Weight = 0m;
			AssertHasMessageErrorContaining(cusMawb.WeightInfo, "Weight");
		}

		public void TestGoodsDescription()
		{
			var basic = Factory.New<CusMAWB>();
			CusHawbValidationTests.DescriptionOfGoodsTestRunner(basic.DescriptionOfGoodsInfo);
			var mawb = Factory.New<CusMAWB>();
			mawb.ChildBills.AddNew();
			CusHawbValidationTests.DescriptionOfGoodsTestRunner(mawb.DescriptionOfGoodsInfo, false);
		}

		public void TestCheckCM_RL_NKDischargePort()
		{
			PortTest.CreatePort(Factory, "GB", "LHR", "Heathrow", portType: "DES");
			PortTest.CreatePort(Factory, "GB", "STN", "LONDON STANSTED AIRPORT", portType: "COA");
			Factory.Save();

			var cusMawb = Factory.New<CusMAWB>();
			RunTestPort(cusMawb.CM_RL_NKDischargePortInfo, "GBLHR", "AUSYD", "X");
			cusMawb.CM_RL_NKDischargePort = "LON";
			AssertHasErrorContaining(cusMawb.CM_RL_NKDischargePortInfo, "LON is not acceptable");
			cusMawb.CM_RL_NKDischargePort = "LHR";
			AssertNoErrorContaining(cusMawb.CM_RL_NKDischargePortInfo, "LON is not acceptable");
			cusMawb.CM_RL_NKDischargePort = "STN";
			AssertNoMessageErrorContaining(cusMawb.CM_RL_NKDischargePortInfo, "valid airport");
			cusMawb.CM_RL_NKDischargePort = "LSA";
			AssertHasMessageErrorContaining(cusMawb.CM_RL_NKDischargePortInfo, "valid airport");
		}

		public void TestCheckCM_RL_NKLoadPort()
		{
			PortTest.CreatePort(Factory, "GB", "LHR", "Heathrow", portType: "DES");
			Factory.Save();

			var cusMawb = Factory.New<CusMAWB>();
			RunTestPort(cusMawb.CM_RL_NKLoadPortInfo, "AUSYD", "GBLHR", "X");
		}

		public void TestCheckCM_RL_NKFirstArrivalPort()
		{
			PortTest.CreatePort(Factory, "GB", "LHR", "Heathrow", portType: "DES");
			PortTest.CreatePort(Factory, "GB", "STN", "LONDON STANSTED AIRPORT", portType: "COA");
			Factory.Save();

			var cusMawb = Factory.New<CusMAWB>();
			RunTestPort(cusMawb.CM_RL_NKFirstArrivalPortInfo, "GBLHR", "AUSYD", "X");
			cusMawb.CM_RL_NKFirstArrivalPort = "STN";
			AssertNoMessageErrorContaining(cusMawb.CM_RL_NKFirstArrivalPortInfo, "valid airport");
			cusMawb.CM_RL_NKFirstArrivalPort = "LSA";
			AssertHasMessageErrorContaining(cusMawb.CM_RL_NKFirstArrivalPortInfo, "valid airport");
		}

		public static void RunTestPort(ZPropertyInfo propInfo, ZString allowedRLCode, ZString disallowedRLCode, ZString invalidRLCode)
		{
			propInfo.SetValueFromString(allowedRLCode);
			AssertNoNotifications(propInfo);
			propInfo.SetValueFromString(ZString.Empty);
			AssertHasNotifications("You have not entered a", propInfo);

			propInfo.SetValueFromString(allowedRLCode);
			AssertNoNotifications(propInfo);

			propInfo.SetValueFromString(disallowedRLCode);
			AssertHasNotifications("Please enter a valid airport", propInfo);

			propInfo.SetValueFromString(allowedRLCode);
			AssertNoNotifications(propInfo);
			propInfo.SetValueFromString(invalidRLCode);
			AssertHasNotifications("Please enter a valid airport", propInfo);
		}

		public void TestCheckCM_ArrivalDateAndNPR_ShedAndAgentNew()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var cusMawbShed = Factory.New<CusMAWB>();

			cusMawbShed.Profile = "CUKAIR98LHRABC"; //shed
			cusMawbShed.CM_ArrivalDate = ZDateTime.Empty;
			cusMawbShed.NumberOfPiecesReceived = 0;
			AssertNoMessageErrorContaining(cusMawbShed.CM_ArrivalDateInfo, "(NPR) is present");
			cusMawbShed.NumberOfPiecesReceived = 69;
			cusMawbShed.Validation.ValidateCM_ArrivalDate();
			AssertHasMessageErrorContaining(cusMawbShed.CM_ArrivalDateInfo, "(NPR) is present");
			cusMawbShed.CM_ArrivalDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(cusMawbShed.CM_ArrivalDateInfo, "(NPR) is present");

			var cusMawbAgent = Factory.New<CusMAWB>();
			cusMawbAgent.Profile = "CUKFFW98000LXA"; //agent 
			cusMawbAgent.CM_ArrivalDate = ZDateTime.Empty;
			cusMawbAgent.NumberOfPiecesReceived = 0;
			AssertNoMessageErrorContaining(cusMawbAgent.CM_ArrivalDateInfo, "(NPR) is present");
			cusMawbAgent.NumberOfPiecesReceived = 69;
			cusMawbAgent.Validation.ValidateCM_ArrivalDate();
			AssertNoMessageErrorContaining(cusMawbAgent.CM_ArrivalDateInfo, "(NPR) is present");  //Agent not encouraged to supply NPR
			cusMawbAgent.CM_ArrivalDate = ZDateTime.Now;
			AssertHasErrorContaining(cusMawbAgent.CM_ArrivalDateInfo, "shed");
			cusMawbAgent.CM_ArrivalDate = ZDateTime.Empty;
			AssertNoErrorContaining(cusMawbAgent.CM_ArrivalDateInfo, "shed");
		}

		public void TestCheckCM_ArrivalDateAndNPR_AgentExisting()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			var secondFactory = new BusinessObjectFactory();
			var cusMawb = secondFactory.New<CusMAWB>();
			cusMawb.Profile = "CUKFFW98000LXA"; //agent 
			cusMawb.CM_ArrivalDate = ZDateTime.Now;
			cusMawb.NumberOfPiecesReceived = 69;
			secondFactory.Save();

			var cusMawbReloaded = Factory.Load<CusMAWB>(cusMawb.PK);
			cusMawbReloaded.Validation.ValidateCM_ArrivalDate();
			AssertNoErrorContaining(cusMawbReloaded.CM_ArrivalDateInfo, "shed");
			cusMawbReloaded.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			AssertHasErrorContaining(cusMawbReloaded.CM_ArrivalDateInfo, "shed");
		}

		public void TestWarnIfMawbNumberNotUniqueWithin12Months()
		{
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_ArrivalDate = ZDateTime.Now.AddMonths(-11);
			mawb1.CM_MAWB = "12345678905";
			mawb1.CargoTerminalOperatorAirport = "LHR";
			mawb1.CargoTerminalOperator = "BAC";
			Factory.Save();

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = mawb1.CM_MAWB;
			mawb2.CargoTerminalOperatorAirport = "LHR";
			mawb2.CargoTerminalOperator = "SLS";
			AssertHasWarningContaining(mawb2.CM_MAWBInfo, "already exists");
			mawb2.CargoTerminalOperator = "BAC";
			AssertHasMessageErrorContaining(mawb2.CM_MAWBInfo, "already exists");

			mawb2.CargoTerminalOperator = "SLS";
			var mawb3 = Factory.New<CusMAWB>();
			mawb3.CM_MAWB = mawb1.CM_MAWB;
			mawb3.CargoTerminalOperatorAirport = "LHR";
			mawb3.CargoTerminalOperator = "DKR";
			AssertHasWarningContaining(mawb3.CM_MAWBInfo, "already exists");
			mawb3.CargoTerminalOperator = "SLS";
			AssertHasMessageErrorContaining(mawb3.CM_MAWBInfo, "already exists");
			mawb2.Validation.ValidateCM_MAWB();
			AssertHasMessageErrorContaining(mawb2.CM_MAWBInfo, "already exists");

			mawb1.CM_ArrivalDate = ZDateTime.Now.AddMonths(-13);
			mawb3.CM_MAWB = "22345678905";
			Factory.Save();
			mawb2.Validation.ValidateCM_MAWB();
			AssertNoMessageErrorContaining(mawb2.CM_MAWBInfo, "already exists");
			AssertNoWarningContaining(mawb2.CM_MAWBInfo, "already exists");
			mawb1.CM_ArrivalDate = ZDateTime.Now.AddMonths(-5);
			mawb1.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Factory.Save();
			AssertNoMessageErrorContaining(mawb2.CM_MAWBInfo, "already exists");
			AssertNoWarningContaining(mawb2.CM_MAWBInfo, "already exists");

			var consol = Factory.New<ForwardingConsol>();
			mawb1.CM_JK = consol.PK;
			mawb1.CM_ArrivalDate = ZDateTime.Now.AddMonths(-11);
			mawb1.CM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.GbCcsuk;
			consol.JK_UniqueConsignRef = "Daniel";
			Factory.Save();
			mawb2.Validation.ValidateCM_MAWB();
			AssertHasWarningContaining(mawb2.CM_MAWBInfo, "Daniel");
		}

		public void TestChildEditable()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.AgentBadge = "YYY";
			mawb.Factory.Save();
			AssertEquals(false, mawb.HasChanges);
			mawb.AgentBadge = "XXX";
			AssertEquals(true, mawb.HasChanges);
		}

		public void TestArePortsInEU()
		{
			ZString gbPort = Core.Constants.CountryCodes.UnitedKingdom + "LHR";
			ZString euPort = Core.Constants.CountryCodes.France + "PAR";
			var xiPort = CusMawbTests.GetNorthernIrelandPort(Factory);

			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = "";
			var frCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			frCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			Factory.Save();

			AssertEquals("EU->XI both in EU", CusMAWBValidation.PortsInEU.BothInEU, CusMAWBValidation.ArePortsInEU(euPort, xiPort, Factory));
			AssertEquals("EU->GB origin in EU", CusMAWBValidation.PortsInEU.OriginInEU, CusMAWBValidation.ArePortsInEU(euPort, gbPort, Factory));
			AssertEquals("GB->EU origin not in EU", CusMAWBValidation.PortsInEU.OriginNotInEU, CusMAWBValidation.ArePortsInEU(gbPort, euPort, Factory));
		}
	}

	class CusMAWBReturnsNullOutturnsForTest : CusMAWB
	{
		public CusMAWBReturnsNullOutturnsForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusOutTurnCollection OutTurns => null;
	}

	[TestedType(typeof(CusMAWBCollection))]
	class CusMAWBCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusMAWBCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusMAWB>();
		}
	}
}
