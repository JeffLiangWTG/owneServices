using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public abstract class GoodsLocationListProvider<T>
	where T : GoodsLocationList, new()
{
	protected GoodsLocationListProvider(IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider, BusinessObjectFactory factory)
	{
		this.authorisationWithCustomsOfficeProvider = Argument.NotNull(authorisationWithCustomsOfficeProvider, nameof(authorisationWithCustomsOfficeProvider));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider;
	protected readonly BusinessObjectFactory factory;

	public CodeDescriptionPairList Locations
	{
		get
		{
			var authorisationNumber = authorisationWithCustomsOfficeProvider.AuthorizationNumber;
			if (authorisationNumber.IsEmpty)
			{
				return authorisationWithCustomsOfficeProvider.IsExport ? factory.GetCachedValue<T>() : new CodeDescriptionPairList();
			}
			return GetLocationsByAuthorisation();
		}
	}

	#region Implementation

	CodeDescriptionPairList GetLocationsByAuthorisation()
	{
		var authorisation = authorisationWithCustomsOfficeProvider.Authorization;
		if (authorisation != null)
		{
			var customsOffice = authorisationWithCustomsOfficeProvider.CustomsOffice;
			return factory.GetCachedValue(FormattableString.Invariant($"{authorisation.PK}|{customsOffice}"), () =>
			{
				return GetLocations(authorisation, customsOffice);
			});
		}
		return new CodeDescriptionPairList();
	}

	CodeDescriptionPairList GetLocations(Customs.Business.CusAuthorisationHeader authorisation, CargoWise.Types.ZString customsOffice)
	{
		var cachedCodeDescriptionPairList = new CodeDescriptionPairList();
		var locationList = authorisation.CusAuthorisationRules
			.Where(x => x.CPR_RuleCode == ITCusAuthorisationRuleTypeList.Codes.Location && (customsOffice.IsEmpty || x.LinkedCusAuthorisationRules.Any(y => y.CPR_ValueFrom == customsOffice)));

		foreach (var location in locationList)
		{
			cachedCodeDescriptionPairList.AddPairIfNotExist(location.CPR_ValueFrom, location.CPR_Description);
		}

		return cachedCodeDescriptionPairList;
	}

	#endregion
}
