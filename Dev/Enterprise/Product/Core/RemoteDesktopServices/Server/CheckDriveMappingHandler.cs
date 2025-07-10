using System;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class CheckDriveMappingHandler : XmlMessageHandlerWithReturn<string, DriveMappingResult>
	{
		protected override DriveMappingResult DoHandle(IEnterpriseChannel channel, string message)
		{
			if (!EnvProxy.Instance.Registry.RemoteAppEnableDragDropLite)
			{
				TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Drive mapping is Disabled");
				return DriveMappingResult.Disabled;
			}
			try
			{
				new MappedClientPath().GetMappedPathOfExistingFile(message);
				TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Drive mapping check succeeded");
				return DriveMappingResult.Success;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				TrackingInfoLogger.Instance.NewLog(() => $"Drive mapping check failed due to exception: {ex}");
				return DriveMappingResult.FileDoesNotExist;
			}
		}
	}
}
