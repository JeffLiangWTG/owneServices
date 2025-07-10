using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	public class ServiceTaskBusinessObjectBindingTest : TestCase
	{
		public void TestBusinessObjectBindings()
		{
			Assert("Service task bindings should be disabled on test environments", !SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.Value);
		}
	}
}
