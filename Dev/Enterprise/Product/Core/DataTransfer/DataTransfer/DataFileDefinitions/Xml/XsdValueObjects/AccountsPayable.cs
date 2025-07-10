using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAccountsPayable")]
	public class AccountsPayable : Xsd.AutoAccountsPayable
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified && (DefaultCurrencySpecified || CreditLimitSpecified
				|| GSTIsApplicableSpecified || WithholdingTaxIsApplicableSpecified
				|| PaymentTermsSpecified || PaymentDaysSpecified || AccountGroupSpecified
				|| SettlementDetails.StandardInvoiceTermsSpecified || SettlementDetails.StandardInvoiceDaysSpecified
				|| SettlementDetails.DisbursementInvoiceDaysSpecified || SettlementDetails.DisbursementInvoiceTermsSpecified
				|| SettlementDetails.SettlementGroupSpecified);
			}
		}
	}
}
