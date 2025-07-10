using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice
{
	[DataContract]
	public class VietnamEInvoiceAdjustment
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "user", Order = 1)]
		public User User { get; set; } = new User();

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "lang", Order = 2)]
		public string Language { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "inv", Order = 3)]
		public Inv Inv { get; set; } = new Inv();
	}

	[DataContract]
	public class Inv
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "adj", Order = 1)]
		public Adj Adj { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "aun", Order = 2)]
		public int Aun { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "sid", Order = 3)]
		public string Sid { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "idt", Order = 4)]
		public string Idt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "type", Order = 5)]
		public string Type { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "form", Order = 6)]
		public string Form { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "serial", Order = 7)]
		public string Serial { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "ud", Order = 8)]
		public int Ud { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "seq", Order = 9)]
		public string Seq { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "bname", Order = 10)]
		public string Bname { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "buyer", Order = 11)]
		public string Buyer { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "btax", Order = 12)]
		public string Btax { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "baddr", Order = 13)]
		public string Baddr { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "btel", Order = 14)]
		public string Btel { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bmail", Order = 15)]
		public string Bmail { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "paym", Order = 16)]
		public string Paym { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "curr", Order = 17)]
		public string Curr { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "exrt", Order = 18)]
		public decimal Exrt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bacc", Order = 19)]
		public string Bacc { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bbank", Order = 20)]
		public string Bbank { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "note", Order = 21)]
		public string Note { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "sumv", Order = 22)]
		public decimal Sumv { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "sum", Order = 23)]
		public decimal Sum { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "vatv", Order = 24)]
		public decimal Vatv { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "vat", Order = 25)]
		public decimal Vat { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "word", Order = 26)]
		public string Word { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "totalv", Order = 27)]
		public decimal Totalv { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "discount", Order = 28)]
		public string Discount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "total", Order = 29)]
		public decimal Total { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "type_ref", Order = 30)]
		public int TypeRef { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "sendtype", Order = 31)]
		public int SendType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "stax", Order = 32)]
		public string Stax { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "items", Order = 33)]

		public List<AdjustItem> Items => items ?? (items = new List<AdjustItem>());
		List<AdjustItem> items;

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c0", Order = 34)]
		public string DueDate { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c1", Order = 35)]
		public string Consignor { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c2", Order = 36)]
		public string Reference { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c3", Order = 37)]
		public string Consignee { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c4", Order = 38)]
		public string ETD { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c5", Order = 39)]
		public string VoyageFlightJourneyDetails { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c6", Order = 40)]
		public string ETA { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c7", Order = 41)]
		public string MasterBill { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c8", Order = 42)]
		public string HouseBill { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c9", Order = 43)]
		public string AdditionalInfoC9 { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c10", Order = 44)]
		public string AdditionalInfoC10 { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c11", Order = 45)]
		public string AdditionalInfoC11 { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c12", Order = 46)]
		public string AdditionalInfoC12 { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "c13", Order = 47)]
		public string AdditionalInfoC13 { get; set; }
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

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "seq", Order = 4)]
		public string Seq { get; set; }
	}

	[DataContract]
	public class AdjustItem
	{
		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "type", Order = 1)]
		public string Type { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "vrt", Order = 2)]
		public string VRT { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "name", Order = 3)]
		public string Name { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "unit", Order = 4)]
		public string Unit { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "amount", Order = 5)]
		public decimal Amount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "amountv", Order = 6)]
		public decimal LocalAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "price", Order = 7)]
		public decimal Price { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "quantity", Order = 8)]
		public decimal Quantity { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "vat", Order = 9)]
		public decimal VAT { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "vatv", Order = 10)]
		public decimal LocalTax { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "total", Order = 11)]
		public decimal Total { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "totalv", Order = 12)]
		public decimal LocalTotal { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "status", Order = 13)]
		public int Status { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c0", Order = 14)]
		public string CustomizedLocalTax { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c1", Order = 15)]
		public string CustomizedLocalTotal { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c2", Order = 16)]
		public string CustomizedLocalAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c3", Order = 17)]
		public string HouseBill { get; set; }
	}
}
