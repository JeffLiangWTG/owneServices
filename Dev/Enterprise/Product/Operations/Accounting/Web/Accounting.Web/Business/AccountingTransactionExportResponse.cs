using System;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class AccountingTransactionExportResponse : ResponseBase
	{
		public long BatchNumber { get; set; }
		public string PayLoad { get; set; }
	}
}
