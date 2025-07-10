using System;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[Serializable]
	class SystemWebCachingNullReferenceExceptionForTest : NullReferenceException
	{
		public SystemWebCachingNullReferenceExceptionForTest(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected SystemWebCachingNullReferenceExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public override string StackTrace => "at System.Web.Caching.UsageBucket.GetFreeUsageEntry()";
	}
}
