using System;
using System.ComponentModel.Design;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Common.Design.Testing
{
	[RequiresSoftware(RequiredSoftware.VisualStudio)]
	class DesignTimeAssemblyLoaderTest : TestCase
	{
		public void TestLoadAssembly()
		{
			Assembly loadedAssembly = AssemblyLoader.LoadAssembly(typeof(TestTypeResolutionService).Assembly.GetName());
			AssertEquals(loadedAssembly, typeof(TestTypeResolutionService).Assembly);
		}

		public void TestLoadAssembly_WhenTypeResolutionServiceUnavailable()
		{
			Assembly loadedAssembly = new DesignTimeAssemblyLoader(new EmptyServiceProvider()).LoadAssembly(typeof(TestTypeResolutionService).Assembly.GetName());
			AssertEquals("No assembly can be loaded if there is no ITypeResolutionService", null, loadedAssembly);
		}

		DesignTimeAssemblyLoader AssemblyLoader
		{
			get
			{
				return assemblyLoader ?? (assemblyLoader = new DesignTimeAssemblyLoader(new TestTypeResolutionService()));
			}
		}

		DesignTimeAssemblyLoader assemblyLoader;
		#region Test Classes
		class EmptyServiceProvider : IServiceProvider
		{
			public object GetService(Type serviceType)
			{
				return null;
			}
		}

		class TestTypeResolutionService : IServiceProvider, ITypeResolutionService, ITypeResolutionServiceEx
		{
			public Assembly LoadAssembly(Assembly resolvedAssembly, AssemblyName assemblyName)
			{
				if (assemblyName.Name == typeof(TestTypeResolutionService).Assembly.GetName().Name)
				{
					return typeof(TestTypeResolutionService).Assembly;
				}

				return null;
			}

			Assembly ITypeResolutionServiceEx.LoadAssembly(string path)
			{
				throw new NotSupportedException();
			}

			#region IServiceProvider Members
			public object GetService(Type serviceType)
			{
				if (serviceType == typeof(ITypeResolutionService))
				{
					return this;
				}

				return null;
			}

			#endregion
			#region ITypeResolutionService Members
			Assembly ITypeResolutionService.GetAssembly(AssemblyName name, bool throwOnError)
			{
				throw new NotImplementedException();
			}

			Assembly ITypeResolutionService.GetAssembly(AssemblyName name)
			{
				throw new NotImplementedException();
			}

			string ITypeResolutionService.GetPathOfAssembly(AssemblyName name)
			{
				throw new NotImplementedException();
			}

			Type ITypeResolutionService.GetType(string name, bool throwOnError, bool ignoreCase)
			{
				throw new NotImplementedException();
			}

			Type ITypeResolutionService.GetType(string name, bool throwOnError)
			{
				throw new NotImplementedException();
			}

			Type ITypeResolutionService.GetType(string name)
			{
				throw new NotImplementedException();
			}

			void ITypeResolutionService.ReferenceAssembly(AssemblyName name)
			{
				throw new NotImplementedException();
			}
			#endregion
		}
		#endregion
	}
}