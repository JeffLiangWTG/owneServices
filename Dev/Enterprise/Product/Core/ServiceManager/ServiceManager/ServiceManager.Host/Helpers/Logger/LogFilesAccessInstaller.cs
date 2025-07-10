using System.Collections;
using System.Configuration.Install;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Enterprise.ServiceManager.Host
{
	public class LogFilesAccessInstaller : Installer
	{
		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			stateSaver["logFilesSid"] = Sid.Value;
			GrantAccess();
		}

		public override void Rollback(IDictionary savedState)
		{
			base.Rollback(savedState);

			RevokeAccess();
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			if (savedState != null && savedState.Contains("logFilesSid"))
			{
				Sid = new SecurityIdentifier((string)savedState["logFilesSid"]);
			}
			RevokeAccess();
		}

		void GrantAccess()
		{
			if (!Directory.Exists(LogDirectoryName))
			{
				Directory.CreateDirectory(LogDirectoryName);
			}

			var logDirectoryInfo = new DirectoryInfo(LogDirectoryName);

			var security = logDirectoryInfo.GetAccessControl();
			var rule = new FileSystemAccessRule(Sid,
				FileSystemRights.Modify,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.None,
				AccessControlType.Allow);
			security.AddAccessRule(rule);

			logDirectoryInfo.SetAccessControl(security);
		}

		void RevokeAccess()
		{
			if (Directory.Exists(LogDirectoryName))
			{
				if (Directory.GetFileSystemEntries(LogDirectoryName).Length > 0)
				{
					if (Sid != null)
					{
						var logDirectoryInfo = new DirectoryInfo(LogDirectoryName);

						var security = logDirectoryInfo.GetAccessControl();
						var rules = security.GetAccessRules(true, false, typeof(SecurityIdentifier));
						for (var i = rules.Count - 1; i >= 0; i--)
						{
							if (Sid.Equals((SecurityIdentifier)rules[i].IdentityReference))
							{
								security.RemoveAccessRule((FileSystemAccessRule)rules[i]);
							}
						}
						logDirectoryInfo.SetAccessControl(security);
					}
				}
				else
				{
					Directory.Delete(LogDirectoryName);
				}
			}
		}

		public SecurityIdentifier Sid
		{
			get { return sid; }
			set { sid = value; }
		}

		SecurityIdentifier sid;

		public string LogDirectoryName
		{
			get { return logDirectoryName; }
			set { logDirectoryName = value; }
		}

		string logDirectoryName;
	}
}
