using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRUnderbondExporter))]
	public class CMRUnderbondExporterCTOCusHAWBTest : CMRDataExporterCSVTest
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
			var mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "QF32";
			mAWB.CM_RL_NKDischargePort = "AUMEL";
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "08112344321";
			hAWB.CS_GoodsDescription = "GOODS DESC.";
			hAWB.CS_ConsignorName = "THE CONSIGNOR";
			hAWB.CS_ConsigneeName = "THE CONSIGNEE";
			hAWB.CS_ConsigneeStreet = "STREET";
			hAWB.CS_ConsigneeStreet2 = "STREET 2";
			hAWB.CS_ConsigneeCity = "CITY";
			hAWB.CS_ConsigneeState = "STATE";
			hAWB.CS_ConsigneePostcode = "2222";
			hAWB.CS_IsSelfAssessedClearance = true;
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ModeOfMovement = "MOV";
			underbond.C4_OriginPremiseID = "1234X";
			underbond.C4_DestinationPremiseID = "5678A";
			underbond.C4_PiecesManifested = 7;
			underbond.C4_SendersMessageReference = "U999999";
			underbond.C4_ParentID = hAWB.PK;
			underbond.C4_ParentTableCode = CusHAWBSchema.Constants.Prefix;

			return new CMRUnderbondExporter(underbond);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,,QF32,08112344321,,,,GOODS DESC.,THE CONSIGNOR,THE CONSIGNEE,STREET STREET 2 CITY STATE 2222,AUMEL,,YES,MOV,,1234X,5678A,7,U999999\r\n";
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
