using System;
using System.Collections.Generic;

namespace ServiceManager.Common.Abstractions
{
	[Serializable]
	public class CommandLineException : Exception
	{
		public CommandLineException(IEnumerable<CommandLineParseError> errors)
			: base("Error parsing command line arguments")
		{
			Errors = errors;
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected CommandLineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			Errors ??= new List<CommandLineParseError>();
		}
#endif

		public IEnumerable<CommandLineParseError> Errors { get; }
	}
}
