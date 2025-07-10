using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromCartage))]
	sealed class ContainerWrapperFromCartageTest : ContainerWrapperTest
	{
		public void TestArrivalReleaseNumberAndArrivalCartageRef()
		{
			var cartage = Factory.New<CommonCartage>();
			var container = Factory.New<ForwardingContainer>();
			var wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			container.JC_ArrivalPickupByRail = true;
			container.AMSNumber = "MB2";
			container.ITReferenceNumber = "V2";
			AssertEquals("Release number", "V2", wrapper.ITReferenceNumber);
			AssertEquals("Cartage Ref", "MB2", wrapper.AMSNumber);
		}

		public void TestFreightJob()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "22222";
			ForwardingContainer container = Factory.New<ForwardingContainer>();

			ContainerWrapperFromCartage wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), null, Factory);

			AssertEquals("Wrapped business object should be CommonCartage", "22222", wrapper.FreightJob.JobNumber);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S7777";
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;

			AssertEquals("Wrapped business object should be Shipment", "S7777", wrapper.FreightJob.JobNumber);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C2468";
			consol.Containers.Add(container);

			wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			AssertEquals("Wrapped business object should be Consol", "C2468", wrapper.FreightJob.JobNumber);
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

		public void TestImportDetention()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				CommonCartage cartage = Factory.New<CommonCartage>();

				CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_ContainerNum = "OOCL0000011";
				container.JC_FCLAvailable = new ZDateTime(2010, 1, 1);
				container.JC_EmptyReturnedBy = new ZDateTime(2010, 1, 10);
				container.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2010, 1, 15);

				ContainerWrapper wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
				AssertEquals("wrapper.ImportDetention.Released", new ZDateTime(2010, 1, 1), wrapper.ImportDetention.Released);
				AssertEquals("wrapper.ImportDetention.LastFreeDay", new ZDateTime(2010, 1, 10), wrapper.ImportDetention.LastFreeDay);
				AssertEquals("wrapper.ImportDetention.Returned", new ZDateTime(2010, 1, 15), wrapper.ImportDetention.Returned);
				AssertEquals("wrapper.ImportDetention.FreeDays", 10, wrapper.ImportDetention.FreeDays);
				AssertEquals("wrapper.ImportDetention.DetentionDays", 5, wrapper.ImportDetention.DetentionDays);
			}
		}

		public override void TestWrapperMappingFull()
		{
			var departureAddress = Factory.New<JobDocAddress>();
			departureAddress.E2_CompanyName = "TEST DEPARTURE CONTAINER YARD ADDRESS";
			var arrivalAddress = Factory.New<OrgHeader>();
			arrivalAddress.OH_FullName = "TEST ARRIVAL CONTAINER YARD ADDRESS";

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_ISOType = "2340";
			containerType.RC_TareWeight = 1450m;

			var cartage = Factory.New<CommonCartage>();
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = containerType.PK;
			container.JC_ContainerMode = "ABX";
			container.JC_DunnageWeight = 168m;
			container.JC_GrossWeight = container.JC_DunnageWeight + container.JC_TareWeight + 333m;
			container.JC_ContainerNum = "OOCL0000011";
			container.JC_SealNum = "HATE_DOCS";
			container.JC_AdditionalSealNum = "HATE_DOCS_MORE";
			container.JC_Additional2SealNum = "HATE_DOCS_EVEN_MORE";
			container.JC_ReleaseNum = "PERSISTENCEWINS";
			container.JC_ContainerImportDORelease = "OBEYTHEFREIGHT";
			container.JC_EmptyReadyForReturn = new ZDateTime(2011, 4, 5);
			container.JC_EmptyReturnedBy = new ZDateTime(2007, 3, 25);
			container.JC_EmptyRequired = new ZDateTime(2007, 3, 23);
			container.JC_FCLWharfGateOut = new ZDateTime(2007, 3, 24);
			container.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2006, 5, 23);
			container.JC_ArrivalEstimatedDelivery = new ZDateTime(2011, 4, 1);
			container.JC_ArrivalCartageRef = "ARV111";
			container.JC_DepartureCartageRef = "DEP222";
			container.JC_ArrivalSlotReference = "MLJK1234V";
			container.JC_DepartureSlotReference = "PQW0983B";
			container.JC_ArrivalSlotDateTime = new ZDateTime(2007, 8, 24);
			container.JC_DepartureSlotDateTime = new ZDateTime(2007, 8, 25);
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;
			container.JC_ExportDepotCustomsReference = "LKL89435(REF)";
			container.JC_EmptyReturnReference = "REEE123";
			container.JC_DepartureEstimatedPickup = new ZDateTime(2007, 8, 12);
			container.JC_TotalLength = 12.34m;
			container.JC_TotalWidth = 9.10m;
			container.JC_TotalHeight = 3.87m;
			container.JC_SetPointTemp = 34.9m;
			container.JC_SetPointTempUnit = "F";
			container.JC_IsDamaged = true;
			container.JC_IsControlledAtmosphere = true;
			container.JC_HumidityPercent = new ZByte(78);
			container.JC_AirVentFlow = 345;
			container.JC_AirVentFlowRateUnit = "MQH";
			container.JC_RefrigGeneratorID = "ABC123";
			container.JC_OA_DepartureContainerYardAddress = departureAddress.PK;
			container.JC_OA_ArrivalContainerYardAddress = arrivalAddress.MainAddress.PK;
			container.JC_ContainerJobID = "JobID";
			container.JC_ContainerQuality = "FOD";
			container.JC_StowagePosition = "ABC1";

			var move = cartage.GetBookedMoves(container)[0];
			move.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "A", "IMO").First().PK;

			var service = container.Services.AddNew();
			service.ES_ServiceCode = "FUM";

			var wrapperFull = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			AssertEquals("wrapperFull.Mode.Code", "ABX", wrapperFull.Mode.Code);
			AssertEquals("wrapperFull.Type.Code", "ZZGP", wrapperFull.Type.Code);
			AssertEquals("wrapperFull.Type.Description", "General Purpose Box", wrapperFull.Type.Description);
			AssertEquals("wrapperFull.WeightTare.Value", 1450m, wrapperFull.WeightTare.Value);
			AssertEquals("wrapperFull.WeightTare.Unit.Code", "KG", wrapperFull.WeightTare.Unit.Code);
			AssertEquals("wrapperFull.WeightGoods.Value", 333m, wrapperFull.WeightGoods.Value);
			AssertEquals("wrapperFull.WeightGoods.Unit.Code", "KG", wrapperFull.WeightGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightDunnage.Value", 168m, wrapperFull.WeightDunnage.Value);
			AssertEquals("wrapperFull.WeightDunnage.Unit.Code", "KG", wrapperFull.WeightDunnage.Unit.Code);
			AssertEquals("wrapperFull.WeightGross.Value", 1951m, wrapperFull.WeightGross.Value);
			AssertEquals("wrapperFull.WeightGross.Unit.Code", "KG", wrapperFull.WeightGross.Unit.Code);
			AssertEquals("wrapperFull.ContainerNo", "OOCL0000011", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "OOCL0000011", wrapperFull.ContainerNumberOrTypeCount);
			AssertEquals("wrapperFull.SealNo", "HATE_DOCS", wrapperFull.SealNo);
			AssertEquals("wrapperFull.SealNo2", "HATE_DOCS_MORE", wrapperFull.SealNo2);
			AssertEquals("wrapperFull.SealNo3", "HATE_DOCS_EVEN_MORE", wrapperFull.SealNo3);
			AssertEquals("wrapperFull.ReleaseNumber", "PERSISTENCEWINS", wrapperFull.ReleaseNumber);
			AssertEquals("wrapperFull.ArrivalReleaseNumber", "OBEYTHEFREIGHT", wrapperFull.ArrivalReleaseNumber);
			AssertEquals("wrapperFull.Services.Count", 1, wrapperFull.Services.Count);
			AssertEquals("wrapperFull.Services[0].Type.Code", "FUM", wrapperFull.Services[0].Type.Code);
			AssertEquals("wrapperFull.EmptyReadyForReturn", new ZDateTime(2011, 4, 5), wrapperFull.EmptyReadyForReturn);
			AssertEquals("wrapperFull.EmptyReturnedBy", new ZDateTime(2007, 3, 25), wrapperFull.EmptyReturnedBy);
			AssertEquals("wrapperFull.WharfGateOut", new ZDateTime(2007, 3, 24), wrapperFull.WharfGateOut);
			AssertEquals("wrapperFull.ContainerYardEmptyReturnGateIn", new ZDateTime(2006, 5, 23), wrapperFull.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperFull.PackCount.Value", 0m, wrapperFull.PackCount.Value);
			AssertEquals("wrapperFull.PackCount.Unit.Code", "PLT", wrapperFull.PackCount.Unit.Code);
			AssertEquals("wrapperFull.Commodity.Count", 0, wrapperFull.Commodities.Count);
			AssertEquals("wrapperFull.BookingReference", "", wrapperFull.BookingReference);
			AssertEquals("wrapperFull.ArrivalEstimatedDelivery", new ZDateTime(2011, 4, 1), wrapperFull.ArrivalEstimatedDelivery);
			AssertEquals("ArrivalCartageRef", "ARV111", wrapperFull.ArrivalCartageRef);
			AssertEquals("DepartureCartageRef", "DEP222", wrapperFull.DepartureCartageRef);
			AssertEquals("wrapperFull.ArrivalSlotReference", "MLJK1234V", wrapperFull.ArrivalSlotReference);
			AssertEquals("wrapperFull.DepartureSlotReference", "PQW0983B", wrapperFull.DepartureSlotReference);
			AssertEquals("ArrivalSlotTime", new ZDateTime(2007, 8, 24), wrapperFull.ArrivalSlotTime);
			AssertEquals("DepartureSlotTime", new ZDateTime(2007, 8, 25), wrapperFull.DepartureSlotTime);
			AssertEquals("wrapperFull.DeliveryMode.Description", "CFS/CFS", wrapperFull.DeliveryMode.Description);
			AssertEquals("wrapperFull.ExportDepotCustomsReference", "LKL89435(REF)", wrapperFull.ExportDepotCustomsReference);
			AssertEquals("wrapperFull.EmptyRequired", new ZDateTime(2007, 3, 23), wrapperFull.EmptyRequired);
			AssertEquals("wrapperFull.EmptyReturnReference", "REEE123", wrapperFull.EmptyReturnReference);
			AssertEquals("wrapperFull.DepartureEstimatedPickup", new ZDateTime(2007, 8, 12), wrapperFull.DepartureEstimatedPickup);
			AssertEquals("wrapperFull.Length", 12.34m, wrapperFull.Length);
			AssertEquals("wrapperFull.Width", 9.10m, wrapperFull.Width);
			AssertEquals("wrapperFull.Height", 3.87m, wrapperFull.Height);
			AssertEquals("wrapperFull.SetPointTemperature.ValueAndUnitCodeBlankIfZero", "34.9 F", wrapperFull.SetPointTemperature.ValueAndUnitCodeBlankIfZero);
			Assert("wrapperFull.Damaged", wrapperFull.Damaged);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
			Assert("wrapperFull.ControlledAtmosphere", wrapperFull.ControlledAtmosphere);
			AssertEquals("wrapperFull.HumidityPercentage", new ZByte(78), wrapperFull.HumidityPercentage);
			AssertEquals("wrapperFull.AirVentFlow.ValueAndUnitCodeBlankIfZero", "345 MQH", wrapperFull.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFull.ClipOnUnit", "ABC123", wrapperFull.ClipOnUnit);
			AssertEquals("wrapperFull.UNDGSubstances.Count", 1, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.UNDGSubstances[0].UNNumber", "0004", wrapperFull.UNDGSubstances[0].UNNumber);
			AssertEquals("wrapperFull.DepartureContainerYardAddress.CompanyName", "TEST DEPARTURE CONTAINER YARD ADDRESS", wrapperFull.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ArrivalContainerYardAddress.CompanyName", "TEST ARRIVAL CONTAINER YARD ADDRESS", wrapperFull.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ContainerJobID", "JobID", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.IsChargeable", "No", wrapperFull.IsChargeable);
			AssertEquals("wrapperFull.IsPalletized", "No", wrapperFull.IsPalletized);
			AssertEquals("wrapperFull.Items", "0", wrapperFull.Packages);
			AssertEquals("wrapperFull.Pallets", "0", wrapperFull.Pallets);
			AssertEquals("wrapperFull.ContainerQuality", "FOD", wrapperFull.ContainerQuality.Code);
			AssertEquals("wrapperFull.PrintTACImage", ZBool.False, wrapperFull.PrintTACImage);
			AssertEquals("wrapperFull.ExportDetention.Released", ZDateTime.Empty, wrapperFull.ExportDetention.Released);
			AssertEquals("wrapperFull.ImportDetention.Released", ZDateTime.Empty, wrapperFull.ImportDetention.Released);
			AssertEquals("wrapperFull.CFSClient", null, wrapperFull.CFSClient);
			AssertEquals("wrapperFull.UnpackShed", ZString.Empty, wrapperFull.UnpackShed);
			AssertEquals("wrapperFull.StowagePosition", "ABC1", wrapperFull.StowagePosition);
		}

		public void TestPackCount()
		{
			var cartage = Factory.New<CommonCartage>();
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var containerWrapper1 = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			AssertEquals("0 PLT", containerWrapper1.PackCount.ValueAndUnitCode);

			var move = cartage.GetBookedMoves(container)[0];
			move.EW_BookedPackCount = 10;
			move.EW_F3_NKPackType = "PLT";
			var containerWrapper2 = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			AssertEquals("10 PLT", containerWrapper2.PackCount.ValueAndUnitCode);
		}

		public void TestContainerWrapperWithMTCartageLeg()
		{
			var cartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_TareWeight = 1200m;
			container.JC_GrossWeight = 2200m;
			container.JC_GrossWeightUQ = "KG";
			var move = cartage.GetBookedMoves(container)[0];
			move.EW_BookedPackCount = 10;
			move.EW_F3_NKPackType = "PLT";
			var leg = move.CartageLegs.AddNew();
			leg.JU_IsEmptyContainer = false;

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			cartageWrapper.LocalTransportLegs.RemoveAndDeleteAll();
			cartageWrapper.LocalTransportLegs.Add(new LocalTransportLegWrapper(leg, Factory));
			var containerWrapper1 = new ContainerWrapperFromCartage(cartageWrapper, container, Factory);
			AssertEquals("containerWrapper1.PackCount.ValueAndUnitCode", "10 PLT", containerWrapper1.PackCount.ValueAndUnitCode);
			AssertEquals("containerWrapper1.WeightGross.ValueAndUnitCode", "2200.000 KG", containerWrapper1.WeightGross.ValueAndUnitCode);
			AssertEquals("containerWrapper1.WeightGoods.ValueAndUnitCode", "1000.000 KG", containerWrapper1.WeightGoods.ValueAndUnitCode);

			leg.JU_IsEmptyContainer = true;

			var containerWrapper2 = new ContainerWrapperFromCartage(cartageWrapper, container, Factory);
			AssertEquals("containerWrapper1.PackCount.ValueAndUnitCode", "0", containerWrapper2.PackCount.ValueAndUnitCode);
			AssertEquals("containerWrapper1.WeightGross.ValueAndUnitCode", "1200.000 KG", containerWrapper2.WeightGross.ValueAndUnitCode);
			AssertEquals("containerWrapper1.WeightGoods.ValueAndUnitCode", "0.0", containerWrapper2.WeightGoods.ValueAndUnitCode);
		}

		public void TestExceptionIsNotThrownWhenLocalTransportLegsHasMoreThanOneLeg()
		{
			var cartage = Factory.New<CommonCartage>();
			var wrapper = new FreightWrapperFromCartage(cartage, Factory);
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.GetBookedMoves(container)[0];
			move.EW_BookedPackCount = 10;
			move.EW_F3_NKPackType = "PLT";
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();

			var cartageWrapper = new FreightWrapperFromCartage(cartage, Factory);
			cartageWrapper.LocalTransportLegs.RemoveAndDeleteAll();
			cartageWrapper.LocalTransportLegs.Add(new LocalTransportLegWrapper(leg1, Factory));
			cartageWrapper.LocalTransportLegs.Add(new LocalTransportLegWrapper(leg2, Factory));
			var containerWrapper1 = new ContainerWrapperFromCartage(cartageWrapper, container, Factory);
			AssertNoExceptionThrown("Accessing Goods Weight", () => { var test = containerWrapper1.WeightGoods; });
			AssertNoExceptionThrown("Accessing Gross Weight", () => { var test = containerWrapper1.WeightGross; });
			AssertNoExceptionThrown("Accessing Package Count", () => { var test = containerWrapper1.PackCount; });
		}

		public void TestFrozen()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = -5.3m;

			ContainerWrapper wrapperFull = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			Assert("wrapperFull.Frozen", wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", !wrapperFull.Chilled);
		}

		public void TestChilled()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = 7.8m;

			ContainerWrapper wrapperFull = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
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
			containerType.RC_TareWeight = 1450.164m;

			var cartage = Factory.New<CommonCartage>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			cartage.JJ_ParentID = shipment.PK;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = containerType.PK;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container.JC_ContainerMode = "ABX";
			container.JC_DunnageWeight = 168.236m;
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;
			container.JC_GrossWeight = 1663.846m;
			container.JC_JK = consol.PK;
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ActualVolume = 12.369m;
			packline.JL_ActualWeight = 1546.236m;
			packline.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			container.PackLines.Add(packline);

			var wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);

			AssertEquals(1450.16m, wrapper.WeightTare.Value);
			AssertEquals("KG", wrapper.WeightTare.Unit.Code);
			AssertEquals("1450.16 KG", wrapper.WeightTare.ValueAndUnitCode);
			AssertEquals(1546.24m, wrapper.WeightGoods.Value);
			AssertEquals("KG", wrapper.WeightGoods.Unit.Code);
			AssertEquals("1546.24 KG", wrapper.WeightGoods.ValueAndUnitCode);
			AssertEquals(3164.64m, wrapper.WeightGross.Value);
			AssertEquals("KG", wrapper.WeightGross.Unit.Code);
			AssertEquals("3164.64 KG", wrapper.WeightGross.ValueAndUnitCode);
			AssertEquals(168.24m, wrapper.WeightDunnage.Value);
			AssertEquals("KG", wrapper.WeightDunnage.Unit.Code);
			AssertEquals("168.24 KG", wrapper.WeightDunnage.ValueAndUnitCode);

			AssertEquals(12.36m, wrapper.VolumeGoods.Value);
			AssertEquals("M3", wrapper.VolumeGoods.Unit.Code);
			AssertEquals("12.36 M3", wrapper.VolumeGoods.ValueAndUnitCode);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 345 MQH
ArrivalContainerYardAddress : 
CFSClient :  is null
ContainerQuality : 
DeliveryMode : CFS/CFS
DepartureContainerYardAddress : TEST DEPARTURE CONTAINER YARD ADDRESS
ExportDetention : 
FreightJob : 
ImportDetention : 
Mode : ABX
OffHirePort : 
OnHirePort : 
Owner : 
PackCount : 
Registry : (No Default Field Value Available on Registry)
SetPointTemperature : 34.9 F
Status : 
Type : ZZGP - General Purpose Box
VGMMethod : NON - Not Verified
VGMVerifiedByAddress : 
VolumeGoods : 
WeightDunnage : 168.000 KG
WeightGoods : 45.800 KG
WeightGross : 1663.800 KG
WeightTare : 1450.000 KG
";
			}
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_CompanyName = "TEST DEPARTURE CONTAINER YARD ADDRESS";

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450m;

			var cartage = Factory.New<CommonCartage>();
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = containerType.PK;
			container.JC_ContainerMode = "ABX";
			container.JC_DunnageWeight = 168m;
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;
			container.JC_SetPointTemp = 34.9m;
			container.JC_SetPointTempUnit = "F";
			container.JC_AirVentFlow = 345;
			container.JC_AirVentFlowRateUnit = "MQH";
			container.JC_OA_DepartureContainerYardAddress = docAddress.PK;

			container.JC_GrossWeight = 1663.8m;

			return new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromCartage(null, null, Factory);
		}
	}
}
