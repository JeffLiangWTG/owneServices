using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromPkgPackage))]
	sealed class ContainerWrapperFromPkgPackageTest : ContainerWrapperTest
	{
		#region TestGoodsVolumeWithInnerPackages

		public void TestGoodsVolumeWithInnerPackages()
		{
			var parent = Factory.New<DtbBookingConsolidation>();
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = Factory.New<PkgPackage>();
			container.KP_PackageQty = 2;
			container.KP_Volume = 50m; // Containers volume, should never be returned
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;

			var innerPkg1 = Factory.New<PkgPackage>();
			var innerPkg2 = Factory.New<PkgPackage>();
			innerPkg1.KP_Volume = 10m;
			innerPkg2.KP_Volume = 20m;

			parent.PackageJob.Packages.AddRange(new[] { container, innerPkg1, innerPkg2 });
			container.Packages.AddRange(new[] { innerPkg1, innerPkg2 });
			var wrapper1 = new ContainerWrapperFromPkgPackage(container, Factory);
			AssertEquals("VolumeGoods should return the sum of the packages volumes", 30m, wrapper1.VolumeGoods.Value);

			container.Packages.DeleteAll();
			var wrapper2 = new ContainerWrapperFromPkgPackage(container, Factory);
			AssertEquals("VolumeGoods should return 0 if there are no packages", 0m, wrapper2.VolumeGoods.Value);
		}

		public void TestGoodsVolumeWithInnerPackagesDoesNotRoundVolume()
		{
			var parent = Factory.New<DtbBookingConsolidation>();
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = Factory.New<PkgPackage>();
			container.KP_PackageQty = 2;
			container.KP_Volume = 50m; // Containers volume, should never be returned
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;

			var innerPkg1 = Factory.New<PkgPackage>();
			var innerPkg2 = Factory.New<PkgPackage>();
			innerPkg1.KP_Volume = 10.1m;
			innerPkg2.KP_Volume = 20.4m;

			parent.PackageJob.Packages.AddRange(new[] { container, innerPkg1, innerPkg2 });
			container.Packages.AddRange(new[] { innerPkg1, innerPkg2 });
			var wrapper1 = new ContainerWrapperFromPkgPackage(container, Factory);
			AssertEquals("VolumeGoods should return the sum of the packages volumes", 30.5m, wrapper1.VolumeGoods.Value);
		}

		#endregion

		#region TestWrapperMappingFull

		public override void TestWrapperMappingFull()
		{
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = Factory.New<PkgPackage>();
			container.KP_PackageQty = 3;
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;
			container.Container.K0_AirVentFlowRate = 7m;
			container.Container.K0_AirVentFlowRateUnit = "M2";
			container.Container.K0_ContainerMode = "AIR";
			container.KP_DunnageWeight = 8m;
			container.Container.K0_HumidityPercent = 9;
			container.Container.K0_IsControlledAtmosphere = true;
			container.Container.K0_IsDamaged = true;
			container.Container.K0_IsEmpty = true;
			container.Container.K0_IsSealOk = true;
			container.Container.K0_IsShipperOwned = true;
			container.Container.K0_Quality = "RIC";
			container.Container.K0_RC_ContainerType = containerType20GP.PK;
			container.Container.K0_RefrigGeneratorID = "REFRIG123";
			container.Container.K0_Seal1 = "SEAL-1";
			container.Container.K0_Seal2 = "SEAL-2";
			container.Container.K0_Seal3 = "SEAL-3";
			container.Container.K0_SetPointTemp = 10m;
			container.Container.K0_SetPointTempUnit = "C";
			container.Container.K0_Status = "ARV";
			container.KP_TareWeight = 5m;
			container.Container.K0_TempRecorderSerialNumber = "TEMPSER123";
			container.KP_DimensionUQ = "M";
			container.KP_Height = 1m;
			container.KP_Length = 2m;
			container.KP_PackageID = "CONT-1";
			container.KP_VolumeUQ = "M3";
			container.KP_Weight = 11m;
			container.KP_WeightUQ = "T";
			container.KP_Width = 6m;
			container.KP_Volume = 12m;
			container.KP_TransportRef = "TRANSPORT REF";

			var wrapperFull = new ContainerWrapperFromPkgPackage(container, Factory);
			AssertEquals("wrapperFull.Hazardous", false, wrapperFull.IsHazardous);
			AssertEquals("wrapperFull.Mode.Code", "AIR", wrapperFull.Mode.Code);
			AssertEquals("wrapperFull.DeliveryMode.Code", "", wrapperFull.DeliveryMode.Code);
			AssertEquals("wrapperFull.Type.Code", "20GP", wrapperFull.Type.Code);
			AssertEquals("wrapperFull.Type.Description", "Twenty foot general purpose", wrapperFull.Type.Description);
			AssertEquals("wrapperFull.WeightTare.Value", 5m, wrapperFull.WeightTare.Value);
			AssertEquals("wrapperFull.WeightTare.Unit.Code", "T", wrapperFull.WeightTare.Unit.Code);
			AssertEquals("wrapperFull.WeightGoods.Value", 6m, wrapperFull.WeightGoods.Value);
			AssertEquals("wrapperFull.WeightGoods.Unit.Code", "T", wrapperFull.WeightGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightDunnage.Value", 8m, wrapperFull.WeightDunnage.Value);
			AssertEquals("wrapperFull.WeightDunnage.Unit.Code", "T", wrapperFull.WeightDunnage.Unit.Code);
			AssertEquals("wrapperFull.WeightGross.Value", 11m, wrapperFull.WeightGross.Value);
			AssertEquals("wrapperFull.WeightGross.Unit.Code", "T", wrapperFull.WeightGross.Unit.Code);
			AssertEquals("wrapperFull.VolumeGoods.Value", 0m, wrapperFull.VolumeGoods.Value);
			AssertEquals("wrapperFull.VolumeGoods.Unit.Code", "M3", wrapperFull.VolumeGoods.Unit.Code);
			AssertEquals("wrapperFull.PackCount.Value", 0m, wrapperFull.PackCount.Value);
			AssertEquals("wrapperFull.PackCount.Unit.Code", "", wrapperFull.PackCount.Unit.Code);
			AssertEquals("wrapperFull.Commodity.Count", 0, wrapperFull.Commodities.Count);
			AssertEquals("wrapperFull.Services.Count", 0, wrapperFull.Services.Count);
			AssertEquals("wrapperFull.ContainerNo", "CONT-1", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "CONT-1", wrapperFull.ContainerNumberOrTypeCount);
			AssertEquals("wrapperFull.SealNo", "SEAL-1", wrapperFull.SealNo);
			AssertEquals("wrapperFull.SealNo2", "SEAL-2", wrapperFull.SealNo2);
			AssertEquals("wrapperFull.SealNo3", "SEAL-3", wrapperFull.SealNo3);
			AssertEquals("wrapperFull.ReleaseNumber", "", wrapperFull.ReleaseNumber);
			AssertEquals("wrapperFull.ArrivalEstimatedDelivery", ZDateTime.Empty, wrapperFull.ArrivalEstimatedDelivery);
			AssertEquals("wrapperFull.ArrivalReleaseNumber", "", wrapperFull.ArrivalReleaseNumber);
			AssertEquals("wrapperFull.EmptyReturnedBy", ZDateTime.Empty, wrapperFull.EmptyReturnedBy);
			AssertEquals("wrapperFull.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapperFull.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperFull.BookingReference", "", wrapperFull.BookingReference);
			AssertEquals("wrapperFull.ArrivalSlotReference", "", wrapperFull.ArrivalSlotReference);
			AssertEquals("wrapperFull.DepartureSlotReference", "", wrapperFull.DepartureSlotReference);
			AssertEquals("wrapperFull.ArrivalSlotTime", ZDateTime.Empty, wrapperFull.ArrivalSlotTime);
			AssertEquals("wrapperFull.DepartureSlotTime", ZDateTime.Empty, wrapperFull.DepartureSlotTime);
			AssertEquals("wrapperFull.ExportDepotCustomsReference", "", wrapperFull.ExportDepotCustomsReference);
			AssertEquals("wrapperFull.EmptyReadyForReturn", ZDateTime.Empty, wrapperFull.EmptyReadyForReturn);
			AssertEquals("wrapperFull.EmptyRequired", ZDateTime.Empty, wrapperFull.EmptyRequired);
			AssertEquals("wrapperFull.WharfGateOut", ZDateTime.Empty, wrapperFull.WharfGateOut);
			AssertEquals("wrapperFull.DepartureEstimatedPickup", ZDateTime.Empty, wrapperFull.DepartureEstimatedPickup);
			AssertEquals("wrapperFull.Length", 2m, wrapperFull.Length);
			AssertEquals("wrapperFull.Width", 6m, wrapperFull.Width);
			AssertEquals("wrapperFull.Height", 1m, wrapperFull.Height);
			AssertEquals("wrapperFull.SetPointTemperature.Value", 10m, wrapperFull.SetPointTemperature.Value);
			AssertEquals("wrapperFull.SetPointTemperature.Unit.Code", "C", wrapperFull.SetPointTemperature.Unit.Code);
			Assert("wrapperFull.Damaged", wrapperFull.Damaged);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
			Assert("wrapperFull.ControlledAtmosphere", wrapperFull.ControlledAtmosphere);
			AssertEquals("wrapperFull.HumidityPercentage", (ZByte)9, wrapperFull.HumidityPercentage);
			AssertEquals("wrapperFull.AirVentFlow.Value", 7m, wrapperFull.AirVentFlow.Value);
			AssertEquals("wrapperFull.AirVentFlow.Unit.Code", "M2", wrapperFull.AirVentFlow.Unit.Code);
			AssertEquals("wrapperFull.ClipOnUnit", "", wrapperFull.ClipOnUnit);
			AssertEquals("wrapperFull.UNDGSubstances.Count", 0, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.DepartureContainerYardAddress.CompanyName", "", wrapperFull.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ContainerJobID", "", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.IsChargeable", "", wrapperFull.IsChargeable);
			AssertEquals("wrapperFull.IsPalletized", "", wrapperFull.IsPalletized);
			AssertEquals("wrapperFull.Items", "", wrapperFull.Packages);
			AssertEquals("wrapperFull.Pallets", "", wrapperFull.Pallets);
			AssertEquals("wrapperFull.ContainerQuality", "RIC", wrapperFull.ContainerQuality.Code);
			AssertEquals("wrapperFull.PrintTACImage", false, wrapperFull.PrintTACImage);
			AssertEquals("wrapperFull.ImportDetention.Released", ZDateTime.Empty, wrapperFull.ImportDetention.Released);
			AssertEquals("wrapperFull.ExportDetention.Released", ZDateTime.Empty, wrapperFull.ExportDetention.Released);
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (ContainerWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Mode.Code", "", wrapperEmpty.Mode.Code);
			AssertEquals("wrapperEmpty.Type.Code", "", wrapperEmpty.Type.Code);
			AssertEquals("wrapperEmpty.SetPointTemperature", "", wrapperEmpty.SetPointTemperature.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.VolumeGoods.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.VolumeGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightTare.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.WeightTare.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightGoods.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.WeightGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightDunnage.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.WeightDunnage.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightGross.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.WeightGross.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.ContainerNo", "", wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.SealNo", "", wrapperEmpty.SealNo);
			AssertEquals("wrapperEmpty.SealNo2", "", wrapperEmpty.SealNo2);
			AssertEquals("wrapperEmpty.SealNo3", "", wrapperEmpty.SealNo3);
			AssertEquals("wrapperEmpty.Services", 0, wrapperEmpty.Services.Count);
			AssertEquals("wrapperEmpty.PackCount.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.EmptyReturnedBy", ZDateTime.Empty, wrapperEmpty.EmptyReturnedBy);
			AssertEquals("wrapperEmpty.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapperEmpty.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperEmpty.ExportDepotCustomsReference", "", wrapperEmpty.ExportDepotCustomsReference);
			AssertEquals("wrapperEmpty.EmptyRequired", ZDateTime.Empty, wrapperEmpty.EmptyRequired);
			AssertEquals("wrapperEmpty.WharfGateOut", ZDateTime.Empty, wrapperEmpty.WharfGateOut);
			AssertEquals("wrapperEmpty.DepartureEstimatedPickup", ZDateTime.Empty, wrapperEmpty.DepartureEstimatedPickup);
			AssertEquals("wrapperEmpty.Length", 0m, wrapperEmpty.Length);
			AssertEquals("wrapperEmpty.Width", 0m, wrapperEmpty.Width);
			AssertEquals("wrapperEmpty.Height", 0m, wrapperEmpty.Height);
			AssertEquals("wrapper.ContainerJobID", "", wrapperEmpty.ContainerJobID);
			Assert("wrapperEmpty.Damaged", !wrapperEmpty.Damaged);
			Assert("wrapperEmpty.Frozen", !wrapperEmpty.Frozen);
			Assert("wrapperEmpty.Chilled", !wrapperEmpty.Chilled);
			AssertEquals("wrapperEmpty.HumidityPercentage", ZByte.Zero, wrapperEmpty.HumidityPercentage);
			AssertEquals("wrapperEmpty.AirVentFlow.ValueAndUnitCodeBlankIfZero", "", wrapperEmpty.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.UNDGNumbers.Count", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.DepartureContainerYardAddress.CompanyName", "", wrapperEmpty.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperEmpty.ArrivalContainerYardAddress.CompanyName", "", wrapperEmpty.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapperEmpty.IsChargeable", "", wrapperEmpty.IsChargeable);
			AssertEquals("wrapperEmpty.IsPalletized", "", wrapperEmpty.IsPalletized);
			AssertEquals("wrapperEmpty.Items", "", wrapperEmpty.Packages);
			AssertEquals("wrapperEmpty.Pallets", "", wrapperEmpty.Pallets);
			AssertEquals("wrapperEmpty.ImportDetention.Released", ZDateTime.Empty, wrapperEmpty.ImportDetention.Released);
			AssertEquals("wrapperEmpty.ExportDetention.Released", ZDateTime.Empty, wrapperEmpty.ExportDetention.Released);
			AssertEquals("wrapperEmpty.FreightJob", null, wrapperEmpty.FreightJob);
			AssertEquals("wrapperEmpty.CFSClient", null, wrapperEmpty.CFSClient);
			AssertEquals("wrapperEmpty.UnpackShed", ZString.Empty, wrapperEmpty.UnpackShed);
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromPkgPackage(null, Factory);
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
Type : 
VGMMethod : 
VGMVerifiedByAddress : 
VolumeGoods : 
WeightDunnage : 
WeightGoods : 
WeightGross : 
WeightTare :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var container = Factory.New<PkgPackage>();
			container.KP_PackageQty = 1;
			container.KP_F3_NKPackType = Constants.PkgUnit.Container;

			return new ContainerWrapperFromPkgPackage(container, Factory);
		}

		#endregion
	}
}
