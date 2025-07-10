using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT.Testing
{
	public abstract class VATDataFileWriterTest : TestCaseWithFactory
	{
		public void TestGetPeriodYear()
		{
			AssertEquals("108", Writer.GetPeriodYear(201901));
		}

		public void TestGetPeriodMonth()
		{
			AssertEquals("01", Writer.GetPeriodMonth(201901));
		}

		public virtual void TestGetExTaxAmount()
		{
			var detail = new ComplianceDocumentHeaderDetails();
			detail.ExTaxAmount = 123;

			AssertEquals("000000000123", Writer.GetExTaxAmount(detail));
		}

		protected abstract ZString ReportType { get; }

		protected abstract VATDataFileWriter Writer { get; }

		protected AccComplianceReport Report;

		protected override void SetUp()
		{
			base.SetUp();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				Creator.CreateTestPeriodsForEntireYear(2019);

				Report = Factory.NewWithValidTestData<AccComplianceReport>();
				Report.ACR_ReportType = ReportType;
				Report.ACR_DateFrom = new ZDate(2019, 1, 1);
				Report.ACR_DateTo = new ZDate(2019, 1, 30);
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				var configCollection = CreateComplianceReportConfiguration();
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, configCollection);

				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, true);
			}

			Factory.Save();
		}

		protected virtual ComplianceReportConfigurationCollection CreateComplianceReportConfiguration()
		{
			var configCollection = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			configCollection.RemoveAndDeleteAll();
			var reportConfig = configCollection.AddNew();
			reportConfig.ReportCode = Report.ACR_ReportType;
			reportConfig.ReportTitle = Report.ACR_ReportType + " Report Title";
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
			reportConfig.Country = Report.Company.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "APC";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

			return configCollection;
		}

		protected void CreateSequenceBook(ZString code, ZString sequenceClass, ZString prefix)
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_Code = code;
			sequence.XD_SequenceClass = sequenceClass;
			sequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			sequence.XD_Prefix = prefix;
			sequence.XD_StartNumber = 1;
			sequence.XD_NextNumber = 1;
			sequence.XD_EndNumber = 6;
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_StartDate = new ZDate(2019, 1, 20);
			sequence.XD_ExpiryDate = new ZDateTime(2019, 1, 21);
			sequence.XD_GC_Company = Report.ACR_GC_Company;
		}

		protected void AssertEncoding(MemoryStream stream)
		{
			var buffer = new byte[5];
			stream.Read(buffer, 0, 5);
			// Here we only consider UTF8 and ASCII.
			// Please add new byte comparison here if need to introduce other encoding standards.
			var result = (buffer[0] == 0XEF && buffer[1] == 0XBB && buffer[2] == 0XBF) ? Encoding.UTF8 : Encoding.ASCII;

			AssertEquals(Encoding.ASCII, result);
		}

		#region Implementation

		protected TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;

		#endregion
	}
}
