using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(PackingGroup))]
	class PackingGroupTest : Customs.Business.Testing.BasePackingGroupTest
	{
		public void TestWarehouseNumberOfPacksAndOuterPacks()
		{
			CreateDeclaration();
			var package1 = packingGroup.Packages.AddNew();
			var package2 = packingGroup.Packages.AddNew();
			package1.CW_InBondPackQty = 2;
			package2.CW_InBondPackQty = 7;
			package1.CW_OuterPacks = 3;
			package2.CW_OuterPacks = 1;
			AssertEquals(9, packingGroup.WarehouseNumberOfPackages);
			AssertEquals(4, packingGroup.OuterPackingUnitCount);
		}

		public void TestAddTotalOuterPackageIfRequired()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			packingGroup = declaration.PackingGroups.AddNew();
			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 0;

			packingGroup.AddTotalOuterPackageIfRequired(50);
			AssertEquals(50, packingGroup.TotalPackageCount());

			packingGroup.AddTotalOuterPackageIfRequired(100);
			AssertEquals(50, packingGroup.TotalPackageCount());

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			packingGroup.AddTotalOuterPackageIfRequired(100);
			AssertEquals(100, packingGroup.TotalPackageCount());
		}

		public void TestAddTotalOuterPackageIfRequired_InBond()
		{
			declaration = Factory.New<JobDeclaration>();
			packingGroup = declaration.PackingGroups.AddNew();
			var package = packingGroup.Packages.AddNew();
			package.CW_InBondPackQty = 2;

			packingGroup.AddTotalOuterPackageIfRequired(50);
			AssertEquals(2, packingGroup.WarehouseNumberOfPackages);
			AssertEquals(0, packingGroup.TotalPackageCount());
		}

		public void TestContainerMode()
		{
			CreateDeclaration();
			packingGroup.CR_CO_Container = ZGuid.Empty;
			AssertEquals("ContainerMode", true, ((IPackingGroup)packingGroup).ContainerMode.IsEmpty);

			packingGroup.CR_CO_Container = cusContainer.PK;
			cusContainer.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("ContainerMode", cusContainer.CO_FCL_LCL_AIR, ((IPackingGroup)packingGroup).ContainerMode);
		}

		public void TestContainerNumber()
		{
			CreateDeclaration();
			packingGroup.CR_CO_Container = ZGuid.Empty;
			AssertEquals("ContainerMode", true, ((IPackingGroup)packingGroup).ContainerNumber.IsEmpty);

			packingGroup.CR_CO_Container = cusContainer.PK;
			AssertEquals("ContainerMode", "CONTAINERNUM", ((IPackingGroup)packingGroup).ContainerNumber);
		}

		public void TestHouseBillNumber()
		{
			CreateDeclaration();
			packingGroup.CR_CU_HouseBill = ZGuid.Empty;
			AssertEquals("House Bill", true, ((IPackingGroup)packingGroup).HouseBillNumber.IsEmpty);

			packingGroup.CR_CU_HouseBill = houseBillBizObj.PK;
			AssertEquals("House Bill", ((IPackingGroup)packingGroup).HouseBillNumber);
		}

		public void TestMasterBillNumber()
		{
			CreateDeclaration();
			packingGroup.CR_CU_HouseBill = ZGuid.Empty;
			AssertEquals("Master Bill", true, ((IPackingGroup)packingGroup).MasterBillNumber.IsEmpty);

			packingGroup.CR_CU_HouseBill = houseBillBizObj.PK;
			var masterBill = houseBillBizObj.Declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "Master Bill";
			houseBillBizObj.CU_MasterBill = "Master Bill";
			AssertEquals("Master Bill", ((IPackingGroup)packingGroup).MasterBillNumber);
		}

		public void TestConsignRefNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_fPartShipConsignmentReference = "B001";

			var packs = bill.PackingGroups.AddNew();
			AssertEquals("B001", ((IPackingGroup)packs).ConsignRefNumber);
		}

		protected void CreateDeclaration()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			houseBillBizObj = declaration.Bills.AddNew();
			houseBillBizObj.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBillBizObj.CU_HouseBill = "House Bill";

			cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "ContainerNum";

			packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = cusContainer.PK;
			packingGroup.CR_CU_HouseBill = houseBillBizObj.PK;
		}

		CusContainer cusContainer;
		Bill houseBillBizObj;
		JobDeclaration declaration;
		PackingGroup packingGroup;

		public void TestActionCodeForHouseBillContainerPivotForMessage()
		{
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			IPackingGroup packInterface = packingGroup;
			AssertEquals("Insert", LineAction.Insert, packInterface.ActionCodeForMessage(entryHeader));
			entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = 2;
			AssertEquals("Amend", LineAction.Amend, packInterface.ActionCodeForMessage(entryHeader));
		}

		public void TestActionCodeInvalidActionException()
		{
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = 1;
			IPackingGroup packInterface = packingGroup;

			AssertEquals("Line action should be Amend", LineAction.Amend, packInterface.ActionCodeForMessage(entryHeader));
			ErrorReporter.Clear();
		}

		public void TestMarksAndNumber()
		{
			CreateDeclaration();
			var pack1 = packingGroup.Packages.AddNew();
			pack1.CW_MarksAndNos = "comrade";
			var pack2 = packingGroup.Packages.AddNew();
			pack2.CW_MarksAndNos = "noodle";
			AssertEquals("comrade\r\nnoodle", packingGroup.MarksAndNumbers);
		}

		public void TestMostRecentCARSTOrDSAMessageIgnoresOtherMessageTypes()
		{
			// CARST messages are on PackGroup, DSA messages are no longer cloned onto the PackGroup but use the original DSA message on the EntryHeader
			var testPackGroup = declaration.PackingGroups.AddNew();
			var pack1 = testPackGroup.Packages.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var message1 = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message1);
			message1.EM_MessageNum = "3";
			var message2 = Factory.New<CMRCARSTMessage>();
			testPackGroup.Messages.Add(message2);
			message2.EM_MessageNum = "4";
			var message3 = entryHeader.Messages.AddNew();
			message3.EM_MessageNum = "5";
			message3.EM_MessageType = CMRMessage.CMRMessageTypes.SAM;
			var message4 = Factory.New<CMRCARSTMessage>();
			testPackGroup.Messages.Add(message4);
			message4.EM_MessageNum = "1";
			var message5 = Factory.New<CMRCARSTMessage>();
			testPackGroup.Messages.Add(message5);
			message5.EM_MessageNum = "2";
			AssertEquals("Most recent is a CARST", message2.PK, testPackGroup.MostRecentCARSTorDSAMessage.PK);
		}

		public void TestMostRecentCARSTOrDSAMessageDoesNotIgnoreDSA()
		{
			// CARST messages are on PackGroup, DSA messages are no longer cloned onto the PackGroup but use the original DSA message on the EntryHeader
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			var message1 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message1);
			message1.EM_MessageNum = "3";
			var message2 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message2);
			message2.EM_MessageNum = "4";
			var message3 = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message3);
			message3.EM_MessageNum = "5";
			var message4 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message4);
			message4.EM_MessageNum = "1";
			var message5 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message5);
			message5.EM_MessageNum = "2";
			AssertEquals("Most recent is a DSA", message3.PK, packingGroup.MostRecentCARSTorDSAMessage.PK);
		}

		public void TestMostRecentCARSTOrDSAMessageSelectsDSAandIgnoresSAM()
		{
			// CARST messages are on PackGroup, DSA messages are no longer cloned onto the PackGroup but use the original DSA message on the EntryHeader
			packingGroup.CR_HouseContainerNumber = 1;
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			var message1 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message1);
			message1.EM_MessageNum = "3";
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			var message2 = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message2);
			message2.EM_MessageNum = "4";
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.DSA;
			var message3 = packingGroup.Messages.AddNew();
			message3.EM_MessageNum = "5";
			message3.EM_MessageType = CMRMessage.CMRMessageTypes.SAM;
			var message4 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message4);
			message4.EM_MessageNum = "1";
			message4.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			var message5 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message1);
			message5.EM_MessageNum = "2";
			message5.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			AssertEquals("Most recent is a DSA", message2.PK, packingGroup.MostRecentCARSTorDSAMessage.PK);
		}

		public void TestGetCargoStatusFromLatestMessageForCARST()
		{
			var message1 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message1);
			message1.EM_MessageNum = "1";
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message1.EM_MessageText = "UNH+000002+CUSRES:D:99B:UN'BGM+34:::CARST+1JA0 B149 8I68:1+8'DTM+9:20080818113851494730:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'UNT+35+000002'";
			AssertEquals("GetCargoStatusFromLatestMessage", "HELD", packingGroup.GetCargoStatusFromLatestMessage());
		}

		public void TestGetCargoStatusFromLatestMessageForDSA()
		{
			// DSA messages are no longer cloned onto the PackGroup but use the original DSA message on the EntryHeader
			packingGroup.CR_HouseContainerNumber = 2;
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			EDIMessage message1 = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message1);
			message1.EM_MessageNum = "1";
			message1.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+4EH6 2759 1868:1+11'DTM+9:20080818154500000000:ZZZ'TDT+20++S'RFF+ABO:S00001181/1/DAT1::2'RFF+ABT:AAACG767S::2'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'DOC+S+2'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'UNT+44+000001'";
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.DSA;
			AssertEquals("GetCargoStatusFromLatestMessage", "CLEAR", packingGroup.GetCargoStatusFromLatestMessage());
		}

		public void TestConsolidatedCargoStatusDescriptionForCARST()
		{
			// CARST messages are on PackGroup, DSA messages are no longer cloned onto the PackingGroup but use the original DSA message on the EntryHeader
			packingGroup.CR_HouseContainerNumber = 1;
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			EDIMessage message1 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message1);
			message1.EM_MessageNum = "2";
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message1.EM_MessageText = "UNH+000002+CUSRES:D:99B:UN'BGM+34:::CARST+1JA0 B149 8I68:1+8'DTM+9:20080818113851494730:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'UNT+35+000002'";
			EDIMessage message2 = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message2);
			message2.EM_MessageNum = "1";
			message2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+4EH6 2759 1868:1+11'DTM+9:20080818154500000000:ZZZ'TDT+20++S'RFF+ABO:S00001181/1/DAT1::2'RFF+ABT:AAACG767S::2'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'DOC+S+2'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'UNT+44+000001'";
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.DSA;
			AssertEquals("Most recent is a CARST", message1.PK, packingGroup.MostRecentCARSTorDSAMessage.PK);
			var includeAdditionalDSAStatusSection = true;
			AssertMultilineASCIIEquals("CARST Status Description", expectedResult1, packingGroup.LineConsolidatedCargoStatusDescription(ref includeAdditionalDSAStatusSection));
		}
		readonly string expectedResult1 = @"***CONSOLIDATED CARGO STATUS: ***HELD***
