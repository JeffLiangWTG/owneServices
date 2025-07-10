using System;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	[Serializable]
	public class GEPMessageCreationException : Exception
	{
		public string UserFriendlyMessage { get; set; }

		public GEPMessageCreationException(string message)
			: base(message)
		{
		}

		public GEPMessageCreationException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected GEPMessageCreationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
