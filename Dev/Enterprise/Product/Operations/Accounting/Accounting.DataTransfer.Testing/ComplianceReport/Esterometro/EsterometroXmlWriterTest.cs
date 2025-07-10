using System;
using System.IO;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.Testing
{
	sealed class EsterometroXmlWriterTest : EsterometroTestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10)]
		public void TestWriteEsterometroReportAndValidateWithXsd()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = CreateComplianceReportForXsdTest();
				var writer = new EsterometroXmlWriter(report, indentXmlOutput: true);       // Pretty print XML output for ease of testing.
				using (var stream = new MemoryStream())
				{
					writer.InitializeProgressForm();
					AssertEquals("There should be 6 progress steps after initialization.", 6, writer.TotaItemsToComplete);

					writer.WriteXmlToStream(stream, fileNumber: 1);

					AssertEquals("After writing XML to stream, there should be one item to complete (XSD validation)", writer.TotaItemsToComplete - 1, writer.CompletedItems);
					AssertNotEquals("There must be items to complete for progress updates to work", 0, writer.TotaItemsToComplete);
					AssertEquals("Esterometro XML Generation Completed (file 1 of 1)", writer.CurrentStatusText);

					var notify = new NotificationBuffer();
					writer.ValidateXml(stream, notify, 1);
					Assert("There should be no Errors in XSD validation", !notify.HasErrors);
					Assert("There should be no Warnings in XSD validation", !notify.HasWarnings);
					AssertEquals("After validating XSD, all items should be complete", writer.TotaItemsToComplete, writer.CompletedItems);
					AssertEquals("Validating the Esterometro XML... (file 1 of 1)", writer.CurrentStatusText);

					stream.Position = 0;
					string xml = null;
					using (var reader = new StreamReader(stream))
					{
						xml = reader.ReadToEnd();
					}
					AssertNotNull(xml);
					AssertFileSameAsString(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\Esterometro\TestFiles\Esterometro-IT.xml", xml);
				}
			}
		}

		[TestDate(2018, 9, 10)]
		public void TestEsterometroXsdValidationFailure()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = CreateComplianceReportForXsdTest();
				var writer = new EsterometroXmlWriterForTestingXsdValidationFailure(report, indentXmlOutput: true);       // Pretty print XML output for ease of testing.
				writer.InitializeProgressForm();
				using (var stream = new MemoryStream())
				{
					writer.WriteXmlToStream(stream, fileNumber: 1);

					var notify = new NotificationBuffer();
					writer.ValidateXml(stream, notify, 1);
					Assert("There should be errors in XSD validation when invalid XML is created.", notify.HasErrors);
					Assert("There should be one notification in XSD validation when invalid XML is created.", notify.Events.Length == 1);
					AssertEquals("The error notification should be for the IncorrectHeader / DatiFattura element in XSD validation when invalid XML is created.", "The 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v2.0:IncorrectHeader' element is not declared. (Line 2, Position 2)", notify.Events[0].Message);
					AssertEquals("After validating XSD, all items should be complete", writer.TotaItemsToComplete, writer.CompletedItems);
				}
			}
		}

		#region Progressive Number Tests
		public void TestProgressiveNumberFormat()
		{
			AssertExceptionThrown<ArgumentOutOfRangeException>("Negative numbers are not possible for formatted progressive number.", () => (-1).ProgressiveNumberToBase36());
			AssertEquals("00000", 0.ProgressiveNumberToBase36());
			AssertEquals("00001", 1.ProgressiveNumberToBase36());
			AssertEquals("0000A", 10.ProgressiveNumberToBase36());
			AssertEquals("0000Z", 35.ProgressiveNumberToBase36());
			AssertEquals("00010", 36.ProgressiveNumberToBase36());
			AssertEquals("00100", 1296.ProgressiveNumberToBase36());
			AssertEquals("01000", 46656.ProgressiveNumberToBase36());
			AssertEquals("10000", 1679616.ProgressiveNumberToBase36());     // Suggested initial number to use if a company has already issued Esterometro reports.
			AssertEquals("ZZZZZ", 60466175.ProgressiveNumberToBase36());
			AssertExceptionThrown<ArgumentOutOfRangeException>("Numbers which cannot be formatted in 5 characters not possible for formatted progressive number.", () => 60466176.ProgressiveNumberToBase36());
		}

		public void TestIncrementSequenceNumber()
		{
			var report = CreateComplianceReportForNonXsdValidationTests();
			var writer = new EsterometroXmlWriter(report);
			AssertEquals("Precondition: ComplianceReportFileNextSequenceNumber is 1 by default.", 1, AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.Value);
			writer.IncrementSequenceNumber();
			AssertEquals("IncrementSequenceNumber() should increment ComplianceReportFileNextSequenceNumber.", 2, AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.Value);
			writer.IncrementSequenceNumber();
			AssertEquals("IncrementSequenceNumber() should increment ComplianceReportFileNextSequenceNumber again, when called twice.", 3, AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.Value);
		}
		#endregion

		#region Next Filename Tests
		public void TestNextFilename()
		{
			var report = CreateComplianceReportForNonXsdValidationTests();
			var writer = new EsterometroXmlWriter(report);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "66778899", Constants.CountryCodes.Italy);
				AssertEquals("Filename is created from IVA and progressive number.", $"IT66778899_DF_00001.xml", writer.GetNextFilename());
				AssertEquals("Filename does not change before IncrementSequenceNumber().", $"IT66778899_DF_00001.xml", writer.GetNextFilename());

				writer.IncrementSequenceNumber();
				AssertEquals("Filename changes after incrementing progressive number.", $"IT66778899_DF_00002.xml", writer.GetNextFilename());

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "22113366445", Constants.CountryCodes.Italy);
				AssertEquals("Filename changes for differnt IVA.", $"IT22113366445_DF_00002.xml", writer.GetNextFilename());
			}
		}
		#endregion

		#region Mutex Tests
		public void TestMutexPreventsMultipleWritersOfSameCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = CreateComplianceReportForNonXsdValidationTests();
				var writer = new EsterometroXmlWriter(report);
				using (var mutex = writer.CreateMutexForWriting())
				{
					Assert("Mutex should be acquired.", mutex.Lock());
					using (var mutex2 = writer.CreateMutexForWriting())
					{
						Assert("Mutex should prevent multiple writers of the same company.", !mutex2.Lock());
					}
				}

				using (var mutex = writer.CreateMutexForWriting())
				{
					Assert("Mutex should be acquired, after orginal mutex was released.", mutex.Lock());
				}
			}
		}

		public void TestMutexAllowsMultipleWritersOfDifferentCompany()
		{
			var companyTheFirst = Creator.CreateNewCompany("ITA", Core.Constants.CountryCodes.Italy);
			var branchTheFirst = Creator.CreateBranch("BIA", companyTheFirst);
			var companyTheSecond = Creator.CreateNewCompany("ITB", Core.Constants.CountryCodes.Italy);
			var branchTheSecond = Creator.CreateBranch("BIB", companyTheSecond);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchTheFirst.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var reportForFirstCompany = CreateComplianceReportForNonXsdValidationTests();
				AssertEquals("Precondition: report is for the first company.", companyTheFirst.PK, reportForFirstCompany.ACR_GC_Company);
				var writerForFirstCompany = new EsterometroXmlWriter(reportForFirstCompany);
				using (var mutexForFirstCompany = writerForFirstCompany.CreateMutexForWriting())
				{
					Assert("Mutex should be acquired for the first company.", mutexForFirstCompany.Lock());

					using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchTheSecond.PK.ToGuid(), Env.CurrentDepartmentPK))
					{
						var reportForSecondCompany = CreateComplianceReportForNonXsdValidationTests();
						AssertEquals("Precondition: report is for the second company.", companyTheSecond.PK, reportForSecondCompany.ACR_GC_Company);
						var writerForSecondCompany = new EsterometroXmlWriter(reportForFirstCompany);
						using (var mutexForSecondCompany = writerForSecondCompany.CreateMutexForWriting())
						{
							Assert("Mutex should allow multiple writers of different companies.", mutexForSecondCompany.Lock());
						}
					}
				}
			}
		}
		#endregion

		#region CalculateTotalFilesToBeWritten() Tests
		[TestDate(2018, 9, 10)]
		public void TestCalculateTotalFilesToBeWritten_ZeroTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = Creator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro);
				report.Factory.Save();
				var writer = new EsterometroXmlWriter(report);
				AssertEquals("One file should be required for zero transactions.", 1, writer.CalculateTotalFilesToBeWritten());
			}
		}

		[TestDate(2018, 9, 10)]
		public void TestCalculateTotalFilesToBeWritten_SingleFilePartiallyFilled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = Creator.CreateComplianceReportWithTransactions(AccComplianceReport.ReportTypes.Esterometro, periodicity: Lookups.ReportPeriodicityCodes.DateRange, numberOfTransactions: 1);
				var writer = new EsterometroXmlWriterWithCustomisablePageSize(report, indentXmlOutput: false, pageSize: 2);
				AssertEquals("One file should be required for one transaction, with 2 transactions per file.", 1, writer.CalculateTotalFilesToBeWritten());
			}
		}

		[TestDate(2018, 9, 10)]
		public void TestCalculateTotalFilesToBeWritten_SingleFileWithMaximumAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = Creator.CreateComplianceReportWithTransactions(AccComplianceReport.ReportTypes.Esterometro, periodicity: Lookups.ReportPeriodicityCodes.DateRange, numberOfTransactions: 2);
				var writer = new EsterometroXmlWriterWithCustomisablePageSize(report, indentXmlOutput: false, pageSize: 2);
				AssertEquals("One file should be required for two transactions, with 2 transactions per file.", 1, writer.CalculateTotalFilesToBeWritten());
			}
		}

		[TestDate(2018, 9, 10)]
		public void TestCalculateTotalFilesToBeWritten_TwoFilesPartial()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = Creator.CreateComplianceReportWithTransactions(AccComplianceReport.ReportTypes.Esterometro, periodicity: Lookups.ReportPeriodicityCodes.DateRange, numberOfTransactions: 3);
				var writer = new EsterometroXmlWriterWithCustomisablePageSize(report, indentXmlOutput: false, pageSize: 2);
				AssertEquals("Two file should be required for three transactions, with 2 transactions per file.", 2, writer.CalculateTotalFilesToBeWritten());
			}
		}

		[TestDate(2018, 9, 10)]
		public void TestCalculateTotalFilesToBeWritten_Larger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var report = Creator.CreateComplianceReportWithTransactions(AccComplianceReport.ReportTypes.Esterometro, periodicity: Lookups.ReportPeriodicityCodes.DateRange, numberOfTransactions: 20);
				var writer = new EsterometroXmlWriterWithCustomisablePageSize(report, indentXmlOutput: false, pageSize: 6);
				AssertEquals("Four files should be required for twenty transactions, with 6 transactions per file.", 4, writer.CalculateTotalFilesToBeWritten());
			}
		}
		#endregion

		#region Test Helpers
		AccComplianceReport CreateComplianceReportForNonXsdValidationTests() => Factory.NewWithValidTestData<AccComplianceReport>();

		AccComplianceReport CreateComplianceReportForXsdTest(string invoiceNumber = "I0000")
		{
			var report = Creator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro, AccComplianceReport.Status.ReportGenerated, Lookups.ReportPeriodicityCodes.AccountingPeriod);
			Factory.Save();

			Creator.ABIGAS.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GovBusinessCode, "2233445661", Core.Constants.CountryCodes.Australia);
			Creator.ABIGAS.MainAddress.OA_Address2 = "";
			Creator.ABIGAS.MainAddress.OA_City = "BOWEN HILLS";
			Creator.ABIGAS.MainAddress.OA_State = "QLD";

			ObjectCreator.CC1.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			ObjectCreator.CC2.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			Creator.TaxMsg1.A9_TaxGroupCode = "N2";

			var shipment = Creator.CreateShipment("S10002");
			var job = Creator.CreateJob(shipment);
			var apInvoice = Creator.CreateInvoice(typeof(APInvoice), "I0000", Creator.EUR, 1m, Creator.ABIGAS);
			apInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.APS;
			var line1 = Creator.CreateInvoiceLine(apInvoice, job, Creator.CC1, 100m, ObjectCreator.EUR, 1m);
			line1.AL_A9_VATClass = Creator.TaxMsg1.PK;
			ObjectCreator.CreateCharge(line1);
			var line2 = Creator.CreateInvoiceLine(apInvoice, job, Creator.CC2, 80m, ObjectCreator.EUR, 1m);
			line2.AL_AT = Creator.GST1.PK;
			line2.AL_A9_VATClass = Creator.TaxMsg1.PK;
			ObjectCreator.CreateCharge(line2);
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, apInvoice.Lines[0], 1, "");
			Creator.CreateComplianceReportTransactionPivot(report, apInvoice.Lines[1], 2, "");

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.IVA, "12345678901", Core.Constants.CountryCodes.Italy);
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2 = "";
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_City = "ALBION";
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_State = "QLD";

			return report;
		}
		#endregion

		#region Implementation

		TestObjectCreator Creator => ObjectCreator;

		internal class EsterometroXmlWriterForTestingXsdValidationFailure : EsterometroXmlWriter
		{
			public EsterometroXmlWriterForTestingXsdValidationFailure(AccComplianceReport report, bool indentXmlOutput)
				: base(report, indentXmlOutput)
			{
			}

			protected override string HeaderElementName => "IncorrectHeader";
		}

		internal class EsterometroXmlWriterWithCustomisablePageSize : EsterometroXmlWriter
		{
			public EsterometroXmlWriterWithCustomisablePageSize(AccComplianceReport report, bool indentXmlOutput, int pageSize)
				: base(report, indentXmlOutput)
			{
				fMaximumTransactionsPerFile = pageSize;
			}

			readonly int fMaximumTransactionsPerFile;
			protected override int MaximumTransactionsPerFile => fMaximumTransactionsPerFile;
		}
		#endregion
	}
}
