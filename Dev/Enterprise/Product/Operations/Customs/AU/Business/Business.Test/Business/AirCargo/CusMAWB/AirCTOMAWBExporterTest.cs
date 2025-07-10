using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirCTOMAWBExporter))]
	sealed class AirCTOMAWBExporterTest : CMRDataExporterCSVTest
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

			CTOCusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "992233";
			hAWB1.CS_ConsignorName = "Manually Entered ,Consignor";
			hAWB1.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			hAWB1.CS_GoodsDescription = "These are my\r\ngoods ok?";
			hAWB1.CS_IsSelfAssessedClearance = true;
			hAWB1.CS_ShipmentType = "DOC";
			hAWB1.CS_RL_NKDischargePort = "USNYN";
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 12;
			mAWB.CM_RL_NKRoutePort1 = "SGSIN";
			mAWB.CM_RL_NKRoutePort2 = "AUPER";

			CTOCusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "1233";
			hAWB2.CS_ConsignorName = "SOme Other, Consignor, OK?";
			hAWB2.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			hAWB2.CS_GoodsDescription = "SOme other goods";
			hAWB2.CS_IsSelfAssessedClearance = false;
			hAWB2.CS_ShipmentType = "STD";
			hAWB2.CS_RL_NKDestination = "AUMEL";
			hAWB2.CS_RL_NKOrigin = "INBOM";
			hAWB2.CS_PiecesManifested = 13;

			var exporter = new AirCTOMAWBExporter(mAWB);
			exporter.AdditionalData.OriginPremise = "AUBNE";
			return exporter;
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,123,20051126,992233,,,,These are my goods ok?,Manually Entered Consignor,My Consignee,18 Henricks Avenue Newington NSW 2127,,SGSIN AUPER,,USNYN,AUSYD,,,,N,AUBNE,12,\r\n" +
"123 441,test@example.com,123,20051126,1233,,,,SOme other goods,SOme Other Consignor OK?,My Consignee,18 Henricks Avenue Newington NSW 2127,,INBOM SGSIN AUPER,,,AUMEL,,,,N,AUBNE,13,\r\n";
			}
		}

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
