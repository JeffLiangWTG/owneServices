using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class ComplianceDocumentLineDetail
	{
		public ZGuid LinePK { get; set; }
		public ZString LineDescription { get; set; }
		public ZString TaxCode { get; set; }
		public ZDecimal Rate { get; set; }
		public ZDecimal Amount { get; set; }
		public ZDecimal TaxAmount { get; set; }
	}
}
