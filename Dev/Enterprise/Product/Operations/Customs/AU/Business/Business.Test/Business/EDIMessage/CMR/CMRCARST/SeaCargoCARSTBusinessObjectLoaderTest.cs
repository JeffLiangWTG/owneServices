using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class SeaCargoCARSTBusinessObjectLoaderTest : TestCaseWithFactory
	{
		public void TestMatchHouseBillWithSenderReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var oceanBillInOtherCountry = (BusinessObject)Factory.New<Integration.Customs.NZ.ICusSCAOceanBill>();
				oceanBillInOtherCountry.FillWithValidTestData();

				var houseBillInOtherCountry = (BaseCusSCAHouse)Factory.New<Integration.Customs.NZ.ICusSCAHouse>();
				houseBillInOtherCountry.CA_CB = oceanBillInOtherCountry.PK;
				houseBillInOtherCountry.CA_BGMReference = "L3020037H0004";
				houseBillInOtherCountry.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			}

			var oceanBill = CreateTestSeaCargoRecords();

			var houseBill1 = oceanBill.HouseBills[0];
			houseBill1.CA_BGMReference = string.Empty;
			houseBill1.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			AssertEquals("Precondition", "L3020037H0004", SeaCARSTHouseBillContainer.CARSTSendersReference);

			using (AUCustomsDataRegistry.Instance.UseSenderReferenceToFilterSeaCargoReport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var matchedHouseBill = Creator.LoadOrCreateRecordForMessageCore(SeaCARSTHouseBillContainer).OfType<BaseCusSCAHouse>().FirstOrDefault();
				AssertNull("Should be null as the CA_BGMReference is empty.", matchedHouseBill);

				houseBill1.CA_BGMReference = "L3020037H0004";

				var matchedPivot = Creator.LoadOrCreateRecordForMessageCore(SeaCARSTHouseBillContainer).OfType<CusSCAPivot>().Single();
				AssertSame("Should find the first pivot of house bill 1 as the CA_BGMReference is equal to the SendersReference of message.", houseBill1.Pivot[0], matchedPivot);
			}

			using (AUCustomsDataRegistry.Instance.UseSenderReferenceToFilterSeaCargoReport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var matchedHouseBill = Creator.LoadOrCreateRecordForMessageCore(SeaCARSTHouseBillContainer).OfType<BaseCusSCAHouse>().FirstOrDefault();
				AssertNull("Should be null as the value of UseSenderReferenceToFilterSeaCargoReport is disable.", matchedHouseBill);
			}

			using (AUCustomsDataRegistry.Instance.UseSenderReferenceToFilterSeaCargoReport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				houseBill1.CA_MessageStatus = string.Empty;

				var houseBill2 = oceanBill.HouseBills.AddNew();
				houseBill2.CA_HouseBill = houseBill1.CA_HouseBill;
				houseBill2.CA_BGMReference = "L3020037H0004";
				houseBill2.CA_MessageStatus = CMRBaseStatuses.Codes.NotSent;

				var houseBill3 = oceanBill.HouseBills.AddNew();
				houseBill3.CA_HouseBill = houseBill1.CA_HouseBill;
				houseBill3.CA_BGMReference = "L3020037H0004";
				houseBill3.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

				houseBill2.Messages.Add(Factory.New<CMRCARSTMessage>());
				houseBill3.Messages.Add(Factory.New<CMRCARSTMessage>());

				var matchedHouseBill = Creator.LoadOrCreateRecordForMessageCore(SeaCARSTHouseBillContainer).OfType<BaseCusSCAHouse>().Single();
				AssertSame("Should find the third house bill as the CA_MessageStatus is not empty and NOT.", houseBill3, matchedHouseBill);
			}
		}

		public void TestMatchHouseBillWithoutAPivotMatch()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			ocean.CB_OceanBill = "OB0987123";
			ocean.CB_LloydsIMO = "8811924";
			ocean.CB_Voyage = " 0436s";
			house.CA_HouseBill = "HB4000";
			house.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			house.Messages.Add(Factory.New<CMRCARSTMessage>());

			var record = Creator.LoadOrCreateRecordForMessageCore(SeaMasterAndHouseCARSTMessage).Single();

			AssertSame("House will be matched since there is no pivot match and house has CARST messages", house, record);
		}

		public void TestIsInterestedInCARST()
		{
			CreateTestSeaCargoRecords();
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(SeaMasterAndHouseCARSTMessage));
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(SeaMasterCARSTMessage));
			AssertEquals("IsInterestedInCARST", true, Creator.IsInterestedInCARST(SeaCARSTHouseBillContainer));
			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(AirMasterCARSTMessage));
			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(AirMasterAndHouseCARSTMessage));

			AssertEquals("IsInterestedInCARST", false, Creator.IsInterestedInCARST(InvalidCARSTNoMAWB));
		}

		public void TestLoadOrCreateRecordForMessageCoreWhenNoRecord()
		{
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(SeaMasterAndHouseCARSTMessage)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(SeaMasterCARSTMessage)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(AirMasterCARSTMessage)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(AirMasterAndHouseCARSTMessage)[0]);

			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(SeaCARSTHouseBillContainer)[0]);
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(InvalidCARSTNoMAWB)[0]);
		}

		#region Implementation

		protected CusSCAOceanBill CreateTestSeaCargoRecords()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB0987123";
			oceanBill.CB_LloydsIMO = "8811924";
			oceanBill.CB_Voyage = " 0436s";

			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "HB4000";
			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "C001";
			container1.Pivots.Add(houseBill.Pivot.AddNew());

			var container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			container2.Pivots.Add(houseBill.Pivot.AddNew());

			return oceanBill;
		}

		SeaCargoCARSTBusinessObjectLoaderForTest Creator => fCreator ?? (fCreator = new SeaCargoCARSTBusinessObjectLoaderForTest());
		SeaCargoCARSTBusinessObjectLoaderForTest fCreator;

		class SeaCargoCARSTBusinessObjectLoaderForTest : SeaCargoCARSTBusinessObjectLoader
		{
			public new bool IsInterestedInCARST(CMRCARSTMessage message)
			{
				return base.IsInterestedInCARST(message);
			}

			public new BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
			{
				return base.LoadOrCreateRecordForMessageCore(message);
			}
		}

		#region Sea CARST Messages

		CMRCARSTMessage SeaMasterAndHouseCARSTMessage
		{
			get
			{
				if (fSeaMasterAndHouseCARSTMessage == null)
				{
					fSeaMasterAndHouseCARSTMessage = Factory.New<CMRCARSTMessage>();
					fSeaMasterAndHouseCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
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
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+436S++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
				}
				return fSeaMasterAndHouseCARSTMessage;
			}
		}

		CMRCARSTMessage SeaMasterCARSTMessage
		{
			get
			{
				if (fSeaMasterCARSTMessage == null)
				{
					fSeaMasterCARSTMessage = Factory.New<CMRCARSTMessage>();
					fSeaMasterCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
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
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+436S++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB0987123'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+33+000001'".Replace("\r\n", "");
				}
				return fSeaMasterCARSTMessage;
			}
		}

		CMRCARSTMessage fSeaMasterAndHouseCARSTMessage;
		CMRCARSTMessage fSeaMasterCARSTMessage;

		#endregion

		#region Air CARST Messages

		CMRCARSTMessage AirMasterCARSTMessage
		{
			get
			{
				if (fAirMasterCARSTMessage == null)
				{
					fAirMasterCARSTMessage = Factory.New<CMRCARSTMessage>();
					fAirMasterCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+MWB:08112347775'
DOC+1'
PAC+0000200'
UNT+16+000001'".Replace("\r\n", "");
				}
				return fAirMasterCARSTMessage;
			}
		}

		CMRCARSTMessage AirMasterAndHouseCARSTMessage
		{
			get
			{
				if (fAirMasterAndHouseCARSTMessage == null)
				{
					fAirMasterAndHouseCARSTMessage = Factory.New<CMRCARSTMessage>();
					fAirMasterAndHouseCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+MWB:08112347775'
RFF+HWB:AH3'
DOC+1'
PAC+0000200'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fAirMasterAndHouseCARSTMessage;
			}
		}

		CMRCARSTMessage fAirMasterCARSTMessage;
		CMRCARSTMessage fAirMasterAndHouseCARSTMessage;

		#endregion

		#region Invalid CARST

		CMRCARSTMessage InvalidCARSTNoMAWB
		{
			get
			{
				if (fInvalidCARSTNoMAWB == null)
				{
					fInvalidCARSTNoMAWB = Factory.New<CMRCARSTMessage>();
					fInvalidCARSTNoMAWB.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+HWB:08112347775'
DOC+1'
PAC+0000200'
UNT+16+000001'".Replace("\r\n", "");
				}
				return fInvalidCARSTNoMAWB;
			}
		}

		CMRCARSTMessage fInvalidCARSTNoMAWB;

		CMRCARSTMessage SeaCARSTHouseBillContainer
		{
			get
			{
				if (fSeaCARSTHouseBillContainer == null)
				{
					fSeaCARSTHouseBillContainer = Factory.New<CMRCARSTMessage>();
					fSeaCARSTHouseBillContainer.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
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
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+436S++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
				}
				return fSeaCARSTHouseBillContainer;
			}
		}

		CMRCARSTMessage fSeaCARSTHouseBillContainer;

		#endregion

		#endregion
	}
}
