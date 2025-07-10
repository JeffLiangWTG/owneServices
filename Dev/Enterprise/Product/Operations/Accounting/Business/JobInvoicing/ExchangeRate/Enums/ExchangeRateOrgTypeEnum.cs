using System.ComponentModel;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public enum ExchangeRateOrgTypeEnum
	{
		[Description("")]
		None,
		[Description("CRD")]
		Creditor,
		[Description("DEB")]
		Debtor
	}
}
