using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManTranHeadExporter))]
	public class CusSeaManTranHeadActualArrivalExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "AARSEA"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Actual Arrival Report - Sea"; }
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
			arrival.BA_ArrivalPortATA = new ZDateTime(2007, 1, 18, 9, 5, 0);
			arrival.BA_DischargeIndicator = false;
			arrival.BA_SendersMessageReference = "SENDREF1";
			arrival.BA_StevadoreID = "STEVO1";
			arrival.BA_BerthCode = "BERTH1";

			CusSeaManArrivalPort arrival2 = head.Arrivals.AddNew();
			arrival2.BA_OA_CTOAddress = cTOAddress2.PK;
			arrival2.BA_RL_NKArrivalPort = "AUMEL";
			arrival2.BA_IsFirstArrival = false;
			arrival2.BA_ArrivalPortATA = new ZDateTime(2007, 1, 18, 14, 45, 10);
			arrival2.BA_DischargeIndicator = true;
			arrival2.BA_SendersMessageReference = "SENDREF2";
			arrival2.BA_StevadoreID = "STEVO2";
			arrival2.BA_BerthCode = "BERTH2";

			CusSeaManArrivalPort arrival3 = head.Arrivals.AddNew();
			arrival3.BA_OA_CTOAddress = cTOAddress2.PK;
			arrival3.BA_RL_NKArrivalPort = "AUPER";
			arrival3.BA_IsFirstArrival = false;
			arrival3.BA_ArrivalPortETA = new ZDateTime(2007, 1, 18, 14, 45, 10);
			arrival3.BA_DischargeIndicator = true;
			arrival3.BA_SendersMessageReference = "SENDREF2";
			arrival3.BA_StevadoreID = "STEVO2";
			arrival3.BA_BerthCode = "BERTH2";

			CusSeaManArrivalPort arrival4 = head.Arrivals.AddNew();
			arrival4.BA_OA_CTOAddress = cTOAddress2.PK;
			arrival4.BA_RL_NKArrivalPort = "AUPER";
			arrival4.BA_IsFirstArrival = false;
			arrival4.BA_ArrivalPortATA = new ZDateTime(2007, 1, 18, 14, 45, 10);
			arrival4.BA_DischargeIndicator = true;
			arrival4.BA_SendersMessageReference = "SENDREF2";
			arrival4.BA_StevadoreID = "STEVO2";
			arrival4.BA_BerthCode = "BERTH2";
			arrival4.ActualArrivalResponseStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			return new CusSeaManTranHeadExporter(head, "AA");
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,121234,999111,AUSYD,20070117,2205,STEVO1,5678X,BERTH1,SENDREF1\r\n" +
"123 441,test@example.com,121234,999111,AUMEL,20070118,0345,STEVO2,9876A,BERTH2,SENDREF2\r\n";
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
