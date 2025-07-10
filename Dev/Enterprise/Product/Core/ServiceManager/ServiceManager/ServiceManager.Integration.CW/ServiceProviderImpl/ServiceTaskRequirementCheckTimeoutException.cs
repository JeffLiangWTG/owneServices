using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.CW
{
	[Serializable]
	public class ServiceTaskRequirementCheckTimeoutException : TimeoutException
	{
		public ServiceTaskRequirementCheckTimeoutException()
			: base()
		{
		}

		public ServiceTaskRequirementCheckTimeoutException(string message)
			: base(message)
		{
		}

		public ServiceTaskRequirementCheckTimeoutException(IHostedServiceAttribute hostedServiceAttribute, string methodName, double timeLimit)
			: this($@"Service task {hostedServiceAttribute.Code} ({hostedServiceAttribute.Description}) timed out on the {methodName} requirement check ({timeLimit}s limit).")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete, See https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/12001/Serializable-and-ISerializable")]
		protected ServiceTaskRequirementCheckTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
