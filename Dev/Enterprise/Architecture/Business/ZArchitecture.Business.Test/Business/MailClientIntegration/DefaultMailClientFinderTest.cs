using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.MailClientIntegration.Testing
{
	sealed class DefaultMailClientFinderTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the registry.")]
		[ExpectNoExceptions]
		public void TestGetDefaultMailClient()
		{
			string currentMailToClientPath = DefaultMailClientFinder.DefaultMailClientPath;

			try
			{
				if (SetDefaultMailClient(MailClientType.Outlook))
				{
					AssertEquals("Should have set Outlook", MailClientType.Outlook, DefaultMailClientFinder.DefaultMailClient);
				}

				if (SetDefaultMailClient(MailClientType.Unknown))
				{
					AssertEquals("Should have set Unknown", MailClientType.Unknown, DefaultMailClientFinder.DefaultMailClient);
				}

				if (SetDefaultMailClient(MailClientType.LotusNotes))
				{
					AssertEquals("Should have set LotusNotes", MailClientType.LotusNotes, DefaultMailClientFinder.DefaultMailClient);
				}
			}
			finally
			{
				SetDefaultMailClient(currentMailToClientPath);
			}
		}

		bool SetDefaultMailClient(MailClientType mailClient)
		{
			string mailToCommand = "some unknown mail client";
			if (mailClient == MailClientType.Outlook)
			{
				mailToCommand = "office.outlook";
			}
			else if (mailClient == MailClientType.LotusNotes)
			{
				mailToCommand = "Lotus Notes";
			}
			return SetDefaultMailClient(mailToCommand);
		}

		bool SetDefaultMailClient(string mailClientPath)
		{
			RegistryKey mailtoKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("mailto");
			RegistryKey shellKey = null;
			if (mailtoKey != null)
			{
				shellKey = mailtoKey.CreateSubKey("Shell");
			}
			RegistryKey openKey = null;
			if (shellKey != null)
			{
				openKey = shellKey.CreateSubKey("Open");
			}
			RegistryKey commandKey = null;
			if (openKey != null)
			{
				commandKey = openKey.CreateSubKey("Command");
			}
			if (commandKey != null)
			{
				commandKey.SetValue("", mailClientPath);
				return true;
			}
			return false;
		}
	}
}
