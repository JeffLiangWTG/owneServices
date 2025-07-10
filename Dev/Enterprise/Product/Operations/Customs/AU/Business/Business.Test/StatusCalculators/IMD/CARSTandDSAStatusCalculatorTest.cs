using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CARSTandDSAStatusCalculator))]
	sealed class CARSTandDSAStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			var calculator = GetNewBusinessObject() as CARSTandDSAStatusCalculator;
			AssertEquals("StatusInfo.Name", "CR_CargoStatus", calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			var calculator = GetNewBusinessObject() as CARSTandDSAStatusCalculator;
			AssertEquals("InterestedMessageTypes.Length", 2, calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.CARST, calculator.InterestedMessageTypes[0]);
			AssertEquals("InterestedMessageTypes[1]", CMRMessage.CMRMessageTypes.DSA, calculator.InterestedMessageTypes[1]);
		}

		public void TestCargoStatusCalculatorLikeStatus()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_DeclarationReference = "B00122382";
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;

			var masterBill = testJobDeclaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill";

			var houseBill = testJobDeclaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "Master Bill";
			houseBill.CU_HouseBill = "House Bill";
			testJobDeclaration.JE_TotalNoOfPacks = 150;

			var entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			var message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220052'
UNT+34+000001'".Replace("\r\n", "");

			var message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220053'
