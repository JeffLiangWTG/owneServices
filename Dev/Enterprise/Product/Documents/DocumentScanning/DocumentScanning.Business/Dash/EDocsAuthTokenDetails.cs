using System;

namespace Enterprise.DocumentScanning.Business
{
	public class EDocsAuthTokenDetails
	{
		public EDocsAuthTokenDetails(Guid ediMessagePK, DateTime eDocLastEditTime, string docType, string dataType)
		{
			EDIMessagePK = ediMessagePK;
			EDocLastEditTime = eDocLastEditTime;
			DocType = docType;
			DataType = dataType;
		}

		public EDocsAuthTokenDetails()
		{
		}

		public Guid EDIMessagePK { get; set; }

		public DateTime EDocLastEditTime { get; set; }

		public string DocType { get; set; }

		public string DataType { get; set; }
	}
}
