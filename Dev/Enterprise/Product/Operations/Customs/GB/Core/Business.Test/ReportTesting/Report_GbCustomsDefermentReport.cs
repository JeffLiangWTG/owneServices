using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.GB.Business.ReportTesting
{
	public abstract class Report_GbCustomsDefermentReport : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var dan = new ReportSchemaColumn(typeof(string), "DAN");
				var decRef = new ReportSchemaColumn(typeof(string), "DeclarationReference");
				var taxType = new ReportSchemaColumn(typeof(string), "TaxType");
				var taxAmount = new ReportSchemaColumn(typeof(decimal), "TaxAmount");
				var lineNum = new ReportSchemaColumn(typeof(short), "InvoiceLineNumber");
				var importer = new ReportSchemaColumn(typeof(string), "Importer");
				return new List<ReportSchemaColumn>() { dan, decRef, taxType, taxAmount, lineNum, importer };
			}
		}

		protected override void AssertTestResults(DataTable results)
		{
			// TaxAmount shows only 2 decimal places in report and the output of the function, however it shows all 4 decimal places in DataTable due to its "MONEY" type
			foreach (DataRow row in results.Rows)
			{
				var taxAmount = (decimal)row["TaxAmount"];
				AssertEquals(decimal.Round(taxAmount, 2), taxAmount);
				row["TaxAmount"] = decimal.Round(taxAmount, 2);
			}

			AssertEquals(6, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			var row2 = FormatRowsValues(results.Rows[1], results);
			var row3 = FormatRowsValues(results.Rows[2], results);
			var row4 = FormatRowsValues(results.Rows[3], results);
			var row5 = FormatRowsValues(results.Rows[4], results);
			var row6 = FormatRowsValues(results.Rows[5], results);

			AssertContains("[DAN]='1111111'; [DeclarationReference]='B00001000'; [TaxType]='A00'; [TaxAmount]='100.00'; [InvoiceLineNumber]='1'; [Importer]='ABCDEFLHR'", row1);
			AssertContains("[DAN]='1111111'; [DeclarationReference]='B00001000'; [TaxType]='B00'; [TaxAmount]='200.00'; [InvoiceLineNumber]='1'; [Importer]='ABCDEFLHR'", row2);
			AssertContains("[DAN]='2222222'; [DeclarationReference]='B00001001'; [TaxType]='A00'; [TaxAmount]='100.00'; [InvoiceLineNumber]='1'; [Importer]='ABCDEFLHR'", row3);
			AssertContains("[DAN]='2222222'; [DeclarationReference]='B00001001'; [TaxType]='B00'; [TaxAmount]='200.00'; [InvoiceLineNumber]='1'; [Importer]='ABCDEFLHR'", row4);
			AssertContains("[DAN]='3333333'; [DeclarationReference]='B00001002'; [TaxType]='A00'; [TaxAmount]='100.00'; [InvoiceLineNumber]='1'; [Importer]='ABCDEFLHR'", row5);
			AssertContains("[DAN]='4444444'; [DeclarationReference]='B00001002'; [TaxType]='B00'; [TaxAmount]='200.00'; [InvoiceLineNumber]='1'; [Importer]='ABCDEFLHR'", row6);
		}

		protected abstract string ApplicationCode { get; }

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override ZString ObjectName => "Report_GbCustomsDefermentReport";

		protected override List<string> ParametersValuesList => new List<string>()
		{
			$"'{GlbCompany.CurrentCompany.PK}'", // CompanyPk
			$"'{ZDateTime.Now.AddDays(-1):yyyy-MM-dd}'", // DateFrom
			$"'{ZDateTime.Now.AddDays(1):yyyy-MM-dd}'", // DateTo
			"''", // OrgList
			$"'{ApplicationCode}'" // ApplicationCode
		};

		protected override void PrepareTestData()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCDEFLHR";
			var declarationGood = SetupDeclaration(org, "IMP", ApplicationCode);
			declarationGood.JE_DefermentAccountNumber = "1111111";

			var declarationGood2 = SetupDeclaration(org, "IMP", ApplicationCode);
			declarationGood2.JE_DefermentAccountNumber = "2222222";

			var declarationGood3TwoDans = SetupDeclaration(org, "IMP", ApplicationCode);
			declarationGood3TwoDans.JE_DefermentAccountNumber = "3333333";
			declarationGood3TwoDans.ZG_VATDeferNumber = "4444444";

			var declarationBadExport = SetupDeclaration(org, "EXP", ApplicationCode);
			var declarationBadNoDan = SetupDeclaration(org, "IMP", ApplicationCode);

			var declarationBadTooOld = SetupDeclaration(org, "IMP", ApplicationCode);
			declarationBadTooOld.JE_DefermentAccountNumber = "5555555";
			declarationBadTooOld.JE_EntrySubmittedDate = ZDateTime.BrettsBirthday;

			var wrongAppCode = ApplicationCode == "CDS" ? "CHF" : "CDS";
			var otherDeclaration = SetupDeclaration(org, "IMP", wrongAppCode);
			otherDeclaration.JE_DeclarationReference = "A00000001";
			otherDeclaration.JE_DefermentAccountNumber = "6666666";
		}

		JobDeclaration SetupDeclaration(OrgHeader org, string messageType, string applicationCode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = applicationCode;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = messageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_EntrySubmittedDate = ZDateTime.Now;
			SetupTaxesAndFees(invoiceLine);

			return declaration;
		}

		void SetupTaxesAndFees(JobComInvoiceLine invoiceLine)
		{
			SetupTax(invoiceLine, "A00", DeferredMOP, 100m);
			SetupTax(invoiceLine, "B00", DeferredMOP, 200m);
			SetupTax(invoiceLine, "A00", ImmediateMOP, 300m);
			SetupTax(invoiceLine, "B00", ImmediateMOP, 400m);
		}

		protected abstract void SetupTax(JobComInvoiceLine invoiceLine, string type, string mop, ZDecimal amount);
		protected abstract string DeferredMOP { get; }
		protected abstract string ImmediateMOP { get; }
	}
}
