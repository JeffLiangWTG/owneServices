using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCTOHAWBExporter))]
	sealed class AirCTOHAWBExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName => "Cargo";

		protected override string ExpectedMailSubject => "Contingency Cargo Report";

		protected override bool IsDocManagerSupported => true;

		protected override CMRDataExporterCSV GetNewExporter()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "My, Consignee";
			consignee.MainAddress.OA_Address1 = "18 Henricks Avenue,";
			consignee.MainAddress.OA_City = "Newington,";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_PostCode = "2127";

			var mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "123";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 11, 26, 10, 45, 00);
			mAWB.CM_MAWB = "918525";
			mAWB.CM_RL_NKDischargePort = "AUBNE";
			mAWB.CM_RL_NKRoutePort1 = "SGSIN";
			mAWB.CM_RL_NKRoutePort2 = "AUMEL";

			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_HAWB = "992233";
			hAWB.CS_ConsignorName = "Manually Entered ,Consignor";
			hAWB.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			hAWB.CS_GoodsDescription = "These are my\r\ngoods ok?";
			hAWB.CS_IsSelfAssessedClearance = true;
			hAWB.CS_ShipmentType = "DOC";
			hAWB.CS_RL_NKOrigin = "INBOM";
			hAWB.CS_RL_NKDischargePort = "USNYN";
			hAWB.CS_RL_NKDestination = "AUSYD";
			hAWB.CS_PiecesManifested = 12;

			var exporter = new AirCTOHAWBExporter(hAWB);
			exporter.AdditionalData.OriginPremise = "AUBNE";
			return exporter;
		}

		protected override string ExpectedCSVResult => "123 441,test@example.com,123,20051126,992233,,,,These are my goods ok?,Manually Entered Consignor,My Consignee,18 Henricks Avenue Newington NSW 2127,,INBOM SGSIN AUMEL,,USNYN,AUSYD,,,,N,AUBNE,12,\r\n";

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
