using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAIRIARRMessage))]
	sealed class CMRAIRIARRMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetReport()
		{
			ZString report = Message.GetReport();
			AssertEquals("Flight Number", true, report.Contains("Flight Number: 123"));
			AssertEquals("Flight Date", true, report.Contains("Flight Date: "));
			AssertEquals("Port of Loading", true, report.Contains("Port of Loading: USLAX"));
			AssertEquals("Port of Loading", true, report.Contains("Port of Loading: NZAKL"));
			AssertEquals("Port of Discharge", true, report.Contains("Port of Discharge: AUSYD"));
			AssertEquals("Port of Discharge", true, report.Contains("Port of Discharge: AUBNE"));
		}

		CMRAIRIARRMessage fMessage;
		CMRAIRIARRMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.JV_FlightDate = ZDateTime.Today.AddDays(-2);
					voyage.JV_VoyageFlight = "123";

					VoyageOrigin voyOrigin1 = voyage.Origins.AddNew();
					voyOrigin1.JA_RL_NKPortOfLoading = "USLAX";

					VoyageOrigin voyOrigin2 = voyage.Origins.AddNew();
					voyOrigin2.JA_RL_NKPortOfLoading = "NZAKL";

					VoyageDestination voyDest1 = voyage.Destinations.AddNew();
					voyDest1.JB_RL_NKPortOfDischarge = "AUSYD";

					VoyageDestination voyDest2 = voyage.Destinations.AddNew();
					voyDest2.JB_RL_NKPortOfDischarge = "AUBNE";

					fMessage = (CMRAIRIARRMessage)voyage.Messages.AddNew(typeof(CMRAIRIARRMessage));

					ZString response =
						"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRIARR+DOC MSG NUMBER:VERS+11'" +
						"FTX+AHN+++CLEAR:CLEAR'NAD+MR+OUT MSG RECIPIENT::95'" +
						"RFF+ABO:SEND REF::VERS'RFF+ACW:AIRIAR'RFF+AFM:5'ERP+1'UNT+11+1'";

					fMessage.EM_MessageText = response;
					fMessage.EM_LinkedObject = voyage;
				}
				return fMessage;
			}
		}
	}
}
