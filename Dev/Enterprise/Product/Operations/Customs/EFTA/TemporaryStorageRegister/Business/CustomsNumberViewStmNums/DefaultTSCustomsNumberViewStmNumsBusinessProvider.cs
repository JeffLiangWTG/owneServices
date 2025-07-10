using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public sealed class DefaultTSCustomsNumberViewStmNumsBusinessProvider : TSCustomsNumberViewStmNumsBusinessProvider
{
	public DefaultTSCustomsNumberViewStmNumsBusinessProvider(BusinessObjectFactory factory, ZString countryCode, ZGuid ownerPk) : base(factory, countryCode, ownerPk)
	{
	}
}
