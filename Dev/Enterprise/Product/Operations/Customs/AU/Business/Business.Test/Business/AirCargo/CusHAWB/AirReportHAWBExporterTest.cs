using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirReportHAWBExporter))]
	sealed class AirReportHAWBExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName => "Cargo";

		protected override string ExpectedMailSubject => "Contingency Cargo Report";

		protected override bool IsDocManagerSupported => true;

		protected override CMRDataExporterCSV GetNewExporter()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "EM18N";
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "My, Consignee";
			consignee.MainAddress.OA_Address1 = "18 Henricks Avenue,";
			consignee.MainAddress.OA_City = "Newington,";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_PostCode = "2127";

			var mAWB = Factory.New<CusMAWB>();
			var hAWB = Factory.New<CusHAWB>();
			hAWB.CS_CM = mAWB.PK;

			mAWB.CM_FlightNo = "123";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 11, 26, 10, 45, 00);
			mAWB.CM_MAWB = "918525";
			mAWB.CM_RL_NKDischargePort = "AUMEL";

			hAWB.CS_HAWB = "992233";
			hAWB.CS_ConsignorName = "Manually Entered ,Consignor";
			hAWB.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			hAWB.CS_GoodsDescription = "These are, my \"goods ok\"?";
			hAWB.CS_IsSelfAssessedClearance = true;
			hAWB.IsDocuments = true;
			hAWB.CS_ShipmentType = "DOC";
			hAWB.CS_RL_NKDestination = "AUSYD";
			hAWB.CS_PiecesManifested = 7;

			return new AirReportHAWBExporter(hAWB);
		}

		protected override string ExpectedCSVResult => "123 441,test@example.com,123,20051126,918525,992233,,,These are my 'goods ok'?,Manually Entered Consignor,My Consignee,18 Henricks Avenue Newington NSW 2127,,,,AUMEL,AUSYD,Y,,,Y,EM18N,7,\r\n";

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
	}
}