UNT+34+000001'".Replace("\r\n", "");

			var packGroup1 = testJobDeclaration.PackingGroups[0];
			packGroup1.Messages.Add(message1);

			var packGroup2 = testJobDeclaration.PackingGroups.AddNew();
			packGroup2.Messages.Add(message2);

			_ = packGroup1.Packages.AddNew();
			_ = packGroup2.Packages.AddNew();
			packGroup1.CR_CU_HouseBill = houseBill.PK;
			packGroup2.CR_CU_HouseBill = houseBill.PK;

			Assert("Pre-condition, Entry header is subject to redline", entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
			AssertEquals("Pre-condition, Entry Status is Held", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Pre-condition, Declaration Entry Status is Held", CMRImportEntryAdvice.Held.Code, testJobDeclaration.JE_EntryStatus);

			var calculator1 = new CARSTandDSAStatusCalculatorForTest(packGroup1);
			calculator1.DeriveStatus();
			Assert("Entry header is still subject to redline", entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
			AssertEquals("Entry Status is still Held", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status is still Held", CMRImportEntryAdvice.Held.Code, testJobDeclaration.JE_EntryStatus);

			var calculator2 = new CARSTandDSAStatusCalculatorForTest(packGroup2);
			calculator2.DeriveStatus();
			AssertEquals("Pack1 Cargo Status", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, packGroup1.CR_CargoStatus);
			AssertEquals("Pack2 Cargo Status", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, packGroup2.CR_CargoStatus);
			AssertEquals("Declaration Cargo Status", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, testJobDeclaration.JE_ConsolidatedCargoStatus);
			Assert("Entry header is no longer subject to redlien", !entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
			AssertEquals("Entry Status is still Held", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status is still Held", CMRImportEntryAdvice.Held.Code, testJobDeclaration.JE_EntryStatus);

			var aTDMessage = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			aTDMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'" +
				"TDT+20++6'LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
				"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:TTTTTTTT'RFF+AAE:N10'" +
				"DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'CNT+5:1'CNT+2:1'CNT+3:0'" +
				"UNT+30+000001'UNZ+1+00000000273283'";

			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			calculator2.DeriveStatus();
			Assert("Entry header is no longer subject to redlien", !entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
			AssertEquals("Entry Status should now be ATD", CMRImportEntryAdvice.ATDReceived.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status should now be ATD", CMRImportEntryAdvice.ATDReceived.Code, testJobDeclaration.JE_EntryStatus);
		}

		public void TestCargoStatusCalculatorWithExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B00122382";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "Master Bill";
			houseBill.CU_HouseBill = "House Bill";
			declaration.JE_TotalNoOfPacks = 150;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			var packingGroup = declaration.PackingGroups.AddNew();

			var package = packingGroup.Packages.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;

			var message = entryHeader.Messages.AddNew(typeof(CMRATDMessage));
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'" +
				"TDT+20++6'LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
				"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:TTTTTTTT'RFF+AAE:N10'" +
				"DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'CNT+5:1'CNT+2:1'CNT+3:0'" +
				"UNT+30+000001'UNZ+1+00000000273283'";

			var calculator = new CARSTandDSAStatusCalculatorForTest(packingGroup);
			AssertNoExceptionThrown("Should not throw any exceptions when the CustomsEntryHeaders is loading.", () => calculator.DeriveStatus());
		}

		public void TestCargoStatusCalculatorMixedStatus()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_DeclarationReference = "B00122382";
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			var masterBill = testJobDeclaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill";
			var houseBill = testJobDeclaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "Master Bill";
			houseBill.CU_HouseBill = "House Bill";
			testJobDeclaration.JE_TotalNoOfPacks = 150;
			var entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			var message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++DECONSOLIDATION UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4XMFL'
FTX+AHN+++ACS EVALUATION COMPLETE:NO'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220052'
UNT+34+000001'".Replace("\r\n", "");
			var message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:L3020041H0001::1'
RFF+MWB:08122220053'
UNT+34+000001'".Replace("\r\n", "");

			var packGroup1 = testJobDeclaration.PackingGroups[0];
			packGroup1.Messages.Add(message1);
			var packGroup2 = testJobDeclaration.PackingGroups.AddNew();
			packGroup2.Messages.Add(message2);
			_ = packGroup1.Packages.AddNew();
			_ = packGroup2.Packages.AddNew();
			packGroup1.CR_CU_HouseBill = houseBill.PK;
			packGroup2.CR_CU_HouseBill = houseBill.PK;

			var calculator1 = new CARSTandDSAStatusCalculatorForTest(packGroup1);
			calculator1.DeriveStatus();
			var calculator2 = new CARSTandDSAStatusCalculatorForTest(packGroup2);

			calculator2.DeriveStatus();
			AssertEquals("Pack1 Cargo Status", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, packGroup1.CR_CargoStatus);
			AssertEquals("Pack2 Cargo Status", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, packGroup2.CR_CargoStatus);
			AssertEquals("Declaration Cargo Status", CMRConsolidatedCargoStatuses.Codes.SeePackingDetails, testJobDeclaration.JE_ConsolidatedCargoStatus);
			Assert("Entry header is still subject to redline", entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden);
			AssertEquals("Entry Status is still Held", CMRImportEntryAdvice.Held.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration Entry Status is still Held", CMRImportEntryAdvice.Held.Code, testJobDeclaration.JE_EntryStatus);
		}

		public void TestStatusCalculationFromDSA()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_DeclarationReference = "B00122382";
			testJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			var masterBill = testJobDeclaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill";
			var houseBill = testJobDeclaration.Bills.AddNew();
			houseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "Master Bill";
			houseBill.CU_HouseBill = "House Bill";
			testJobDeclaration.JE_TotalNoOfPacks = 150;
			var entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			entryHeader.AddInfo.ZA_IsSubjectToRedLine_Hidden = true;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;

			var message1 = Factory.New<CMRDSAMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+31C8 FJB0 JEAG:1+11'DTM+9:20160705153100000000:ZZZ'TDT+20++S'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+WISETECH GLOBAL LIMITED'NAD+IM++MS AMY TEST'RFF+ABO:B40007229/1/CMT1::1'RFF+ACW:FID'RFF+AAE:N10'RFF+ABT:AAAC97N3M::1'RFF+ABQ:DSA23'RFF+ADU:B00166653/1'RFF+AMI:CLEAR'DOC+S+1'DTM+192:20160705:102'DTM+192:1555:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'CST+1'FTX+AHN+++SCREENING PERIOD EXPIRED:NO'CST+1'FTX+AHN+++ACS EVALUATION COMPLETE:NO'CST+1'FTX+AHN+++IMPORT DECLARATION PAID:NO'UNT+30+000001'".Replace("\r\n", "");
			entryHeader.Messages.Add(message1);

			var packGroup1 = testJobDeclaration.PackingGroups[0];
			packGroup1.CR_HouseContainerNumber = 1;
			var calculator1 = new CARSTandDSAStatusCalculatorForTest(packGroup1);
			calculator1.DeriveStatus();
			AssertEquals("Pack1 Cargo Status", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, packGroup1.CR_CargoStatus);
		}

		protected override BusinessObject GetNewBusinessObject() => new CARSTandDSAStatusCalculator(Factory.New<PackingGroup>());

		internal class CARSTandDSAStatusCalculatorForTest : CARSTandDSAStatusCalculator
		{
			public CARSTandDSAStatusCalculatorForTest(PackingGroup pivot) : base(pivot)
			{
			}

			internal new void DeriveStatus() => base.DeriveStatus();
		}
	}
}
