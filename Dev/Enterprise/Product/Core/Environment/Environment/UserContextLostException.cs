using System;
using CargoWise.Common;

namespace Enterprise.Environment
{
	[Serializable]
	public class UserContextLostException : Exception, ICriticalException
	{
		public UserContextLostException(
			IUserContext expectedUserContext,
			IUserContext currentUserContext,
			Exception innerException = null)
			: base($"UserContext is now in an invalid state. Unable to set user context to: {new UserContextInfo(expectedUserContext).Print()}. User context as a result of failure: {new UserContextInfo(currentUserContext).Print()}", innerException)
		{
		}

#if NETFRAMEWORK
		protected UserContextLostException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public bool IsCriticalException => true;
	}
}
