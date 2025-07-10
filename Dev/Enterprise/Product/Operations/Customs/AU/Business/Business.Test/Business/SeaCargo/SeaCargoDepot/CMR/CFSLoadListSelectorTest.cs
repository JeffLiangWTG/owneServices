using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CFSLoadListSelector))]
	sealed class CFSLoadListSelectorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateConsolPK()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateIncorrectSailing();
			Factory.Save();

			CFSLoadListSelector selector = new CFSLoadListSelector(VesselName, Voyage);
			selector.ConsolPK = consol.PK;
			AssertHasErrors("Consol in incorrect sailing", selector.ConsolPKInfo);

			CFSLoadListConsol consol2 = Factory.New<CFSLoadListConsol>();

			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = CreateIncorrectSailing();
			transport2.JW_Vessel = VesselName;
			transport2.JW_VoyageFlight = Voyage;
			Factory.Save();

			selector.ConsolPK = consol2.PK;
			AssertNoErrors("Consol Selector should be error free now", selector.ConsolPKInfo);
		}

		const string Voyage = "123";
		const string VesselName = "SOUTHERN CROSS MARU";

		const string TestVesselName = "VESSEL";
		const string TestVoyageNum = "1425";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CFSLoadListSelector(TestVesselName, TestVoyageNum);
		}

		ZGuid CreateIncorrectSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Enterprise.Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "432";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("CALIFORNIA STAR", Factory).First().RV_FK;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			VoyageDestination dest = voyage.Destinations.AddNew();
			dest.JB_RL_NKPortOfDischarge = "AUSYD";
			dest.JB_E_ARV = ZDateTime.Now;
			voyage.GenerateSailings();
			return voyage.Sailings[0].PK;
		}
	}
}
