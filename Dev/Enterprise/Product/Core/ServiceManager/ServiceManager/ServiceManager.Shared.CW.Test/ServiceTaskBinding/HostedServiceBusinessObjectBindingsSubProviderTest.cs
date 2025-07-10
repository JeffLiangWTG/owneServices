using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Shared.Testing
{
	[TestsSubclassesOf(typeof(IHostedServiceBusinessObjectBindingsSubProvider))]
	public abstract class HostedServiceBusinessObjectBindingsSubProviderTest<TSubProvider> : TestCase where TSubProvider : IHostedServiceBusinessObjectBindingsSubProvider
	{
		public void TestBusinessObjectBindings()
		{
			var bindings = CreateSubProvider().BusinessObjectBindings?.ToArray() ?? Array.Empty<IHostedServiceBusinessObjectBinding>();
			AssertEquals("Binding Count", ExpectedBindingsCount, bindings.Length);
		}

		public void TestHostedBusinessObjectBindingType()
		{
			var bindings = CreateSubProvider().BusinessObjectBindings?.ToArray();
			AssertType(ExpectedBindingItemType, bindings?.FirstOrDefault());
		}

		public void TestIsRegistered()
		{
			var subProvider = ObjectFactory.Get<TSubProvider>();
			AssertNotNull(subProvider);
		}

		public void TestIsRegistered_InListOfSubProviders()
		{
			var subProviders = ObjectFactory.Get<IEnumerable>("HostedServiceBusinessObjectBindingsSubProviders")
				.Cast<IHostedServiceBusinessObjectBindingsSubProvider>();
			Assert($"{typeof(TSubProvider).Name} should be registered", subProviders.Any(b => b is TSubProvider));
		}

		protected virtual IHostedServiceBusinessObjectBindingsSubProvider CreateSubProvider() => (IHostedServiceBusinessObjectBindingsSubProvider)Activator.CreateInstance(typeof(TSubProvider));

		protected abstract int ExpectedBindingsCount { get; }

		protected abstract Type ExpectedBindingItemType { get; }
	}
}
