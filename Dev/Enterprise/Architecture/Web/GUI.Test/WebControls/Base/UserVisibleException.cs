using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[ExceptionVisibility(ExceptionVisibility.User), Serializable]
	class UserVisibleException : Exception
	{
		public UserVisibleException() : base()
		{
		}

#if NETFRAMEWORK
		protected UserVisibleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
