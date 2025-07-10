#if DEBUG
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using CargoWise.Common;
using CargoWise.Common.Design.Testing;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KTypeResolutionServiceTests : TestCase
	{
		#region GetType

		protected override void SetUp()
		{
			base.SetUp();
			TypeResolutionService.AddAssembly(typeof(DesignerServices).Assembly);
			TypeResolutionService.AddAssembly(typeof(ActiveDesignerEventArgs).Assembly);
		}

		public void TestGetType()
		{
			Type testType = typeof(DesignerServices);
			AssertEquals("GetType for existant type", testType, TypeResolutionService.GetType(testType.FullName));
			AssertEquals("GetType for existant type", testType, TypeResolutionService.GetType(testType.FullName.ToLower(), false, true));
			AssertEquals("GetType for non-existant type", testType, TypeResolutionService.GetType(testType.FullName.ToLower(), false, true));
		}

		public void TestGetType_ThrowOnError()
		{
			Type testType = typeof(DesignerServices);
			AssertEquals("GetType for existant type", testType, TypeResolutionService.GetType(testType.FullName, true));
			try
			{
				Type type = TypeResolutionService.GetType("NonExistantType", true);
				Fail("Expected an exception");
			}
			catch (TypeLoadException)
			{
				Assert(true);
			}
		}

		public void TestGetType_WithAddedAssembly()
		{
			TypeResolutionService.AddAssembly(typeof(ActiveDesignerEventArgs).Assembly);
			AssertNotNull(TypeResolutionService.GetType(typeof(ActiveDesignerEventArgs).FullName));
		}

		#endregion

		#region GetAssembly

		public void TestGetAssembly()
		{
			AssertEquals("GetAssembly with existing assembly", typeof(DesignerServices).Assembly, TypeResolutionService.GetAssembly(typeof(DesignerServices).Assembly.GetName(), false));
			AssertEquals("GetAssembly with non-existant assembly", null, TypeResolutionService.GetAssembly(new AssemblyName("xxx"), false));
		}

		public void TestGetAssembly_ThrowOnError()
		{
			try
			{
				TypeResolutionService.GetAssembly(new AssemblyName("xxx"), true);
				Fail("Expected an exception due to non-existant assembly");
			}
			catch (FileNotFoundException)
			{
				Assert(true);
			}
		}

		public void TestGetAssembly_WithAddedAssembly()
		{
			AssemblyBuilder mockAssembly =  AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("MockAssembly"), AssemblyBuilderAccess.Run);
			TypeResolutionService.AddAssembly(mockAssembly);
			AssertNotNull(TypeResolutionService.GetAssembly(new AssemblyName("MockAssembly")));
		}

		#endregion

		#region LoadAssemblyFrom

		public void TestLoadAssemblyFrom()
		{
			Assembly diagnosticsAssembly = TypeResolutionService.LoadAssemblyFrom(typeof(Assertion).Assembly.Location);
			AssertEquals(typeof(Assertion).Assembly, diagnosticsAssembly);
		}

		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestExistsTypeResolutionService()
		{
			AssertNotNull("Microsoft.VisualStudio.Shell.Design.DynamicTypeService exists", TypeResolutionService.BaseDynamicTypeServiceType);
		}

		#endregion

		#region Test Classes

		class TestTypeResolutionService : KTypeResolutionService
		{
			public TestTypeResolutionService(IServiceProvider serviceProvider, ITypeResolutionService inner) : base(serviceProvider, inner)
			{ }

			protected override Type DynamicTypeServiceType
			{ get { return typeof(MockDynamicTypeService); } }

			public Type BaseDynamicTypeServiceType
			{ get { return base.DynamicTypeServiceType; } }
		}

		class MockDotNetTypeResolutionService : ITypeResolutionService
		{
			#region ITypeResolutionService Members

			public Assembly GetAssembly(AssemblyName name)
			{ return GetAssembly(name, false); }

			public Assembly GetAssembly(AssemblyName name, bool throwOnError)
			{
				Assembly result = null;
				try
				{
					result = Assembly.Load(name);
				}
				catch (Exception ex)
				{
					if (throwOnError || ex.IsCriticalException())
					{
						throw;
					}
				}
				return result;
			}

			public Type GetType(string name)
			{ return GetType(name, false, true); }

			public Type GetType(string name, bool throwOnError)
			{ return GetType(name, throwOnError, false); }

			public Type GetType(string name, bool throwOnError, bool ignoreCase)
			{ return Type.GetType(name, throwOnError, ignoreCase); }

			public string GetPathOfAssembly(AssemblyName name)
			{ throw new NotImplementedException(); }

			public void ReferenceAssembly(AssemblyName name)
			{ throw new NotImplementedException(); }

			#endregion
		}

		class MockServiceProvider : IServiceProvider
		{
			public MockServiceProvider(KTypeResolutionServiceTests owner)
			{ this.owner = owner; }

			public object GetService(Type serviceType)
			{
				object result = null;
				if (serviceType == typeof(MockDynamicTypeService))
				{
					result = owner.DynamicTypeService;
				}
				return result;
			}

			readonly KTypeResolutionServiceTests owner;
		}

		#endregion

		#region Implementation

		TestTypeResolutionService TypeResolutionService
		{
			get
			{
				if (typeResolutionService == null)
				{
					typeResolutionService = new TestTypeResolutionService(ServiceProvider, DotNetTypeResolutionService);
				}
				return typeResolutionService;
			}
		}
		TestTypeResolutionService typeResolutionService;

		MockDotNetTypeResolutionService DotNetTypeResolutionService
		{
			get
			{
				if (dotNetTypeResolutionService == null)
				{
					dotNetTypeResolutionService = new MockDotNetTypeResolutionService();
				}
				return dotNetTypeResolutionService;
			}
		}
		MockDotNetTypeResolutionService dotNetTypeResolutionService;

		MockServiceProvider ServiceProvider
		{
			get
			{
				if (serviceProvider == null)
				{
					serviceProvider = new MockServiceProvider(this);
				}
				return serviceProvider;
			}
		}
		MockServiceProvider serviceProvider;

		readonly MockDynamicTypeService DynamicTypeService = new MockDynamicTypeService();

		#endregion
	}
}
#endif
