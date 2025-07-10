using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class CloneAreaException : DocumentEngineException
	{
		internal CloneAreaException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected CloneAreaException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal CloneAreaException(CloneAreaExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion
	}
}
