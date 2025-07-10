using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class Report_GBCommercialInvoiceHeaders_CDS : Report_GbDeclarationsImport_CDS
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var colls = new List<ReportSchemaColumn>();

				foreach (var col in new[] { "JobNumber", "MessageType", "Box1DeclarationType", "Box1EntryType", "Box25TransportMode", "ContainerMode", "Box2SupplierCode",
											"Box2SupplierName", "Box8ImporterCode", "Box8ImporterName", "AllEntryNumbers", "PortOfLoading", "PortofDischarge", "Box15PortOfOrigin",
											"Box17PortOfDestination", "MasterUCR", "BranchCode", "BranchName", "CurrencyCode", "CurrencyName", "SupplierCode", "SupplierName",
											"ImporterCode", "ImporterName", "JZ_InvoiceNumber", "GrossWeightUnit", "NetWeightUnit", "JZ_IncoTerm", "JZ_PaymentNo", "Badge",
											"JZ_ValuationCode", "JZ_RelatedIndicator", "BuyerCode", "BuyerName", "ConsigneeCode", "ConsigneeName", "ExporterCode", "ExporterName",
											 "ManufacturerCode", "ManufacturerName", "SellerCode", "SellerName", "SoldToPartyCode", "SoldToPartyName", "BuyingAgentCode",
											"BuyingAgentName", "SellingAgentCode", "SellingAgentName", "JZ_InvoiceCurrExRateType", "JE_ApplicationCode" })
				{
					colls.Add(new ReportSchemaColumn(typeof(string), col));
				}

				foreach (var col in new[] { "JZ_InvoiceAmount", "JZ_InvoiceCurrExRate", "GrossWeight", "NetWeight", "JZ_NoOfPacks", "JZ_FOBValue",
											"JZ_PaymentAmount", "JZ_PaymentExRate", "DeclarationTotalDuty", "DeclarationTotalVAT" })
				{
					colls.Add(new ReportSchemaColumn(typeof(decimal), col));
				}

				foreach (var col in new[] { "CreatedTime", "JZ_InvoiceDate", "JZ_PaymentDate" })
				{
					colls.Add(new ReportSchemaColumn(typeof(DateTime), col));
				}

				foreach (var col in new[] { "JE_PK", "JZ_PK" })
				{
					colls.Add(new ReportSchemaColumn(typeof(Guid), col));
				}

				return colls;
			}
		}

		protected override List<string> ParametersValuesList => GetParametersValuesList("CDS", null);

		protected List<string> GetParametersValuesList(string applicationCode, string direction)
		{
			var parms = DeclarationReportTestHelper.GetParametersValuesListImport(applicationCode);
			parms[21] = (null == direction) ? "null" : "'" + direction + "'";  // 21 = @MessageType

			parms.AddRange(new List<string>
			{
				"null", // 30 @InvoiceNo
				"null", // 31 @InvoiceNoPartial
				"null", // 32 @InvoiceDateFrom
				"null", // 33 @InvoiceDateTo
				"null", // 34 @CurrencyCode
				"null", // 35 @IncoTerm
				"null", // 36 @InvoiceAmountFrom
				"null"  // 37 @InvoiceAmountTo
			});

			return parms;
		}

		protected override void PrepareTestData()
		{
			// Make sure you make the row to be included first, so that it gets the lowest/first B-job number
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var imp, isShipmentLinked: ShouldBeShipmentLinked);
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var imp2, isShipmentLinked: ShouldBeShipmentLinked);
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var exp);
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var exp2);

			ModifyImportJob(imp);
			AddInvoiceData(imp, 1);
			imp.JE_ApplicationCode = "CHF";

			ModifyImportJob(imp2);
			AddInvoiceData(imp2, 2);
			imp2.JE_ApplicationCode = "CDS";

			AddInvoiceData(exp, 3);
			exp.JE_ApplicationCode = "CDS";

			AddInvoiceData(exp2, 4);
			exp2.JE_ApplicationCode = "CHF";
		}

		void AddInvoiceData(JobDeclaration dec, int offset)
		{
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INVNR987" + offset;
			invoice.JZ_InvoiceDate = new ZDateTime(2017, 09, (14 + offset), 11, 33, 0);
			invoice.JZ_InvoiceAmount = 70400 + 100 * offset;
			invoice.JZ_RX_NKInvoice_Currency = testCurrencyCodes[offset];
			invoice.JZ_InvoiceCurrExRateType = "FIX";
			invoice.JZ_InvoiceCurrExRate = 0.5 + offset;
			invoice.JZ_Weight = 27500 + offset;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_NetWeight = 16400 + offset;
			invoice.JZ_NetWeightUQ = "LB";
			invoice.JZ_NoOfPacks = 200 + offset;
			invoice.JZ_IncoTerm = testIncoTerms[offset];
			invoice.JZ_FOBValue = 130000 + offset;
			invoice.JZ_PaymentNo = "PMTNO-234" + offset;
			invoice.JZ_PaymentAmount = 50000 + offset;
			invoice.JZ_PaymentDate = new ZDateTime(2017, 11, (16 + offset), 13, 44, 0);
			invoice.JZ_PaymentExRate = 0.3 + offset;
			invoice.JZ_ValuationCode = (0 + offset).ToString();
			invoice.JZ_RelatedIndicator = "N";

			if (1 == offset)
			{
				invoice.JZ_OH_Supplier = LinkOrg("Supplier", offset);
				invoice.JZ_OH_Buyer = LinkOrg("Importer", offset);
				invoice.JZ_OH_BuyerAgent = LinkOrg("BuyingAgent", offset);
				invoice.JZ_OH_SellingAgent = LinkOrg("SellingAgnt", offset);

				invoice.JZ_OA_BuyerAddress = LinkAddress("Buyer", offset);
				invoice.JZ_OA_ConsigneeAddress = LinkAddress("Consignee", offset);
				invoice.JZ_OA_ExporterAddress = LinkAddress("Exporter", offset);
				invoice.JZ_OA_ManufacturerAddress = LinkAddress("Manufactur", offset);
				invoice.JZ_OA_SellerAddress = LinkAddress("Seller", offset);
				invoice.JZ_OA_SoldToPartyAddress = LinkAddress("SoldToParty", offset);
			}
		}

		readonly List<string> testCurrencyCodes = new List<string>() { "CNY", "ZAR", "USD", "GBP", "JPY" };

		readonly List<string> testIncoTerms = new List<string>() { "FCA", "FOB", "EXW", "CIF", "CFR" };

		ZGuid LinkAddress(string role, int offset)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = role + offset;
			org.OH_FullName = role + " Name " + offset;

			var addr = org.Addresses.AddNew();
			addr.Address1 = "21 Long Street";
			return addr.PK;
		}

		ZGuid LinkOrg(string role, int offset)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = role + offset;
			org.OH_FullName = role + " Name " + offset;
			return org.PK;
		}

		protected override bool ShouldBeShipmentLinked => false;

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override ZString ObjectName => "Report_GBCommercialInvoiceHeaders";

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(2, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001001'", row1);
			var row2 = FormatRowsValues(results.Rows[1], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001002'", row2);
		}
	}

	class Report_GBCommercialInvoiceHeaders_CDS_Export : Report_GBCommercialInvoiceHeaders_CDS
	{
		protected override List<string> ParametersValuesList => GetParametersValuesList("CDS", "EXP");

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(1, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001002'", row1);
		}
	}
}
