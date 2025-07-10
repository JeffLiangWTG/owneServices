using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	[Serializable]
	class PortableGitNotFoundException : Exception
	{
		public PortableGitNotFoundException(string endpointUrl, string portableGitPath, Exception innerException)
			: base($"Portable Git archive not found from url {endpointUrl}{portableGitPath}", innerException)
		{
		}

#if NETFRAMEWORK
		protected PortableGitNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

