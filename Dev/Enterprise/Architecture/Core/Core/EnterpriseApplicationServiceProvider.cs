using System;
using CargoWise.Application;

namespace Enterprise.ZArchitecture.Core
{
	sealed class EnterpriseApplicationServiceProvider : IServiceProvider
	{
		public object GetService(Type serviceType) => ObjectFactory.Get(serviceType);
	}
}
