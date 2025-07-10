namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public class BookVersionUpdatedRequestDetails
	{
		public string BookName { get; set; }
		public int BookVersion { get; set; }
		public string CountryCode { get; set; }
		public string ReasonDescription { get; set; }
		public bool RequiresManualConversion { get; set; }
	}
}
