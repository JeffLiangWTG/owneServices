using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class SynchroniserTest : TestCaseWithFactory
	{
		public void TestOverrideFreightDefaults_NotInDb()
		{
			OverrideFreightDefaultsRunner(false);
		}

		public void TestOverrideFreightDefaults_InDB()
		{
			OverrideFreightDefaultsRunner(true);
		}

		void OverrideFreightDefaultsRunner(bool shoudlSave)
		{
			SetupSourceConsolAndShipments();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			nctsHeader.Synchroniser.Synchronise(true);
			if (shoudlSave)
			{
				Factory.Save();
			}

			AssertEquals("GBDTE", nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
			AssertEquals("VUAUY", nctsHeader.PlaceOfUnloadingCode);
			nctsHeader.DepartureHeaderContainers.Load();
			AssertEquals("AAAA1234567", nctsHeader.DepartureHeaderContainers[0].BC_ContainerNum);
			AssertEquals(true, nctsHeader.MovementHeader.BM_RL_NKForeignDestPortInfo.ReadOnly);
			AssertEquals(true, nctsHeader.PlaceOfUnloadingCodeInfo.ReadOnly);
			nctsHeader.BH_OverrideFreightDefaults = true;
			AssertEquals(false, nctsHeader.MovementHeader.BM_RL_NKForeignDestPortInfo.ReadOnly);
			AssertEquals(false, nctsHeader.PlaceOfUnloadingCodeInfo.ReadOnly);
			AssertEquals(false, nctsHeader.DepartureHeaderContainers[0].BC_ContainerNumInfo.ReadOnly);
		}

		void SetupSourceConsolAndShipments(bool addCommonShipment = true, bool createShipmentForConsolTest = false)
		{
			sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = "C4321";
			sourceConsol.JK_RL_NKDischargePort = "VUVLI";
			sourceConsol.JK_RL_NKLoadPort = "GBFXT";
			sourceConsol.JK_TransportMode = "ROA";
			sourceConsol.JK_ConsolMode = "FCL";
			sourceConsol.JK_MasterBillNum = "BOL123";
			sourceConsol.Transports[0].JW_VoyageFlight = "W1";
			sourceConsol.Transports[0].JW_Vessel = "ADMIRALENGRACHT";
			sourceConsol.Transports[0].JW_ETA = ZDateTime.BrettsBirthday;
			sourceConsol.Transports[0].JW_ETD = ZDateTime.BrettsBirthday.AddDays(-1);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			localClient = org.Addresses.AddNew();
			localClient.OA_Address1 = "A";
			refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "20DC";
			containerA = sourceConsol.Containers.AddNew();
			containerA.JC_ContainerNum = "AAAA1234567";
			containerA.JC_RC = refContainer.PK;
			containerA.JC_SealNum = "S1";
			containerA.JC_AdditionalSealNum = "S2";
			containerB = sourceConsol.Containers.AddNew();
			containerB.JC_ContainerNum = "BBBB1234567";
			containerC = sourceConsol.Containers.AddNew();
			containerC.JC_ContainerNum = "CCCC1234567";

			consignee = org.Addresses.AddNew();
			consignor = org.Addresses.AddNew();
			consignee.OA_Address1 = "B";
			consignor.OA_Address1 = "C";

			principal = org.Addresses.AddNew();
			principal.OA_Address1 = "P";
			sourceConsol.JK_OA_SendingForwarderAddress = principal.PK;

			AddShipment("S0001", "BOOKS", "GBDTE", "VUAUY", consignor, consignee, "T1", TransportTypeList.Codes.Road);

			if (createShipmentForConsolTest)
			{
				if (addCommonShipment)
				{
					AddShipment("S0002", "MAGS", "GBDTE", "VUAUY", consignor, consignee, "T1", TransportTypeList.Codes.Road);
				}
				else
				{
					var org2 = Factory.NewWithValidTestData<OrgHeader>();
					consignee2 = org2.Addresses.AddNew();
					consignor2 = org2.Addresses.AddNew();
					consignee2.OA_Address1 = "D";
					consignor2.OA_Address1 = "E";
					AddShipment("S0002", "MAGS", "AUSYD", "USATL", consignor2, consignee2, "T2", TransportTypeList.Codes.Road);
				}
			}
		}

		void AddShipment(ZString shipmentNo, ZString description, ZString origin, ZString destination, OrgAddress shipmentConsignor, OrgAddress shipmentConsignee, ZString ctStatus, ZString transportMode)
		{
			shipment = sourceConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipmentConsignor.PK;
			shipment.CreateJobHeaderWithMutex();
			shipment.JobHeader.JH_OA_LocalChargesAddr = localClient.PK;
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_OuterPacks = 26;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_ActualWeight = 35m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_GoodsDescription = description;
			shipment.JS_MarksAndNumbers = "MARKS";
			shipment.JS_ActualVolume = 38m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_GoodsValue = 1m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 2m;
			shipment.JS_RX_NKInsuranceCurrency = "NZD";
			shipment.JS_CommunityTransitStatus = ctStatus;
			shipment.JS_TransportMode = transportMode;

			shipment.OuterPackLines.RemoveAndDeleteAll(); // gets rid of the automatically-created one that appears when you set the pieces on the shipment
			var pivot = shipment.OuterPackLines.AddNew();
			pivot.JL_JC = containerA.PK;
			pivot.JL_PackageCount = 69;
			pivot.JL_F3_NKPackType = "BOX";
			pivot.JL_MarksAndNumbers = "Marks and numbers";
		}

		ForwardingConsol SetupSourceConsolAndShipmentsWithDirection(ZString uniqueConsignRef, ZString shipmentnumber, bool import = false, bool hasShipment = false)
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = uniqueConsignRef;
			sourceConsol.JK_RL_NKDischargePort = import ? "LV6LV" : "USVLI";
			sourceConsol.JK_RL_NKLoadPort = import ? "USVLI" : "LV6LV";
			sourceConsol.JK_TransportMode = "ROA";
			sourceConsol.JK_ConsolMode = "FCL";
			sourceConsol.JK_MasterBillNum = "BOL123";
			sourceConsol.Transports[0].JW_VoyageFlight = "W1";
			sourceConsol.Transports[0].JW_Vessel = "ADMIRALENGRACHT";
			sourceConsol.Transports[0].JW_ETA = ZDateTime.BrettsBirthday;
			sourceConsol.Transports[0].JW_ETD = ZDateTime.BrettsBirthday.AddDays(-1);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			localClient = org.Addresses.AddNew();
			localClient.OA_Address1 = "A";
			consignee = org.Addresses.AddNew();
			consignor = org.Addresses.AddNew();
			consignee.OA_Address1 = "B";
			consignor.OA_Address1 = "C";
			var principal = org.Addresses.AddNew();
			principal.OA_Address1 = "P";
			sourceConsol.JK_OA_SendingForwarderAddress = principal.PK;

			if (hasShipment)
			{
				var shipment = NewShipment(shipmentnumber, "BOOKS", sourceConsol.JK_RL_NKLoadPort, sourceConsol.JK_RL_NKDischargePort, consignor, consignee, localClient, "T1", TransportTypeList.Codes.Road);
				sourceConsol.Shipments.Add(shipment);
			}

			AssertEquals("Precondition: direction", import ? Directions.Import : Directions.Export, sourceConsol.JobDirection);
			return sourceConsol;
		}

		ForwardingShipment NewShipment(ZString shipmentNo, ZString description, ZString origin, ZString destination, OrgAddress shipmentConsignor, OrgAddress shipmentConsignee, OrgAddress localClient, ZString ctStatus, ZString transportMode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipmentConsignor.PK;
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_OuterPacks = 26;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_ActualWeight = 35m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_GoodsDescription = description;
			shipment.JS_MarksAndNumbers = "MARKS";
			shipment.JS_ActualVolume = 38m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_GoodsValue = 1m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 2m;
			shipment.JS_RX_NKInsuranceCurrency = "NZD";
			shipment.JS_CommunityTransitStatus = ctStatus;
			shipment.JS_TransportMode = transportMode;
			return shipment;
		}

		public void TestShipmentSynchronisation_Departure()
		{
			CombineAssertions(() =>
			{
				SetupGBDeclarationTypeList();
				SetupSourceConsolAndShipments();

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("VU", nctsHeader.MovementHeader.BM_RL_NKDestinationPort);
				AssertEquals("VUAUY", nctsHeader.PlaceOfUnloadingCode);
				AssertEquals("GBDTE", nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("3", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("S0001", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals(consignee.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals(consignor.PK, nctsHeader.Consignor.E2_OA_Address);
				AssertEquals(localClient.PK, nctsHeader.Principal.E2_OA_Address);

				nctsHeader.DepartureHeaderContainers.Load();
				var container = nctsHeader.DepartureHeaderContainers[0];
				AssertEquals("AAAA1234567", container.BC_ContainerNum);
				AssertEquals("S1", container.BC_Seal1);
				AssertEquals("S2", container.BC_Seal2);

				AssertEquals(1, nctsHeader.MovementHeader.GoodsItems.Count);
				var line = nctsHeader.MovementHeader.GoodsItems[0];
				AssertEquals("", line.BY_RN_NKCountryOfDispatch);
				AssertEquals("", line.BY_RN_NKCountryOfDestination);
				AssertEquals("S0001", line.BY_CommercialReferenceNumber);
				AssertEquals("BOOKS", line.BY_Description);
				AssertEquals(35m, line.BY_GrossWeight);
				var package = line.Packages[0];
				AssertEquals(69L, package.B5_UnitCount);
				AssertEquals("BX", package.B5_UnitType);  // Converted
				AssertEquals("Marks and numbers", package.B5_MarksAndNumbers);
			});
		}

		public void TestShipmentSynchronisation_Departure_CorrectContainerClass()
		{
			SetupGBDeclarationTypeList();
			SetupSourceConsolAndShipments();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			nctsHeader.Synchroniser.Synchronise(true);

			var cont = nctsHeader.DepartureHeaderContainers.AddNew();
			cont.BC_ContainerNum = "ABC123";
			AssertNoExceptionThrown(() => nctsHeader.DepartureHeaderContainers.Load());
		}

		public void TestShipmentSynchronisation_Departure_TransportModeNotRoad()
		{
			CombineAssertions(() =>
			{
				SetupGBDeclarationTypeList();
				SetupSourceConsolAndShipments();
				shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("", nctsHeader.MovementHeader.BM_TOLCarrierID);
			});
		}

		public void TestShipmentSynchronisation_Departure_TransportModeNotRoadButRegistrySetToCopyTransportInformation()
		{
			using (Registry.EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					SetupGBDeclarationTypeList();
					SetupSourceConsolAndShipments();
					shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = shipment.PK;
					nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("7", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("7", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}

			using (Registry.EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					SetupGBDeclarationTypeList();
					SetupSourceConsolAndShipments();
					shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = shipment.PK;
					nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}
		}

		public void TestShipmentSynchronisation_Arrival()
		{
			CombineAssertions(() =>
			{
				SetupGBDeclarationTypeList();
				SetupSourceConsolAndShipments();

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("NCTS Arrival Destination Trader synced", shipment.JobHeader.JH_OA_LocalChargesAddr, nctsHeader.DestinationTrader.E2_OA_Address);
				AssertEquals("NCTS Arrival Destination Port synced", shipment.JS_RL_NKDestination.Left(2), nctsHeader.ArrivalMovementHeader.BM_RL_NKDestinationPort);
			});
		}

		public void TestConsolSynchronisationAtLineLevel()
		{
			CombineAssertions(() =>
			{
				SetupGBDeclarationTypeList();
				SetupSourceConsolAndShipments(false, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("GBFXT", nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("3", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("Header declaration type synced because of different ct statuses on shipment", "T-", nctsHeader.MovementHeader.BM_InBondEntryType);

				AssertEquals(ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals(ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
				AssertEquals(ZString.Empty, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("", nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals(principal.PK, nctsHeader.Principal.E2_OA_Address);
				nctsHeader.DepartureHeaderContainers.Load();
				var container = nctsHeader.DepartureHeaderContainers[0];
				AssertEquals("AAAA1234567", container.BC_ContainerNum);
				AssertEquals("S1", container.BC_Seal1);
				AssertEquals("S2", container.BC_Seal2);
				AssertEquals(2, nctsHeader.MovementHeader.GoodsItems.Count);

				AssertLineItem(nctsHeader, 0, "S0001", "BOOKS", "GB", "VU", consignee.PK, consignor.PK, "T1");
				AssertLineItem(nctsHeader, 1, "S0002", "MAGS", "AU", "US", consignee2.PK, consignor2.PK, "T2");
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel()
		{
			CombineAssertions(() =>
			{
				SetupSourceConsolAndShipments(true, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("GBFXT", nctsHeader.MovementHeader.BM_RL_NKForeignDestPort);
				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("3", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("C4321", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals("Header declaration type synced from common shipments' declaration type", "", nctsHeader.MovementHeader.BM_InBondEntryType);

				AssertEquals(consignee.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals(consignor.PK, nctsHeader.Consignor.E2_OA_Address);
				AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("VU", nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals(principal.PK, nctsHeader.Principal.E2_OA_Address);
				nctsHeader.DepartureHeaderContainers.Load();
				var container = nctsHeader.DepartureHeaderContainers[0];
				AssertEquals("AAAA1234567", container.BC_ContainerNum);
				AssertEquals("S1", container.BC_Seal1);
				AssertEquals("S2", container.BC_Seal2);
				AssertEquals(2, nctsHeader.MovementHeader.GoodsItems.Count);

				AssertLineItem(nctsHeader, 0, "S0001", "BOOKS", "", "", ZGuid.Empty, ZGuid.Empty, "");
				AssertLineItem(nctsHeader, 1, "S0002", "MAGS", "", "", ZGuid.Empty, ZGuid.Empty, "");

				AssertEquals(true, nctsHeader.Principal.E2_OA_AddressInfo.ReadOnly);
				nctsHeader.BH_OverrideFreightDefaults = true;
				nctsHeader.Principal.E2_OA_AddressInfo.RefreshBinding();
				AssertEquals(false, nctsHeader.Principal.E2_OA_AddressInfo.ReadOnly);
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel_TransportModeNotRoad()
		{
			CombineAssertions(() =>
			{
				SetupSourceConsolAndShipments(true, true);
				sourceConsol.JK_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				sourceConsol.Transports[0].JW_VoyageFlight = "W1";

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("", nctsHeader.MovementHeader.BM_TOLCarrierID);
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel_TransportModeNotRoadButRegistrySetToCopyTransportInformation()
		{
			SetupSourceConsolAndShipments(true, true);
			sourceConsol.JK_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			sourceConsol.Transports[0].JW_VoyageFlight = "W1";

			using (Registry.EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = sourceConsol.PK;
					nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("7", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("7", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}

			using (Registry.EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = sourceConsol.PK;
					nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}
		}

		void AssertLineItem(NctsHeader nctsHeader, int lineNo, ZString shipmentNo, ZString description, ZString origin, ZString destination, ZGuid consigneePK, ZGuid consignorPK, ZString declarationType)
		{
			var line1 = nctsHeader.MovementHeader.GoodsItems[lineNo];
			AssertEquals(shipmentNo, line1.BY_CommercialReferenceNumber);
			AssertEquals(description, line1.BY_Description);
			AssertEquals(35m, line1.BY_GrossWeight);
			AssertEquals("KG", line1.BY_GrossWeightUnit);
			AssertEquals(consigneePK, line1.Consignee.E2_OA_Address);
			AssertEquals(consignorPK, line1.Consignor.E2_OA_Address);
			AssertEquals(origin, line1.BY_RN_NKCountryOfDispatch);
			AssertEquals(destination, line1.BY_RN_NKCountryOfDestination);
			var package = line1.Packages[0];
			AssertEquals(69L, package.B5_UnitCount);
			AssertEquals("BX", package.B5_UnitType);  // Converted
			AssertEquals("Marks and numbers", package.B5_MarksAndNumbers);
			AssertEquals("line commercial reference number should be synched from shipment.JS_UniqueConsignRef", shipmentNo, line1.BY_CommercialReferenceNumber);
			AssertEquals("line declaration type should be synched from shipment CTStatus", declarationType, line1.BY_Type);
		}

		public void TestCtStatusSyncedWithShipment()
		{
			CombineAssertions(() =>
			{
				SetupGBDeclarationTypeList();
				sourceConsol = Factory.New<ForwardingConsol>();
				shipment = sourceConsol.Shipments.AddNew();
				shipment.JS_CommunityTransitStatus = "T1";
				AssertEquals("Pre-req", "T1", shipment.JS_CommunityTransitStatus);
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				Factory.Save();
				nctsHeader.Synchroniser.Synchronise(true);
				AssertEquals("CTStatus synced to T1", "T1", nctsHeader.MovementHeader.BM_InBondEntryType);
				AssertEquals("CTStatus read only", true, nctsHeader.MovementHeader.BM_InBondEntryTypeInfo.ReadOnly);
				shipment.JS_CommunityTransitStatus = "T2";
				AssertEquals("CTStatus synched to T2", "T2", nctsHeader.MovementHeader.BM_InBondEntryType);
			});
		}

		public void TestDestinationTraderSyncedWithShipment()
		{
			CombineAssertions(() =>
			{
				SetupSourceConsolAndShipments();
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				AssertEquals("", nctsHeader.DestinationTrader.E2_Address1);
				nctsHeader.Synchroniser.Synchronise(true);
				AssertEquals("A", nctsHeader.DestinationTrader.E2_Address1);
				Factory.Save();
				var arrival = new BusinessObjectFactory().Load<NctsHeader>(nctsHeader.PK);
				AssertEquals("A", arrival.DestinationTrader.E2_Address1);
			});
		}

		public void TestPrincipalSynchronisationFromShipmentBasedOnDefaultPrincipalRegistryItemValue()
		{
			var organisation = Factory.New<OrgHeader>();
			shipment = Factory.New<ForwardingShipment>();
			shipment.CreateJobHeaderWithMutex();
			shipment.JobHeader.JH_OA_LocalChargesAddr = organisation.MainAddress.PK;

			var defaultPrincipalRegistryManagerMock = new Mock<INctsDefaultPrincipalRegistryManager>();
			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultPrincipalRegistryManager>("GetNewDefaultPrincipalRegistryManager")
				.Returns(defaultPrincipalRegistryManagerMock.Object);
			nctsHeaderMock.Object.ResetDefaultPrincipalRegistryManagerExposed();
			nctsHeaderMock.Object.BH_ParentID = shipment.PK;
			nctsHeaderMock.Object.BH_ParentTableCode = shipment.TablePrefix;

			defaultPrincipalRegistryManagerMock.Setup(mock => mock.IsRegistryEnabled()).Returns(true);
			nctsHeaderMock.Object.Synchroniser.Synchronise(force: true);
			AssertEquals("When 'Default Principal' registry item has override, synchronised Principal", ZGuid.Empty, nctsHeaderMock.Object.Principal.E2_OA_Address);

			defaultPrincipalRegistryManagerMock.Setup(mock => mock.IsRegistryEnabled()).Returns(false);
			nctsHeaderMock.Object.Synchroniser.SetEnabled(enabled: false, enableDetection: false);
			nctsHeaderMock.Object.Synchroniser.SetEnabled(enabled: true, enableDetection: false);
			nctsHeaderMock.Object.Synchroniser.Synchronise(force: true);
			AssertEquals("When 'Default Principal' registry item has not override, synchronised Principal", organisation.MainAddress.PK, nctsHeaderMock.Object.Principal.E2_OA_Address);
			shipment.Job.Dispose();
			nctsHeaderMock.VerifyAll();
		}

		public void TestPrincipalSynchronisationFromConsolBasedOnDefaultPrincipalRegistryItemValue()
		{
			var organisation = Factory.New<OrgHeader>();
			sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_OA_SendingForwarderAddress = organisation.MainAddress.PK;

			var defaultPrincipalRegistryManagerMock = new Mock<INctsDefaultPrincipalRegistryManager>();
			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultPrincipalRegistryManager>("GetNewDefaultPrincipalRegistryManager")
				.Returns(defaultPrincipalRegistryManagerMock.Object);
			nctsHeaderMock.Object.ResetDefaultPrincipalRegistryManagerExposed();
			nctsHeaderMock.Object.BH_ParentID = sourceConsol.PK;
			nctsHeaderMock.Object.BH_ParentTableCode = sourceConsol.TablePrefix;

			defaultPrincipalRegistryManagerMock.Setup(mock => mock.IsRegistryEnabled()).Returns(true);
			nctsHeaderMock.Object.Synchroniser.Synchronise(force: true);
			AssertEquals("When 'Default Principal' registry item has override, synchronised Principal", ZGuid.Empty, nctsHeaderMock.Object.Principal.E2_OA_Address);

			defaultPrincipalRegistryManagerMock.Setup(mock => mock.IsRegistryEnabled()).Returns(false);
			nctsHeaderMock.Object.Synchroniser.SetEnabled(enabled: false, enableDetection: false);
			nctsHeaderMock.Object.Synchroniser.SetEnabled(enabled: true, enableDetection: false);
			nctsHeaderMock.Object.Synchroniser.Synchronise(force: true);
			AssertEquals("When 'Default Principal' registry item has not override, synchronised Principal", organisation.MainAddress.PK, nctsHeaderMock.Object.Principal.E2_OA_Address);
			nctsHeaderMock.VerifyAll();
		}

		public void TestConsignorConsigneeSynchronisationFromConsolBasedOnDefaultConsignorAndConsigneeRegistryItemValue_WithoutShipment()
		{
			SetupGBDeclarationTypeList();
			var localclientorg = Factory.NewWithValidTestData<OrgHeader>();
			var localclient = localclientorg.Addresses.AddNew();
			localclient.OA_Address1 = "A";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consignorlocal = org.Addresses.AddNew();
			consignorlocal.OA_Address1 = "A";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var consigneelocal = org2.Addresses.AddNew();
			consigneelocal.OA_Address1 = "B";

			var sourceConsol = SetupSourceConsolAndShipmentsWithDirection("C341", "S0001", import: true, hasShipment: false);
			sourceConsol.JK_OA_PackDepotAddress = consignorlocal.PK;
			sourceConsol.JK_OA_UnpackDepotAddress = consigneelocal.PK;

			AssertEquals("Precondition: Import", Directions.Import, sourceConsol.JobDirection);

			var nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and No Shipment when isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsLeaveBlank is true, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsLeaveBlank is true, Consignor should not be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and No Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false", () =>
			{
				AssertEquals("When IsConsignee is false, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is true, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and No Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true", () =>
			{
				AssertEquals("When IsConsignee is true, Consignee should be synchronized.", consigneelocal.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is false, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and No Shipment when isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsRegistryEnabled is not enabled, value should be empty as there is no value to synchronise as there are no shipment.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsRegistryEnabled is not enabled, value should be empty as there is no value to synchronise as there are no shipment.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			var sourceConsol2 = SetupSourceConsolAndShipmentsWithDirection("C342", "S0002", import: false, hasShipment: false);

			sourceConsol2.JK_OA_PackDepotAddress = consignorlocal.PK;
			sourceConsol2.JK_OA_UnpackDepotAddress = consigneelocal.PK;

			AssertEquals("Precondition: Export", Directions.Export, sourceConsol2.JobDirection);

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and No Shipment when isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsLeaveBlank is true, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsLeaveBlank is true, Consignor should not be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and No Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false", () =>
			{
				AssertEquals("When IsConsignee is false, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is true, Consignor should be synchronized.", consignorlocal.PK, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and No Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true", () =>
			{
				AssertEquals("When IsConsignee is true, Consignee should be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is false, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and No Shipment when isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsRegistryEnabled is not enabled, synchronisation should be done on both Consignor and Consignee.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsRegistryEnabled is not enabled, synchronisation should be done on both Consignor and Consignee.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});
		}

		public void TestConsignorConsigneeSynchronisationFromConsolBasedOnDefaultConsignorAndConsigneeRegistryItemValue_WithShipment()
		{
			SetupGBDeclarationTypeList();
			var localclientorg = Factory.NewWithValidTestData<OrgHeader>();
			var localclient = localclientorg.Addresses.AddNew();
			localclient.OA_Address1 = "A";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consignorlocal = org.Addresses.AddNew();
			consignorlocal.OA_Address1 = "A";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var consigneelocal = org2.Addresses.AddNew();
			consigneelocal.OA_Address1 = "B";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var shipConsignor = org3.Addresses.AddNew();
			shipConsignor.OA_Address1 = "A";

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var shipConsignee = org4.Addresses.AddNew();
			shipConsignee.OA_Address1 = "B";

			var sourceConsol = SetupSourceConsolAndShipmentsWithDirection("C341", "S0001", import: true, hasShipment: true);
			sourceConsol.JK_OA_PackDepotAddress = consignorlocal.PK;
			sourceConsol.JK_OA_UnpackDepotAddress = consigneelocal.PK;

			var shipment = sourceConsol.Shipments[0];
			shipment.JS_OA_ExportReceivingDepot = org3.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org4.PK;

			AssertEquals("Precondition: Import", Directions.Import, sourceConsol.JobDirection);

			var nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and Shipment when isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsLeaveBlank is true, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsLeaveBlank is true, Consignor should not be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false", () =>
			{
				AssertEquals("When IsConsignee is false, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is true, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import and Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true", () =>
			{
				AssertEquals("When IsConsignee is true, Consignee should be synchronized.", sourceConsol.JK_OA_UnpackDepotAddress, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is false, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			var sourceConsol2 = SetupSourceConsolAndShipmentsWithDirection("C342", "S0002", import: false, hasShipment: true);

			sourceConsol2.JK_OA_PackDepotAddress = consignorlocal.PK;
			sourceConsol2.JK_OA_UnpackDepotAddress = consigneelocal.PK;

			AssertEquals("Precondition: Export", Directions.Export, sourceConsol2.JobDirection);

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and Shipment when isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsLeaveBlank is true, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsLeaveBlank is true, Consignor should not be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false", () =>
			{
				AssertEquals("When IsConsignee is false, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is true, Consignor should be synchronized.", sourceConsol2.JK_OA_PackDepotAddress, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(sourceConsol2, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export and Shipment when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true", () =>
			{
				AssertEquals("When IsConsignee is true, Consignee should be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is false, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});
		}

		public void TestConsignorConsigneeSynchronisationFromShipmentBasedOnDefaultConsignorAndConsigneeRegistryItemValue()
		{
			SetupGBDeclarationTypeList();
			var orgLocalClient = Factory.NewWithValidTestData<OrgHeader>();
			var localClient = orgLocalClient.Addresses.AddNew();
			localClient.OA_Address1 = "LC";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consignorlocal = org.Addresses.AddNew();
			consignorlocal.OA_Address1 = "A";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var consigneelocal = org2.Addresses.AddNew();
			consigneelocal.OA_Address1 = "B";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = org3.Addresses.AddNew();
			consignor.OA_Address1 = "A";

			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = org4.Addresses.AddNew();
			consignee.OA_Address1 = "B";

			var shipment = NewShipment("S0002", "MAGS", "USLAX", "LV6LV", consignor, consignee, localClient, "T2", TransportTypeList.Codes.Road);

			AssertEquals("Precondition: Import", Directions.Import, shipment.JobDirection);
			shipment.JS_OA_ExportReceivingDepot = consignorlocal.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org2.PK;

			var nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment, isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import when isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsLeaveBlank is true, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsLeaveBlank is true, Consignor should not be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false", () =>
			{
				AssertEquals("When IsConsignee is false, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is true, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true", () =>
			{
				AssertEquals("When IsConsignee is true, Consignee should be synchronized.", shipment.ConsigneeDeliveryAddress.E2_OA_Address, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is false, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment, isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Import when isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsRegistryEnabled is not enabled, synchronisation should be done on both Consignor and Consignee.", consignee.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsRegistryEnabled is not enabled, synchronisation should be done on both Consignor and Consignee.", consignor.PK, nctsHeader.Consignor.E2_OA_Address);
			});

			var shipment2 = NewShipment("S0003", "MAGS", "LV6LV", "USSYD", consignor, consignee, localClient, "T2", TransportTypeList.Codes.Road);
			shipment2.JS_OA_ExportReceivingDepot = consignorlocal.PK;
			shipment2.ConsigneeDeliveryAddress.OrganisationPK = org2.PK;
			Factory.Save();

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment2, isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			AssertEquals("Precondition: Export", Directions.Export, shipment2.JobDirection);
			CombineAssertions("Case Export when isRegistryEnabled: true, isLeaveBlank: true, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsLeaveBlank is true, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsLeaveBlank is true, Consignor should not be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment2, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: true, isConsignee: false", () =>
			{
				AssertEquals("When IsConsignee is false, Consignee should not be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is true, Consignor should be synchronized.", consignorlocal.PK, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment2, isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export when isRegistryEnabled: true, isLeaveBlank: false, isConsignor: false, isConsignee: true", () =>
			{
				AssertEquals("When IsConsignee is true, Consignee should be synchronized.", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsConsignor is false, Consignor should be synchronized.", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
			});

			nctsHeader = CreateNctsHeaderForConsignorConsigneeTest(shipment2, isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false);
			nctsHeader.Synchroniser.Synchronise(force: true);
			CombineAssertions("Case Export when isRegistryEnabled: false, isLeaveBlank: false, isConsignor: false, isConsignee: false", () =>
			{
				AssertEquals("When IsRegistryEnabled is not enabled, synchronisation should be done on both Consignor and Consignee.", consignee.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("When IsRegistryEnabled is not enabled, synchronisation should be done on both Consignor and Consignee.", consignor.PK, nctsHeader.Consignor.E2_OA_Address);
			});
		}

		NctsHeaderForTest CreateNctsHeaderForConsignorConsigneeTest(BusinessObject source, bool isRegistryEnabled, bool isLeaveBlank, bool isConsignor, bool isConsignee)
		{
			var defaultConsignorConsigneeRegistryManagerMock = new Mock<INctsDefaultConsignorAndConsigneeRegistryManager>();
			var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
			nctsHeaderMock.Object.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderMock.Object.BH_ParentID = source.PK;
			nctsHeaderMock.Object.BH_ParentTableCode = source.TablePrefix;

			nctsHeaderMock.Object.ResetDefaultConsigneeConsignorRegistryManagerExposed();

			defaultConsignorConsigneeRegistryManagerMock.Setup(mock => mock.IsRegistryEnabled()).Returns(isRegistryEnabled);
			defaultConsignorConsigneeRegistryManagerMock.Setup(mock => mock.IsLeaveBlank()).Returns(isLeaveBlank);
			defaultConsignorConsigneeRegistryManagerMock.Setup(mock => mock.IsConsignor()).Returns(isConsignor);
			defaultConsignorConsigneeRegistryManagerMock.Setup(mock => mock.IsConsignee()).Returns(isConsignee);
			nctsHeaderMock
				.Protected()
				.Setup<INctsDefaultConsignorAndConsigneeRegistryManager>("GetNewDefaultConsignorConsigneeRegistryManager")
				.Returns(defaultConsignorConsigneeRegistryManagerMock.Object);

			nctsHeaderMock.Object.Synchroniser.SetEnabled(enabled: false, enableDetection: false);
			nctsHeaderMock.Object.Synchroniser.SetEnabled(enabled: true, enableDetection: false);

			nctsHeaderMock.Object.Consignor.E2_OA_Address = ZGuid.Empty;
			nctsHeaderMock.Object.Consignee.E2_OA_Address = ZGuid.Empty;

			return nctsHeaderMock.Object;
		}

		void SetupGBDeclarationTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (sourceConsol != null)
			{
				foreach (ForwardingShipment shipment in sourceConsol.Shipments)
				{
					if (shipment != null && shipment.Job != null)
					{
						shipment.Job.Dispose();
					}
				}
			}
		}

		ForwardingConsol sourceConsol;
		OrgAddress principal;
		OrgAddress consignee;
		OrgAddress consignor;
		OrgAddress consignee2;
		OrgAddress consignor2;
		OrgAddress localClient;
		RefContainer refContainer;
		ForwardingContainer containerA;
		ForwardingContainer containerB;
		ForwardingContainer containerC;
		ForwardingShipment shipment;
	}
}
