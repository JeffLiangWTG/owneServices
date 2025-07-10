using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentNotifyPartyAddressWrapper : IDeclarationConsignmentNotifyPartyAddress
	{
		DeclarationConsignmentNotifyPartyAddressWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentNotifyPartyAddressWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentNotifyPartyAddressWrapper(asycudaBill) : null;

		public ITextType CityName => TextTypeWrapper.NewOrNull(asycudaBill.ABL_NotifyPartyCity);

		public ICodeType CountryCode => CodeTypeWrapper.NewOrNull(asycudaBill.ABL_RN_NKNotifyPartyCountry);

		public IIDType CountrySubDivisionId => null;

		public ITextType Line => TextTypeWrapper.NewOrNull(asycudaBill.ABL_NotifyPartyStreet1);

		public IIDType PostcodeId => IDTypeWrapper.NewOrNull(asycudaBill.ABL_NotifyPartyPostcode);

		readonly AsycudaBill asycudaBill;
	}
}
