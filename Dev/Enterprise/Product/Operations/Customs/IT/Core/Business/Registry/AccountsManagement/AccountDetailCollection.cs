using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Registry;

public class AccountDetailCollection : NonPersistentBusinessObjectCollection<AccountDetail>
{
	public AccountDetailCollection(Account account, BusinessObjectFactory factory)
		: base(factory)
	{
		this.account = Argument.NotNull(account, nameof(account));
		Argument.NotNull(factory, nameof(factory));
	}
	readonly Account account;

	protected override BusinessObject CreateNonPersistentBusinessObject() => new AccountDetail(account, Factory);
}
