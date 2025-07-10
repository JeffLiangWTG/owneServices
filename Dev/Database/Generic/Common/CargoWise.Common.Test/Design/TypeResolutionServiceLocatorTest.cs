using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Common.Design
{
	class TypeResolutionServiceLocatorTest : TestCase
	{
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestGet()
		{
			MockServiceProvider serviceProvider = new MockServiceProvider();
			serviceProvider.AddService(TypeResolutionServiceLocator.DynamicTypeServiceType, DynamicTypeService);
			AssertEquals("ITypeResolutionService comes from DynamicTypeService.ActiveResolver", typeof(Testing.MockTypeResolutionService), TypeResolutionServiceLocator.Get(serviceProvider).GetType());
		}

		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestGetDynamicTypeService()
		{
			MockServiceProvider serviceProvider = new MockServiceProvider();
			serviceProvider.AddService(TypeResolutionServiceLocator.DynamicTypeServiceType, new Testing.MockDynamicTypeService());
			AssertEquals("ITypeResolutionService comes from DynamicTypeService.ActiveResolver", typeof(Testing.MockDynamicTypeService), TypeResolutionServiceLocator.GetDynamicTypeService(serviceProvider).GetType());
		}

		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestDynamicTypeServiceType()
		{
			AssertEquals("DynamicTypeService", TypeResolutionServiceLocator.DynamicTypeServiceType.Name);
		}

		#region Test Classes
		class MockServiceProvider : IServiceProvider
		{
			public void AddService(Type serviceType, object service)
			{
				services.Add(serviceType, service);
			}

			public object GetService(Type serviceType)
			{
				object result = null;
				services.TryGetValue(serviceType, out result);
				return result;
			}

			readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
		}

		#endregion
		#region Implementation
		Testing.MockDynamicTypeService DynamicTypeService
		{
			get
			{
				if (dynamicTypeService == null)
				{
					dynamicTypeService = new Testing.MockDynamicTypeService(new Testing.MockTypeResolutionService(typeof(int).Assembly));
				}

				return dynamicTypeService;
			}
		}

		Testing.MockDynamicTypeService dynamicTypeService;
		#endregion
	}
}