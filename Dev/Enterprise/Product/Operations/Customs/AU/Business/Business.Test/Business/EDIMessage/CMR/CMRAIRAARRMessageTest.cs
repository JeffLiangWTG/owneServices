using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAIRAARRMessage))]
	sealed class CMRAIRAARRMessageTest : CMRCUSRESMessageTest
	{
		public void TestGetReport()
		{
			ZString report = Message.GetReport();
			AssertEquals("Flight Number", true, report.Contains("Flight Number: 123"));
			AssertEquals("Flight Date", true, report.Contains("Flight Date: "));
			AssertEquals("Port of Discharge", true, report.Contains("Port of Discharge: AUSYD"));
		}

		CMRAIRAARRMessage fMessage;
		CMRAIRAARRMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.JV_FlightDate = ZDateTime.Today.AddDays(-2);
					voyage.JV_VoyageFlight = "123";

					VoyageDestination voyDest = Factory.New<VoyageDestination>();
					voyDest.JB_JV = voyage.PK;
					voyDest.JB_RL_NKPortOfDischarge = "AUSYD";

					fMessage = (CMRAIRAARRMessage)voyDest.Messages.AddNew(typeof(CMRAIRAARRMessage));

					ZString response =
						"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRAARR+DOC MSG NUMBER:VERS+11'" +
						"FTX+AHN+++CLEAR:CLEAR'NAD+MR+OUT MSG RECIPIENT::95'" +
						"RFF+ABO:SEND REF::VERS'RFF+ACW:AIRAAR'RFF+AFM:5'ERP+1'UNT+11+1'";

					fMessage.EM_MessageText = response;
					fMessage.EM_LinkedObject = voyDest;
				}
				return fMessage;
			}
		}
	}
}
