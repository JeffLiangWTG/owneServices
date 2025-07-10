using System;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class FileAccessException : Exception
	{
		public FileAccessException(string message, string filename, Exception innerException)
			: base(message, innerException)
		{
			this.Filename = filename;
		}

#if NETFRAMEWORK
		protected FileAccessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string Filename;
	}
}
