using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.ResString;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	public sealed class FtpRegistry : RegistryItemSet
	{
		public static FtpRegistry Instance
		{
			get { return instance ?? (instance = new FtpRegistry()); }
		}
		[ThreadStatic]
		static FtpRegistry instance;

		FtpRegistry()
		{
		}

		public override bool IsForProductivityWise => true;

		public FtpProfileCollectionRegistryItem Profiles
		{
			get
			{
				return GetItem("ServiceManagerFtpGenericPusherPuller_Profiles", delegate
				{
					return new FtpProfileCollectionRegistryItem(
						"ServiceManagerFtpGenericPusherPuller_Profiles",
						RawDataRegistry.Categories.System_FTPService,
						ResString.GetMultilingualString("ac5e5ee7-f14e-641e-978c-978cf14e1fb7", "Profiles"),
						ResString.GetMultilingualString("de5e5ee7-641e-4f26-978c-24ebf14e1fa8", "Profiles for configuration of FTP engine."),
						RegistryStorageFlags.System);
				});
			}
		}
	}
}
