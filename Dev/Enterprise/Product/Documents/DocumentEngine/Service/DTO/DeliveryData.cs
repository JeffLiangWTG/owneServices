using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class DeliveryData
	{
		public Guid PrintQueuePk { get; set; }
		public int Copies { get; set; }
		public bool IsDraft { get; set; }
		public bool IncludeCoverNote { get; set; }
		public string CoverNote { get; set; }
		public SelectedValueReportData ReportData { get; set; }
		public List<DeliveryContactData> Contacts { get; } = new List<DeliveryContactData>();
	}

	public class DeliveryContactData
	{
		public string DeliveryMethod { get; set; }
		public string AttachmentType { get; set; }
		public string EmailOrFax { get; set; }
		public string EmailCC { get; set; }
		public string EmailBCC { get; set; }
		public string SendFrom { get; set; }
		public string EmailSubject { get; set; }
		public string Salutation { get; set; }
		public Guid OrgHeaderPK { get; set; }
		public string ContactName { get; set; }
	}
}
