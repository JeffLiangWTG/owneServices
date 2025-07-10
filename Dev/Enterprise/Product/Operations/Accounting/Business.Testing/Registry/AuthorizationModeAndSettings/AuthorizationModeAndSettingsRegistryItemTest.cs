using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AuthorizationModeAndSettingsRegistryItem))]
	public class AuthorizationModeAndSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<AuthorizationModeAndSettings>
	{
		protected override StronglyTypedRegistryItem<AuthorizationModeAndSettings, AuthorizationModeAndSettings> GetNewRegistryItem()
		{
			return new AuthorizationModeAndSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
