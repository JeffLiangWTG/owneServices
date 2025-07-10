using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	[DataContract]
	public class VietnamEInvoice
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "user", Order = 1)]
		public User User { get; set; } = new User();

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "lang", Order = 2)]
		public string Language { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "inv", Order = 3)]
		public Inv Inv { get; set; } = new Inv();
	}

	[DataContract]
	public class User
	{
		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "username", Order = 1)]
		public string Username { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "password", Order = 2)]
		public string Password { get; set; }
	}

	[DataContract]
	public class Inv
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "sid", Order = 1)]
		public string Sid { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "idt", Order = 2)]
		public string Idt { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "type", Order = 3)]
		public string Type { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "form", Order = 4)]
		public string Form { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "serial", Order = 5)]
		public string Serial { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "seq", Order = 6)]
		public string Seq { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "aun", Order = 7)]
		public int Aun { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bcode", Order = 8)]
		public string Bcode { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "bname", Order = 9)]
		public string Bname { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "buyer", Order = 10)]
		public string Buyer { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "btax", Order = 11)]
		public string Btax { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "baddr", Order = 12)]
		public string Baddr { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "btel", Order = 13)]
		public string Btel { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bmail", Order = 14)]
		public string Bmail { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "sendfile", Order = 15)]
		public int Sendfile { get; set; }

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

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "items", Order = 32)]
		public List<Item> Items => items ?? (items = new List<Item>());
		List<Item> items;

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "stax", Order = 33)]
		public string Stax { get; set; }

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
	public class Item
	{
		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "type", Order = 1)]
		public string Type { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "vrt", Order = 2)]
		public string VRT { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "line", Order = 3)]
		public int Line { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "name", Order = 4)]
		public string Name { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "unit", Order = 5)]
		public string Unit { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "amount", Order = 6)]
		public decimal Amount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = true, Name = "amountv", Order = 7)]
		public decimal LocalAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "price", Order = 8)]
		public decimal Price { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "quantity", Order = 9)]
		public decimal Quantity { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "vat", Order = 10)]
		public decimal VAT { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "vatv", Order = 11)]
		public decimal LocalTax { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "total", Order = 12)]
		public decimal Total { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "totalv", Order = 13)]
		public decimal LocalTotal { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c0", Order = 14)]
		public string CustomizedLocalTax { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c1", Order = 15)]
		public string CustomizedLocalTotal { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c2", Order = 16)]
		public string CustomizedLocalAmount { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "c3", Order = 17)]
		public string HouseBill { get; set; }
	}

	public class AdditionalInfo
	{
		public AdditionalInfoC9 additionalInfoC9 { get; set; }

		public AdditionalInfoC10 additionalInfoC10 { get; set; }

		public AdditionalInfoC11 additionalInfoC11 { get; set; }

		public AdditionalInfoC12 additionalInfoC12 { get; set; }

		public AdditionalInfoC13 additionalInfoC13 { get; set; }
	}

	[DataContract]
	public class AdditionalInfoC9
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Load Port", Order = 1)]
		public string Origin { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Discharge Port", Order = 2)]
		public string Destination { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Incoterm", Order = 3)]
		public string Incoterms { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Payment Term", Order = 4)]
		public string CreditTerms { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Sell Reference", Order = 5)]
		public string SellReference { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Transaction Number", Order = 6)]
		public string TransactionNumber { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Transport Mode", Order = 7)]
		public string TransportMode { get; set; }
	}

	[DataContract]
	public class AdditionalInfoC10
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Owner Reference", Order = 1)]
		public string OwnerReference { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Order Reference", Order = 2)]
		public string OrderReference { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Transaction Department", Order = 3)]
		public string InvoiceDepartmentDesc { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Job Department", Order = 4)]
		public string JobDepartmentDesc { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "External Debtor Code", Order = 5)]
		public string ExternalSystemDebtorCode { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Goods", Order = 6)]
		public string GoodDescription { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Consol ID", Order = 7)]
		public string ConsolID { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Package", Order = 8)]
		public string Package { get; set; }
	}

	[DataContract]
	public class AdditionalInfoC11
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Total Weight", Order = 1)]
		public string Weight { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Total Chargeable", Order = 2)]
		public string ChargeableWeight { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Total Volume", Order = 3)]
		public string Volume { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Sending Agent", Order = 4)]
		public string SendingAgent { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Receiving Agent", Order = 5)]
		public string ReceivingAgent { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Carrier", Order = 6)]
		public string LineCarrier { get; set; }
	}

	[DataContract]
	public class AdditionalInfoC12
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Operation Staff", Order = 1)]
		public string JobOperationStaff { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Invoicing Staff", Order = 2)]
		public string InvoiceCreatingStaff { get; set; }
	}

	[DataContract]
	public class AdditionalInfoC13
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "Sales Staff", Order = 23)]
		public string JobSalesStaff { get; set; }
	}
}

