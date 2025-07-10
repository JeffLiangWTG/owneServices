using System;

namespace Enterprise.DocumentScanning.Integration
{
	[Serializable]
	public class ExternalStorageException : Exception
	{
		public virtual string UnableToAccessStorageFriendlyMessage => Res.GetString("B7DFC57A-6DA0-4023-B17B-560A0612A9F7", "Unable to access {0} storage, please contact your system administrator to check the configuration of the eDocs storage. Error message: {1}", Provider, Message);

		public ExternalStorageException(string message, string provider, Exception innerException)
			: base(message, innerException)
		{
			Provider = provider;
		}

#if NETFRAMEWORK
		protected ExternalStorageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string Provider { get; }

		public virtual void ReportExceptionForDeveloper()
		{
		}

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue(nameof(Provider), Provider);
			base.GetObjectData(info, context);
		}
	}
}
