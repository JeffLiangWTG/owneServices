using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class AuthorizationListProvider
{
	public AuthorizationListProvider(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly BusinessObjectFactory factory;

	public CodeDescriptionPairList GetAuthorizations(IAuthorizationListDataProvider authorizationListDataProvider)
	{
		Argument.NotNull(authorizationListDataProvider, nameof(authorizationListDataProvider));
		var eligibleHolders = authorizationListDataProvider.GetEligibleHolders();
		var authorisationTypes = authorizationListDataProvider.AuthorizationTypes.ToArray();

		if (!authorisationTypes.Any() || !eligibleHolders.Any())
		{
			return new CodeDescriptionPairList();
		}

		var transactionDate = ZDateTime.Today;
		var cacheKey = FormattableString.Invariant($"IT.AuthorizationNumberList_{JoinValuesForCacheKey(authorisationTypes)}_{transactionDate}_{JoinValuesForCacheKey(eligibleHolders)}");

		return factory.GetCachedValue(cacheKey, () => GetAuthorisationsCachedList(eligibleHolders, authorisationTypes, transactionDate));

		string JoinValuesForCacheKey<T>(IEnumerable<T> values) => string.Join("_", values);
	}

	CodeDescriptionPairList GetAuthorisationsCachedList(IEnumerable<ZGuid> eligibleHolders, ZString[] authorisationTypes, ZDateTime transactionDate)
	{
		var result = new CodeDescriptionPairList();
		var authorisationList = CusAuthorisationHeader.Loader.GetAuthorisations(factory, Core.Constants.CountryCodes.Italy, authorisationTypes, transactionDate, eligibleHolders.ToArray());
		foreach (var authorisationNumber in authorisationList)
		{
			result.AddPairIfNotExist(authorisationNumber.CPH_Number, authorisationNumber.PermitHolder?.OH_Code ?? ZString.Empty);
		}
		result.Sort();
		return result;
	}
}
