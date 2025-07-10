using System;

namespace Enterprise.Services.OperationalActions.Business
{
	[Serializable]
	public sealed class ParseException : Exception
	{
		public ParseException()
			: base()	{ }

		public ParseException(string message)
			: base(message) { }

		public ParseException(string message, Exception innerException)
			: base(message, innerException) { }

#if NETFRAMEWORK
		ParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
