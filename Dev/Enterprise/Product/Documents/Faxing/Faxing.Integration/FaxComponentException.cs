using System;
using CargoWise.Common;

namespace Enterprise.Faxing.Integration
{
	[Serializable]
	public class FaxComponentException : Exception
	{
		public FaxComponentException(string message)
			: base(message)
		{
			Argument.NotNullOrEmpty(message, nameof(message));
		}

		public FaxComponentException(string message, Exception innerException)
			: base(message, innerException)
		{
			Argument.NotNullOrEmpty(message, nameof(message));
		}

#if NETFRAMEWORK
		public FaxComponentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			Argument.NotNull(info, nameof(info));
		}
#endif
	}
}
