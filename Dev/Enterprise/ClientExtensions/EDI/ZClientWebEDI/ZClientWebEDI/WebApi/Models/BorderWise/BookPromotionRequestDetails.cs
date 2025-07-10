namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.BorderWise
{
	public sealed class BookPromotionRequestDetails
	{
		public string WorkItemNumber { get; set; }
		public string PromotedItemVersion { get; set; }
		public string PromotedItemName { get; set; }
		public bool Completed { get; set; }
	}
}
