using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyActionMethodControllerWithoutMethods : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return System.Array.Empty<OperationalActionMethod>();
		}
	}
}
