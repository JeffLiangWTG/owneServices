using System;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class VATReportFee
	{
		[VATReportReturnedParameter]
		[ResourceStringData("3C3D820B-01CE-4258-A18A-DBA336E1C3C8", Caption = "Job Number")]
		public string JobNumber { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("FA2F1B8E-CC6C-488C-BBA7-03AA267FE231", Caption = "DUCR")]
		public string DUCR { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("53084E1B-C01D-4BDC-AD01-62BB07089BCB", Caption = "Entry Number")]
		public string EntryNumber { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("65FFB956-25D4-4D0E-8E13-BBCFE7511B4E", Caption = "Entry Reference")]
		public string EntryReference { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("97D9684A-CB44-4D07-B322-6CA724D4C01C", Caption = "BAE Date")]
		public DateTime BAEDate { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("E33629A8-5476-4AB7-B301-3402A7550959", Caption = "CPC")]
		public string CPC { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("B629597F-F5BE-4F9C-8A26-010EA0FA2E50", Caption = "Country Of Supply")]
		public string CountriesOfSupply { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("622339C8-594E-423D-ABA7-8FDF062470DE", Caption = "Invoice Number")]
		public string InvoiceNumbers { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("7AB7DD4A-ABD8-48C2-9804-502AAF847F4E", Caption = "Invoice Amount")]
		public decimal TotalInvoiceAmount { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("944E0D76-7E75-4C2C-99D6-A2C5EBFEC755", Caption = "Currency")]
		public string Currency { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("144CD351-A6F2-4207-9C32-EE1E1D06BBBD", Caption = "Rate")]
		public decimal Rate { get; set; }

		[ResourceStringData("6A4CFA8F-9B21-4999-A588-E630B7728E5C", Caption = "Importer Code")]
		public string ImporterCode { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("8DC40D86-1FDD-4C61-80B2-25463FD05B1B", Caption = "Importer")]
		public string ImporterName { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("1FDB577B-CE69-46C3-A0E5-F29EB506E0A4", Caption = "Supplier")]
		public string SupplierName { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("83829CC8-78C9-42CE-AC54-B84FAD8CA604", Caption = "EORI")]
		public string EORI { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("8D9D6598-0CD1-467F-9313-4B0B19A7512C", Caption = "VAT Number")]
		public string VATNumber { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("9D248BBC-598E-4604-949B-7AC208E6F5B2", Caption = "VAT Base Amount")]
		public decimal TotalVATBaseAmount { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("0D241B76-EEE6-4BE1-9BDC-8039AAAED3D2", Caption = "VAT Amount")]
		public decimal TotalVATAmount { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("09A1BD1A-7510-4F8B-B97A-A056CFC58B0A", Caption = "VAT Procedure")]
		public string VATProcedureCode { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("82E82146-9954-414D-8EA3-36875CAE61A8", Caption = "VAT Procedure Description")]
		public string VATProcedureDescription { get; set; }

		[VATReportReturnedParameter]
		[ResourceStringData("DED263A0-AD64-4E31-A94D-62BF1B74E159", Caption = "Order Refs")]
		public string OrderRefs { get; set; }
	}
}
