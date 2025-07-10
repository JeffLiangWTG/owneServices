using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class InvoiceLineTraderJobDocAddressValidation : EU.Business.Declaration.InvoiceLineTraderJobDocAddressValidation
{
	public InvoiceLineTraderJobDocAddressValidation(JobDocAddress parent, IUCC6AndTransitionPeriodProvider uCC6AndTransitionPeriodProvider)
		: base(parent)
	{
		this.uCC6AndTransitionPeriodProvider = Argument.NotNull(uCC6AndTransitionPeriodProvider, nameof(uCC6AndTransitionPeriodProvider));
	}
	readonly IUCC6AndTransitionPeriodProvider uCC6AndTransitionPeriodProvider;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		var parent = Parent;
		var propertyInfo = parent.OrganisationPKInfo;
		var address = parent.Address;

		if (address != null)
		{
			new CustomsAddressValidator(address, CustomizeName(), uCC6AndTransitionPeriodProvider)
				.ValidateMaximumLengthCustomsFields(propertyInfo);
		}

		string CustomizeName()
		{
			if (parent.E2_AddressType == DocAddressTypes.Codes.BuyingParty)
			{
				return Res.GetString("E84908A2-0F7B-4906-B94B-C3E7F5185E48", "Buyer");
			}
			else if (parent.E2_AddressType == DocAddressTypes.Codes.SellingParty)
			{
				return Res.GetString("05EBE43E-2060-4151-A548-DB77F7A3E102", "Seller");
			}

			return propertyInfo.HumanReadableName;
		}
	}
}
