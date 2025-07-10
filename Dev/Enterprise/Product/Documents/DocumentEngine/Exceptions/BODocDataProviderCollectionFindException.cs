using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class BODocDataProviderCollectionFindException : DocumentEngineException
	{
		internal BODocDataProviderCollectionFindException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BODocDataProviderCollectionFindException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		#region Constructor For IJsonSerializable

		internal BODocDataProviderCollectionFindException(BODocDataProviderCollectionFindExceptionJsonData data)
			: base(data.Message, new Exception(data.InnerExceptionMessage))
		{
		}

		#endregion
	}
}
