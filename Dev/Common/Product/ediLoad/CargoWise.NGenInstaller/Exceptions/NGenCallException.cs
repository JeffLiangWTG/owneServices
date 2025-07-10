using System;

namespace CargoWise.NGenInstallerProgram.Exceptions
{
	[Serializable]
	class NGenCallException : Exception
	{
		public NGenCallException(int exitCode, string argument, string output)
			: base($"Process finished with error: Process exit code {exitCode} Arguments: \"{argument}\" Output: \"{output}\"")
		{
		}

#if NETFRAMEWORK
		protected NGenCallException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
