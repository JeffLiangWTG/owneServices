using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Registry;

public class AccountLookups
{
	public AccountLookups(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	readonly BusinessObjectFactory factory;

	public CodeDescriptionPairList AccountStatusList => factory.GetCachedValue<AccountStatusList>();

	public CodeDescriptionPairList AccountCertificateStatusList => factory.GetCachedValue<AccountCertificateStatusList>();
}
