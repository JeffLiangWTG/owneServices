using System;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.BorderWise
{
	public class CreateWorkItemRequest
	{
		public string Product { get; set; }
		public string ProductArea { get; set; }
		public string Module { get; set; }
		public string ChangeType { get; set; }
		public string Summary { get; set; }
		public string Description { get; set; }
		public string ApiKey { get; set; }
		public string Priority { get; set; }
		public string CapDev { get; set; }
		public string RND { get; set; }
		public DateTime? DueDate { get; set; }
	}
}
