using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodControllerWithMethods : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new DummyActionMethodWithGui(),
				new DummyActionMethodWithLargeGui(),
				new DummyActionMethodWithOversizedGui(),
				new DummyActionMethodWithoutGui(),
				new DummyActionMethodRunWithoutUI(),
			};
		}
	}
}
