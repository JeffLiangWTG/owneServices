using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAirArrivalExporter))]
	public class CMRAirArrivalImpendingArrivalExporterTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "IARAIR"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Impending Arrival Report - Air"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			OrgHeader cTO = OrgHeader.New(Factory);
			cTO.OH_FullName = "I am a CTO";
			OrgAddress cTOAddress = cTO.Addresses.AddNew();
			cTO.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, "5678X");

			OrgHeader cTO2 = OrgHeader.New(Factory);
			cTO2.OH_FullName = "I am another CTO";
			OrgAddress cTOAddress2 = cTO2.Addresses.AddNew();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "QF123";
			voyage.JV_FlightDate = new ZDateTime(2007, 1, 1, 03, 46, 00);

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_A_DEP = new ZDateTime(2007, 1, 1, 03, 45, 00);

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_OA_ArrivalCTOAddress = cTOAddress.PK;
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination1.JB_E_ARV = new ZDateTime(2007, 1, 3, 10, 15, 00);
			destination1.JB_SendersMessageReference = "SENDREF1";

			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_OA_ArrivalCTOAddress = cTOAddress2.PK;
			destination2.JB_RL_NKPortOfDischarge = "AUMEL";
			destination2.JB_E_ARV = new ZDateTime(2007, 1, 3, 18, 30, 00);
			destination2.JB_SendersMessageReference = "SENDREF2";

			CustomsJobVoyageWrapper voyageWrapper = CustomsJobVoyageWrapper.Load(Factory, voyage.PK);

			return new CMRAirArrivalExporter(voyageWrapper, "IA");
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,,QF123,NZAKL,20061231,1445,AUSYD,AUSYD,20070102,2315,5678X,YES,SENDREF1\r\n" +
"123 441,test@example.com,,QF123,NZAKL,20061231,1445,AUSYD,AUMEL,20070103,0730,,,SENDREF2\r\n";
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
