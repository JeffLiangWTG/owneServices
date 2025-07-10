using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class UncommittedTransactionException : EnvironmentCorruptedException
	{
		public UncommittedTransactionException(string message) : base(message)
		{
		}

		public UncommittedTransactionException(IHostedServiceAttribute hostedServiceAttribute, int transactionCount)
			: this($@"The service task code: '{hostedServiceAttribute.Code} - {hostedServiceAttribute.TypeName}, {hostedServiceAttribute.TypeAssemblyName}' has left [{transactionCount}] opened transaction(s) at the end of the run.")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public UncommittedTransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
