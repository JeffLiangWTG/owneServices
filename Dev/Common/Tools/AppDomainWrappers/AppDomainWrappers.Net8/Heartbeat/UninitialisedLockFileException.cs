using System.Diagnostics.CodeAnalysis;

namespace AppDomainWrappers.Net8
{
	[Serializable]
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public class UninitialisedLockFileException : Exception
	{
		public UninitialisedLockFileException()
			: base("The provided filepath was empty or null. Have you initialised the heartbeat first?")
		{
		}

		public UninitialisedLockFileException(string parameterName)
			: base($"The provided filepath for '{parameterName}' was empty or null. Have you initialised the heartbeat first?")
		{
		}
	}
}
