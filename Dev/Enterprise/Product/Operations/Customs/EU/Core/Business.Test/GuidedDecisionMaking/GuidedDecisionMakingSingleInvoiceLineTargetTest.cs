using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingSingleInvoiceLineTargetTest : TestCaseWithFactory
	{
		public void TestTariffCode()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_Tariff = "1234567894";

			AssertEquals("1234567894", invoiceline.JI_Tariff);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.TariffCode = "2345678945";
			AssertEquals("2345678945", invoiceline.JI_Tariff);
		}

		public void TestCountryOfOrigin()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CountryOfOrigin = "FR";

			AssertEquals("FR", invoiceline.JI_CountryOfOrigin);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CountryOfOrigin = "DE";
			AssertEquals("DE", invoiceline.JI_CountryOfOrigin);
		}

		public void TestPreference()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_PrimaryPreference = "pre";

			AssertEquals("pre", invoiceline.JI_PrimaryPreference);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.Preference = "pr2";
			AssertEquals("pr2", invoiceline.JI_PrimaryPreference);
		}

		public void TestQuotaOrderNumber()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_ConcessionOrder = "ord1";

			AssertEquals("ord1", invoiceline.JI_ConcessionOrder);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.QuotaOrderNumber = "ord2";
			AssertEquals("ord2", invoiceline.JI_ConcessionOrder);
		}

		public void TestCustomsFirstQuantity()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CustomsQuantity = 12;

			AssertEquals(12m, invoiceline.JI_CustomsQuantity);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CustomsFirstQuantity = 14;
			AssertEquals(14m, invoiceline.JI_CustomsQuantity);
		}

		public void TestCustomsFirstUnitQty()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CustomsUnitQty = "KGM";

			AssertEquals("KGM", invoiceline.JI_CustomsUnitQty);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CustomsFirstUnitQty = "LTR";
			AssertEquals("LTR", invoiceline.JI_CustomsUnitQty);
		}

		public void TestCustomsSecondQuantity()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CustomsSecondQuantity = 12;

			AssertEquals(12m, invoiceline.JI_CustomsSecondQuantity);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CustomsSecondQuantity = 14;
			AssertEquals(14m, invoiceline.JI_CustomsSecondQuantity);
		}

		public void TestCustomsSecondUnitQty()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CustomsSecondUnitQty = "KGM";

			AssertEquals("KGM", invoiceline.JI_CustomsSecondUnitQty);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CustomsSecondUnitQty = "LTR";
			AssertEquals("LTR", invoiceline.JI_CustomsSecondUnitQty);
		}

		public void TestCustomsThirdQuantity()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CustomsThirdQuantity = 12;

			AssertEquals(12m, invoiceline.JI_CustomsThirdQuantity);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CustomsThirdQuantity = 14;
			AssertEquals(14m, invoiceline.JI_CustomsThirdQuantity);
		}

		public void TestCustomsThirdUnitQty()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.JI_CustomsThirdUnitQty = "KGM";

			AssertEquals("KGM", invoiceline.JI_CustomsThirdUnitQty);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CustomsThirdUnitQty = "LTR";
			AssertEquals("LTR", invoiceline.JI_CustomsThirdUnitQty);
		}

		public void TestTaxOrFeeDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var taxOrFeeDetails = invoiceLine.Lookups.TaxOrFeeDetailEntities;
			var tax1 = new TaxOrFeeDetailEntity() { VATCode = "RED", AdditionalCode = "V001", Category = "A001" };
			var tax2 = new TaxOrFeeDetailEntity() { VATCode = "STD", AdditionalCode = "V002", Category = "A001" };
			var tax3 = new TaxOrFeeDetailEntity() { VATCode = "SRR", AdditionalCode = "", Category = "A001" };
			var tax4 = new TaxOrFeeDetailEntity() { VATCode = "RED", AdditionalCode = "", Category = "A001" };

			taxOrFeeDetails.Add(tax1);
			taxOrFeeDetails.Add(tax2);
			taxOrFeeDetails.Add(tax3);
			taxOrFeeDetails.Add(tax4);

			invoiceLine.JI_TaxOrFeeDetail = tax1.PK;

			var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			var selectedVAT = new Mock<GuidedDecisionMakingVAT>(gdmBasic);
			selectedVAT.Setup(v => v.AdditionalCode).Returns("V002");
			selectedVAT.Setup(v => v.VATCode).Returns("STD");
			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);
			guidedDecisionMakingTarget.SetVATCode(selectedVAT.Object.VATCode, selectedVAT.Object.AdditionalCode);
			AssertEquals("JI_TaxOrFeeDetail should be correctly set.", tax2.PK, invoiceLine.JI_TaxOrFeeDetail);
			AssertEquals("JI_ZZF_NKTaxType should be correctly set.", "STD", invoiceLine.JI_ZZF_NKTaxType);

			selectedVAT.Setup(v => v.AdditionalCode).Returns("");
			selectedVAT.Setup(v => v.VATCode).Returns("SRR");
			guidedDecisionMakingTarget.SetVATCode(selectedVAT.Object.VATCode, selectedVAT.Object.AdditionalCode);
			AssertEquals("JI_TaxOrFeeDetail should be correctly set.", tax3.PK, invoiceLine.JI_TaxOrFeeDetail);
			AssertEquals("JI_ZZF_NKTaxType should be correctly set.", "SRR", invoiceLine.JI_ZZF_NKTaxType);
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

			AssertContainsExactElementsInAnyOrder("Prerequisite: Effective Vat Applicabilites", new ZString[] { "Z001", "Z002", "Z003", "Z004" }, invoiceLine.GetEffectiveVATApplicabilities().Select(x => x.ZX5_AdditionalCode).ToArray());

			var suppCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode.CY_Code = "Z001";
			var suppCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode2.CY_Code = "Z002";
			var suppCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode3.CY_Code = "Z003";
			var suppCode4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode4.CY_Code = "Z004";
			var suppCode5 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode5.CY_Code = "VT3";

			AssertContainsExactElementsInAnyOrder("AdditionalSupplementaryCodes before.", new ZString[] { "Z001", "Z002", "Z003", "Z004", "VT3" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);

			guidedDecisionMakingTarget.SetAdditionalCodes(new System.Collections.Generic.List<ZString> { "DIP", "DIP2", "DIP3" });

			AssertContainsExactElementsInAnyOrder("AdditionalSupplementaryCodes after.", new ZString[] { "DIP", "DIP2", "DIP3" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
			AssertEquals("invoiceline Ji supplementary code 1.", "DIP", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("invoiceline Ji supplementary code 2.", "DIP2", invoiceLine.JI_SupplementaryCode2);

			invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();

			suppCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			suppCode.CY_Code = "Z001";

			AssertContainsExactElementsInAnyOrder("AdditionalSupplementaryCodes before : only Z001", new ZString[] { "Z001" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
			guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);

			guidedDecisionMakingTarget.SetAdditionalCodes(new System.Collections.Generic.List<ZString> { "DIP", "DIP2", "DIP3" });
			AssertContainsExactElementsInAnyOrder("AdditionalSupplementaryCodes after.", new ZString[] { "DIP", "DIP2", "DIP3" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
			AssertEquals("invoiceline Ji supplementary code 1.", "DIP", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("invoiceline Ji supplementary code 2.", "DIP2", invoiceLine.JI_SupplementaryCode2);

			invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;

			AssertEquals("AdditionalSupplementaryCodes before : empty list.", false, invoiceLine.SupplementaryCodes.Any());
			guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);

			guidedDecisionMakingTarget.SetAdditionalCodes(new System.Collections.Generic.List<ZString> { "DIP", "DIP2", "DIP3" });
			AssertContainsExactElementsInAnyOrder("AdditionalSupplementaryCodes after.", new ZString[] { "DIP", "DIP2", "DIP3" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
			AssertEquals("invoiceline Ji supplementary code 1.", "DIP", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("invoiceline Ji supplementary code 2.", "DIP2", invoiceLine.JI_SupplementaryCode2);
		}

		public void TestSetSupportingAndAdditionalDocumentsInUCC5()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (new DeclarationValidationDeciderTestContext(declaration, isUCC6: false))
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

				var sup1 = invoiceLine.SupportingDocuments.AddNew();
				sup1.FillWithValidTestData();
				sup1.CSI_Code = "0001";
				sup1.CSI_DateOfIssue = ZDateTime.Today;
				sup1.CSI_Quantity3 = 10;
				sup1.CSI_Value = 1;
				sup1.CSI_ReferenceNumber = "ref1";
				var sup2 = invoiceLine.SupportingDocuments.AddNew();
				sup2.FillWithValidTestData();
				sup2.CSI_Code = "0002";
				sup2.CSI_DateOfIssue = ZDateTime.Today;
				sup2.CSI_Quantity3 = 10;
				sup2.CSI_Value = 1;
				sup2.CSI_ReferenceNumber = "ref2";

				AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine has 2 supporting documents", new ZString[] { "0001 ref1", "0002 ref2" }, invoiceLine.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber).ToArray());

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

				var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);

				guidedDecisionMakingTarget.SetSupportingAndAdditionalDocuments(new System.Collections.Generic.List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> { ("0001", "ref3", new ZDateTime(2023, 06, 22)), ("0004", "ref4", new ZDateTime(2023, 06, 23)) });

				AssertContainsExactElementsInAnyOrder("Existing supporting document 0001 should have been updated while supporting document 0004 should have been added to the invoice line.", new ZString[] { "0001 ref3", "0002 ref2", "0004 ref4" }, invoiceLine.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber).ToArray());
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
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

					var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
					supportingDocument.FillWithValidTestData();
					supportingDocument.CSI_Code = "0001";
					supportingDocument.CSI_DateOfIssue = ZDateTime.MinSmallDateTimeValue;
					supportingDocument.CSI_ReferenceNumber = "ref1";

					var supportingDocumentToAdd1 = ("0001", "newRef1", new ZDateTime(2024, 12, 4));
					var supportingDocumentToAdd2 = ("0002", "ref2", new ZDateTime(2024, 12, 4));

					var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);

					var additionalInformation = invoiceLine.AdditionalInfos.AddNew();
					additionalInformation.FillWithValidTestData();
					referenceDataHelper.CreateCusCodeList("FR", "AI44I", "0003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					additionalInformation.CSI_Code = "0003";
					additionalInformation.CSI_ReferenceNumber = "ref3";

					var additionalInformationToAdd = ("0003", "newRef3", new ZDateTime(2024, 12, 4));

					referenceDataHelper.CreateCusCodeList("FR", "AR44I", "0004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					var additionalReferenceToAdd = ("0004", "ref4", new ZDateTime(2024, 12, 4));

					referenceDataHelper.CreateCusCodeList("FR", "TD44I", "0005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					var transportDocumentToAdd = ("0005", "ref5", new ZDateTime(2024, 12, 4));

					Factory.Save();

					AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine has 1 supporting document", new string[] { "0001 ref1 01-Jan-00 00:00" }, invoiceLine.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_DateOfIssue.FormatDateTime()).ToArray());

					AssertContainsExactElementsInAnyOrder("Prerequisite: InvoiceLine has 1 additional document", new string[] { "0003 ref3 " }, invoiceLine.AdditionalInfos.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_SubType).ToArray());

					var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);

					guidedDecisionMakingTarget.SetSupportingAndAdditionalDocuments(new System.Collections.Generic.List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> { supportingDocumentToAdd1, supportingDocumentToAdd2, additionalInformationToAdd, additionalReferenceToAdd, transportDocumentToAdd });

					AssertContainsExactElementsInAnyOrder("Existing supporting document 0001 should have been updated while supporting document 0002 should have been added to the invoice line.", new string[] { "0001 newRef1 04-Dec-24 00:00", "0002 ref2 04-Dec-24 00:00" }, invoiceLine.SupportingDocuments.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_DateOfIssue.FormatDateTime()).ToArray());

					AssertContainsExactElementsInAnyOrder("Existing additional document 0003 should have been updated while additional documents 0004 and 0005 should have been added to the invoice line. When additional documents are updated or added their CSI_SubType should be automatically retrieved based on their code.", new string[] { "0003 newRef3 INF", "0004 ref4 REF", "0005 ref5 TRA" }, invoiceLine.AdditionalInfos.Select(x => x.CSI_Code + " " + x.CSI_ReferenceNumber + " " + x.CSI_SubType).ToArray());
				}
			}
		}

		public void TestSetSupportingDocuments_TrimReference()
		{
			const string tooLong = "THIS REFERENCE IS TOO LONG 890123456789012345678901234567890";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "0001";
			var code0001Update = ("0001", tooLong, ZDateTime.Empty);
			var code0002Add = ("0002", tooLong, ZDateTime.Empty);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);
			AssertNoExceptionThrown(() =>
				guidedDecisionMakingTarget.SetSupportingAndAdditionalDocuments([code0001Update, code0002Add])
			);

			AssertEquals("Updated reference should be trimmed to 50 characters", tooLong.Substring(0, 50), supportingDocument.CSI_ReferenceNumber);
			AssertEquals("Added reference should be trimmed to 50 characters", tooLong.Substring(0, 50), invoiceLine.SupportingDocuments[1].CSI_ReferenceNumber);
		}

		public void TestCountryofDestination()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceline = invoiceHeader.InvoiceLines.AddNew();
			invoiceline.ZG_CountryOfDestination = "FR";
			AssertEquals("Prerequisite: ", "FR", invoiceline.ZG_CountryOfDestination);

			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceline);
			guidedDecisionMakingTarget.CountryOfDestination = "DE";
			AssertEquals("DE", invoiceline.ZG_CountryOfDestination);
		}
	}
}
