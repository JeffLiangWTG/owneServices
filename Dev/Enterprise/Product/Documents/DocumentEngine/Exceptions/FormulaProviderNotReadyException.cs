using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class FormulaProviderNotReadyException : Exception
	{
		public FormulaProviderNotReadyException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected FormulaProviderNotReadyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
