using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CMRSeaCargoDepotTestCase : SeaCargoDepotTestCase
	{
		#region Sailing Info
		protected const string TestScenarioVoyageNum = "936";
		protected const string TestScenarioLloydsNumber = "8811924";
		#endregion

		#region Test Messages

		#region OBL250805001

		#region Message Text

		protected const string OBL250805001_UnderbondApprovalAdvice_FRCU3948922 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+287I AFF8 F615:1+32'
DTM+9:20050830104906876127:ZZZ'
FTX+AAH+++AAA447YL5040040C0001/2/FRCU3948922'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0001/2/FRCU3948922::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:FRCU3948922'
UNT+18+000001'
";
		protected const string OBL250805001_UnderbondApprovalAdvice_OOLU3849386 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+402H DDBD F615:1+32'
DTM+9:20050830104907676459:ZZZ'
FTX+AAH+++AAA447YL5040040C0002/3/OOLU3849386'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0002/3/OOLU3849386::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:OOLU3849386'
UNT+18+000001'";
		protected const string OBL250805001_UnderbondApprovalAdvice_TRCU3382910 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+I7D4 DHIF 615:1+32'
DTM+9:20050830104908243298:ZZZ'
FTX+AAH+++AAA447YL5040040C0003/3/TRCU3382910'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0003/3/TRCU3382910::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:TRCU3382910'
UNT+18+000001'
";

		protected const string OBL250805001_ExpectedCargoArrivalAdvice_FRCU3948922 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+E24D A93F 615:1+32'
DTM+9:20050830104907306473:ZZZ'
FTX+AAH+++AAA447YL5040040C0001/2/FRCU3948922'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0001/2/FRCU3948922::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:FRCU3948922'
UNT+18+000001'";
		#region Faked Rescind Message
		protected const string OBL250805001_ExpectedCargoArrivalRescind_FRCU3948922 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+E24D A93F 615:1+32'
DTM+9:20050830104907306473:ZZZ'
FTX+AAH+++AAA447YL5040040C0001/2/FRCU3948922'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0001/2/FRCU3948922::1'
RFF+ANX:EXPECTED CARGO ARRIVAL RESCIND NTCE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:FRCU3948922'
UNT+18+000001'";
		#endregion
		protected const string OBL250805001_ExpectedCargoArrivalAdvice_OOLU3849386 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+H182 593F 615:1+32'
DTM+9:20050830104907997904:ZZZ'
FTX+AAH+++AAA447YL5040040C0002/3/OOLU3849386'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0002/3/OOLU3849386::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:OOLU3849386'
UNT+18+000001'";
		protected const string OBL250805001_ExpectedCargoArrivalAdvice_TRCU3382910 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2DJ8 97CI F615:1+32'
DTM+9:20050830104908466724:ZZZ'
FTX+AAH+++AAA447YL5040040C0003/3/TRCU3382910'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040C0003/3/TRCU3382910::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
RFF+AAQ:TRCU3382910'
UNT+18+000001'";

		protected const string OBL250805001_CargoStatusAdvice_FRCU3948922 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+492E F0B1 BDB5:1+8'
DTM+9:20050831154536167637:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040H0001::1'
RFF+MB:OBL250805001'
RFF+BH:DUMMY'
RFF+AAQ:FRCU3948922'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";

		protected const string OBL250805001_CargoStatusHeld_FRCU3948922 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+406I 22BJ ED65:1+8'
DTM+9:20050825153625537737:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:NO'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
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
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00000018/SYD1::1'
RFF+MB:OBL250805001'
RFF+AAQ:FRCU3948922'
DOC+1'
PAC+++FCL:67:95'
UNT+33+000001'";

		protected const string OBL250805001_CargoStatusAdvice_OOLU3849386 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4489 C13B BDB5:1+8'
DTM+9:20050831154538105867:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040H0001::1'
RFF+MB:OBL250805001'
RFF+BH:DUMMY'
RFF+AAQ:OOLU3849386'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";
		protected const string OBL250805001_CargoStatusHeld_TRCU3382910 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4ED1 C5G1 C365:1+8'
DTM+9:20050825154133753435:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:NO'
FTX+AHN+++LCL UNDERBOND SATISFIED:N/A'
FTX+AHN+++CARGO NOT A CONSOLIDATION:NO'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:N/A'
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
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00000018/SYD1::1'
RFF+MB:OBL250805001'
RFF+AAQ:TRCU3382910'
DOC+1'
PAC+++FCL:67:95'
UNT+33+000001'";
		protected const string OBL250805001_CargoStatusAdviceSubUBMov_TRCU3382910 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+34E1 G3EG 5615:1+8'
DTM+9:20050830104913475790:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:SUBUBMOV'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00000018/SYD1::3'
RFF+MB:OBL250805001'
RFF+AAQ:TRCU3382910'
DOC+1'
PAC+++FCL:67:95'
UNT+15+000001'";

		protected const string OBL250805001_CargoStatusAdvice_TRCU3382910 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+231J I994 1DB5:1+8'
DTM+9:20050831154540243568:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040H0001::1'
RFF+MB:OBL250805001'
RFF+BH:DUMMY'
RFF+AAQ:TRCU3382910'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";

		protected const string OBL250805001_CargoLineStatusAdvice_TRCU3382910 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+231J I994 1DB5:1+8'
DTM+9:20050831154540243568:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00000018/CMT1::1'
RFF+AAQ:TRCU3382910'
RFF+ACC:E'
DOC+1'
PAC+++FCL:67:95'
UNT+18+000001'";

		#endregion

		protected const string OBL250805001_OceanBillNum = "OBL250805001";
		protected const string OBL250805001_Container1 = "FRCU3948922";
		protected const string OBL250805001_Container2 = "OOLU3849386";
		protected const string OBL250805001_Container3 = "TRCU3382910";
		protected const string OBL250805001_HouseBill1 = "DUMMY";

		protected const string OBL250805001_ContainerMode = "FCL";
		protected const string OBL250805001_ConsolType = "AGT";

		protected CMRUBMREQRMessage OBL250805001_UnderbondApprovalAdviceMessage_FRCU3948922
		{
			get
			{
				if (fOBL250805001_UnderbondApprovalAdviceMessage_FRCU3948922 == null)
				{
					fOBL250805001_UnderbondApprovalAdviceMessage_FRCU3948922 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_UnderbondApprovalAdviceMessage_FRCU3948922.EM_MessageText = OBL250805001_UnderbondApprovalAdvice_FRCU3948922.Replace("\r\n", "");
				}
				return fOBL250805001_UnderbondApprovalAdviceMessage_FRCU3948922;
			}
		}
		CMRUBMREQRMessage fOBL250805001_UnderbondApprovalAdviceMessage_FRCU3948922;

		protected CMRUBMREQRMessage OBL250805001_UnderbondApprovalAdviceMessage_OOLU3849386
		{
			get
			{
				if (fOBL250805001_UnderbondApprovalAdviceMessage_OOLU3849386 == null)
				{
					fOBL250805001_UnderbondApprovalAdviceMessage_OOLU3849386 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_UnderbondApprovalAdviceMessage_OOLU3849386.EM_MessageText = OBL250805001_UnderbondApprovalAdvice_OOLU3849386.Replace("\r\n", "");
				}
				return fOBL250805001_UnderbondApprovalAdviceMessage_OOLU3849386;
			}
		}
		CMRUBMREQRMessage fOBL250805001_UnderbondApprovalAdviceMessage_OOLU3849386;

		protected CMRUBMREQRMessage OBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910
		{
			get
			{
				if (fOBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910 == null)
				{
					fOBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910.EM_MessageText = OBL250805001_UnderbondApprovalAdvice_TRCU3382910.Replace("\r\n", "");
				}
				return fOBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910;
			}
		}
		CMRUBMREQRMessage fOBL250805001_UnderbondApprovalAdviceMessage_TRCU3382910;

		protected CMRUBMREQRMessage OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922
		{
			get
			{
				if (fOBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922 == null)
				{
					fOBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922.EM_MessageText = OBL250805001_ExpectedCargoArrivalAdvice_FRCU3948922.Replace("\r\n", "");
				}
				return fOBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922;
			}
		}
		CMRUBMREQRMessage fOBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922;

		protected CMRUBMREQRMessage OBL250805001_ExpectedCargoArrivalRescindMessageFRCU3948922
		{
			get
			{
				if (fOBL250805001_ExpectedCargoArrivalRescindMessage_FRCU3948922 == null)
				{
					fOBL250805001_ExpectedCargoArrivalRescindMessage_FRCU3948922 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_ExpectedCargoArrivalRescindMessage_FRCU3948922.EM_MessageText = OBL250805001_ExpectedCargoArrivalRescind_FRCU3948922.Replace("\r\n", "");
				}
				return fOBL250805001_ExpectedCargoArrivalRescindMessage_FRCU3948922;
			}
		}
		CMRUBMREQRMessage fOBL250805001_ExpectedCargoArrivalRescindMessage_FRCU3948922;

		protected CMRUBMREQRMessage OBL250805001_ExpectedCargoArrivalAdviceMessage_OOLU3849386
		{
			get
			{
				if (fOBL250805001_ExpectedCargoArrivalAdviceMessage_OOLU3849386 == null)
				{
					fOBL250805001_ExpectedCargoArrivalAdviceMessage_OOLU3849386 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_ExpectedCargoArrivalAdviceMessage_OOLU3849386.EM_MessageText = OBL250805001_ExpectedCargoArrivalAdvice_OOLU3849386.Replace("\r\n", "");
				}
				return fOBL250805001_ExpectedCargoArrivalAdviceMessage_OOLU3849386;
			}
		}
		CMRUBMREQRMessage fOBL250805001_ExpectedCargoArrivalAdviceMessage_OOLU3849386;

		protected CMRUBMREQRMessage OBL250805001_ExpectedCargoArrivalAdviceMessage_TRCU3382910
		{
			get
			{
				if (fOBL250805001_ExpectedCargoArrivalAdviceMessage_TRCU3382910 == null)
				{
					fOBL250805001_ExpectedCargoArrivalAdviceMessage_TRCU3382910 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805001_ExpectedCargoArrivalAdviceMessage_TRCU3382910.EM_MessageText = OBL250805001_ExpectedCargoArrivalAdvice_TRCU3382910.Replace("\r\n", "");
				}
				return fOBL250805001_ExpectedCargoArrivalAdviceMessage_TRCU3382910;
			}
		}
		CMRUBMREQRMessage fOBL250805001_ExpectedCargoArrivalAdviceMessage_TRCU3382910;

		#region Cargo Status Advice Messages

		protected CMRCARSTMessage OBL250805001_CargoStatusAdviceMessage_FRCU3948922
		{
			get
			{
				if (fOBL250805001_CargoStatusAdviceMessage_FRCU3948922 == null)
				{
					fOBL250805001_CargoStatusAdviceMessage_FRCU3948922 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoStatusAdviceMessage_FRCU3948922.EM_MessageText = OBL250805001_CargoStatusAdvice_FRCU3948922.Replace("\r\n", "");
				}
				return fOBL250805001_CargoStatusAdviceMessage_FRCU3948922;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoStatusAdviceMessage_FRCU3948922;

		protected CMRCARSTMessage OBL250805001_CargoStatusHeldMessage_FRCU3948922
		{
			get
			{
				if (fOBL250805001_CargoStatusHeldMessage_FRCU3948922 == null)
				{
					fOBL250805001_CargoStatusHeldMessage_FRCU3948922 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoStatusHeldMessage_FRCU3948922.EM_MessageText = OBL250805001_CargoStatusHeld_FRCU3948922.Replace("\r\n", "");
				}
				return fOBL250805001_CargoStatusHeldMessage_FRCU3948922;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoStatusHeldMessage_FRCU3948922;

		protected CMRCARSTMessage OBL250805001_CargoStatusAdviceMessage_OOLU3849386
		{
			get
			{
				if (fOBL250805001_CargoStatusAdviceMessage_OOLU3849386 == null)
				{
					fOBL250805001_CargoStatusAdviceMessage_OOLU3849386 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoStatusAdviceMessage_OOLU3849386.EM_MessageText = OBL250805001_CargoStatusAdvice_OOLU3849386.Replace("\r\n", "");
				}
				return fOBL250805001_CargoStatusAdviceMessage_OOLU3849386;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoStatusAdviceMessage_OOLU3849386;

		protected CMRCARSTMessage OBL250805001_CargoStatusAdviceMessage_TRCU3382910
		{
			get
			{
				if (fOBL250805001_CargoStatusAdviceMessage_TRCU3382910 == null)
				{
					fOBL250805001_CargoStatusAdviceMessage_TRCU3382910 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoStatusAdviceMessage_TRCU3382910.EM_MessageText = OBL250805001_CargoStatusAdvice_TRCU3382910.Replace("\r\n", "");
				}
				return fOBL250805001_CargoStatusAdviceMessage_TRCU3382910;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoStatusAdviceMessage_TRCU3382910;

		protected CMRCARSTMessage OBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910
		{
			get
			{
				if (fOBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910 == null)
				{
					fOBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910.EM_MessageText = OBL250805001_CargoStatusAdviceSubUBMov_TRCU3382910.Replace("\r\n", "");
				}
				return fOBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910;

		protected CMRCARSTMessage OBL250805001_CargoStatusAdviceHeldMessage_TRCU3382910
		{
			get
			{
				if (fOBL250805001_CargoStatusAdviceHeldMessage_TRCU3382910 == null)
				{
					fOBL250805001_CargoStatusAdviceHeldMessage_TRCU3382910 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoStatusAdviceHeldMessage_TRCU3382910.EM_MessageText = OBL250805001_CargoStatusHeld_TRCU3382910.Replace("\r\n", "");
				}
				return fOBL250805001_CargoStatusAdviceHeldMessage_TRCU3382910;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoStatusAdviceHeldMessage_TRCU3382910;

		protected CMRCARSTMessage OBL250805001_CargoLineStatusAdviceMessage_TRCU3382910
		{
			get
			{
				if (fOBL250805001_CargoLineStatusAdviceMessage_TRCU3382910 == null)
				{
					fOBL250805001_CargoLineStatusAdviceMessage_TRCU3382910 = Factory.New<CMRCARSTMessage>();
					fOBL250805001_CargoLineStatusAdviceMessage_TRCU3382910.EM_MessageText = OBL250805001_CargoLineStatusAdvice_TRCU3382910.Replace("\r\n", "");
				}
				return fOBL250805001_CargoLineStatusAdviceMessage_TRCU3382910;
			}
		}
		CMRCARSTMessage fOBL250805001_CargoLineStatusAdviceMessage_TRCU3382910;

		#endregion

		protected CFSLoadListConsol CreateDepotJobOBL250805001()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			consol.JK_MasterBillNum = OBL250805001_OceanBillNum;

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestScenarioLloydsNumber, TestScenarioVoyageNum).PK;

			CFSContainer container1 = consol.Containers.AddNew();
			CFSContainer container2 = consol.Containers.AddNew();
			CFSContainer container3 = consol.Containers.AddNew();
			container1.JC_ContainerNum = OBL250805001_Container1;
			container2.JC_ContainerNum = OBL250805001_Container2;
			container3.JC_ContainerNum = OBL250805001_Container3;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = OBL250805006_HouseBill1;
			shipment.JS_OuterPacks = 30;
			return consol;
		}

		#endregion

		#region Forwarding OBL250805001

		protected OrgHeader ForwardingBranchOrgProxy
		{
			get
			{
				foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
				{
					if (branch.OrgProxy != null && branch.OrgProxy.OH_IsForwarder)
					{
						return branch.OrgProxy;
					}
				}
				return null;
			}
		}

		protected void CreateOBL250805001ForwardingConsol(BusinessObjectFactory factory)
		{
			ForwardingConsol consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;

			Transport transport = consol.Transports[0];
			transport.JW_JX = TestSailing(factory).PK;
			consol.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			consol.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			consol.JK_MasterBillNum = OBL250805001_OceanBillNum;
			AssertNotNull("No Forwarding Branch Found", ForwardingBranchOrgProxy);
			consol.SetDefaultReceivingForwarderAddress(ForwardingBranchOrgProxy);
			consol.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = OBL250805001_Container1;
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = OBL250805001_Container2;
			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = OBL250805001_Container3;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = OBL250805001_HouseBill1;
			shipment.ConsigneePK = CreateConsignee("MITCH", factory).PK;
			shipment.ConsigneePK = CreateConsignor("LARRY", factory).PK;
			shipment.JS_OuterPacks = 30;
			shipment.JS_F3_NKPackType = "BOX";
			PackLine packLine1 = shipment.OuterPackLines[0];
			packLine1.JL_PackageCount = 10;
			packLine1.SetContainer(consol, container1);
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 10;
			packLine2.SetContainer(consol, container2);
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 10;
			packLine3.SetContainer(consol, container3);
			shipment.JS_OuterPacks = 30;
			factory.Save();
		}

		#endregion

		#region OBL250805002

		#region Message Text

		protected const string OBL250805002_ExpectedCargoArrival_GGHU4483920 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+49EB B8AD G0G5:1+32'
DTM+9:20050826102622429232:ZZZ'
FTX+AAH+++AAA447YL5040041C0001/GGHU4483920'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040041C0001/GGHU4483920::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000300++BX:185:95'
RFF+AAQ:GGHU4483920'
UNT+18+000001'";

		protected const string OBL250805002_UnderbondApprovalAdvice_GGHU4483920 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+21CB 53AD G0G5:1+32'
DTM+9:20050826102622273080:ZZZ'
FTX+AAH+++AAA447YL5040041C0001/GGHU4483920'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040041C0001/GGHU4483920::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000300++BX:185:95'
RFF+AAQ:GGHU4483920'
UNT+18+000001'";

		protected const string OBL250805002_CargoStatusAdvice_GGHU4483920 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1IBH 141D IF1F:1+8'
DTM+9:20051027134147882158:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::6'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00000019/SYD1::1'
RFF+AAQ:GGHU4483920'
DOC+1'
PAC+++FCX:67:95'
PAC+++PK:185:95'
UNT+16+000001'";

		#endregion

		protected const string OBL250805002_OceanBillNum = "OBL250805001";
		protected const string OBL250805002_Container1 = "GGHU4483920";
		protected const string OBL250805002_HouseBill1 = "";

		protected const string OBL250805002_ContainerMode = "FCL";
		protected const string OBL250805002_ConsolType = "FCX";

		protected CMRUBMREQRMessage OBL250805002_ExpectedCargoArrivalMessage_GGHU4483920
		{
			get
			{
				if (fOBL250805002_ExpectedCargoArrivalMessage_GGHU4483920 == null)
				{
					fOBL250805002_ExpectedCargoArrivalMessage_GGHU4483920 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805002_ExpectedCargoArrivalMessage_GGHU4483920.EM_MessageText = OBL250805002_ExpectedCargoArrival_GGHU4483920.Replace("\r\n", "");
				}
				return fOBL250805002_ExpectedCargoArrivalMessage_GGHU4483920;
			}
		}
		CMRUBMREQRMessage fOBL250805002_ExpectedCargoArrivalMessage_GGHU4483920;

		protected CMRUBMREQRMessage OBL250805002_UnderbondApprovalAdviceMessage_GGHU4483920
		{
			get
			{
				if (fOBL250805002_UnderbondApprovalAdviceMessage_GGHU4483920 == null)
				{
					fOBL250805002_UnderbondApprovalAdviceMessage_GGHU4483920 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805002_UnderbondApprovalAdviceMessage_GGHU4483920.EM_MessageText = OBL250805002_UnderbondApprovalAdvice_GGHU4483920.Replace("\r\n", "");
				}
				return fOBL250805002_UnderbondApprovalAdviceMessage_GGHU4483920;
			}
		}
		CMRUBMREQRMessage fOBL250805002_UnderbondApprovalAdviceMessage_GGHU4483920;

		protected CMRCARSTMessage OBL250805002_CargoStatusAdviceMessage_GGHU4483920
		{
			get
			{
				if (fOBL250805002_CargoStatusAdviceMessage_GGHU4483920 == null)
				{
					fOBL250805002_CargoStatusAdviceMessage_GGHU4483920 = Factory.New<CMRCARSTMessage>();
					fOBL250805002_CargoStatusAdviceMessage_GGHU4483920.EM_MessageText = OBL250805002_CargoStatusAdvice_GGHU4483920.Replace("\r\n", "");
				}
				return fOBL250805002_CargoStatusAdviceMessage_GGHU4483920;
			}
		}
		CMRCARSTMessage fOBL250805002_CargoStatusAdviceMessage_GGHU4483920;

		#endregion

		#region OBL250805003

		#region Message Text

		protected const string OBL250805003_ExpectedCargoArrival_BreakBulk = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3H23 25B6 J115:1+32'
DTM+9:20050830113617386418:ZZZ'
FTX+AAH+++AAA447YL5040042H0001/3'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040042H0001/3::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
RFF+MB:OBL250805003'
FTX+AAA+++BIG PIPE'
UNT+18+000001'";

		#region Fake Rescind Message

		protected const string OBL250805003_ExpectedCargoArrivalRescind_BreakBulk = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3H23 25B6 J115:1+32'
DTM+9:20050830113617386418:ZZZ'
FTX+AAH+++AAA447YL5040042H0001/3'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040042H0001/3::1'
RFF+ANX:EXPECTED CARGO ARRIVAL RESCIND NTCE'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
RFF+MB:OBL250805003'
FTX+AAA+++BIG PIPE'
UNT+18+000001'";

		#endregion

		protected const string OBL250805003_UnderbondApprovalAdvice_BreakBulk = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+1H05 J63G J115:1+32'
DTM+9:20050830113617231435:ZZZ'
FTX+AAH+++AAA447YL5040042H0001/3'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040042H0001/3::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
RFF+MB:OBL250805003'
FTX+AAA+++BIG PIPE'
UNT+18+000001'
";

		protected const string OBL250805003_CargoStatusAdvice_BreakBulk = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1IIG EFCC G15:1+8'
DTM+9:20050830120622841743:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N:95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040042H0001::3'
RFF+MB:OBL250805003'
RFF+BH:DUMMY'
DOC+1'
PAC+++B/B:67:95'
PAC+0000010++BX:185:95'
FTX+AAA+++BREAK BULK WIDGETS'
FTX+MKS+++;XXXX'
UNT+30+000001'";

		#endregion

		protected const string OBL250805003_OceanBillNum = "OBL250805003";
		protected const string OBL250805003_HouseBill1 = "DUMMY";

		protected const string OBL250805003_ContainerMode = "B/B";
		protected const string OBL250805003_ConsolType = "AGT";

		protected CMRUBMREQRMessage OBL250805003_UnderbondApprovalAdviceMessage_BreakBulk
		{
			get
			{
				if (fOBL250805003_UnderbondApprovalAdviceMessage_BreakBulk == null)
				{
					fOBL250805003_UnderbondApprovalAdviceMessage_BreakBulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805003_UnderbondApprovalAdviceMessage_BreakBulk.EM_MessageText = OBL250805003_UnderbondApprovalAdvice_BreakBulk.Replace("\r\n", "");
				}
				return fOBL250805003_UnderbondApprovalAdviceMessage_BreakBulk;
			}
		}
		CMRUBMREQRMessage fOBL250805003_UnderbondApprovalAdviceMessage_BreakBulk;

		protected CMRUBMREQRMessage OBL250805003_ExpectedCargoArrivalAdviceMessage_BreakBulk
		{
			get
			{
				if (fOBL250805003_CargoArrivalAdviceMessage_BreakBulk == null)
				{
					fOBL250805003_CargoArrivalAdviceMessage_BreakBulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805003_CargoArrivalAdviceMessage_BreakBulk.EM_MessageText = OBL250805003_ExpectedCargoArrival_BreakBulk.Replace("\r\n", "");
				}
				return fOBL250805003_CargoArrivalAdviceMessage_BreakBulk;
			}
		}
		CMRUBMREQRMessage fOBL250805003_CargoArrivalAdviceMessage_BreakBulk;

		protected CMRUBMREQRMessage OBL250805003_ExpectedCargoArrivalAdviceRescindMessage_BreakBulk
		{
			get
			{
				if (fOBL250805003_ExpectedCargoArrivalAdviceRescindMessage_BreakBulk == null)
				{
					fOBL250805003_ExpectedCargoArrivalAdviceRescindMessage_BreakBulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805003_ExpectedCargoArrivalAdviceRescindMessage_BreakBulk.EM_MessageText = OBL250805003_ExpectedCargoArrivalRescind_BreakBulk.Replace("\r\n", "");
				}
				return fOBL250805003_ExpectedCargoArrivalAdviceRescindMessage_BreakBulk;
			}
		}
		CMRUBMREQRMessage fOBL250805003_ExpectedCargoArrivalAdviceRescindMessage_BreakBulk;

		protected CMRCARSTMessage OBL250805003_CargoStatusAdviceMessage_BreakBulk
		{
			get
			{
				if (fOBL250805003_CargoStatusAdviceMessage_BreakBulk == null)
				{
					fOBL250805003_CargoStatusAdviceMessage_BreakBulk = Factory.New<CMRCARSTMessage>();
					fOBL250805003_CargoStatusAdviceMessage_BreakBulk.EM_MessageText = OBL250805003_CargoStatusAdvice_BreakBulk.Replace("\r\n", "");
				}
				return fOBL250805003_CargoStatusAdviceMessage_BreakBulk;
			}
		}
		CMRCARSTMessage fOBL250805003_CargoStatusAdviceMessage_BreakBulk;

		protected void CreateDepotJobOBL250805003()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			consol.JK_MasterBillNum = OBL250805003_OceanBillNum;

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestScenarioLloydsNumber, TestScenarioVoyageNum).PK;

			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = OBL250805003_HouseBill1;
		}

		#endregion

		#region OBL250805004

		#region Message Text

		protected const string OBL250805004_UnderbondApprovalAdvice = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+184D BD16 J115:1+32'
DTM+9:20050830113617512418:ZZZ'
FTX+AAH+++AAA447YL5040043H0001/3'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040043H0001/3::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++BLK:67:95'
RFF+MB:OBL250805004'
FTX+AAA+++BULK LIQUID'
UNT+18+000001'";

		protected const string OBL250805004_ExpectedCargoArrival = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+HCAC 5B6J 115:1+32'
DTM+9:20050830113617594451:ZZZ'
FTX+AAH+++AAA447YL5040043H0001/3'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040043H0001/3::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++BLK:67:95'
RFF+MB:OBL250805004'
FTX+AAA+++BULK LIQUID'
UNT+18+000001'
";

		protected const string OBL250805004_CargoStatusClear = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1DJD HI83 43B5:1+8'
DTM+9:20050831111837301361:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040043H0001::2'
RFF+MB:OBL250805004'
RFF+BH:DUMMY'
DOC+1'
PAC+++BLK:67:95'
FTX+AAA+++BULK WIDGETS'
UNT+17+000001'";

		protected const string OBL250805004_CargoStatusHeld = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1AB7 CBJ7 CG15:1+8'
DTM+9:20050830120623720723:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040043H0001::2'
RFF+MB:OBL250805004'
RFF+BH:DUMMY'
DOC+1'
PAC+++BLK:67:95'
FTX+AAA+++BULK WIDGETS'
UNT+28+000001'";

		#endregion

		protected const string OBL250805004_OceanBillNum = "OBL250805004";
		protected const string OBL250805004_HouseBill1 = "DUMMY";

		protected const string OBL250805004_ContainerMode = "BLK";
		protected const string OBL250805004_ConsolType = "BLK";

		protected CMRUBMREQRMessage OBL250805004_UnderbondApprovalAdviceMessage_Bulk
		{
			get
			{
				if (fOBL250805004_UnderbondApprovalAdviceMessage_Bulk == null)
				{
					fOBL250805004_UnderbondApprovalAdviceMessage_Bulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805004_UnderbondApprovalAdviceMessage_Bulk.EM_MessageText = OBL250805004_UnderbondApprovalAdvice.Replace("\r\n", "");
				}
				return fOBL250805004_UnderbondApprovalAdviceMessage_Bulk;
			}
		}
		CMRUBMREQRMessage fOBL250805004_UnderbondApprovalAdviceMessage_Bulk;

		protected CMRUBMREQRMessage OBL250805004_CargoArrivalAdviceMessage_Bulk
		{
			get
			{
				if (fOBL250805004_CargoArrivalAdviceMessage_Bulk == null)
				{
					fOBL250805004_CargoArrivalAdviceMessage_Bulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805004_CargoArrivalAdviceMessage_Bulk.EM_MessageText = OBL250805004_ExpectedCargoArrival.Replace("\r\n", "");
				}
				return fOBL250805004_CargoArrivalAdviceMessage_Bulk;
			}
		}
		CMRUBMREQRMessage fOBL250805004_CargoArrivalAdviceMessage_Bulk;

		protected CMRCARSTMessage OBL250805004_CargoStatusClearMessage_Bulk
		{
			get
			{
				if (fOBL250805004_CargoStatusClearMessage_Bulk == null)
				{
					fOBL250805004_CargoStatusClearMessage_Bulk = Factory.New<CMRCARSTMessage>();
					fOBL250805004_CargoStatusClearMessage_Bulk.EM_MessageText = OBL250805004_CargoStatusClear.Replace("\r\n", "");
				}
				return fOBL250805004_CargoStatusClearMessage_Bulk;
			}
		}
		CMRCARSTMessage fOBL250805004_CargoStatusClearMessage_Bulk;

		protected CMRCARSTMessage OBL250805004_CargoStatusHeldMessage_Bulk
		{
			get
			{
				if (fOBL250805004_CargoStatusHeldMessage_Bulk == null)
				{
					fOBL250805004_CargoStatusHeldMessage_Bulk = Factory.New<CMRCARSTMessage>();
					fOBL250805004_CargoStatusHeldMessage_Bulk.EM_MessageText = OBL250805004_CargoStatusHeld.Replace("\r\n", "");
				}
				return fOBL250805004_CargoStatusHeldMessage_Bulk;
			}
		}
		CMRCARSTMessage fOBL250805004_CargoStatusHeldMessage_Bulk;

		protected void CreateDepotJobOBL250805004()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			consol.JK_MasterBillNum = OBL250805004_OceanBillNum;

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestScenarioLloydsNumber, TestScenarioVoyageNum).PK;

			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = OBL250805004_HouseBill1;
		}

		#endregion

		#region OBL250805005

		#region Test Messages

		protected const string OBL250805005_UnderbondApprovalAdvice_GFCU3049101 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+34E2 GF25 9FG5:1+32'
DTM+9:20050826172746006718:ZZZ'
FTX+AAH+++AAA447YL5040044H0001'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0001::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DEL'
DOC+1'
PAC+++LCL:67:95'
RFF+AAQ:GFCU3049101'
RFF+MB:OBL250805005'
RFF+BH:OBL250805005H1'
UNT+19+000001'";

		protected const string OBL250805005_ExpectedCargoArrival_GFCU3049101 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3685 9H9D IAG5:1+32'
DTM+9:20050826120936670475:ZZZ'
FTX+AAH+++AAA447YL5040044C0001/GFCU3049101'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044C0001/GFCU3049101::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000600++BX:185:95'
RFF+AAQ:GFCU3049101'
UNT+18+000001'";
		protected const string OBL250805005_ExpectedCargoArrivalRescind_GFCU3049101 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3685 9H9D IAG5:1+32'
DTM+9:20050826120936670475:ZZZ'
FTX+AAH+++AAA447YL5040044C0001/GFCU3049101'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044C0001/GFCU3049101::1'
RFF+ANX:EXPECTED CARGO ARRIVAL RESCIND NTCE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000600++BX:185:95'
RFF+AAQ:GFCU3049101'
UNT+18+000001'";

		protected const string OBL250805005_H1_ExpectedCargoArrival_GFCU3049101 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+4068 BBJF 9FG5:1+32'
DTM+9:20050826172746194622:ZZZ'
FTX+AAH+++AAA447YL5040044H0001'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0001::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DEL'
DOC+1'
PAC+++LCL:67:95'
RFF+AAQ:GFCU3049101'
RFF+MB:OBL250805005'
RFF+BH:OBL250805005H1'
UNT+19+000001'";

		protected const string OBL250805005_H1_ExpectedCargoArrivalRescind_GFCU3049101 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+4068 BBJF 9FG5:1+32'
DTM+9:20050826172746194622:ZZZ'
FTX+AAH+++AAA447YL5040044H0001'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9914N::95'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0001::1'
RFF+ANX:EXPECTED CARGO ARRIVAL RESCIND NTCE'
RFF+ACD:DEL'
DOC+1'
PAC+++LCL:67:95'
RFF+AAQ:GFCU3049101'
RFF+MB:OBL250805005'
RFF+BH:OBL250805005H1'
UNT+19+000001'";

		protected const string OBL250805005_CargoStatusAdvice_H1 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1253 VR76 7IB5:1+8'
DTM+9:20050831124911240255:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0001::3'
RFF+MB:OBL250805005'
RFF+BH:OBL250805005H1'
RFF+AAQ:GFCU3049101'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";

		protected const string OBL250805005_CargoStatusAdvice_H2 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4G14 J3B7 CIB5:1+8'
DTM+9:20050831124849876814:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0002::3'
RFF+MB:OBL250805005'
RFF+BH:OBL250805005H2'
RFF+AAQ:GFCU3049101'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";

		protected const string OBL250805005_CargoStatusAdvice_H3 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1253 BJ76 7IB5:1+8'
DTM+9:20050831124911240255:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040044H0003::3'
RFF+MB:OBL250805005'
RFF+BH:OBL250805005H3'
RFF+AAQ:GFCU3049101'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";

		#endregion

		protected const string OBL250805005_OceanBillNum = "OBL250805005";
		protected const string OBL250805005_ContainerNumber = "GFCU3049101";
		protected const string OBL250805005_HouseBill1 = "OBL250805005H1";
		protected const string OBL250805005_HouseBill2 = "OBL250805005H2";
		protected const string OBL250805005_HouseBill3 = "OBL250805005H3";

		protected const string OBL250805005_ContainerMode = "LCL";
		protected const string OBL250805005_ConsolType = "LCL";

		protected CMRUBMREQRMessage OBL250805005_UnderbondApprovalAdviceMessage_GFCU3049101
		{
			get
			{
				if (fOBL250805005_UnderbondApprovalAdviceMessage_GFCU3049101 == null)
				{
					fOBL250805005_UnderbondApprovalAdviceMessage_GFCU3049101 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805005_UnderbondApprovalAdviceMessage_GFCU3049101.EM_MessageText = OBL250805005_UnderbondApprovalAdvice_GFCU3049101.Replace("\r\n", "");
				}
				return fOBL250805005_UnderbondApprovalAdviceMessage_GFCU3049101;
			}
		}
		CMRUBMREQRMessage fOBL250805005_UnderbondApprovalAdviceMessage_GFCU3049101;

		protected CMRUBMREQRMessage OBL250805005_CargoArrivalAdviceMessage_GFCU3049101
		{
			get
			{
				if (fOBL250805005_CargoArrivalAdviceMessage_GFCU3049101 == null)
				{
					fOBL250805005_CargoArrivalAdviceMessage_GFCU3049101 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805005_CargoArrivalAdviceMessage_GFCU3049101.EM_MessageText = OBL250805005_ExpectedCargoArrival_GFCU3049101.Replace("\r\n", "");
				}
				return fOBL250805005_CargoArrivalAdviceMessage_GFCU3049101;
			}
		}
		CMRUBMREQRMessage fOBL250805005_CargoArrivalAdviceMessage_GFCU3049101;

		protected CMRUBMREQRMessage OBL250805005_ExpectedCargoArrivalRescindMessage_GFCU3049101
		{
			get
			{
				if (fOBL250805005_ExpectedCargoArrivalRescindMessage_GFCU3049101 == null)
				{
					fOBL250805005_ExpectedCargoArrivalRescindMessage_GFCU3049101 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805005_ExpectedCargoArrivalRescindMessage_GFCU3049101.EM_MessageText = OBL250805005_ExpectedCargoArrivalRescind_GFCU3049101.Replace("\r\n", "");
				}
				return fOBL250805005_ExpectedCargoArrivalRescindMessage_GFCU3049101;
			}
		}
		CMRUBMREQRMessage fOBL250805005_ExpectedCargoArrivalRescindMessage_GFCU3049101;

		protected CMRCARSTMessage OBL250805005_CargoStatusAdviceMessage_H1
		{
			get
			{
				if (fOBL250805005_CargoStatusAdviceMessage_H1 == null)
				{
					fOBL250805005_CargoStatusAdviceMessage_H1 = Factory.New<CMRCARSTMessage>();
					fOBL250805005_CargoStatusAdviceMessage_H1.EM_MessageText = OBL250805005_CargoStatusAdvice_H1.Replace("\r\n", "");
				}
				return fOBL250805005_CargoStatusAdviceMessage_H1;
			}
		}
		CMRCARSTMessage fOBL250805005_CargoStatusAdviceMessage_H1;

		protected CMRCARSTMessage OBL250805005_CargoStatusAdviceMessage_H2
		{
			get
			{
				if (fOBL250805005_CargoStatusAdviceMessage_H2 == null)
				{
					fOBL250805005_CargoStatusAdviceMessage_H2 = Factory.New<CMRCARSTMessage>();
					fOBL250805005_CargoStatusAdviceMessage_H2.EM_MessageText = OBL250805005_CargoStatusAdvice_H2.Replace("\r\n", "");
				}
				return fOBL250805005_CargoStatusAdviceMessage_H2;
			}
		}
		CMRCARSTMessage fOBL250805005_CargoStatusAdviceMessage_H2;

		protected CMRCARSTMessage OBL250805005_CargoStatusAdviceMessage_H3
		{
			get
			{
				if (fOBL250805005_CargoStatusAdviceMessage_H3 == null)
				{
					fOBL250805005_CargoStatusAdviceMessage_H3 = Factory.New<CMRCARSTMessage>();
					fOBL250805005_CargoStatusAdviceMessage_H3.EM_MessageText = OBL250805005_CargoStatusAdvice_H3.Replace("\r\n", "");
				}
				return fOBL250805005_CargoStatusAdviceMessage_H3;
			}
		}
		CMRCARSTMessage fOBL250805005_CargoStatusAdviceMessage_H3;

		#endregion

		#region OBL250805006

		#region Message Text

		protected const string OBL250805006_UnderbondApprovalAdvice_TXCU9304939 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+1EA8 G46J 58B5:1+32'
DTM+9:20050831082855505832:ZZZ'
FTX+AAH+++AAA447YL5040046C0001/TXCU9304939'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040046C0001/TXCU9304939::1'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000300++BX:185:95'
RFF+AAQ:TXCU9304939'
PCI+28+TRFI3000000'
UNT+19+000001'";

		protected const string OBL250805006_ExpectedCargoArrival_TXCU9304939 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2IH8 955E 58B5:1+32'
DTM+9:20050831082856048653:ZZZ'
FTX+AAH+++AAA447YL5040046C0001/TXCU9304939'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040046C0001/TXCU9304939::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:DCL'
DOC+1'
PAC+++FCL:67:95'
PAC+0000300++BX:185:95'
RFF+AAQ:TXCU9304939'
PCI+28+TRFI3000000'
UNT+19+000001'";

		protected const string OBL250805006_CargoStatusAdviceHeld_OBL250805006H3 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+44IH DBJ7 D8G5:1+8'
DTM+9:20050914120606829426:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++IMPORT DECLARATION PAID:NO'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040046H0003::2'
RFF+MB:OBL250805006'
RFF+BH:OBL250805006H3'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000100++BX:185:95'
UNT+29+000001'
";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H1_SUB1 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+J7I4 CG6E 615:1+8'
DTM+9:20050901072047017300:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:T5042441H0001::1'
RFF+MB:OBL250805006'
RFF+BH:OBL6H1'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H1_SUB2 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3533 F599 4615:1+8'
DTM+9:20050901072053233229:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:T5042441H0002::1'
RFF+MB:OBL250805006'
RFF+BH:OBL6H2'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H1_SUB3 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+24AC 8F99 4615:1+8'
DTM+9:20050901072034900059:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:T5042441H0003::1'
RFF+MB:OBL250805006'
RFF+BH:OBL6H3'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000040++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H2_SUB1 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1JH1 4AJ8 3115:1+8'
DTM+9:20050901101224591253:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:T5042441H0006::1'
RFF+MB:OBL250805006'
RFF+BH:OBL62H1'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H2_CM1_SUB1 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4C80 GECJ 83B5:1+8'
DTM+9:20050902105714792071:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:Z4102091H0002::1'
RFF+MB:OBL250805006'
RFF+BH:OBL62H2H1'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H2_CM1_SUB2 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1I5B 08B7 5DB5:1+8'
DTM+9:20050902063841269631:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:Z4102091H0003::1'
RFF+MB:OBL250805006'
RFF+BH:OBL62H2H2'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H2_CM1_SUB3 = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+422G FCEF E8B5:1+8'
DTM+9:20050902113516331558:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CARGO HIERARCHY INCOMPLETE. STATUS MAY CHANGE AS OTHER BILLS ARE REPORTED.'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:Z4102091H0004::1'
RFF+MB:OBL250805006'
RFF+BH:OBL62H2H3'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000040++BX:185:95'
UNT+20+000001'";

		protected const string OBL250805006_CargoStatusAdviceClear_OBL250805006H3 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4HB1 538B 7IB5:1+8'
DTM+9:20050831124912450305:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040046H0003::2'
RFF+MB:OBL250805006'
RFF+BH:OBL250805006H3'
RFF+AAQ:TXCU9304939'
DOC+1'
PAC+++LCL:67:95'
PAC+0000100++BX:185:95'
UNT+18+000001'";

		#endregion

		protected const string OBL250805006_OceanBillNum = "OBL250805006";
		protected const string OBL250805006_ContainerNumber = "TXCU9304939";
		protected const string OBL250805006_HouseBill1 = "OBL250805006H1";
		protected const string OBL250805006_H1_SUB1 = "OBL6H1";
		protected const string OBL250805006_H1_SUB2 = "OBL6H2";
		protected const string OBL250805006_H1_SUB3 = "OBL6H3";
		protected const string OBL250805006_HouseBill2 = "OBL250805006H2";
		protected const string OBL250805006_H2_CM1 = "OBL62H1";
		protected const string OBL250805006_H2_CM2_H1 = "OBL62H2H1";
		protected const string OBL250805006_H2_CM2_H2 = "OBL62H2H2";
		protected const string OBL250805006_H2_CM2_H3 = "OBL62H2H3";
		protected const string OBL250805006_HouseBill3 = "OBL250805006H3";

		protected const string OBL250805006_ContainerMode = "LCL";
		protected const string OBL250805006_ConsolType = "GRP";

		protected CMRUBMREQRMessage OBL250805006_UnderbondApprovalAdviceMessage_TXCU9304939
		{
			get
			{
				if (fOBL250805006_UnderbondApprovalAdviceMessage_TXCU9304939 == null)
				{
					fOBL250805006_UnderbondApprovalAdviceMessage_TXCU9304939 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805006_UnderbondApprovalAdviceMessage_TXCU9304939.EM_MessageText = OBL250805006_UnderbondApprovalAdvice_TXCU9304939.Replace("\r\n", "");
				}
				return fOBL250805006_UnderbondApprovalAdviceMessage_TXCU9304939;
			}
		}
		CMRUBMREQRMessage fOBL250805006_UnderbondApprovalAdviceMessage_TXCU9304939;

		protected CMRUBMREQRMessage OBL250805006_CargoArrivalAdviceMessage_TXCU9304939
		{
			get
			{
				if (fOBL250805006_CargoArrivalAdviceMessage_TXCU9304939 == null)
				{
					fOBL250805006_CargoArrivalAdviceMessage_TXCU9304939 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805006_CargoArrivalAdviceMessage_TXCU9304939.EM_MessageText = OBL250805006_ExpectedCargoArrival_TXCU9304939.Replace("\r\n", "");
				}
				return fOBL250805006_CargoArrivalAdviceMessage_TXCU9304939;
			}
		}
		CMRUBMREQRMessage fOBL250805006_CargoArrivalAdviceMessage_TXCU9304939;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB1
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB1 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB1 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB1.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H1_SUB1.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB1;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB1;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB2
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB2 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB2 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB2.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H1_SUB2.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB2;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB2;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB3
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB3 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB3 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB3.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H1_SUB3.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB3;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H1_SUB3;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_SUB1
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_SUB1 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_SUB1 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_SUB1.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H2_SUB1.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_SUB1;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_SUB1;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB1
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB1 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB1 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB1.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H2_CM1_SUB1.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB1;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB1;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB2
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB2 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB2 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB2.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H2_CM1_SUB2.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB2;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB2;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB3
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB3 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB3 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB3.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H2_CM1_SUB3.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB3;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H2_CM1_SUB3;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceClearMessage_OBL250805006H3
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H3 == null)
				{
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H3 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H3.EM_MessageText = OBL250805006_CargoStatusAdviceClear_OBL250805006H3.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H3;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceClearMessage_OBL250805006H3;

		protected CMRCARSTMessage OBL250805006_CargoStatusAdviceHeldMessage_OBL250805006H3
		{
			get
			{
				if (fOBL250805006_CargoStatusAdviceHeldMessage_OBL250805006H3 == null)
				{
					fOBL250805006_CargoStatusAdviceHeldMessage_OBL250805006H3 = Factory.New<CMRCARSTMessage>();
					fOBL250805006_CargoStatusAdviceHeldMessage_OBL250805006H3.EM_MessageText = OBL250805006_CargoStatusAdviceHeld_OBL250805006H3.Replace("\r\n", "");
				}
				return fOBL250805006_CargoStatusAdviceHeldMessage_OBL250805006H3;
			}
		}
		CMRCARSTMessage fOBL250805006_CargoStatusAdviceHeldMessage_OBL250805006H3;

		protected CFSLoadListConsol CreateDepotJobOBL250805006()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = OBL250805006_OceanBillNum;

			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestScenarioLloydsNumber, TestScenarioVoyageNum).PK;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = OBL250805006_ContainerNumber;
			CFSShipment shipment1 = consol.Shipments.AddNew();
			CFSShipment shipment2 = consol.Shipments.AddNew();
			CFSShipment shipment3 = consol.Shipments.AddNew();

			shipment1.JS_HouseBill = OBL250805006_HouseBill1;
			shipment2.JS_HouseBill = OBL250805006_HouseBill2;
			shipment3.JS_HouseBill = OBL250805006_HouseBill3;

			shipment1.JS_OuterPacks = 10;
			shipment2.JS_OuterPacks = 20;
			shipment3.JS_OuterPacks = 30;
			return consol;
		}

		#endregion

		#region OBL250805007

		#region Message Text

		protected const string OBL250805007_UnderbondApprovalAdvice_TECU4493024 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+BEF6 473F 8B5:1+32'
DTM+9:20050831100959991455:ZZZ'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000100++BX:185:95'
RFF+AAQ:TECU4493024'
PCI+28+N/M'
UNT+17+000001'";
		protected const string OBL250805007_UnderbondApprovalAdvice_BreakBulk = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+415A CA00 CIB5:1+32'
DTM+9:20050831101000272641:ZZZ'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
PAC+0000020++BX:185:95'
RFF+MB:OBL250805007'
PCI+28+N/M'
FTX+AAA+++BREAK BULK TIMBER'
UNT+18+000001'";

		protected const string OBL250805007_ExpectedCargoArrival_TECU4493024 = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+3A0A BE7A CIB5:1+32'
DTM+9:20050831101000171132:ZZZ'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++FCL:67:95'
PAC+0000100++BX:185:95'
RFF+AAQ:TECU4493024'
PCI+28+N/M'
UNT+17+000001'";
		protected const string OBL250805007_ExpectedCargoArrival_BreakBulk = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+4GBE 532A CIB5:1+32'
DTM+9:20050831101000731431:ZZZ'
TDT+20+936++11++++8811924::11'
TDT+1++ROA'
LOC+5+9122P::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++B/B:67:95'
PAC+0000020++BX:185:95'
RFF+MB:OBL250805007'
PCI+28+N/M'
FTX+AAA+++BREAK BULK TIMBER'
UNT+18+000001'";

		#endregion

		protected const string OBL250805007_OceanBillNum = "OBL250805007";
		protected const string OBL250805007_ContainerNumber = "TECU4493024";
		protected const string OBL250805007_HouseBill1 = "";

		protected const string OBL250805007_ContainerMode = "OTH";
		protected const string OBL250805007_ConsolType = "";

		protected CMRUBMREQRMessage OBL250805007_UnderbondApprovalAdviceMessage_TECU4493024
		{
			get
			{
				if (fOBL250805007_UnderbondApprovalAdviceMessage_TECU4493024 == null)
				{
					fOBL250805007_UnderbondApprovalAdviceMessage_TECU4493024 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805007_UnderbondApprovalAdviceMessage_TECU4493024.EM_MessageText = OBL250805007_UnderbondApprovalAdvice_TECU4493024.Replace("\r\n", "");
				}
				return fOBL250805007_UnderbondApprovalAdviceMessage_TECU4493024;
			}
		}
		CMRUBMREQRMessage fOBL250805007_UnderbondApprovalAdviceMessage_TECU4493024;

		protected CMRUBMREQRMessage OBL250805007_UnderbondApprovalAdviceMessage_BreakBulk
		{
			get
			{
				if (fOBL250805007_UnderbondApprovalAdviceMessage_BreakBulk == null)
				{
					fOBL250805007_UnderbondApprovalAdviceMessage_BreakBulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805007_UnderbondApprovalAdviceMessage_BreakBulk.EM_MessageText = OBL250805007_UnderbondApprovalAdvice_BreakBulk.Replace("\r\n", "");
				}
				return fOBL250805007_UnderbondApprovalAdviceMessage_BreakBulk;
			}
		}
		CMRUBMREQRMessage fOBL250805007_UnderbondApprovalAdviceMessage_BreakBulk;

		protected CMRUBMREQRMessage OBL250805007_CargoArrivalAdviceMessage_TECU4493024
		{
			get
			{
				if (fOBL250805007_CargoArrivalAdviceMessage_TECU4493024 == null)
				{
					fOBL250805007_CargoArrivalAdviceMessage_TECU4493024 = Factory.New<CMRUBMREQRMessage>();
					fOBL250805007_CargoArrivalAdviceMessage_TECU4493024.EM_MessageText = OBL250805007_ExpectedCargoArrival_TECU4493024.Replace("\r\n", "");
				}
				return fOBL250805007_CargoArrivalAdviceMessage_TECU4493024;
			}
		}
		CMRUBMREQRMessage fOBL250805007_CargoArrivalAdviceMessage_TECU4493024;

		protected CMRUBMREQRMessage OBL250805007_CargoArrivalAdviceMessage_BreakBulk
		{
			get
			{
				if (fOBL250805007_CargoArrivalAdviceMessage_BreakBulk == null)
				{
					fOBL250805007_CargoArrivalAdviceMessage_BreakBulk = Factory.New<CMRUBMREQRMessage>();
					fOBL250805007_CargoArrivalAdviceMessage_BreakBulk.EM_MessageText = OBL250805007_ExpectedCargoArrival_BreakBulk.Replace("\r\n", "");
				}
				return fOBL250805007_CargoArrivalAdviceMessage_BreakBulk;
			}
		}
		CMRUBMREQRMessage fOBL250805007_CargoArrivalAdviceMessage_BreakBulk;

		#endregion

		#region CARST Messages Status

		protected CFSLoadListConsol CreateFCLLoadListConsol(ZString oceanBill, ZString lloydsNumber, ZString voyageNumber)
		{
			CFSLoadListConsol result = GetImportFCLConsol();
			result.JK_MasterBillNum = oceanBill;

			Transport transport = result.Transports[0];
			transport.JW_JX = CreateSailing(lloydsNumber, voyageNumber).PK;
			return result;
		}

		protected CFSLoadListConsol CreateBreakBulkLoadListConsol(ZString oceanBill, ZString lloydsNumber, ZString voyageNumber)
		{
			CFSLoadListConsol result = GetImportBBKConsol();
			result.JK_MasterBillNum = oceanBill;

			Transport transport = result.Transports[0];
			transport.JW_JX = CreateSailing(lloydsNumber, voyageNumber).PK;

			return result;
		}

		protected CFSLoadListConsol CreateLCLLoadListConsol(ZString oceanBill, ZString lloydsNumber, ZString voyageNumber)
		{
			CFSLoadListConsol result = GetImportLCLConsol();
			result.JK_MasterBillNum = oceanBill;

			Transport transport = result.Transports[0];
			transport.JW_JX = CreateSailing(lloydsNumber, voyageNumber).PK;
			return result;
		}

		#region ACS Seized

		protected CFSLoadListConsol CreateContainerACSSeizedDepotJob()
		{
			CFSLoadListConsol consol = CreateLCLLoadListConsol("RTMQC830100", "9227340", "5039");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "01497261");
			CFSContainer container = (CFSContainer)AddContainerToConsol(consol, "CASU0800480");
			ExpectedArrivalUnderbond(container);
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ShipmentACSSeized
		{
			get
			{
				if (fCargoStatusAdviceMessage_ShipmentACSSeized == null)
				{
					fCargoStatusAdviceMessage_ShipmentACSSeized = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ShipmentACSSeized.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+298D 66IB G2GF:1+8'
DTM+9:20051115184329692068:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:ACSSEIZED'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:SEIZED'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:SEIZED UNDER ACS DIRECTION'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:SEIZURE ASSESSMENT PLACED AND ACQUITTED TO TRIGGER STATUS CALCULATION - REFER G COOMBES X 42 6045'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+5039++11++++9227340::11'
LOC+12+AUMEL::6'
NAD+MR+FGA469X::95'
NAD+UD+65007252333::95'
RFF+ABO:S00014381/LVE1::1'
RFF+MB:RTMQC830100'
RFF+BH:01497261'
RFF+AAQ:CASU0800480'
DOC+1'
PAC+++LCL:67:95'
UNT+20+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ShipmentACSSeized;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ShipmentACSSeized;

		#endregion

		#region Quarantine Break Bulk Inspect

		protected CFSLoadListConsol CreateContainerQuarantineBreakBulkInspectDepotJob()
		{
			CFSLoadListConsol consol = CreateBreakBulkLoadListConsol("KB0511303", "8714669", "91616");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_BreakBulkInspect
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_BreakBulkInspect == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_BreakBulkInspect = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_BreakBulkInspect.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3B4I 3IJE 0JB5:1+8'
DTM+9:20051014140558356325:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:INSPECTION - BREAK BULK INSPECTION'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+91616++11++++8714669::11'
LOC+12+AUBNE::6'
LOC+4+FK85D::95'
NAD+MR+FGH674T::95'
NAD+UD+50003815441::95'
RFF+ABO:28BG CF50 E715::1'
RFF+MB:KB0511303'
DOC+1'
PAC+++B/B:67:95'
FTX+AAA+++USED CONTRUCTION MACHINERY MODEL 980G'
UNT+21+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_BreakBulkInspect;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_BreakBulkInspect;

		#endregion

		#region Quarantine Cleaning

		protected CFSLoadListConsol CreateContainerQuarantineCleaningDepotJob()
		{
			CFSLoadListConsol consol = CreateBreakBulkLoadListConsol("KKLUCV0135538", "8025214", "119");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_Cleaning
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_Cleaning == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_Cleaning = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_Cleaning.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+437E 9ADH D06F:1+8'
DTM+9:20051110124406478784:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Pending AQIS Action (Quarantine)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Inspection - Re-inspection of goods'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Other treatments - Cleaning as Directed'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+119++11++++8025214::11'
LOC+12+AUMEL::6'
LOC+4+FL53K::95'
NAD+MR+FFY669C::95'
NAD+UD+74057873426::95'
RFF+ABO:B00003761/1/SYD1::1'
RFF+ABT:AAAPN6X9K'
RFF+MB:KKLUCV0135538'
DOC+1'
PAC+++B/B:67:95'
PAC+0000002++PK:185:95'
FTX+AAA+++USED MOTOR VEHICLESNO.348 NISSAN SKYLINEER33-070660NO.355 NISSAN SKYLINEER33-053411'
FTX+MKS+++;DICENO.348355MADE IN JAPAN'
UNT+26+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_Cleaning;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_Cleaning;

		#endregion

		#region QuarantineFreshProduce

		protected CFSLoadListConsol CreateContainerQuarantineFreshProduceDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("TST036482", "8505989", "521S");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "MAEU5673763");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_FreshProduce
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_FreshProduce == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_FreshProduce = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_FreshProduce.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3089 EB4E EH15:1+8'
DTM+9:20051015151930548235:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:INSPECTION - FRESH PRODUCE INSPECT'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+521S++11++++8505989::11'
LOC+12+AUFRE::6'
LOC+4+FE70K::95'
NAD+MR+FFF369C::95'
NAD+UD+19089105537::95'
RFF+ABO:B00002487/1/PER4::4'
RFF+MB:TST036482'
RFF+AAQ:MAEU5673763'
DOC+1'
PAC+++FCL:67:95'
PAC+0001880++CT:185:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_FreshProduce;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_FreshProduce;

		#endregion

		#region QuarantineFood

		protected CFSLoadListConsol CreateContainerQuarantineFoodDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("PH0074301", "7614317", "495SB");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "INBU5229935");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_Food
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_Food == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_Food = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_Food.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2IF5 CJF6 6C65:1+8'
DTM+9:20051019110526447501:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (FOOD)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:FOOD PROGRAM - RELEASE AFTER INSPECTION'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+495SB++11++++7614317::11'
LOC+12+AUSYD::6'
LOC+4+FM27N::95'
NAD+MR+FGH496N::95'
NAD+UD+92175318026::95'
RFF+ABO:B00001038/1/MEL1::3'
RFF+MB:PH0074301'
RFF+AAQ:INBU5229935'
DOC+1'
PAC+++FCL:67:95'
PAC+0000991++CT:185:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_Food;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_Food;

		#endregion

		#region QuarantineFoodTestAndHold

		protected CFSLoadListConsol CreateContainerQuarantineFoodTestAndHoldDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("OOLU63820724", "8913681", "032S");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "DE00063731");
			AddContainerToConsol(consol, "TTNU2848895");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodTestAndHold
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodTestAndHold == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodTestAndHold = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodTestAndHold.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+40A6 G24F 5265:1+8'
DTM+9:20051017142322494833:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (FOOD)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:FOOD PROGRAM - TEST AND HOLD'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+032S++11++++8913681::11'
LOC+12+AUMEL::6'
LOC+4+FK78H::95'
NAD+MR+FFP496M::95'
NAD+UD+79439911849::95'
RFF+ABO:S00017222/1/MEL1::1'
RFF+MB:OOLU63820724'
RFF+BH:DE00063731'
RFF+AAQ:TTNU2848895'
DOC+1'
PAC+++FCL:67:95'
PAC+0001080++CT:185:95'
UNT+23+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodTestAndHold;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodTestAndHold;

		#endregion

		#region QuarantineFoodHOTestAndHold

		protected CFSLoadListConsol CreateContainerQuarantineFoodHOTestAndHoldDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("HLCULIV050922852", "8505989", "521S");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "HLXU4764776");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodHOTestAndHold
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodHOTestAndHold == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodHOTestAndHold = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodHOTestAndHold.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2IGD 77DH 9EB5:1+8'
DTM+9:20051014093252454641:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (FOOD)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:FOOD PROGRAM - H/O TEST AND HOLD'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+521S++11++++8505989::11'
LOC+12+AUFRE::6'
LOC+4+FE70K::95'
NAD+MR+FGC647G::95'
NAD+UD+31004265721::95'
RFF+ABO:B00006405/4/MEL1::1'
RFF+MB:HLCULIV050922852'
RFF+AAQ:HLXU4764776'
DOC+1'
PAC+++FCX:67:95'
PAC+0001134++PK:185:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodHOTestAndHold;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFoodHOTestAndHold;

		#endregion

		#region QuarantineFumigation

		protected CFSLoadListConsol CreateContainerQuarantineFumigationDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("500104867", "8505989", "521S");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "317527100");
			AddContainerToConsol(consol, "TOLU4856321");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFumigation
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFumigation == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFumigation = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFumigation.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+283F 12AF 0265:1+8'
DTM+9:20051017104244144881:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:FUMIGATION - CH3BR 48GM3 24 HR 21C OR ABOVE'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+521S++11++++8505989::11'
LOC+12+AUFRE::6'
LOC+4+FE70K::95'
NAD+MR+FGG733H::95'
NAD+UD+69073665595::95'
RFF+ABO:L00000449/SYD2::3'
RFF+MB:500104867'
RFF+BH:317527100'
RFF+AAQ:TOLU4856321'
DOC+1'
PAC+++FCL:67:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFumigation;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_QuarantineFumigation;

		#endregion

		#region QuarantineInspection

		protected CFSLoadListConsol CreateContainerQuarantineInspectionDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("HDMUAYAS1022979", "9248679", "109");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "HDMU2431879");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalQuarantineInspection
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalQuarantineInspection == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalQuarantineInspection = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalQuarantineInspection.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2575 J649 B4B5:1+8'
DTM+9:20051016123817635955:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+109++11++++9248679::11'
LOC+12+AUFRE::6'
LOC+4+FE92E::95'
NAD+MR+FGF643F::95'
NAD+UD+47000565766::95'
RFF+ABO:H00000264/SYD1::1'
RFF+MB:HDMUAYAS1022979'
RFF+AAQ:HDMU2431879'
DOC+1'
PAC+++FCL:67:95'
UNT+19+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalQuarantineInspection;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalQuarantineInspection;

		#endregion

		#region Quarantine Steam Clean After Inspection

		protected CFSLoadListConsol CreateContainerQuarantineSteamCleaningDepotJob()
		{
			CFSLoadListConsol consol = CreateBreakBulkLoadListConsol("KKLUCVY105703", "8511691", "001");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_SteamCleaning
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_SteamCleaning == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_SteamCleaning = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_SteamCleaning.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3F28 27G2 87BF:1+8'
DTM+9:20051103185958133438:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Pending AQIS Action (Quarantine)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:Other treatments - Steam Cleaning (inspect after)'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+001++11++++8511691::11'
LOC+12+AUSYD::6'
LOC+4+FM22K::95'
NAD+MR+FFN364M::95'
NAD+UD+20007392789::95'
RFF+ABO:ICJ-02-26224::2'
RFF+MB:KKLUCVY105703'
DOC+1'
PAC+++B/B:67:95'
PAC+0000001++PK:185:95'
FTX+AAA+++1 UNIT USED MOTOR VEHICLE ------------------ NISSAN SKYLINE CH/NO. BNR32-007958 ` FREIGHT COLLECT `'
FTX+MKS+++;SMRZ;SYDNEY;MADE IN JAPAN;BNR32-007958'
UNT+23+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_SteamCleaning;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_SteamCleaning;

		#endregion

		#region ContainerQuarantineTailgate

		protected CFSLoadListConsol CreateContainerQuarantineTailgateDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("SGHI49400", "8505989", "521S");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "MSKU3243397");
			shipment.JS_OuterPacks = 671;
			shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Piece;
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Tailgate
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Tailgate == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Tailgate = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Tailgate.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+4976 DDF9 JH15:1+8'
DTM+9:20051015095121511898:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:INSPECTION - TAILGATE'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+521S++11++++8505989::11'
LOC+12+AUFRE::6'
LOC+4+FE70K::95'
NAD+MR+FFG737N::95'
NAD+UD+69073665595::95'
RFF+ABO:ICJ-01-35663 /L1::2'
RFF+MB:SGHI49400'
RFF+AAQ:MSKU3243397'
DOC+1'
PAC+++FCL:67:95'
PAC+0000671++CT:185:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Tailgate;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Tailgate;

		#endregion

		#region ContainerQuarantineTailgateRural

		protected CFSLoadListConsol CreateContainerQuarantineTailgateRuralDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("KLISPKG036363", "8505989", "521S");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "KKFU1090500");
			shipment.JS_OuterPacks = 671;
			shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Piece;
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_TailgateRural
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_TailgateRural == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_TailgateRural = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_TailgateRural.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+HIGB 0CI5 265:1+8'
DTM+9:20051017123127917751:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:INSPECTION - TAILGATE - RURAL DESTINATION'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+521S++11++++8505989::11'
LOC+12+AUFRE::6'
LOC+4+FE70K::95'
NAD+MR+FFG737N::95'
NAD+UD+69073665595::95'
RFF+ABO:ICJ-01-35637 /L5::5'
RFF+MB:KLISPKG036363'
RFF+AAQ:KKFU1090500'
DOC+1'
PAC+++FCL:67:95'
PAC+0000267++PK:185:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_TailgateRural;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_TailgateRural;

		#endregion

		#region ContainerQuarantineUnpack

		protected CFSLoadListConsol CreateContainerQuarantineUnpackDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("MSCUCP416062", "8419702", "31A");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "INBU3323475");
			shipment.JS_OuterPacks = 20;
			shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Bag;
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Unpack
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Unpack == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Unpack = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Unpack.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1JIF HC5E H215:1+8'
DTM+9:20051013173645290181:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:INSPECTION - INSPECT (UNPACK)'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+31A++11++++8419702::11'
LOC+12+AUFRE::6'
LOC+4+FE92E::95'
NAD+MR+FGC673R::95'
NAD+UD+88102989220::95'
RFF+ABO:B00006881/5/BNE2::2'
RFF+MB:MSCUCP416062'
RFF+AAQ:INBU3323475'
DOC+1'
PAC+++FCL:67:95'
PAC+0000020++SJ:185:95'
UNT+22+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Unpack;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_Unpack;

		#endregion

		#region ContainerQuarantineUnpackFood

		protected CFSLoadListConsol CreateContainerQuarantineUnpackFoodDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("HDMUGJAU3106061", "7614317", "495SB");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "");
			AddContainerToConsol(consol, "HDMU6202628");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_UnpackFood
		{
			get
			{
				if (fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_UnpackFood == null)
				{
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_UnpackFood = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_UnpackFood.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+31H3 5EA3 5265:1+8'
DTM+9:20051017125821824763:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++SUPPLEMENTARY INFORMATION:YES'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (QUARANTINE)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:INSPECTION - INSPECT (UNPACK)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:PENDING AQIS ACTION (FOOD)'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:FOOD PROGRAM - AUTOMATIC RELEASE'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+495SB++11++++7614317::11'
LOC+12+AUMEL::6'
LOC+4+FK78H::95'
NAD+MR+FGF643F::95'
NAD+UD+47000565766::95'
RFF+ABO:H00000154/SYD1::1'
RFF+MB:HDMUGJAU3106061'
RFF+AAQ:HDMU6202628'
DOC+1'
PAC+++FCL:67:95'
UNT+25+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_UnpackFood;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_ContainerConditionalClear_Quarantine_UnpackFood;

		#endregion

		#region Shipment Transhipment

		protected CFSLoadListConsol CreateShipmentTranshipmentdDepotJob()
		{
			CFSLoadListConsol consol = CreateFCLLoadListConsol("CSCD002", "7104673", "932");
			CFSShipment shipment = (CFSShipment)AddShipmentToConsol(consol, "P20051239");
			AddContainerToConsol(consol, "IFTU7823492");
			return consol;
		}

		protected CMRCARSTMessage CargoStatusAdviceMessage_Shipment_Transhipment
		{
			get
			{
				if (fCargoStatusAdviceMessage_Shipment_Transhipment == null)
				{
					fCargoStatusAdviceMessage_Shipment_Transhipment = Factory.New<CMRCARSTMessage>();
					fCargoStatusAdviceMessage_Shipment_Transhipment.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1153 H489 C665:1+8'
DTM+9:20050915131348897235:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:TRANSHIP'
FTX+AHN+++TRANSHIPMENT NUMBER:AAAA4WTGX'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+932++11++++7104673::11'
LOC+12+AUSYD::6'
LOC+4+9913C::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:S00040879/SYD1::1'
RFF+MB:CSCD002'
RFF+BH:P20051239'
RFF+AAQ:IFTU7823492'
DOC+1'
PAC+++LCL:67:95'
UNT+18+000001'".Replace("\r\n", "");
				}
				return fCargoStatusAdviceMessage_Shipment_Transhipment;
			}
		}
		CMRCARSTMessage fCargoStatusAdviceMessage_Shipment_Transhipment;

		#endregion

		#endregion

		#endregion

		#region Underbonds

		protected CusUnderbond ExpectedArrivalUnderbond(CFSContainer container)
		{
			CFSContainerWrapper wrapper = CFSContainerWrapper.Load(container);
			CusUnderbond result = wrapper.Underbonds.AddNew();
			//			CusUnderbond Result = (CusUnderbond)Container.Factory.New(typeof(CusUnderbond));
			//			Result.C4_ParentTableCode = JobContainerSchema.Constants.Prefix;
			//			Result.C4_ParentID = Container.PK;
			result.C4_DestinationPremiseID = GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID;
			//Result.C4_OA_DestinationAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			return result;
		}

		#endregion

		#region Sailing

		protected virtual ZString TestSailingOriginPort
		{
			get { return "SGSIN"; }
		}

		protected virtual ZDateTime TestSailingOriginDepartureDate
		{
			get { return ZDateTime.Today.AddDays(-12); }
		}

		protected JobSailing TestSailing(BusinessObjectFactory factory)
		{
			JobVoyage voyage = factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = TestScenarioVoyageNum;
			voyage.JV_RV_NKVessel = (factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, TestScenarioLloydsNumber))).RV_FK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = TestSailingOriginPort;
			origin.JA_E_DEP = TestSailingOriginDepartureDate;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.GenerateSailings();
			JobSailing result = voyage.Sailings[0];
			return result;
		}

		#endregion

		protected JobSailing TestSailing()
		{
			return TestSailing(Factory);
		}

		#region Setup

		protected const string TestPremiseID = "9914N";

		protected override void SetUp()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = TestPremiseID;
			base.SetUp();
		}

		#endregion
	}
}
