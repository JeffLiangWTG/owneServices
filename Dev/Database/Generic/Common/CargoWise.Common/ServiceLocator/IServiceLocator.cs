using System;

namespace CargoWise.Common
{
	public interface IServiceLocator
	{
		object GetService(Type serviceType);
	}
}