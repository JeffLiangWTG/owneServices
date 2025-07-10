using System;
using Enterprise.DocumentScanning.Integration;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class ExternalStorageUriFormatException : ExternalStorageException
	{
		public ExternalStorageUriFormatException(string message, string provider, Exception innerException)
			: base(message, provider, innerException)
		{
		}

#if NETFRAMEWORK
		protected ExternalStorageUriFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
