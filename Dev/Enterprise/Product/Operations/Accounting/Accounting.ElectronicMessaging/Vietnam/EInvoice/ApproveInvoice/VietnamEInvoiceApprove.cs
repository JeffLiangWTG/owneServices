using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice
{
	[DataContract]
	public class VietnamEInvoiceApprove
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "user", Order = 1)]
		public UserForApprove User { get; set; } = new UserForApprove();

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "inv", Order = 2)]
		public InvForApprove Inv { get; set; } = new InvForApprove();
	}

	[DataContract]
	public class UserForApprove
	{
		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "username", Order = 1)]
		public string Username { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "password", Order = 2)]
		public string Password { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "lang", Order = 3)]
		public string Lang { get; set; }
	}

	[DataContract]
	public class InvForApprove
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "sid", Order = 1)]
		public string Sid { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "form", Order = 2)]
		public string Form { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "serial", Order = 3)]
		public string Serial { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "seq", Order = 4)]
		public string Seq { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "stax", Order = 5)]
		public string Stax { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "sendfile", Order = 6)]
		public int SendFile { get; set; }
	}
}
