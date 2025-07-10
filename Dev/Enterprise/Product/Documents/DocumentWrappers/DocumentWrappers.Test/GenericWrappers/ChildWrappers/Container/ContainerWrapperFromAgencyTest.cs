using System;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromAgency))]
	sealed class ContainerWrapperFromAgencyTest : ContainerWrapperTest
	{
		public void TestFreightJob()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			ContainerWrapperFromAgency wrapper = new ContainerWrapperFromAgency(container, Factory);
			AssertEquals("Wrapped business object should be AgencyShipment", "S00001234", wrapper.FreightJob.JobNumber);
		}

		public void TestPrintTACImage()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container1 = shipment.BookedContainers.AddNew();
			AgencyShipmentContainer container2 = shipment.BookedContainers.AddNew();
			FreightWrapperFromAgencyShipment agencyShipment = new FreightWrapperFromAgencyShipment(container1.Booking, Factory);
			agencyShipment.Containers.Add(container2);

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			shipment.JS_OH_DeliveryAgent = principal.PK;

			Factory.Save();

			Assert(!agencyShipment.Containers[0].PrintTACImage);
			Assert(!agencyShipment.Containers[1].PrintTACImage);

			DeliveryOrderCollection collection = new DeliveryOrderCollection();
			DeliveryOrder element = collection.AddNew();
			element.PrincipalPK = principal.PK;
			element.PrintParameter = DeliveryOrder.PrintConstants.Code.PDO;
			element.Image = new Bitmap(10, 10);
			DocumentsDataRegistry.Instance.DeliveryOrderTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Assert(agencyShipment.Containers[0].PrintTACImage);
			Assert(!agencyShipment.Containers[1].PrintTACImage);
		}

		public void TestIsReefer()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			ContainerWrapperFromAgency wrapper = new ContainerWrapperFromAgency(container, Factory);
			AssertEquals("Container: IsReefer", true, wrapper.IsReefer);
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("Container: IsNotReefer", false, wrapper.IsReefer);
		}

		public void TestIsHazardous()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_RH_NKContainerCommodityCode = "HAZ";
			ContainerWrapperFromAgency wrapper = new ContainerWrapperFromAgency(container, Factory);
			AssertEquals("Container: IsHazardous", true, wrapper.IsHazardous);
			container.JC_RH_NKContainerCommodityCode = "GEN";
			AssertEquals("Container: IsNotHazardous", false, wrapper.IsHazardous);
			AgencyShipmentPackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			packline.JL_RH_NKCommodityCode = "HAZ";
			AssertEquals("PackLine: IsHazardous", true, wrapper.IsHazardous);
			packline.JL_RH_NKCommodityCode = "GEN";
			AssertEquals("PackLine: IsNotHazardous", false, wrapper.IsHazardous);
			container.JC_RH_NKContainerCommodityCode = "";
			packline.JL_RH_NKCommodityCode = "";
			AssertEquals("Container & PackLine: IsEmpty", false, wrapper.IsHazardous);
		}

		public void TestDetention()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZDateTime now = ZDateTime.Now.ToSmallDateTime();

				OrgHeader brisbaneDepot = Factory.NewWithValidTestData<OrgHeader>();
				brisbaneDepot.OH_Code = "BNE";
				brisbaneDepot.OH_RL_NKClosestPort = "AUBNE";

				OrgHeader aklDepot = Factory.NewWithValidTestData<OrgHeader>();
				aklDepot.OH_Code = "AKL";
				aklDepot.OH_RL_NKClosestPort = "NZAKL";

				OrgHeader kyivDepot = Factory.NewWithValidTestData<OrgHeader>();
				kyivDepot.OH_Code = "KYV";
				kyivDepot.OH_RL_NKClosestPort = "UAIEV";

				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";

				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NLAMS";
				destination.JB_AvailabilityDate = now.AddDays(-15);

				BillOfLading bill = Factory.New<BillOfLading>();
				bill.JS_JX = voyage.Sailings[0].PK;

				BillOfLadingContainer container = bill.RealContainers.AddNew();
				container.JC_ContainerNum = "TEST4100013";
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_EmptyReturnedBy = now.AddDays(-6);
				container.JC_OA_DepartureContainerYardAddress = aklDepot.MainAddress.PK;
				container.JC_OA_ArrivalContainerYardAddress = kyivDepot.MainAddress.PK;

				RefContainerStock stock = container.Stock;

				ContainerMovement ygoMovement = stock.Movements.AddNew();
				ygoMovement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
				ygoMovement.E9_MovementDate = now.AddDays(-115);
				ygoMovement.E9_JV = voyage.PK;
				ygoMovement.E9_OA_Depot = aklDepot.MainAddress.PK;

				ContainerMovement wgiMovement = stock.Movements.AddNew();
				wgiMovement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
				wgiMovement.E9_MovementDate = now.AddDays(-101);
				wgiMovement.E9_JV = voyage.PK;
				wgiMovement.E9_OA_Depot = brisbaneDepot.MainAddress.PK;

				ContainerMovement ygiMovement = stock.Movements.AddNew();
				ygiMovement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				ygiMovement.E9_MovementDate = now.AddDays(-1);
				ygiMovement.E9_JV = voyage.PK;
				ygiMovement.E9_OA_Depot = kyivDepot.MainAddress.PK;

				Factory.Save();

				ContainerWrapperFromAgency wrapper = new ContainerWrapperFromAgency(container, Factory);

				AssertEquals("ExportDetention.Released", now.AddDays(-115), wrapper.ExportDetention.Released);
				AssertEquals("ExportDetention.LastFreeDay", now.AddDays(-106), wrapper.ExportDetention.LastFreeDay);
				AssertEquals("ExportDetention.Returned", now.AddDays(-101), wrapper.ExportDetention.Returned);
				AssertEquals("ExportDetention.FreeDays", 10, wrapper.ExportDetention.FreeDays);
				AssertEquals("ExportDetention.DetentionDays", 5, wrapper.ExportDetention.DetentionDays);

				AssertEquals("ImportDetention.Released", now.AddDays(-15), wrapper.ImportDetention.Released);
				AssertEquals("ImportDetention.LastFreeDay", now.AddDays(-6), wrapper.ImportDetention.LastFreeDay);
				AssertEquals("ImportDetention.Returned", now.AddDays(-1), wrapper.ImportDetention.Returned);
				AssertEquals("ImportDetention.FreeDays", 10, wrapper.ImportDetention.FreeDays);
				AssertEquals("ImportDetention.DetentionDays", 5, wrapper.ImportDetention.DetentionDays);
			}
		}

		public void TestDetentionWhenEmptyReturnedByIsBlank()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZDateTime today = DateTime.Today;

				OrgHeader brisbaneDepot = Factory.NewWithValidTestData<OrgHeader>();
				brisbaneDepot.OH_Code = "BNE";
				brisbaneDepot.OH_RL_NKClosestPort = "AUBNE";

				OrgHeader aklDepot = Factory.NewWithValidTestData<OrgHeader>();
				aklDepot.OH_Code = "AKL";
				aklDepot.OH_RL_NKClosestPort = "NZAKL";

				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";

				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NLAMS";
				destination.JB_AvailabilityDate = today.AddDays(-15);

				BillOfLading bill = Factory.New<BillOfLading>();
				bill.JS_JX = voyage.Sailings[0].PK;

				BillOfLadingContainer container = bill.RealContainers.AddNew();
				container.JC_ContainerNum = "TEST4100013";
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;

				Factory.Save();

				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				ContainerWrapperFromAgency wrapper = new ContainerWrapperFromAgency(container, Factory);

				AssertEquals("EmptyReturnedBy", ZDateTime.Empty, container.JC_EmptyReturnedBy);
				var lastFreeDay = destination.JB_AvailabilityDate.AddDays(10 - 1);
				AssertEquals("ImportDetention.LastFreeDay", lastFreeDay, wrapper.ImportDetention.LastFreeDay);
				AssertEquals("IsOverdue", true, wrapper.ImportDetention.IsOverdue);
			}
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Shipping);
			var defaultNumberOfDecimals_Weight = collection.AddNew();
			defaultNumberOfDecimals_Weight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_Weight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Weight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_Volume = collection.AddNew();
			defaultNumberOfDecimals_Volume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_Volume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_Volume.RoundingMode = RoundingModes.Down;

			AgencyRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.BookedContainers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_JC = container.PK;

			container.JC_GrossWeight = 125.237m;
			container.JC_TareWeight = 47.236m;
			container.JC_DunnageWeight = 162.463m;

			packLine.JL_ActualVolume = 5.649m;
			packLine.JL_ActualWeight = 52.321m;

			var wrapper = new ContainerWrapperFromAgency(container, Factory);

			AssertEquals(47.24m, wrapper.WeightTare.Value);
			AssertEquals("KG", wrapper.WeightTare.Unit.Code);
			AssertEquals("47.24 KG", wrapper.WeightTare.ValueAndUnitCode);
			AssertEquals(52.33m, wrapper.WeightGoods.Value);
			AssertEquals("KG", wrapper.WeightGoods.Unit.Code);
			AssertEquals("52.33 KG", wrapper.WeightGoods.ValueAndUnitCode);
			AssertEquals(262.04m, wrapper.WeightGross.Value);
			AssertEquals("KG", wrapper.WeightGross.Unit.Code);
			AssertEquals("262.04 KG", wrapper.WeightGross.ValueAndUnitCode);
			AssertEquals(162.47m, wrapper.WeightDunnage.Value);
			AssertEquals("KG", wrapper.WeightDunnage.Unit.Code);
			AssertEquals("162.47 KG", wrapper.WeightDunnage.ValueAndUnitCode);

			AssertEquals(5.64m, wrapper.VolumeGoods.Value);
			AssertEquals("M3", wrapper.VolumeGoods.Unit.Code);
			AssertEquals("5.64 M3", wrapper.VolumeGoods.ValueAndUnitCode);
		}

		[TestDate(2016, 1, 2, 3, 4, 5)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestVGM()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_GrossWeight = 456;
			container.JC_GrossWeightUQ = "KG";
			container.JC_GrossWeightVerificationType = "PKG";

			var vgmAddress = container.DocAddresses.FindByDocAddressType(DocAddressType.GrossWeightVerifiedBy);
			vgmAddress.E2_AddressOverride = true;
			vgmAddress.E2_Contact = "Mr Robot";
			vgmAddress.E2_CompanyName = "Evil Corp";
			vgmAddress.E2_AddressType = "VGM";

			var wrapper = new ContainerWrapperFromFreight(container, Factory);
			AssertEquals(456m, wrapper.WeightGross.Value);
			AssertEquals("KG", wrapper.WeightGross.Unit.Code);
			AssertEquals("02-Jan-16 03:04", wrapper.VGMVerifiedDate.ToString("dd-MMM-yy HH:mm"));
			AssertEquals("Method 2 - Packages", wrapper.VGMMethod.Description);
			AssertEquals("Mr Robot", wrapper.VGMVerifiedByAddress.ContactName);
			AssertEquals("EVIL CORP", wrapper.VGMVerifiedByAddress.CompanyName);
		}

		public override void TestWrapperMappingFull()
		{
			var exportYard = Factory.New<OrgHeader>();
			exportYard.OH_FullName = "TEST DEPARTURE CONTAINER YARD ADDRESS";
			var arrival = Factory.New<OrgHeader>();
			arrival.OH_FullName = "TEST ARRIVAL CONTAINER YARD ADDRESS";

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_ISOType = "2340";
			containerType.RC_TareWeight = 1450m;

			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_HouseBill = "MASTERCAT";
			shipment.JS_UniqueConsignRef = "MNZXC98735";

			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "OOCL0000011";
			container.JC_RC = containerType.PK;
			container.JC_SealNum = "SEAL_ROCKS";
			container.JC_AdditionalSealNum = "SEAL_ROCKS_RULE";
			container.JC_Additional2SealNum = "SEAL_ROCKS_SUCK";
			container.JC_ReleaseNum = "PERSIST AND WIN";
			container.JC_ContainerImportDORelease = "OBEYTHEFREIGHT";
			container.JC_DunnageWeight = 340m;
			container.JC_EmptyReadyForReturn = new ZDateTime(2011, 4, 5);
			container.JC_EmptyReturnedBy = new ZDateTime(2007, 8, 15);
			container.JC_EmptyRequired = new ZDateTime(2007, 8, 12);
			container.JC_EmptyReturnReference = "REEE123";
			container.JC_FCLWharfGateOut = new ZDateTime(2007, 8, 20);
			container.JC_ArrivalEstimatedDelivery = new ZDateTime(2011, 4, 1);
			container.JC_ArrivalSlotReference = "HJK3435ML";
			container.JC_DepartureSlotReference = "MHUI78P934O";
			container.JC_ArrivalSlotDateTime = new ZDateTime(2007, 8, 24);
			container.JC_DepartureSlotDateTime = new ZDateTime(2007, 8, 25);
			container.JC_ExportDepotCustomsReference = "MKJ1223REF";
			container.JC_DepartureEstimatedPickup = new ZDateTime(2007, 8, 13);
			container.JC_ArrivalCartageRef = "ARV111";
			container.JC_DepartureCartageRef = "DEP222";
			container.JC_SetPointTemp = 23.4m;
			container.JC_SetPointTempUnit = "C";
			container.JC_IsDamaged = true;
			container.JC_HumidityPercent = new ZByte(23);
			container.JC_AirVentFlow = 89.3m;
			container.JC_AirVentFlowRateUnit = "P1";
			container.JC_RefrigGeneratorID = "RG18181";
			container.JC_OA_DepartureContainerYardAddress = exportYard.MainAddress.PK;
			container.JC_OA_ArrivalContainerYardAddress = arrival.MainAddress.PK;
			container.JC_ContainerJobID = "JobID";
			container.JC_ContainerQuality = "FOD";
			container.JC_StowagePosition = "1234567";

			var number = container.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = "AMS";
			number.CE_EntryNum = "AMS0001";

			var package = shipment.OuterPackLines.AddNew();
			package.JL_JC = container.PK;
			package.JL_PackageCount = 23;
			package.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			package.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			package.JL_ActualWeight = 23630m;
			package.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			package.JL_ActualVolume = 23m;

			package.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var service = container.JobContainer.Services.AddNew();
			service.ES_ServiceCode = "CLN";

			var wrapper = new ContainerWrapperFromAgency(container, Factory);
			AssertEquals("Mode.Code", "FCL", wrapper.Mode.Code);
			AssertEquals("Type", "ZZGP - General Purpose Box", wrapper.Type.ToString());
			AssertEquals("VolumeGoods.Value", 23m, wrapper.VolumeGoods.Value);
			AssertEquals("VolumeGoods.Unit.Code", "M3", wrapper.VolumeGoods.Unit.Code);
			AssertEquals("WeightTare.Value", 1450m, wrapper.WeightTare.Value);
			AssertEquals("WeightTare.Unit.Code", "KG", wrapper.WeightTare.Unit.Code);
			AssertEquals("WeightGoods.Value", 23630m, wrapper.WeightGoods.Value);
			AssertEquals("WeightGoods.Unit.Code", "KG", wrapper.WeightGoods.Unit.Code);
			AssertEquals("WeightDunnage.Value", 340m, wrapper.WeightDunnage.Value);
			AssertEquals("WeightDunnage.Unit.Code", "KG", wrapper.WeightDunnage.Unit.Code);
			AssertEquals("WeightGross.Value", 25420m, wrapper.WeightGross.Value);
			AssertEquals("WeightGross.Unit.Code", "KG", wrapper.WeightGross.Unit.Code);
			AssertEquals("SetPointTemperature.Value", 23.4m, wrapper.SetPointTemperature.Value);
			AssertEquals("SetPointTemperature.Unit.Code", "C", wrapper.SetPointTemperature.Unit.Code);
			AssertEquals("ContainerNo", "OOCL0000011", wrapper.ContainerNo);
			AssertEquals("ContainerNumberOrTypeCount", "OOCL0000011", wrapper.ContainerNumberOrTypeCount);
			AssertEquals("SealNo", "SEAL_ROCKS", wrapper.SealNo);
			AssertEquals("SealNo2", "SEAL_ROCKS_RULE", wrapper.SealNo2);
			AssertEquals("SealNo3", "SEAL_ROCKS_SUCK", wrapper.SealNo3);
			AssertEquals("ReleaseNumber", "PERSIST AND WIN", wrapper.ReleaseNumber);
			AssertEquals("ArrivalReleaseNumber", "OBEYTHEFREIGHT", wrapper.ArrivalReleaseNumber);
			AssertEquals("Services.Count", 1, wrapper.Services.Count);
			AssertEquals("Services[0].Type.Code", "CLN", wrapper.Services[0].Type.Code);
			AssertEquals("EmptyReturnedBy", new ZDateTime(2007, 8, 15), wrapper.EmptyReturnedBy);
			AssertEquals("EmptyReturnReference", "REEE123", wrapper.EmptyReturnReference);
			AssertEquals("PackCount.ValueAndUnitCodeBlankIfZero", "23 PKG", wrapper.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("BookingReference", "MNZXC98735", wrapper.BookingReference);
			AssertEquals("ArrivalEstimatedDelivery", new ZDateTime(2011, 4, 1), wrapper.ArrivalEstimatedDelivery);
			AssertEquals("ArrivalSlotReference", "HJK3435ML", wrapper.ArrivalSlotReference);
			AssertEquals("DepartureSlotReference", "MHUI78P934O", wrapper.DepartureSlotReference);
			AssertEquals("ArrivalSlotTime", new ZDateTime(2007, 8, 24), wrapper.ArrivalSlotTime);
			AssertEquals("DepartureSlotTime", new ZDateTime(2007, 8, 25), wrapper.DepartureSlotTime);
			AssertEquals("ArrivalCartageRef", "ARV111", wrapper.ArrivalCartageRef);
			AssertEquals("DepartureCartageRef", "DEP222", wrapper.DepartureCartageRef);
			AssertEquals("ExportDepotCustomsReference", "MKJ1223REF", wrapper.ExportDepotCustomsReference);
			AssertEquals("EmptyReadyForReturn", new ZDateTime(2011, 4, 5), wrapper.EmptyReadyForReturn);
			AssertEquals("EmptyRequired", new ZDateTime(2007, 8, 12), wrapper.EmptyRequired);
			AssertEquals("DepartureEstimatedPickup", new ZDateTime(2007, 8, 13), wrapper.DepartureEstimatedPickup);
			AssertEquals("Damaged", true, wrapper.Damaged);
			AssertEquals("Frozen", true, !wrapper.Frozen);
			AssertEquals("Chilled", true, wrapper.Chilled);
			AssertEquals("ControlledAtmosphere", true, wrapper.ControlledAtmosphere);
			AssertEquals("HumidityPercentage", new ZByte(23), wrapper.HumidityPercentage);
			AssertEquals("AirVentFlow.ValueAndUnitCodeBlankIfZero", "89 P1", wrapper.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("ClipOnUnit", "RG18181", wrapper.ClipOnUnit);
			AssertEquals("UNDGSubstances.Count", 1, wrapper.UNDGSubstances.Count);
			AssertEquals("UNDGSubstances[0].UNNumber", "0004", wrapper.UNDGSubstances[0].UNNumber);
			AssertEquals("DepartureContainerYardAddress.CompanyName", "TEST DEPARTURE CONTAINER YARD ADDRESS", wrapper.DepartureContainerYardAddress.CompanyName);
			AssertEquals("ArrivalContainerYardAddress.CompanyName", "TEST ARRIVAL CONTAINER YARD ADDRESS", wrapper.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("ContainerJobID", "JobID", wrapper.ContainerJobID);
			AssertEquals("Commodity.Count", 1, wrapper.Commodities.Count);
			AssertEquals("Commodity[0].Code", "GEN", wrapper.Commodities[0].Code);
			AssertEquals("Commodity[0].Description", "General", wrapper.Commodities[0].Description);
			AssertEquals("IsChargeable", "No", wrapper.IsChargeable);
			AssertEquals("IsPalletized", "No", wrapper.IsPalletized);
			AssertEquals("Items", "0", wrapper.Packages);
			AssertEquals("Pallets", "0", wrapper.Pallets);
			AssertEquals("ContainerQuality", "FOD", wrapper.ContainerQuality.Code);
			AssertEquals("StowagePosition", "1234567", wrapper.StowagePosition);

			var entry = wrapper.CustomsEntries[0];
			AssertEquals("entry.EntryType.Code", "AMS", entry.EntryType.Code);
			AssertEquals("entry.EntryType.Code", "AMS0001", entry.EntryNumber);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromAgency(null, Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = (ContainerWrapper)GetNewDocumentWrapper();
			AssertEquals("Mode.Code", "FCL", wrapper.Mode.Code);
			AssertEquals("Type.Code", ZString.Empty, wrapper.Type.Code);
			AssertEquals("DeliveryMode", ZString.Empty, wrapper.DeliveryMode.ToString());
			AssertEquals("SetPointTemperature", ZString.Empty, wrapper.SetPointTemperature.ValueAndUnitCodeBlankIfZero);
			AssertEquals("VolumeGoods.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.VolumeGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("WeightTare.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.WeightTare.ValueAndUnitCodeBlankIfZero);
			AssertEquals("WeightGoods.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.WeightGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("WeightDunnage.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.WeightDunnage.ValueAndUnitCodeBlankIfZero);
			AssertEquals("WeightGross.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.WeightGross.ValueAndUnitCodeBlankIfZero);
			AssertEquals("ContainerNo", ZString.Empty, wrapper.ContainerNo);
			AssertEquals("SealNo", ZString.Empty, wrapper.SealNo);
			AssertEquals("SealNo2", ZString.Empty, wrapper.SealNo2);
			AssertEquals("SealNo3", ZString.Empty, wrapper.SealNo3);
			AssertEquals("ReleaseNumber", ZString.Empty, wrapper.ReleaseNumber);
			AssertEquals("ArrivalReleaseNumber", ZString.Empty, wrapper.ArrivalReleaseNumber);
			AssertEquals("Services", 0, wrapper.Services.Count);
			AssertEquals("PackCount.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("EmptyReturnedBy", ZDateTime.Empty, wrapper.EmptyReturnedBy);
			AssertEquals("ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapper.ContainerYardEmptyReturnGateIn);
			AssertEquals("ExportDepotCustomsReference", ZString.Empty, wrapper.ExportDepotCustomsReference);
			AssertEquals("EmptyRequired", ZDateTime.Empty, wrapper.EmptyRequired);
			AssertEquals("WharfGateOut", ZDateTime.Empty, wrapper.WharfGateOut);
			AssertEquals("DepartureEstimatedPickup", ZDateTime.Empty, wrapper.DepartureEstimatedPickup);
			AssertEquals("Length", ZDecimal.Zero, wrapper.Length);
			AssertEquals("Width", ZDecimal.Zero, wrapper.Width);
			AssertEquals("Height", ZDecimal.Zero, wrapper.Height);
			AssertEquals("ContainerJobID", ZString.Empty, wrapper.ContainerJobID);
			AssertEquals("Damaged", false, wrapper.Damaged);
			AssertEquals("Frozen", false, wrapper.Frozen);
			AssertEquals("Chilled", false, wrapper.Chilled);
			AssertEquals("HumidityPercentage", ZByte.Zero, wrapper.HumidityPercentage);
			AssertEquals("AirVentFlow.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("UNDGNumbers.Count", 0, wrapper.UNDGSubstances.Count);
			AssertEquals("DepartureContainerYardAddress.CompanyName", ZString.Empty, wrapper.DepartureContainerYardAddress.CompanyName);
			AssertEquals("IsChargeable", "No", wrapper.IsChargeable);
			AssertEquals("IsPalletized", "No", wrapper.IsPalletized);
			AssertEquals("Packages", "0", wrapper.Packages);
			AssertEquals("Pallets", "0", wrapper.Pallets);
			AssertEquals("CFSClient", null, wrapper.CFSClient);
			AssertEquals("UnpackShed", ZString.Empty, wrapper.UnpackShed);
			AssertEquals("StowagePosition", ZString.Empty, wrapper.StowagePosition);
			Assert("CustomsEntries", wrapper.CustomsEntries.Count == 0);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var exportYard = Factory.New<OrgHeader>();
			exportYard.OH_FullName = "TEST DEPARTURE CONTAINER YARD ADDRESS";
			exportYard.MainAddress.OA_RN_NKCountryCode = "AU";

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450.000m;

			var shipment = Factory.New<BillOfLading>();

			var container = shipment.RealContainers.AddNew();
			container.JC_RC = containerType.PK;
			container.JC_DunnageWeight = 340.000m;
			container.JC_SetPointTemp = 23.4m;
			container.JC_SetPointTempUnit = "C";
			container.JC_AirVentFlow = 89.3m;
			container.JC_AirVentFlowRateUnit = "P1";
			container.JC_OA_DepartureContainerYardAddress = exportYard.MainAddress.PK;

			var package = shipment.OuterPackLines.AddNew();
			package.JL_JC = container.PK;
			package.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			package.JL_PackageCount = 23;
			package.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			package.JL_ActualWeight = 23630.000m;
			package.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			package.JL_ActualVolume = 23.320m;

			return new ContainerWrapperFromAgency(container, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 89 P1
ArrivalContainerYardAddress : 
CFSClient :  is null
ContainerQuality : 
DeliveryMode : 
DepartureContainerYardAddress : TEST DEPARTURE CONTAINER YARD ADDRESS\nAUSTRALIA
ExportDetention : 
FreightJob : 
ImportDetention : 
Mode : FCL - Full Container Load
OffHirePort : 
OnHirePort : 
Owner : 
PackCount : 23 PKG
Registry : (No Default Field Value Available on Registry)
SetPointTemperature : 23.4 C
Status : 
Type : ZZGP - General Purpose Box
VGMMethod : NON - Not Verified
VGMVerifiedByAddress : 
VolumeGoods : 23.320 M3
WeightDunnage : 340.000 KG
WeightGoods : 23630.000 KG
WeightGross : 25420.000 KG
WeightTare : 1450.000 KG
";
			}
		}
	}
}
