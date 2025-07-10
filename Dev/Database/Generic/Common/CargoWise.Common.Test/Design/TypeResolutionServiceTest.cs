using System;
using System.ComponentModel.Design;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Common.Design.Testing
{
	[RequiresSoftware(RequiredSoftware.VisualStudio)]
	class TypeResolutionServiceTest : TestCase
	{
		public void TestLoadAssembly()
		{
			Assembly assemblyInSameDirectory = typeof(TypeResolutionServiceTest).Assembly;
			Assembly loadedAssembly = TypeResolutionService.LoadAssembly(new TestServiceProvider(), assemblyInSameDirectory, new AssemblyName("CargoWise.Common.Testing"));
			AssertEquals("Assembly loaded without wrapper", typeof(TypeResolutionServiceTest).Assembly, loadedAssembly);
			loadedAssembly = new ATypeResolutionServiceWrapper(TypeResolutionService).LoadAssembly(new TestServiceProvider(), assemblyInSameDirectory, new AssemblyName("CargoWise.Common.Testing"));
			AssertEquals("Assembly loaded with wrapper", typeof(TypeResolutionServiceTest).Assembly, loadedAssembly);
		}

		public void TestDynamicTypeServiceType_TypeAndMethodExists()
		{
			AssertNotNull("DynamicTypeService class exists", TypeResolutionServiceEx.DynamicTypeServiceType);
			MethodInfo createDynamicAssemblyMethod = TypeResolutionServiceEx.DynamicTypeServiceType.GetMethod("CreateDynamicAssembly", new Type[] { typeof(string) }, null);
			AssertNotNull("CreateDynamicAssembly method exists", createDynamicAssemblyMethod);
		}

		#region Test Classes
		class TestServiceProvider : IServiceProvider
		{
			public object GetService(Type serviceType)
			{
				if (serviceType == TypeResolutionServiceEx.DynamicTypeServiceType)
				{
					return new TestDynamicTypeService();
				}
				else if (serviceType == typeof(ITypeResolutionService))
				{
					return new TestTypeResolutionService();
				}

				return null;
			}
		}

		class ATypeResolutionServiceWrapper : ITypeResolutionService
		{
			public ATypeResolutionServiceWrapper(ITypeResolutionService inner)
			{
				this.Inner = inner;
			}

			public ITypeResolutionService Inner { get; private set; }

			public string GetPathOfAssembly(AssemblyName name)
			{
				return Inner.GetPathOfAssembly(name);
			}

			#region ITypeResolutionService Members
			Assembly ITypeResolutionService.GetAssembly(AssemblyName name, bool throwOnError)
			{
				throw new NotImplementedException();
			}

			Assembly ITypeResolutionService.GetAssembly(AssemblyName name)
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

		class TestTypeResolutionService : ITypeResolutionService
		{
			public string GetPathOfAssembly(AssemblyName name)
			{
				return new Uri(Assembly.Load(name).Location).LocalPath;
			}

			#region ITypeResolutionService Members
			Assembly ITypeResolutionService.GetAssembly(AssemblyName name, bool throwOnError)
			{
				throw new NotImplementedException();
			}

			Assembly ITypeResolutionService.GetAssembly(AssemblyName name)
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

		class TestDynamicTypeService
		{
			public Assembly CreateDynamicAssembly(string path)
			{
				if (path.Contains("CargoWise.Common.Testing"))
				{
					return typeof(TestTypeResolutionService).Assembly;
				}

				return null;
			}
		}

		#endregion
		#region Implementation

		TestTypeResolutionService TypeResolutionService
		{
			get
			{
				return typeResolutionService ?? (typeResolutionService = new TestTypeResolutionService());
			}
		}

		TestTypeResolutionService typeResolutionService;
		#endregion
	}
}
