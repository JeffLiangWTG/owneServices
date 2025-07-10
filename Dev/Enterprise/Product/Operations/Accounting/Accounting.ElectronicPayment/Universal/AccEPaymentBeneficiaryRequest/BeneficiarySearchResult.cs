using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.Accounting.ElectronicPayment
{
	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicPayment.XmlSerializers")]
	public class BeneficiarySearchResult
	{
		public List<BeneficiaryDetails> Beneficiaries => beneficiaries ?? (beneficiaries = new List<BeneficiaryDetails>());
		List<BeneficiaryDetails> beneficiaries;

		public bool IsComplete { get; set; }
		public int StartPageNumber { get; set; }
		public int EndPageNumber { get; set; }
	}

	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicPayment.XmlSerializers")]
	public class BeneficiaryDetails
	{
		public string BeneficiaryFullName { get; set; }
		public string BeneficiaryNickName { get; set; }
		public string ProviderCode { get; set; }
		public string ProviderReference { get; set; }
		public string ProviderClassification { get; set; }
		public string Currency { get; set; }
		public string Address { get; set; }
		public string Country { get; set; }
		public string EmailAddress { get; set; }
		public string BankName { get; set; }
		public string BankBranchName { get; set; }
		public string BankAddress { get; set; }
		public string BankAccount { get; set; }
		public string BankAccountSuffix { get; set; }
		public string BankCode { get; set; }
		public string BranchCode { get; set; }
		public string BankSwift { get; set; }
		public string PaymentReason { get; set; }
		public string BusinessNumber { get; set; }
	}
}
