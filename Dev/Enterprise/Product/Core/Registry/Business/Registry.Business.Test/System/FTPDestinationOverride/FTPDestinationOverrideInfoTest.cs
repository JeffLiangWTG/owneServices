using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FTPDestinationOverrideInfo))]
	sealed class FTPDestinationOverrideInfoTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			var ftpDestinationOverride = new FTPDestinationOverrideInfo();
			AssertEquals("Has errors?", false, ftpDestinationOverride.HasErrors);

			ftpDestinationOverride.FtpAddress = "SomeFtpAddress";
			AssertEquals("Has errors?", false, ftpDestinationOverride.HasErrors);

			ftpDestinationOverride.UserName = "SomeUserName";
			AssertEquals("Has errors?", false, ftpDestinationOverride.HasErrors);

			ftpDestinationOverride.Password = "SomePassword";
			AssertEquals("Has errors?", false, ftpDestinationOverride.HasErrors);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new FTPDestinationOverrideInfo();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
