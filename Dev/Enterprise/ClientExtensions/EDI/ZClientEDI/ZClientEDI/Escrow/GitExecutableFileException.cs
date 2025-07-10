using System;
using System.ComponentModel;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	[Serializable]
	class GitExecutableFileException : GitAdapterException
	{
		public GitExecutableFileException(Win32Exception outerException)
			: base($"System exception encountered trying to run process: git. {outerException.Message}. Native Error code: [{outerException.NativeErrorCode}]", outerException)
		{
		}

#if NETFRAMEWORK
		protected GitExecutableFileException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
