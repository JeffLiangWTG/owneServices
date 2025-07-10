using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry;

public class CompanyAccountCollection
{
	public CompanyAccountCollection(ZGuid companyPK, AccountCollection accounts)
	{
		CompanyPK = companyPK;
		Accounts = accounts;
	}

	public ZGuid CompanyPK { get; }
	public AccountCollection Accounts { get; set; }
}
