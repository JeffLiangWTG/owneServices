using System;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.DocumentScanning.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class ExternalStorageObjectNotFoundException : ExternalStorageException
	{
		public ExternalStorageObjectNotFoundException(string key, string message, string provider, Exception innerException)
			: base(message, provider, innerException)
		{
			Key = key;
		}

#if NETFRAMEWORK
		protected ExternalStorageObjectNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public override void ReportExceptionForDeveloper()
		{
			if (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI)
			{
				ErrorReporter.ReportOnce("ExternalStorageObjectNotFoundExceptionForDeveloper", $"eDoc with primary key '{Key}' could not be found in external storage '{Provider}'.", this);
			}
		}

		public string Key { get; private set; }

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue(nameof(Key), Key);
			base.GetObjectData(info, context);
		}
	}
}
