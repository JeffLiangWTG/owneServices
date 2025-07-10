using System;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IDeliveryRecipient
	{
		string Name { get; }
		Guid OrganizationId { get; }
		string DeliveryMethod { get; }
		string Email { get; }
		string CC { get; }
		string BCC { get; }
		string EmailAttachmentType { get; }
		string FaxNumber { get; }
	}

	public class DeliveryRecipientBase : IDeliveryRecipient
	{
		public string Name { get; set; }
		public Guid OrganizationId { get; set; }
		public string DeliveryMethod { get; set; }
		public string Email { get; set; }
		public string CC { get; set; }
		public string BCC { get; set; }
		public string EmailAttachmentType { get; set; }
		public string FaxNumber { get; set; }
	}
}
