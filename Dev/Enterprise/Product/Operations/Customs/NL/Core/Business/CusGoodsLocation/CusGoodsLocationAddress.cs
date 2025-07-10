using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocation GoodsLocation => base.GoodsLocation as CusGoodsLocation;

	protected override bool E2_RN_NKCountryCodeReadonlyCore => GoodsLocation?.IsInAuthorisationMode ?? false;

	public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

	protected override JobDocAddressValidation GetNewValidation()
	{
		return new CusGoodsLocationAddressValidation(this);
	}
}
