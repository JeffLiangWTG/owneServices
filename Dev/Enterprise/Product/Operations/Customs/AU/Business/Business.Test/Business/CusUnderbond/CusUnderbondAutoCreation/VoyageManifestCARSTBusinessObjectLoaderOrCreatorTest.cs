using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class VoyageManifestCARSTBusinessObjectLoaderOrCreatorTest : TestCaseWithFactory
	{
		public void TestIsInterestedInCARST()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbBranch.CurrentBranch.OrgProxy.OH_IsSeaCTO = true;
			AssertEquals("Is Interested In OceanBillCMRCARSTMessage", true, Creator.IsInterestedInCARST(OceanBillCMRCARSTMessage));
			AssertEquals("Is Interested In CargoLineStatusCMRCARSTMessage", true, Creator.IsInterestedInCARST(CargoLineStatusCMRCARSTMessage));
			AssertEquals("Is NOT Interested In HAWBCMRCARSTMessage", false, Creator.IsInterestedInCARST(HAWBCMRCARSTMessage));
		}

		public void TestLoadOrCreateRecordForMessageCore_WhenNoRecord()
		{
			AssertEquals("Result", null, Creator.LoadOrCreateRecordForMessageCore(HAWBCMRCARSTMessage)[0]);
		}

		public void TestLoadOrCreateRecordForMessageCore_ManifestBill_WhenRecordExists()
		{
			var manifest = Factory.New<CusSeaManTranHead>();
			manifest.BT_VoyageNum = "4365";
			manifest.BT_VesselName = "ADMIRALENGRACHT";
			CusSeaManOBLHeader oceanBill = manifest.OceanBills.AddNew();
			oceanBill.BO_OceanBill = "OB0987123";
			BusinessObject result = Creator.LoadOrCreateRecordForMessageCore(OceanBillCMRCARSTMessage)[0];
			AssertEquals(oceanBill, result);
		}

		public void TestLoadOrCreateRecordForMessageCore_ManifestBill_WhenNoMatch()
		{
			var manifest = Factory.New<CusSeaManTranHead>();
			manifest.BT_VoyageNum = "4365";
			manifest.BT_VesselName = "ADMIRALENGRACHT";
			CusSeaManOBLHeader oceanBill = manifest.OceanBills.AddNew();
			oceanBill.BO_OceanBill = "OB0987124";
			BusinessObject result = Creator.LoadOrCreateRecordForMessageCore(OceanBillCMRCARSTMessage)[0];
			AssertNull("Result", result);
		}

		public void TestLoadOrCreateRecordForMessageCore_CargoLineStatus_WhenRecordExists()
		{
			var manifest = Factory.New<CusSeaManTranHead>();
			manifest.BT_VoyageNum = "4365";
			manifest.BT_VesselName = "ADMIRALENGRACHT";
			var arrival1 = manifest.Arrivals.AddNew();
			arrival1.BA_SendersMessageReference = "A00000532";
			var cargoLine1 = arrival1.CargoLines.AddNew();
			cargoLine1.Detail.BD_ContainerNumber = "CN001";
			var cargoLine2 = arrival1.CargoLines.AddNew();
			cargoLine2.Detail.BD_ContainerNumber = "CN002";
			var cargoLine3 = arrival1.CargoLines.AddNew();
			cargoLine3.Detail.BD_ContainerNumber = "CN003";
			Factory.Save();

			BusinessObject result = Creator.LoadOrCreateRecordForMessageCore(CargoLineStatusCMRCARSTMessage)[0];
			AssertEquals(cargoLine2, result);
		}

		public void TestLoadOrCreateRecordForMessageCore_CargoLineStatus_WhenNoMatch()
		{
			var manifest = Factory.New<CusSeaManTranHead>();
			manifest.BT_VoyageNum = "4365";
			manifest.BT_VesselName = "ADMIRALENGRACHT";
			var arrival1 = manifest.Arrivals.AddNew();
			arrival1.BA_SendersMessageReference = "A00000999/CMT9";
			var cargoLine1 = arrival1.CargoLines.AddNew();
			cargoLine1.Detail.BD_ContainerNumber = "CN002";

			BusinessObject result = Creator.LoadOrCreateRecordForMessageCore(CargoLineStatusCMRCARSTMessage)[0];
			AssertNull("Result", result);
		}

		#region Implementation

		VoyageManifestCARSTBusinessObjectLoaderOrCreator fCreator;
		VoyageManifestCARSTBusinessObjectLoaderOrCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new VoyageManifestCARSTBusinessObjectLoaderOrCreator();
				}
				return fCreator;
			}
		}

		CMRCARSTMessage fOceanBillCMRCARSTMessage;
		CMRCARSTMessage OceanBillCMRCARSTMessage
		{
			get
			{
				if (fOceanBillCMRCARSTMessage == null)
				{
					fOceanBillCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
					fOceanBillCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
TDT+20+4365++11++++8811924::11'
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
				return fOceanBillCMRCARSTMessage;
			}
		}

		CMRCARSTMessage fHAWBCMRCARSTMessage;
		CMRCARSTMessage HAWBCMRCARSTMessage
		{
			get
			{
				if (fHAWBCMRCARSTMessage == null)
				{
					fHAWBCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
					fHAWBCMRCARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
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
				return fHAWBCMRCARSTMessage;
			}
		}

		CMRCARSTMessage fCargoLineStatusCMRCARSTMessage;
		CMRCARSTMessage CargoLineStatusCMRCARSTMessage
		{
			get
			{
				if (fCargoLineStatusCMRCARSTMessage == null)
				{
					fCargoLineStatusCMRCARSTMessage = Factory.New<CMRCARSTMessage>();
					fCargoLineStatusCMRCARSTMessage.EM_MessageText = @"UNH+000004+CUSRES:D:99B:UN'
BGM+34:::CARST+2D4H 75J9 C4AI:1+8'
DTM+9:20180706133245212750:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+4365++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:A00000532/CMT1::1'
RFF+AAQ:CN002'
RFF+ACC:E'
DOC+1'
PAC+++FCL:67:95'
UNT+16+000004'".Replace("\r\n", "");
				}
				return fCargoLineStatusCMRCARSTMessage;
			}
		}

		#endregion
	}
}
