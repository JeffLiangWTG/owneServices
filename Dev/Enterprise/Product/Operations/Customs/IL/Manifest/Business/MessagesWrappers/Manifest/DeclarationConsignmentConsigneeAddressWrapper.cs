using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsigneeAddressWrapper : IDeclarationConsignmentConsigneeAddress
	{
		DeclarationConsignmentConsigneeAddressWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentConsigneeAddressWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentConsigneeAddressWrapper(asycudaBill) : null;

		public ITextType CityName => TextTypeWrapper.NewOrNull(asycudaBill.ABL_ConsigneeCity);

		public ICodeType CountryCode => CodeTypeWrapper.NewOrNull(asycudaBill.ABL_RN_NKConsigneeCountry);

		public IIDType CountrySubDivisionID => null;

		public ITextType CountrySubDivisionName => null;

		public ITextType Line => TextTypeWrapper.NewOrNull(asycudaBill.ABL_ConsigneeStreet1);

		public IIDType PostcodeId => IDTypeWrapper.NewOrNull(asycudaBill.ABL_ConsigneePostcode);

		readonly AsycudaBill asycudaBill;
	}
}
