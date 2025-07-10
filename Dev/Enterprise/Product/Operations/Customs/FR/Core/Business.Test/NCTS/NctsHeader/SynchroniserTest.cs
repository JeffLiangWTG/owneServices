using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	class SynchroniserTest : TestCaseWithFactory
	{
		public void TestSyncFromShipment()
		{
			using (EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupConsolAndShipment(TransportTypeList.Codes.Air);

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

				var nctsLine = nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsLine.BY_RN_NKCountryOfDispatch = ZString.Empty;

				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("Prerequisite: synchronization was successful.", "S0001", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals("PortOfDispatch should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.PortOfDispatch);
				AssertEquals("BH_RL_NKImportLoadPort should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("BM_RL_NKForeignDestPort should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("BM_RL_NKDestinationPort should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals("BM_TransportAtDeparture should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("BM_RN_NKTransportAtDepartureCountry should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry);
				AssertEquals("BM_TOLCarrierID should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("BM_TOLCarrierCode should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierCode);
				AssertEquals("BM_ExportTransportMode should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_ExportTransportMode);

				AssertEquals("BM_InlandTransportMode should be equal 3 by default when synchronising from Shipment.", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
			}

			using (EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				SetupConsolAndShipment(TransportTypeList.Codes.Air);

				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

				var nctsLine = nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsLine.BY_RN_NKCountryOfDispatch = ZString.Empty;

				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("Prerequisite: synchronization was successful.", "S0001", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals("PortOfDispatch should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.PortOfDispatch);
				AssertEquals("BH_RL_NKImportLoadPort should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("BM_RL_NKForeignDestPort should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("BM_RL_NKDestinationPort should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals("BM_TransportAtDeparture should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("BM_RN_NKTransportAtDepartureCountry should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry);
				AssertEquals("BM_TOLCarrierID should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("BM_TOLCarrierCode should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierCode);
				AssertEquals("BM_ExportTransportMode should be left empty when synchronising from Shipment.", ZString.Empty, nctsHeader.MovementHeader.BM_ExportTransportMode);

				AssertEquals("BM_InlandTransportMode should be equal 3 by default when synchronising from Shipment.", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
			}
		}

		public void TestSyncFromConsol()
		{
			using (EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupConsolAndShipment(TransportTypeList.Codes.Air);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = consol.PK;
				nctsHeader.BH_ParentTableCode = consol.TablePrefix;

				var nctsLine = nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsLine.BY_RN_NKCountryOfDispatch = ZString.Empty;

				nctsHeader.Synchroniser.Synchronise(true);
				AssertEquals("Prerequisite: synchronization was successful.", "C0001", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals("PortOfDispatch should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.PortOfDispatch);
				AssertEquals("BH_RL_NKImportLoadPort should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("BM_RL_NKForeignDestPort should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("BM_RL_NKDestinationPort should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals("BM_TransportAtDeparture should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("BM_RN_NKTransportAtDepartureCountry should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry);
				AssertEquals("BM_TOLCarrierID should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("BM_TOLCarrierCode should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierCode);
				AssertEquals("BM_ExportTransportMode should be left empty when synchronising from Consol.", "3", nctsHeader.MovementHeader.BM_ExportTransportMode);

				AssertEquals("BM_InlandTransportMode should be equal 3 by default when synchronising from Consol.", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
			}

			using (EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				SetupConsolAndShipment(TransportTypeList.Codes.Air);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = consol.PK;
				nctsHeader.BH_ParentTableCode = consol.TablePrefix;

				var nctsLine = nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsLine.BY_RN_NKCountryOfDispatch = ZString.Empty;

				nctsHeader.Synchroniser.Synchronise(true);
				AssertEquals("Prerequisite: synchronization was successful.", "C0001", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals("PortOfDispatch should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.PortOfDispatch);
				AssertEquals("BH_RL_NKImportLoadPort should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("BM_RL_NKForeignDestPort should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("BM_RL_NKDestinationPort should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals("BM_TransportAtDeparture should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("BM_RN_NKTransportAtDepartureCountry should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry);
				AssertEquals("BM_TOLCarrierID should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("BM_TOLCarrierCode should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_TOLCarrierCode);
				AssertEquals("BM_ExportTransportMode should be left empty when synchronising from Consol.", ZString.Empty, nctsHeader.MovementHeader.BM_ExportTransportMode);

				AssertEquals("BM_InlandTransportMode should be equal 3 by default when synchronising from Consol.", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
			}
		}

		public void TestShipmentSynchronisation_Departure_TransportModeIsRoad()
		{
			using (EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					SetupGBDeclarationTypeList();
					SetupConsolAndShipment(TransportTypeList.Codes.Road);

					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = shipment.PK;
					nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("Prerequisite: synchronization was successful.", "S0001", nctsHeader.MovementHeader.BM_AdditionalText);
					AssertEquals("Registry is true - BM_InlandTransportMode", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("Registry is true - BM_ExportTransportMode", "", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("Registry is true - BM_TransportAtDeparture", "", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("Registry is true - BM_TOLCarrierID", "", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}

			using (EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					SetupGBDeclarationTypeList();
					SetupConsolAndShipment(TransportTypeList.Codes.Road);
					shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = shipment.PK;
					nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("Prerequisite: synchronization was successful.", "S0001", nctsHeader.MovementHeader.BM_AdditionalText);
					AssertEquals("Registry is false - BM_InlandTransportMode", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("Registry is false - BM_ExportTransportMode", "", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("Registry is false - BM_TransportAtDeparture", "", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("Registry is false - BM_TOLCarrierID", "", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}
		}

		void SetupConsolAndShipment(string transportMode)
		{
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0001";
			consol.JK_TransportMode = transportMode;
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "CORIG";
			transport.JW_VoyageFlight = "W1";
			shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "SORIG";
			shipment.JS_UniqueConsignRef = "S0001";
			shipment.JS_RL_NKDestination = "SDEST";
			shipment.JS_TransportMode = transportMode;
		}

		void SetupGBDeclarationTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.France);
			Factory.Save();
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
	}
}
