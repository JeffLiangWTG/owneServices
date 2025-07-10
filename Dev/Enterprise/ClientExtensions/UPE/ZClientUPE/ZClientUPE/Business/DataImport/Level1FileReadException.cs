using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.Client.UPE.Business
{
#if NETFRAMEWORK
	[Serializable]
#endif
	public class Level1FileReadException : Exception
	{
		public Level1FileReadException()
			: base()
		{
		}

		public Level1FileReadException(string message)
			: base(message)
		{
		}

		public Level1FileReadException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected Level1FileReadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public int LineNumber;
		public string PreviousLineValue;
		public string LineValue;
	}
}
