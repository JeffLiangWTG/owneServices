using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithStrategyProvider : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.Dummy3;

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			StrategyFactory = factory;
		}

		internal BusinessObjectFactory StrategyFactory;
	}
}
