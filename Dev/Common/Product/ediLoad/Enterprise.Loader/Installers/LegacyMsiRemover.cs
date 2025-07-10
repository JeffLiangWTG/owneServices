using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;
using Microsoft.Win32;

namespace Enterprise.Loader
{
	class LegacyMsiRemover : WindowsInstallerAppManagerInvoker
	{
		public LegacyMsiRemover()
			: this(null)
		{
		}

		public LegacyMsiRemover(ClientInstallation installation)
			: base(installation)
		{
			IgnoreMutexErrors = true;
		}

		protected override string Arguments
		{
			get { return "/qn /x " + productCode; }
		}

		public override string InstallerAbsolutePath
		{
			get { return "msiexec.exe"; }
		}

		protected override string NameOfComponentBeingInstalled
		{
			get { return "ediEnterprise Instance"; }
		}

		protected override string TaskDescription
		{
			get { return "Removing ediEnterprise Instance"; }
		}

		protected override bool NeedsToInstallCore()
		{
			var needsToInstall = false;
			if (((EnterpriseConfiguration)Installation.Configuration).UsingConfigFile)
			{
				using (var regKey = Registry.LocalMachine.OpenSubKey(SetupHelper.KeyName, false))
				{
					if (regKey != null)
					{
						var instanceKey = SetupHelper.GetInstanceSubKey(regKey, ((EnterpriseConfiguration)Installation.Configuration).LegacyInstanceName);
						if (instanceKey != null)
						{
							instanceName = instanceKey.Name.Substring(regKey.Name.Length + 1);
							productCode = instanceKey.GetValue(LegacyClientSetupHelper.ProductCodeValueName) as string;
							needsToInstall = !string.IsNullOrEmpty(productCode);
						}
					}
				}
			}
			return needsToInstall;
		}

		readonly int[] errorCodesToIgnore =
		{
			1605, // ERROR_UNKNOWN_PRODUCT
			1619  // ERROR_INSTALL_PACKAGE_OPEN_FAILED
		};

		bool IsErrorCodeToIgnore(InstallationResultCollection result)
		{
			return result.Count == 1 && result[0].IsError && errorCodesToIgnore.Select(errorCodeToIgnore => new Win32Exception(errorCodeToIgnore).Message).Any(errorMessage => result[0].Message.Contains(errorMessage));
		}

		protected override void ProcessResult(ref InstallationResultCollection result)
		{
			if (!IsErrorCodeToIgnore(result))
			{
				return;
			}

			result = new InstallationResultCollection();
			result.Add(InstallationResult.OK());

			RemoveRegistryInstance();
		}

		protected virtual void RemoveRegistryInstance()
		{
			try
			{
				using (var regKey = Registry.LocalMachine.OpenSubKey(SetupHelper.KeyName, true))
				{
					if (regKey != null)
					{
						regKey.DeleteSubKey(instanceName);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Do nothing
			}
		}

		protected override MutexRequest CreateMutexRequest()
		{
			return MutexRequest.Create(TaskDescription, 1);
		}

		protected override object GetAppManagerState()
		{
			return new string[] { InstallerAbsolutePath, productCode, instanceName };
		}

		protected override void SetAppManagerState(object state)
		{
			fullPathOfProgramToRun = ((string[])state)[0];
			productCode = ((string[])state)[1];
			instanceName = ((string[])state)[2];
		}

		protected override Configuration GetNewConfiguration()
		{
			return new EnterpriseConfiguration();
		}

		protected LegacyClientSetupHelper SetupHelper
		{
			get { return setupHelper ?? (setupHelper = new LegacyClientSetupHelper()); }
		}
		LegacyClientSetupHelper setupHelper;

		protected string productCode;
		protected string instanceName;
	}
}


