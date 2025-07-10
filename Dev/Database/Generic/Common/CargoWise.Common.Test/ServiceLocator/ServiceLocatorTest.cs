using System;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class ServiceLocatorTest : TestCase
	{
		public void TestLocatorWithNull()
		{
			AssertEquals(null, ServiceLocator.GetService<IAssemblyLoader>(null));
		}

		public void TestLocatorWithMiss()
		{
			AssertEquals(null, ServiceLocator.GetService<IAssemblyLoader>(this));
		}

		public void TestLocatorWithInterfaceOnClass()
		{
			AssertEquals(this, ServiceLocator.GetService<ITest>(this));
		}

		class Provider : IServiceLocator
		{
			#region IServiceLocator Members
			object IServiceLocator.GetService(Type serviceType)
			{
				return string.Empty;
			}
			#endregion
		}

		public void TestLocatorWithInterfaceOffClass()
		{
			object provider = new Provider();
			AssertEquals(string.Empty, ServiceLocator.GetService<string>(provider));
		}
	}
}