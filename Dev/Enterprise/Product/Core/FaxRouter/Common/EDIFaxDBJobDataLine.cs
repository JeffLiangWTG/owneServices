using System;

namespace Enterprise.FaxRouter
{
	public class EDIFaxDBJobDataLine
	{
		public Guid FaxJobId
		{
			get { return fFaxJobId; }
			set { fFaxJobId = value; }
		}

		public DateTime ReceivedDateTime
		{
			get { return fReceivedDateTime; }
			set { fReceivedDateTime = value; }
		}

		public String Sender
		{
			get { return fSender; }
			set { fSender = value; }
		}

		public String ChargeCode
		{
			get { return fChargeCode; }
			set { fChargeCode = value; }
		}

		public String EmailBody
		{
			get { return fEmailBody; }
			set { fEmailBody = value; }
		}

		public String FaxNumber
		{
			get { return fFaxNumber; }
			set { fFaxNumber = value; }
		}

		public int PageCount
		{
			get { return fPageCount; }
			set { fPageCount = value; }
		}

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

		public string EnterpriseCode
		{
			get { return enterpriseCode; }
			set { enterpriseCode = value; }
		}

		public string CompanyCode
		{
			get { return companyCode; }
			set { companyCode = value; }
		}

		public string ServerCode
		{
			get { return serverCode; }
			set { serverCode = value; }
		}

		Guid fFaxJobId = Guid.Empty;
		DateTime fReceivedDateTime = DateTime.Now;
		String fSender = "";
		String fChargeCode = "";
		String fEmailBody = "";
		String fFaxNumber = "";
		int fPageCount;
		String fSysFaxJobId = "";
		String fSysId = "";
		string enterpriseCode = "";
		string companyCode = "";
		string serverCode = "";
	}
}
