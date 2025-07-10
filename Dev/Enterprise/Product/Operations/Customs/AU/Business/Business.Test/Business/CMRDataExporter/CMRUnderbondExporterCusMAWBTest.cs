using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRUnderbondExporter))]
	public class CMRUnderbondExporterCusMAWBTest : CMRDataExporterCSVTest
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
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_FlightNo = "QF32";
			mAWB.CM_MAWB = "08111112222";
			mAWB.CM_RL_NKDischargePort = "AUMEL";
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ModeOfMovement = "MOV";
			underbond.C4_OriginPremiseID = "1234X";
			underbond.C4_DestinationPremiseID = "5678A";
			underbond.C4_PiecesManifested = 7;
			underbond.C4_SendersMessageReference = "U999999";
			underbond.C4_ParentID = mAWB.PK;
			underbond.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;

			return new CMRUnderbondExporter(underbond);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,,QF32,08111112222,,,,,,,,AUMEL,,,MOV,,1234X,5678A,7,U999999\r\n";
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
