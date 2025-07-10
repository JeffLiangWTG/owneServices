using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : EU.Business.CusGoodsLocationAddress(factory, row)
{
	public override ZGuid IdentificationHolderPK
	{
		get => base.IdentificationHolderPK;
		set
		{
			var oldValue = IdentificationHolderPK;
			base.IdentificationHolderPK = value;
			if (!IsCopying && oldValue != IdentificationHolderPK)
			{
				CusGoodsLocationAddressHelper.SetDefaultAuthorisationNumber(this);
			}
		}
	}

	public override ZString AuthorisationNumber
	{
		get => base.AuthorisationNumber;
		set
		{
			var oldValue = AuthorisationNumber;
			base.AuthorisationNumber = value;
			if (!IsCopying && oldValue != AuthorisationNumber)
			{
				DefaultAdditionalIdentifier();
			}
		}
	}

	protected override bool AuthorisationNumberReadOnly => base.AuthorisationNumberReadOnly
		&& GoodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber;

	public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

	protected override JobDocAddressLookups GetNewLookups() => new CusGoodsLocationAddressLookups(this);

	protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

	void DefaultAdditionalIdentifier()
	{
		if (GoodsLocation is CusGoodsLocation goodsLocation)
		{
			var additionalIdentifierList = goodsLocation.Lookups.AdditionalIdentifierList;
			if (additionalIdentifierList.Count == 1)
			{
				goodsLocation.AdditionalIdentifier = additionalIdentifierList[0].Code;
			}
		}
	}
}
