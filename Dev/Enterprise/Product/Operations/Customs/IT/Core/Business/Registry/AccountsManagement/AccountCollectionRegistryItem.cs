using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Registry;

public class AccountCollectionRegistryItem : StronglyTypedRegistryItem<AccountCollection>
{
	public AccountCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AccountCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new AccountCollectionRegistryDataType(), storage, defaultValue))
	{
	}

	public AccountCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, AccountCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new AccountCollectionRegistryDataType(), storage, options, defaultValue))
	{
	}

	public IEnumerable<CompanyAccountCollection> AccountCollectionsGroupedByCompany => GetAndMergeAccountNumbersGroupedByCompany();
	List<CompanyAccountCollection> accountCollectionGroupedByCompany;

	Dictionary<ZGuid, AccountCollection> AccountCollectionsGroupedByCompanyInEditing => accountCollectionsGroupedByCompanyInEditing ?? (accountCollectionsGroupedByCompanyInEditing = new Dictionary<ZGuid, AccountCollection>());
	Dictionary<ZGuid, AccountCollection> accountCollectionsGroupedByCompanyInEditing;

	public void AddToAccountCollectionInEditingCache(AccountCollection collection, FallbackLevel fallbackLevel) => AccountCollectionsGroupedByCompanyInEditing[fallbackLevel.CompanyPK(false)] = collection;

	public void ResetAccountCollectionsGroupedByCompany()
	{
		accountCollectionGroupedByCompany = null;
		accountCollectionsGroupedByCompanyInEditing.Clear();
	}

	IEnumerable<CompanyAccountCollection> GetAndMergeAccountNumbersGroupedByCompany()
	{
		if (accountCollectionGroupedByCompany == null)
		{
			accountCollectionGroupedByCompany = LoadAccountNumbersGroupedByCompanyFromSavedValues();
		}

		foreach (var companyInEditing in AccountCollectionsGroupedByCompanyInEditing)
		{
			var companyWithAccounts = accountCollectionGroupedByCompany.SingleOrDefault(x => x.CompanyPK == companyInEditing.Key);
			if (companyWithAccounts == null)
			{
				accountCollectionGroupedByCompany.Add(new CompanyAccountCollection(companyInEditing.Key, companyInEditing.Value));
			}
			else
			{
				companyWithAccounts.Accounts = companyInEditing.Value;
			}
		}

		return accountCollectionGroupedByCompany;
	}

	List<CompanyAccountCollection> LoadAccountNumbersGroupedByCompanyFromSavedValues()
	{
		var accountsNumberGroupedByCompany = new List<CompanyAccountCollection>();
		foreach (var company in GlbCompany.GetActiveCompanies())
		{
			var companyPK = company.PK;
			var accountCollection = GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			if (accountCollection.Any())
			{
				accountsNumberGroupedByCompany.Add(new CompanyAccountCollection(companyPK, accountCollection));
			}
		}
		return accountsNumberGroupedByCompany;
	}
}

[RegistryEditor("Enterprise.Customs.IT.GUI.Registry.AccountsManagement.AccountManagementRegistryItemEditor, Enterprise.Customs.IT.GUI")]
public class AccountCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AccountCollection>
{
	public AccountCollectionRegistryDataType()
	{
	}
}
