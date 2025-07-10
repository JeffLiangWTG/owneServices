using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
{
	public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent) : base(parent)
	{
	}

	public new CusGoodsLocationAddress Parent => base.Parent as CusGoodsLocationAddress;

	protected override void CheckE2_AdditionalAddressInformation()
	{
		base.CheckE2_AdditionalAddressInformation();

		ListValidation.ErrorIfInvalidPK(Parent.IdentificationHolderPKInfo);
	}

	protected override bool ApplyC0065Rule => false;
}
