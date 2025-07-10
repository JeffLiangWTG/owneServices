using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class TraderJobDocAddressValidation : JobDocAddressValidation
{
	public TraderJobDocAddressValidation(AutoJobDocAddress parent, string traderName, IUCC6AndTransitionPeriodProvider uCC6AndTransitionPeriodProvider, bool isMandatory = true) : base(parent)
	{
		TraderName = traderName;
		this.uCC6AndTransitionPeriodProvider = uCC6AndTransitionPeriodProvider;
		IsMandatory = isMandatory;
	}

	readonly IUCC6AndTransitionPeriodProvider uCC6AndTransitionPeriodProvider;

	public bool IsMandatory { get; }
	protected string TraderName { get; }

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		var organisation = Organisation;
		if (organisation is null && IsMandatory)
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(TraderName);
			AddMessageErrorIfNotEmpty(messageError);
		}
		else if (organisation != null)
		{
			ValidateRequiredCustomsCode();
			CheckAddress();
		}
	}

	#region ValidateRequiredCustomsCode

	public void ValidateRequiredCustomsCode()
	{
		var organisation = Organisation;

		if (organisation == null)
		{
			return;
		}

		var isOrganisationEuropean = Parent.Country?.IsPartOfEuropeanUnion ?? false;

		var requiredCodeTypeListForTrader = isOrganisationEuropean
			? GetRequiredCodeTypeListForEuropeanTrader(organisation)
			: GetRequiredCodeTypeListForNotEuropeanTrader(organisation);

		if (OrganisationDoesNotHaveAnyRequiredCustomsCode())
		{
			var messageError = GetMissingCustomsCodeMessageError();
			AddMessageErrorIfNotEmpty(messageError);
		}

		bool OrganisationDoesNotHaveAnyRequiredCustomsCode()
		{
			return requiredCodeTypeListForTrader.Any() && organisation.GetFirstCusCodeMatchingTypeInOrder(requiredCodeTypeListForTrader) == null;
		}
	}

	protected virtual ZString[] GetRequiredCodeTypeListForEuropeanTrader(OrgHeader organisation) => organisation.GetCustomsCodeTypeListRequiredForEUTrader();

	protected virtual ZString[] GetRequiredCodeTypeListForNotEuropeanTrader(OrgHeader organisation) => Array.Empty<ZString>();

	protected virtual string GetMissingCustomsCodeMessageError()
	{
		if (Organisation.IsNaturalPersonIndividual())
		{
			return ValidationCaptions.TraderJobDocAddressValidation.EoriCodeOrFiscalCodeIsRequired;
		}
		return ValidationCaptions.TraderJobDocAddressValidation.EoriCodeOrVatCodeIsRequired;
	}

	protected void AddMessageErrorIfNotEmpty(ZString messageErrorToAdd)
	{
		if (!messageErrorToAdd.IsEmpty && !OrganisationPKInfo.HasMessageError(messageErrorToAdd))
		{
			OrganisationPKInfo.AddMessageError(messageErrorToAdd);
		}
	}

	protected OrgHeader Organisation => Parent.Organisation;
	protected ZPropertyInfo OrganisationPKInfo => Parent.OrganisationPKInfo;

	void CheckAddress()
	{
		var address = Parent.Address;
		if (address != null)
		{
			new CustomsAddressValidator(address, TraderName, uCC6AndTransitionPeriodProvider)
				.ValidateMaximumLengthCustomsFields(OrganisationPKInfo);
		}
	}

	#endregion
}
