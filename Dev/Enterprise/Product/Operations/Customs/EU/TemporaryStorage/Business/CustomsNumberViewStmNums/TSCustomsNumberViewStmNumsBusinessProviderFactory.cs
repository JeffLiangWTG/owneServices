using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using TS = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class TSCustomsNumberViewStmNumsBusinessProviderFactory : TS.TSCustomsNumberViewStmNumsBusinessProviderFactory
{
	public TSCustomsNumberViewStmNumsBusinessProviderFactory(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override string DefaultProviderValueResolverKey => (NoResString)"EUN";
}
