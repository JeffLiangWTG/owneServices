using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ServerUsernamePasswordConfiguration))]
	sealed class ServerUsernamePasswordConfigurationTest : RegistryBusinessObjectTemplateTestCase<ServerUsernamePasswordConfiguration>
	{
		#region TestUserNameValidation

		public void TestUserNameValidation()
		{
			AssertMandatoryValidation(Config.UserNameInfo);
		}

		#endregion

		#region TestPasswordValidation

		public void TestPasswordValidation()
		{
			AssertMandatoryValidation(Config.PasswordInfo);
		}

		#endregion

		#region TestConfirmPasswordValidation

		public void TestConfirmPasswordValidation()
		{
			AssertMandatoryValidation(Config.ConfirmPasswordInfo);

			ZString passwordMismatchError = "The passwords you typed do not match. Type the same password into both text boxes.";

			Config.Password = "";
			Config.ConfirmPassword = "";
			AssertNoError(Config.ConfirmPasswordInfo, passwordMismatchError);

			Config.Password = "sega";
			Config.ConfirmPassword = "";
			AssertNoError(Config.ConfirmPasswordInfo, passwordMismatchError);

			Config.Password = "";
			Config.ConfirmPassword = "nintendo";
			AssertNoError(Config.ConfirmPasswordInfo, passwordMismatchError);

			Config.Password = "sega";
			Config.ConfirmPassword = "nintendo";
			AssertHasError(Config.ConfirmPasswordInfo, passwordMismatchError);

			Config.Password = "nintendo";
			Config.ConfirmPassword = "nintendo";
			AssertNoError(Config.ConfirmPasswordInfo, passwordMismatchError);
		}

		#endregion

		#region TestSettingPasswordClearsConfirmPassword

		public void TestSettingPasswordClearsConfirmPassword()
		{
			Config.ConfirmPassword = "paradigm";
			AssertEquals(false, Config.ConfirmPassword.IsEmpty);

			Config.Password = "denon";
			AssertEquals(true, Config.ConfirmPassword.IsEmpty);
			AssertNoError(Config.ConfirmPasswordInfo, "Please enter a value.");

			// this is stupid, it is only here because of the shitty test that forces
			// you to check for error if you have previously checked for no error.
			Config.ConfirmPassword = "abc";
			Config.ConfirmPassword = "";
			AssertHasError(Config.ConfirmPasswordInfo, "Please enter a value.");
		}

		#endregion

		public void TestServerNameAndUserNameWithoutServer()
		{
			ServerUsernamePasswordConfiguration config = new ServerUsernamePasswordConfiguration();
			AssertEquals("", config.ServerName);
			AssertEquals("", config.UserNameWithoutServer);

			config.UserName = "megan.fox";
			AssertEquals("", config.ServerName);
			AssertEquals("megan.fox", config.UserNameWithoutServer);

			config.UserName = "corporate.cargowise.com\\megan.fox";
			AssertEquals("corporate.cargowise.com", config.ServerName);
			AssertEquals("megan.fox", config.UserNameWithoutServer);
		}

		void AssertMandatoryValidation(ZPropertyInfo propertyToTestForMandatoryValidation)
		{
			propertyToTestForMandatoryValidation.Value = (ZString)"wii";
			AssertNoError(propertyToTestForMandatoryValidation, "Please enter a value.");

			propertyToTestForMandatoryValidation.Value = ZString.Empty;
			AssertHasError(propertyToTestForMandatoryValidation, "Please enter a value.");

			// ensures notifications are cleared
			propertyToTestForMandatoryValidation.Value = (ZString)"wii";
			AssertNoError(propertyToTestForMandatoryValidation, "Please enter a value.");
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ServerUsernamePasswordConfiguration GetBusinessObjectToClone()
		{
			ServerUsernamePasswordConfiguration result = new ServerUsernamePasswordConfiguration();

			result.UserName = "geoffuser";
			result.Password = "geoffpass";
			result.ConfirmPassword = "geoffpass";

			return result;
		}

		protected override ServerUsernamePasswordConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		ServerUsernamePasswordConfiguration Config
		{
			get
			{
				if (fConfig == null)
				{
					fConfig = (ServerUsernamePasswordConfiguration)GetNewBusinessObject();
				}
				return fConfig;
			}
		}

		ServerUsernamePasswordConfiguration fConfig;

		#endregion
	}
}
