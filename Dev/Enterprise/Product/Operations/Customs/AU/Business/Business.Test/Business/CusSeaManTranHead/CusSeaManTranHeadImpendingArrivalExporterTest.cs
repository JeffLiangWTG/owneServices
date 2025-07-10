using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHeadExporter))]
	public class CusSeaManTranHeadImpendingArrivalExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "IARSEA"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Impending Arrival Report - Sea"; }
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

			OrgHeader cTO2 = OrgHeader.New(Factory);
			cTO2.OH_FullName = "I am another CTO";
			OrgAddress cTOAddress2 = cTO2.Addresses.AddNew();
			cTO2.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "9876A");

			head.BT_RL_NKPortOfLastForeignPort = "NZAKL";
			head.BT_PortOfLastForeignPortATD = new ZDateTime(2007, 1, 15, 7, 45, 00);

			CusSeaManArrivalPort arrival = head.Arrivals.AddNew();
			arrival.BA_OA_CTOAddress = cTOAddress.PK;
			arrival.BA_RL_NKArrivalPort = "AUSYD";
			arrival.BA_IsFirstArrival = true;
			arrival.BA_ArrivalPortETA = new ZDateTime(2007, 1, 18, 9, 5, 0);
			arrival.BA_DischargeIndicator = false;
			arrival.BA_SendersMessageReference = "SENDREF1";

			CusSeaManArrivalPort arrival2 = head.Arrivals.AddNew();
			arrival2.BA_OA_CTOAddress = cTOAddress2.PK;
			arrival2.BA_RL_NKArrivalPort = "AUMEL";
			arrival2.BA_IsFirstArrival = false;
			arrival2.BA_ArrivalPortETA = new ZDateTime(2007, 1, 18, 14, 45, 10);
			arrival2.BA_DischargeIndicator = true;
			arrival2.BA_SendersMessageReference = "SENDREF2";

			return new CusSeaManTranHeadExporter(head, "IA");
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,121234,999111,NZAKL,20070114,1845,AUSYD,AUSYD,20070117,2205,5678X,,SENDREF1\r\n" +
"123 441,test@example.com,121234,999111,NZAKL,20070114,1845,AUSYD,AUMEL,20070118,0345,9876A,YES,SENDREF2\r\n";
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
