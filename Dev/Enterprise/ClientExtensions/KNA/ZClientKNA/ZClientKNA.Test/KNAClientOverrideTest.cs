using System;
using Enterprise.Client.KNA;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class KNAClientOverrideTest : ClientOverrideTest
	{
		public void TestAdditionalUserVisibleRegistryItems()
		{
			AssertEquals(KNADataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		public new void TestNotPuttingClassesInWrongNamespace()
		{
			Assert("Purposefully used Customs and Masterfile namespaces to allow for future integration into core code.", true);
		}

		public void TestModuleOverrides()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertNotNull(clientOverride.ModuleOverrides);
		}
	}
}
