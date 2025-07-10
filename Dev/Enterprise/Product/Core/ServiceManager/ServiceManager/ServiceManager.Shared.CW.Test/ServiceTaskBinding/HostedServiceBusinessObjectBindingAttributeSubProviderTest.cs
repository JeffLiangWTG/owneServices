using System;
using System.Linq;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	[TestedType(typeof(HostedServiceBusinessObjectBindingAttributeSubProvider))]
	sealed class HostedServiceBusinessObjectBindingAttributeSubProviderTest : HostedServiceBusinessObjectBindingsSubProviderTest<HostedServiceBusinessObjectBindingAttributeSubProvider>
	{
		protected override int ExpectedBindingsCount => AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().ToArray().Length;
		protected override Type ExpectedBindingItemType => typeof(HostedServiceBusinessObjectBindingAttribute);
	}
}
