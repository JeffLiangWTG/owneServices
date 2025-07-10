using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.Common;
using Moq;

namespace CargoWise.IO.Testing
{
	public static class FileSystemAccessControlUtilsTestHelper
	{
		public static void CreateFolderWithBlockedPermissionsForTest(string directory)
		{
			DeleteFolderIfExists(directory);

			// remove all user-specific permissions on the folder, and leave FullControl on "Everyone"
			// when FileSystem.IsDirectoryWritable checks if the current user is in "Everyone", an exception is thrown in this test
			// it is a bit of a hack because at the end of the test, the current user needs to be able to delete the folder
			MockEveryoneRoleForIsDirectoryWritable();

			var testDirectory = Directory.CreateDirectory(directory);
			var testDirectorySecurity = testDirectory.GetAccessControl();
			testDirectorySecurity.GetAccessRules(true, true, typeof(NTAccount)).OfType<FileSystemAccessRule>().ForEach(rule => { testDirectorySecurity.RemoveAccessRule(rule); });
			testDirectorySecurity.SetAccessRuleProtection(true, false);
			var accessRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
			testDirectorySecurity.AddAccessRule(accessRule);
			testDirectory.SetAccessControl(testDirectorySecurity);
		}

		static void MockEveryoneRoleForIsDirectoryWritable()
		{
			var principal = new Mock<WindowsPrincipal>(WindowsIdentity.GetCurrent());
			principal.Setup(p => p.IsInRole("Everyone")).Throws(new SystemException("Exception for Unit Test"));
			FileSystemAccessControlUtils.CurrentPrincipal.Value = principal.Object;
		}

		public static void DeleteFolderIfExists(string directory)
		{
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
			}
		}
	}
}
