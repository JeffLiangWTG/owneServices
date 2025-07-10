using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicPayment
{
	[DataContract]
	public class PaymentRequest
	{
		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "dealInternalReference", Order = 1)]
		[Required]
		public string DealInternalReference { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "quoteProviderReference", Order = 2)]
		[Required]
		public string QuoteIdUsedForPayment { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "bankAccountCode", Order = 3)]
		[Required]
		public string BankAccountCode { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "minFundAmount", Order = 4)]
		[Required]
		public decimal MinimumFundAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "maxFundAmount", Order = 5)]
		[Required]
		public decimal MaximumFundAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "fundCurrency", Order = 6)]
		[Required]
		public string FundCurrency { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "payAmount", Order = 7)]
		[Required]
		public decimal PayAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "payCurrency", Order = 8)]
		[Required]
		public string PayCurrency { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "paymentItems", Order = 9)]
		[Required]
		public List<PaymentLine> PaymentItems { get; set; } = null;
	}
}
