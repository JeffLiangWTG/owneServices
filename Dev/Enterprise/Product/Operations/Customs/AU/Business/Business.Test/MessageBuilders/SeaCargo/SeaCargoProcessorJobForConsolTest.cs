using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoProcessorJobForConsolTest : TestCaseWithFactory
	{
		public void TestIsAcceptable()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertEquals("consol.Transports.Count", 1, consol.Transports.Count);

			var seaCargoProcessorJobForConsol = new SeaCargoProcessorJobForConsol(consol);

			consol.Transports[0].JW_RL_NKDiscPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica)).RL_Code;
			Assert("Is not acceptable", !seaCargoProcessorJobForConsol.IsAcceptable);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Assert("Is not acceptable", !seaCargoProcessorJobForConsol.IsAcceptable);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Is not acceptable", !seaCargoProcessorJobForConsol.IsAcceptable);

			consol.Transports[0].JW_RL_NKDiscPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia)).RL_Code;
			Assert("Is acceptable", seaCargoProcessorJobForConsol.IsAcceptable);

			var oceanBill = seaCargoProcessorJobForConsol.OceanBill;
			Assert("oceanBill No Errors", !oceanBill.HasErrors);
			Assert("Is acceptable", seaCargoProcessorJobForConsol.IsAcceptable);

			oceanBill.CB_GB = ZGuid.Empty;
			Assert("oceanBill Has Errors", oceanBill.HasErrors);
			Assert("Is not acceptable", !seaCargoProcessorJobForConsol.IsAcceptable);
		}

		public void TestSynchronise()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.AddNew();

			var initialOceanBillCount = Factory.GetDatabaseCount(typeof(CusSCAOceanBill));
			var initialHouseCount = Factory.GetDatabaseCount(typeof(CusSCAHouse));

			_ = new SeaCargoProcessorJobForConsol(consol).OceanBill;
			Factory.Save();

			AssertEquals("CusSCAOceanBills should be 1", initialOceanBillCount + 1, Factory.GetDatabaseCount(typeof(CusSCAOceanBill)));
			AssertEquals("CusSCAHouses should be 1", initialHouseCount + 1, Factory.GetDatabaseCount(typeof(CusSCAHouse)));
		}

		public void TestJobNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "MB111";
			var seaCargoProcessorJobForConsol = new SeaCargoProcessorJobForConsol(consol);
			AssertEquals("Job Number is from Consol", "MB111", seaCargoProcessorJobForConsol.JobNumber);

			seaCargoProcessorJobForConsol.OceanBill.CB_OceanBill = "OB222";
			AssertEquals("Job Number is from Ocean Bill", "OB222", seaCargoProcessorJobForConsol.JobNumber);
		}
	}
}
