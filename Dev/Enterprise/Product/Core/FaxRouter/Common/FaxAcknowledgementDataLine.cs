using System;

namespace Enterprise.FaxRouter
{
	public class FaxAcknowledgementDataLine
	{
		public String AckId
		{
			get { return fAckId; }
			set { fAckId = value; }
		}

		public String AckBody
		{
			get { return fAckBody; }
			set { fAckBody = value; }
		}

		public String AckChargeCode
		{
			get { return fAckChargeCode; }
			set { fAckChargeCode = value; }
		}

		public DateTime AckReceivedDateTime
		{
			get { return fAckReceivedDateTime; }
			set { fAckReceivedDateTime = value; }
		}

		public String FaxReciepientId
		{
			get { return fFaxReciepientId; }
			set { fFaxReciepientId = value; }
		}

		String fAckId = "";
		String fAckBody = "";
		String fAckChargeCode = "";
		DateTime fAckReceivedDateTime = DateTime.Now;
		String fFaxReciepientId = "";
	}
}
