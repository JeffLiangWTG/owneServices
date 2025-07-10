using CargoWise.Common;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class AccountAndCompanyPair
{
	public AccountAndCompanyPair(Account account, GlbCompany company)
	{
		Account = Argument.NotNull(account, nameof(account));
		Company = Argument.NotNull(company, nameof(company));
	}

	public Account Account { get; }
	public GlbCompany Company { get; }
}
