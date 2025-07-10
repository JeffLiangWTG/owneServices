using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromFreight))]
	sealed class ContainerWrapperFromFreightTest : ContainerWrapperTest
	{
		public void TestCoverSheetForASMShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			var asmShipment = consol.Shipments.AddNew();
			asmShipment.JS_ShipmentType = "ASM";
			var subShipment1 = asmShipment.CoLoadShipments.AddNew();
			var packLine1 = subShipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(container.PK);
			packLine1.JL_PackageCount = 1;
			packLine1.JL_ActualWeight = 2m;
			packLine1.JL_Height = 3m;
			packLine1.JL_Length = 1m;
			packLine1.JL_Width = 1m;
			var subShipment2 = asmShipment.CoLoadShipments.AddNew();
			var packLine2 = subShipment2.OuterPackLines.AddNew();
			packLine2.SetContainer(container.PK);
			packLine2.JL_PackageCount = 4;
			packLine2.JL_ActualWeight = 5m;
			packLine2.JL_Height = 6m;
			packLine2.JL_Length = 1m;
			packLine2.JL_Width = 1m;

			var wrapper = new ContainerWrapperFromFreight(container, asmShipment, Factory);
			AssertEquals(7m, wrapper.WeightGoods.Value);
			AssertEquals(5m, wrapper.PackCount.Value);
			AssertEquals(27m, wrapper.VolumeGoods.Value);
		}

		public void TestArrivalReleaseNumberAndArrivalCartageRef()
		{
			var container = Factory.New<ForwardingContainer>();
			var wrapper = new ContainerWrapperFromFreight(container, Factory);

			container.JC_ArrivalPickupByRail = true;
			container.AMSNumber = "MB2";
			container.ITReferenceNumber = "V2";
			AssertEquals("Release number", "V2", wrapper.ITReferenceNumber);
			AssertEquals("Cartage Ref", "MB2", wrapper.AMSNumber);
		}

		public void TestFreightJob()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			ForwardingContainer container = consol.Containers.AddNew();
			ContainerWrapperFromFreight wrapper = new ContainerWrapperFromFreight(container, Factory);
			AssertEquals("Wrapped business object should be CommonConsol", "C00001111", wrapper.FreightJob.JobNumber);
		}

		public void TestIsReefer()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			ContainerWrapperFromCartage wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			AssertEquals("Container: IsReefer", true, wrapper.IsReefer);
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("Container: IsNotReefer", false, wrapper.IsReefer);
		}

		public void TestIsHazardous()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(consol, container);

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;
			var move = Factory.New<CommonBookedCtgMove>();
			move.EW_JC_Container = container.PK;
			cartage.ContainerBookedMoves.Add(move);

			ContainerWrapperFromCartage wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);

			container.JC_RH_NKContainerCommodityCode = "HAZ";
			AssertEquals("Container: IsHazardous", true, wrapper.IsHazardous);
			container.JC_RH_NKContainerCommodityCode = "GEN";
			AssertEquals("Container: IsNotHazardous", false, wrapper.IsHazardous);
			packline.JL_RH_NKCommodityCode = "HAZ";
			AssertEquals("PackLine: IsHazardous", true, wrapper.IsHazardous);
			packline.JL_RH_NKCommodityCode = "GEN";
			AssertEquals("PackLine: IsNotHazardous", false, wrapper.IsHazardous);
			container.JC_RH_NKContainerCommodityCode = "";
			packline.JL_RH_NKCommodityCode = "";
			AssertEquals("Container & PackLine: IsEmpty", false, wrapper.IsHazardous);
		}

		public override void TestWrapperMappingFull()
		{
			var registryDeliveryList = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value;
			registryDeliveryList.Add(new DeliveryMode { Code = "EFG", Description = (NoResString)"Blah blah blah", UserDefinedCode = "EFG", UserDefinedDescription = (NoResString)"Blah blah blah" });
			FreightDataRegistry.Instance.ContainerDeliveryModeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDeliveryList);

			var exportYard = Factory.New<OrgHeader>();
			exportYard.OH_FullName = "TEST DEPARTURE CONTAINER YARD ADDRESS";
			var arrivalAddress = Factory.New<OrgHeader>();
			arrivalAddress.OH_FullName = "TEST ARRIVAL CONTAINER YARD ADDRESS";
			var cfsClient = Factory.New<OrgHeader>();
			cfsClient.OH_FullName = "THE COOLEST CFS CLIENT OUT";

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_ISOType = "2340";
			containerType.RC_TareWeight = 1450m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MASTERCAT";
			consol.JK_BookingReference = "MNZXC98735";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000011";
			container.JC_RC = containerType.PK;
			container.JC_ContainerMode = "FCX";
			container.JC_SealNum = "SEAL_ROCKS";
			container.JC_AdditionalSealNum = "SEAL_ROCKS_RULE";
			container.JC_Additional2SealNum = "SEAL_ROCKS_SUCK";
			container.JC_ReleaseNum = "PERSIST AND WIN";
			container.JC_ContainerImportDORelease = "OBEYTHEFREIGHT";
			container.JC_DunnageWeight = 340m;
			container.JC_EmptyReadyForReturn = new ZDateTime(2011, 4, 5);
			container.JC_EmptyReturnedBy = new ZDateTime(2007, 8, 15);
			container.JC_EmptyRequired = new ZDateTime(2007, 8, 12);
			container.JC_FCLWharfGateOut = new ZDateTime(2007, 8, 20);
			container.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2008, 5, 13);
			container.JC_ArrivalEstimatedDelivery = new ZDateTime(2011, 4, 1);
			container.JC_ArrivalSlotReference = "HJK3435ML";
			container.JC_DepartureSlotReference = "MHUI78P934O";
			container.JC_ArrivalSlotDateTime = new ZDateTime(2007, 8, 24);
			container.JC_DepartureSlotDateTime = new ZDateTime(2007, 8, 25);
			container.JC_ArrivalCartageRef = "ARV111";
			container.JC_DepartureCartageRef = "DEP222";
			container.JC_DeliveryMode = "EFG";
			container.JC_ExportDepotCustomsReference = "MKJ1223REF";
			container.JC_EmptyReturnReference = "REEE123";
			container.JC_DepartureEstimatedPickup = new ZDateTime(2007, 8, 13);
			container.JC_TotalLength = 39.87m;
			container.JC_TotalWidth = 3.79m;
			container.JC_TotalHeight = 0.75m;
			container.JC_SetPointTemp = 23.4m;
			container.JC_SetPointTempUnit = "C";
			container.JC_IsDamaged = true;
			container.JC_HumidityPercent = new ZByte(23);
			container.JC_AirVentFlow = 89.3m;
			container.JC_AirVentFlowRateUnit = "P1";
			container.JC_RefrigGeneratorID = "R123";
			container.JC_OA_DepartureContainerYardAddress = exportYard.MainAddress.PK;
			container.JC_OA_ArrivalContainerYardAddress = arrivalAddress.MainAddress.PK;
			container.JC_ContainerJobID = "JobID";
			container.JC_ContainerQuality = "FOD";
			container.JC_OH_CFSClient = cfsClient.PK;
			container.JC_UnpackShed = "WOOP WOOP";
			container.JC_StowagePosition = "ABCDEF";

			var number = container.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = "AMS";
			number.CE_EntryNum = "AMS0001";

			var shipment = consol.Shipments.AddNew();

			var package = shipment.OuterPackLines.AddNew();
			package.JL_PackageCount = 23;
			package.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			package.JL_JC = container.PK;
			package.JL_ActualWeight = 23630m;
			package.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			package.JL_ActualVolume = 23.32m;
			package.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			package.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var service = container.JobContainer.Services.AddNew();
			service.ES_ServiceCode = "CLN";

			var wrapperFull = new ContainerWrapperFromFreight(container, Factory);
			AssertEquals("wrapperFull.Mode.Code", "FCX", wrapperFull.Mode.Code);
			AssertEquals("wrapperFull.Type.Code", "ZZGP", wrapperFull.Type.Code);
			AssertEquals("wrapperFull.Type.Description", "General Purpose Box", wrapperFull.Type.Description);
			AssertEquals("wrapperFull.VolumeGoods.Value", 23.32m, wrapperFull.VolumeGoods.Value);
			AssertEquals("wrapperFull.VolumeGoods.Unit.Code", "M3", wrapperFull.VolumeGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightTare.Value", 1450m, wrapperFull.WeightTare.Value);
			AssertEquals("wrapperFull.WeightTare.Unit.Code", "KG", wrapperFull.WeightTare.Unit.Code);
			AssertEquals("wrapperFull.WeightGoods.Value", 23630m, wrapperFull.WeightGoods.Value);
			AssertEquals("wrapperFull.WeightGoods.Unit.Code", "KG", wrapperFull.WeightGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightDunnage.Value", 340m, wrapperFull.WeightDunnage.Value);
			AssertEquals("wrapperFull.WeightDunnage.Unit.Code", "KG", wrapperFull.WeightDunnage.Unit.Code);
			AssertEquals("wrapperFull.WeightGross.Value", 25420m, wrapperFull.WeightGross.Value);
			AssertEquals("wrapperFull.WeightGross.Unit.Code", "KG", wrapperFull.WeightGross.Unit.Code);
			AssertEquals("wrapperFull.SetPointTemperature.Value", 23.4m, wrapperFull.SetPointTemperature.Value);
			AssertEquals("wrapperFull.SetPointTemperature.Unit.Code", "C", wrapperFull.SetPointTemperature.Unit.Code);
			AssertEquals("wrapperFull.ContainerNo", "OOCL0000011", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "OOCL0000011", wrapperFull.ContainerNumberOrTypeCount);
			AssertEquals("wrapperFull.SealNo", "SEAL_ROCKS", wrapperFull.SealNo);
			AssertEquals("wrapperFull.SealNo2", "SEAL_ROCKS_RULE", wrapperFull.SealNo2);
			AssertEquals("wrapperFull.SealNo3", "SEAL_ROCKS_SUCK", wrapperFull.SealNo3);
			AssertEquals("wrapperFull.ReleaseNumber", "PERSIST AND WIN", wrapperFull.ReleaseNumber);
			AssertEquals("wrapperFull.ArrivalEstimatedDelivery", new ZDateTime(2011, 4, 1), wrapperFull.ArrivalEstimatedDelivery);
			AssertEquals("wrapperFull.ArrivalReleaseNumber", "OBEYTHEFREIGHT", wrapperFull.ArrivalReleaseNumber);
			AssertEquals("wrapperFull.Services.Count", 1, wrapperFull.Services.Count);
			AssertEquals("wrapperFull.Services[0].Type.Code", "CLN", wrapperFull.Services[0].Type.Code);
			AssertEquals("wrapperFull.EmptyReadyForReturn", new ZDateTime(2011, 4, 5), wrapperFull.EmptyReadyForReturn);
			AssertEquals("wrapperFull.EmptyReturnedBy", new ZDateTime(2007, 8, 15), wrapperFull.EmptyReturnedBy);
			AssertEquals("wrapperFull.EmptyReturnReference", "REEE123", wrapperFull.EmptyReturnReference);
			AssertEquals("wrapperFull.ContainerYardEmptyReturnGateIn", new ZDateTime(2008, 5, 13), wrapperFull.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperFull.PackCount.ValueAndUnitCodeBlankIfZero", "23 PKG", wrapperFull.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFull.BookingReference", "MNZXC98735", wrapperFull.BookingReference);
			AssertEquals("wrapperFull.ArrivalSlotReference", "HJK3435ML", wrapperFull.ArrivalSlotReference);
			AssertEquals("wrapperFull.DepartureSlotReference", "MHUI78P934O", wrapperFull.DepartureSlotReference);
			AssertEquals("wrapperFull.ArrivalSlotTime", new ZDateTime(2007, 8, 24), wrapperFull.ArrivalSlotTime);
			AssertEquals("wrapperFull.DepartureSlotTime", new ZDateTime(2007, 8, 25), wrapperFull.DepartureSlotTime);
			AssertEquals("ArrivalCartageRef", "ARV111", wrapperFull.ArrivalCartageRef);
			AssertEquals("DepartureCartageRef", "DEP222", wrapperFull.DepartureCartageRef);
			AssertEquals("wrapperFull.DeliveryMode.Description", "Blah blah blah", wrapperFull.DeliveryMode.Description);
			AssertEquals("wrapperFull.ExportDepotCustomsReference", "MKJ1223REF", wrapperFull.ExportDepotCustomsReference);
			AssertEquals("wrapperFull.EmptyRequired", new ZDateTime(2007, 8, 12), wrapperFull.EmptyRequired);
			AssertEquals("wrapperFull.WharfGateOut", new ZDateTime(2007, 8, 20), wrapperFull.WharfGateOut);
			AssertEquals("wrapperFull.DepartureEstimatedPickup", new ZDateTime(2007, 8, 13), wrapperFull.DepartureEstimatedPickup);
			AssertEquals("wrapperFull.Length", 39.87m, wrapperFull.Length);
			AssertEquals("wrapperFull.Width", 3.79m, wrapperFull.Width);
			AssertEquals("wrapperFull.Height", 0.75m, wrapperFull.Height);
			Assert("wrapperFull.Damaged", wrapperFull.Damaged);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
			Assert("wrapperFull.ControlledAtmosphere", wrapperFull.ControlledAtmosphere);
			AssertEquals("wrapperFull.HumidityPercentage", new ZByte(23), wrapperFull.HumidityPercentage);
			AssertEquals("wrapperFull.AirVentFlow.ValueAndUnitCodeBlankIfZero", "89 P1", wrapperFull.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFull.ClipOnUnit", "R123", wrapperFull.ClipOnUnit);
			AssertEquals("wrapperFull.UNDGSubstances.Count", 1, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.UNDGSubstances[0].UNNumber", "0004", wrapperFull.UNDGSubstances[0].UNNumber);
			AssertEquals("wrapperFull.DepartureContainerYardAddress.CompanyName", "TEST DEPARTURE CONTAINER YARD ADDRESS", wrapperFull.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ArrivalContainerYardAddress.CompanyName", "TEST ARRIVAL CONTAINER YARD ADDRESS", wrapperFull.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ContainerJobID", "JobID", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.Commodity.Count", 1, wrapperFull.Commodities.Count);
			AssertEquals("wrapperFull.Commodity[0].Code", "GEN", wrapperFull.Commodities[0].Code);
			AssertEquals("wrapperFull.Commodity[0].Description", "General", wrapperFull.Commodities[0].Description);
			AssertEquals("wrapperFull.IsChargeable", "No", wrapperFull.IsChargeable);
			AssertEquals("wrapperFull.IsPalletized", "No", wrapperFull.IsPalletized);
			AssertEquals("wrapperFull.Items", "0", wrapperFull.Packages);
			AssertEquals("wrapperFull.Pallets", "0", wrapperFull.Pallets);
			AssertEquals("wrapperFull.OriginConfirm.PlannedPickupTime", new ZDateTime(2007, 8, 13), wrapperFull.OriginConfirm.PlannedPickupTime);
			AssertEquals("wrapperFull.ContainerQuality", "FOD", wrapperFull.ContainerQuality.Code);
			AssertEquals("wrapperFull.PrintTACImage", ZBool.False, wrapperFull.PrintTACImage);
			AssertEquals("wrapperFull.ImportDetention.Released", ZDateTime.Empty, wrapperFull.ImportDetention.Released);
			AssertEquals("wrapperFull.ExportDetention.Released", ZDateTime.Empty, wrapperFull.ExportDetention.Released);
			AssertEquals("wrapperFull.CFSClient.CompanyName", "THE COOLEST CFS CLIENT OUT", wrapperFull.CFSClient.CompanyName);
			AssertEquals("wrapperFull.UnpackShed", "WOOP WOOP", wrapperFull.UnpackShed);
			AssertEquals("wrapperFull.StowagePosition", "ABCDEF", wrapperFull.StowagePosition);

			var entry = wrapperFull.CustomsEntries[0];
			AssertEquals("entry.EntryType.Code", "AMS", entry.EntryType.Code);
			AssertEquals("entry.EntryType.Code", "AMS0001", entry.EntryNumber);
		}

		public void TestTotalsByShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var ship1packLine = shipment1.OuterPackLines.AddNew();
			ship1packLine.SetContainer(container1.PK);
			ship1packLine.JL_PackageCount = 3;
			ship1packLine.JL_ActualVolume = 4m;
			ship1packLine.JL_ActualWeight = 5m;

			var ship2packLine1 = shipment2.OuterPackLines.AddNew();
			ship2packLine1.SetContainer(container1.PK);
			ship2packLine1.JL_PackageCount = 6;
			ship2packLine1.JL_ActualVolume = 7m;
			ship2packLine1.JL_ActualWeight = 8m;

			var ship2packLine2 = shipment1.OuterPackLines.AddNew();
			ship2packLine2.SetContainer(container2.PK);
			ship2packLine2.JL_PackageCount = 9;
			ship2packLine2.JL_ActualVolume = 10m;
			ship2packLine2.JL_ActualWeight = 11m;

			var wrapper = new ContainerWrapperFromFreight(container1, shipment1, Factory);
			AssertEquals(3m, wrapper.PackCount.Value);
			AssertEquals(4m, wrapper.VolumeGoods.Value);
			AssertEquals(5m, wrapper.WeightGoods.Value);

			wrapper = new ContainerWrapperFromFreight(container2, shipment2, Factory);
			AssertEquals(0m, wrapper.PackCount.Value);
			AssertEquals(0m, wrapper.VolumeGoods.Value);
			AssertEquals(0m, wrapper.WeightGoods.Value);

			wrapper = new ContainerWrapperFromFreight(container1, Factory);
			AssertEquals(9m, wrapper.PackCount.Value);
			AssertEquals(11m, wrapper.VolumeGoods.Value);
			AssertEquals(13m, wrapper.WeightGoods.Value);
		}

		public void TestTotalsByShipment_BCN()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var bcnShipment = consol.Shipments.AddNew();
			var subShipment = consol.Shipments.AddNew();

			bcnShipment.JS_ShipmentType = "BCN";
			bcnShipment.CoLoadShipments.Add(subShipment);

			var bcnShipPackLine = bcnShipment.OuterPackLines.AddNew();
			bcnShipPackLine.SetContainer(container1.PK);
			bcnShipPackLine.JL_PackageCount = 3;
			bcnShipPackLine.JL_ActualVolume = 4m;
			bcnShipPackLine.JL_ActualWeight = 5m;

			var subShipPackLine = subShipment.OuterPackLines.AddNew();
			subShipPackLine.SetContainer(container1.PK);
			subShipPackLine.JL_PackageCount = 6;
			subShipPackLine.JL_ActualVolume = 7m;
			subShipPackLine.JL_ActualWeight = 8m;

			var ship2packLine2 = bcnShipment.OuterPackLines.AddNew();
			ship2packLine2.SetContainer(container2.PK);
			ship2packLine2.JL_PackageCount = 9;
			ship2packLine2.JL_ActualVolume = 10m;
			ship2packLine2.JL_ActualWeight = 11m;

			var wrapper = new ContainerWrapperFromFreight(container1, bcnShipment, Factory);
			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			AssertEquals("BCN shipment totals calculated from all shipments on Arrival", 9m, wrapper.PackCount.Value);
			AssertEquals(11m, wrapper.VolumeGoods.Value);
			AssertEquals(13m, wrapper.WeightGoods.Value);

			wrapper = new ContainerWrapperFromFreight(container1, subShipment, Factory);
			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			AssertEquals("Sub-shipment totals are just from the sub-shipment", 6m, wrapper.PackCount.Value);
			AssertEquals(7m, wrapper.VolumeGoods.Value);
			AssertEquals(8m, wrapper.WeightGoods.Value);

			wrapper = new ContainerWrapperFromFreight(container2, bcnShipment, Factory);
			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			AssertEquals("Second container on BCN shipment", 9m, wrapper.PackCount.Value);
			AssertEquals(10m, wrapper.VolumeGoods.Value);
			AssertEquals(11m, wrapper.WeightGoods.Value);

			wrapper = new ContainerWrapperFromFreight(container1, Factory);
			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			AssertEquals("Second container has no packlines from sub-shipment", 9m, wrapper.PackCount.Value);
			AssertEquals(11m, wrapper.VolumeGoods.Value);
			AssertEquals(13m, wrapper.WeightGoods.Value);

			wrapper = new ContainerWrapperFromFreight(container1, bcnShipment, Factory);
			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP));
			AssertEquals("BCN shipment totals are just from shipment itself on Departure", 3m, wrapper.PackCount.Value);
			AssertEquals(4m, wrapper.VolumeGoods.Value);
			AssertEquals(5m, wrapper.WeightGoods.Value);
		}

		public void TestFrozen()
		{
			ForwardingContainer container = Factory.New<ForwardingContainer>();
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = -5.3m;

			ContainerWrapper wrapperFull = new ContainerWrapperFromFreight(container, Factory);
			Assert("wrapperFull.Frozen", wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", !wrapperFull.Chilled);
		}

		public void TestChilled()
		{
			ForwardingContainer container = Factory.New<ForwardingContainer>();
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = 4.2m;

			ContainerWrapper wrapperFull = new ContainerWrapperFromFreight(container, Factory);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
		}

		public void TestImportDetention()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZDateTime now = ZDateTime.Now;

				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";

				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "AUBNE";
				destination.JB_AvailabilityDate = now.AddDays(-15);

				CommonConsol consol = Factory.New<CommonConsol>();
				consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

				CommonContainer container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TEST4100013";
				container.JC_EmptyReturnedBy = now.AddDays(-6);
				container.JC_ContainerYardEmptyReturnGateIn = now.AddDays(-1);

				ContainerWrapperFromFreight wrapper = new ContainerWrapperFromFreight(container, Factory);

				AssertEquals("wrapper.ImportDetention.Released", now.AddDays(-15), wrapper.ImportDetention.Released);
				AssertEquals("wrapper.ImportDetention.LastFreeDay", now.AddDays(-6), wrapper.ImportDetention.LastFreeDay);
				AssertEquals("wrapper.ImportDetention.Returned", now.AddDays(-1), wrapper.ImportDetention.Returned);

				AssertEquals("wrapper.ImportDetention.FreeDays", 10, wrapper.ImportDetention.FreeDays);
				AssertEquals("wrapper.ImportDetention.DetentionDays", 5, wrapper.ImportDetention.DetentionDays);
			}
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450.156m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container.JC_RC = containerType.PK;
			container.JC_DunnageWeight = 340.167m;
			container.JC_TotalLength = 39.47m;
			container.JC_TotalWidth = 3.79m;
			container.JC_TotalHeight = 0.75m;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline.JL_PackageCount = 23;
			packline.JL_JC = container.PK;
			packline.JL_ActualWeight = 23630.157m;
			packline.JL_ActualVolume = 23.323m;

			var wrapper = new ContainerWrapperFromFreight(container, Factory);

			AssertEquals(1450.16m, wrapper.WeightTare.Value);
			AssertEquals("KG", wrapper.WeightTare.Unit.Code);
			AssertEquals("1450.16 KG", wrapper.WeightTare.ValueAndUnitCode);
			AssertEquals(23630.16m, wrapper.WeightGoods.Value);
			AssertEquals("KG", wrapper.WeightGoods.Unit.Code);
			AssertEquals("23630.16 KG", wrapper.WeightGoods.ValueAndUnitCode);
			AssertEquals(25420.49m, wrapper.WeightGross.Value);
			AssertEquals("KG", wrapper.WeightGross.Unit.Code);
			AssertEquals("25420.49 KG", wrapper.WeightGross.ValueAndUnitCode);
			AssertEquals(340.17m, wrapper.WeightDunnage.Value);
			AssertEquals("KG", wrapper.WeightDunnage.Unit.Code);
			AssertEquals("340.17 KG", wrapper.WeightDunnage.ValueAndUnitCode);

			AssertEquals(23.32m, wrapper.VolumeGoods.Value);
			AssertEquals("M3", wrapper.VolumeGoods.Unit.Code);
			AssertEquals("23.32 M3", wrapper.VolumeGoods.ValueAndUnitCode);
		}

		public void TestGrossWeight()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_TareWeight = 1234;
			container.JC_GrossWeight = 456;
			container.JC_GrossWeightUQ = "KG";

			var wrapper = new ContainerWrapperFromFreight(container, Factory);
			AssertEquals(1234m, wrapper.WeightTare.Value);
			AssertEquals(456m, wrapper.WeightGross.Value);
			AssertEquals("KG", wrapper.WeightGross.Unit.Code);
		}

		[TestDate(2016, 1, 2, 3, 4, 5)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestVGM()
		{
			var container = Factory.New<ForwardingContainer>();
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

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromFreight(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var registryDeliveryList = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value;
			registryDeliveryList.Add(new DeliveryMode { Code = "ABC", Description = (NoResString)"Blah blah blah", UserDefinedCode = "ABC", UserDefinedDescription = (NoResString)"Blah blah blah" });
			FreightDataRegistry.Instance.ContainerDeliveryModeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDeliveryList);

			var exportYard = Factory.New<OrgHeader>();
			exportYard.OH_FullName = "TEST DEPARTURE CONTAINER YARD ADDRESS";
			exportYard.MainAddress.OA_RN_NKCountryCode = "AU";

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450m;

			var consol = Factory.New<ForwardingConsol>();

			var container = consol.Containers.AddNew();
			container.JC_RC = containerType.PK;
			container.JC_ContainerMode = "FCX";
			container.JC_DunnageWeight = 340m;
			container.JC_DeliveryMode = "ABC";
			container.JC_TotalLength = 39.87m;
			container.JC_TotalWidth = 3.79m;
			container.JC_TotalHeight = 0.75m;
			container.JC_SetPointTemp = 23.4m;
			container.JC_SetPointTempUnit = "C";
			container.JC_AirVentFlow = 89.3m;
			container.JC_AirVentFlowRateUnit = "P1";
			container.JC_OA_DepartureContainerYardAddress = exportYard.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();

			var package = shipment.OuterPackLines.AddNew();
			package.JL_PackageCount = 23;
			package.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			package.JL_JC = container.PK;
			package.JL_ActualWeight = 23630m;
			package.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			package.JL_ActualVolume = 23.32m;
			package.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			return new ContainerWrapperFromFreight(container, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 89 P1
ArrivalContainerYardAddress : 
CFSClient : 
ContainerQuality : 
DeliveryMode : ABC - Blah blah blah
DepartureContainerYardAddress : TEST DEPARTURE CONTAINER YARD ADDRESS\nAUSTRALIA
ExportDetention : 
FreightJob : 
ImportDetention : 
Mode : FCX
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
