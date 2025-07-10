using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business;

public class ManifestHeadersWrapperValidation : ZValidation
{
	public ManifestHeadersWrapperValidation(ManifestHeadersWrapper wrapper)
		: base(wrapper)
	{
		this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
	}

	readonly ManifestHeadersWrapper wrapper;

	public void ValidateWR_CountryCode()
	{
		ValidateCalculatedProperty(wrapper.WR_CountryCodeInfo);
	}

	protected void CheckWR_CountryCode()
	{
		ListValidation.MessageErrorIfInvalidCode(wrapper.WR_CountryCodeInfo, wrapper.CountryCodes);
	}

	public void ValidateWR_ManifestType()
	{
		ValidateCalculatedProperty(wrapper.WR_ManifestTypeInfo);
	}

	protected void CheckWR_ManifestType()
	{
		if (wrapper.CountryCodes.ContainsCode(wrapper.WR_CountryCode))
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(wrapper.WR_ManifestTypeInfo, wrapper.ManifestTypes, ResString.GetMultilingualString("88E113F1-C02C-4C5B-A281-A56F34F88848", "New Manifest Type"));
		}
	}

	public void ValidateWR_KeywordCombination()
	{
		ValidateCalculatedProperty(wrapper.WR_KeywordCombinationInfo);
	}

	protected void CheckWR_KeywordCombination()
	{
		ListValidation.MessageErrorIfInvalidCode(wrapper.WR_KeywordCombinationInfo);
	}

	public override void ValidateAll()
	{
		ValidateWR_CountryCode();
		ValidateWR_ManifestType();
		ValidateWR_KeywordCombination();
	}

	public override Type AutoValidationType => typeof(ManifestHeadersWrapperValidation);
}
