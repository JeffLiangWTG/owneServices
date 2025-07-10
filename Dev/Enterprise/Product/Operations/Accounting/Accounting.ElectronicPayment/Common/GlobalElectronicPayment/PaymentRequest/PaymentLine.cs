using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicPayment
{
	[DataContract]
	public class PaymentLine
	{
		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "payeeId", Order = 1)]
		[Required]
		public string PayeeId { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "amount", Order = 2)]
		[Required]
		public decimal Amount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "payReason", Order = 3)]
		[Required]
		public string PayReason { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "payReference", Order = 4)]
		[Required]
		public string PayReference { get; set; } = null;
	}
}
