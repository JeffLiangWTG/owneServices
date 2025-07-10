using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT.Testing
{
	sealed class PurchaseAndSalesVATWriterTest : VATDataFileWriterTest
	{
		[TestDate(2019, 1, 20)]
		public void TestGetDocumentHeaderDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				CreateSequenceBook("ABC", "TDP", "XX");
				Factory.Save();

				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var org2 = Creator.CreateOrgHeader("org2", false, true);
				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 10M, 0M, Creator.CC1.PK);
				line1.AL_AT = Creator.GST1.PK;

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.TWD, 1M, org2);
				var line2 = Creator.CreateInvoiceLine(invoice2, Creator.TWD, 1M, 200M, 20M, 0M, Creator.CC1.PK);
				line2.AL_AT = Creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice, invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
				AssertEquals("2 compliance documents are created", 2, complianceDocuments.Length);

				Factory.Save();

				Report.GenerateFromQueue();

				var headerDetails = Writer.GetDocumentHeaderDetails();
				AssertEquals("3 compliance documents are collected", 3, headerDetails.Length);
				var documentNumbers = headerDetails.Select(x => x.DocumentNumber);
				AssertCollectionContains("document XX00000001 is in collection to export", "XX00000001", documentNumbers);
				AssertCollectionContains("document XX00000002 is in collection to export", "XX00000002", documentNumbers);
				AssertCollectionContains("un-used sequence book with next number XX00000003 is in collection to export", "XX00000003", documentNumbers);
			}
		}

		[TestDate(2019, 1, 20)]
		public void TestGetDocumentHeaderDetailsFromDifferentPeriod()
		{
			// In product environment, report period composed by current year and month could not be duplicate. So, here the report would cover two month.
			Report.ACR_DateFrom = new ZDate(2019, 1, 1);
			Report.ACR_DateTo = new ZDate(2019, 2, 28);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				CreateSequenceBook("ABC", "TDP", "XX", new ZDate(2019, 1, 1), new ZDateTime(2019, 1, 31));
				CreateSequenceBook("ABC", "TDP", "XX", new ZDate(2019, 2, 1), new ZDateTime(2019, 2, 28));

				Factory.Save();

				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var org2 = Creator.CreateOrgHeader("org2", false, true);

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 10M, 0M, Creator.CC1.PK);
				line1.AL_AT = Creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				TestDateAttribute.AddDays(15);

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.TWD, 1M, org2);
				var line2 = Creator.CreateInvoiceLine(invoice2, Creator.TWD, 1M, 200M, 20M, 0M, Creator.CC1.PK);
				line2.AL_AT = Creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				Factory.Save();

				Report.GenerateFromQueue();

				AssertNoExceptionThrown("The exception should not occur while report contains more than one report periods", () => Writer.GetDocumentHeaderDetails());
			}
		}

		[TestDate(2019, 1, 20)]
		public void TestGetDocumentHeaderDetailsForCreditNote()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var org2 = Creator.CreateOrgHeader("org2", false, true);

				var creditNote1 = Creator.CreateARCreditNote("C001", org1, Creator.TWD, 1M);
				var creditNote1Line1 = Creator.CreateARCreditNoteLine(creditNote1, null, Creator.CC1, 10M, Creator.TWD, 1M);
				creditNote1Line1.AL_AT = Creator.GST1.PK;

				var creditNote2 = Creator.CreateARCreditNote("C002", org2, Creator.TWD, 1M);
				var creditNote2Line1 = Creator.CreateARCreditNoteLine(creditNote2, null, Creator.CC1, 10M, Creator.TWD, 1M);
				creditNote2Line1.AL_AT = Creator.GST1.PK;

				var documentHeaderList = new ComplianceDocumentCreator(new[] { creditNote1, creditNote2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				documentHeaderList.ForEach(header => header.ADH_DocumentNumber = "TX00000001");

				Factory.Save();

				Report.GenerateFromQueue();

				AssertNoExceptionThrown("The exception should not occur while report contains report documents from credit note with same document number", () => Writer.GetDocumentHeaderDetails());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 1, 20)]
		public void TestExportVATFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "12345675";

				var cusCode2 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = "GTX";
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode2.OK_CustomsRegNo = "565566675";

				Factory.Save();

				var capvat = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "CAPVAT"));
				var freevat = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "FREEVAT"));
				var exempt = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "EXEMPT"));
				capvat.AT_PostingGroupId = 1;
				freevat.AT_PostingGroupId = 2;
				exempt.AT_PostingGroupId = 3;

				var org1 = Creator.CreateOrgHeader("org1", false, true);
				org1.OH_RL_NKClosestPort = "TWTPE";
				var org2 = Creator.CreateOrgHeader("org2", false, true);
				org2.OH_RL_NKClosestPort = "TWTPE";

				var cusCode3 = org1.CustomsCodes.AddNew();
				cusCode3.OK_CodeType = "VAT";
				cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode3.OK_CustomsRegNo = "87654321";

				Factory.Save();

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 10M, 0M, Creator.CC1.PK);
				line1.AL_AT = capvat.PK;
				line1.AL_TaxRateNumerator = 10;
				var line2 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 200M, 0M, 0M, Creator.CC1.PK);
				line2.AL_AT = freevat.PK;
				var line3 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 300M, 0M, 0M, Creator.CC1.PK);
				line3.AL_AT = exempt.PK;

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.TWD, 1M, org2);
				var line4 = Creator.CreateInvoiceLine(invoice2, Creator.TWD, 1M, 400M, 40M, 0M, Creator.CC1.PK);
				line4.AL_AT = capvat.PK;

				new ComplianceDocumentCreator(new[] { invoice, invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("compliance documents are created", 4, complianceDocuments.Length);
				Factory.Save();

				CreateSequenceBook("002", "TXC", "AA");
				Factory.Save();

				complianceDocuments.OrderBy(x => x.Amount).ForEach(x =>
				{
					x.ADH_ComplianceSubType = "TXC";
					x.SetComplianceSequenceBook();
					x.SetComplianceDocumentNumber();
				});

				var voidComplianceDocument = complianceDocuments.First(x => x.ADH_OH_Organisation == org2.PK);
				voidComplianceDocument.Void();
				Factory.Save();

				Report.GenerateFromQueue();

				using (var stream = new MemoryStream())
				{
					Writer.WriteDataToStream(stream);

					AssertEquals(Writer.TotaItemsToComplete, Writer.CompletedItems);
					AssertNotEquals(0, Writer.TotaItemsToComplete);
					AssertEquals("VAT File Export Completed.", Writer.CurrentStatusText);
					AssertEncoding(stream);

					stream.Position = 0;
					string result = null;

					using (var reader = new StreamReader(stream))
					{
						result = reader.ReadToEnd();
					}
					AssertNotNull(result);
					AssertFileSameAsString(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\TW.VAT\TestFiles\PurchaseAndSalesVAT.txt", result);
				}
			}
		}

		[TestDate(2019, 1, 20)]
		public void TestExportVoidVATFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var cusCode1 = org1.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "87654321";

				var cusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "VAT";
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode.OK_CustomsRegNo = "12345675";
				Factory.Save();

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 10M, 0M, Creator.CC1.PK);
				line1.AL_AT = Creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
				AssertEquals("1 compliance documents are created", 1, complianceDocuments.Length);
				complianceDocuments[0].ADH_DocumentNumber = "AA00000001";
				complianceDocuments[0].Void();

				Factory.Save();

				Report.GenerateFromQueue();

				using (var stream = new MemoryStream())
				{
					Writer.WriteDataToStream(stream);

					AssertEquals(Writer.TotaItemsToComplete, Writer.CompletedItems);
					AssertNotEquals(0, Writer.TotaItemsToComplete);
					AssertEquals("VAT File Export Completed.", Writer.CurrentStatusText);
					AssertEncoding(stream);

					stream.Position = 0;
					string result = null;

					using (var reader = new StreamReader(stream))
					{
						result = reader.ReadToEnd();
					}
					AssertNotNull(result);
					var expectedString = @"35000000110801        12345675AA00000001000000000000F0000000000         
";
					AssertEquals(expectedString, result);
				}
			}
		}

		public override void TestGetExTaxAmount()
		{
			var detail = new ComplianceDocumentHeaderDetails();
			detail.Ledger = LedgerTypes.AccountsReceivable;
			detail.ExTaxAmount = 123;
			detail.TaxAmount = 5;
			detail.OrgHeaderCategory = "NAT";
			detail.TransactionType = "INV";

			AssertEquals("When is AR invoice and is NAT, GetExTaxAmount will be ExTaxAmount + TaxAmount.", "000000000128", Writer.GetExTaxAmount(detail));

			detail.OrgHeaderCategory = "BUS";
			detail.OrgHeaderCountryCode = "AU";

			AssertEquals("When is AR invoice and is non-TW BUS, GetExTaxAmount will be ExTaxAmount + TaxAmount.", "000000000128", Writer.GetExTaxAmount(detail));

			detail.OrgHeaderCategory = "NAT";
			detail.OrgHeaderCountryCode = "TW";
			detail.Ledger = LedgerTypes.AccountsPayable;
			detail.ComplianceSubType = "TDI";

			AssertEquals("When is AP and compliance subtype is duplicate compliance subType, GetExTaxAmount will be ExTaxAmount + TaxAmount.", "000000000128", Writer.GetExTaxAmount(detail));

			detail.ComplianceSubType = "TXI";

			AssertEquals("When is AP and compliance subtype is triplicate compliance subType, GetExTaxAmount will be ExTaxAmount.", "000000000123", Writer.GetExTaxAmount(detail));
		}

		public void TestGetTaxAmount()
		{
			var detail = new ComplianceDocumentHeaderDetails();
			detail.Ledger = LedgerTypes.AccountsReceivable;
			detail.ExTaxAmount = 123;
			detail.TaxAmount = 5;
			detail.ReportingPeriod = 202001;
			detail.LineRateCode = new ZString[] { "CAP" };
			detail.OrgHeaderCategory = "NAT";
			detail.TransactionType = "INV";

			var purchaseAndSalesVATWriter = new PurchaseAndSalesVATWriter(Report);
			var result = purchaseAndSalesVATWriter.BuildDocumentData(detail);

			var expectedString = @"  000000010901                00000000012810000000000         ";
			AssertEquals("When is AR invoice and is NAT, GetExTaxAmount will be ExTaxAmount + TaxAmount and GetTaxAmount is 0.", expectedString, result);

			detail.OrgHeaderCountryCode = "AU";
			detail.OrgHeaderCategory = "BUS";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			expectedString = @"  000000110901                00000000012810000000000         ";
			AssertEquals("When is AR invoice and is non-TW BUS, GetExTaxAmount will be ExTaxAmount + TaxAmount and GetTaxAmount is 0.", expectedString, result);

			detail.OrgHeaderCategory = "NAT";
			detail.Ledger = LedgerTypes.AccountsPayable;
			detail.ComplianceSubType = "TDI";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			expectedString = @"22000000210901                00000000012810000000000         ";

			AssertEquals("When is AP and compliance subtype is duplicate compliance subType, GetExTaxAmount will be ExTaxAmount + TaxAmount and GetTaxAmount is 0.", expectedString, result);

			detail.OrgHeaderCategory = "BUS";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			expectedString = @"22000000310901                00000000012810000000000         ";
			AssertEquals("When is AP and compliance subtype is duplicate compliance subType, GetExTaxAmount will be ExTaxAmount + TaxAmount and GetTaxAmount is 0.", expectedString, result);

			detail.OrgHeaderCategory = "NAT";
			detail.ComplianceSubType = "TXI";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			expectedString = @"21000000410901                00000000012310000000005         ";

			AssertEquals("When is AP and compliance subtype is triplicate compliance subType, GetExTaxAmount will be ExTaxAmount and GetTaxAmount is TaxAmount.", expectedString, result);

			detail.OrgHeaderCategory = "BUS";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			expectedString = @"21000000510901                00000000012310000000005         ";
			AssertEquals("When is AP and compliance subtype is triplicate compliance subType, GetExTaxAmount will be ExTaxAmount and GetTaxAmount is TaxAmount.", expectedString, result);
		}

		public void TestGetTaxCode()
		{
			var detail = new ComplianceDocumentHeaderDetails();
			detail.Ledger = LedgerTypes.AccountsReceivable;
			detail.ExTaxAmount = 123;
			detail.TaxAmount = 5;
			detail.ReportingPeriod = 202001;
			detail.LineRateCode = new ZString[] { "EXT" };
			detail.OrgHeaderCategory = "NAT";
			detail.TransactionType = "INV";

			var purchaseAndSalesVATWriter = new PurchaseAndSalesVATWriter(Report);
			var result = purchaseAndSalesVATWriter.BuildDocumentData(detail);

			var expectedString = @"00000000012810000000000";
			AssertContains("When it's NAT and LineRateCode is not all FREEVAT, TaxCode will be 1.", expectedString, result);

			detail.OrgHeaderCountryCode = "AU";
			detail.OrgHeaderCategory = "BUS";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			AssertContains("When it's non-TW BUS and LineRateCode is not all FREEVAT, TaxCode will be 1.", expectedString, result);

			detail.OrgHeaderCategory = "NAT";
			detail.LineRateCode = new ZString[] { "EXT", "FREEVAT" };
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			AssertContains("When it's NAT and LineRateCode is not all FREEVAT, TaxCode will be 1.", expectedString, result);

			detail.OrgHeaderCategory = "BUS";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			AssertContains("When it's non-TW BUS and LineRateCode is not all FREEVAT, TaxCode will be 1.", expectedString, result);

			detail.OrgHeaderCategory = "NAT";
			detail.LineRateCode = new ZString[] { "FREEVAT", "FREEVAT" };
			expectedString = @"00000000012820000000000";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);

			AssertContains("When LineRateCode is all FREEVAT, TaxCode will be 2.", expectedString, result);

			detail.OrgHeaderCategory = "BUS";
			result = purchaseAndSalesVATWriter.BuildDocumentData(detail);
			AssertContains("When LineRateCode is all FREEVAT, TaxCode will be 2.", expectedString, result);
		}

		protected override ComplianceReportConfigurationCollection CreateComplianceReportConfiguration()
		{
			var configCollection = base.CreateComplianceReportConfiguration();
			configCollection[0].ReportLineOrdering = ReportLineOrderingListCodes.FormatCodeAndDocumentNumber;

			Creator.CreateConfigurationSettingsForComplianceReport(configCollection[0]
				, LedgerTypes.AccountsReceivable
				, ""
				, complianceSubType: "TXC");

			Creator.CreateConfigurationSettingsForComplianceReport(configCollection[0]
				, LedgerTypes.AccountsReceivable
				, ""
				, complianceSubType: "TDP");

			Creator.CreateConfigurationSettingsForComplianceReport(configCollection[0]
				, LedgerTypes.AccountsReceivable
				, ""
				, complianceSubType: "TCD");

			return configCollection;
		}

		void CreateSequenceBook(ZString code, ZString sequenceClass, ZString prefix, ZDate startDate, ZDateTime expiryDate)
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
			sequence.XD_StartDate = startDate;
			sequence.XD_ExpiryDate = expiryDate;
			sequence.XD_GC_Company = Report.ACR_GC_Company;
		}

		protected override ZString ReportType => "TXT";

		protected override VATDataFileWriter Writer => writer ?? (writer = new PurchaseAndSalesVATWriter(Report));
		VATDataFileWriter writer;
	}
}
