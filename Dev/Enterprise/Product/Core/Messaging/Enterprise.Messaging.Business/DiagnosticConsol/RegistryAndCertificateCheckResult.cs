using Enterprise.StabilityChecker;

namespace Enterprise.Messaging.Business
{
	public class RegistryAndCertificateCheckResult
	{
		public string Message { get; set; }
		public StabilityResultLevel Level { get; set; }

		public RegistryAndCertificateCheckResult(string message, StabilityResultLevel level)
		{
			Message = message;
			Level = level;
		}
	}
}
