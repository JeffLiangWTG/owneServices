using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_InvoiceLinesReportDeclaration))]
	class Report_InvoiceLinesReportDeclarationTest : DbCreateScriptTest
	{
		public void TestFunctionalityDbFunction()
		{
			var gc_pk = TestDataCreator.CreateCompany("NLD", "NL", "EUR");
			var gb_pk = TestDataCreator.CreateBranch(gc_pk, "NL1", "");

			var je_importer = TestDataCreator.CreateOrganisation("JEIMPORTER", "JEIMPORTERNAME");
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
				goodsDescription: "DescriptionOfTheGoods",
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
			var ce2Pk = TestDataCreator.CreateCusEntryNum(ch_pk, "CusEntryHeader", "UCRFROMCUSENTRYNUMBER00001", "IMP", "CUS", "NL");

			var jp_pk = TestDataCreator.CreateJobDocsAndCartage(je_pk, "JE");
			var jt_pk = TestDataCreator.CreateJobOrderItem(jp_pk, "OWNREF1");

			var jlt_pk1 = TestDataCreator.CreateJobComInvoiceLineTax(100, 21, invoiceLine1, "A", clusterKey);
			var jlt_pk2 = TestDataCreator.CreateJobComInvoiceLineTax(50, 6, invoiceLine2, "A", clusterKey);
			var jlt_pk3 = TestDataCreator.CreateJobComInvoiceLineTax(75, 12, invoiceLine3, "A", clusterKey);

			using (var command = CargoWise.Data.Db.Connection.Command(
				$@"SELECT * FROM Report_InvoiceLinesReportDeclaration ('{je_pk}')"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", true, reader.Read());

						AssertEquals("JobNumber", "B00000500", reader["JobNumber"]);
						AssertEquals("CustomsEntryNumber", "AAAANGWMT", reader["CustomsEntryNumber"]);
						AssertEquals("DeclarationSequence", "UCRFROMCUSENTRYNUMBER00001", reader["DeclarationSequence"]);
						AssertEquals("ImporterName", "JEIMPORTERNAME", reader["ImporterName"]);
						AssertEquals("Transport", "MSC123/NL123/lloyds1", reader["Transport"]);
						AssertEquals("MasterBill", "OOCL12345678", reader["MasterBill"]);
						AssertEquals("GoodsDescription", "DescriptionOfTheGoods", reader["GoodsDescription"]);
						AssertEquals("HouseBill", "S00049567", reader["HouseBill"]);
						AssertEquals("TransportMode", "SEA", reader["TransportMode"]);
					});
				}
			}
		}
	}
}
