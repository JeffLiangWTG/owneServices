namespace Enterprise.DocumentEngineIntegration
{
	public enum EDocUpdateStatus
	{
		Undefined = 0,
		Succeeded = 1,
		BusinessObjectNotFound = 2,
		BusinessObjectNotSupported = 3,
		EDocNotFound = 4,
		InvalidDocumentType = 5,
		ContentNotSpecified = 6,
		InvalidDocument = 7,
		UnpublishedForContact = 8,
		VirusDetected = 9
	}

	public class EDocUpdateResult
	{
		public EDocDetail EDocDetail { get; set; }
		public EDocUpdateStatus Status { get; set; }
	}
}
