using System;
using System.IO;
using System.Reflection;
using CargoWise.Application;
using CargoWise.BrandManager;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class RemoteDesktopServicesPostLoginTask : IPostLoginTask, ILicensedComponent
	{
		public string TaskDescription
		{
			get { return Res.GetString("3254237a-f51b-466b-ac4c-43fb95696d8f", "Initializing Remote Desktop Services"); }
		}

		public bool ShouldExecute()
		{
			return RemoteDesktopServicesInitializationTask.ChannelInitialized;
		}

		public void Execute()
		{
			EnterpriseChannel.InitializeUI(GlbCompany.CurrentCompany.LicenceKeyIdentifier, GlbBranch.CurrentBranch.GB_Code);

			// Login to licence.
			// Ignore failure. User count is required to be the same as core user count, so should always succeed.
			// Form will Logout when it is disposed.
			Env.Licence.RemoteDesktopServices.Login(this);
		}

		public static bool Upgrade()
		{
			if (!RemoteFile.IsSupported)
			{
				Globals.Message.ShowWarning(Res.GetString("1628c95c-9f0d-4237-9179-b10a4b2a1208", "{0} cannot launch the upgrade automatically, you will need to manually save and run the upgrade installer.", BrandingFactory.Instance.ProductName));
			}
			else
			{
				try
				{
					using (var downloadFormManager = new MasterFiles.GUI.WaitDownloadingFormManager())
					{
						var upgraderFile = ObjectFactory.Get<TerminalService>().IsCitrixICA ? UpgraderResources.CitrixUpgraderExeName : UpgraderResources.RdsUpgraderExeName;
						downloadFormManager.Start();
						using (var remoteFile = ObjectFactory.Get<IRemoteFile>(nameof(IRemoteFile), upgraderFile, File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), upgraderFile)), false, true))
						{
							return remoteFile.Open();
						}
					}
				}
				catch (OperationCanceledException)
				{
					Globals.Message.ShowWarning(Res.GetString("1628c95c-9f0d-4237-9179-b10a4b2a1208", "{0} cannot launch the upgrade automatically, you will need to manually save and run the upgrade installer.", BrandingFactory.Instance.ProductName));
				}
			}
			return false;
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return licensedComponentManager ?? (licensedComponentManager = new LicensedComponentManager(this)); }
		}
		LicensedComponentManager licensedComponentManager;
	}
}

// Tests in RemoteDesktopServicesInitializationTaskTestS
