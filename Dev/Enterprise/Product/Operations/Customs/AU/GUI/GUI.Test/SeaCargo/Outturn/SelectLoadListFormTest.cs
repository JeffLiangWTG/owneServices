using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(SelectLoadListForm))]
	sealed class SelectLoadListFormTest : ZFormBasherTest
	{
		public void TestOKButtonWithErrors()
		{
			const string TestVesselName = "SOUTHERN CROSS MARU";
			const string TestVoyageNum = "1425";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var selector = new CFSLoadListSelector(TestVesselName, TestVoyageNum);
			var consol = Factory.New<CFSLoadListConsol>();
			var transport = consol.Transports[0];
			transport.JW_JX = CreateIncorrectSailing();
			selector.ConsolPK = consol.PK;
			using (var form = new SelectLoadListForm(selector))
			{
				form.OKButton1_Click(null, EventArgs.Empty);
				Assert("Last Message was error", UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var consol2 = Factory.New<CFSLoadListConsol>();
				var transport2 = consol2.Transports[0];
				transport2.JW_JX = CreateIncorrectSailing();
				transport2.JW_Vessel = TestVesselName;
				transport2.JW_VoyageFlight = TestVoyageNum;
				selector.ConsolPK = consol2.PK;
				Assert("Last Message was error", !UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		protected override Form GetFormToBashCore() => new SelectLoadListForm(new CFSLoadListSelector("VESSEL", "1425"));

		ZGuid CreateIncorrectSailing()
		{
			var vessel = RefVessel.LookupVesselByName("CALIFORNIA STAR", Factory).First();
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "432";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			var dest = voyage.Destinations.AddNew();
			dest.JB_RL_NKPortOfDischarge = "AUSYD";
			dest.JB_E_ARV = ZDateTime.Now;
			voyage.GenerateSailings();
			return voyage.Sailings[0].PK;
		}
	}
}
