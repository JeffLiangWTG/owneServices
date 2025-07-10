namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public sealed class BookRejectionRequestDetails
	{
		public string BookName { get; set; }
		public string BookId { get; set; }
		public string ContentId { get; set; }
		public string RejectionReason { get; set; }
		public string Details { get; set; }
		public string CountryCode { get; set; }
		public string WorkItemNumber { get; set; }
		public string Type { get; set; }
		public string ReviewedItemName { get; set; }
	}
}
