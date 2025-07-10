using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Testing
{
	public class ComplianceReportDocumentDataCollectorTest : TestCaseWithFactory
	{
		[TestDate(2019, 3, 19)]
		public void TestGetDocumentHeaderDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				Creator.CreateTestPeriodsForEntireYear(2019);

				var sequenceBook = CreateAccComplianceSequence();

				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var org2 = Creator.CreateOrgHeader("org2", false, true);

				var cusCode1 = org1.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "12345675";

				var cusCode2 = org2.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = "VAT";
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode2.OK_CustomsRegNo = "565566675";

				Factory.Save();

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 10M, 0M, Creator.CC1.PK);
				line1.AL_AT = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "CAPVAT")).PK;
				line1.AL_TaxRateNumerator = 10;

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.TWD, 1M, org2);
				var line2 = Creator.CreateInvoiceLine(invoice2, Creator.TWD, 1M, 200M, 20M, 0M, Creator.CC1.PK);
				line2.AL_AT = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "VAT")).PK;
				line2.AL_TaxRateNumerator = 10;

				new ComplianceDocumentCreator(new[] { invoice, invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("2 compliance documents are created", 2, complianceDocuments.Length);
				complianceDocuments.OrderBy(x => x.Amount).ForEach(x =>
				{
					x.SetComplianceSequenceBook();
					x.SetComplianceDocumentNumber();
					x.ADH_SupportingDocumentType = "ABC";
					x.ADH_SupportingDocumentNumber = "12345678912345";
				});

				Factory.Save();

				Report.GenerateFromQueue();

				var dataCollector = new ComplianceReportDocumentDataCollector(Report);
				AssertEquals("2 compliance documents are collected", 2, dataCollector.ComplianceDocumentHeader.Length);

				var documentNumbers = new string[] { "AA00000002", "AA00000003" };
				foreach (var documentNumber in documentNumbers)
				{
					var headerDetail = dataCollector.ComplianceDocumentHeader.FirstOrDefault(x => x.DocumentNumber == documentNumber);
					AssertNotNull(headerDetail);
					AssertEquals("Ledger", "AR", headerDetail.Ledger);
					AssertEquals("ComplianceSubType", "TXC", headerDetail.ComplianceSubType);
					AssertEquals("VATRegistrationNum", documentNumber == "AA00000002" ? "12345675" : "565566675", headerDetail.VATRegistrationNum);
					AssertEquals("ReportingPeriod", 201903, headerDetail.ReportingPeriod);
					AssertEquals("DocumentNumber", documentNumber, headerDetail.DocumentNumber);
					AssertEquals("DocumentStatus", Core.Constants.ComplianceDocumentStatus.NumberSet, headerDetail.DocumentStatus);
					AssertEquals("ExTaxAmount", documentNumber == "AA00000002" ? 100M : 200M, headerDetail.ExTaxAmount);
					AssertEquals("TaxAmount", documentNumber == "AA00000002" ? 10M : 20M, headerDetail.TaxAmount);
					AssertEquals("CustomRelated", false, headerDetail.CustomRelated);
					AssertEquals("SupportingReason", ZString.Empty, headerDetail.SupportingReason);
					AssertEquals("DocumentDate", ZDateTime.Today, headerDetail.DocumentDate.Date);
					AssertEquals("SupportingDocumentType", "ABC", headerDetail.SupportingDocumentType);
					AssertEquals("SupportingDocumentNumber", "12345678912345", headerDetail.SupportingDocumentNumber);
					AssertCollectionContains("LineRateCode", documentNumber == "AA00000002" ? "CAPVAT" : "VAT", headerDetail.LineRateCode);
				}
			}
		}

		public void TestGetUnusedComplianceSequenceDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var sequenceBook = CreateAccComplianceSequence();

				var dataCollector = new ComplianceReportDocumentDataCollector(Report);
				var sequenceDetails = dataCollector.GetUnusedComplianceSequenceDetails();
				AssertEquals("1 compliance sequence are collected", 1, sequenceDetails.Length);
				AssertEquals("ComplianceSubType", "TXC", sequenceDetails[0].ComplianceSubType);
				AssertEquals("Prefix", "AA", sequenceDetails[0].Prefix);
				AssertEquals("NextNumber", 2M, sequenceDetails[0].NextNumber);
				AssertEquals("EndNumber", 6M, sequenceDetails[0].EndNumber);
				AssertEquals("MaximumNumberDigits", (ZByte)8, sequenceDetails[0].MaximumNumberDigits);
				AssertEquals("ExpiryDate", sequenceBook.XD_ExpiryDate, sequenceDetails[0].ExpiryDate);
			}
		}

		AccComplianceSequence CreateAccComplianceSequence()
		{
			var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequenceBook.XD_Code = "ABC";
			sequenceBook.XD_SequenceClass = "TXC";
			sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			sequenceBook.XD_Prefix = "AA";
			sequenceBook.XD_StartNumber = 1;
			sequenceBook.XD_NextNumber = 2;
			sequenceBook.XD_EndNumber = 6;
			sequenceBook.XD_MaximumNumberDigits = 8;
			sequenceBook.XD_StartDate = new ZDate(2019, 3, 19);
			sequenceBook.XD_ExpiryDate = new ZDate(2019, 3, 20);
			Factory.Save();

			return sequenceBook;
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupReport();
		}

		void SetupReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				Report = Factory.NewWithValidTestData<AccComplianceReport>();

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.Country = Env.CurrentCompany.Country.Code;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
				reportConfig.ReportLineOrdering = ReportLineOrderingListCodes.FormatCodeAndDocumentNumber;

				Creator.CreateConfigurationSettingsForComplianceReport(reportConfig
					, LedgerTypes.AccountsReceivable
					, ""
					, complianceSubType: "TXC");

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Report.ACR_ReportType = "TST";
				Report.ACR_DateFrom = new ZDate(2019, 3, 19);
				Report.ACR_DateTo = new ZDate(2019, 3, 20);
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				Factory.Save();
			}
		}

		AccComplianceReport Report;

		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;
	}
}
