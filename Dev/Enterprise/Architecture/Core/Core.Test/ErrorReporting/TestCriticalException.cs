using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[Serializable]
	class TestCriticalException : Exception, ICriticalException
	{
		public TestCriticalException()
		{
		}

#if NETFRAMEWORK
		protected TestCriticalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public bool IsCriticalException => true;
	}
}
