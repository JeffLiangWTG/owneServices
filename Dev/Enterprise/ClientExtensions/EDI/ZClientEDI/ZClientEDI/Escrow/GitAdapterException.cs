using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	[Serializable]
	class GitAdapterException : Exception
	{
		public GitAdapterException(int exitCode, string argument, string output)
			: base($"Process finished with error: Process exit code [{exitCode}] Arguments: \"{argument}\" Output: \"{output}\"")
		{
		}

		public GitAdapterException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected GitAdapterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
