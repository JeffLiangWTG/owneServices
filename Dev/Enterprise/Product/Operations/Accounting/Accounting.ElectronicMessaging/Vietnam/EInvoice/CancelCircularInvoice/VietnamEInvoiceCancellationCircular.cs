using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	[DataContract]
	public class VietnamEInvoiceCancellationCircular
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "lang", Order = 1)]
		public string Language { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "user", Order = 2)]
		public User User { get; set; } = new User();

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "wrongnotice", Order = 3)]
		public WrongNotice Wrongnotice { get; set; } = new WrongNotice();
	}

	[DataContract]
	public class WrongNotice
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "stax", Order = 1)]
		public string Stax { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "noti_taxtype", Order = 2)]
		public string Taxtype { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "noti_taxnum", Order = 3)]
		public string Taxnum { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "noti_taxdt", Order = 4)]
		public string Taxdt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "budget_relationid", Order = 5)]
		public string BudgetRelationid { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "place", Order = 6)]
		public string Place { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "items", Order = 7)]
		public List<ItemForCancelCir78> Items => items ?? (items = new List<ItemForCancelCir78>());
		List<ItemForCancelCir78> items;
	}

	[DataContract]
	public class ItemForCancelCir78
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "form", Order = 1)]
		public string Form { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "serial", Order = 2)]
		public string Serial { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "seq", Order = 3)]
		public string Seq { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "idt", Order = 4)]
		public string Idt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "type_ref", Order = 5)]
		public int TypeRef { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "noti_type", Order = 6)]
		public string NotiType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "rea", Order = 7)]
		public string Rea { get; set; }
	}
}
