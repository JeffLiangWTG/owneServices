using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	// Code will be removed in WI00829724 once we are confident all customers support new style XUE messages

	[DataContract]
	public class IndiaEInvoiceResponse
	{
		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "AckNo", Order = 1)]
		public long? AckNo { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "AckDt", Order = 2)]
		public string AckDateTime { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Irn", Order = 3)]
		public string Irn { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "SignedInvoice", Order = 4)]
		public string SignedInvoiceAsJwt { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "SignedQRCode", Order = 5)]
		public string SignedQRCodeAsJwt { get; set; } = null;

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Status", Order = 6)]
		public string Status { get; set; } = null;
	}
}
