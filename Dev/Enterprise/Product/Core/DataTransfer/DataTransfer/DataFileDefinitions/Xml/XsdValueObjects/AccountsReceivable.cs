using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAccountsReceivable")]
	public class AccountsReceivable : Xsd.AutoAccountsReceivable
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && (DefaultCurrencySpecified || CreditLimitSpecified || CreditOnHoldSpecified || UseSettlementGroupCreditLimitSpecified || CreditApprovedSpecified
				|| GSTIsApplicableSpecified || WithholdingTaxIsApplicableSpecified || AccountGroupSpecified
				|| SettlementDetails.StandardInvoiceTermsSpecified || SettlementDetails.StandardInvoiceDaysSpecified
				|| SettlementDetails.DisbursementInvoiceDaysSpecified || SettlementDetails.DisbursementInvoiceTermsSpecified
				|| SettlementDetails.SettlementGroupSpecified);
			}
		}
	}
}
