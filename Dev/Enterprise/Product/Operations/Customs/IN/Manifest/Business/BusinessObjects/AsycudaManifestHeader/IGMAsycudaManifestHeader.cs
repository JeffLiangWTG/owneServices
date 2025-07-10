using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class IGMAsycudaManifestHeader : AsycudaManifestHeader
	, Integration.Customs.ASYCUDA.INManifest.IIGMAsycudaManifestHeader
{
	public IGMAsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		AMA_ManifestType = INManifestTypes.Codes.IGM;
		AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
	}
}
