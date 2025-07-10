using System;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class NetworkVisualisationErrorReporter
	{
		public static void ReportError()
		{
			ReportError(string.Empty);
		}

		public static void ReportError(string message)
		{
			throw new NetworkVisualisationException(message);
		}

		public static void ReportIfAssertionFailed(bool assertionPassed)
		{
			ReportIfAssertionFailed(assertionPassed, string.Empty);
		}

		public static void ReportIfAssertionFailed(bool assertionPassed, string message)
		{
			if (!assertionPassed)
			{
				ReportError(message);
			}
		}
	}

	[Serializable]
	public class NetworkVisualisationException : Exception
	{
		public NetworkVisualisationException(string message)
			: base(message)
		{
		}

		public NetworkVisualisationException()
			: base()
		{
		}

#if NETFRAMEWORK
		protected NetworkVisualisationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
