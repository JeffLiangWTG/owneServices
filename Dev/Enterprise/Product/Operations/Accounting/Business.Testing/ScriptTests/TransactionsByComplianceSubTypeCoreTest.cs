using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	abstract class TransactionsByComplianceSubTypeCoreTest : ScriptTest
	{
		[TestDate(2020, 11, 13)]
		public void TestNoExceptionWhenComplianceNumberUpdatedAfterAllocated()
		{
			if (ShouldTestComplianceNumber)
			{
				GlbCompany.CurrentCompany.SetCountry(CountryCodes.Mexico);
				SetupComplianceRuleRegistry();

				var newFactory = new BusinessObjectFactory();
				var testObjectCreator = new TestObjectCreator(newFactory);
				var menuPK = newFactory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice PE Factura")).PK;
				var sequence = testObjectCreator.CreateNewComplianceSequence(menuPK, "TXI", 1, 100, 25);
				sequence.XD_Prefix = "AAA";
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				newFactory.Save();

				var invoice = TestObjectCreator.CreateInvoice(TransactionType, "00001000", TestObjectCreator.USD);
				invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
				invoice.AH_Desc = "test";
				invoice.AH_TransactionReference = "";
				invoice.AH_ComplianceSubType = "TXI";
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_AT = TestObjectCreator.GST1.PK;
				Factory.Save();

				AssertEquals("should generate a sequence number", "AAA000000025", invoice.AH_TransactionReference);
				AssertTransactionComplianceNumber(invoice.PostDate, sequence.XD_Prefix, invoice.AH_TransactionReference.Remove(0, 3));

				invoice.AH_TransactionReference = "12";
				Factory.Save();

				AssertNoExceptionThrown("Expect no exception when compliance number doesn't come from  sequence book.", () => AssertTransactionComplianceNumber(invoice.PostDate, sequence.XD_Prefix, ZString.Empty));
			}
			else
			{
				Assert("Shouldn't test it for APTransactionsByComplianceSubType", true);
			}
		}

		public virtual bool ShouldTestComplianceNumber => true;

		void SetupComplianceRuleRegistry()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			item.OrganisationLocation = "";

			item = collection.AddNew();
			item.Country = CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AP";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			item.OrganisationLocation = "";
			item.SelfBillingRule = SelfBillingRuleCodes.StandardTransactions;

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
		}

		void AssertTransactionComplianceNumber(ZDateTime postDate, string prefix, string numberWithoutPrefix)
		{
			var headers = new[] { "TransactionNum", "BookSeries", "BookNumber" };

			var result = RunScript(postDate.ToString("yyyy-MM-dd"), postDate.ToString("yyyy-MM-dd"), "", "ALL", "", "BTH");

			var lines = new[] { new object[] { "00001000", prefix, numberWithoutPrefix } };

			AssertDataTableAllRowsByKeyColumns(TransactionType.Name, result, headers, lines);
		}

		[TestDate(2019, 03, 19)]
		public void TestTransactionsByComplianceSubTypeReport_PostBrexit_EuropeanUnionTransactionCountry_ReportGeneratedInGB()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var orgHeader = TestObjectCreator.ABIGAS;
			SetupHeaderWithClosestPortAndCustomsCode(orgHeader, "DE25B", Core.Constants.CountryCodes.Germany, "111111_DE", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Germany));

			var invoice1 = TestObjectCreator.CreateInvoice(TransactionType, "00001000", TestObjectCreator.GBP);
			invoice1.AH_OH = orgHeader.PK;
			invoice1.AH_PostDate = brexitDate.AddDays(-1);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoice(TransactionType, "00001001", TestObjectCreator.GBP);
			invoice2.AH_OH = orgHeader.PK;
			invoice2.AH_PostDate = brexitDate;
			Factory.Save();
			AssertTransactionRegNoForBrexit(invoice1.AH_PostDate, invoice2.AH_PostDate, Core.Constants.CountryCodes.UnitedKingdom, "DE25B", "111111_DE");
		}

		[TestDate(2019, 03, 19)]
		public void TestTransactionsByComplianceSubTypeReport_PostBrexit_GBTransactionCountry_ReportGeneratedInEU()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var orgHeader = TestObjectCreator.ABIGAS;
			SetupHeaderWithClosestPortAndCustomsCode(orgHeader, "GBLON", Core.Constants.CountryCodes.UnitedKingdom, "111111_GB", Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.UnitedKingdom));

			var invoice1 = TestObjectCreator.CreateInvoice(TransactionType, "00001000", TestObjectCreator.GBP);
			invoice1.AH_OH = orgHeader.PK;
			invoice1.AH_PostDate = brexitDate.AddDays(-1);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoice(TransactionType, "00001001", TestObjectCreator.GBP);
			invoice2.AH_OH = orgHeader.PK;
			invoice2.AH_PostDate = brexitDate;
			Factory.Save();
			AssertTransactionRegNoForBrexit(invoice1.AH_PostDate, invoice2.AH_PostDate, Core.Constants.CountryCodes.Germany, "GBLON", "111111_GB");
		}

		void AssertTransactionRegNoForBrexit(ZDateTime fromDate, ZDateTime toDate, string countryCodeToTemporarilySwitch, string transactionUNLOCOToAssert, string transactionRegNoToAssert)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeToTemporarilySwitch))
			{
				var headers = new[] { "TransactionNum", "TransactionUNLOCO", "TransactionRegNo" };

				var result = RunScript(fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), "", "ALL", "", "BTH");

				var lines = new[] {
					new object[] { "00001000", transactionUNLOCOToAssert, transactionRegNoToAssert },
					new object[] { "00001001", transactionUNLOCOToAssert, string.Empty }
				};

				AssertDataTableAllRowsByKeyColumns(TransactionType.Name, result, headers, lines);
			}
		}

		void SetupHeaderWithClosestPortAndCustomsCode(OrgHeader header, string closestPort, string countryCode, ZString customsRegNo, string codeType)
		{
			header.OH_RL_NKClosestPort = closestPort;
			OrgCusCode taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = countryCode;
			taxCode.OK_CustomsRegNo = customsRegNo;
			taxCode.OK_CodeType = codeType;
		}

		protected abstract DataTable RunScript(string fromDate, string toDate, string transactionType, string status, string subtype, string allocationLevel, string orgCusCode = null);
		protected abstract Type TransactionType { get; }
	}
}
