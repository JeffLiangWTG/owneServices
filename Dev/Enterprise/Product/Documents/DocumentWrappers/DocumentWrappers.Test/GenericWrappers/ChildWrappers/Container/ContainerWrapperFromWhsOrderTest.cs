using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromWhsDocket))]
	sealed class ContainerWrapperFromWhsOrderTest : ContainerWrapperTest
	{
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

		public override void TestWrapperMappingFull()
		{
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";

			var docketContainer = Factory.New<WhsDocketContainer>();
			docketContainer.WC_ContainerNum = "ZZGP";
			docketContainer.WC_IsChargeable = ZBool.True;
			docketContainer.WC_IsPalletised = ZBool.True;
			docketContainer.WC_ItemCount = 50;
			docketContainer.WC_PalletCount = 10;
			docketContainer.WC_RC = containerType.PK;

			var wrapperFull = new ContainerWrapperFromWhsDocket(docketContainer, Factory);
			AssertEquals("wrapperFull.Hazardous", false, wrapperFull.IsHazardous);
			AssertEquals("wrapperFull.Mode.Code", ZString.Empty, wrapperFull.Mode.Code);
			AssertEquals("wrapperFull.DeliveryMode.Code", ZString.Empty, wrapperFull.DeliveryMode.Code);
			AssertEquals("wrapperFull.Type.Code", "ZZGP", wrapperFull.Type.Code);
			AssertEquals("wrapperFull.Type.Description", "General Purpose Box", wrapperFull.Type.Description);
			AssertEquals("wrapperFull.WeightTare.Value", 0m, wrapperFull.WeightTare.Value);
			AssertEquals("wrapperFull.WeightTare.Unit.Code", "KG", wrapperFull.WeightTare.Unit.Code);
			AssertEquals("wrapperFull.WeightGoods.Value", ZDecimal.Zero, wrapperFull.WeightGoods.Value);
			AssertEquals("wrapperFull.WeightGoods.Unit.Code", ZString.Empty, wrapperFull.WeightGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightDunnage.Value", ZDecimal.Zero, wrapperFull.WeightDunnage.Value);
			AssertEquals("wrapperFull.WeightDunnage.Unit.Code", ZString.Empty, wrapperFull.WeightDunnage.Unit.Code);
			AssertEquals("wrapperFull.WeightGross.Value", ZDecimal.Zero, wrapperFull.WeightGross.Value);
			AssertEquals("wrapperFull.WeightGross.Unit.Code", ZString.Empty, wrapperFull.WeightGross.Unit.Code);
			AssertEquals("wrapperFull.VolumeGoods.Value", ZDecimal.Zero, wrapperFull.VolumeGoods.Value);
			AssertEquals("wrapperFull.VolumeGoods.Unit.Code", ZString.Empty, wrapperFull.VolumeGoods.Unit.Code);
			AssertEquals("wrapperFull.PackCount.Value", ZDecimal.Zero, wrapperFull.PackCount.Value);
			AssertEquals("wrapperFull.PackCount.Unit.Code", ZString.Empty, wrapperFull.PackCount.Unit.Code);
			AssertEquals("wrapperFull.Commodity.Count", 0, wrapperFull.Commodities.Count);
			AssertEquals("wrapperFull.Services.Count", 0, wrapperFull.Services.Count);
			AssertEquals("wrapperFull.ContainerNo", "ZZGP", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "ZZGP", wrapperFull.ContainerNumberOrTypeCount);
			AssertEquals("wrapperFull.SealNo", ZString.Empty, wrapperFull.SealNo);
			AssertEquals("wrapperFull.SealNo2", ZString.Empty, wrapperFull.SealNo2);
			AssertEquals("wrapperFull.SealNo3", ZString.Empty, wrapperFull.SealNo3);
			AssertEquals("wrapperFull.ReleaseNumber", ZString.Empty, wrapperFull.ReleaseNumber);
			AssertEquals("wrapperFull.ArrivalEstimatedDelivery", ZDateTime.Empty, wrapperFull.ArrivalEstimatedDelivery);
			AssertEquals("wrapperFull.ArrivalReleaseNumber", ZString.Empty, wrapperFull.ArrivalReleaseNumber);
			AssertEquals("wrapperFull.EmptyReadyForReturn", ZDateTime.Empty, wrapperFull.EmptyReadyForReturn);
			AssertEquals("wrapperFull.EmptyReturnedBy", ZDateTime.Empty, wrapperFull.EmptyReturnedBy);
			AssertEquals("wrapperFull.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapperFull.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperFull.BookingReference", ZString.Empty, wrapperFull.BookingReference);
			AssertEquals("wrapperFull.ArrivalSlotReference", ZString.Empty, wrapperFull.ArrivalSlotReference);
			AssertEquals("wrapperFull.DepartureSlotReference", ZString.Empty, wrapperFull.DepartureSlotReference);
			AssertEquals("wrapperFull.ArrivalSlotTime", ZDateTime.Empty, wrapperFull.ArrivalSlotTime);
			AssertEquals("wrapperFull.DepartureSlotTime", ZDateTime.Empty, wrapperFull.DepartureSlotTime);
			AssertEquals("wrapperFull.ExportDepotCustomsReference", ZString.Empty, wrapperFull.ExportDepotCustomsReference);
			AssertEquals("wrapperFull.EmptyRequired", ZDateTime.Empty, wrapperFull.EmptyRequired);
			AssertEquals("wrapperFull.WharfGateOut", ZDateTime.Empty, wrapperFull.WharfGateOut);
			AssertEquals("wrapperFull.DepartureEstimatedPickup", ZDateTime.Empty, wrapperFull.DepartureEstimatedPickup);
			AssertEquals("wrapperFull.Length", ZDecimal.Zero, wrapperFull.Length);
			AssertEquals("wrapperFull.Width", ZDecimal.Zero, wrapperFull.Width);
			AssertEquals("wrapperFull.Height", ZDecimal.Zero, wrapperFull.Height);
			AssertEquals("wrapperFull.SetPointTemperature.Value", ZDecimal.Zero, wrapperFull.SetPointTemperature.Value);
			AssertEquals("wrapperFull.SetPointTemperature.Unit.Code", ZString.Empty, wrapperFull.SetPointTemperature.Unit.Code);
			Assert("wrapperFull.Damaged", !wrapperFull.Damaged);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", !wrapperFull.Chilled);
			Assert("wrapperFull.ControlledAtmosphere", !wrapperFull.ControlledAtmosphere);
			AssertEquals("wrapperFull.HumidityPercentage", ZByte.Zero, wrapperFull.HumidityPercentage);
			AssertEquals("wrapperFull.AirVentFlow.Value", ZDecimal.Zero, wrapperFull.AirVentFlow.Value);
			AssertEquals("wrapperFull.AirVentFlow.Unit.Code", ZString.Empty, wrapperFull.AirVentFlow.Unit.Code);
			AssertEquals("wrapperFull.ClipOnUnit", ZString.Empty, wrapperFull.ClipOnUnit);
			AssertEquals("wrapperFull.UNDGSubstances.Count", 0, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.DepartureContainerYardAddress.CompanyName", ZString.Empty, wrapperFull.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ContainerJobID", ZString.Empty, wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.IsChargeable", "Yes", wrapperFull.IsChargeable);
			AssertEquals("wrapperFull.IsPalletized", "Yes", wrapperFull.IsPalletized);
			AssertEquals("wrapperFull.Items", "50", wrapperFull.Packages);
			AssertEquals("wrapperFull.Pallets", "10", wrapperFull.Pallets);
			AssertEquals("wrapperFull.ContainerQuality", ZString.Empty, wrapperFull.ContainerQuality.Code);
			AssertEquals("wrapperFull.PrintTACImage", ZBool.False, wrapperFull.PrintTACImage);
			AssertEquals("wrapperFull.ImportDetention.Released", ZDateTime.Empty, wrapperFull.ImportDetention.Released);
			AssertEquals("wrapperFull.ExportDetention.Released", ZDateTime.Empty, wrapperFull.ExportDetention.Released);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (ContainerWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Mode.Code", ZString.Empty, wrapperEmpty.Mode.Code);
			AssertEquals("wrapperEmpty.Type.Code", ZString.Empty, wrapperEmpty.Type.Code);
			AssertEquals("wrapperEmpty.SetPointTemperature", ZString.Empty, wrapperEmpty.SetPointTemperature.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.VolumeGoods.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.VolumeGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightTare.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightTare.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightGoods.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightDunnage.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightDunnage.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightGross.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightGross.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.ContainerNo", ZString.Empty, wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.SealNo", ZString.Empty, wrapperEmpty.SealNo);
			AssertEquals("wrapperEmpty.SealNo2", ZString.Empty, wrapperEmpty.SealNo2);
			AssertEquals("wrapperEmpty.SealNo3", ZString.Empty, wrapperEmpty.SealNo3);
			AssertEquals("wrapperEmpty.Services", 0, wrapperEmpty.Services.Count);
			AssertEquals("wrapperEmpty.PackCount.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.EmptyReturnedBy", ZDateTime.Empty, wrapperEmpty.EmptyReturnedBy);
			AssertEquals("wrapperEmpty.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapperEmpty.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperEmpty.ExportDepotCustomsReference", ZString.Empty, wrapperEmpty.ExportDepotCustomsReference);
			AssertEquals("wrapperEmpty.EmptyRequired", ZDateTime.Empty, wrapperEmpty.EmptyRequired);
			AssertEquals("wrapperEmpty.WharfGateOut", ZDateTime.Empty, wrapperEmpty.WharfGateOut);
			AssertEquals("wrapperEmpty.DepartureEstimatedPickup", ZDateTime.Empty, wrapperEmpty.DepartureEstimatedPickup);
			AssertEquals("wrapperEmpty.Length", ZDecimal.Zero, wrapperEmpty.Length);
			AssertEquals("wrapperEmpty.Width", ZDecimal.Zero, wrapperEmpty.Width);
			AssertEquals("wrapperEmpty.Height", ZDecimal.Zero, wrapperEmpty.Height);
			AssertEquals("wrapper.ContainerJobID", ZString.Empty, wrapperEmpty.ContainerJobID);
			Assert("wrapperEmpty.Damaged", !wrapperEmpty.Damaged);
			Assert("wrapperEmpty.Frozen", !wrapperEmpty.Frozen);
			Assert("wrapperEmpty.Chilled", !wrapperEmpty.Chilled);
			AssertEquals("wrapperEmpty.HumidityPercentage", ZByte.Zero, wrapperEmpty.HumidityPercentage);
			AssertEquals("wrapperEmpty.AirVentFlow.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.UNDGNumbers.Count", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.DepartureContainerYardAddress.CompanyName", ZString.Empty, wrapperEmpty.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperEmpty.ArrivalContainerYardAddress.CompanyName", ZString.Empty, wrapperEmpty.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapperEmpty.IsChargeable", "Yes", wrapperEmpty.IsChargeable);
			AssertEquals("wrapperEmpty.IsPalletized", "No", wrapperEmpty.IsPalletized);
			AssertEquals("wrapperEmpty.Items", "0", wrapperEmpty.Packages);
			AssertEquals("wrapperEmpty.Pallets", "0", wrapperEmpty.Pallets);
			AssertEquals("wrapperEmpty.ImportDetention.Released", ZDateTime.Empty, wrapperEmpty.ImportDetention.Released);
			AssertEquals("wrapperEmpty.ExportDetention.Released", ZDateTime.Empty, wrapperEmpty.ExportDetention.Released);
			AssertEquals("wrapperEmpty.FreightJob", null, wrapperEmpty.FreightJob);
			AssertEquals("wrapperEmpty.CFSClient", null, wrapperEmpty.CFSClient);
			AssertEquals("wrapperEmpty.UnpackShed", ZString.Empty, wrapperEmpty.UnpackShed);
		}

		public void TestGetIsPalletizedIsMappedCorrectly()
		{
			var docketContainer = Factory.New<WhsDocketContainer>();

			var wrapper = new ContainerWrapperFromWhsDocket(docketContainer, Factory);

			docketContainer.WC_IsPalletised = false;
			AssertEquals("No", wrapper.IsPalletized);

			docketContainer.WC_IsPalletised = true;
			AssertEquals("Yes", wrapper.IsPalletized);
		}

		public void TestGetIsChargeableIsMappedCorrectly()
		{
			var docketContainer = Factory.New<WhsDocketContainer>();

			var wrapper = new ContainerWrapperFromWhsDocket(docketContainer, Factory);

			docketContainer.WC_IsChargeable = false;
			AssertEquals("No", wrapper.IsChargeable);

			docketContainer.WC_IsChargeable = true;
			AssertEquals("Yes", wrapper.IsChargeable);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromWhsDocket(null, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 
ArrivalContainerYardAddress : 
CFSClient :  is null
ContainerQuality : 
DeliveryMode : 
DepartureContainerYardAddress : 
ExportDetention : 
FreightJob :  is null
ImportDetention : 
Mode : 
OffHirePort : 
OnHirePort : 
Owner : 
PackCount : 
Registry : (No Default Field Value Available on Registry)
SetPointTemperature : 
Status : 
Type : ZZGP - General Purpose Box
VGMMethod : 
VGMVerifiedByAddress : 
VolumeGoods : 
WeightDunnage : 
WeightGoods : 
WeightGross : 
WeightTare : 1450.000 KG
";
			}
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450m;

			WhsDocketContainer docketContainer = Factory.New<WhsDocketContainer>();
			docketContainer.WC_RC = containerType.PK;
			return new ContainerWrapperFromWhsDocket(docketContainer, Factory);
		}
	}
}
