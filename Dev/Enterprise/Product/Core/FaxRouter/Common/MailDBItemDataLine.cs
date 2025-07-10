using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Enterprise.FaxRouter
{
	public class MailDBItemDataLine
	{
		#region Fields

		Guid fPrimaryKey = Guid.Empty;
		String fDirection = "";
		DateTime fReceivedDateTime = new DateTime(1900, 1, 1);
		DateTime fSentDateTime = new DateTime(1900, 1, 1);
		DateTime fLastAttemptDateTime = new DateTime(1900, 1, 1);
		String fFrom = "";
		String fHeader = "";
		String fBody = "";
		ArrayList fAttachment = new ArrayList();

		Guid fFaxRecipientId = Guid.Empty;
		String fFaxRecipientNumber = "";
		String fFaxRecipientAttentionName = "";
		String fFaxRecipientCompany = "";
		DateTime fFaxRecipientSentDateTime = new DateTime(1900, 1, 1);
		String fFaxRecipientDocumentName = "";
		bool fFaxRecipientIsAcknowledged;
		String fFaxRecipientAckSuccess = "";
		DateTime fFaxRecipientAckDateTime = new DateTime(1900, 1, 1);

		String fChargeCode = "";
		int fPageCount;

		String fFaxKey = "";
		String fFaxKeyDateTime = "";

		String fSysFaxJobId = "";
		String fSysId = "";

		Guid fReportedLicenceHeader = Guid.Empty;

		#endregion

		#region FaxRecipientProperties

		public Guid FaxRecipientId
		{
			get { return fFaxRecipientId; }
			set { fFaxRecipientId = value; }
		}

		public String FaxRecipientNumber
		{
			get { return fFaxRecipientNumber; }
			set { fFaxRecipientNumber = value; }
		}

		public String FaxRecipientAttentionName
		{
			get { return fFaxRecipientAttentionName; }
			set { fFaxRecipientAttentionName = value; }
		}

		public String FaxRecipientCompany
		{
			get { return fFaxRecipientCompany; }
			set { fFaxRecipientCompany = value; }
		}

		public String FaxRecipientDocumentName
		{
			get { return fFaxRecipientDocumentName; }
			set { fFaxRecipientDocumentName = value; }
		}

		public DateTime FaxRecipientSentDateTime
		{
			get { return fFaxRecipientSentDateTime; }
			set { fFaxRecipientSentDateTime = value; }
		}

		public bool FaxRecipientIsAcknowledged
		{
			get { return fFaxRecipientIsAcknowledged; }
			set { fFaxRecipientIsAcknowledged = value; }
		}

		public String FaxRecipientAckSuccess
		{
			get { return fFaxRecipientAckSuccess; }
			set { fFaxRecipientAckSuccess = value; }
		}

		public DateTime FaxRecipientAckDateTime
		{
			get { return fFaxRecipientAckDateTime; }
			set { fFaxRecipientAckDateTime = value; }
		}

		#endregion

		#region MailProperties
		public Guid PrimaryKey
		{
			get { return fPrimaryKey; }
			set { fPrimaryKey = value; }
		}

		public String Direction
		{
			get { return fDirection; }
			set { fDirection = value; }
		}

		public DateTime ReceivedDateTime
		{
			get { return fReceivedDateTime; }
			set { fReceivedDateTime = value; }
		}

		public DateTime SentDateTime
		{
			get { return fSentDateTime; }
			set { fSentDateTime = value; }
		}

		public DateTime LastAttemptDateTime
		{
			get { return fLastAttemptDateTime; }
			set { fLastAttemptDateTime = value; }
		}

		public String From
		{
			get { return fFrom; }
			set { fFrom = value; }
		}

		public String Header
		{
			get { return fHeader; }
			set
			{
				fHeader = value;
				fHeaders = null;
			}
		}

		public String Body
		{
			get { return fBody; }
			set { fBody = value; }
		}

		public String ChargeCode
		{
			get { return fChargeCode; }
			set { fChargeCode = value; }
		}

		public int PageCount
		{
			get { return fPageCount; }
			set { fPageCount = value; }
		}

		public NameValueCollection Headers
		{
			get
			{
				if (fHeaders == null)
				{
					fHeaders = new NameValueCollection();
					foreach (string headerLine in Regex.Split(Header, @"\r\n(?!\s)"))
					{
						Match match = Regex.Match(headerLine, @"(.*?):\s*(.*)", RegexOptions.Singleline);
						if (match.Success)
						{
							fHeaders.Add(match.Groups[1].ToString(), match.Groups[2].ToString());
						}
					}
				}
				return fHeaders;
			}
		}
		NameValueCollection fHeaders;

		public string Subject
		{
			get
			{
				return Headers["Subject"];
			}
		}

		#endregion

		#region AttachmentProperties

		public ArrayList Attachments
		{
			get { return fAttachment; }
			set { fAttachment = value; }
		}

		#endregion

		#region FaxKeyProperties

		public String FaxKey
		{
			get { return fFaxKey; }
			set { fFaxKey = value; }
		}

		public String FaxKeyDateTime
		{
			get { return fFaxKeyDateTime; }
			set { fFaxKeyDateTime = value; }
		}

		#endregion

		#region RemoteSysProperties

		public String SysFaxJobId
		{
			get { return fSysFaxJobId; }
			set { fSysFaxJobId = value; }
		}

		public String SysId
		{
			get { return fSysId; }
			set { fSysId = value; }
		}

		public Guid ReportedLicenceHeader
		{
			get { return fReportedLicenceHeader; }
			set { fReportedLicenceHeader = value; }
		}
		#endregion
	}
}
