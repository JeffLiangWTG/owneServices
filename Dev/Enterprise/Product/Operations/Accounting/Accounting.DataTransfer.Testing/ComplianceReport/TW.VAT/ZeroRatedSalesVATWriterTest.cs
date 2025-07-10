using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT.Testing
{
	sealed class ZeroRatedSalesVATWriterTest : VATDataFileWriterTest
	{
		[TestDate(2019, 1, 20)]
		public void TestGetDocumentHeaderDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				CreateSequenceBook("ABC", "TDP", "XX");
				Factory.Save();

				var freevat = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "FREEVAT"));

				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var org2 = Creator.CreateOrgHeader("org2", false, true);
				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 0M, 0M, Creator.CC1.PK);
				line1.AL_AT = freevat.PK;

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.TWD, 1M, org2);
				var line2 = Creator.CreateInvoiceLine(invoice2, Creator.TWD, 1M, 200M, 0M, 0M, Creator.CC1.PK);
				line2.AL_AT = freevat.PK;

				new ComplianceDocumentCreator(new[] { invoice, invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
				AssertEquals("2 compliance documents are created", 2, complianceDocuments.Length);

				Factory.Save();

				Report.GenerateFromQueue();

				var headerDetails = Writer.GetDocumentHeaderDetails();
				AssertEquals("2 compliance documents are collected", 2, headerDetails.Length);
				var documentNumbers = headerDetails.Select(x => x.DocumentNumber);
				AssertCollectionContains("document XX00000001 is in collection to export", "XX00000001", documentNumbers);
				AssertCollectionContains("document XX00000002 is in collection to export", "XX00000002", documentNumbers);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 1, 20)]
		public void TestExportVATFileWithZNGComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var item = collection.AddNew();
				item.Country = Core.Constants.CountryCodes.Taiwan;
				item.SubType = "ZNG";
				item.LedgerType = "AR";
				item.InvoiceType = "INV";
				item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
				item.DisbursementRule = DisbursementRuleCodes.AllTransactions; // "NDB";
				item.OriginalRule = OriginalRuleCodes.AllTransactions; //"OTO";
				item.TaxRegistrationLocationRule = Core.Constants.CountryCodes.Taiwan;
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				CreateSequenceBook("003", "ZNG", "BB");
				Factory.Save();

				var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "12345675";

				var org1 = Creator.CreateOrgHeader("org1", false, true);

				var cusCode3 = org1.CustomsCodes.AddNew();
				cusCode3.OK_CodeType = "VAT";
				cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode3.OK_CustomsRegNo = "22222222";

				Factory.Save();

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 0M, 0M, Creator.CC1.PK);
				line1.AL_AT = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "FREEVAT")).PK;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
				AssertEquals("compliance documents is created", 1, complianceDocuments.Length);

				Factory.Save();

				Report.GenerateFromQueue();

				using (var stream = new MemoryStream())
				{
					Writer.WriteDataToStream(stream);

					stream.Position = 0;
					string result = null;
					using (var reader = new StreamReader(stream))
					{
						result = reader.ReadToEnd();
					}
					AssertNotNull(result);
					AssertFileSameAsString(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\TW.VAT\TestFiles\ZeroRatedSalesVATForZNGSubType.txt", result);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 1, 20)]
		public void TestExportVATFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				CreateSequenceBook("002", "TXC", "AA");
				Factory.Save();

				var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "12345675";

				var cusCode2 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = "GTX";
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode2.OK_CustomsRegNo = "565566675";

				var org1 = Creator.CreateOrgHeader("org1", false, true);
				var org2 = Creator.CreateOrgHeader("org2", false, true);

				var cusCode3 = org1.CustomsCodes.AddNew();
				cusCode3.OK_CodeType = "VAT";
				cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode3.OK_CustomsRegNo = "22222222";

				var cusCode4 = org2.CustomsCodes.AddNew();
				cusCode4.OK_CodeType = "VAT";
				cusCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode4.OK_CustomsRegNo = "33333333";

				Factory.Save();

				var freevat = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "FREEVAT"));

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, org1);
				var line1 = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 0M, 0M, Creator.CC1.PK);
				line1.AL_AT = freevat.PK;

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.TWD, 1M, org2);
				var line2 = Creator.CreateInvoiceLine(invoice2, Creator.TWD, 1M, 200M, 0M, 0M, Creator.CC1.PK);
				line2.AL_AT = freevat.PK;

				new ComplianceDocumentCreator(new[] { invoice, invoice2 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(query);
				AssertEquals("compliance documents are created", 2, complianceDocuments.Length);

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
					AssertFileSameAsString(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\TW.VAT\TestFiles\ZeroRatedSalesVAT.txt", result);
				}
			}
		}

		public void TestProvinceCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var zeroRatedWriter = Writer as ZeroRatedSalesVATWriter;
				var address = GlbCompany.CurrentCompany.OrgProxy.Addresses.AddNew();

				var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "12345675";
				cusCode1.OK_OA_PremisesAddress = address.PK;

				var stateDict = new Dictionary<string, string>();
				stateDict.Add("TPE", "A");
				stateDict.Add("TXG", "B");
				stateDict.Add("KEE", "C");
				stateDict.Add("TNN", "D");
				stateDict.Add("KHH", "E");
				stateDict.Add("NWT", "F");
				stateDict.Add("ILA", "G");
				stateDict.Add("TAO", "H");
				stateDict.Add("CYI", "I");
				stateDict.Add("HSQ", "J");
				stateDict.Add("MIA", "K");
				stateDict.Add("NAN", "M");
				stateDict.Add("HSZ", "O");
				stateDict.Add("YUN", "P");
				stateDict.Add("CYQ", "Q");
				stateDict.Add("PIF", "T");
				stateDict.Add("HUA", "U");
				stateDict.Add("TTT", "V");
				stateDict.Add("KIN", "W");
				stateDict.Add("PEN", "X");
				stateDict.Add("LIE", "Z");

				foreach (var state in stateDict.Keys)
				{
					address.OA_State = state;
					AssertEquals($"state {state} should have mapping province code", stateDict[state], zeroRatedWriter.ProvinceCode_ForTestOnly);
				}

				address.OA_State = "XXX";
				AssertEquals("state XXX should not have mapping province code", " ", zeroRatedWriter.ProvinceCode_ForTestOnly);
			}
		}

		protected override ComplianceReportConfigurationCollection CreateComplianceReportConfiguration()
		{
			var configCollection = base.CreateComplianceReportConfiguration();
			configCollection[0].ReportLineOrdering = ReportLineOrderingListCodes.ComplianceDocumentNumber;

			Creator.CreateConfigurationSettingsForComplianceReport(configCollection[0]
				, LedgerTypes.AccountsReceivable
				, ""
				, taxInvoiceRule: TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount
				, complianceSubType: "TXC");

			Creator.CreateConfigurationSettingsForComplianceReport(configCollection[0]
				, LedgerTypes.AccountsReceivable
				, ""
				, taxInvoiceRule: TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount
				, complianceSubType: "TDP");

			Creator.CreateConfigurationSettingsForComplianceReport(configCollection[0]
				, LedgerTypes.AccountsReceivable
				, ""
				, taxInvoiceRule: TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount
				, complianceSubType: "ZNG");

			return configCollection;
		}

		protected override ZString ReportType => "T02";

		protected override VATDataFileWriter Writer => writer ?? (writer = new ZeroRatedSalesVATWriter(Report));
		VATDataFileWriter writer;
	}
}