ACSDec/ACSCR/AQISDec/AQISCR: N/N/N/N
Detailed Status Description:
	CONSOLIDATED STATUS: HELD";

		public void TestConsolidatedCargoStatusDescriptionForDSA()
		{
			// CARST messages are on PackGroup, DSA messages are no longer cloned onto the PackingGroup but use the original DSA message on the EntryHeader
			packingGroup.CR_HouseContainerNumber = 2;
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			EDIMessage message1 = Factory.New<CMRCARSTMessage>();
			packingGroup.Messages.Add(message1);
			message1.EM_MessageNum = "1";
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message1.EM_MessageText = "UNH+000002+CUSRES:D:99B:UN'BGM+34:::CARST+1JA0 B149 8I68:1+8'DTM+9:20080818113851494730:ZZZ'FTX+AHN+++CONSOLIDATED STATUS:HELD'UNT+35+000002'";
			EDIMessage message2 = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message2);
			message2.EM_MessageNum = "2";
			message2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+4EH6 2759 1868:1+11'DTM+9:20080818154500000000:ZZZ'TDT+20++S'RFF+ABO:S00001181/1/DAT1::2'RFF+ABT:AAACG767S::2'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'DOC+S+2'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'UNT+44+000001'";
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.DSA;
			AssertEquals("Most recent is a DSA", message2.PK, packingGroup.MostRecentCARSTorDSAMessage.PK);
			var includeAdditionalDSAStatusSection = true;
			AssertMultilineASCIIEquals("DSA Status Description", expectedResult2, packingGroup.LineConsolidatedCargoStatusDescription(ref includeAdditionalDSAStatusSection));
		}
		readonly string expectedResult2 = @"Status for Packing (Transport) Line: 2
