using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business;

public class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[MaxLength(17)]
	public override ZString AuthorisationNumber { get => base.AuthorisationNumber; set => base.AuthorisationNumber = value; }

	protected override JobDocAddressLookups GetNewLookups() => new CusGoodsLocationAddressLookups(this);

	protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

	protected override bool AuthorisationNumberReadOnly => !GoodsLocation.CGL_Qualifier.Equals(CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
}
