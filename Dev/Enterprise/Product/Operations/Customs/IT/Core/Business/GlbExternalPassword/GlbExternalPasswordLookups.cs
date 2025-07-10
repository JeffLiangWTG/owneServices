using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class GlbExternalPasswordLookups : MasterFiles.Business.GlbExternalPasswordLookups
{
	public GlbExternalPasswordLookups(GlbExternalPassword parent)
		: base(parent)
	{
	}

	public IEnumerable<AccountAndCompanyPair> AllAccountsWithCompany
	{
		get
		{
			return Factory.GetCachedValue("GlbExternalPasswordLookups_IT.AllAccountsWithCompany", () =>
			{
				var activeCompanies = GlbCompany.GetActiveCompanies(countryCode: null, Factory);
				var accountList = new List<AccountAndCompanyPair>();
				foreach (var company in activeCompanies)
				{
					foreach (var account in ITAccountsManagementRegistry.Instance.AccountsManagement.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty).Cast<Account>())
					{
						accountList.Add(new AccountAndCompanyPair(account, company));
					}
				}
				return accountList;
			});
		}
	}
}
