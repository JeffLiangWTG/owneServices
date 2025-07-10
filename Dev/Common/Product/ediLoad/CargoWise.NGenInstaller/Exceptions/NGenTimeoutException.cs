using System;

namespace CargoWise.NGenInstallerProgram.Exceptions
{
	[Serializable]
	class NGenTimeoutException : Exception
	{
		public NGenTimeoutException(TimeSpan timeout)
			: base($"Running NGen.exe timed out after {timeout}.")
		{
		}

#if NETFRAMEWORK
		protected NGenTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
