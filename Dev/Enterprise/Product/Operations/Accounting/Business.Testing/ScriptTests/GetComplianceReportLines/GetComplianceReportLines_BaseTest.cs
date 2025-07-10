using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public abstract class GetComplianceReportLines_BaseTest : ScriptTest
	{
		#region Brexit

		[TestDate(2019, 03, 19)]
		public void TestRepCountryRegNoForBrexitDate_OrgCountry()
		{
			if (TablePrefix == "AH" && GroupByCode != "ORG")
			{
				var brexitDate = TestObjectCreator.SetupPostBrexitData();
				var orgHeader = TestObjectCreator.Creditor1;
				SetupHeaderWithClosestPortAndCustomsCode(orgHeader, "GBLON", CountryCodes.UnitedKingdom, "111111_GB", Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCodes.UnitedKingdom));

				TestObjectCreator.SetupCashBasisVAT();

				var invoice1 = CreateInvoice(orgHeader, "002A", brexitDate, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);
				var invoice2 = CreateInvoice(orgHeader, "001A", brexitDate.AddDays(-1), AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code);
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_DateFrom = brexitDate.AddDays(-1).Date;
				report.ACR_DateTo = brexitDate.AddDays(1).Date;
				TestObjectCreator.CreateConfigurationForComplianceReport(report, AccTransactionHeaderSchema.Constants.Prefix);
				Factory.Save();

				TestObjectCreator.CreateComplianceReportQueueEntry(report, invoice1, invoice2);
				report.GenerateFromQueue();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
				{
					var headers = new[] { "AH_TransactionNum", "OK_CustomsRegNo" };

					var result = RunScript(report, ReportName, TablePrefix);

					var lines = new[] {
						new object[] { "001A", "GB111111_GB" },
						new object[] { "002A", string.Empty },
					};

					AssertDataTableAllRowsByKeyColumns("", result, headers, lines);
				}
			}
			else
			{
				Assert(true);
			}

			InvoicingBase CreateInvoice(OrgHeader orgHeader, string invoiceNumber, ZDateTime postDate, string vatBasis)
			{
				var inv = TestObjectCreator.CreateInvoice(typeof(APInvoice), invoiceNumber, TestObjectCreator.GBP, organisation: orgHeader);
				inv.AH_PostDate = postDate;
				var line = TestObjectCreator.CreateInvoiceLine(inv, TestObjectCreator.GLHeader1.PK, 100M);
				line.AL_GSTVATBasis = vatBasis;

				return inv;
			}
		}

		static void SetupHeaderWithClosestPortAndCustomsCode(OrgHeader header, string closestPort, string countryCode, ZString customsRegNo, string codeType)
		{
			header.OH_RL_NKClosestPort = closestPort;
			header.CompanyData.SetAPTaxApplicable(true);
			var taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = customsRegNo;
			taxCode.OK_CodeType = codeType;
		}

		#endregion

		#region Business Object Creation

		protected InvoicingLineBase CreateARInvoiceLine(ARInvoice invoice, decimal amount, AccTaxRate accTaxRate, decimal taxAmount, ZDateTime postDate)
		{
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1m, amount);
			line.AL_AT = accTaxRate.PK;
			line.AL_GSTVAT = taxAmount;
			line.AL_PostDate = postDate;
			return line;
		}

		protected InvoicingLineBase CreateAPLine(APInvoice apInvoice, ZDecimal amount, ZDecimal taxAmount, ZDateTime postDate)
		{
			var apLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.EUR, 1m, amount);
			apLine.AL_AT = TestObjectCreator.GST2.PK;
			apLine.AL_GSTVAT = taxAmount;
			apLine.AL_PostDate = postDate;
			return apLine;
		}

		protected DependentTransactionLine CreateACashbookQueueEntry(int year, int month, int day, decimal amount, ZGuid bankAccountPK)
		{
			var postDate = new ZDate(year, month, day);

			var directPayment = TestObjectCreator.CreateDirectPayment(postDate, amount, taxAmount: 0, amount2: 0, taxAmount2: 0, bankAccountPK, exchangeRate: 1M);

			return directPayment.Lines[0];
		}

		protected void CreateComplianceReportQueueEntry(int year, int month, int day, int offSet, string reportSubCode, AccComplianceReport report, DependentTransactionLine paymentLine)
		{
			var postDate = new ZDate(year, month, day);
			postDate = postDate.AddMonths(offSet);
			reportSubCode = reportSubCode + " - " + postDate.Year.ToString("D4") + postDate.Month.ToString("D2");

			var result = TestObjectCreator.CreateComplianceReportQueueEntry(report, subCode: reportSubCode, overrideDate: postDate, paymentLine);
			Factory.Save();
		}

		#endregion

		#region Compile memory

		public virtual void TestCompileMemory()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, baseTablePrefix: TablePrefix, reportLineGrouping: GroupByCode);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I000" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, line1);
			report.GenerateFromQueue();

			AssertCompileMemory(report, ReportName, TablePrefix);
		}

		protected void AssertCompileMemory(AccComplianceReport report, string reportName, string tablePrefix, string roundingType = "", ZDate? reportDateFrom = null, ZDate? reportDateTo = null, string regType = "1ST", bool reverseSign = true)
		{
			var sql = $"SELECT * FROM {reportName}(@PK, @ReportTablePrefix, @ReportDateFrom, @ReportDateTo_PlusOneDay, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @SubCodeToGLAccountMapping, @GS, @RoundingType, @Rounding, @ReverseSign)";
			using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameterBasedOnDbColumn("@PK", report.PK.ToGuid(), AccComplianceReportSchema.PK);
					command.AddParameterBasedOnDbColumn("@ReportTablePrefix", tablePrefix, CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameterBasedOnDbColumn("@ReportDateFrom", reportDateFrom.HasValue ? reportDateFrom.Value.ToDateTime() : DBNull.Value, CargoWise.Schema.Schema.GenericDateTimeColumn);
					command.AddParameterBasedOnDbColumn("@ReportDateTo_PlusOneDay", reportDateTo.HasValue ? reportDateTo.Value.AddDays(1).ToDateTime() : DBNull.Value, CargoWise.Schema.Schema.GenericDateTimeColumn);
					command.AddParameterBasedOnDbColumn("@RegType", regType, CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameterBasedOnDbColumn("@ReportCountry", "AU", CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameterBasedOnDbColumn("@RepCountryRegistrationCodeType", "", CargoWise.Schema.Schema.GenericStringSchemaColumn);

					var mappingTable = (new ControlAccountAndReportSubCodeMapping()).AccountPkToSubCodesTable;
					command.AddTableValuedParameter("@SubCodeToGLAccountMapping", "dbo.TVP_CodeToGuidMapping", mappingTable);

					command.AddParameterBasedOnDbColumn("@GS", "", CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameterBasedOnDbColumn("@RoundingType", roundingType, CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameterBasedOnDbColumn("@Rounding", 2, CargoWise.Schema.Schema.GenericIntSchemaColumn);
					command.AddParameterBasedOnDbColumn("@ReverseSign", reverseSign, CargoWise.Schema.Schema.GenericBitSchemaColumn);

					using (var reader = command.ExecuteReader())
					{
						reader.Read();

						reader.NextResult();
						var queryPlan = Db.Connection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains(reportName));
						var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

						AssertLessThanOrEqualTo("Compile memory should not greater than 500MB.", queryPlanAnalyzer.QueryPlan.CompileMemory, 500000L);
					}
				}
			}
		}

		#endregion

		public abstract void TestGetComplianceReportLines();

		protected abstract string ReportName { get; }

		protected abstract string GroupByCode { get; }

		protected abstract string TablePrefix { get; }

		protected void AssertRow(EnumerableRowCollection<DataRow> rows, string reportSubCode, ZGuid pk, decimal exTaxAmount, int sequenceNo, decimal taxAmount)
		{
			var row = rows.FirstOrDefault(x => x.Field<string>("ReportSubCode") == reportSubCode && x.Field<Guid>("AH_PK") == pk);
			AssertNotNull(row);
			AssertEquals("Wrong Sequence Number", sequenceNo, row.Field<int>("ACL_ReportSequence"));
			AssertEquals("Wrong Net Amount", exTaxAmount, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Wrong Tax Amount", taxAmount, row.Field<decimal>("TotalTaxAmount"));
		}

		protected static DataTable RunScript(AccComplianceReport report, string reportName, string tablePrefix, string roundingType = "", ZDate? reportDateFrom = null, ZDate? reportDateTo = null, string regType = "1ST", bool reverseSign = true)
		{
			var sql = $"SELECT * FROM {reportName}(@PK, @ReportTablePrefix, @ReportDateFrom, @ReportDateTo_PlusOneDay, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @SubCodeToGLAccountMapping, @GS, @RoundingType, @Rounding, @ReverseSign)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@PK", report.PK.ToGuid(), AccComplianceReportSchema.PK);
				command.AddParameterBasedOnDbColumn("@ReportTablePrefix", tablePrefix, CargoWise.Schema.Schema.GenericStringSchemaColumn);
				command.AddParameterBasedOnDbColumn("@ReportDateFrom", reportDateFrom.HasValue ? reportDateFrom.Value.ToDateTime() : DBNull.Value, CargoWise.Schema.Schema.GenericDateTimeColumn);
				command.AddParameterBasedOnDbColumn("@ReportDateTo_PlusOneDay", reportDateTo.HasValue ? reportDateTo.Value.AddDays(1).ToDateTime() : DBNull.Value, CargoWise.Schema.Schema.GenericDateTimeColumn);
				command.AddParameterBasedOnDbColumn("@RegType", regType, CargoWise.Schema.Schema.GenericStringSchemaColumn);
				command.AddParameterBasedOnDbColumn("@ReportCountry", "AU", CargoWise.Schema.Schema.GenericStringSchemaColumn);
				command.AddParameterBasedOnDbColumn("@RepCountryRegistrationCodeType", "", CargoWise.Schema.Schema.GenericStringSchemaColumn);

				var mappingTable = (new ControlAccountAndReportSubCodeMapping()).AccountPkToSubCodesTable;
				command.AddTableValuedParameter("@SubCodeToGLAccountMapping", "dbo.TVP_CodeToGuidMapping", mappingTable);

				command.AddParameterBasedOnDbColumn("@GS", "", CargoWise.Schema.Schema.GenericStringSchemaColumn);
				command.AddParameterBasedOnDbColumn("@RoundingType", roundingType, CargoWise.Schema.Schema.GenericStringSchemaColumn);
				command.AddParameterBasedOnDbColumn("@Rounding", 2, CargoWise.Schema.Schema.GenericIntSchemaColumn);
				command.AddParameterBasedOnDbColumn("@ReverseSign", reverseSign, CargoWise.Schema.Schema.GenericBitSchemaColumn);
				return DataUtils.GetDataTableFromCommand(command);
			}
		}
	}
}
