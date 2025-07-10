using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class LoginPasswordListEditControlForRegistryForTest : LoginPasswordListEditControlForRegistry
	{
		public LoginPasswordListEditControlForRegistryForTest()
			: base(new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System), "Login", "Password", 50)
		{
		}

		protected override bool CheckValidDeveloperPassword()
		{
			return IsValidPasswordForTest;
		}

		public bool IsValidPasswordForTest { get; set; }
	}
}
