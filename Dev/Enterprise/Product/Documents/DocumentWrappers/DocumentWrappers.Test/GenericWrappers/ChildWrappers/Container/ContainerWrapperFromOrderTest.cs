using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromOrder))]
	sealed class ContainerWrapperFromOrderTest : ContainerWrapperTest
	{
		public void TestFreightJob()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "P7777";
			OrderContainer container = order.PlannedContainers.AddNew();
			ContainerWrapperFromOrder wrapper = new ContainerWrapperFromOrder(container, Factory);
			AssertEquals("Wrapped business object should be Order", "P7777", wrapper.FreightJob.JobNumber);
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

		public void TestContainerNumberOrTypeCount()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";

			OrderContainer orderContainer = Factory.New<OrderContainer>();
			orderContainer.J1_RC = containerType.PK;
			orderContainer.J1_ContainerNumber = "AJUR9834711";
			orderContainer.J1_ContainerCount = 2;

			ContainerWrapperFromOrder wrapper = new ContainerWrapperFromOrder(orderContainer, Factory);
			AssertEquals("wrapper.ContainerNumberOrTypeCount", "AJUR9834711", wrapper.ContainerNumberOrTypeCount);

			orderContainer.J1_ContainerNumber = "";
			wrapper = new ContainerWrapperFromOrder(orderContainer, Factory);
			AssertEquals("wrapper.ContainerNumberOrTypeCount", "ZZGP (2)", wrapper.ContainerNumberOrTypeCount);
		}

		public override void TestWrapperMappingFull()
		{
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_ISOType = "2340";
			containerType.RC_TareWeight = 1450m;

			var orderContainer = Factory.New<OrderContainer>();
			orderContainer.J1_ContainerNumber = "AJUR9834711";
			orderContainer.J1_SealNum = "MOO234";
			orderContainer.J1_AdditionalSealNum = "BARK234";
			orderContainer.J1_Additional2SealNum = "MEOW234";
			orderContainer.J1_RC = containerType.PK;

			var wrapper = new ContainerWrapperFromOrder(orderContainer, Factory);
			AssertEquals("wrapper.Hazardous", false, wrapper.IsHazardous);
			AssertEquals("wrapper.Mode.Code", ZString.Empty, wrapper.Mode.Code);
			AssertEquals("wrapper.DeliveryMode.Code", ZString.Empty, wrapper.DeliveryMode.Code);
			AssertEquals("wrapper.Type.Code", "ZZGP", wrapper.Type.Code);
			AssertEquals("wrapper.Type.Description", "General Purpose Box", wrapper.Type.Description);
			AssertEquals("wrapper.WeightTare.Value", 1450m, wrapper.WeightTare.Value);
			AssertEquals("wrapper.WeightTare.Unit.Code", "KG", wrapper.WeightTare.Unit.Code);
			AssertEquals("wrapper.WeightGoods.Value", ZDecimal.Zero, wrapper.WeightGoods.Value);
			AssertEquals("wrapper.WeightGoods.Unit.Code", ZString.Empty, wrapper.WeightGoods.Unit.Code);
			AssertEquals("wrapper.WeightDunnage.Value", ZDecimal.Zero, wrapper.WeightDunnage.Value);
			AssertEquals("wrapper.WeightDunnage.Unit.Code", ZString.Empty, wrapper.WeightDunnage.Unit.Code);
			AssertEquals("wrapper.WeightGross.Value", ZDecimal.Zero, wrapper.WeightGross.Value);
			AssertEquals("wrapper.WeightGross.Unit.Code", ZString.Empty, wrapper.WeightGross.Unit.Code);
			AssertEquals("wrapper.VolumeGoods.Value", ZDecimal.Zero, wrapper.VolumeGoods.Value);
			AssertEquals("wrapper.VolumeGoods.Unit.Code", ZString.Empty, wrapper.VolumeGoods.Unit.Code);
			AssertEquals("wrapper.PackCount.Value", ZDecimal.Zero, wrapper.PackCount.Value);
			AssertEquals("wrapper.PackCount.Unit.Code", ZString.Empty, wrapper.PackCount.Unit.Code);
			AssertEquals("wrapper.Commodity.Count", 0, wrapper.Commodities.Count);
			AssertEquals("wrapper.Services.Count", 0, wrapper.Services.Count);
			AssertEquals("wrapper.ContainerNo", "AJUR9834711", wrapper.ContainerNo);
			AssertEquals("wrapper.ContainerNumberOrTypeCount", "AJUR9834711", wrapper.ContainerNumberOrTypeCount);
			AssertEquals("wrapper.SealNo", "MOO234", wrapper.SealNo);
			AssertEquals("wrapper.SealNo2", "BARK234", wrapper.SealNo2);
			AssertEquals("wrapper.SealNo3", "MEOW234", wrapper.SealNo3);
			AssertEquals("wrapper.ReleaseNumber", ZString.Empty, wrapper.ReleaseNumber);
			AssertEquals("wrapper.ArrivalEstimatedDelivery", ZDateTime.Empty, wrapper.ArrivalEstimatedDelivery);
			AssertEquals("wrapper.ArrivalReleaseNumber", ZString.Empty, wrapper.ArrivalReleaseNumber);
			AssertEquals("wrapper.EmptyReadyForReturn", ZDateTime.Empty, wrapper.EmptyReadyForReturn);
			AssertEquals("wrapper.EmptyReturnedBy", ZDateTime.Empty, wrapper.EmptyReturnedBy);
			AssertEquals("wrapper.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapper.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapper.BookingReference", ZString.Empty, wrapper.BookingReference);
			AssertEquals("wrapper.ArrivalSlotReference", ZString.Empty, wrapper.ArrivalSlotReference);
			AssertEquals("wrapper.DepartureSlotReference", ZString.Empty, wrapper.DepartureSlotReference);
			AssertEquals("wrapper.ArrivalSlotTime", ZDateTime.Empty, wrapper.ArrivalSlotTime);
			AssertEquals("wrapper.DepartureSlotTime", ZDateTime.Empty, wrapper.DepartureSlotTime);
			AssertEquals("wrapper.ExportDepotCustomsReference", ZString.Empty, wrapper.ExportDepotCustomsReference);
			AssertEquals("wrapper.EmptyRequired", ZDateTime.Empty, wrapper.EmptyRequired);
			AssertEquals("wrapper.WharfGateOut", ZDateTime.Empty, wrapper.WharfGateOut);
			AssertEquals("wrapper.DepartureEstimatedPickup", ZDateTime.Empty, wrapper.DepartureEstimatedPickup);
			AssertEquals("wrapper.Length", ZDecimal.Zero, wrapper.Length);
			AssertEquals("wrapper.Width", ZDecimal.Zero, wrapper.Width);
			AssertEquals("wrapper.Height", ZDecimal.Zero, wrapper.Height);
			AssertEquals("wrapper.SetPointTemperature.Value", ZDecimal.Zero, wrapper.SetPointTemperature.Value);
			AssertEquals("wrapper.SetPointTemperature.Unit.Code", ZString.Empty, wrapper.SetPointTemperature.Unit.Code);
			Assert("wrapper.Damaged", !wrapper.Damaged);
			Assert("wrapper.Frozen", !wrapper.Frozen);
			Assert("wrapper.Chilled", !wrapper.Chilled);
			Assert("wrapper.ControlledAtmosphere", !wrapper.ControlledAtmosphere);
			AssertEquals("wrapper.HumidityPercentage", ZByte.Zero, wrapper.HumidityPercentage);
			AssertEquals("wrapper.AirVentFlow.Value", ZDecimal.Zero, wrapper.AirVentFlow.Value);
			AssertEquals("wrapper.AirVentFlow.Unit.Code", ZString.Empty, wrapper.AirVentFlow.Unit.Code);
			AssertEquals("wrapper.ClipOnUnit", ZString.Empty, wrapper.ClipOnUnit);
			AssertEquals("wrapper.UNDGSubstances.Count", 0, wrapper.UNDGSubstances.Count);
			AssertEquals("wrapper.DepartureContainerYardAddress.CompanyName", ZString.Empty, wrapper.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapper.ArrivalContainerYardAddress.CompanyName", ZString.Empty, wrapper.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapper.ContainerJobID", ZString.Empty, wrapper.ContainerJobID);
			AssertEquals("wrapper.IsChargeable", "No", wrapper.IsChargeable);
			AssertEquals("wrapper.IsPalletized", "No", wrapper.IsPalletized);
			AssertEquals("wrapper.Items", "0", wrapper.Packages);
			AssertEquals("wrapper.Pallets", "0", wrapper.Pallets);
			AssertEquals("wrapper.ContainerQuality", ZString.Empty, wrapper.ContainerQuality.Code);
			AssertEquals("wrapper.PrintTACImage", ZBool.False, wrapper.PrintTACImage);
			AssertEquals("wrapper.ImportDetention.Released", ZDateTime.Empty, wrapper.ImportDetention.Released);
			AssertEquals("wrapper.ExportDetention.Released", ZDateTime.Empty, wrapper.ExportDetention.Released);
			AssertEquals("wrapper.CFSClient", null, wrapper.CFSClient);
			AssertEquals("wrapper.UnpackShed", ZString.Empty, wrapper.UnpackShed);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromOrder(null, Factory);
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

			OrderContainer orderContainer = Factory.New<OrderContainer>();
			orderContainer.J1_RC = containerType.PK;
			return new ContainerWrapperFromOrder(orderContainer, Factory);
		}
	}
}
