using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	[DataContract]
	public class ChinaEInvoice
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "reqType", Order = 1)]
		public string ReqType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "taxNo", Order = 2)]
		public string TaxNo { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "clientNo", Order = 3)]
		public string ClientNo { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "autoexec", Order = 4)]
		public string Autoexec { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "data", Order = 5)]
		public Data Data { get; set; } = new Data();
	}

	[DataContract]
	public class Data
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "serialNumber", Order = 1)]
		public string SerialNumber { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "extend", Order = 2)]
		public string Extend { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "invType", Order = 3)]
		public string InvType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "version", Order = 4)]
		public string Version { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "drawer", Order = 5)]
		public string Drawer { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "payee", Order = 6)]
		public string Payee { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "reviewer", Order = 7)]
		public string Reviewer { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "seller", Order = 8)]
		public Seller Seller = new Seller();

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "order", Order = 9)]
		public Order Order = new Order();
	}

	[DataContract]
	public class Seller
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "identifier", Order = 1)]
		public string Identifier { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "name", Order = 2)]
		public string Name { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "address", Order = 3)]
		public string Address { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "telephoneNo", Order = 4)]
		public string TelephoneNo { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bank", Order = 5)]
		public string Bank { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "bankAcc", Order = 6)]
		public string BankAcc { get; set; }
	}

	[DataContract]
	public class Order
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "orderNo", Order = 1)]
		public string OrderNo { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "invoiceList", Order = 2)]
		public string InvoiceList { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "invoiceSplit", Order = 3)]
		public string InvoiceSplit { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "invoiceSfdy", Order = 4)]
		public string InvoiceSfdy { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "orderDate", Order = 5)]
		public string OrderDate { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "chargeTaxWay", Order = 6)]
		public string ChargeTaxWay { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "totalAmount", Order = 7)]
		public string TotalAmount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "taxMark", Order = 8)]
		public string TaxMark { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "totalDiscount", Order = 9)]
		public string TotalDiscount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "remark", Order = 10)]
		public string Remark { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "extractedCode", Order = 11)]
		public string ExtractedCode { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "buyer", Order = 12)]
		public Buyer Buyer = new Buyer();

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "orderDetails", Order = 13)]
		public List<OrderDetail> OrderDetails => orderDetails ?? (orderDetails = new List<OrderDetail>());

		List<OrderDetail> orderDetails;
	}

	[DataContract]
	public class Buyer
	{
		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "customerType", Order = 1)]
		public string CustomerType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "identifier", Order = 2)]
		public string Identifier { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "name", Order = 3)]
		public string Name { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "address", Order = 4)]
		public string Address { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "telephoneNo", Order = 5)]
		public string TelephoneNo { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "bank", Order = 6)]
		public string Bank { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "bankAcc", Order = 7)]
		public string BankAcc { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "email", Order = 8)]
		public string Email { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "memberId", Order = 9)]
		public string MemberId { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "isSend", Order = 10)]
		public string IsSend { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "recipient", Order = 11)]
		public string Recipient { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "reciAddress", Order = 12)]
		public string ReciAddress { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "zip", Order = 13)]
		public string Zip { get; set; }
	}

	[DataContract]
	public class OrderDetail
	{
		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "venderOwnCode", Order = 1)]
		public string VenderOwnCode { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "productCode", Order = 2)]
		public string ProductCode { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "productName", Order = 3)]
		public string ProductName { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "rowType", Order = 4)]
		public string RowType { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "spec", Order = 5)]
		public string Spec { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "unit", Order = 6)]
		public string Unit { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "quantity", Order = 7)]
		public string Quantity { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "unitPrice", Order = 8)]
		public string UnitPrice { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "amount", Order = 9)]
		public string Amount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "deductAmount", Order = 10)]
		public string DeductAmount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "taxRate", Order = 11)]
		public string TaxRate { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "taxAmount", Order = 12)]
		public string TaxAmount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = true, Name = "mxTotalAmount", Order = 13)]
		public string MxTotalAmount { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "taxRateMark", Order = 14)]
		public string TaxRateMark { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "policyMark", Order = 15)]
		public string PolicyMark { get; set; }

		[DataMember(EmitDefaultValue = true, IsRequired = false, Name = "policyName", Order = 16)]
		public string PolicyName { get; set; }
	}
}
