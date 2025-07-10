using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public abstract class UPEDataLineTest : TestCaseWithFactory
	{
		public void TestSetManifestType()
		{
			Mapper.SetManifestType();
			AssertEquals(ManifestTypeList.Codes.ConsolidationExportSubManifest, Header.ED_ManifestType);
		}

		public void TestSetTransportMode()
		{
			Mapper.SetTransportMode();
			AssertEquals(Enterprise.Core.Constants.TransportModes.Air, Header.ED_TransportMode);
		}

		public void TestSetPortOfDeparture()
		{
			InsertRequiredDataToDB();
			Mapper.SetPortOfDeparture("AU", "____");
			AssertEquals("LocalPortCode doesn't exist", ZString.Empty, Header.ED_RL_NKPortOfDeparture);
			Mapper.SetPortOfDeparture("AU", "9639");
			AssertEquals("Setting PortOfDeparture, 1st attempt", ExpectedUNLOCO1, Header.ED_RL_NKPortOfDeparture);
			Mapper.SetPortOfDeparture("NZ", "8989");
			AssertEquals("2nd attempt won't overwrite", ExpectedUNLOCO1, Header.ED_RL_NKPortOfDeparture);
		}

		public void TestSetPortOfDestination()
		{
			InsertRequiredDataToDB();
			Mapper.SetPortOfDeparture("NZ", "----");
			AssertEquals("LocalPortCode doesn't exist", ZString.Empty, Header.ED_RL_NKPortOfDestination);
			Mapper.SetPortOfDestination("NZ", "8989");
			AssertEquals("Setting PortOfDestination, 1st attempt", ExpectedUNLOCO2, Header.ED_RL_NKPortOfDestination);
			Mapper.SetPortOfDestination("AU", "9639");
			AssertEquals("2nd attempt won't overwrite", ExpectedUNLOCO2, Header.ED_RL_NKPortOfDestination);
		}

		public void TestSetCountryOfDestination()
		{
			Mapper.SetCountryOfDestination("AU");
			AssertEquals("AU", Header.ED_RN_NKCountryOfDestination);
		}

		public void TestSetAirwayBill()
		{
			Mapper.SetAirWayBill("AWB101101101");
			AssertEquals(ZString.Empty, Header.ED_AirWayBill);
			Mapper.SetAirWayBill("AWB10110110");
			AssertEquals("AWB10110110", Header.ED_AirWayBill);
			Mapper.SetAirWayBill("12345");
			AssertEquals("AWB10110110", Header.ED_AirWayBill);
		}

		public void TestPopulateHeaderFromLine1()
		{
			InsertRequiredDataToDB();
			Mapper.internalLine = Line1;
			Mapper.Process();
			AssertEquals(ManifestTypeList.Codes.ConsolidationExportSubManifest, Header.ED_ManifestType);
			AssertEquals(Enterprise.Core.Constants.TransportModes.Air, Header.ED_TransportMode);
			AssertEquals(ExpectedUNLOCO1, Header.ED_RL_NKPortOfDeparture);
			AssertEquals(ExpectedUNLOCO2, Header.ED_RL_NKPortOfDestination);
			AssertEquals("NZ", Header.ED_RN_NKCountryOfDestination);
			AssertEquals("06311283926", Header.ED_AirWayBill);
		}

		public void TestPopulateHeaderFromLine2()
		{
			InsertRequiredDataToDB();
			Mapper.internalLine = Line2;
			Mapper.Process();
			AssertEquals(ManifestTypeList.Codes.ConsolidationExportSubManifest, Header.ED_ManifestType);
			AssertEquals(Enterprise.Core.Constants.TransportModes.Air, Header.ED_TransportMode);
			AssertEquals("NZAKL", Header.ED_RL_NKPortOfDeparture);
			AssertEquals(ZString.Empty, Header.ED_RL_NKPortOfDestination);
			AssertEquals(ZString.Empty, Header.ED_RN_NKCountryOfDestination);
			AssertEquals("06311283926", Header.ED_AirWayBill);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Header = Factory.New<ExportCustomsManifestHeader>();
			AUGuid = new ZGuid("E4EB6E97-AA78-4C46-BF86-35B7FBB5FB0E");
			NZGuid = new ZGuid("7A546412-B30C-46BB-AC7A-E60FC6120D1F");
		}

		protected void InsertRequiredDataToDB()
		{
			// Check if LocoMaps required for testing are available, otherwise add new ones
			RefLocoConverter converter = new RefLocoConverter(Factory);
			ExpectedUNLOCO1 = converter.GetUNLOCOString(UPEDataLine.Constants.RefLocoSystemUsage, "9639", AUGuid);
			if (ExpectedUNLOCO1.IsEmpty)
			{
				MasterFiles.Business.RefLocoMap locoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
				PopulateRefLocoMap(locoMap, "9639", AUGuid, "AUSYD");
				ExpectedUNLOCO1 = "AUSYD";
			}

			ExpectedUNLOCO2 = converter.GetUNLOCOString(UPEDataLine.Constants.RefLocoSystemUsage, "8989", NZGuid);
			if (ExpectedUNLOCO2.IsEmpty)
			{
				MasterFiles.Business.RefLocoMap locoMap = Factory.New<MasterFiles.Business.RefLocoMap>();
				PopulateRefLocoMap(locoMap, "8989", NZGuid, "NZAKL");
				ExpectedUNLOCO2 = "NZAKL";
			}

			Factory.Save();
		}

		protected void PopulateRefLocoMap(MasterFiles.Business.RefLocoMap locoMap, ZString localPortCode, ZGuid countryGuid, ZString uNLOCO)
		{
			locoMap.RY_LocalPortCode = localPortCode.SubstringSafe(0, locoMap.RY_LocalPortCodeInfo.MaxLength);
			locoMap.RY_RN = countryGuid;
			locoMap.RY_SystemUsage = UPEDataLine.Constants.RefLocoSystemUsage;
			locoMap.RY_RL_NKLocoPort = uNLOCO;
		}

		protected UPEDataLine Mapper;
		protected ExportCustomsManifestHeader Header;
		protected ZGuid AUGuid;
		protected ZGuid NZGuid;
		protected ZString ExpectedUNLOCO1;
		protected ZString ExpectedUNLOCO2;
		protected const string Line1 = "AU9639NZ898904091406311283926               100000 AU9639040914      SB141       040914                                                                              SB141                                                                                                                                                                                                                ";
		protected const string Line2 = "NZ8989      04091406311283926   DA46107HLPWQ200000A46107HLPWQ           N 1 12   KGSNAU          AUDNNNN N         AUD              07041             AUD         AUD29190     AUDN0NN   NSYD2S09SEP200412 KGS         AUDD1     NN NN  NN  N AUD           AUD    T1                      09SEP20041800           29190      P/PNLY       22   KGSNNB4996817340        SB141   N N 1   N ";
		#endregion
	}
}
