using System;
using Microsoft.Win32;

namespace Enterprise.ZArchitecture.Business.MailClientIntegration
{
	public enum MailClientType
	{
		Outlook,
		LotusNotes,
		Unknown
	}

	public static class DefaultMailClientFinder
	{
		public static MailClientType DefaultMailClient
		{
			get
			{
				MailClientType result = MailClientType.Unknown;

				string mailToCommand = DefaultMailClientPath;

				if (mailToCommand.IndexOf("office", StringComparison.InvariantCultureIgnoreCase) != -1 &&
					mailToCommand.IndexOf("outlook", StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					result = MailClientType.Outlook;
				}
				else if (mailToCommand.IndexOf("notes", StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					result = MailClientType.LotusNotes;
				}

				return result;
			}
		}

		public static string DefaultMailClientPath
		{
			get
			{
				string mailToCommand = "";

				RegistryKey mailtoKey = Registry.ClassesRoot.OpenSubKey("mailto"); // May be an identifier or GUID.
				RegistryKey shellKey = null;
				if (mailtoKey != null)
				{
					shellKey = mailtoKey.OpenSubKey("Shell"); // May be an identifier or GUID.
				}
				RegistryKey openKey = null;
				if (shellKey != null)
				{
					openKey = shellKey.OpenSubKey("Open"); // May be an identifier or GUID.
				}
				RegistryKey commandKey = null;
				if (openKey != null)
				{
					commandKey = openKey.OpenSubKey("Command"); // May be an identifier or GUID.
				}
				if (commandKey != null)
				{
					mailToCommand = (string)commandKey.GetValue("");
					if (mailToCommand == null)
					{
						mailToCommand = "";
					}
				}

				return mailToCommand;
			}
		}
	}
}
