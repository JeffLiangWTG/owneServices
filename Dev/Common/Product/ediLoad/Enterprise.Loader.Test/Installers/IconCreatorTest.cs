using System;
using System.IO;
using System.Text;
using CargoWise.ApplicationManager.Common;
using CargoWise.IO;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Native;
using CargoWise.Loader.Common.Testing;
using Enterprise.Client.Common;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	[TestRequiresAdministrativePrivileges("This test modifies the registry.")]
	class IconCreatorTest : TestCase
	{
		MockEnterpriseConfiguration configuration;
		MockIconCreator creator;
		MockNativeMethods mockNativeMethods;
		MockClientSetupHelper setupHelper;
		const string TestProgramsSubFolder = @"\WiseTech Global";

		protected override void SetUp()
		{
			MockIconCreationDialog.ShowDialogCount = 0;
			MockIconCreationDialog.MockIconCreationOptions = null;

			mockNativeMethods = new MockNativeMethods();
			MockServiceContainer services = new MockServiceContainer();
			services.PopulateWithRealServices();
			services.NativeMethods = mockNativeMethods;

			configuration = new MockEnterpriseConfiguration();
			configuration.ServerName = "myserver";
			configuration.DatabaseName = "mydb";
			configuration.Services = services;

			setupHelper = new MockClientSetupHelper();
			creator = new MockIconCreator(new ClientInstallation(configuration), typeof(MockIconCreationDialog), setupHelper);
			configuration.SetAppManagerClient(new IconCreatorAppManager(creator));

			setupHelper.ClearRegistry();
		}

		protected override void TearDown()
		{
			setupHelper.ClearRegistry();
			mockNativeMethods.Dispose();
			base.TearDown();
		}

		void SetMock(bool desktop, bool programs, bool allUsers)
		{
			MockIconCreationDialog.MockIconCreationOptions = new IconCreationOptions();
			MockIconCreationDialog.MockIconCreationOptions.Desktop = desktop;
			MockIconCreationDialog.MockIconCreationOptions.Programs = programs;
			MockIconCreationDialog.MockIconCreationOptions.AllUsers = allUsers;
			MockIconCreationDialog.MockIconCreationOptions.InstanceDescription = "mydb";
		}

		void AssertShowDialogCalls(int count)
		{
			AssertEquals(count, MockIconCreationDialog.ShowDialogCount);
		}

		public void TestInstallIfInstanceRegistrySubKeyDoesNotExist()
		{
			setupHelper.ClearRegistry();
			Assert(creator.NeedsToInstall());
		}

		public void TestHaveNotAskedAboutIcons()
		{
			Assert("Test setup has cleared the registry, so we should not have asked", creator.NeedsToInstall());
		}

		public void TestAlreadyAskedThisUserAboutIcons()
		{
			string subKeyName = setupHelper.KeyName + @"\myserver mydb";
			using (RegistryKey machineKey = Registry.LocalMachine.CreateSubKey(subKeyName))
			{
				using (RegistryKey userKey = Registry.CurrentUser.CreateSubKey(subKeyName))
				{
					userKey.SetValue(ClientSetupHelper.ShortcutsInstalledValueName, 1);
				}
			}
			Assert(!creator.NeedsToInstall());
			AssertShowDialogCalls(0);
		}

		public void TestAlreadyAskedAllUsersAboutIcons()
		{
			using (RegistryKey key = Registry.LocalMachine.CreateSubKey(setupHelper.KeyName + @"\myserver mydb"))
			{
				key.SetValue(ClientSetupHelper.ShortcutsInstalledValueName, 1);
			}
			Assert(!creator.NeedsToInstall());
			AssertShowDialogCalls(0);
		}

		public void TestCreateInUserPrograms()
		{
			SetMock(false, true, false);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(true, false, false, false);
		}

		public void TestCreateInAllUsersPrograms()
		{
			SetMock(false, true, true);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(false, false, true, false);
		}

		public void TestCreateOnUserDesktop()
		{
			SetMock(true, false, false);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(false, true, false, false);
		}

		public void TestCreateOnAllUsersDesktop()
		{
			SetMock(true, false, true);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(false, false, false, true);
		}

		public void TestCreateOnUserBoth()
		{
			SetMock(true, true, false);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(true, true, false, false);
		}

		public void TestCreateOnAllUsersBoth()
		{
			SetMock(true, true, true);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(false, false, true, true);
		}

		public void TestDoNotCreate()
		{
			SetMock(false, false, false);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(false, false, false, false);
		}

		public void TestDoNotRunWithNoUI()
		{
			configuration.UILevel = UILevel.AutomatedWithNoUI;
			Assert("Should not run when no UI", !creator.NeedsToInstall());
		}

		public void TestDoNotRunInRemoteAppSession()
		{
			setupHelper.ClearRegistry();
			creator.EnableRemoteAppSession();
			Assert(!creator.NeedsToInstall());
		}

		public void TestMockWithInstance()
		{
			configuration.InstanceName = "myinstance";
			SetMock(true, false, false);
			Assert(creator.InstallExcludingDependencies().IsOK);
			AssertShowDialogCalls(1);
			AssertIcons(false, true, false, false, "-Instance:myinstance", "-Instance:myinstance");
		}

		void AssertIcon(bool expected, string folder, int csidl, RegistryKey key, string arguments)
		{
			var options = new IconCreationOptions() { InstanceDescription = "mydb" };
			string iconPath = Path.Combine(folder, creator.GetIconName(options));
			AssertEquals("Does the icon exist?", expected, File.Exists(iconPath));
			if (expected)
			{
				string[] registryValue = (string[])key.GetValue(ClientSetupHelper.ShortcutPathsValueName);
				Assert(Array.IndexOf(registryValue, iconPath) != -1);
				ShellShortcut shortcut = new ShellShortcut(iconPath);
				AssertEquals("Path", Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise", "CargoWise.Start.exe"), shortcut.Path);
				AssertEquals("Arguments", arguments, shortcut.Arguments);
			}
		}

		void AssertIcons(bool inPrograms, bool onDesktop, bool inCommonPrograms, bool onCommonDesktop, string subkey = "myserver mydb", string arguments = "myserver mydb")
		{
			using (RegistryKey machineKey = Registry.LocalMachine.OpenSubKey(setupHelper.KeyName + '\\' + subkey))
			using (RegistryKey userKey = Registry.CurrentUser.OpenSubKey(setupHelper.KeyName + '\\' + subkey))
			{
				if (inCommonPrograms || onCommonDesktop)
				{
					AssertEquals(1, machineKey.GetValue(ClientSetupHelper.ShortcutsInstalledValueName));
				}
				else if (inPrograms || onDesktop)
				{
					AssertEquals(1, userKey.GetValue(ClientSetupHelper.ShortcutsInstalledValueName));
				}
				AssertIcon(inPrograms, mockNativeMethods.ProgramsDirectory + TestProgramsSubFolder, NativeMethods.CSIDL_PROGRAMS, userKey, arguments);
				AssertIcon(onDesktop, mockNativeMethods.DesktopDirectory, NativeMethods.CSIDL_DESKTOPDIRECTORY, userKey, arguments);
				AssertIcon(inCommonPrograms, mockNativeMethods.CommonProgramsDirectory + TestProgramsSubFolder, NativeMethods.CSIDL_COMMON_PROGRAMS, machineKey, arguments);
				AssertIcon(onCommonDesktop, mockNativeMethods.CommonDesktopDirectory, NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY, machineKey, arguments);
			}
		}

		#region IconCreatorAppManager

		class IconCreatorAppManager : MockAppManager
		{
			readonly MockIconCreator host;

			public IconCreatorAppManager(MockIconCreator host)
			{
				this.host = host;
			}

			public override AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
			{
				return ((IAppManagerInvocable)host).Invoke(false, state);
			}
		}

		#endregion

		#region MockIconCreationDialog

		class MockIconCreationDialog : IIconCreationDialog
		{
			public void ShowDialog()
			{
				ShowDialogCount++;
			}

			public static int ShowDialogCount { get; set; }
			public static IconCreationOptions MockIconCreationOptions { get; set; }

			public IconCreationOptions IconCreationOptions
			{
				get
				{
					if (ShowDialogCount > 0)
					{
						return MockIconCreationOptions;
					}
					else
					{
						throw new InvalidOperationException("Tried to access icon creation options without showing the dialog");
					}
				}
			}

			public void SetInstanceDescription(string description, bool isReadonly)
			{
			}

			public void Dispose()
			{
			}
		}

		#endregion

		#region MockIconCreator

		class MockIconCreator : IconCreator
		{
			readonly MockClientSetupHelper setupHelper;

			public MockIconCreator(ClientInstallation installation, Type typeOfIconCreationDialog, MockClientSetupHelper setupHelper)
				: base(installation, typeOfIconCreationDialog)
			{
				this.setupHelper = setupHelper;
			}

			protected override ClientSetupHelper GetNewSetupHelper()
			{
				return setupHelper;
			}

			public new InstallationResult InstallExcludingDependencies()
			{
				return base.InstallExcludingDependencies();
			}

			protected override bool IsRemoteAppSession => isRemoteAppSession;
			bool isRemoteAppSession;

			public void EnableRemoteAppSession() => isRemoteAppSession = true;
		}

		class MockClientSetupHelper : ClientSetupHelper
		{
			public override string KeyName
			{
				get { return @"SOFTWARE\WiseTech Global\CargoWise One (Test)"; }
			}

			public void ClearRegistry()
			{
				foreach (RegistryKey rootKey in new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser })
				{
					using (RegistryKey subKey = rootKey.OpenSubKey(KeyName))
					{
						if (subKey != null)
						{
							rootKey.DeleteSubKeyTree(KeyName);
						}
					}
				}
			}
		}

		#endregion

		#region MockNativeMethods

		class MockNativeMethods : INativeMethods, IDisposable
		{
			TempDirectory commonDesktopDirectory;
			TempDirectory commonProgramsDirectory;
			TempDirectory desktopDirectory;
			TempDirectory programsDirectory;

			public TempDirectory CommonDesktopDirectory
			{
				get { return commonDesktopDirectory ?? (commonDesktopDirectory = new TempDirectory()); }
			}

			public TempDirectory CommonProgramsDirectory
			{
				get { return commonProgramsDirectory ?? (commonProgramsDirectory = new TempDirectory()); }
			}

			public TempDirectory DesktopDirectory
			{
				get { return desktopDirectory ?? (desktopDirectory = new TempDirectory()); }
			}

			public TempDirectory ProgramsDirectory
			{
				get { return programsDirectory ?? (programsDirectory = new TempDirectory()); }
			}

			public void Dispose()
			{
				if (commonDesktopDirectory != null)
				{
					commonDesktopDirectory.Dispose();
					commonDesktopDirectory = null;
				}
				if (commonProgramsDirectory != null)
				{
					commonProgramsDirectory.Dispose();
					commonProgramsDirectory = null;
				}
				if (desktopDirectory != null)
				{
					desktopDirectory.Dispose();
					desktopDirectory = null;
				}
				if (programsDirectory != null)
				{
					programsDirectory.Dispose();
					programsDirectory = null;
				}
			}

			public int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath)
			{
				string result = null;
				AssertEquals("hwndOwner", IntPtr.Zero, hwndOwner);
				AssertEquals("hToken", IntPtr.Zero, hToken);
				AssertEquals("dwFlags", NativeMethods.SHGFP_TYPE_CURRENT, dwFlags);
				Assert("lpszPath.Capacity >= " + NativeMethods.MAX_PATH, lpszPath.Capacity >= NativeMethods.MAX_PATH);
				Assert("Should ask to create the folder if it does not exist.", (nFolder & NativeMethods.CSIDL_FLAG_CREATE) == NativeMethods.CSIDL_FLAG_CREATE);
				nFolder &= ~NativeMethods.CSIDL_FLAG_CREATE;

				switch (nFolder)
				{
					case NativeMethods.CSIDL_COMMON_DESKTOPDIRECTORY:
						result = CommonDesktopDirectory.DirectoryName;
						break;
					case NativeMethods.CSIDL_COMMON_PROGRAMS:
						result = CommonProgramsDirectory.DirectoryName;
						break;
					case NativeMethods.CSIDL_PROGRAMS:
						result = ProgramsDirectory.DirectoryName;
						break;
					case NativeMethods.CSIDL_DESKTOPDIRECTORY:
						result = DesktopDirectory.DirectoryName;
						break;
				}

				AssertNotNull("No directory - folder: " + nFolder.ToString(), result);
				lpszPath.Append(result);
				return 0;
			}

			#region INativeMethods Members

			int INativeMethods.AddFontResource(string lpszFilename)
			{
				throw new NotImplementedException();
			}

			void INativeMethods.CloseHandle(IntPtr hObject)
			{
				throw new NotImplementedException();
			}

			bool INativeMethods.FreeLibrary(IntPtr hLibModule)
			{
				throw new NotImplementedException();
			}

			IntPtr INativeMethods.GetCurrentProcess()
			{
				throw new NotImplementedException();
			}

			bool INativeMethods.GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes)
			{
				throw new NotImplementedException();
			}

			int INativeMethods.GetFileVersion(string szFilename, StringBuilder szBuffer, int cchBuffer, out int dwLength)
			{
				throw new NotImplementedException();
			}

			IntPtr INativeMethods.GetModuleHandle(string lpModuleName)
			{
				throw new NotImplementedException();
			}

			int INativeMethods.GetSystemWindowsDirectory(StringBuilder lpBuffer, int uSize)
			{
				throw new NotImplementedException();
			}

			bool INativeMethods.GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength)
			{
				throw new NotImplementedException();
			}

			IntPtr INativeMethods.LoadLibrary(string lpLibFileName)
			{
				throw new NotImplementedException();
			}

			bool INativeMethods.OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out int TokenHandle)
			{
				throw new NotImplementedException();
			}

			void INativeMethods.StrongNameFreeBuffer(IntPtr pbMemory)
			{
				throw new NotImplementedException();
			}

			bool INativeMethods.StrongNameSignatureVerificationEx(string wszFilePath, bool fForceVerification, out bool pfWasVerified)
			{
				throw new NotImplementedException();
			}

			bool INativeMethods.StrongNameTokenFromAssembly(string wszFilePath, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken)
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		#endregion
	}
}
