using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	[Serializable]
	class PortableGitExtractionException : Exception
	{
		public PortableGitExtractionException(string archiveFileName, Exception innerException)
			: base($"Fail to unzip PortableGit file {archiveFileName} downloaded from ProGet server", innerException)
		{
		}

#if NETFRAMEWORK
		protected PortableGitExtractionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

