using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.DE;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.DE.Testing
{
	[TestedType(typeof(Report_DEMonthlyClosingDutyTaxReport))]
	class Report_DEMonthlyClosingDutyTaxReportTest : DbCreateScriptTest
	{
		public void TestReport()
		{
			CombineAssertions(() =>
			{
				var sql =
					@"select PeriodFrom, PeriodTo, JobNumber, Duty, Vat, OtherLevies
from dbo.Report_DEMonthlyClosingDutyTaxReport (@branchPK, @dateFrom, @dateTo, @jobNumber)";
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPK);
					command.AddParameter("@dateFrom", SqlDbType.DateTime, periodFrom.AddDays(-1));
					command.AddParameter("@dateTo", SqlDbType.DateTime, periodTo.AddDays(1).AddSeconds(1));
					command.AddParameter("@jobNumber", SqlDbType.VarChar, DBNull.Value);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var from = (DateTime)reader["PeriodFrom"];
							var to = (DateTime)reader["PeriodTo"];
							var jobNumber = (string)reader["JobNumber"];
							var duty = (decimal)reader["Duty"];
							var vat = (decimal)reader["Vat"];
							var other = (decimal)reader["OtherLevies"];
							AssertLessThanOrEqualTo("Should filter on period from date", periodFrom.AddDays(-1), from);
							AssertGreaterThanOrEqualTo("Should filter on period to date", periodTo.AddDays(1), to);
							AssertEquals("duty", 25m, duty);
							AssertEquals("vat", 35m, vat);
							AssertEquals("other", 36m, other);
						}
					}
				}
			});
		}

		public void TestReport_WithJobNumber()
		{
			CombineAssertions(() =>
			{
				var sql =
					@"select PeriodFrom, PeriodTo, JobNumber, Duty, Vat, OtherLevies
from dbo.Report_DEMonthlyClosingDutyTaxReport (@branchPK, @dateFrom, @dateTo, @jobNumber)";
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPK);
					command.AddParameter("@dateFrom", SqlDbType.DateTime, periodFrom);
					command.AddParameter("@dateTo", SqlDbType.DateTime, periodTo.AddSeconds(1));
					command.AddParameter("@jobNumber", SqlDbType.VarChar, "B007");
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var jobNumber = (string)reader["JobNumber"];
							AssertEquals("Should filter on job number", "B007", jobNumber);
						}
					}
				}
			});
		}

		public void TestReport_NoDuplicateEntries()
		{
			var clusterKey = 1;
			var testHelper = new TestDbHelper(Db.Connection);
			var jobDeclaration = CreateJobDeclaration(testHelper, branchPK, companyPK, clusterKey);

			var header1 = CreateCusEntryHeader(testHelper, jobDeclaration, clusterKey);
			var header2 = CreateCusEntryHeader(testHelper, jobDeclaration, clusterKey);
			var crd = CreateCusReconDeclaration(testHelper, periodFrom, periodTo, "B019", branchPK);

			TestDataCreator.CreateCusEntryNum(crd, CusReconDeclarationSchema.Constants.TableName, "Test002", "MRN",
			"CUS",
			"DE");

			var recon1 = CreateCusReconEntry(testHelper, crd, addressPK, branchPK, "", header1, periodFrom.AddDays(1));
			var recon2 = CreateCusReconEntry(testHelper, crd, addressPK, branchPK, "", header2, periodFrom.AddDays(1));

			var line1 = CreateCusEntryLine(testHelper, header1, clusterKey, 1);
			var line2 = CreateCusEntryLine(testHelper, header2, clusterKey, 1);

			var invoiceHeader = CreateInvoiceHeader(testHelper, jobDeclaration, clusterKey, "INV1", new DateTime(2020, 10, 01));
			var invoiceLine1 = CreateInvoiceLine(testHelper, invoiceHeader, line1, clusterKey, 10);
			var invoiceLine2 = CreateInvoiceLine(testHelper, invoiceHeader, line2, clusterKey, 20);

			var reconLine1 = CreateCusReconEntryLine(recon1, originalLineNumber: 1, lineNumber: 1);
			var reconLine2 = CreateCusReconEntryLine(recon2, originalLineNumber: 1, lineNumber: 2);

			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 10f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 20f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line1, "C00", 21f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "C01", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "C00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 12f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 13f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 21f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 14f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "C00", 22f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C02", 2f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C01", 12f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C00", 11f, clusterKey);

			CombineAssertions(() =>
			{
				var sql =
					@"select LineNum, EgzLineNo, PeriodFrom, PeriodTo, JobNumber, Duty, Vat, OtherLevies
from dbo.Report_DEMonthlyClosingDutyTaxReport (@branchPK, @dateFrom, @dateTo, @jobNumber)
order by EgzLineNo";
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPK);
					command.AddParameter("@dateFrom", SqlDbType.DateTime, periodFrom.AddDays(-1));
					command.AddParameter("@dateTo", SqlDbType.DateTime, periodTo.AddDays(1).AddSeconds(1));
					command.AddParameter("@jobNumber", SqlDbType.VarChar, "B019");

					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						var lineNum = (short)reader["LineNum"];
						var egzLineNo = (short)reader["EgzLineNo"];
						var from = (DateTime)reader["PeriodFrom"];
						var to = (DateTime)reader["PeriodTo"];
						var duty = (decimal)reader["Duty"];
						var vat = (decimal)reader["Vat"];
						var other = (decimal)reader["OtherLevies"];
						AssertEquals("lineNum", (short)1, lineNum);
						AssertEquals("egzLineNo", (short)1, egzLineNo);
						AssertLessThanOrEqualTo("Should filter on period from date", periodFrom.AddDays(-1), from);
						AssertGreaterThanOrEqualTo("Should filter on period to date", periodTo.AddDays(1), to);
						AssertEquals("duty", 25m, duty);
						AssertEquals("vat", 35m, vat);
						AssertEquals("other", 36m, other);

						AssertEquals("A second row should be returned", true, reader.Read());

						lineNum = (short)reader["LineNum"];
						egzLineNo = (short)reader["EgzLineNo"];
						from = (DateTime)reader["PeriodFrom"];
						to = (DateTime)reader["PeriodTo"];
						duty = (decimal)reader["Duty"];
						vat = (decimal)reader["Vat"];
						other = (decimal)reader["OtherLevies"];
						AssertEquals("Second lineNum", (short)1, lineNum);
						AssertEquals("Second egzLineNo", (short)2, egzLineNo);
						AssertLessThanOrEqualTo("Should filter on period from date", periodFrom.AddDays(-1), from);
						AssertGreaterThanOrEqualTo("Should filter on period to date", periodTo.AddDays(1), to);
						AssertEquals("duty", 25m, duty);
						AssertEquals("vat", 35m, vat);
						AssertEquals("other", 36m, other);

						AssertEquals("Two rows should be returned", false, reader.Read());
					}
				}
			});
		}

		public void TestReport_WithInvoiceData()
		{
			var clusterKey = 1;
			var testHelper = new TestDbHelper(Db.Connection);

			var jobDeclaration = CreateJobDeclaration(testHelper, branchPK, companyPK, clusterKey);

			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Supplier Name");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "SSSS", "Supplier Address", "Address 2", "Supplier City", "", "P1234", "", "CN");
			TestDataCreator.CreateDocAddress(addressPK, "", jobDeclaration, "JE", "SUD");

			var header = CreateCusEntryHeader(testHelper, jobDeclaration, clusterKey);
			var crd = CreateCusReconDeclaration(testHelper, periodFrom, periodTo, "B019", branchPK);

			TestDataCreator.CreateCusEntryNum(crd, CusReconDeclarationSchema.Constants.TableName, "Test002", "MRN",
			"CUS",
			"DE");

			var recon1 = CreateCusReconEntry(testHelper, crd, addressPK, branchPK, "", header, periodFrom.AddDays(1));

			var line1 = CreateCusEntryLine(testHelper, header, clusterKey, 1);
			var line2 = CreateCusEntryLine(testHelper, header, clusterKey, 2);

			var invoiceHeader = CreateInvoiceHeader(testHelper, jobDeclaration, clusterKey, "INV1", new DateTime(2020, 10, 01));
			var invoiceLine1 = CreateInvoiceLine(testHelper, invoiceHeader, line1, clusterKey, 10m);
			var invoiceLine2 = CreateInvoiceLine(testHelper, invoiceHeader, line1, clusterKey, 20m);
			var invoiceLine3 = CreateInvoiceLine(testHelper, invoiceHeader, line2, clusterKey, 30m);
			var invoiceLine4 = CreateInvoiceLine(testHelper, invoiceHeader, line2, clusterKey, 10m);

			var charge010Line1 = CreateInvoiceHeaderCharge(testHelper, invoiceLine1, "010", 5m);
			var charge012Line1 = CreateInvoiceHeaderCharge(testHelper, invoiceLine1, "012", 6m);
			var charge014Line1 = CreateInvoiceHeaderCharge(testHelper, invoiceLine1, "014", 7m);
			var charge010Line2 = CreateInvoiceHeaderCharge(testHelper, invoiceLine2, "010", 8m);
			var charge012Line2 = CreateInvoiceHeaderCharge(testHelper, invoiceLine2, "012", 9m);

			var charge010Line3 = CreateInvoiceHeaderCharge(testHelper, invoiceLine3, "010", 1m);
			var charge012Line3 = CreateInvoiceHeaderCharge(testHelper, invoiceLine3, "012", 2m);
			var charge010Line4 = CreateInvoiceHeaderCharge(testHelper, invoiceLine4, "010", 3m);
			var charge014Line4 = CreateInvoiceHeaderCharge(testHelper, invoiceLine4, "014", 4m);

			CreateCusSupportingInfo(testHelper, invoiceLine1, "ABC", "REF123", new DateTime(2020, 10, 15), 1);
			CreateCusSupportingInfo(testHelper, invoiceLine1, "DEF", "REF456", new DateTime(2020, 10, 15), 2);
			CreateCusSupportingInfo(testHelper, invoiceLine1, "XYZ", "REF123", new DateTime(2020, 10, 15), 3);
			CreateCusSupportingInfo(testHelper, invoiceLine2, "ABC", "REF123", new DateTime(2020, 10, 15), 5);
			CreateCusSupportingInfo(testHelper, invoiceLine2, "DEF", "REF123", new DateTime(2020, 10, 15), 6);
			CreateCusSupportingInfo(testHelper, invoiceLine3, "QWE", "REF123", new DateTime(2020, 10, 15), 1);

			var reconLine1 = CreateCusReconEntryLine(recon1, originalLineNumber: 1, lineNumber: 1);
			var reconLine2 = CreateCusReconEntryLine(recon1, originalLineNumber: 2, lineNumber: 2);

			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 10f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 20f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line1, "C00", 21f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "C01", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "C00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 12f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 13f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 21f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 14f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "C00", 22f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C02", 2f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C01", 12f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C00", 11f, clusterKey);

			CombineAssertions(() =>
			{
				var sql =
					@"select JobNumber, PeriodFrom, PeriodTo, RegistrationNumberEgz, EgzLineNo, JobNumberImport, OwnerReference, EntryDate, EntryType, RegistrationNumberVzaAz, LineNum, Status, Tariff, CustomsValue, Description, Duty, Vat, OtherLevies, InvoiceNumber, InvoiceDate, CPC, NetPriceAmount, PriceAmount, PriceCurrency, PriceExchangeRate, Charges010Amount, Charges010Currency, Charges010ExchangeRate, Charges014Amount, Charges014Currency, Charges014ExchangeRate, Charges012Amount, Charges012Currency, Charges012ExchangeRate, IATACode, CountryOfDispatch, CountryOfOrigin, IncotermCode, AgreedPlace, IncotermKey, SupplierName, SupplierAddress, SupplierCountry, SupplierPostCode, SupplierCity, PreviousEntryNumber, PreviousEntryLine, Quantity, QuantityUnit, Product, SupportingDocType1, SupportingDocReference1, SupportingDocDate1, SupportingDocType2, SupportingDocReference2, SupportingDocDate2, SupportingDocType3, SupportingDocReference3, SupportingDocDate3, SupportingDocType4, SupportingDocReference4, SupportingDocDate4, SupportingDocType5, SupportingDocReference5, SupportingDocDate5
from dbo.Report_DEMonthlyClosingDutyTaxReport (@branchPK, @dateFrom, @dateTo, @jobNumber)
order by EgzLineNo";
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@branchPk", SqlDbType.UniqueIdentifier, branchPK);
					command.AddParameter("@dateFrom", SqlDbType.DateTime, periodFrom.AddDays(-1));
					command.AddParameter("@dateTo", SqlDbType.DateTime, periodTo.AddDays(1).AddSeconds(1));
					command.AddParameter("@jobNumber", SqlDbType.VarChar, "B019");

					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						var lineNum = (short)reader["LineNum"];
						var egzLineNo = (short)reader["EgzLineNo"];
						var from = (DateTime)reader["PeriodFrom"];
						var to = (DateTime)reader["PeriodTo"];
						var duty = (decimal)reader["Duty"];
						var vat = (decimal)reader["Vat"];
						var other = (decimal)reader["OtherLevies"];
						var invoiceNumber = (string)reader["invoiceNumber"];
						var invoiceDate = reader["InvoiceDate"];
						var cpc = (string)reader["CPC"];
						var netPrice = (decimal)reader["NetPriceAmount"];
						var price = (decimal)reader["PriceAmount"];
						var currency = (string)reader["PriceCurrency"];
						var exchangeRate = (decimal)reader["PriceExchangeRate"];
						var charges010Amount = (decimal)reader["Charges010Amount"];
						var charges010Currency = (string)reader["Charges010Currency"];
						var charges010Rate = (decimal)reader["Charges010ExchangeRate"];
						var charges014Amount = (decimal)reader["Charges014Amount"];
						var charges014Currency = (string)reader["Charges014Currency"];
						var charges014Rate = (decimal)reader["Charges014ExchangeRate"];
						var charges012Amount = (decimal)reader["Charges012Amount"];
						var charges012Currency = (string)reader["Charges012Currency"];
						var charges012Rate = (decimal)reader["Charges012ExchangeRate"];
						var iata = (string)reader["IATACode"];
						var countryOfDispatch = (string)reader["CountryOfDispatch"];
						var countryOfOrigin = (string)reader["CountryOfOrigin"];
						var incotermCode = (string)reader["IncotermCode"];
						var agreedPlace = (string)reader["AgreedPlace"];
						var incotermKey = (string)reader["IncotermKey"];
						var supplierName = (string)reader["SupplierName"];
						var supplierAddress = (string)reader["SupplierAddress"];
						var supplierCountry = (string)reader["SupplierCountry"];
						var supplierPostCode = (string)reader["SupplierPostCode"];
						var supplierCity = (string)reader["SupplierCity"];
						var previousEntryNumber = (string)reader["PreviousEntryNumber"];
						var previousEntryLine = (short)reader["PreviousEntryLine"];
						var quantity = (decimal)reader["Quantity"];
						var quantityUnit = (string)reader["QuantityUnit"];
						var product = (string)reader["Product"];
						var supportingDocType1 = (string)reader["SupportingDocType1"];
						var supportingDocReference1 = (string)reader["SupportingDocReference1"];
						var supportingDocDate1 = reader["SupportingDocDate1"];
						var supportingDocType2 = (string)reader["SupportingDocType2"];
						var supportingDocReference2 = (string)reader["SupportingDocReference2"];
						var supportingDocDate2 = reader["SupportingDocDate2"];
						var supportingDocType3 = (string)reader["SupportingDocType3"];
						var supportingDocReference3 = (string)reader["SupportingDocReference3"];
						var supportingDocDate3 = reader["SupportingDocDate3"];
						var supportingDocType4 = (string)reader["SupportingDocType4"];
						var supportingDocReference4 = (string)reader["SupportingDocReference4"];
						var supportingDocDate4 = reader["SupportingDocDate4"];
						var supportingDocType5 = (string)reader["SupportingDocType5"];
						var supportingDocReference5 = (string)reader["SupportingDocReference5"];
						var supportingDocDate5 = reader["SupportingDocDate5"];
						AssertEquals("lineNum", (short)1, lineNum);
						AssertEquals("egzLineNo", (short)1, egzLineNo);
						AssertLessThanOrEqualTo("Should filter on period from date", periodFrom.AddDays(-1), from);
						AssertGreaterThanOrEqualTo("Should filter on period to date", periodTo.AddDays(1), to);
						AssertEquals("duty", 25m, duty);
						AssertEquals("vat", 35m, vat);
						AssertEquals("other", 36m, other);
						AssertEquals("invoiceDate", "INV1", invoiceNumber);
						AssertEquals("InvoiceDate", new DateTime(2020, 10, 01), invoiceDate);
						AssertEquals("CPC", "100", cpc);

						AssertEquals("netPrice", 30m, netPrice);
						AssertEquals("linePrice", 32m, price);
						AssertEquals("currency", "EUR", currency);
						AssertEquals("exchangeRate", 1m, exchangeRate);
						AssertEquals("charges010Amount", 13m, charges010Amount);
						AssertEquals("charges010Currency", "USD", charges010Currency);
						AssertEquals("charges010Rate", 1.1m, charges010Rate);
						AssertEquals("charges014Amount", 7m, charges014Amount);
						AssertEquals("charges014Currency", "USD", charges014Currency);
						AssertEquals("charges014Rate", 1.1m, charges014Rate);
						AssertEquals("charges012Amount", 15m, charges012Amount);
						AssertEquals("charges012Currency", "USD", charges012Currency);
						AssertEquals("charges012Rate", 1.1m, charges012Rate);

						AssertEquals("iata", "CNS", iata);
						AssertEquals("countryOfDispatch", "CN", countryOfDispatch);
						AssertEquals("countryOfOrigin", "SG", countryOfOrigin);
						AssertEquals("incotermCode", "ABC", incotermCode);
						AssertEquals("agreedPlace", "ITP", agreedPlace);
						AssertEquals("incotermKey", "PLACE", incotermKey);
						AssertEquals("supplierName", "Supplier Name", supplierName);
						AssertEquals("supplierAddress", "Supplier Address", supplierAddress);
						AssertEquals("supplierCountry", "CN", supplierCountry);
						AssertEquals("supplierPostCode", "P1234", supplierPostCode);
						AssertEquals("supplierCity", "Supplier City", supplierCity);
						AssertEquals("previousEntryNumber", "PREV123", previousEntryNumber);
						AssertEquals("previousEntryLine", (short)2, previousEntryLine);

						AssertEquals("quantity", 200m, quantity);
						AssertEquals("quantityUnit", "KGM", quantityUnit);
						AssertEquals("product", "PROD1", product);

						AssertEquals("supportingDocType1", "ABC", supportingDocType1);
						AssertEquals("supportingDocReference1", "REF123", supportingDocReference1);
						AssertEquals("supportingDocDate1", new DateTime(2020, 10, 15), supportingDocDate1);
						AssertEquals("supportingDocType2", "DEF", supportingDocType2);
						AssertEquals("supportingDocReference2", "REF456", supportingDocReference2);
						AssertEquals("supportingDocDate2", new DateTime(2020, 10, 15), supportingDocDate2);
						AssertEquals("supportingDocType3", "XYZ", supportingDocType3);
						AssertEquals("supportingDocReference3", "REF123", supportingDocReference3);
						AssertEquals("supportingDocDate3", new DateTime(2020, 10, 15), supportingDocDate3);
						AssertEquals("supportingDocType4", "DEF", supportingDocType4);
						AssertEquals("supportingDocReference4", "REF123", supportingDocReference4);
						AssertEquals("supportingDocDate4", new DateTime(2020, 10, 15), supportingDocDate4);
						AssertEquals("supportingDocType5", "", supportingDocType5);
						AssertEquals("supportingDocReference5", "", supportingDocReference5);
						AssertEquals("supportingDocDate5", DBNull.Value, supportingDocDate5);

						AssertEquals("A second row should be returned", true, reader.Read());

						lineNum = (short)reader["LineNum"];
						egzLineNo = (short)reader["EgzLineNo"];
						charges010Amount = (decimal)reader["Charges010Amount"];
						charges014Amount = (decimal)reader["Charges014Amount"];
						charges012Amount = (decimal)reader["Charges012Amount"];
						AssertEquals("Second lineNum", (short)2, lineNum);
						AssertEquals("Second egzLineNo", (short)2, egzLineNo);
						AssertEquals("charges010Amount", 4m, charges010Amount);
						AssertEquals("charges014Amount", 4m, charges014Amount);
						AssertEquals("charges012Amount", 2m, charges012Amount);

						AssertEquals("Two rows should be returned", false, reader.Read());
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestDbHelper(Db.Connection);
			periodFrom = new DateTime(2020, 2, 1, 00, 00, 00, DateTimeKind.Utc);
			periodTo = new DateTime(2021, 2, 1, 00, 00, 00, DateTimeKind.Utc);

			companyPK = TestDataCreator.CreateCompany("TC1", "DE", "DDE");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "DEBER");
			TestDataCreator.CreateDepartment("TD1");
			var org = TestDataCreator.CreateOrganisation("DDE", "DDE");
			addressPK = TestDataCreator.CreateAddress(org, "ANY", "ANY", "DE");

			TestDataCreator.CreateRefDatabaseRefDataGrouping("EUN", "DESC");

			var dty = TestDataCreator.CreateRefCusRateType("DTY", "DESC", 0, "EUN", "");
			var oth = TestDataCreator.CreateRefCusRateType("OTH", "DESC", 0, "EUN", "");

			TestDataCreator.CreateRefCusRateCode("A00", dty, "DESC");
			TestDataCreator.CreateRefCusRateCode("C00", oth, "DESC");

			CreateJobWithFees(branchPK, companyPK, testHelper, periodFrom, periodTo, "B007", addressPK,
				periodFrom.AddDays(1), 2);

			CreateJobWithFees(branchPK, companyPK, testHelper, periodFrom, periodTo, "B008", addressPK,
				periodFrom.AddDays(1), 3);

			CreateJobWithFees(branchPK, companyPK, testHelper, periodFrom, periodTo.AddDays(1), "B009", addressPK,
				periodFrom.AddDays(1), 4);

			CreateJobWithFees(branchPK, companyPK, testHelper, periodFrom, periodTo.AddDays(2), "B010", addressPK,
				periodFrom.AddDays(1), 5);

			CreateJobWithFees(branchPK, companyPK, testHelper, periodFrom.AddDays(1), periodTo, "B011", addressPK,
				periodFrom.AddDays(2), 6);

			CreateJobWithFees(branchPK, companyPK, testHelper, periodFrom.AddDays(-1), periodTo, "B012", addressPK,
				periodFrom.AddDays(-2), 7);
		}

		static void CreateJobWithFees(Guid branch, Guid company, TestDbHelper testHelper, DateTime periodFrom1,
			DateTime periodTo1, string jobNumber, Guid address, DateTime entryDate, int clusterKey)
		{
			var jobDeclaration = CreateJobDeclaration(testHelper, branch, company, clusterKey);

			var header1 = CreateCusEntryHeader(testHelper, jobDeclaration, clusterKey);
			var crd = CreateCusReconDeclaration(testHelper, periodFrom1, periodTo1, jobNumber, branch);

			TestDataCreator.CreateCusEntryNum(crd, CusReconDeclarationSchema.Constants.TableName, "Test001", "MRN",
				"CUS",
				"DE");

			var recon1 = CreateCusReconEntry(testHelper, crd, address, branch, "", header1, entryDate);
			var line1 = CreateCusEntryLine(testHelper, header1, clusterKey, 1);
			var line2 = CreateCusEntryLine(testHelper, header1, clusterKey, 2);

			var reconLine1 = CreateCusReconEntryLine(recon1, originalLineNumber: 1, lineNumber: 1);
			var reconLine2 = CreateCusReconEntryLine(recon1, originalLineNumber: 2, lineNumber: 2);

			var invoiceHeader = CreateInvoiceHeader(testHelper, jobDeclaration, clusterKey, "INV1", new DateTime(2020, 10, 01));
			var invoiceLine = CreateInvoiceLine(testHelper, invoiceHeader, line1, clusterKey, 10m);

			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 10f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "A00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 20f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "B00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line1, "C00", 21f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "C01", 15f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line1, "C00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 12f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 13f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "A00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 21f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 14f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "B00", 11f, clusterKey);

			TestDataCreator.CreateCusEntryLineFee(line2, "C00", 22f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C02", 2f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C01", 12f, clusterKey, "CUS");
			TestDataCreator.CreateCusEntryLineFee(line2, "C00", 11f, clusterKey);
		}

		public static Guid CreateInvoiceHeader(TestDbHelper testHelper, Guid jobDeclaration, int clusterKey, string invoiceNumber, DateTime invoiceDate)
		{
			var result = Guid.NewGuid();

			testHelper.Insert(JobComInvoiceHeaderSchema.Constants.TableName, new
			{
				JZ_PK = result,
				JZ_DataModel = "DE",
				JZ_JE = jobDeclaration,
				JZ_ClusterKey = clusterKey,
				JZ_InvoiceNumber = invoiceNumber,
				JZ_InvoiceDate = invoiceDate,
				JZ_RX_NKInvoice_Currency = "EUR",
				JZ_InvoiceCurrExRate = 1m,
				JZ_IncoTerm = "ABC",
				JZ_IncoTermPlace = "ITP",
				JZ_AddInfo = "AgreedPlaceCode=PLACE"
			});

			return result;
		}

		public static Guid CreateInvoiceLine(TestDbHelper testHelper, Guid invoiceHeader, Guid entryLine, int clusterKey, decimal netPrice)
		{
			var result = Guid.NewGuid();

			testHelper.Insert(JobComInvoiceLineSchema.Constants.TableName, new
			{
				JI_PK = result,
				JI_DataModel = "DE",
				JI_JZ = invoiceHeader,
				JI_ClusterKey = clusterKey,
				JI_CL = entryLine,
				JI_Procedure = "100",
				JI_AddInfo = "NetPrice=" + netPrice.ToString() + "*OtherData=OtherVal",
				JI_LinePrice = netPrice + 1,
				JI_CountryOfOrigin = "SG",
				JI_BondedWhsQuantity = 100,
				JI_BondedWhsUnitQty = "KGM",
				JI_PartNo = "PROD1",
				JI_PreviousEntryNumber = "PREV123",
				JI_PreviousEntryLineNumber = 2,
			});

			return result;
		}

		public static Guid CreateInvoiceHeaderCharge(TestDbHelper testHelper, Guid parent, string chargeType, decimal amount)
		{
			var result = Guid.NewGuid();

			testHelper.Insert(JobComInvHeaderChargeSchema.Constants.TableName, new
			{
				J7_PK = result,
				J7_ParentTableCode = "JI",
				J7_ParentID = parent,
				J7_ChargeType = chargeType,
				J7_Amount = amount,
				J7_RX_NKCurrency = "USD",
				J7_ExchangeRate = 1.1m
			});

			return result;
		}

		public static Guid CreateCusSupportingInfo(TestDbHelper testHelper, Guid parent, string code, string refNo, DateTime date, int lineNo)
		{
			var result = Guid.NewGuid();

			testHelper.Insert(CusSupportingInfoSchema.Constants.TableName, new
			{
				CSI_PK = result,
				CSI_ParentTableCode = "JI",
				CSI_ParentID = parent,
				CSI_Code = code,
				CSI_Type = "SUP",
				CSI_SubType = "REF",
				CSI_ReferenceNumber = refNo,
				CSI_DateOfIssue = date,
				CSI_LineNo = lineNo,
				CSI_DataModel = "DE"
			});

			return result;
		}

		public static Guid CreateCusEntryHeader(
			TestDbHelper testHelper,
			Guid jobDeclaration,
			int clusterKey
		)
		{
			var result = Guid.NewGuid();

			testHelper.Insert(CusEntryHeaderSchema.Constants.TableName, new
			{
				CH_PK = result,
				CH_DataModel = "DE",
				CH_JE = jobDeclaration,
				CH_ClusterKey = clusterKey,
				CH_MessageType = "IMP",
				CH_AddInfo = "",
			});

			return result;
		}

		public static Guid CreateJobDeclaration(TestDbHelper testHelper, Guid branch, Guid company, int clusterKey, string declarationReference = "")
		{
			var result = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;

			testHelper.Insert(JobDeclarationSchema.Constants.TableName, new
			{
				JE_PK = result,
				JE_DataModel = "DE",
				JE_MessageType = "IMP",
				JE_GB = branch,
				JE_GC = company,
				JE_ClusterKey = clusterKey,
				JE_DeclarationReference = declarationRef,
				JE_AddInfo = "",
				JE_GS_NKCustomsCommencedUser = "",
				JE_AgentsReference = "",
				JE_OwnerRef = "",
				JE_GS_NKCusAgent = "",
				JE_IATALoadPort = "CNS",
				JE_GoodsOrigin = "CN",
			});

			return result;
		}

		public static Guid CreateCusEntryLine(TestDbHelper testHelper, Guid clCh, int clusterKey, int lineNumber)
		{
			var result = Guid.NewGuid();

			testHelper.Insert(CusEntryLineSchema.Constants.TableName, new
			{
				CL_PK = result,
				CL_DataModel = "DE",
				CL_CH = clCh,
				CL_ClusterKey = clusterKey,
				CL_LineNumber = lineNumber,
				CL_DutyPercent = 0,
				CL_FlatAmount = 0,
				CL_FlatAmountUQ = "",
			});

			return result;
		}

		static Guid CreateCusReconEntry(TestDbHelper testHelper, Guid crd, Guid address, Guid branch,
			string originalEntryNumber, Guid header1, DateTime entryDate)
		{
			var recon1 = Guid.NewGuid();

			testHelper.Insert(CusReconEntrySchema.Constants.TableName, new
			{
				CRE_PK = recon1,
				CRE_CRD = crd,
				CRE_OA_DeclarantAddress = address,
				CRE_GB_Branch = branch,
				CRE_OriginalEntryNumber = originalEntryNumber,
				CRE_CH_OriginalEntry = header1,
				CRE_EntryType = "EGZ",
				CRE_EntryDate = entryDate,
			});
			return recon1;
		}

		static Guid CreateCusReconDeclaration(TestDbHelper testHelper, DateTime periodFrom, DateTime periodTo,
			string jobNumber, Guid branch)
		{
			var crd = Guid.NewGuid();
			testHelper.Insert(CusReconDeclarationSchema.Constants.TableName, new
			{
				CRD_ApplicationCode = "CLS",
				CRD_PeriodFrom = periodFrom.ToSqlFormat(),
				CRD_PeriodTo = periodTo.ToSqlFormat(),
				CRD_JobReferenceNumber = jobNumber,
				CRD_Pk = crd,
				CRD_GB_Branch = branch,
				CRD_DataModel = "DE",
			});
			return crd;
		}

		static Guid CreateCusReconEntryLine(
			Guid reconEntry,
			Guid? crlPk = null,
			int lineNumber = 1,
			int originalLineNumber = 1
		)
		{
			crlPk = crlPk ?? Guid.NewGuid();
			const string sql = @"
INSERT INTO [dbo].[CusReconEntryLine] (
			[CRL_PK]
		   ,[CRL_CRE]
		   ,[CRL_LineNumber]
		   ,[CRL_CustomsStatus]
		   ,[CRL_Description]
		   ,[CRL_OriginalEntryLineNumber]
		   ,[CRL_SystemCreateTimeUtc]
		   ,[CRL_SystemCreateUser]
		   ,[CRL_SystemLastEditTimeUtc]
		   ,[CRL_SystemLastEditUser]
	) VALUES (
		@crlPK			--	  <CRL_PK, uniqueidentifier,>
		,@CRL_CRE		--   ,<CRL_CRE, uniqueidentifier,>
		,@lineNumber				--   ,<CRL_LineNumber, smallint,>
		,''				--   ,<CRL_CustomsStatus, varchar(3),>
		,''				--   ,<CRL_Description, varchar(512),>
		,@originalLineNumber				--   ,<CRL_OriginalEntryLineNumber, smallint,>
		,GetUtcDate()	--   ,<CRL_SystemCreateTimeUtc, smalldatetime,>
		,'~BP'			--   ,<CRL_SystemCreateUser, varchar(3),>
		,GetUtcDate()   --   ,<CRL_SystemLastEditTimeUtc, smalldatetime,>
		,'~BP'			--   ,<CRL_SystemLastEditUser, varchar(3),>
	)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@crlPK", SqlDbType.UniqueIdentifier, crlPk.Value);
				command.AddParameter("@crl_cre", SqlDbType.UniqueIdentifier, reconEntry);
				command.AddParameter("@originalLineNumber", SqlDbType.SmallInt, originalLineNumber);
				command.AddParameter("@lineNumber", SqlDbType.SmallInt, lineNumber);

				command.ExecuteNonQuery();
			}

			return crlPk.Value;
		}

		Guid companyPK, branchPK, addressPK;
		DateTime periodFrom;
		DateTime periodTo;
	}
}
