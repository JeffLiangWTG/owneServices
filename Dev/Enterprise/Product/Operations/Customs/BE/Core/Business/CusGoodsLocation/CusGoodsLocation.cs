using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public sealed class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.BE.ICusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new EU.Business.CusGoodsLocationLookups Lookups => base.Lookups;

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups()
	{
		if (Parent is TemporaryStorageHeader)
		{
			return new PNTSCusGoodsLocationLookups(this);
		}
		else
		{
			return new CusGoodsLocationLookups(this);
		}
	}

	public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);
}
