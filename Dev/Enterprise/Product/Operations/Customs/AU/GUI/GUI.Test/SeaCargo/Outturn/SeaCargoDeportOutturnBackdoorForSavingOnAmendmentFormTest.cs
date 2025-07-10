using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm))]
	sealed class SeaCargoDeportOutturnBackdoorForSavingOnAmendmentFormTest : ZFormBasherTest
	{
		public void TestSeaCargoDepotOutturnBackdoorForSavingOnAmendmentFormOpensWhenSave()
		{
			var header = Factory.NewWithValidTestData<CusOutturnHeader>();
			header.C6_LloydsIMO = "7619410";
			header.C6_OutturningPremiseID = "9912J";
			header.C6_VoyageNum = "3339";
			var outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = "FCL";
			outturn.C5_ContainerNumber = "CN1234";
			outturn.C5_CargoReceiptDate = ZDateTime.Today;
			outturn.C5_CargoUnpackDate = ZDateTime.Today;
			outturn.C5_OuterPacks = 3;
			outturn.C5_OuterPackUnits = "PK";
			outturn.C5_PackagesOutturned = 1;
			outturn.C5_PackagesUnits = "PK";
			outturn.C5_GoodsDescription = "Magazines";
			header.OutturnStatus.Code = Common.AU.CMR.CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			var sutTransmitMsg1 = Factory.New<CMRSEAOUTMessage>();
			sutTransmitMsg1.EM_LinkedObject = header;
			sutTransmitMsg1.EM_MessageSubType = "ORG";
			sutTransmitMsg1.EM_ReceiveTransmit = "TRX";
			sutTransmitMsg1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-50);
			sutTransmitMsg1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+263:::SEAOUT+O00000428/CMT1:4+4
NAD+VW+41065894724::95
TDT+20+3339++11++++7619410::11
LOC+4+9912J::95
CNI++:::I
RFF+AAQ:OCLU1233510
GID+1
RFF+ACU:SH
GID+1
RFF+BH:H3337C7
GID+1
RFF+MB:OBL3337
GIS+N:62:95
GIS+U:63:95
GIS+U:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20140205:102
DTM+420:0204:401
DTM+570:20140205:102
DTM+570:0204:401
GID+1
PAC+2
PAC+++PK:185:95
PAC+++LCL:67:95
UNT+30+1
".Replace("\r\n", "'");
			header.Messages.Add(sutTransmitMsg1);
			var sutReceiveMsg1 = Factory.New<CMRSEAOUTRMessage>();
			sutReceiveMsg1.EM_LinkedObject = header;
			sutReceiveMsg1.EM_MessageSubType = "CLR";
			sutReceiveMsg1.EM_ReceiveTransmit = "RCV";
			sutReceiveMsg1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-49);
			sutReceiveMsg1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN
BGM+961:::SEAOUTR+2AF5 A882 H0F4:001+11
NAD+MR+AAA374M::95
RFF+ACW:SEAOUT
RFF+AFM:4
RFF+ABO:O00000428/CMT1::004
DTM+310:20140205040823:204
ERP+1
ERC+ADVICE:80:95
ERC+MS5203:6:95
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS
CNT+55:000
UNT+13+000001
".Replace("\r\n", "'");
			header.Messages.Add(sutReceiveMsg1);
			outturn.C5_OuterPacks = 5;
			using (var form = new SeaCargoDepotOutturnForm(header))
			{
				form.Show();
				form.FireSaveButton();
				AssertType(typeof(SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		protected override System.Windows.Forms.Form GetFormToBashCore() => new SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm(new DeferredCusOutturnHeaderSavingOptions(Factory.New<CusOutturnHeader>()));
	}
}
