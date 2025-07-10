using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public sealed class NodeCustomsProfileListProvider : ICustomsProfileListProvider
{
	public NodeCustomsProfileListProvider(BusinessObjectFactory factory, ICustomsProfileListProviderSupportingData customsProfileListProviderSupportingData)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.customsProfileListProviderSupportingData = Argument.NotNull(customsProfileListProviderSupportingData, nameof(customsProfileListProviderSupportingData));
	}

	readonly BusinessObjectFactory factory;
	readonly ICustomsProfileListProviderSupportingData customsProfileListProviderSupportingData;

	CodeDescriptionPairList ICustomsProfileListProvider.GetAccountDetails(bool fetchAllIfNoneFound)
	{
		var organizationCodes = GetOrganizationCodes();
		var effectiveCompanyPK = GetEffectiveCompanyPK();

		if (organizationCodes.Count == 0)
		{
			return GetAllAccountDetailsForCompanyCached(effectiveCompanyPK);
		}

		return GetAccountDetailsFilteredByOrganizationsCached(organizationCodes, effectiveCompanyPK);
	}

	#region Implementation

	ZGuid GetEffectiveCompanyPK()
	{
		var supportingDataCompanyPK = customsProfileListProviderSupportingData.CompanyPK;
		return !supportingDataCompanyPK.IsEmpty ? supportingDataCompanyPK : GlbCompany.CurrentCompany.PK;
	}

	CodeDescriptionPairList GetAsCodeDescriptionPairList(IEnumerable<AccountDetail> accountDetails)
	{
		var result = new CodeDescriptionPairList();
		foreach (var accountDetail in accountDetails)
		{
			var account = accountDetail.Account;
			result.AddPairIfNotExist(accountDetail.InternalCode, FormattableString.Invariant($"{account.AccountNode} {accountDetail.AuthorizedUser}"));
		}
		result.Sort();
		result.DefaultCode = result.Count == 1 ? result[0].Code : ZString.Empty;
		return result;
	}

	CodeDescriptionPairList GetAllAccountDetailsForCompanyCached(ZGuid effectiveCompanyPK)
	{
		return factory.GetCachedValue(FormattableString.Invariant($"IT.NodeListLookups|NodesForCompany_{effectiveCompanyPK}"), () =>
		{
			var accountDetails = CustomsCredentialHelper.GetAllAccountDetailsForCompany();
			return GetAsCodeDescriptionPairList(accountDetails);
		});
	}

	CodeDescriptionPairList GetAccountDetailsFilteredByOrganizationsCached(HashSet<ZString> organizationCodes, ZGuid effectiveCompanyPK)
	{
		return factory.GetCachedValue(FormattableString.Invariant($"IT.NodeListLookups|NodesForCompany:{effectiveCompanyPK}_Organizations:{ZString.Join("_", organizationCodes.OrderBy(x => x).ToArray())}"), () =>
		{
			var accountDetails = new List<AccountDetail>();
			organizationCodes.ForEach(organizationCode => accountDetails.AddRange(CustomsCredentialHelper.GetAccountDetailsForDeclarant(organizationCode)));
			return GetAsCodeDescriptionPairList(accountDetails);
		});
	}

	HashSet<ZString> GetOrganizationCodes()
	{
		var organizations = customsProfileListProviderSupportingData.GetEligibleOrganizations() ?? Enumerable.Empty<OrgHeader>();

		return organizations
			.WhereNotNull()
			.Select(x => x.OH_Code)
			.ToHashSet();
	}

	#endregion
}
