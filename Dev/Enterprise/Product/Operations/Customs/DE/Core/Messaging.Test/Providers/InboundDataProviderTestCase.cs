using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	[TestsSubclassesOf(typeof(IInboundProvider))]
	public abstract class InboundDataProviderTestCase<T, TTestClass> : TestCaseWithFactory
		where TTestClass : class
	{
		[ExpectNoExceptions]
		public void TestForAllInterfacePropertiesExistsATestCase()
		{
			var existingTestMethods = GetType().GetMethods().AsParallel().Where(methodInfo => methodInfo.Name.StartsWith("Test")).Select(methodInfo => methodInfo.Name).ToArray();
			var interfaceType = typeof(T);
			var propertyNames = new List<string>();
			CombineAssertions(() =>
			{
				AssertPropertiesExist(interfaceType);
				foreach (var inhertitedInterface in interfaceType.GetInterfaces())
				{
					AssertPropertiesExist(inhertitedInterface);
				}
			});

			void AssertPropertiesExist(Type typeOfInterface)
			{
				foreach (var propertyInfo in typeOfInterface.GetRuntimeProperties())
				{
					NUnit.Framework.Assert.That(existingTestMethods, Has.Some.EqualTo("Test" + propertyInfo.Name), $"Testclass should contain a test for property {propertyInfo.Name} named \"Test{propertyInfo.Name}\"");
				}
			}
		}

		public void TestEnumerablesShouldBeCached()
		{
			if (EnumerableProperties.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var propertyInfo in EnumerableProperties)
					{
						AssertSame($"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name} should be cached.", propertyInfo.GetValue(Provider), propertyInfo.GetValue(Provider));
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestEnumerablesShouldBeInstanceOfCollection()
		{
			if (EnumerableProperties.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var propertyInfo in EnumerableProperties)
					{
						NUnit.Framework.Assert.That(propertyInfo.GetValue(Provider) is ICollection, Is.True, $"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name} should return an instance of ICollection so that it won't be evaluated multiple times.");
					}
				});
			}
			else
			{
				Assert(true);
			}
		}
		protected abstract TTestClass GetProvider();

		TTestClass Provider => provider ?? (provider = GetProvider());
		TTestClass provider;

		List<PropertyInfo> EnumerableProperties => enumerableProperties ?? (enumerableProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(x.PropertyType)).ToList());
		List<PropertyInfo> enumerableProperties;
	}
}
