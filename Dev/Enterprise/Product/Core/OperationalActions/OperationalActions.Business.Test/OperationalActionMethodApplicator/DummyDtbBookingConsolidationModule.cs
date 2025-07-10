using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	class DummyDtbBookingConsolidationModule : DummyFilterGridModule, IFilterModuleForFactory
	{
		public bool GetNewFactoryForFilterModeCalled { get; private set; }

		BusinessObjectFactory IFilterModuleForFactory.GetNewFactoryForFilterModule()
		{
			GetNewFactoryForFilterModeCalled = true;
			return new BusinessObjectFactory();
		}
	}
}
