using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Registry;

public class AccountDetailLookups
{
	public AccountDetailLookups(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly BusinessObjectFactory factory;

	public OrgHeaderCollection Declarants => new OrgHeaderCollection(factory);
}
