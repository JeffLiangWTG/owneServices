using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpProfile))]
	sealed class FtpProfileRegoTest : RegistryBusinessObjectTemplateTestCase<FtpProfile>
	{
		FtpProfile Profile
		{
			get
			{
				if (profile == null)
				{
					profile = new FtpProfile(Factory);
				}
				return profile;
			}
		}
		FtpProfile profile;

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override FtpProfile GetBusinessObjectToClone()
		{
			return new FtpProfile(Factory);
		}

		protected override FtpProfile GetBusinessObjectToSerialise()
		{
			Profile.FriendlyName = "ABC";
			Profile.PushOrPull = "PUS";
			return Profile;
		}
	}
}
