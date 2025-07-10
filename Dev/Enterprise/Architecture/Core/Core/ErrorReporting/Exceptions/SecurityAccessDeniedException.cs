using System;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class SecurityAccessDeniedException : Exception
	{
		public SecurityAccessDeniedException(string message) : base(message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				fMessage = message;
			}
			else
			{
				throw new InvalidOperationException("Exception message should not be null or empty.");
			}
		}

#if NETFRAMEWORK
		protected SecurityAccessDeniedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static string fMessage;

		public override string Message
		{
			get
			{
				return fMessage;
			}
		}
	}
}
