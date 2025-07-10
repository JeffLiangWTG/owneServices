using Enterprise.LogShipping.Setup;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	abstract class LogShippingTestFixture : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var services = Program.CreateApplicationServiceComposition();
			ConfigureTestServices(services);
			Program.ServiceProvider = services.BuildServiceProvider();
		}

		public virtual void ConfigureTestServices(IServiceCollection services)
		{
		}
	}
}
