using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignorAddressWrapper : IDeclarationConsignmentConsignorAddress
	{
		DeclarationConsignmentConsignorAddressWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentConsignorAddressWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentConsignorAddressWrapper(asycudaBill) : null;

		public ITextType CityName => TextTypeWrapper.NewOrNull(asycudaBill.ABL_ShipperCity);

		public ICodeType CountryCode => CodeTypeWrapper.NewOrNull(asycudaBill.ABL_RN_NKShipperCountry);

		public IIDType CountrySubDivisionID => null;

		public ITextType Line => TextTypeWrapper.NewOrNull(asycudaBill.ABL_ShipperStreet1);

		public IIDType PostcodeId => IDTypeWrapper.NewOrNull(asycudaBill.ABL_ShipperPostcode);

		readonly AsycudaBill asycudaBill;
	}
}
