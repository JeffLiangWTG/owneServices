using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerWrapperFromPkgPackage : ContainerWrapperEmpty
	{
		public ContainerWrapperFromPkgPackage(PkgPackage packageBO, BusinessObjectFactory factory)
			: base(packageBO ?? (packageBO = factory.GetNull<PkgPackage>()), factory)
		{
			Argument.NotNull(factory, "factory");
			if (!packageBO.IsContainer)
			{
				packageBO.KP_PackageQty = 1;
				packageBO.KP_F3_NKPackType = Constants.PkgUnit.Container;

				var packageContainer = factory.GetNull<PkgPackageContainer>();
				packageContainer.K0_KP_Package = packageBO.PK;
			}

			PackageBO = packageBO;
			PackageContainerBO = PackageBO.Container;
		}

		readonly PkgPackage PackageBO;
		readonly PkgPackageContainer PackageContainerBO;

		protected override CodeAndDescriptionWrapper GetContainerQuality()
		{
			return new CodeAndDescriptionWrapper(PackageContainerBO.K0_Quality, PackageContainerBO.Lookups.ContainerQualities, Factory);
		}

		protected override CodeAndDescriptionWrapper GetMode()
		{
			return new CodeAndDescriptionWrapper(PackageContainerBO.K0_ContainerMode, PackageContainerBO.Lookups.ContainerModes, Factory);
		}

		protected override ContainerTypeWrapper GetTypeWrapper()
		{
			return new ContainerTypeWrapper(PackageContainerBO.ContainerType, Factory);
		}

		protected override WeightWrapper GetWeightTare()
		{
			return new WeightWrapper(PackageBO.KP_TareWeight, PackageBO.KP_WeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGoods()
		{
			return new WeightWrapper(PackageContainerBO.GoodsWeight, PackageBO.KP_WeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightDunnage()
		{
			return new WeightWrapper(PackageBO.KP_DunnageWeight, PackageBO.KP_WeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetWeightGross()
		{
			return new WeightWrapper(PackageBO.KP_Weight, PackageBO.KP_WeightUQ, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolumeGoods()
		{
			var volume = 0m;
			if (PackageBO.PackageJob != null)
			{
				volume = PackageBO.Packages.Any() ? PackageBO.Packages.Sum(p => p.KP_Volume) : 0m;
			}

			return new VolumeWrapper(volume, PackageBO.KP_VolumeUQ, PackageBO.Lookups.VolumeUQs, null);
		}

		protected override ValueAndUnitWrapper GetSetPointTemperature()
		{
			return new ValueAndUnitWrapper(PackageContainerBO.K0_SetPointTemp, PackageContainerBO.K0_SetPointTempUnit, PackageBO.Lookups.TemperatureUnits, Factory);
		}

		protected override ValueAndUnitWrapper GetAirVentFlow()
		{
			return new ValueAndUnitWrapper(PackageContainerBO.K0_AirVentFlowRate, PackageContainerBO.K0_AirVentFlowRateUnit, PackageContainerBO.Lookups.AirVentFlowRateUnits, Factory);
		}

		protected override ZBool GetIsReefer()
		{
			RefContainer type = PackageContainerBO.ContainerType;
			return type != null && type.RC_ContainerType == Constants.ContainerTypes.Refrigerated;
		}

		protected override ZString GetContainerNo()
		{
			return PackageBO.KP_PackageID;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return ContainerNo.IsEmpty ? ZString.Format("{0} ({1})", Type.Code, PackageBO.KP_PackageID) : ContainerNo;
		}

		protected override ZInt GetContainerCount()
		{
			return PackageBO.KP_PackageQty;
		}

		protected override ZString GetSealNo()
		{
			return PackageContainerBO.K0_Seal1;
		}

		protected override ZString GetSealNo2()
		{
			return PackageContainerBO.K0_Seal2;
		}

		protected override ZString GetSealNo3()
		{
			return PackageContainerBO.K0_Seal3;
		}

		protected override ZDecimal GetLength()
		{
			return PackageBO.KP_Length;
		}

		protected override ZDecimal GetWidth()
		{
			return PackageBO.KP_Width;
		}

		protected override ZDecimal GetHeight()
		{
			return PackageBO.KP_Height;
		}

		protected override ZBool GetDamaged()
		{
			return PackageContainerBO.K0_IsDamaged;
		}

		protected override ZBool GetFrozen()
		{
			return PackageContainerBO.IsFreezer;
		}

		protected override ZBool GetChilled()
		{
			return PackageContainerBO.IsChiller;
		}

		protected override ZBool GetControlledAtmosphere()
		{
			return PackageContainerBO.K0_IsControlledAtmosphere;
		}

		protected override ZByte GetHumidityPercentage()
		{
			return PackageContainerBO.K0_HumidityPercent;
		}

		protected override bool? GetIsPalletized() => null;

		protected override bool? GetIsIsChargeable() => null;

		protected override ZString GetPackages() => ZString.Empty;

		protected override ZString GetPallets() => ZString.Empty;

		protected override ZString GetUnpackShed() => ZString.Empty;

		protected override LocationWrapper GetOffHirePort()
		{
			return new LocationWrapper(ZString.Empty, Factory);
		}

		protected override LocationWrapper GetOnHirePort()
		{
			return new LocationWrapper(ZString.Empty, Factory);
		}

		protected override ZString GetCreatedByUserName() => ZString.Empty;
	}
}
