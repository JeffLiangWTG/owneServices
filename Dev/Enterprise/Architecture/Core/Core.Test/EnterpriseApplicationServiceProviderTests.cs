using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class EnterpriseApplicationServiceProviderTests : TestCase
	{
		public void TestResolvesViaObjectFactory()
		{
			using (ObjectFactory.Substitute<IDummy>(new Dummy()))
			{
				AssertSame(ObjectFactory.Get<IDummy>(), provider.GetService(typeof(IDummy)));
				AssertSame(ObjectFactory.Get<IDummy>(), provider.GetService<IDummy>());
				AssertSame(ObjectFactory.Get<IDummy>(), provider.GetRequiredService(typeof(IDummy)));
				AssertSame(ObjectFactory.Get<IDummy>(), provider.GetRequiredService<IDummy>());
			}
		}

		readonly IServiceProvider provider = new EnterpriseApplicationServiceProvider();

		interface IDummy
		{
		}

		sealed class Dummy : IDummy
		{
		}
	}
}
