#if DEBUG
using System;
using CargoWise.Common;
using NUnit.Framework;

#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ValidTestDataGenerationException : ApplicationException
	{
		public ValidTestDataGenerationException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected ValidTestDataGenerationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
		public static bool ShouldThrow
		{
			get { return !TestingState.IsRunningTests && !disabled; }
		}

		public static IDisposable Disable()
		{
			if (disabled)
			{
				throw new InvalidOperationException("Already Disabled");
			}
			disabled = true;
			return new DisposableAction(() => disabled = false);
		}
		static bool disabled;
	}
}
#endif
