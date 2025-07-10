using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_InvoiceLinesReport))]
	class Report_InvoiceLinesReportTest : DbCreateScriptTest
	{
		public void TestFunctionalityDbFunction()
		{
			var gc_pk = TestDataCreator.CreateCompany("NLD", "NL", "EUR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "NL1", "");

			var je_importer = TestDataCreator.CreateOrganisation("JEIMPORTER", "JE IMPORTER NAME");
			var je_supplier = TestDataCreator.CreateOrganisation("JESUPPLIER", "JE SUPPLIER NAME");

			var add1_pk = TestDataCreator.CreateAddress(je_importer, "ADD1", "ADDRESS 1");

			var clusterKey = 1;

			var rv_pk = TestDataCreator.CreateRefVessel(
				code: "MSC123",
				lloydsnumber: "lloyds1",
				clusterKey: clusterKey);

			var js_pk = TestDataCreator.CreateJobShipment();

			var jd_pk = TestDataCreator.CreateJobOrderHeader(
				shipment: js_pk,
				orderNumber: "OrderNumber",
				buyerAddress: add1_pk);

			var je_pk = TestDataCreator.CreateJobDeclaration(
				declarationReference: "B00000500",
				branchPK: gb_pk,
				companyPK: gc_pk,
				transportMode: "SEA",
				voyageFlightNo: "NL123",
				exportDate: new DateTime(2017, 1, 1, 12, 13, 0),
				masterBill: "OOCL12345678",
				goodsDescription: "Description of the goods",
				messageType: "IMP",
				ownerReference: "OwnerRef",
				houseBill: "S00049567",
				importer: je_importer,
				vessel: "MSC123",
				shipment: js_pk,
				clusterKey: clusterKey
				);

			var ch_pk = TestDataCreator.CreateCusEntryHeader(
				jePk: je_pk,
				clusterKey: clusterKey
			);

			var jz_pk = TestDataCreator.CreateJobComInvoiceHeader(
				invoiceCurrency: "EUR",
				defaultOrigin: "NL",
				invoiceNumber: "123",
				incoTerm: "CIP",
				supplierPK: je_supplier,
				jobDeclarationPK: je_pk,
				clusterKey: clusterKey
				);

			var cl_pk1 = TestDataCreator.CreateCusEntryLine(
				clCh: ch_pk,
				lineNumber: 1,
				dutyPercent: 2,
				flatAmount: 300,
				flatAmountUQ: "KG",
				clusterKey: clusterKey
				);

			var cl_pk2 = TestDataCreator.CreateCusEntryLine(
				clCh: ch_pk,
				lineNumber: 2,
				dutyPercent: 4,
				flatAmount: 600,
				flatAmountUQ: "KG",
				clusterKey: clusterKey
				);

			var cl_pk3 = TestDataCreator.CreateCusEntryLine(
				clCh: ch_pk,
				lineNumber: 3,
				dutyPercent: 7,
				flatAmount: 800,
				flatAmountUQ: "KG",
				clusterKey: clusterKey
				);

			var cc_pk = TestDataCreator.CreateCusClassification(
				lookupCode: "ABC",
				countryCode: "NL",
				classificationType: "IMP");

			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(
				lineNo: 1,
				partNo: 1,
				description: "First invoice line",
				invoiceQuantity: 100,
				invoiceUQ: "EUR",
				customsQuantity: 101,
				customsUnitQty: "EUR",
				linePrice: 45,
				tariff: 2,
				jobComInvoiceHeaderPK: jz_pk,
				cl_pk: cl_pk1,
				cc_pk: cc_pk,
				clusterKey: clusterKey);

			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(
				lineNo: 2,
				partNo: 2,
				description: "Second invoice line",
				invoiceQuantity: 100,
				invoiceUQ: "EUR",
				customsQuantity: 101,
				customsUnitQty: "EUR",
				linePrice: 10,
				tariff: 1,
				jobComInvoiceHeaderPK: jz_pk,
				cl_pk: cl_pk2,
				cc_pk: cc_pk,
				clusterKey: clusterKey);

			var invoiceLine3 = TestDataCreator.CreateJobComInvoiceLine(
				lineNo: 3,
				partNo: 3,
				description: "Third invoice line",
				invoiceQuantity: 100,
				invoiceUQ: "EUR",
				customsQuantity: 101,
				customsUnitQty: "EUR",
				linePrice: 100,
				tariff: 1,
				jobComInvoiceHeaderPK: jz_pk,
				cl_pk: cl_pk3,
				cc_pk: cc_pk,
				clusterKey: clusterKey);

			var ce1Pk = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", "AAAANGWMT", "MRN", "CUS", "NL");
			var ce2Pk = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", "UCRFROMCUSENTRYNUMBER00001", "UCR", "CUS", "NL");

			var jp_pk = TestDataCreator.CreateJobDocsAndCartage(je_pk, "JE");
			var jt_pk = TestDataCreator.CreateJobOrderItem(jp_pk, "OWNREF1");

			var jlt_pk1 = TestDataCreator.CreateJobComInvoiceLineTax(100, 21, invoiceLine1, "A", clusterKey);
			var jlt_pk2 = TestDataCreator.CreateJobComInvoiceLineTax(50, 6, invoiceLine2, "A", clusterKey);
			var jlt_pk3 = TestDataCreator.CreateJobComInvoiceLineTax(75, 12, invoiceLine3, "A", clusterKey);

			using (var command = CargoWise.Data.Db.Connection.Command(
				$@"SELECT * FROM Report_InvoiceLinesReport ('1,3', '{je_pk}')"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", true, reader.Read());

						AssertEquals("LineNumber", (short)1, reader["LineNumber"]);
						AssertEquals("MergedLineNumber", "1", reader["MergedLineNumber"]);
						AssertEquals("partno", "1", reader["partno"]);
						AssertEquals("LineDescription", "First invoice line", reader["LineDescription"]);
						AssertEquals("Invoiceqty", 100m, reader["Invoiceqty"]);
						AssertEquals("invoiceUQ", "EUR", reader["invoiceUQ"]);
						AssertEquals("CustomsQty", 101m, reader["CustomsQty"]);
						AssertEquals("customsunitqty", "EUR", reader["customsunitqty"]);
						AssertEquals("LinePrice", 45m, reader["LinePrice"]);
						AssertEquals("InvoiceCurr", "EUR", reader["InvoiceCurr"]);
						AssertEquals("RefCountryCode", "NL", reader["RefCountryCode"]);
						AssertEquals("TariffLookupCode", "ABC", reader["TariffLookupCode"]);
						AssertEquals("ClassificationDetails", "2", reader["ClassificationDetails"]);
						AssertEquals("GSTRate", 21m, reader["GSTRate"]);
						AssertEquals("InvoiceNumber", "123", reader["InvoiceNumber"]);
						AssertEquals("SupplierCode", "JESUPPLIER", reader["SupplierCode"]);
						AssertEquals("SupplierName", "JE SUPPLIER NAME", reader["SupplierName"]);
						AssertEquals("IncoTerm", "CIP", reader["IncoTerm"]);

						AssertEquals("Result must contain at least 2 entries", true, reader.Read());

						AssertEquals("LineNumber", (short)3, reader["LineNumber"]);
						AssertEquals("MergedLineNumber", "3", reader["MergedLineNumber"]);
						AssertEquals("partno", "3", reader["partno"]);
						AssertEquals("LineDescription", "Third invoice line", reader["LineDescription"]);
						AssertEquals("Invoiceqty", 100m, reader["Invoiceqty"]);
						AssertEquals("invoiceUQ", "EUR", reader["invoiceUQ"]);
						AssertEquals("CustomsQty", 101m, reader["CustomsQty"]);
						AssertEquals("customsunitqty", "EUR", reader["customsunitqty"]);
						AssertEquals("LinePrice", 100m, reader["LinePrice"]);
						AssertEquals("InvoiceCurr", "EUR", reader["InvoiceCurr"]);
						AssertEquals("RefCountryCode", "NL", reader["RefCountryCode"]);
						AssertEquals("TariffLookupCode", "ABC", reader["TariffLookupCode"]);
						AssertEquals("ClassificationDetails", "1", reader["ClassificationDetails"]);
						AssertEquals("GSTRate", 16m, reader["GSTRate"]);
						AssertEquals("InvoiceNumber", "123", reader["InvoiceNumber"]);
						AssertEquals("SupplierCode", "JESUPPLIER", reader["SupplierCode"]);
						AssertEquals("SupplierName", "JE SUPPLIER NAME", reader["SupplierName"]);
						AssertEquals("IncoTerm", "CIP", reader["IncoTerm"]);

						AssertEquals("Result must not contain more than 2 entries", false, reader.Read());
					});
				}
			}
		}
	}
}
