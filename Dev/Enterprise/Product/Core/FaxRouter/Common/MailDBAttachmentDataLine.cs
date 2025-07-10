using System;

namespace Enterprise.FaxRouter
{
	public class MailDBAttachmentDataLine
	{
		public Guid PrimaryKey
		{
			get { return fPrimaryKey; }
			set { fPrimaryKey = value; }
		}

		public String FileName
		{
			get { return fFileName; }
			set { fFileName = value; }
		}

		public byte[] Data
		{
			get { return fData; }
			set { fData = value; }
		}

		public String Encoding
		{
			get { return fEncoding; }
			set { fEncoding = value; }
		}

		public Guid MailItemsPK
		{
			get { return fMailItemsPK; }
			set { fMailItemsPK = value; }
		}

		Guid fPrimaryKey = Guid.Empty;
		String fFileName = string.Empty;
		byte[] fData;
		String fEncoding = string.Empty;
		Guid fMailItemsPK = Guid.Empty;
	}
}
