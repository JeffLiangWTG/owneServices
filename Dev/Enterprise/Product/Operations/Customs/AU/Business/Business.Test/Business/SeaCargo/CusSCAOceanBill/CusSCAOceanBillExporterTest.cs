using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBillExporter))]
	sealed class CusSCAOceanBillExporterTest : CMRDataExporterCSVTest
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
			OrgHeader depot = OrgHeader.New(Factory);
			depot.OH_FullName = "I am a depot";
			OrgAddress depotAddress = depot.Addresses.AddNew();
			depot.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "1234A");

			OrgHeader cTO = OrgHeader.New(Factory);
			cTO.OH_FullName = "I am a CTO";
			OrgAddress cTOAddress = cTO.Addresses.AddNew();
			cTO.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "5678X");

			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			ocean.CB_LloydsIMO = "121234";
			ocean.CB_Voyage = "999111";
			ocean.CB_OceanBill = "OOO111";
			ocean.CB_RL_NKPortOfDischarge = "AUBNE";
			ocean.CB_RL_NKPortOfLoading = "USLAX";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress = depotAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = cTOAddress.PK;
			ocean.CB_ParentId = consol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			CusSCAHouse house1 = ocean.HouseBills.AddNew();
			house1.CA_HouseBill = "Z1123";
			house1.CA_ConsignorName = "My consignor";
			house1.CA_ConsigneeName = "El Scorcho";
			house1.CA_ConsigneeAddress1 = "Sleepy";
			house1.CA_ConsigneeAddress2 = "People should";
			house1.CA_ConsigneeSuburb = "I AM";
			house1.CA_ConsigneePostcode = "2015";
			house1.CA_NotifyName = "NOTIFY ME";
			house1.CA_MasterHouseBill = "SUB MASTER";
			house1.CA_BGMReference = "BGM REF";
			house1.CA_RL_NK_PortOfDestination = ZString.Empty;

			CusSCAHouse house2 = ocean.HouseBills.AddNew();
			house2.CA_HouseBill = "Z2222";
			house2.CA_ConsignorName = "Supreme";
			house2.CA_ConsigneeName = "Pizza";
			house2.CA_ConsigneeAddress1 = "Tired";
			house2.CA_ConsigneeAddress2 = "CMR";
			house2.CA_ConsigneeSuburb = "LONG";
			house2.CA_ConsigneePostcode = "2015";
			house2.CA_NotifyName = "NOTIFY THEM TOO";
			house2.CA_RL_NK_PortOfDestination = "AUXXX";

			CusSCAContainer container1 = ocean.Containers.AddNew();
			container1.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.CN_ContainerNumber = "OCLU9991110";

			CusSCAContainer container2 = ocean.Containers.AddNew();
			container2.CN_ContainerMode = Core.Constants.ContainerModes.LCL;
			container2.CN_ContainerNumber = "OCLU3333310";

			CusSCAPivot house1Pivot1 = house1.Pivot.AddNew();
			house1Pivot1.CV_CN = container1.PK;
			house1Pivot1.CV_GoodsDescription = "Goods in Container 1";
			house1Pivot1.CV_HazardousGoods = true;
			house1Pivot1.CV_IsSAC = true;
			house1Pivot1.CV_PackageCount = 8;

			CusSCAPivot house1Pivot2 = house1.Pivot.AddNew();
			house1Pivot2.CV_CN = container2.PK;
			house1Pivot2.CV_GoodsDescription = "Goods in Container 2";
			house1Pivot2.CV_HazardousGoods = true;
			house1Pivot2.CV_PackageCount = 9;

			CusSCAPivot house2Pivot1 = house2.Pivot.AddNew();
			house2Pivot1.CV_CN = container2.PK;
			house2Pivot1.CV_GoodsDescription = "More in\r\nContainer 2";
			house2Pivot1.CV_IsSAC = true;
			house2Pivot1.CV_PackageCount = 10;

			return new CusSCAOceanBillExporter(ocean);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,121234,999111,OOO111,Z1123,SUB MASTER,OCLU9991110,Goods in Container 1,My consignor,El Scorcho,Sleepy People should I AM 2015,FCL,,USLAX,AUBNE,,,Y,NOTIFY ME,Y,1234A,8,BGM REF\r\n" +
"123 441,test@example.com,121234,999111,OOO111,Z1123,SUB MASTER,OCLU3333310,Goods in Container 2,My consignor,El Scorcho,Sleepy People should I AM 2015,LCL,,USLAX,AUBNE,,,Y,NOTIFY ME,N,1234A,9,BGM REF\r\n" +
"123 441,test@example.com,121234,999111,OOO111,Z2222,,OCLU3333310,More in Container 2,Supreme,Pizza,Tired CMR LONG 2015,LCL,,USLAX,AUBNE,AUXXX,,N,NOTIFY THEM TOO,Y,1234A,10,\r\n";
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
