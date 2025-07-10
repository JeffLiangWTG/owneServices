using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRUnderbondExporter))]
	public class CMRUnderbondExporterSeaManOBLDetailTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "UBond"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Underbond Movement"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			var header = Factory.New<CusSeaManOBLHeader>();
			CusSeaManOBLDetail oBL = header.Details.AddNew();
			var transportHeader = Factory.New<CusSeaManTranHead>();
			header.BO_BT = transportHeader.PK;
			transportHeader.BT_LloydsIMO = "1234567";
			transportHeader.BT_VoyageNum = "4321";
			header.BO_OceanBill = "OBL";
			oBL.BD_LineCargoType = "FCL";
			oBL.BD_ContainerNumber = "OCLU1234567";
			oBL.BD_GoodsDescription = "GOODS DESCRIPION";
			header.BO_ConsignorName = "CONSIGNOR";
			header.BO_ConsigneeName = "CONSIGNEE";
			header.BO_ConsigneeAddress1 = "STREET";
			header.BO_ConsigneeAddress2 = "STREET2";
			header.BO_ConsigneeCity = "CITY";
			header.BO_ConsigneeState = "STATE";
			header.BO_ConsigneePostCode = "1234";
			header.BO_RL_NKDischargePort = "AUPER";
			oBL.BD_HazardousIndicator = true;
			oBL.BD_SACIndicator = true;
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ModeOfMovement = "MOV";
			underbond.C4_OriginPremiseID = "1234X";
			underbond.C4_DestinationPremiseID = "5678A";
			underbond.C4_PiecesManifested = 7;
			underbond.C4_SendersMessageReference = "U999999";
			underbond.C4_ParentID = oBL.PK;
			underbond.C4_ParentTableCode = CusSeaManOBLDetailSchema.Constants.Prefix;

			return new CMRUnderbondExporter(underbond);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,1234567,4321,OBL,,FCL,OCLU1234567,GOODS DESCRIPION,CONSIGNOR,CONSIGNEE,STREET STREET2 CITY STATE 1234,AUPER,YES,YES,MOV,,1234X,5678A,7,U999999\r\n";
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
