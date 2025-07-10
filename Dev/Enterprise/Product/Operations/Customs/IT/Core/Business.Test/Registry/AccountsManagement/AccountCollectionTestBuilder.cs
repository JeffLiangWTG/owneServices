using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry.Testing;

public class AccountCollectionTestBuilder
{
	public AccountCollectionTestBuilder(ZGuid companyPk)
	{
		accountCollection = new AccountCollection();
		this.companyPk = companyPk;
	}

	readonly AccountCollection accountCollection;
	readonly ZGuid companyPk;

	public AccountTestBuilder AppendAccount(ZString number, ZString node, string rangeStart = null, string rangeEnd = null)
	{
		var newAccount = accountCollection.AddNew();
		newAccount.AccountNumber = number;
		newAccount.AccountNode = node;
		newAccount.AccountPassword = "12345678";
		newAccount.AccountRangeStart = rangeStart;
		newAccount.AccountRangeEnd = rangeEnd;

		return new AccountTestBuilder(this, newAccount);
	}

	public AccountCollection Build()
	{
		ITAccountsManagementRegistry.Instance.AccountsManagement.SetValue(companyPk.ToGuid(), Guid.Empty, Guid.Empty, accountCollection);
		return accountCollection;
	}
}

public class AccountTestBuilder
{
	public AccountTestBuilder(AccountCollectionTestBuilder parent, Account account)
	{
		this.parent = parent;
		this.account = account;
	}

	readonly AccountCollectionTestBuilder parent;
	readonly Account account;

	public AccountTestBuilder AppendAccountDetail(ZString internalCode, ZString declarantCode, string authorizationUser = null)
	{
		var accountDetail = account.AccountDetails.AddNew();
		accountDetail.InternalCode = internalCode;
		accountDetail.DeclarantCode = declarantCode;
		accountDetail.AuthorizedUser = authorizationUser ?? account.AccountNumber;

		return this;
	}

	public AccountTestBuilder AppendAccount(ZString number, ZString node, string rangeStart = null, string rangeEnd = null)
	{
		return parent.AppendAccount(number, node, rangeStart, rangeEnd);
	}

	public AccountCollection Build()
	{
		return parent.Build();
	}
}
