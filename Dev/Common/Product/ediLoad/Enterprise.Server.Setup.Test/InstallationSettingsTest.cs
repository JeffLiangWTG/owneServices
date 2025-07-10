using System.Collections.Generic;
using CargoWise.Loader.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	class InstallationSettingsTest : TestCase
	{
		InstallationSettings settings;

		protected override void SetUp()
		{
			base.SetUp();
			settings = new InstallationSettings(new MockConfiguration(@"X:\Source", @"X:\Company\Target"));
		}

		public void TestCommandLineProperties()
		{
			foreach (KeyValuePair<string, bool> property in settings.CommandLineProperties)
			{
				settings.SetPropertyValue(property.Key, "x");
				AssertEquals("x", settings.GetPropertyValue(property.Key));
				settings.SetPropertyValue(property.Key, "y");
				AssertEquals("y", settings.GetPropertyValue(property.Key));
				if (property.Value)
				{
					string useDefaultPropertyName = InstallationSettings.UseDefaultPropertyPrefix + property.Key;
					settings.SetPropertyValue(useDefaultPropertyName, false);
					AssertEquals(false, settings.GetPropertyValue(useDefaultPropertyName));
					settings.SetPropertyValue(useDefaultPropertyName, true);
					AssertEquals(true, settings.GetPropertyValue(useDefaultPropertyName));
				}
			}
		}

		public void TestLicenceNineCode()
		{
			settings.LicenseCode = "ENTSRV";
			AssertEquals("ENT___SRV", settings.LicenseNineCode);

			settings.LicenseCode = "FOO";
			AssertEquals("FOO", settings.LicenseNineCode);
		}

		public void TestDefaultValues()
		{
			AssertEquals("CDPath", @"X:\Source", settings.CDPath);
			AssertEquals("DataPath", @"C:\Program Files\WiseTech Global\Databases\Data", settings.DataPath);
			AssertEquals("LogPath", @"C:\Program Files\WiseTech Global\Databases\Log", settings.LogPath);

			AssertEquals("UseDefaultDbName", true, settings.UseDefaultDbName);

			AssertEquals("DbName", "CargoWiseOne", settings.DbName);
		}

		public void TestDbNameStripsQuotes()
		{
			settings.DbName = "' select me'";
			AssertEquals("DbName", "select me", settings.DbName);
		}

		public void TestUseDefaultProperties()
		{
			settings.UseDefaultDbName = false;
			settings.DbName = "c";
			AssertEquals("DbName", "c", settings.DbName);
			settings.UseDefaultDbName = true;
			AssertEquals("DbName", "CargoWiseOne", settings.DbName);

			settings.LicenseCode = "AAACCC";
			AssertEquals("DbName", "CargoWiseOneAAACCC", settings.DbName);

			settings.UseDefaultDbName = false;
			settings.DbName = "h";
			AssertEquals("DbName", "h", settings.DbName);
		}
	}
}
