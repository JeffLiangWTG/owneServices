using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class ExpressionEvaluationException : DocumentEngineException
	{
		internal ExpressionEvaluationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ExpressionEvaluationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		#region Constructor For IJsonSerializable

		internal ExpressionEvaluationException(ExpressionEvaluationExceptionJsonData data)
			: base(data.Message, new Exception(data.InnerExceptionMessage))
		{
		}

		#endregion
	}
}
