extern alias Sys;

using System;
using Sys::System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Text;

namespace eServices.eHubAdmin.ViewModels.Messages
{
    public class OutboxMessage : Message
    {
        [DisplayFormat(DataFormatString = "{0:u}")]
        public DateTime? Delivered { get; set; }
        [DisplayFormat(DataFormatString = "{0:u}")]
        public DateTime? Processed { get; set; }
        [Display(Name = "Outbox Sender")]
        public string Sender { get; set; }
        [Display(Name = "Outbox Recipient")]
        public string Recipient { get; set; }
        [Display(Name = "Outbox Message Tracking ID")]
        public string MessageTrackingID { get; set; }
        [Display(Name = "Outbox File Name")]
        public string FileName { get; set; }
        [Display(Name = "Outbox Content Raw")]
        public long? ContentRawLength { get; set; }
        [Display(Name = "Outbox Content XML")]
		public string ContentRaw { get; set; }
		public string ContentXml { get; set; }
		public long? ContentXmlLength { get; set; }

	    [JsonIgnore]
		public string StatusDescription
	    {
		    get
		    {
			    switch (this.Status)
			    {
				    case 0:
					    return "Send Queue";
				    case 1:
					    return "Sending";
				    case 3:
					    return "Delivered";
				    case 255:
					    return "ERROR";
				    default:
					    return string.Empty;
			    }
		    }
	    }

	    public string FullStatus => $"{Status} - ({StatusDescription})";

		public string GetPKIDsHTML()
		{
			var sb = new StringBuilder();

			sb.AppendFormat("<div><span>{0}: {1}</span><span class=\"glyphicon glyphicon-copy js-copy copy-click blue-glyph action-glyph\" style=\"font-size:15px\" data-copy=\"{1}\" /></span></div>", PK.HasValue ? "OI_PK" : "AM_PK", PK.HasValue ? PK : AM_PK);
			sb.AppendFormat("<div><span>MsgID: {0}</span><span class=\"glyphicon glyphicon-copy js-copy copy-click blue-glyph action-glyph\" style=\"font-size:15px\" data-copy=\"{0}\" /></span></div>", MessageTrackingID.ToLower());

			return sb.ToString();
		}
	}
}