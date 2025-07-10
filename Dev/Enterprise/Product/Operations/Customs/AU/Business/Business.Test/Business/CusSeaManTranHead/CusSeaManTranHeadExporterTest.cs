using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHeadExporter))]
	public class CusSeaManTranHeadExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "Cargo"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Cargo Report"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			CusSeaManTranHead head = Factory.New<CusSeaManTranHead>();
			head.BT_LloydsIMO = "121234";
			head.BT_VoyageNum = "999111";

			OrgHeader cTO = OrgHeader.New(Factory);
			cTO.OH_FullName = "I am a CTO";
			OrgAddress cTOAddress = cTO.Addresses.AddNew();
			cTO.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "5678X");
			CusSeaManArrivalPort arrival = head.Arrivals.AddNew();
			arrival.BA_OA_CTOAddress = cTOAddress.PK;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "My consignee";
			consignee.MainAddress.OA_Address1 = "Test Address 1";

			CusSeaManOBLHeader oBL1 = head.OceanBills.AddNew();
			oBL1.BO_OceanBill = "O111222";
			oBL1.BO_ConsigneeName = "My, consignee";
			oBL1.BO_ConsignorName = "Consignors are weird";
			oBL1.BO_RL_NKLoadPort = "USLAX";
			oBL1.BO_RL_NKDischargePort = "AUSYD";
			oBL1.BO_RL_NKDestinationPort = "AUMEL";
			oBL1.BO_SendersMessageReference = "SENDREF";
			oBL1.BO_OH_Consignee = consignee.PK;

			CusSeaManOBLDetail detail1 = oBL1.Details.AddNew();
			detail1.BD_GoodsDescription = "My first\r\ncontainer";
			detail1.BD_HazardousIndicator = true;
			detail1.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			detail1.BD_ContainerNumber = "111133332";
			detail1.BD_NoOfPacks = 50;

			CusSeaManOBLDetail detail2 = oBL1.Details.AddNew();
			detail2.BD_GoodsDescription = "My second detail";
			detail2.BD_HazardousIndicator = false;
			detail2.BD_LineCargoType = CMRCargoTypes.Codes.BreakBulk;
			detail2.BD_GrossWeight = 2000m;
			detail2.BD_GrossWeightUM = "KG";
			detail2.BD_CargoVolume = 10m;
			detail2.BD_CargoVolumeUM = "M3";
			detail2.BD_NoOfPacks = 51;

			CusSeaManOBLHeader oBL2 = head.OceanBills.AddNew();
			oBL2.BO_OceanBill = "O999111";
			oBL2.BO_ConsigneeName = "My, 2nd consignee";
			oBL2.BO_ConsignorName = "Consignors are nice people";
			oBL2.BO_RL_NKLoadPort = "USMEM";
			oBL2.BO_RL_NKDischargePort = "AUMEL";
			oBL2.BO_RL_NKDestinationPort = "AUSYD";

			CusSeaManOBLDetail detail3 = oBL2.Details.AddNew();
			detail3.BD_GoodsDescription = "My 3rd container";
			detail3.BD_LineCargoType = Core.Constants.ContainerModes.Bulk;
			detail3.BD_GrossWeight = 4000m;
			detail3.BD_GrossWeightUM = "AA";
			detail3.BD_CargoVolume = 20m;
			detail3.BD_CargoVolumeUM = "DD";
			detail3.BD_NoOfPacks = 52;

			CusSeaManOBLDetail detail4 = oBL2.Details.AddNew();
			detail4.BD_GoodsDescription = "Go away";
			detail4.BD_HazardousIndicator = true;
			detail4.BD_LineCargoType = Core.Constants.ContainerModes.LCL;
			detail4.BD_ContainerNumber = "325235";
			detail4.BD_NoOfPacks = 53;

			return new CusSeaManTranHeadExporter(head, "CR");
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,121234,999111,O111222,,,111133332,My first container,Consignors are weird,My consignee,MY CONSIGNEE TEST ADDRESS 1,,,USLAX,AUSYD,AUMEL,,Y,,N,5678X,50,SENDREF\r\n" +
"123 441,test@example.com,121234,999111,O111222,,,B/B 2000 KG 10 M3,My second detail,Consignors are weird,My consignee,MY CONSIGNEE TEST ADDRESS 1,,,USLAX,AUSYD,AUMEL,,N,,N,5678X,51,SENDREF\r\n" +
"123 441,test@example.com,121234,999111,O999111,,,BULK 4000 AA 20 DD,My 3rd container,Consignors are nice people,My 2nd consignee,,,,USMEM,AUMEL,AUSYD,,N,,N,5678X,52,\r\n" +
"123 441,test@example.com,121234,999111,O999111,,,325235,Go away,Consignors are nice people,My 2nd consignee,,,,USMEM,AUMEL,AUSYD,,Y,,N,5678X,53,\r\n";
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			currentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbStaff.CurrentUser.GS_EmailAddress = "test@example.com";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "123 441";
		}

		protected override void TearDown()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = currentUserEmailAddress;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = currentyCompanyABN;
			base.TearDown();
		}

		string currentUserEmailAddress;
		string currentyCompanyABN;

		#endregion
	}
}
