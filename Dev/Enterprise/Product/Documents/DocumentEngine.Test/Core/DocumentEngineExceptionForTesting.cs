using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineExceptionForTesting : DocumentEngineException
	{
		public DocumentEngineExceptionForTesting(string message)
			: base(message) { }

#if NETFRAMEWORK
		protected DocumentEngineExceptionForTesting(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
