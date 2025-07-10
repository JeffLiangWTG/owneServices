using System.Runtime.Serialization;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	[DataContract]
	public class MTDVATData
	{
		[DataMember]
		public string periodKey { get; set; }

		[DataMember]
		public decimal vatDueSales { get; set; }

		[DataMember]
		public decimal vatDueAcquisitions { get; set; }

		[DataMember]
		public decimal totalVatDue { get; set; }

		[DataMember]
		public decimal vatReclaimedCurrPeriod { get; set; }

		[DataMember]
		public decimal netVatDue { get; set; }

		[DataMember]
		public decimal totalValueSalesExVAT { get; set; }

		[DataMember]
		public decimal totalValuePurchasesExVAT { get; set; }

		[DataMember]
		public decimal totalValueGoodsSuppliedExVAT { get; set; }

		[DataMember]
		public decimal totalAcquisitionsExVAT { get; set; }

		[DataMember]
		public bool finalised { get; set; }

		public override string ToString()
		{
			return Res.GetString("2460ae2c-46ff-4b10-8293-290a8e4d693d",
@"a. VAT due on sales and other outputs                                                             : {0}
b. VAT due on acquisitions from other EC Member States                                            : {1}
c. Total VAT due (a + b)                                                                          : {2} 
d. VAT reclaimed on purchases and other inputs (including acquisitions from the EC)               : {3}
e. Net VAT Due (c - d)                                                                            : {4}
f. Total value of sales and all other outputs excluding any VAT                                   : {5}
g. Total value of purchases and all other inputs excluding any VAT (including exempt purchases)   : {6}
h. Total value of all supplies of goods and related costs, excluding any VAT, to other EC states  : {7}
i. Total value of acquisitions of goods and related costs excluding any VAT, from other EC states : {8}"
, vatDueSales
, vatDueAcquisitions
, totalVatDue
, vatReclaimedCurrPeriod
, netVatDue
, totalValueSalesExVAT
, totalValuePurchasesExVAT
, totalValueGoodsSuppliedExVAT
, totalAcquisitionsExVAT);
		}
	}
}