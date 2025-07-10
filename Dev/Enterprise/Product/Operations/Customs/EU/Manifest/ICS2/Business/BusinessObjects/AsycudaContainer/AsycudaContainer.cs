using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer
{
	public AsycudaContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

	[ResourceStringData("31619AFC-112F-4182-800C-11B60297BB8E", Caption = "Shipper's Own Container", MediumCaption = "Shipper's Own Cont.", ShortCaption = "SOC")]
	public override ZBool ACN_IsShipperOwned { get => base.ACN_IsShipperOwned; set => base.ACN_IsShipperOwned = value; }
}
