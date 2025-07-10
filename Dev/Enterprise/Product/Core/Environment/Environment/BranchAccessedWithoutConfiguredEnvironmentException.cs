using System;

namespace Enterprise.Environment
{
	[Serializable]
	public class BranchAccessedWithoutConfiguredEnvironmentException : InvalidOperationException
	{
		public BranchAccessedWithoutConfiguredEnvironmentException(string message) : base(message) { }

#if NETFRAMEWORK
		protected BranchAccessedWithoutConfiguredEnvironmentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public BranchAccessedWithoutConfiguredEnvironmentException(string message, Exception innerException) : base(message, innerException) { }
	}
}
