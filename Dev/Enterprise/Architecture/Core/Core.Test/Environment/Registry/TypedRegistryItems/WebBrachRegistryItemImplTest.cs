using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class WebBrachRegistryItemImplTest : TransactionedTestCase
	{
		public void TestDefaultValue()
		{
			var registryItem = new WebBrachRegistryItemImpl("", (NoResString)"", (NoResString)"", (NoResString)"",
				new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				EnvProxy.GetAnyBranchWithWebAddress(new BusinessObjectFactory()));

			AssertEquals(registryItem.DefaultValue, registryItem.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
