using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rethrown"), Serializable]
	public class RethrownByExceptionHandlerException : Exception
	{
#if NETFRAMEWORK
		protected RethrownByExceptionHandlerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public RethrownByExceptionHandlerException(Exception innerException)
			: base(innerException.Message, innerException)
		{
			Argument.NotNull(innerException, nameof(innerException)); // Suggested By ReviewBot 
		}
	}
}
