using System;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class ExternalStorageNetworkException : ExternalStorageException
	{
		public ExternalStorageNetworkException(string message, string provider, Exception innerException)
			: base(message, provider, innerException)
		{
		}

#if NETFRAMEWORK
		protected ExternalStorageNetworkException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public override void ReportExceptionForDeveloper()
		{
			if (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI)
			{
				ErrorReporter.ReportOnce("ExternalStorageNetworkExceptionForDeveloper", $"There was a network issue while accessing '{SystemDataRegistry.Instance.EDocsStorageServiceUrl.Value}'.", this);
			}
		}
	}
}
