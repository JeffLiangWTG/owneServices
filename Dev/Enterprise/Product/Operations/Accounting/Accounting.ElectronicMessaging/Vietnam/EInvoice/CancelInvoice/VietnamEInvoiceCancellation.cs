using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	[DataContract]
	public class VietnamEInvoiceCancellation
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "lang", Order = 1)]
		public string Language { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "user", Order = 2)]
		public User User { get; set; } = new User();

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "inv", Order = 3)]
		public InvForCancel Inv { get; set; } = new InvForCancel();
	}

	[DataContract]
	public class InvForCancel
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "seq", Order = 1)]
		public string Seq { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "adj", Order = 2)]
		public Adj Adj { get; set; }
	}

	[DataContract]
	public class Adj
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "rdt", Order = 1)]
		public string Rdt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "rea", Order = 2)]
		public string Rea { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "ref", Order = 3)]
		public string Ref { get; set; }
	}
}
