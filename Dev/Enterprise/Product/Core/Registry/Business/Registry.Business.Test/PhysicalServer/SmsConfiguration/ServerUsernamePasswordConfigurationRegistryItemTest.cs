using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ServerUsernamePasswordConfigurationRegistryItem))]
	sealed class ServerUsernamePasswordConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ServerUsernamePasswordConfiguration>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<ServerUsernamePasswordConfiguration, ServerUsernamePasswordConfiguration> GetNewRegistryItem()
		{
			return new ServerUsernamePasswordConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override ServerUsernamePasswordConfiguration ValidValue
		{
			get
			{
				ServerUsernamePasswordConfiguration result = new ServerUsernamePasswordConfiguration();
				result.UserName = "geoffuser";
				result.Password = "geoffpass";
				result.ConfirmPassword = "geoffpass";

				return result;
			}
		}

		#endregion
	}
}