***CONSOLIDATED CARGO STATUS: ***CLEAR***
ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y
HB:1
Screening Period Expiry: 18Aug2008 16:08 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: CLEAR";

		public void TestAbbreviatedCargoStatusDescriptionWhenNoCARSTorDSA()
		{
			var testPackGroup = declaration.PackingGroups.AddNew();
			testPackGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			AssertEquals("AbbreviatedCargoStatusDescription when no CARST or DSA", "ACSSEIZED", testPackGroup.AbbreviatedCargoStatusDescription);
		}

		public void TestAbbreviatedCargoStatusDescriptionWhenClearDSA()
		{
			// DSA messages are no longer cloned onto the PackingGroup but use the original DSA message on the EntryHeader
			packingGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			packingGroup.CR_HouseContainerNumber = 1;
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			EDIMessage message = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message);
			message.EM_MessageNum = "1";
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+4EH6 2759 1868:1+11'DTM+9:20080818154500000000:ZZZ'TDT+20++S'RFF+ABO:S00001181/1/DAT1::2'RFF+ABT:AAACG767S::2'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'DOC+S+2'DTM+192:20080818:102'DTM+192:1608:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'UNT+44+000001'";
			message.EM_MessageType = CMRMessage.CMRMessageTypes.DSA;
			AssertEquals("AbbreviatedCargoStatusDescription when Clear DSA", "CLEAR", packingGroup.AbbreviatedCargoStatusDescription);
		}

		public void TestAbbreviatedCargoStatusDescriptionWhenHeldDSA()
		{
			// DSA messages are no longer cloned onto the PackingGroup but use the original DSA message on the EntryHeader
			packingGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			packingGroup.CR_HouseContainerNumber = 1;
			var pack1 = packingGroup.Packages.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.ActiveEntryHeaders[0];
			EDIMessage message = Factory.New<CMRDSAMessage>();
			entryHeader.Messages.Add(message);
			message.EM_MessageNum = "1";
			message.EM_MessageText = CMRBaseMessageProcessorTest.DSAMessageText;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.DSA;
			AssertEquals("AbbreviatedCargoStatusDescription when Clear DSA", "HELD  Y/N/Y/Y", packingGroup.AbbreviatedCargoStatusDescription);
		}

		public void TestIsCargoClear()
		{
			var testPackGroup = declaration.PackingGroups.AddNew();
			testPackGroup.CR_CargoStatus = ZString.Empty;
			Assert(!testPackGroup.IsCargoStatusAvailableAndCargoClear);
			testPackGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			Assert(testPackGroup.IsCargoStatusAvailableAndCargoClear);
			testPackGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			Assert(testPackGroup.IsCargoStatusAvailableAndCargoClear);
			testPackGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			Assert(testPackGroup.IsCargoStatusAvailableAndCargoClear);
			testPackGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			Assert(!testPackGroup.IsCargoStatusAvailableAndCargoClear);
		}

		public new void TestNoExceptionWhenPackagesIsNull()
		{
			base.TestNoExceptionWhenPackagesIsNull();

			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.Bills.AddNew();
			var packingGroup = declaration.PackingGroups.AddNew();
			declaration.PackingGroups.Remove(packingGroup);
			packingGroup.Declaration = null;

			AssertNoExceptionThrown(() =>
			{
				AssertEquals(ZInt.Zero, packingGroup.WarehouseNumberOfPackages);
				AssertEquals(ZInt.Zero, packingGroup.OuterPackingUnitCount);
				AssertEquals(ZString.Empty, packingGroup.MarksAndNumbers);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration.JE_HouseBill = "1";
			var bill = declaration.Bills[0];
			var header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();

			packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_HouseContainerNumber = 1;
			packingGroup.CR_CU_HouseBill = bill.PK;
		}

		#endregion
	}
}
