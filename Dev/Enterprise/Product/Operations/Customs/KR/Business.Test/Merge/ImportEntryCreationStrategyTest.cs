using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ImportEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill1.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var strategy = new ImportEntryCreationStrategy(declaration);
			var keys = strategy.GetKeyForHeader(invoiceLine).Keys;

			CombineAssertions("MergeKeys for Headers", () =>
			{
				Assert("JZ_CU_RelatedHouseBill", keys.Contains(invoice.JZ_CU_RelatedHouseBill));
				Assert("JZ_OA_ManufacturerAddress", keys.Contains(invoice.JZ_OA_ManufacturerAddress));
				Assert("JZ_IncoTerm", keys.Contains(invoice.JZ_IncoTerm));
				Assert("JZ_RX_NKInvoice_Currency", keys.Contains(invoice.JZ_RX_NKInvoice_Currency));
				Assert("JZ_PaymentTerms", keys.Contains(invoice.JZ_PaymentTerms));
				Assert("JZ_OH_Supplier", keys.Contains(invoice.JZ_OH_Supplier));
				Assert("JZ_OA_DistributorAddress", keys.Contains(invoice.JZ_OA_DistributorAddress));
				Assert("JZ_OA_SellerAddress", keys.Contains(invoice.JZ_OA_SellerAddress));
				Assert("JZ_OA_ShipperAddress", keys.Contains(invoice.JZ_OA_ShipperAddress));
				Assert("JZ_OH_SellingAgent", keys.Contains(invoice.JZ_OH_SellingAgent));
				Assert("JZ_ValuationCode", keys.Contains(invoice.JZ_ValuationCode));

				Assert("JZ_ValuationDecAttachCode", keys.Contains(invoice.JZ_ValuationDecAttachCode));
				Assert("JZ_BlanketValuationDeclarationNumber", keys.Contains(invoice.JZ_BlanketValuationDeclarationNumber));
				Assert("JZ_ImportCargoManagementNumber", keys.Contains(invoice.JZ_ImportCargoManagementNumber));
				Assert("JZ_OnlineTradeType", keys.Contains(invoice.JZ_OnlineTradeType));
				Assert("JZ_COOStatus", keys.Contains(invoice.JZ_COOStatus));
			});

			AssertEquals(21, keys.Count);
			Assert(keys.Contains(ZGuid.Empty));
			Assert(!keys.Contains(invoice.PK));
			Assert(!keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.N)));
			Assert(!keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.P + invoice.JZ_BlanketValuationDeclarationNumber)));

			invoice.JZ_ValuationDecAttachCode = "Y";
			strategy = new ImportEntryCreationStrategy(declaration);
			keys = strategy.GetKeyForHeader(invoiceLine).Keys;

			AssertEquals(21, keys.Count);
			Assert(keys.Contains(invoice.PK));
			Assert(!keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.N)));
			Assert(!keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.P + invoice.JZ_BlanketValuationDeclarationNumber)));

			invoice.JZ_ValuationDecAttachCode = "N";
			strategy = new ImportEntryCreationStrategy(declaration);
			keys = strategy.GetKeyForHeader(invoiceLine).Keys;

			AssertEquals(21, keys.Count);
			Assert(!keys.Contains(invoice.PK));
			Assert(keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.N)));
			Assert(!keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.P + invoice.JZ_BlanketValuationDeclarationNumber)));

			invoice.JZ_ValuationDecAttachCode = "P";
			strategy = new ImportEntryCreationStrategy(declaration);
			keys = strategy.GetKeyForHeader(invoiceLine).Keys;

			AssertEquals(21, keys.Count);
			Assert(!keys.Contains(invoice.PK));
			Assert(!keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.N)));
			Assert(keys.Contains(new ZString(ValueDeclarationAttachedCodeList.Codes.P + invoice.JZ_BlanketValuationDeclarationNumber)));
		}

		public void TestGetKeyForHeaderForJZ_Remarks()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var bill1 = declaration.Bills.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDecAttachCode = ValueDeclarationAttachedCodeList.Codes.N;
			invoice.JZ_CU_RelatedHouseBill = bill1.PK;
			invoice.JZ_OA_ManufacturerAddress = orgHeader.MainAddress.PK;
			invoice.JZ_OH_Supplier = orgHeader.PK;
			invoice.JZ_OA_DistributorAddress = orgHeader.MainAddress.PK;
			invoice.JZ_OA_SellerAddress = orgHeader.MainAddress.PK;
			invoice.JZ_OA_ShipperAddress = orgHeader.MainAddress.PK;
			invoice.JZ_OH_SellingAgent = orgHeader.MainAddress.PK;
			invoice.JZ_Remarks = ZString.Empty;
			
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var strategy = new ImportEntryCreationStrategy(declaration);
			var keys = strategy.GetKeyForHeader(invoiceLine).Keys;
			Assert(invoice.JZ_Remarks.IsEmpty);
			Assert(!keys.Contains(invoice.PK));
			Assert(keys.Contains(ZGuid.Empty));

			invoice.JZ_Remarks = "Test Remarks";
			keys = strategy.GetKeyForHeader(invoiceLine).Keys;
			Assert(!invoice.JZ_Remarks.IsEmpty);
			Assert(keys.Contains(invoice.PK));
			Assert(!keys.Contains(ZGuid.Empty));
		}

		public void TestMergeKeyForLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var certificateOfOrigin = Factory.New<CertificateOfOrigin>();
			certificateOfOrigin.CSI_ParentID = invoiceLine.PK;
			var strategy = new ImportEntryCreationStrategy(declaration);
			var keys = strategy.GetKeyForLine(invoiceLine).Keys;

			CombineAssertions("MergeKeys for Lines", () =>
			{
				Assert("JI_BrandName", keys.Contains(invoiceLine.JI_BrandName));
				Assert("JI_Model", keys.Contains(invoiceLine.JI_Model));
				Assert("JI_CountryOfOrigin", keys.Contains(invoiceLine.JI_CountryOfOrigin));
				Assert("JI_ParentLine", keys.Contains(invoiceLine.JI_ParentLine));
				Assert("JI_PrimaryPreference", keys.Contains(invoiceLine.JI_PrimaryPreference));
				Assert("JI_ZZF_NKTaxType", keys.Contains(invoiceLine.JI_ZZF_NKTaxType));
				Assert("JI_SecondaryPreference", keys.Contains(invoiceLine.JI_SecondaryPreference));
				Assert("AdditionalTariffCode", keys.Contains(invoiceLine.AdditionalTariffCode));

				Assert("JI_BrandCode", keys.Contains(invoiceLine.JI_BrandCode));
				Assert("JI_COOLabelLocation", keys.Contains(invoiceLine.JI_COOLabelLocation));
				Assert("JI_COOLabelType", keys.Contains(invoiceLine.JI_COOLabelType));
				Assert("JI_COOExemptionReason", keys.Contains(invoiceLine.JI_COOExemptionReason));
				Assert("JI_ProductTypeCode", keys.Contains(invoiceLine.JI_ProductTypeCode));
				Assert("JI_MightRequireInspection", keys.Contains(invoiceLine.JI_MightRequireInspection));
				Assert("JI_PostClearanceProcedureGA1", keys.Contains(invoiceLine.JI_PostClearanceProcedureGA1));
				Assert("JI_PostClearanceProcedureGA2", keys.Contains(invoiceLine.JI_PostClearanceProcedureGA2));
				Assert("JI_PostClearanceProcedureGA3", keys.Contains(invoiceLine.JI_PostClearanceProcedureGA3));
				Assert("JI_CourierCargoSelectivityIndicator", keys.Contains(invoiceLine.JI_CourierCargoSelectivityIndicator));
				Assert("JI_AdditionalDutyRate", keys.Contains(invoiceLine.JI_AdditionalDutyRate));
				Assert("JI_AdditionalDutyType", keys.Contains(invoiceLine.JI_AdditionalDutyType));
				Assert("JI_DomesticTaxCode", keys.Contains(invoiceLine.JI_DomesticTaxCode));
				Assert("JI_DomesticTaxExemptionCode", keys.Contains(invoiceLine.JI_DomesticTaxExemptionCode));
				Assert("JI_DrawbackUQ", keys.Contains(invoiceLine.JI_DrawbackUQ));
				Assert("JI_VATReductionCode", keys.Contains(invoiceLine.JI_VATReductionCode));
				Assert("JI_InstallmentCode", keys.Contains(invoiceLine.JI_InstallmentCode));
				Assert("JI_IsSpecificUseCode", keys.Contains(invoiceLine.JI_IsSpecificUseCode));
				Assert("JI_UseCode", keys.Contains(invoiceLine.JI_UseCode));
				Assert("JI_SpecificUseCodeDutyRatePermitNo", keys.Contains(invoiceLine.JI_SpecificUseCodeDutyRatePermitNo));
				Assert("JI_PCProcedure", keys.Contains(invoiceLine.JI_PCProcedure));

				Assert("CertificateOfOriginNo", keys.Contains(invoiceLine.CertificateOfOriginNo));
				Assert("CertificateOfOriginPersonName", keys.Contains(invoiceLine.CertificateOfOriginPersonName));
				Assert("CertificateOfOriginCriteriaCode", keys.Contains(invoiceLine.CertificateOfOriginCriteriaCode));
				Assert("CertificateOfOriginIssueDate", keys.Contains(invoiceLine.CertificateOfOriginIssueDate));
				Assert("CertificateOfOriginIssuingCountry", keys.Contains(invoiceLine.CertificateOfOriginIssuingCountry));
				Assert("CertificateOfOriginAgencyName", keys.Contains(invoiceLine.CertificateOfOriginAgencyName));
				Assert("CertificateOfOriginAreaName", keys.Contains(invoiceLine.CertificateOfOriginAreaName));
				Assert("CertificateOfOriginStatus", keys.Contains(invoiceLine.CertificateOfOriginStatus));
				Assert("CriteriaForDeterminingCountryOfOrigin", keys.Contains(invoiceLine.CriteriaForDeterminingCountryOfOrigin));
			});
		}
	}
}
