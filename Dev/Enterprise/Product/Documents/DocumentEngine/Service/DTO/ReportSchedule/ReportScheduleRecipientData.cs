using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class ReportScheduleRecipientData
	{
		public string DeliveryRecipientType { get; set; }

		public Guid S6_OH { get; set; }

		public string ContactName { get; set; }

		public string S6_GS_NKRecipient { get; set; }

		public Guid S6_GG { get; set; }

		public string S6_DeliveryMethod { get; set; }

		public string S6_AttachmentType { get; set; }

		public Guid S6_SQ { get; set; }

		public string S6_EmptyReportDeliveryOptions { get; set; }

		public string EmailFromAddress { get; set; }

		public string ToFaxOrEmail { get; set; }

		public IEnumerable<string> CarbonCopyRecipients { get; set; }

		public IEnumerable<string> BlindCarbonCopyRecipients { get; set; }

		public string S6_FtpAddress { get; set; }

		public string S6_UserName { get; set; }

		public string S6_Password { get; set; }

		public Guid Identifier { get; set; }
	}
}
