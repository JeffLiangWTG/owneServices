using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobDeclarationTransportSupporterTest : TransportSupporterTestCase<JobDeclarationTransportSupporter>
	{
		public void TestUpdateDeclarationTransportData()
		{
			CreateTestLocoMapping();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var routingParent = (ITransportParent)declaration;
			var transport = declaration.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "CNBJS";
			transport.JW_RL_NKLoadPort = "DESSS";
			transport.JW_LegOrder = 0;
			AssertEquals("DESSS", declaration.JE_LastPortBeforeEntry);
			AssertEquals("0001", declaration.JE_CNLastPortBeforeEntry);
			transport.JW_RL_NKLoadPort = "GBSSS";
			AssertEquals("GBSSS", declaration.JE_LastPortBeforeEntry);
			AssertEquals("0002", declaration.JE_CNLastPortBeforeEntry);
			var transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKDiscPort = "CN111";
			transport1.JW_RL_NKLoadPort = "DESSS";
			transport1.JW_LegOrder = 1;
			AssertEquals("DESSS", declaration.JE_LastPortBeforeEntry);
			AssertEquals("0001", declaration.JE_CNLastPortBeforeEntry);
			transport1.Delete();
			AssertEquals("GBSSS", declaration.JE_LastPortBeforeEntry);
			AssertEquals("0002", declaration.JE_CNLastPortBeforeEntry);
			transport1 = declaration.Transports.AddNew();
			transport1.JW_RL_NKDiscPort = "CN111";
			transport1.JW_RL_NKLoadPort = "DESSS";
			transport1.JW_LegOrder = 1;
			AssertEquals("DESSS", declaration.JE_LastPortBeforeEntry);
			AssertEquals("0001", declaration.JE_CNLastPortBeforeEntry);
			transport.JW_LegOrder = 2;
			AssertEquals("GBSSS", declaration.JE_LastPortBeforeEntry);
			AssertEquals("0002", declaration.JE_CNLastPortBeforeEntry);
		}

		protected override ZString TestingCountry => Core.Constants.CountryCodes.China;

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<JobDeclaration>();
			return parent.TransportSupporter;
		}

		void CreateTestLocoMapping()
		{
			CreateLocoMap("0001", "DESSS", CNLocoMapSystemUsageList.Codes.CustomsPortCodeList);
			CreateLocoMap("0002", "GBSSS", CNLocoMapSystemUsageList.Codes.CustomsPortCodeList);
			Factory.Save();
		}

		void CreateLocoMap(string localPort, string unLoco, string usage)
		{
			var locoMap = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap.RY_LocalPortCode = localPort;
			locoMap.RY_RL_NKLocoPort = unLoco;
			locoMap.RY_SystemUsage = usage;
			locoMap.RY_RN = Core.Constants.CountryGuids.China;
		}
	}
}
