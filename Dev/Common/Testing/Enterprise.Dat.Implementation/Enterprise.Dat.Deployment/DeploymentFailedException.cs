using System;

namespace Enterprise.Dat.Implementation
{
	[Serializable]
	public class DeploymentFailedException : Exception
	{
		public DeploymentFailedException()
		{
		}

		public DeploymentFailedException(string message)
			: base(message)
		{
		}

		public DeploymentFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected DeploymentFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
