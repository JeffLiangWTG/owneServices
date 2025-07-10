using System;

namespace Enterprise.DocumentEngineIntegration
{
	public class DeliveryInstructionsBase
	{
		public DeliveryRecipientBase[] Recipients { get; set; }
		public Guid PrinterId { get; set; }
		public int Copies { get; set; }
		public bool IsDraft { get; set; }
		public string Language { get; set; }
		public string CoverNote { get; set; }
		public bool UseBackgroundDelivery { get; set; }
		public bool ShowOnlyPrintersUserCanPrintTo { get; set; }
	}
}
