using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRUnderbondExporter))]
	public class CMRUnderbondExporterSCAContainerTest : CMRDataExporterCSVTest
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
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_LloydsIMO = "1234567";
			oceanBill.CB_Voyage = "4321";
			oceanBill.CB_OceanBill = "OBL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUPER";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.CN_ContainerNumber = "OCLU1234567";

			var pivot = container.Pivots.AddNew();
			pivot.CV_CN = container.PK;
			pivot.CV_GoodsDescription = "GOODS DESCRIPION";
			pivot.CV_HazardousGoods = true;
			pivot.CV_IsSAC = true;

			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.C4_ModeOfMovement = "MOV";
			underbond.C4_OriginPremiseID = "1234X";
			underbond.C4_DestinationPremiseID = "5678A";
			underbond.C4_PiecesManifested = 7;
			underbond.C4_SendersMessageReference = "U999999";

			return new CMRUnderbondExporter(underbond);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,1234567,4321,OBL,,FCL,OCLU1234567,GOODS DESCRIPION,,,,AUPER,YES,YES,MOV,,1234X,5678A,7,U999999\r\n";
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
