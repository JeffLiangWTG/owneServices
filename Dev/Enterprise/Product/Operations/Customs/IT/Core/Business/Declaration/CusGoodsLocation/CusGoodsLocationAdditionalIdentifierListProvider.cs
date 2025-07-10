using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusGoodsLocationAdditionalIdentifierListProvider
{
	public CusGoodsLocationAdditionalIdentifierListProvider(BusinessObjectFactory factory, ICusGoodsLocationWrapper goodsLocationWrapper)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.goodsLocationWrapper = Argument.NotNull(goodsLocationWrapper, nameof(goodsLocationWrapper));
	}

	readonly BusinessObjectFactory factory;
	readonly ICusGoodsLocationWrapper goodsLocationWrapper;

	public CodeDescriptionPairList GetCachedList() => GetCachedList(DefaultLocationAuthorisationTypes.ToArray());

	public CodeDescriptionPairList GetCachedList(ZString[] locationAuthorisationTypes)
	{
		var goodsLocationAddress = goodsLocationWrapper.Address;
		var identificationHolderPk = goodsLocationAddress.IdentificationHolderPK;
		var authorisationNumber = goodsLocationAddress.AuthorisationNumber;
		var customsOffices = goodsLocationWrapper.GetCustomOfficeCodes()?.Where(c => !c.IsEmpty).ToArray() ?? Array.Empty<ZString>();

		var cacheKey = FormattableString.Invariant($"IT|AdditionalIdentifier|{identificationHolderPk}|{string.Join("|", locationAuthorisationTypes)}|{authorisationNumber}|{string.Join("|", customsOffices)}");
		return factory.GetCachedValue(cacheKey, () => GetListCore(identificationHolderPk, locationAuthorisationTypes, authorisationNumber, customsOffices));
	}

	#region Implementation

	CodeDescriptionPairList GetListCore(ZGuid identificationHolderPk, ZString[] locationAuthorisationTypes, ZString authorisationNumber, ZString[] customOffices)
	{
		if (identificationHolderPk.IsEmpty)
		{
			return new CodeDescriptionPairList();
		}

		var cusAuthorisationHeaders = CusAuthorisationHeader.Loader
			.GetAuthorisations(factory, Core.Constants.CountryCodes.Italy, locationAuthorisationTypes, ZDateTime.Now, identificationHolderPk);

		cusAuthorisationHeaders = FilterByAuthorisationNumber(authorisationNumber, cusAuthorisationHeaders);
		var locRules = GetFilteredLocRules(cusAuthorisationHeaders, customOffices);

		return PrepareCodeDescriptionPairListResult(locRules);
	}

	IEnumerable<CusAuthorisationRule> GetFilteredLocRules(CusAuthorisationHeader[] cusAuthorisationHeaders, ZString[] customOffices)
	{
		var locRules = cusAuthorisationHeaders
			.SelectMany(x => x.CusAuthorisationRules)
			.Where(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location);

		if (customOffices.Any())
		{
			locRules = locRules.Where(x => HasLinkedCusRule(x, customOffices));
		}

		return locRules;
	}

	CusAuthorisationHeader[] FilterByAuthorisationNumber(ZString authorisationNumber, CusAuthorisationHeader[] cusAuthorisationHeaders)
	{
		if (!authorisationNumber.IsEmpty)
		{
			return cusAuthorisationHeaders
				.Where(x => x.CPH_Number == authorisationNumber)
				.ToArray();
		}
		return cusAuthorisationHeaders;
	}

	bool HasLinkedCusRule(CusAuthorisationRule rule, ZString[] customOffices)
	{
		return rule.LinkedCusAuthorisationRules
			.Any(y => y.CPR_RuleCode == CusLinkedRuleCode && y.CPR_ValueFrom.In(customOffices));
	}

	CodeDescriptionPairList PrepareCodeDescriptionPairListResult(IEnumerable<CusAuthorisationRule> locRules)
	{
		var result = new CodeDescriptionPairList();
		locRules.ForEach(r => result.AddPair(r.CPR_ValueFrom, GetPairDescription(r.AuthorisationHeader)));
		return result;
	}

	string GetPairDescription(CusAuthorisationHeader cusAuthorisationHeader)
	{
		return FormattableString.Invariant($"{cusAuthorisationHeader.CPH_Number} - {cusAuthorisationHeader.CPH_Type}");
	}

	ImmutableArray<ZString> DefaultLocationAuthorisationTypes => defaultLocationAuthorisationTypes.Value;

	readonly Lazy<ImmutableArray<ZString>> defaultLocationAuthorisationTypes = new(() => new ZString[]
	{
		CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport,
		CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport,
	}.ToImmutableArray());

	const string CusLinkedRuleCode = "CUS";

	#endregion
}
