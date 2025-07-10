using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer
{
	public AsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	[ResourceStringData("a21dfc8e-a988-44ea-8d5c-af103a68264f", Caption = "Temperature")]
	public override ZDecimal ACN_SetPointTemperature
	{
		get => base.ACN_SetPointTemperature;
		set => base.ACN_SetPointTemperature = value;
	}

	[ResourceStringData("9d6f86a8-4ca9-44bf-a662-5dd92d6a1e97", Caption = "Temperature UQ")]
	[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.AEContainerTemperatureUnitCodes))]
	public override ZString ACN_SetPointTemperatureUnit
	{
		get => base.ACN_SetPointTemperatureUnit;
		set => base.ACN_SetPointTemperatureUnit = value;
	}

	public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;

	protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

	protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);
}
