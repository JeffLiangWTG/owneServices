using CargoWise.Loader.Common;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	class RemoveUserSpecificEdiUrlRegistrationTest : TestCase
	{
		public void TestRemoveUserSpecificEdiUrlRegistration()
		{
			using (var iconKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\EdiEnterprise.edient\DefaultIcon"))
			{
				iconKey.SetValue(null, "bla");
			}
			using (var commandKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\EdiEnterprise.edient\shell\open\command"))
			{
				commandKey.SetValue(null, "bla");
			}
			using (var iconKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\edient\DefaultIcon"))
			{
				iconKey.SetValue(null, "bla");
			}
			using (var commandKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\edient\shell\open\command"))
			{
				commandKey.SetValue(null, "bla");
			}

			var item = new RemoveUserSpecificEdiUrlRegistration(null);
			AssertEquals(true, item.NeedsToInstall());
			item.Install(new InstallationResultCollection());

			using (var iconKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\EdiEnterprise.edient\DefaultIcon"))
			{
				AssertNull(iconKey.GetValue(null));
			}
			using (var commandKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\EdiEnterprise.edient\shell\open\command"))
			{
				AssertNull(commandKey.GetValue(null));
			}
			using (var iconKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\edient\DefaultIcon"))
			{
				AssertNull(iconKey.GetValue(null));
			}
			using (var commandKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\edient\shell\open\command"))
			{
				AssertNull(commandKey.GetValue(null));
			}

			item = new RemoveUserSpecificEdiUrlRegistration(null);
			item.Install(new InstallationResultCollection());

			using (var iconKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\EdiEnterprise.edient\DefaultIcon"))
			{
				AssertNull(iconKey.GetValue(null));
			}
			using (var commandKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\EdiEnterprise.edient\shell\open\command"))
			{
				AssertNull(commandKey.GetValue(null));
			}
			using (var iconKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\edient\DefaultIcon"))
			{
				AssertNull(iconKey.GetValue(null));
			}
			using (var commandKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\edient\shell\open\command"))
			{
				AssertNull(commandKey.GetValue(null));
			}
		}
	}
}