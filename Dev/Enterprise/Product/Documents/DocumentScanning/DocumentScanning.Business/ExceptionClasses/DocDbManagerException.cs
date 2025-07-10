using System;

using CargoWise.Data;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class DocDbManagerException : OdysseyDataException
	{
		public DocDbManagerException(string message)
			: base(message)
		{
		}

		public DocDbManagerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocDbManagerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}