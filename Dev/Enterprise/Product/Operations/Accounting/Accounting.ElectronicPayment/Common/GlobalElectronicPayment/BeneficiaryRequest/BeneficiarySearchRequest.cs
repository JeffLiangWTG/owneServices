using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicPayment
{
	[DataContract]
	public class BeneficiarySearchRequest
	{
		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "requestReference", Order = 1)]
		[Required]
		public string RequestReference { get; set; } = string.Empty;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "bankAccountCode", Order = 2)]
		[Required]
		public string BankAccountCode { get; set; } = string.Empty;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "currency", Order = 3)]
		public string Currency { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "updatedDateFrom", Order = 4)]
		public string UpdatedDateFrom { get; set; } = string.Empty;

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "startPageNumber", Order = 5)]
		public int StartPageNumber { get; set; } = 1;

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "maxNumberOfRecordInHttpResponse", Order = 6)]
		public int MaxNumberOfRecordInHttpResponse { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "maxNumberOfRecordInXUE", Order = 7)]
		public int MaxNumberOfRecordInXUE { get; set; } = 1;
	}
}
