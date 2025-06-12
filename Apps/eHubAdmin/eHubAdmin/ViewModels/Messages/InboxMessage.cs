extern alias Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sys::System.ComponentModel.DataAnnotations;

namespace eServices.eHubAdmin.ViewModels.Messages
{
	public class InboxMessage : Message
    {
        [DisplayFormat(DataFormatString = "{0:u}")]
        public DateTime? Received { get; set; }
        [Display(Name = "Inbox Sender")]
        public string Sender { get; set; }
        [Display(Name = "Inbox Recipient")]
        public string Recipient { get; set; }
        [Display(Name = "Application Code")]
        public string ApplicationCode { get; set; }
        public Guid? AM_EI_PK { get; set; }
        [Display(Name = "Inbox Message Tracking ID")]
        public string MessageTrackingID { get; set; }
        [Display(Name = "Inbox File Name")]
        public string FileName { get; set; }
        [Display(Name = "Inbox Content Raw")]
        public long? ContentRawLength { get; set; }
        [Display(Name = "Inbox Content XML")]
        public long? ContentXmlLength { get; set; }
		public string ContentRaw { get; set; }
		public string ContentXml { get; set; }
        public IEnumerable<OutboxMessage> OutboxMessages { get; set; }
		public string MessageType { get; set; }

        public string StatusDescription
        {
            get
            {
				var uniqueStatusList =
					(new[] { this.Status })
						.Concat(OutboxMessages?.Select(o => o.Status) ?? Enumerable.Empty<byte?>())
						.Distinct();

				var allIsSending = (this.Status == 2 && this.OutboxMessages != null && this.OutboxMessages.All(msg => msg.Status == 1)); // Deals with the weird case that sending is 2 for Inbox & 1 for Outbox

				if (uniqueStatusList.Count() == 1 || allIsSending)
				{
					return this.GetStatusText(true);
				}
				return "[Multiple]";
            }
        }

		public string GetStatusText(bool canCallDetails)
		{
			switch (this.Status)
			{
				case 0:
					return "Received";
				case 1:
					return "Processing";
				case 2:
					if (OutboxMessages != null && OutboxMessages.Any(o => o.Status == 1))
						return "Sending";
					else
						return "Processed";
				case 3:
					if (this.AM_PK.HasValue)
						return "Archived";
					else
						return "Delivered";
				case 255:
					return "Error";
				default:
					return (canCallDetails ? GetStatusDetails() : "???"); // Avoid infinite loop from within GetStatusDetails() below
			}
		}

		public string FullStatus => $"{Status} - ({GetStatusText(false)})";

	    [JsonIgnore]
        public string StatusContextColour
        {
            get
            {
                switch (this.StatusDescription)
                {
                    case "Received":
                        return "info";
                    case "Processing":
                        return "warning";
                    case "Processed":
                        return "active";
                    case "Sending":
                        return "warning";
                    case "Delivered":
                        return "success";
                    case "Error":
                        return "danger";
					case "[Multiple]":
						return "warning";
					default:
                        return string.Empty;
                }
            }
        }

		public string GetStatusDetails()
		{
			var sb = new StringBuilder();

			if (AM_PK.HasValue)
			{
				sb.AppendFormat("Archive: {0}", FullStatus);
			}
			else
			{
				sb.AppendFormat("Inbox: {0}", FullStatus);
			}

			if (OutboxMessages.Count() > 0)
			{
				sb.Append("\n");

				if (OutboxMessages.Select(o => o.Status).Distinct().Count() == 1)
				{
					sb.AppendFormat("Outbox: {0}", OutboxMessages.First().FullStatus);
				}
				else
				{
					sb.AppendFormat("Outbox:\n");
					sb.AppendFormat(string.Join(", ", OutboxMessages.Select(o => o.FullStatus).Distinct().OrderBy(s => s).Select(s => string.Format("'{0}' x {1}", s, OutboxMessages.Count(o => o.FullStatus == s)))));
				}
			}
			return sb.ToString();
		}

		public string GetApplicationCodeDetails()
		{
			var sb = new StringBuilder();

			sb.AppendFormat("Code Description: {0}", GetApplicationCodeDescription());
			sb.AppendFormat("\nMessage Type: {0}", string.IsNullOrEmpty(MessageType) ? "None" : MessageType);

            return sb.ToString();
        }

		public string GetPKIDsHTML()
		{
			var sb = new StringBuilder();

			sb.AppendFormat("<div><span>{0}: {1}<span class=\"glyphicon glyphicon-copy js-copy copy-click blue-glyph action-glyph\" style=\"font-size:15px\" data-copy=\"{1}\" /></span></div>", PK.HasValue ? "EI_PK" : "AM_PK", PK.HasValue ? PK : AM_PK);
			sb.AppendFormat("<div><span>MsgID: {0}<span class=\"glyphicon glyphicon-copy js-copy copy-click blue-glyph action-glyph\" style=\"font-size:15px\" data-copy=\"{0}\" /></span></div>", MessageTrackingID.ToLower());

            return sb.ToString();
        }

		public string GetApplicationCodeAndDescription()
		{
			var appCodeDesc = GetApplicationCodeDescription();
			var dash = string.IsNullOrEmpty(appCodeDesc) ? "" : "-";
			return $"{ApplicationCode} {dash} {appCodeDesc}";
		}

		public string GetMessageType()
		{
			return string.IsNullOrEmpty(MessageType) ? "None" : MessageType;
		}

		private string GetApplicationCodeDescription()
		{
			string codeDesc = "";
			Constants.ApplicationCodeTypes.TryGetValue(ApplicationCode, out codeDesc);
			return codeDesc;
		}
    }
}