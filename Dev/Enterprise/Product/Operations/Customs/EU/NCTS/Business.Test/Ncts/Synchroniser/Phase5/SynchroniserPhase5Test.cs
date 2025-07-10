using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class SynchroniserPhase5Test : TestCaseWithFactory
	{
		public void TestOverrideFreightDefaults_NotInDb()
		{
			OverrideFreightDefaultsRunner(false);
		}

		public void TestOverrideFreightDefaults_InDB()
		{
			OverrideFreightDefaultsRunner(true);
		}

		void OverrideFreightDefaultsRunner(bool shouldSave)
		{
			SetupSourceConsolAndShipments();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			nctsHeader.Synchroniser.Synchronise(true);
			if (shouldSave)
			{
				Factory.Save();
			}

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("VUAUY", movementHeader.BM_ForeignDestPortKCode);
			AssertEquals("GBDTE", movementHeader.BM_PlaceOfLoading);
			nctsHeader.DepartureHeaderContainers.Load();
			var container = nctsHeader.DepartureHeaderContainers[0];
			AssertEquals("AAAA1234567", container.BC_ContainerNum);
			AssertEquals(true, movementHeader.BM_ForeignDestPortKCodeInfo.ReadOnly);
			AssertEquals(true, nctsHeader.PlaceOfUnloadingCodeInfo.ReadOnly);
			Assert((nctsHeader.DepartureHeaderContainers[0] as ISynchroniserReadOnlyMembersProvider).SynchroniserReadOnlyMembers.Contains(container.BC_ContainerNumInfo.Name));
			AssertEquals(true, container.BC_ContainerNumInfo.ReadOnly);
			var additionalSeals = container.AdditionalSeals[0];
			AssertEquals(true, additionalSeals.BK_SealNumberInfo.ReadOnly);
			var package = nctsHeader.Bills[0].GoodsItems[0].Packages[0];
			AssertEquals(true, package.ContainersPivotsForBindingOnly[0].ContainerSelectedInfo.ReadOnly);
			nctsHeader.BH_OverrideFreightDefaults = true;
			AssertEquals(false, movementHeader.BM_ForeignDestPortKCodeInfo.ReadOnly);
			AssertEquals(false, nctsHeader.PlaceOfUnloadingCodeInfo.ReadOnly);
			AssertEquals(false, container.BC_ContainerNumInfo.ReadOnly);
			AssertEquals(false, additionalSeals.BK_SealNumberInfo.ReadOnly);
			AssertEquals(false, package.ContainersPivotsForBindingOnly[0].ContainerSelectedInfo.ReadOnly);
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
			containerA.JC_Additional2SealNum = "S3";
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
					containerA.AddPackLine(shipment.OuterPackLines[0]);
					containerB.AddPackLine(shipment.OuterPackLines[2]);
					containerC.AddPackLine(shipment.OuterPackLines[4]);
				}
				else
				{
					var org2 = Factory.NewWithValidTestData<OrgHeader>();
					consignee2 = org2.Addresses.AddNew();
					consignor2 = org2.Addresses.AddNew();
					consignee2.OA_Address1 = "D";
					consignor2.OA_Address1 = "E";
					AddShipment("S0002", "MAGS", "AUSYD", "USATL", consignor2, consignee2, "T2", TransportTypeList.Codes.Road);
					containerA.AddPackLine(shipment.OuterPackLines[0]);
					containerB.AddPackLine(shipment.OuterPackLines[2]);
					containerC.AddPackLine(shipment.OuterPackLines[4]);
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
			shipment.JS_HouseBill = "HB" + shipmentNo;

			shipment.OuterPackLines.RemoveAndDeleteAll(); // gets rid of the automatically-created one that appears when you set the pieces on the shipment
			AddOuterPackLine("00000001", "BE", description + " Package 1", 4m);
			AddOuterPackLine("00000001", "BE", description + " Package 1", 11m);
			AddOuterPackLine("00000002", "BE", description + " Package 2", 8m);
			AddOuterPackLine("00000002", "BE", description + " Package 2", 10m);
			AddOuterPackLine("00000003", "", description + " Package 3", 1m);
			AddOuterPackLine("00000003", "", description + " Package 3", 1m);
		}

		void AddOuterPackLine(ZString harmonisedCode, ZString origin, ZString description, ZDecimal actualWeight)
		{
			var pivot = shipment.OuterPackLines.AddNew();
			pivot.JL_JC = containerA.PK;
			pivot.JL_PackageCount = 69;
			pivot.JL_F3_NKPackType = "BOX";
			pivot.JL_MarksAndNumbers = "Marks and numbers";
			pivot.JL_HarmonisedCode = harmonisedCode;
			pivot.JL_RN_NKOrigin = origin;
			pivot.JL_Description = description;
			pivot.JL_ActualWeight = actualWeight;
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
			shipment.JS_HouseBill = "HB" + shipmentNo;
			return shipment;
		}

		public void TestShipmentSynchronisation_Departure()
		{
			UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments();

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch);
				AssertEquals("VU", nctsHeader.MovementHeader.BM_RL_NKDestinationPort);
				AssertEquals("VUAUY", nctsHeader.MovementHeader.BM_ForeignDestPortKCode);
				AssertEquals("GBDTE", nctsHeader.MovementHeader.BM_PlaceOfLoading);
				AssertEquals("3", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TransportAtDeparture);
				AssertEquals(35m, nctsHeader.MovementHeader.BM_GrossWeight);
				AssertEquals("BM_AdditionalText", "S0001", nctsHeader.MovementHeader.BM_AdditionalText);
				AssertEquals(consignee.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals(consignor.PK, nctsHeader.Consignor.E2_OA_Address);
				AssertEquals(localClient.PK, nctsHeader.Principal.E2_OA_Address);

				nctsHeader.DepartureHeaderContainers.Load();
				var container = nctsHeader.DepartureHeaderContainers[0];
				AssertEquals("AAAA1234567", container.BC_ContainerNum);
				AssertEquals("S1", container.BC_Seal1);
				AssertEquals("S2", container.BC_Seal2);
				AssertEquals("S3", container.AdditionalSeals[0].BK_SealNumber);

				AssertEquals(4, nctsHeader.Bills[0].GoodsItems.Count);
				AssertLineAndPackage(nctsHeader.Bills[0].GoodsItems[0], 15m, "BOOKS Package 1", "0000.00.01", "BE", "AAAA1234567");
				AssertLineAndPackage(nctsHeader.Bills[0].GoodsItems[1], 18m, "BOOKS Package 2", "0000.00.02", "BE", "AAAA1234567");
				AssertLineAndPackage(nctsHeader.Bills[0].GoodsItems[2], 1m, "BOOKS Package 3", "0000.00.03", "", "AAAA1234567");
				AssertLineAndPackage(nctsHeader.Bills[0].GoodsItems[3], 1m, "BOOKS Package 3", "0000.00.03", "", "AAAA1234567");

				AssertEquals("Number of bills", 1, nctsHeader.Bills.Count);

				var bill = nctsHeader.Bills.First();

				AssertEquals("Consignor", ZGuid.Empty, bill.Consignor.E2_OA_Address);
				AssertEquals("Consignee", ZGuid.Empty, bill.Consignee.E2_OA_Address);
				AssertEquals("B0_ReferenceID should have been synced with JS_UniqueConsignRefInfo", "HBS0001", bill.B0_ReferenceID);
				AssertEquals("B0_RN_NKCountryOfExport", "", bill.B0_RN_NKCountryOfExport);
				AssertEquals("B0_RN_NKCountryOfDestination", "", bill.B0_RN_NKCountryOfDestination);

				AssertEquals("Additional Document Transport Outside Transition", "HBS0001", GetAdditionalDocumentForHouseBill(bill.AdditionalDocuments).CSI_ReferenceNumber);
			});
		}

		public void TestShipmentSynchronisation_Departure_CorrectContainerClass()
		{
			SetupDeclarationTypeList();
			SetupSourceConsolAndShipments();

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			nctsHeader.Synchroniser.Synchronise(true);

			var cont = nctsHeader.DepartureHeaderContainers.AddNew();
			cont.BC_ContainerNum = "ABC123";
			AssertNoExceptionThrown(() => nctsHeader.DepartureHeaderContainers.Load());
		}

		public void TestShipmentSynchronisation_Departure_Transition_Period()
		{
			UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments();

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				var bill = nctsHeader.Bills.First();
				AssertNull("No Additional Document Transport Inside Transition", GetAdditionalDocumentForHouseBill(bill.AdditionalDocuments));
			});
		}

		AdditionalInfo GetAdditionalDocumentForHouseBill(INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> docs) => docs.FirstOrDefault(x => x.CSI_Type.EqualsIgnoringCase("OTH") && x.CSI_SubType.EqualsIgnoringCase("TRA") && x.CSI_Code.EqualsIgnoringCase("N714"));

		void AssertLineAndPackage(NctsDepartureCargoDesc line, ZDecimal grossWeight, ZString description, ZString tarrif, ZString origin, ZString containerNumber)
		{
			AssertEquals("", line.BY_RN_NKCountryOfDispatch);
			AssertEquals("", line.BY_RN_NKCountryOfDestination);
			AssertEquals(description, line.BY_Description);
			AssertEquals(tarrif, line.BY_FormattedHarmonisedTariff);
			AssertEquals(origin, line.BY_RN_NKCountryOfOrigin);
			AssertEquals(description, line.BY_Description);
			AssertEquals(ZGuid.Empty, line.Consignee.E2_OA_Address);
			AssertEquals(ZGuid.Empty, line.Consignor.E2_OA_Address);

			var package = line.Packages[0];
			AssertEquals(69L, package.B5_UnitCount);
			AssertEquals("BX", package.B5_UnitType);
			AssertEquals("Marks and numbers", package.B5_MarksAndNumbers);
			AssertEquals("Linked Container", containerNumber, package.ContainersPivotsForBindingOnly[0].ContainerNumber);
		}

		public void TestShipmentSynchronisation_Departure_TransportModeNotRoad()
		{
			CombineAssertions(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments();
				shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
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
					SetupDeclarationTypeList();
					SetupSourceConsolAndShipments();
					shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
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
					SetupDeclarationTypeList();
					SetupSourceConsolAndShipments();
					shipment.JS_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;

					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
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
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments();

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("NCTS Arrival Destination Trader synced", shipment.JobHeader.JH_OA_LocalChargesAddr, nctsHeader.DestinationTrader.E2_OA_Address);
			});
		}

		public void TestShipmentSynchronisation_Arrival_WithDefaultDestinationTrader()
		{
			CombineAssertions(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments();

				var defaultTraderAtDestinationRegistryManagerMock = new Mock<INctsDefaultTraderAtDestinationManager>();
				defaultTraderAtDestinationRegistryManagerMock.Setup(mock => mock.IsEnabled()).Returns(true);

				var nctsHeaderMock = Factory.NewMoq<NctsHeaderForTest>();
				nctsHeaderMock
					.Protected()
					.Setup<INctsDefaultTraderAtDestinationManager>("GetNewDefaultTraderAtDestinationManager")
					.Returns(defaultTraderAtDestinationRegistryManagerMock.Object);
				var nctsHeader = nctsHeaderMock.Object;

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeader.BH_ParentID = shipment.PK;
				nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertNotEquals("NCTS Arrival Destination Trader should not be synced", shipment.JobHeader.JH_OA_LocalChargesAddr, nctsHeader.DestinationTrader.E2_OA_Address);
			});
		}

		public void TestConsolSynchronisationAtLineLevel()
		{
			CombineAssertions(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments(false, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("GBFXT", nctsHeader.MovementHeader.BM_ForeignDestPortKCode);
				AssertEquals("3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("3", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("Header declaration type synced because of different ct statuses on shipment", "T", nctsHeader.MovementHeader.BM_InBondEntryType);

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
				AssertEquals(4, nctsHeader.Bills[0].GoodsItems.Count);
				AssertEquals(4, nctsHeader.Bills[1].GoodsItems.Count);
				AssertEquals(consignee.PK, nctsHeader.Bills[0].Consignee.E2_OA_Address);
				AssertEquals(consignee2.PK, nctsHeader.Bills[1].Consignee.E2_OA_Address);
				AssertEquals(consignor.PK, nctsHeader.Bills[0].Consignor.E2_OA_Address);
				AssertEquals(consignor2.PK, nctsHeader.Bills[1].Consignor.E2_OA_Address);

				AssertLineItem(nctsHeader, 0, "S0001", "BOOKS Package 1", "", "", consignee.PK, consignor.PK, "T1", 15m, 0, "AAAA1234567");
				AssertLineItem(nctsHeader, 0, "S0002", "MAGS Package 1", "", "", consignee2.PK, consignor2.PK, "T2", 15m, 1, "AAAA1234567");
				AssertLineItem(nctsHeader, 1, "S0001", "BOOKS Package 2", "", "", consignee.PK, consignor.PK, "T1", 18m, 0, "AAAA1234567");
				AssertLineItem(nctsHeader, 1, "S0002", "MAGS Package 2", "", "", consignee2.PK, consignor2.PK, "T2", 18m, 1, "BBBB1234567");
				AssertLineItem(nctsHeader, 2, "S0001", "BOOKS Package 3", "", "", consignee2.PK, consignor2.PK, "T1", 1m, 0, "AAAA1234567");
				AssertLineItem(nctsHeader, 3, "S0001", "BOOKS Package 3", "", "", consignee2.PK, consignor2.PK, "T1", 1m, 0, "AAAA1234567");
			});
		}

		public void TestConsolSynchronisationAtBillLevel()
		{
			UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments(false, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("Header Consignee", ZGuid.Empty, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("Header Consignor", ZGuid.Empty, nctsHeader.Consignor.E2_OA_Address);
				AssertEquals("Number of bills", 2, nctsHeader.Bills.Count);

				var bill = nctsHeader.Bills.FirstOrDefault();
				AssertEquals("Reference", "S0001", bill.B0_ReferenceID);
				AssertEquals("Consignor", consignor.PK, bill.Consignor.E2_OA_Address);
				AssertEquals("Consignee", consignee.PK, bill.Consignee.E2_OA_Address);
				AssertEquals("B0_RN_NKCountryOfExport", "GB", bill.B0_RN_NKCountryOfExport);
				AssertEquals("B0_RN_NKCountryOfDestination", "VU", bill.B0_RN_NKCountryOfDestination);

				AssertEquals("Additional Document Transport Outside Transition", "HBS0001", GetAdditionalDocumentForHouseBill(bill.AdditionalDocuments).CSI_ReferenceNumber);

				bill = nctsHeader.Bills.LastOrDefault();
				AssertEquals("Reference", "S0002", bill.B0_ReferenceID);
				AssertEquals("Consignor", consignor2.PK, bill.Consignor.E2_OA_Address);
				AssertEquals("Consignee", consignee2.PK, bill.Consignee.E2_OA_Address);
				AssertEquals("B0_RN_NKCountryOfExport", "AU", bill.B0_RN_NKCountryOfExport);
				AssertEquals("B0_RN_NKCountryOfDestination", "US", bill.B0_RN_NKCountryOfDestination);

				AssertEquals("Additional Document Transport Outside Transition", "HBS0002", GetAdditionalDocumentForHouseBill(bill.AdditionalDocuments).CSI_ReferenceNumber);
			});
		}

		public void TestConsolSynchronisationAtBillLevel_Transition_Period()
		{
			UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				SetupDeclarationTypeList();
				SetupSourceConsolAndShipments(false, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				var bill = nctsHeader.Bills.FirstOrDefault();

				AssertNull("No Additional Document Transport Inside Transition First", GetAdditionalDocumentForHouseBill(bill.AdditionalDocuments));

				bill = nctsHeader.Bills.LastOrDefault();

				AssertNull("No Additional Document Transport Inside Transition Last", GetAdditionalDocumentForHouseBill(bill.AdditionalDocuments));
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel_NoTransitionPeriod()
		{
			UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				SetupSourceConsolAndShipments(true, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("BM_RL_NKForeignDestPort", "GBFXT", nctsHeader.MovementHeader.BM_ForeignDestPortKCode);
				AssertEquals("BM_InlandTransportMode", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
				AssertEquals("BM_ExportTransportMode", "3", nctsHeader.MovementHeader.BM_ExportTransportMode);
				AssertEquals("BM_TOLCarrierID", "W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				AssertEquals("Header declaration type synced from common shipments' declaration type", "T1", nctsHeader.MovementHeader.BM_InBondEntryType);

				AssertEquals("Consignee.E2_OA_Address", consignee.PK, nctsHeader.Consignee.E2_OA_Address);
				AssertEquals("Consignor.E2_OA_Address", consignor.PK, nctsHeader.Consignor.E2_OA_Address);
				AssertEquals("BH_RL_NKImportLoadPort", Core.Constants.CountryCodes.Latvia, nctsHeader.BH_RL_NKImportLoadPort);
				AssertEquals("BM_RL_NKDestinationPort", "VU", nctsHeader.MovementHeader.BM_RL_NKDestinationPort);

				AssertEquals(principal.PK, nctsHeader.Principal.E2_OA_Address);
				nctsHeader.DepartureHeaderContainers.Load();
				var container = nctsHeader.DepartureHeaderContainers[0];
				AssertEquals("container.BC_ContainerNum", "AAAA1234567", container.BC_ContainerNum);
				AssertEquals("BC_Seal1", "S1", container.BC_Seal1);
				AssertEquals("BC_Seal2", "S2", container.BC_Seal2);
				AssertEquals("Bill 1 GoodsItems.Count", 4, nctsHeader.Bills[0].GoodsItems.Count);
				AssertEquals("Bill 2 GoodsItems.Count", 4, nctsHeader.Bills[1].GoodsItems.Count);
				AssertEquals("Bill 1 Consignee", ZGuid.Empty, nctsHeader.Bills[0].Consignee.E2_OA_Address);
				AssertEquals("Bill 2 Consignee", ZGuid.Empty, nctsHeader.Bills[1].Consignee.E2_OA_Address);
				AssertEquals("Bill 1 Consignor", ZGuid.Empty, nctsHeader.Bills[0].Consignor.E2_OA_Address);
				AssertEquals("Bill 2 Consignor", ZGuid.Empty, nctsHeader.Bills[1].Consignor.E2_OA_Address);

				AssertLineItem(nctsHeader, 0, "S0001", "BOOKS Package 1", "", "", ZGuid.Empty, ZGuid.Empty, "", 15m, 0, "AAAA1234567");
				AssertLineItem(nctsHeader, 0, "S0002", "MAGS Package 1", "", "", ZGuid.Empty, ZGuid.Empty, "", 15m, 1, "AAAA1234567");
				AssertLineItem(nctsHeader, 1, "S0001", "BOOKS Package 2", "", "", ZGuid.Empty, ZGuid.Empty, "", 18m, 0, "AAAA1234567");
				AssertLineItem(nctsHeader, 1, "S0002", "MAGS Package 2", "", "", ZGuid.Empty, ZGuid.Empty, "", 18m, 1, "BBBB1234567");
				AssertLineItem(nctsHeader, 2, "S0001", "BOOKS Package 3", "", "", ZGuid.Empty, ZGuid.Empty, "", 1m, 0, "AAAA1234567");
				AssertLineItem(nctsHeader, 3, "S0001", "BOOKS Package 3", "", "", ZGuid.Empty, ZGuid.Empty, "", 1m, 0, "AAAA1234567");

				AssertEquals("Principal.E2_OA_AddressInfo.ReadOnly", true, nctsHeader.Principal.E2_OA_AddressInfo.ReadOnly);
				nctsHeader.BH_OverrideFreightDefaults = true;
				nctsHeader.Principal.E2_OA_AddressInfo.RefreshBinding();
				AssertEquals("Principal.E2_OA_AddressInfo.ReadOnly", false, nctsHeader.Principal.E2_OA_AddressInfo.ReadOnly);

				AssertEquals("Number of transport documents in the NCTS heeader", 1, nctsHeader.AdditionalDocuments.Count);
				AssertEquals("The additional document must have subtype TRA", "TRA", nctsHeader.AdditionalDocuments[0].CSI_SubType);
				AssertEquals("The additional document must have type OTH", "OTH", nctsHeader.AdditionalDocuments[0].CSI_Type);
				AssertEquals("The additional document must have code N705", "N705", nctsHeader.AdditionalDocuments[0].CSI_Code);
				AssertEquals("The additional document must be referenced to the BOL", "BOL123", nctsHeader.AdditionalDocuments[0].CSI_ReferenceNumber);
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel_NoTransitionPeriod_WhenTIR()
		{
			UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				SetupSourceConsolAndShipments(true, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				nctsHeader.Synchroniser.Synchronise(true);

				var line = nctsHeader.Bills[0].GoodsItems[0];
				AssertEquals("Origin - Correct mapped value", Core.Constants.CountryCodes.UnitedKingdom, nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch);
				AssertEquals("Destination - Correct mapped value", Core.Constants.CountryCodes.Vanuatu, nctsHeader.MovementHeader.BM_RL_NKDestinationPort);
				AssertEquals("Origin - This value should not be mapped", ZString.Empty, nctsHeader.Bills[0].B0_RN_NKCountryOfExport);
				AssertEquals("Destination - This value should not be mapped", ZString.Empty, nctsHeader.Bills[0].B0_RN_NKCountryOfDestination);
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel_NoTransitionPeriod_WhenTIR_NoCommonCountryOfDispatch()
		{
			UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				SetupSourceConsolAndShipments(false, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("Origin 1- Correct mapped value", Core.Constants.CountryCodes.UnitedKingdom, nctsHeader.Bills[0].B0_RN_NKCountryOfExport);
				AssertEquals("Destination 1 - Correct mapped value", Core.Constants.CountryCodes.Vanuatu, nctsHeader.Bills[0].B0_RN_NKCountryOfDestination);
				AssertEquals("Origin 2- Correct mapped value", Core.Constants.CountryCodes.Australia, nctsHeader.Bills[1].B0_RN_NKCountryOfExport);
				AssertEquals("Destination 2 - Correct mapped value", Core.Constants.CountryCodes.UnitedStates, nctsHeader.Bills[1].B0_RN_NKCountryOfDestination);
				AssertEquals("Origin - This value should not be mapped", ZString.Empty, nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch);
				AssertEquals("Destination - This value should not be mapped", ZString.Empty, nctsHeader.MovementHeader.BM_RL_NKDestinationPort);
			});
		}

		public void TestConsolSynchronisationAtHeaderLevel_TransitionPeriod()
		{
			UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				SetupSourceConsolAndShipments(true, true);

				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ParentID = sourceConsol.PK;
				nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
				nctsHeader.Synchroniser.Synchronise(true);

				AssertEquals("Number of transport documents in the NCTS heeader", 0, nctsHeader.AdditionalDocuments.Count);
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
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
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
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = sourceConsol.PK;
					nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("BM_InlandTransportMode", "7", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("BM_ExportTransportMode", "7", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("BM_TOLCarrierID", "W1", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}

			using (Registry.EUCustomsDataRegistry.Instance.SyncTransportDetailsFromForwardingToNCTS.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions(() =>
				{
					var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.BH_ParentID = sourceConsol.PK;
					nctsHeader.BH_ParentTableCode = sourceConsol.TablePrefix;
					nctsHeader.Synchroniser.Synchronise(true);

					AssertEquals("BM_InlandTransportMode", "3", nctsHeader.MovementHeader.BM_InlandTransportMode);
					AssertEquals("BM_ExportTransportMode", "", nctsHeader.MovementHeader.BM_ExportTransportMode);
					AssertEquals("BM_TransportAtDeparture", "", nctsHeader.MovementHeader.BM_TransportAtDeparture);
					AssertEquals("BM_TOLCarrierID", "", nctsHeader.MovementHeader.BM_TOLCarrierID);
				});
			}
		}

		void AssertLineItem(NctsHeader nctsHeader, int lineNo, ZString shipmentNo, ZString description, ZString origin, ZString destination, ZGuid consigneePK, ZGuid consignorPK, ZString declarationType, ZDecimal grossWeight, int billNumber, string linkedContainer)
		{
			var line = nctsHeader.Bills[billNumber].GoodsItems[lineNo];
			AssertEquals(description, line.BY_Description);
			AssertEquals(ZGuid.Empty, line.Consignee.E2_OA_Address);
			AssertEquals(ZGuid.Empty, line.Consignor.E2_OA_Address);
			AssertEquals(origin, line.BY_RN_NKCountryOfDispatch);
			AssertEquals(destination, line.BY_RN_NKCountryOfDestination);
			var package = line.Packages[0];
			AssertEquals(69L, package.B5_UnitCount);
			AssertEquals("BX", package.B5_UnitType);  // Converted
			AssertEquals("Marks and numbers", package.B5_MarksAndNumbers);
			AssertEquals("line declaration type should be synched from shipment CTStatus", declarationType, line.BY_Type);
			AssertEquals("Linked Container is selected should be " + linkedContainer, linkedContainer, package.ContainersPivotsForBindingOnly.Where(x => x.ContainerSelected).First().ContainerNumber);
			AssertNotEquals("Linked Container is not selected should not be " + linkedContainer, linkedContainer, package.ContainersPivotsForBindingOnly.Where(x => !x.ContainerSelected).First().ContainerNumber);
		}

		public void TestCtStatusSyncedWithShipment()
		{
			CombineAssertions(() =>
			{
				SetupDeclarationTypeList();
				sourceConsol = Factory.New<ForwardingConsol>();
				shipment = sourceConsol.Shipments.AddNew();
				shipment.JS_CommunityTransitStatus = "T1";
				AssertEquals("Pre-req", "T1", shipment.JS_CommunityTransitStatus);
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
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
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
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
			SetupDeclarationTypeList();
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
			SetupDeclarationTypeList();
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
			SetupDeclarationTypeList();
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

		void SetupDeclarationTypeList()
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
					shipment.Job?.Dispose();
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
