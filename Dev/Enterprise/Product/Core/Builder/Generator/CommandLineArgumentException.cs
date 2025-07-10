using System;

namespace Enterprise.Builder.Generator
{
	[Serializable]
	sealed class CommandLineArgumentException : Exception
	{
		public CommandLineArgumentException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		CommandLineArgumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
