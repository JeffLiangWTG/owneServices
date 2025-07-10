using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class MacroEvaluationException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		public MacroEvaluationException(string message, ValueProvider valueProvider)
			: base(string.Format((NoResString)"Error evaluating Macro [{0}] - Error Message: {1}", valueProvider.GetType().FullName, message))
		{
		}

#if NETFRAMEWORK
		protected MacroEvaluationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal MacroEvaluationException(MacroEvaluationExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new MacroEvaluationExceptionJsonData()
		{
			Message = base.Message
		};
	}
}
