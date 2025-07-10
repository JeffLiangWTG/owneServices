using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	class FormulaProviderException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal FormulaProviderException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected FormulaProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal FormulaProviderException(FormulaProviderExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new FormulaProviderExceptionJsonData()
		{
			Message = base.Message
		};
	}
}
