using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	[DataContract]
	public class VietnamEInvoiceDocument
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "user", Order = 1)]
		public User User { get; set; } = new User();

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "inv", Order = 2)]
		public InvForDocumentReqeust Inv { get; set; } = new InvForDocumentReqeust();
	}

	[DataContract]
	public class InvForDocumentReqeust
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "stax", Order = 1)]
		public string Stax { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "type", Order = 2)]
		public string Type { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "sid", Order = 3)]
		public string Sid { get; set; }
	}
}
