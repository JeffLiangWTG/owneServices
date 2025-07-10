using System;

namespace Enterprise.FaxRouter
{
	public class EDIFaxDBRecipientDataLine
	{
		public Guid FaxRecipientId
		{
			get { return fFaxRecipientId; }
			set { fFaxRecipientId = value; }
		}

		public String FaxNumber
		{
			get { return fFaxNumber; }
			set { fFaxNumber = value; }
		}

		public String AttentionName
		{
			get { return fAttentionName; }
			set { fAttentionName = value; }
		}

		public String Company
		{
			get { return fCompany; }
			set { fCompany = value; }
		}

		public String DocumentName
		{
			get { return fDocumentName; }
			set { fDocumentName = value; }
		}

		public DateTime SentDateTime
		{
			get { return fSentDateTime; }
			set { fSentDateTime = value; }
		}

		public bool IsConfirmed
		{
			get { return fIsConfirmed; }
			set { fIsConfirmed = value; }
		}

		public String ChargeCode
		{
			get { return fChargeCode; }
			set { fChargeCode = value; }
		}

		Guid fFaxRecipientId = Guid.Empty;
		String fFaxNumber = "";
		String fAttentionName = "";
		String fCompany = "";
		DateTime fSentDateTime = new DateTime(1900, 1, 1);
		String fDocumentName = "";
		bool fIsConfirmed;
		String fChargeCode = "";
	}
}
