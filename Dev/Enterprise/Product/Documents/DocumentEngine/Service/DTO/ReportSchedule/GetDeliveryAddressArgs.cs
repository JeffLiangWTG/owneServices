using System;

namespace Enterprise.DocumentEngine
{
	public class GetDeliveryAddressArgs
	{
		public string DeliveryToType { get; set; }
		public string DeliveryMethod { get; set; }
		public string EmptyReportDeliveryOptions { get; set; }
		public Guid OrganizationId { get; set; }
		public string ContactName { get; set; }
		public string StaffCode { get; set; }
		public Guid GroupId { get; set; }
	}
}
