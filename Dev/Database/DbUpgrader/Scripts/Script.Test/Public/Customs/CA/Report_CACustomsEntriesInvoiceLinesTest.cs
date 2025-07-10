using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(Report_CACustomsEntriesInvoiceLines))]
	class Report_CACustomsEntriesInvoiceLinesTest : DbCreateScriptTest
	{
		public void TestDuty()
		{
			var importerPK = TestDataCreator.CreateOrganisation("ACETESPHL", "ACE TEST IMPORTER 1", "USPHL");
			var declaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "IMP", 1, importerPK: importerPK);
			var cc_pk = TestDataCreator.CreateCusClassification(lookupCode: "ABC", countryCode: "NL", classificationType: "IMP");

			var invoice1 = TestDataCreator.CreateJobComInvoiceHeader(declaration1, 1);
			var entryHeader11 = TestDataCreator.CreateCusEntryHeader(declaration1, 1, messageType: "REL");
			var entryLine111 = TestDataCreator.CreateCusEntryLine(entryHeader11, 1);
			var entryHeader12 = TestDataCreator.CreateCusEntryHeader(declaration1, 1, messageType: "CAD");
			var entryLine121 = TestDataCreator.CreateCusEntryLine(entryHeader12, 1);
			var entryLine122 = TestDataCreator.CreateCusEntryLine(entryHeader12, 2);
			var invoiceLine11 = TestDataCreator.CreateJobComInvoiceLine(lineNo: 11, partNo: 1, description: "First invoice line", invoiceQuantity: 100, invoiceUQ: "CAD", customsQuantity: 101, customsUnitQty: "PKG", linePrice: 10, tariff: 2, jobComInvoiceHeaderPK: invoice1, cl_pk: entryLine111, cc_pk: cc_pk, clusterKey: 1);
			var invoiceLine12 = TestDataCreator.CreateJobComInvoiceLine(lineNo: 12, partNo: 2, description: "Second invoice line", invoiceQuantity: 100, invoiceUQ: "CAD", customsQuantity: 101, customsUnitQty: "PKG", linePrice: 20, tariff: 1, jobComInvoiceHeaderPK: invoice1, cl_pk: entryLine111, cc_pk: cc_pk, clusterKey: 1);
			var invoiceLine13 = TestDataCreator.CreateJobComInvoiceLine(lineNo: 13, partNo: 3, description: "Third invoice line", invoiceQuantity: 100, invoiceUQ: "CAD", customsQuantity: 101, customsUnitQty: "PKG", linePrice: 35, tariff: 3, jobComInvoiceHeaderPK: invoice1, cl_pk: entryLine111, cc_pk: cc_pk, clusterKey: 1);
			TestDataCreator.CreateCusUnderBondDec(entryLine121, invoiceLine11, 1);
			TestDataCreator.CreateCusUnderBondDec(entryLine121, invoiceLine12, 1);
			TestDataCreator.CreateCusUnderBondDec(entryLine122, invoiceLine13, 1);
			TestDataCreator.CreateCusEntryLineFee(entryLine121, "DTY", 3f, 1);
			TestDataCreator.CreateCusEntryLineFee(entryLine122, "XXX", 1f, 1);

			var declaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000002", "IMP", 2, importerPK: importerPK);
			var invoice2 = TestDataCreator.CreateJobComInvoiceHeader(declaration2, 2);
			var entryHeader21 = TestDataCreator.CreateCusEntryHeader(declaration2, 2, messageType: "REL");
			var entryLine211 = TestDataCreator.CreateCusEntryLine(entryHeader21, 2);
			var entryHeader22 = TestDataCreator.CreateCusEntryHeader(declaration2, 2, messageType: "B3C");
			var entryLine221 = TestDataCreator.CreateCusEntryLine(entryHeader22, 2);
			var invoiceLine21 = TestDataCreator.CreateJobComInvoiceLine(lineNo: 21, partNo: 1, description: "First invoice line", invoiceQuantity: 100, invoiceUQ: "CAD", customsQuantity: 101, customsUnitQty: "PKG", linePrice: 15, tariff: 1, jobComInvoiceHeaderPK: invoice2, cl_pk: entryLine211, cc_pk: cc_pk, clusterKey: 2);
			var invoiceLine22 = TestDataCreator.CreateJobComInvoiceLine(lineNo: 22, partNo: 2, description: "Second invoice line", invoiceQuantity: 100, invoiceUQ: "CAD", customsQuantity: 101, customsUnitQty: "PKG", linePrice: 25, tariff: 2, jobComInvoiceHeaderPK: invoice2, cl_pk: entryLine211, cc_pk: cc_pk, clusterKey: 2);
			TestDataCreator.CreateCusUnderBondDec(entryLine221, invoiceLine21, 2);
			TestDataCreator.CreateCusUnderBondDec(entryLine221, invoiceLine22, 2);
			TestDataCreator.CreateCusEntryLineFee(entryLine221, "DTY", 1f, 2);

			var declaration3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000003", "IMP", 3, importerPK: importerPK);
			var invoice3 = TestDataCreator.CreateJobComInvoiceHeader(declaration3, 3);
			var entryHeader31 = TestDataCreator.CreateCusEntryHeader(declaration3, 3, messageType: "REL");
			var entryLine311 = TestDataCreator.CreateCusEntryLine(entryHeader31, 3);
			var invoiceLine31 = TestDataCreator.CreateJobComInvoiceLine(lineNo: 31, partNo: 1, description: "First invoice line", invoiceQuantity: 100, invoiceUQ: "CAD", customsQuantity: 101, customsUnitQty: "PKG", linePrice: 15, tariff: 2, jobComInvoiceHeaderPK: invoice3, cl_pk: entryLine311, cc_pk: cc_pk, clusterKey: 3);

			double[] expectedDuties = [1, 2, 0, 0.375, 0.625, 0];
			var sql = @"SELECT Duty FROM Report_CACustomsEntriesInvoiceLines(@CompanyPK, '', '')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				var dutyCollection = new List<double>();
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						dutyCollection.Add(Convert.ToDouble(reader["Duty"]));
					}
					AssertArrayEqualsByElements("Duty", expectedDuties, dutyCollection.ToArray());
				}
			}
		}

		Guid companyPK;
		Guid branchPK;

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("DCA", "CA", "CAD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "BLO", "CABLO");
		}
	}
}
