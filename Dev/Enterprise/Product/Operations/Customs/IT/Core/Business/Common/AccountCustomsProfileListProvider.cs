using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public sealed class AccountCustomsProfileListProvider : ICustomsProfileListProvider
{
	public AccountCustomsProfileListProvider(BusinessObjectFactory factory, ICustomsProfileListProviderSupportingData supportingData)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.supportingData = Argument.NotNull(supportingData, nameof(supportingData));
	}

	readonly BusinessObjectFactory factory;
	readonly ICustomsProfileListProviderSupportingData supportingData;

	CodeDescriptionPairList ICustomsProfileListProvider.GetAccountDetails(bool fetchAllIfNoneFound)
	{
		var organizationCodes = GetOrganizationCodes();
		if (organizationCodes.Count == 0)
		{
			return GetAllAccountDetailsForCompanyCached(supportingData.CompanyPK);
		}

		var accountDetails = GetAccountDetailsFilteredByCompanyAndOrganizationsCached(organizationCodes, supportingData.CompanyPK);
		if (accountDetails.Count == 0 && fetchAllIfNoneFound)
		{
			return GetAllAccountDetailsForCompanyCached(supportingData.CompanyPK);
		}

		return accountDetails;
	}

	#region Implementation

	public CodeDescriptionPairList GetAllAccountDetailsForCompanyCached(ZGuid effectiveCompanyPK)
	{
		var cacheKey = FormattableString.Invariant($"IT.AccountListLookups|AccountsForCompany_{effectiveCompanyPK}");
		return factory.GetCachedValue(cacheKey, () =>
		{
			var accountDetails = GetAccountDetailsForCompany();
			return GetSortedList(accountDetails);
		});
	}

	CodeDescriptionPairList GetSortedList(IEnumerable<GlbMauExternalPassword> accountDetailList)
	{
		var result = new CodeDescriptionPairList();
		foreach (var accountDetail in accountDetailList)
		{
			result.AddPair(accountDetail.GP_UserID, FormattableString.Invariant($"{accountDetail.GP_MailBoxID} - {accountDetail.GP_Name}"));
		}
		result.Sort();
		result.DefaultCode = result.Count == 1 ? result[0].Code : ZString.Empty;
		return result;
	}

	IEnumerable<GlbMauExternalPassword> GetAccountDetailsForCompany()
	{
		var company = factory.Load<GlbCompany>(supportingData.CompanyPK);
		var companyWrapper = GlbCompanyWrapper.Get(company);
		return companyWrapper.PasswordCollection.Cast<GlbMauExternalPassword>();
	}

	CodeDescriptionPairList GetAccountDetailsFilteredByCompanyAndOrganizationsCached(HashSet<ZString> organizationCodes, ZGuid effectiveCompanyPK)
	{
		var cacheKey = FormattableString.Invariant($"IT.AccountListLookups|AccountsForCompany:{effectiveCompanyPK}_Organizations:{ZString.Join("_", organizationCodes.OrderBy(x => x).ToArray())}");
		return factory.GetCachedValue(cacheKey, () =>
		{
			var accountDetails = GetAccountDetailsForCompany().Where(x => organizationCodes.Contains(x.GP_Name));
			return GetSortedList(accountDetails);
		});
	}

	HashSet<ZString> GetOrganizationCodes()
	{
		var organizations = supportingData.GetEligibleOrganizations() ?? Enumerable.Empty<OrgHeader>();

		return organizations
			.WhereNotNull()
			.Select(x => x.OH_Code)
			.ToHashSet();
	}

	#endregion
}
