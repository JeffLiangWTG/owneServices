using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
{
	public CusGoodsLocationAddressValidation(EU.Business.CusGoodsLocationAddress parent) : base(parent)
	{
	}

	new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

	CusGoodsLocation GoodsLocation => Parent.GoodsLocation;

	protected override void CheckE2_Postcode()
	{
		base.CheckE2_Postcode();
		if (GoodsLocation != null && GoodsLocation.IsInAuthorisationMode && Parent != null)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_PostcodeInfo);
			var info = Parent.E2_PostcodeInfo;
			AddressValidation.CheckPostCode(info, Parent.Country, Parent.ValidationSection, Parent.ValidationStatus);
		}
	}

	AddressValidation AddressValidation => addressValidation ?? (addressValidation = new AddressValidation());
	AddressValidation addressValidation;
}
