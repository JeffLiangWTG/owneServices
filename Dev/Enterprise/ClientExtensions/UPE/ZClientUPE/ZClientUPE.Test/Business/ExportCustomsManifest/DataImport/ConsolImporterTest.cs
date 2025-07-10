using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class ConsolImporterTest : UPEDataLineTest
	{
		public void TestInitialiseConsolImporter()
		{
			FlightDetailsLine line = new FlightDetailsLine(null, null, null);
			ConsolImporter importer = new ConsolImporter(line, Factory);
			AssertEquals(Factory, importer.Factory);
			AssertEquals(line, importer.Line);
		}

		public void TestImportToConsol()
		{
			InsertRequiredDataToDB();
			ConsolImporter importer = new ConsolImporter(fMapper, Factory);
			ForwardingConsol consol = importer.ImportToConsol();
			ForwardingConsol expectedConsol = GenerateExpectedConsol(ExpectedUNLOCO1, ZString.Empty, "AUFJ", "810", "60024602", new ZDateTime(2004, 8, 21), new ZDateTime(2004, 8, 23));
			AssertEquals(1, consol.Transports.Count);
			AssertConsols(expectedConsol, consol);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Mapper = new FlightDetailsLine(null, Header, Line1);
			fMapper = (FlightDetailsLine)Mapper;
		}

		protected void AssertConsols(ForwardingConsol expectedConsol, ForwardingConsol generatedConsol)
		{
			AssertEquals(expectedConsol.JK_AgentType, generatedConsol.JK_AgentType);
			AssertEquals(expectedConsol.JK_TransportMode, generatedConsol.JK_TransportMode);
			AssertEquals(expectedConsol.JK_ConsolMode, generatedConsol.JK_ConsolMode);
			AssertEquals(expectedConsol.JK_JX_JA_RL_NKPortOfLoading, generatedConsol.JK_JX_JA_RL_NKPortOfLoading);
			AssertEquals(expectedConsol.JK_JX_JB_RL_NKPortOfDischarge, generatedConsol.JK_JX_JB_RL_NKPortOfDischarge);
			AssertEquals(expectedConsol.JK_JX_JV_VoyageFlight, generatedConsol.JK_JX_JV_VoyageFlight);
			AssertEquals(expectedConsol.JK_AWBServiceLevel, generatedConsol.JK_AWBServiceLevel);
			AssertEquals(expectedConsol.JK_IsNeutralMaster, generatedConsol.JK_IsNeutralMaster);
			AssertEquals(expectedConsol.MasterBillAirlinePrefix, generatedConsol.MasterBillAirlinePrefix);
			AssertEquals(expectedConsol.MasterBillMAWB, generatedConsol.MasterBillMAWB);
			AssertEquals(expectedConsol.JK_PrepaidCollect, generatedConsol.JK_PrepaidCollect);
			AssertEquals(expectedConsol.JK_JX_JA_E_DEP, generatedConsol.JK_JX_JA_E_DEP);
			AssertEquals(expectedConsol.JK_JX_JB_E_ARV, generatedConsol.JK_JX_JB_E_ARV);
		}

		protected ForwardingConsol GenerateExpectedConsol(ZString portOfLoading, ZString portOfDischarge, ZString flightNumber, ZString masterBillAirlinePrefix, ZString masterBillMAWB, ZDateTime departureDate, ZDateTime arrivalDate)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = portOfLoading;
			transport.JW_RL_NKDiscPort = portOfDischarge;
			transport.JW_VoyageFlight = flightNumber;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_IsNeutralMaster = ZBool.True;
			consol.MasterBillAirlinePrefix = masterBillAirlinePrefix;
			consol.MasterBillMAWB = masterBillMAWB;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			transport.JW_ETD = departureDate;
			transport.JW_ETA = arrivalDate;
			return consol;
		}

		FlightDetailsLine fMapper;
		protected new const string Line1 = "AU0000      04082381060024602               100000 AU9639040821      AUFJ        040821       040823    JUPITER                                                      AUFJ                                                                                                                                                                                                                 ";
		#endregion
	}
}
