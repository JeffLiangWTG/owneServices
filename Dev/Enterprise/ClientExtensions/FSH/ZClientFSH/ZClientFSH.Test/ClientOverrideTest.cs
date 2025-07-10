using System;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInstance()
		{
			AssertEquals("Incorrect instance type", typeof(ClientOverride), ClientOverride.Instance.GetType());
		}

		public void TestClient()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Client type is supposed to be FSH", typeof(Clients), overrideForClient.Client.GetType());
		}

		public void TestClientDisplayName()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("Incorrect client display name", "Fortune Shipping", overrideForClient.ClientDisplayName);
		}

		public void TestHelpWebPage()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertEquals("HelpWebPage is supposed to be \"\"", "", overrideForClient.HelpWebPage);
		}

		public void TestDbSchemaUpgradeInfo()
		{
			ClientOverride overrideForClient = ClientOverride.Instance;
			AssertNotNull(overrideForClient.DbSchemaExtensionObjects);
			AssertEquals(typeof(ExtensionObjects), overrideForClient.DbSchemaExtensionObjects.GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
