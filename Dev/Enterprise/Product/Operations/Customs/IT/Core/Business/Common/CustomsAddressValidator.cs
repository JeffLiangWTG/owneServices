using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

sealed class CustomsAddressValidator
{
	public CustomsAddressValidator(OrgAddress orgAddress, string traderName, IUCC6AndTransitionPeriodProvider provider, bool shouldApplyTransitionPeriod = true)
	{
		address = Argument.NotNull(orgAddress, nameof(orgAddress));
		this.traderName = traderName;
		uCC6AndTransitionPeriodProvider = Argument.NotNull(provider, nameof(provider));
		this.shouldApplyTransitionPeriod = shouldApplyTransitionPeriod;
	}

	readonly OrgAddress address;
	readonly string traderName;
	readonly IUCC6AndTransitionPeriodProvider uCC6AndTransitionPeriodProvider;
	readonly bool shouldApplyTransitionPeriod;

	public void ValidateMaximumLengthCustomsFields(ZPropertyInfo targetPropertyInfo)
	{
		Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));

		ValidateCompanyNameMaximumLengthAllowedByCustoms(targetPropertyInfo);
		ValidateAddressAsSingleLineMaximumLengthByCustoms(targetPropertyInfo);
		ValidatePostcodeMaximumLengthAllowedByCustoms(targetPropertyInfo);
		ValidateCityMaximumLengthAllowedByCustoms(targetPropertyInfo);
	}

	void ValidateCompanyNameMaximumLengthAllowedByCustoms(ZPropertyInfo targetPropertyInfo)
	{
		AddWarningIfExceedsCustomsMaxLength(targetPropertyInfo, address.CompanyNameInfo, CompanyNameMaxLength, address.CompanyName);
	}

	void ValidateAddressAsSingleLineMaximumLengthByCustoms(ZPropertyInfo targetPropertyInfo)
	{
		if (address.GetAddressAsASingleLineForCustomsMessage().Length > AddressAsSingleLineMaximumLength)
		{
			var fieldName = Res.GetString("6CF37E61-8F63-4054-980D-0B0B2485AA6F", "Address");
			targetPropertyInfo.AddWarning(ValidationCaptions.Shared.FieldIsLongerThanMaximumLength(traderName, fieldName, AddressAsSingleLineMaximumLength));
		}
	}

	void ValidatePostcodeMaximumLengthAllowedByCustoms(ZPropertyInfo targetPropertyInfo)
	{
		AddWarningIfExceedsCustomsMaxLength(targetPropertyInfo, address.OA_PostCodeInfo, PostcodeMaximumLength);
	}

	void ValidateCityMaximumLengthAllowedByCustoms(ZPropertyInfo targetPropertyInfo)
	{
		AddWarningIfExceedsCustomsMaxLength(targetPropertyInfo, address.OA_CityInfo, Ucc6XmlConstants.CustomsFieldMaxLength.Trader.City);
	}

	void AddWarningIfExceedsCustomsMaxLength(ZPropertyInfo targetPropertyInfo, ZPropertyInfo propertyInfoToCheck, int customsMaxLength, string value = null)
	{
		if (value is null && propertyInfoToCheck is ZPropertyInfoString propertyInfoString)
		{
			value = propertyInfoString.Value;
		}

		if (!string.IsNullOrWhiteSpace(value) && value.Length > customsMaxLength)
		{
			var fieldName = propertyInfoToCheck.HumanReadableName;
			targetPropertyInfo.AddWarning(ValidationCaptions.Shared.FieldIsLongerThanMaximumLength(traderName, fieldName, customsMaxLength));
		}
	}

	#region PostcodeMaximumLength

	ZInt PostcodeMaximumLength => postcodeMaximumLength ?? (postcodeMaximumLength = GetPostCodeMaximumLength()).Value;

	int? postcodeMaximumLength;

	int GetPostCodeMaximumLength()
	{
		if (IsUcc6ExportAndIsNotTransitionPeriod || IsUcc6ExportAndShouldNotApplyTransitionPeriod)
		{
			return Ucc6XmlConstants.CustomsFieldMaxLength.Trader.PostCodeExport;
		}

		return Ucc6XmlConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.PostCode;
	}

	#endregion

	#region CompanyNameMaxLength

	ZInt CompanyNameMaxLength => companyNameMaxLength ?? (companyNameMaxLength = GetCompanyNameMaxLength()).Value;
	int? companyNameMaxLength;

	int GetCompanyNameMaxLength()
	{
		if (IsUcc6ExportAndIsNotTransitionPeriodOrIsImport || IsUcc6ExportAndShouldNotApplyTransitionPeriod)
		{
			return Ucc6XmlConstants.CustomsFieldMaxLength.Trader.Name;
		}

		return SADConstants.CustomsFieldMaxLength.Trader.Name;
	}

	#endregion

	#region AddressAsSingleLineMaximumLength

	ZInt AddressAsSingleLineMaximumLength => addressAsSingleLineMaximumLength ?? (addressAsSingleLineMaximumLength = GetAddressAsSingleLineMaximumLength()).Value;
	int? addressAsSingleLineMaximumLength;

	int GetAddressAsSingleLineMaximumLength()
	{
		if (IsUcc6ExportAndIsNotTransitionPeriodOrIsImport || IsUcc6ExportAndShouldNotApplyTransitionPeriod)
		{
			return Ucc6XmlConstants.CustomsFieldMaxLength.Trader.Address;
		}

		return SADConstants.CustomsFieldMaxLength.Trader.Address;
	}

	#endregion

	bool IsUcc6ExportAndIsNotTransitionPeriod => uCC6AndTransitionPeriodProvider.IsUCC6AndIsExport && !uCC6AndTransitionPeriodProvider.IsTransitionPeriodAES30;

	bool IsUcc6ExportAndIsNotTransitionPeriodOrIsImport => IsUcc6ExportAndIsNotTransitionPeriod || uCC6AndTransitionPeriodProvider.IsImport;

	bool IsUcc6ExportAndShouldNotApplyTransitionPeriod => uCC6AndTransitionPeriodProvider.IsUCC6AndIsExport && !shouldApplyTransitionPeriod;
}
