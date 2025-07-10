using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Testing;

public class GuidedDecisionMakingMultiInvoiceLinesTargetTest : TestCaseWithFactory
{
	public void TestTariffCode()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_Tariff = "1111111111";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_Tariff = "2222222222";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_Tariff = "3333333333";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.TariffCode = "4444444444";
		AssertEquals("Line 1 should be updated", "4444444444", invoiceline.JI_Tariff);
		AssertEquals("Line 2 should be updated", "4444444444", invoiceline2.JI_Tariff);
		AssertEquals("Line 3 should not be updated", "3333333333", invoiceline3.JI_Tariff);
	}

	public void TestCountryOfOrigin()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CountryOfOrigin = "FR";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CountryOfOrigin = "DE";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CountryOfOrigin = "BE";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CountryOfOrigin = "IT";
		AssertEquals("Line 1 should be updated", "IT", invoiceline.JI_CountryOfOrigin);
		AssertEquals("Line 2 should be updated", "IT", invoiceline2.JI_CountryOfOrigin);
		AssertEquals("Line 3 should not be updated", "BE", invoiceline3.JI_CountryOfOrigin);
	}

	public void TestPreference()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_PrimaryPreference = "pr1";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_PrimaryPreference = "pr2";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_PrimaryPreference = "pr3";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.Preference = "pr4";
		AssertEquals("Line 1 should be updated", "pr4", invoiceline.JI_PrimaryPreference);
		AssertEquals("Line 2 should be updated", "pr4", invoiceline2.JI_PrimaryPreference);
		AssertEquals("Line 3 should not be updated", "pr3", invoiceline3.JI_PrimaryPreference);
	}

	public void TestQuotaOrderNumber()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_ConcessionOrder = "ord1";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_ConcessionOrder = "ord2";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_ConcessionOrder = "ord3";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.QuotaOrderNumber = "ord4";
		AssertEquals("Line 1 should be updated", "ord4", invoiceline.JI_ConcessionOrder);
		AssertEquals("Line 2 should be updated", "ord4", invoiceline2.JI_ConcessionOrder);
		AssertEquals("Line 3 should not be updated", "ord3", invoiceline3.JI_ConcessionOrder);
	}

	public void TestCustomsFirstQuantity()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CustomsQuantity = 12;
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CustomsQuantity = 13;
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CustomsQuantity = 14;

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CustomsFirstQuantity = 15;
		AssertEquals("Line 1 should be updated", 15m, invoiceline.JI_CustomsQuantity);
		AssertEquals("Line 2 should be updated", 15m, invoiceline2.JI_CustomsQuantity);
		AssertEquals("Line 3 should not be updated", 14m, invoiceline3.JI_CustomsQuantity);
	}

	public void TestCustomsFirstUnitQty()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CustomsUnitQty = "KGM";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CustomsUnitQty = "KGM";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CustomsUnitQty = "KGM";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CustomsFirstUnitQty = "LTR";
		AssertEquals("Line 1 should be updated", "LTR", invoiceline.JI_CustomsUnitQty);
		AssertEquals("Line 2 should be updated", "LTR", invoiceline2.JI_CustomsUnitQty);
		AssertEquals("Line 3 should not be updated", "KGM", invoiceline3.JI_CustomsUnitQty);
	}

	public void TestCustomsSecondQuantity()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CustomsSecondQuantity = 12;
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CustomsSecondQuantity = 13;
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CustomsSecondQuantity = 14;

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CustomsSecondQuantity = 15;
		AssertEquals("Line 1 should be updated", 15m, invoiceline.JI_CustomsSecondQuantity);
		AssertEquals("Line 2 should be updated", 15m, invoiceline2.JI_CustomsSecondQuantity);
		AssertEquals("Line 3 should not be updated", 14m, invoiceline3.JI_CustomsSecondQuantity);
	}

	public void TestCustomsSecondUnitQty()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CustomsSecondUnitQty = "KGM";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CustomsSecondUnitQty = "KGM";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CustomsSecondUnitQty = "KGM";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CustomsSecondUnitQty = "LTR";
		AssertEquals("Line 1 should be updated", "LTR", invoiceline.JI_CustomsSecondUnitQty);
		AssertEquals("Line 2 should be updated", "LTR", invoiceline2.JI_CustomsSecondUnitQty);
		AssertEquals("Line 3 should not be updated", "KGM", invoiceline3.JI_CustomsSecondUnitQty);
	}

	public void TestCustomsThirdQuantity()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CustomsThirdQuantity = 12;
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CustomsThirdQuantity = 13;
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CustomsThirdQuantity = 14;

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CustomsThirdQuantity = 15;
		AssertEquals("Line 1 should be updated", 15m, invoiceline.JI_CustomsThirdQuantity);
		AssertEquals("Line 2 should be updated", 15m, invoiceline2.JI_CustomsThirdQuantity);
		AssertEquals("Line 3 should not be updated", 14m, invoiceline3.JI_CustomsThirdQuantity);
	}

	public void TestCustomsThirdUnitQty()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.JI_CustomsThirdUnitQty = "KGM";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.JI_CustomsThirdUnitQty = "KGM";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.JI_CustomsThirdUnitQty = "KGM";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CustomsThirdUnitQty = "LTR";
		AssertEquals("Line 1 should be updated", "LTR", invoiceline.JI_CustomsThirdUnitQty);
		AssertEquals("Line 2 should be updated", "LTR", invoiceline2.JI_CustomsThirdUnitQty);
		AssertEquals("Line 3 should not be updated", "KGM", invoiceline3.JI_CustomsThirdUnitQty);
	}

	public void TestTaxOrFeeDetail()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		var tax1 = new TaxOrFeeDetailEntity() { VATCode = "RED", AdditionalCode = "V001", Category = "A001" };
		var tax2 = new TaxOrFeeDetailEntity() { VATCode = "STD", AdditionalCode = "V002", Category = "A001" };

		invoiceline.Lookups.TaxOrFeeDetailEntities.Add(tax1);
		invoiceline.Lookups.TaxOrFeeDetailEntities.Add(tax2);
		invoiceline2.Lookups.TaxOrFeeDetailEntities.Add(tax1);
		invoiceline2.Lookups.TaxOrFeeDetailEntities.Add(tax2);
		invoiceline3.Lookups.TaxOrFeeDetailEntities.Add(tax1);
		invoiceline3.Lookups.TaxOrFeeDetailEntities.Add(tax2);

		invoiceline.JI_TaxOrFeeDetail = tax1.PK;
		invoiceline2.JI_TaxOrFeeDetail = tax1.PK;
		invoiceline3.JI_TaxOrFeeDetail = tax1.PK;

		var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
		var selectedVAT = new Mock<GuidedDecisionMakingVAT>(gdmBasic);
		selectedVAT.Setup(v => v.AdditionalCode).Returns("V002");
		selectedVAT.Setup(v => v.VATCode).Returns("STD");

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.SetVATCode(selectedVAT.Object.VATCode, selectedVAT.Object.AdditionalCode);
		AssertEquals("Line 1 JI_TaxOrFeeDetail should be updated", tax2.PK, invoiceline.JI_TaxOrFeeDetail);
		AssertEquals("Line 1 JI_ZZF_NKTaxType should be updated", "STD", invoiceline.JI_ZZF_NKTaxType);
		AssertEquals("Line 2 JI_TaxOrFeeDetail should be updated", tax2.PK, invoiceline2.JI_TaxOrFeeDetail);
		AssertEquals("Line 2 JI_ZZF_NKTaxType should be updated", "STD", invoiceline.JI_ZZF_NKTaxType);
		AssertEquals("Line 3 JI_TaxOrFeeDetail should not be updated", tax1.PK, invoiceline3.JI_TaxOrFeeDetail);
		AssertEquals("Line 3 JI_ZZF_NKTaxType should not be updated", "RED", invoiceline3.JI_ZZF_NKTaxType);
	}

	public void TestSetAdditionalCodes()
	{
		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);
		var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
		var impTariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff).PK;
		Factory.Save();

		helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
		helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");
		helper.CreateTaxOrFee("VT3", 9999m, currentCountry, description: "VAT Three");

		var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
		helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate, endDate: endDate, additionalCode: "Z001");
		helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate, endDate: endDate, additionalCode: "Z002");
		helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT3", startDate: startDate, endDate: endDate, additionalCode: "Z003");
		helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT4", startDate: startDate, endDate: endDate, additionalCode: "Z004");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "99999999";
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "99999999";
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "99999999";

		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z001";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z002";
		invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z003";
		invoiceLine2.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z004";
		invoiceLine2.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z005";
		invoiceLine2.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z006";
		invoiceLine3.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z007";
		invoiceLine3.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z008";
		invoiceLine3.AdditionalSupplementaryCodes.AddNew().CY_Code = "Z009";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceLine, invoiceLine2 });
		guidedDecisionMakingTarget.SetAdditionalCodes(new List<ZString> { "DIP", "DIP2", "DIP3" });

		AssertEquals("Line 1 JI_SupplementaryCode1 should be updated", "DIP", invoiceLine.JI_SupplementaryCode1);
		AssertEquals("Line 1 JI_SupplementaryCode2 should be updated", "DIP2", invoiceLine.JI_SupplementaryCode2);
		AssertEquals("Line 2 JI_SupplementaryCode1 should be updated", "DIP", invoiceLine2.JI_SupplementaryCode1);
		AssertEquals("Line 2 JI_SupplementaryCode2 should be updated", "DIP2", invoiceLine2.JI_SupplementaryCode2);
		AssertEquals("Line 3 JI_SupplementaryCode1 should not be updated", "", invoiceLine3.JI_SupplementaryCode1);
		AssertEquals("Line 3 JI_SupplementaryCode2 should not be updated", "", invoiceLine3.JI_SupplementaryCode2);

		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
		invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
		invoiceLine2.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine2.JI_SupplementaryCode1 = ZString.Empty;
		invoiceLine2.JI_SupplementaryCode2 = ZString.Empty;
		invoiceLine3.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine3.JI_SupplementaryCode1 = ZString.Empty;
		invoiceLine3.JI_SupplementaryCode2 = ZString.Empty;

		new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceLine, invoiceLine2 });
		guidedDecisionMakingTarget.SetAdditionalCodes(new List<ZString> { "DIP4", "DIP5", "DIP6" });

		AssertEquals("Line 1 JI_SupplementaryCode1 should be updated", "DIP4", invoiceLine.JI_SupplementaryCode1);
		AssertEquals("Line 1 JI_SupplementaryCode2 should be updated", "DIP5", invoiceLine.JI_SupplementaryCode2);
		AssertEquals("Line 2 JI_SupplementaryCode1 should be updated", "DIP4", invoiceLine2.JI_SupplementaryCode1);
		AssertEquals("Line 2 JI_SupplementaryCode2 should be updated", "DIP5", invoiceLine2.JI_SupplementaryCode2);
		AssertEquals("Line 3 JI_SupplementaryCode1 should not be updated", ZString.Empty, invoiceLine3.JI_SupplementaryCode1);
		AssertEquals("Line 3 JI_SupplementaryCode2 should not be updated", ZString.Empty, invoiceLine3.JI_SupplementaryCode2);
	}

	public void TestSetSupportingAndAdditionalDocumentsInUCC5()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (new DeclarationValidationDeciderTestContext(declaration, isUCC6: false))
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();

			var sup1 = invoiceline.SupportingDocuments.AddNew();
			sup1.FillWithValidTestData();
			sup1.CSI_Code = "0001";
			sup1.CSI_DateOfIssue = ZDateTime.Today;
			sup1.CSI_Quantity3 = 10;
			sup1.CSI_Value = 1;
			sup1.CSI_ReferenceNumber = "ref1";
			var sup2 = invoiceline.SupportingDocuments.AddNew();
			sup2.FillWithValidTestData();
			sup2.CSI_Code = "0002";
			sup2.CSI_DateOfIssue = ZDateTime.Today;
			sup2.CSI_Quantity3 = 10;
			sup2.CSI_Value = 1;
			sup2.CSI_ReferenceNumber = "ref2";

			var sup3 = invoiceline2.SupportingDocuments.AddNew();
			sup3.FillWithValidTestData();
			sup3.CSI_Code = "0003";
			sup3.CSI_DateOfIssue = ZDateTime.Today;
			sup3.CSI_Quantity3 = 10;
			sup3.CSI_Value = 1;
			sup3.CSI_ReferenceNumber = "ref31";

			AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine has 2 supporting documents", new ZString[] { "0001 ref1", "0002 ref2" }, invoiceline.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine2 has 1 supporting documents", new ZString[] { "0003 ref31" }, invoiceline2.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber).ToArray());

			var supToAdd = Factory.New<SupportingDocument>();
			supToAdd.FillWithValidTestData();
			supToAdd.CSI_Code = "0001";
			supToAdd.CSI_DateOfIssue = ZDateTime.Today;
			supToAdd.CSI_Quantity3 = 10;
			supToAdd.CSI_Value = 1;
			supToAdd.CSI_ReferenceNumber = "ref3";

			var supToAdd2 = Factory.New<SupportingDocument>();
			supToAdd2.FillWithValidTestData();
			supToAdd2.CSI_Code = "0004";
			supToAdd2.CSI_DateOfIssue = ZDateTime.Today;
			supToAdd2.CSI_Quantity3 = 10;
			supToAdd2.CSI_Value = 1;
			supToAdd2.CSI_ReferenceNumber = "ref4";

			var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });

			guidedDecisionMakingTarget.SetSupportingAndAdditionalDocuments(new List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> { ("0001", "ref3", new ZDateTime(2023, 06, 22)), ("0004", "ref4", new ZDateTime(2023, 06, 23)) });

			AssertContainsExactElementsInAnyOrder("Invoiceline: Existing supporting document 0001 should have been updated while supporting document 0004 should have been added to the invoice line.", new ZString[] { "0001 ref3", "0002 ref2", "0004 ref4" }, invoiceline.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber).ToArray());
			AssertContainsExactElementsInAnyOrder("Invoiceline2: Supporting document 0001, 0004 should have been added to the invoice line.", new ZString[] { "0001 ref3", "0003 ref31", "0004 ref4" }, invoiceline2.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber).ToArray());
		}
	}

	public void TestSetSupportingAndAdditionalDocumentsInUCC6()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
		{
			var declaration = Factory.New<JobDeclaration>();

			using (new DeclarationValidationDeciderTestContext(declaration, isUCC6: true))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceline = invoiceHeader.InvoiceLines.AddNew();
				var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();

				var supportingDocument = invoiceline.SupportingDocuments.AddNew();
				supportingDocument.FillWithValidTestData();
				supportingDocument.CSI_Code = "0001";
				supportingDocument.CSI_DateOfIssue = ZDateTime.MinSmallDateTimeValue;
				supportingDocument.CSI_ReferenceNumber = "ref1";

				var supportingDocument2 = invoiceline2.SupportingDocuments.AddNew();
				supportingDocument2.FillWithValidTestData();
				supportingDocument2.CSI_Code = "0006";
				supportingDocument2.CSI_DateOfIssue = ZDateTime.MinSmallDateTimeValue;
				supportingDocument2.CSI_ReferenceNumber = "ref6";

				var supportingDocumentToAdd1 = ("0001", "newRef1", new ZDateTime(2024, 12, 4));
				var supportingDocumentToAdd2 = ("0002", "ref2", new ZDateTime(2024, 12, 4));

				var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);

				var additionalInformation = invoiceline.AdditionalInfos.AddNew();
				additionalInformation.FillWithValidTestData();
				referenceDataHelper.CreateCusCodeList("FR", "AI44I", "0003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				additionalInformation.CSI_Code = "0003";
				additionalInformation.CSI_ReferenceNumber = "ref3";

				var additionalInformation2 = invoiceline2.AdditionalInfos.AddNew();
				additionalInformation2.FillWithValidTestData();
				referenceDataHelper.CreateCusCodeList("FR", "AI44I", "0007", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				additionalInformation2.CSI_Code = "0007";
				additionalInformation2.CSI_ReferenceNumber = "ref7";

				var additionalInformationToAdd = ("0003", "newRef3", new ZDateTime(2024, 12, 4));

				referenceDataHelper.CreateCusCodeList("FR", "AR44I", "0004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var additionalReferenceToAdd = ("0004", "ref4", new ZDateTime(2024, 12, 4));

				referenceDataHelper.CreateCusCodeList("FR", "TD44I", "0005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var transportDocumentToAdd = ("0005", "ref5", new ZDateTime(2024, 12, 4));

				Factory.Save();

				AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine has 1 supporting document", new string[] { "0001 ref1 01-Jan-00 00:00" }, invoiceline.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_DateOfIssue.FormatDateTime()).ToArray());

				AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine has 1 additional document", new string[] { "0003 ref3 " }, invoiceline.AdditionalInfos.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_SubType).ToArray());

				AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine2 has 1 supporting document", new string[] { "0006 ref6 01-Jan-00 00:00" }, invoiceline2.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_DateOfIssue.FormatDateTime()).ToArray());

				AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine2 has 1 additional document", new string[] { "0007 ref7 " }, invoiceline2.AdditionalInfos.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_SubType).ToArray());

				var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });

				guidedDecisionMakingTarget.SetSupportingAndAdditionalDocuments(new List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> { supportingDocumentToAdd1, supportingDocumentToAdd2, additionalInformationToAdd, additionalReferenceToAdd, transportDocumentToAdd });

				AssertContainsExactElementsInAnyOrder("InvoiceLine: Existing supporting document 0001 should have been updated while supporting document 0002 should have been added to the invoice line.", new string[] { "0001 newRef1 04-Dec-24 00:00", "0002 ref2 04-Dec-24 00:00" }, invoiceline.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_DateOfIssue.FormatDateTime()).ToArray());

				AssertContainsExactElementsInAnyOrder("InvoiceLine2: Existing supporting document 0006 should be unchanged. document 0001 should be added while supporting document 0002 should have been added to the invoice line.", new string[] { "0006 ref6 01-Jan-00 00:00", "0001 newRef1 04-Dec-24 00:00", "0002 ref2 04-Dec-24 00:00" }, invoiceline2.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_DateOfIssue.FormatDateTime()).ToArray());

				AssertContainsExactElementsInAnyOrder("InvoiceLine: Existing additional document 0003 should have been updated while additional documents 0004 and 0005 should have been added to the invoice line. When additional documents are updated or added their CSI_SubType should be automatically retrieved based on their code.", new string[] { "0003 newRef3 INF", "0004 ref4 REF", "0005 ref5 TRA" }, invoiceline.AdditionalInfos.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_SubType).ToArray());

				AssertContainsExactElementsInAnyOrder("InvoiceLine2: Existing additional document 0007 should be unchanged. document 0003 should be added while additional documents are updated or added their CSI_SubType should be automatically retrieved based on their code.", new string[] { "0007 ref7 ", "0003 newRef3 INF", "0004 ref4 REF", "0005 ref5 TRA" }, invoiceline2.AdditionalInfos.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_SubType).ToArray());
			}
		}
	}

	public void TestSetSupportingDocuments_TrimReference()
	{
		const string tooLong = "THIS REFERENCE IS TOO LONG 890123456789012345678901234567890";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "0001";
		var code0001 = ("0001", tooLong, ZDateTime.Empty);
		var code0002 = ("0002", tooLong, ZDateTime.Empty);

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget([invoiceLine, invoiceline2]);
		AssertNoExceptionThrown(() =>
			guidedDecisionMakingTarget.SetSupportingAndAdditionalDocuments([code0001, code0002])
		);

		AssertEquals("Updated reference (line 1, code 0001) should be trimmed to 50 characters", tooLong.Substring(0, 50), supportingDocument.CSI_ReferenceNumber);
		AssertEquals("Added reference (line 1, code 0002) should be trimmed to 50 characters", tooLong.Substring(0, 50), invoiceLine.SupportingDocuments[1].CSI_ReferenceNumber);
		AssertEquals("Both codes added to line2 should have references trimmed to 50 characters", tooLong.Substring(0, 50) + " " + tooLong.Substring(0, 50), string.Join(" ", invoiceline2.SupportingDocuments.Select(x => x.CSI_ReferenceNumber)));
	}

	public void TestCountryOfDestination()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceline = invoiceHeader.InvoiceLines.AddNew();
		invoiceline.ZG_CountryOfDestination = "FR";
		var invoiceline2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline2.ZG_CountryOfDestination = "DE";
		var invoiceline3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceline3.ZG_CountryOfDestination = "BE";

		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<JobComInvoiceLine> { invoiceline, invoiceline2 });
		guidedDecisionMakingTarget.CountryOfDestination = "IT";
		AssertEquals("Line 1 should be updated", "IT", invoiceline.ZG_CountryOfDestination);
		AssertEquals("Line 2 should be updated", "IT", invoiceline2.ZG_CountryOfDestination);
		AssertEquals("Line 3 should not be updated", "BE", invoiceline3.ZG_CountryOfDestination);
	}
}
