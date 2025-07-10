using System;
using CargoWise.Common;
using Enterprise.DocumentScanning.Integration;

namespace Enterprise.ZArchitecture.GUI
{
	public static class EDocsPluginAttachMonitor
	{
		public static IDisposable StartMonitorEDocAttachment(IMonitorEDocAttachment plugIn)
		{
			plugIn.IsMonitoringDocumentAttachment = true;
			plugIn.StartMonitorEDocAttachment();
			var disposableAction = new DisposableAction(() =>
			{
				plugIn.EndMonitorEDocAttachment();
				plugIn.IsMonitoringDocumentAttachment = false;
			});

			return disposableAction;
		}
	}
}
