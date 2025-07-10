using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using BaseOrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;
using OrgSupplierPart = Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart;
using RefCusCodeList = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class JobComInvoiceLineTest<T> : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
		where T : JobComInvoiceLine
	{
		[ExpectNoExceptions]
		public virtual void TestAllAddInfoColumnsAreInModelView()
		{
			var jobComInvoiceLine = Factory.New<T>();

			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobComInvoiceLine, "EUJobComInvoiceLine");
		}

		public void TestGetGuidedDecisionMakingBasic()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var source = invoiceLine.GetGuidedDecisionMakingBasic();
			AssertEquals("GetGuidedDecisionMakingSource return type", ExpectedGuidedDecisionMakingBasicType, source.GetType());

			var source2 = invoiceLine.GetGuidedDecisionMakingBasicForMultiLine();
			AssertEquals("GetGuidedDecisionMakingSource return type", ExpectedGuidedDecisionMakingBasicType, source2.GetType());
		}

		protected virtual Type ExpectedGuidedDecisionMakingBasicType => typeof(GuidedDecisionMakingBasic);

		public virtual void TestComponentPrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_ValuationMarkup = 10m;
			AssertEquals("Prerequisite.", false, Declaration.Configuration.InvoiceLineConfiguration.InflateItemPriceByValuationMarkup(Declaration));
			AssertEquals("JI_ValuationMarkup should have no effect on ComponentPrice calculation in EU, because InflateItemPriceByValuationMarkup is false.", 50m, invoiceLine.ComponentPrice);
		}

		[ExpectNoExceptions]
		public void TestAddPackageWhenSettingInvoiceQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 69;
			package1.CW_PackType = "BX";

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 12;
			package2.CW_PackType = "CL";

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 30;
			package3.CW_PackType = "BX";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "BOX";

			AssertEquals(0, invoiceLine.PackagesPivot.Count);

			invoiceLine.JI_InvoiceUQ = "COI";
			AssertEquals(1, invoiceLine.PackagesPivot.Count);

			var packagePivot = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
			AssertEquals(5, packagePivot.CHC_NumberOfPacks);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = (ZDecimal)int.MaxValue + 10.0m;
			invoiceLine2.JI_InvoiceUQ = "COI";

			AssertEquals(0, invoiceLine2.PackagesPivot.Count);
		}

		public void TestJI_PrimaryPreferenceCaption()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_PrimaryPreference), false, x => x.Caption == "Preference" && x.ShortCaption == "Pref. Code");
		}

		public void TestJI_PrimaryPreferenceInstigatesCountryCodeValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleCD5151ActiveForZG_CountryOfSupply);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PrimaryPreference = "320";
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.CEI_Style = "I1";
				invoiceLine.JI_PrimaryPreference = "100";

				var countryOfSupplyRequiredForPreferenceMsgError = $"[CD5151] {invoiceLine.ZG_CountryOfSupplyInfo.Description} is required when the first digit of {invoiceLine.JI_PrimaryPreferenceInfo.Description} is '1', '4' or '5' and is not equal to Pref. Orig.";
				AssertHasMessageError("CEI_Style = 'I1' and PrimaryPreference starts with 4", invoiceLine.ZG_CountryOfSupplyInfo, countryOfSupplyRequiredForPreferenceMsgError);

				invoiceLine.JI_PrimaryPreference = "220";
				AssertNoMessageError(invoiceLine.ZG_CountryOfSupplyInfo, countryOfSupplyRequiredForPreferenceMsgError);
			}
		}

		public virtual void TestCountryOfSupplyDefaulting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("Pre-Req: DefaultCountryOfSupplyFromSupplier is false for this test to correctly expect no defaulting behavior", false, declaration.Configuration.InvoiceLineConfiguration.DefaultCountryOfSupplyFromSupplier(declaration));

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("An EU invoice line should not have its country of supply defaulted even when its declaration has its supplier country defined because this functionality is deactivated for EU.", ZString.Empty, invoiceLine.ZG_CountryOfSupply);
		}

		public virtual void TestEntryReferenceNumber()
		{
			var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
			var mergedReference = entryHeader.MergedLines.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Prior to merge", "Not Merged", InvoiceLine.EntryReferenceNumber);
				InvoiceLine.JI_CL = mergedReference.PK;
				mergedReference.CL_LineNumber = 456;
				AssertEquals("After merge without entry reference", "", InvoiceLine.EntryReferenceNumber);
				mergedReference.Header.CH_BGMReference = "EntryReference";
				AssertEquals("After merge with entry reference", "EntryReference", InvoiceLine.EntryReferenceNumber);
			});
		}

		public void TestDefaultCustomsWeightUnitIsKGM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("Default should be KGM", "KGM", invoiceLine.JI_CustomsUnitQty);
		}

		[TestDate(2015, 8, 22)]
		public void TestITaxAndDocsProviderValuationDate()
		{
			var invLine = Factory.New<T>();
			var td = invLine as ITaxAndDocsProvider;
			AssertEquals(new ZDateTime(2015, 8, 22), td.DateOfValuation);

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			inv.InvoiceLines.Add(invLine);
			invLine.JI_JZ = inv.PK;
			var entryHeader = dec.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "TST";
			var liana = new ZDateTime(1986, 3, 12);
			entryHeader.CusEntryNumber.CE_IssueDate = liana;
			AssertEquals(liana, td.DateOfValuation);
		}

		public virtual void TestEntryInstuctionDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.EntryInstructionDescription);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(ZString.Empty, invoiceLine.EntryInstructionDescription);

			instruction.CEI_Description = "test desc";
			AssertEquals("test desc", invoiceLine.EntryInstructionDescription);
		}

		public void TestCheckJI_CustomsQuantity_DerivesFrom_JI_NetWeight()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;
			invoiceLine.JI_NetWeight = 10m;

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
			AssertEquals(0.00001m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.LongTons;
			AssertEquals(10160.4691m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
			AssertEquals(0.283495m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = 10.6m;
			AssertEquals(0.300505m, invoiceLine.JI_CustomsQuantity);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;
			invoiceLine.JI_NetWeight = 10m;

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
			AssertEquals(0.00001m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.LongTons;
			AssertEquals(10160.4691m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
			AssertEquals(0.283495m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = 10.6m;
			AssertEquals(0.300505m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCanConvertFromNetWeightToCustomsUnit()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.JI_NetWeight = 10m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(true, invoiceLine.CanConvertFromNetWeightToCustomsUnit(RefCusCodeList.CustomsUq.Weight.Kilogram));

			invoiceLine.JI_NetWeight = 10m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("AB"));

			invoiceLine.JI_NetWeight = -10m;
			invoiceLine.JI_NetWeightUQ = "1";
			AssertEquals(false, invoiceLine.CanConvertFromNetWeightToCustomsUnit(Core.Constants.Weight.Grams));

			invoiceLine.JI_NetWeight = -1m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(false, invoiceLine.CanConvertFromNetWeightToCustomsUnit(RefCusCodeList.CustomsUq.Weight.Kilogram));

			invoiceLine.JI_NetWeight = 10m;
			invoiceLine.JI_NetWeightUQ = "2";
			AssertEquals(false, invoiceLine.CanConvertFromNetWeightToCustomsUnit(RefCusCodeList.CustomsUq.Weight.Kilogram));

			invoiceLine.JI_NetWeight = -1m;
			invoiceLine.JI_NetWeightUQ = "3";
			AssertEquals(false, invoiceLine.CanConvertFromNetWeightToCustomsUnit("AC"));
		}

		public virtual void TestMaxSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			AssertEquals("MaxSupportingDocuments default value", -1, invLine.MaxSupportingDocuments);
		}

		public virtual void TestGetSupportingDocumentsMaxCountReduction()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			AssertEquals("The functions returns 0", 0, invLine.GetSupportingDocumentsMaxCountReduction());
		}

		public void TestAllApplicableRatesSelectionCriteria()
		{
			InvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			InvoiceLine.JI_Tariff = "1234";
			InvoiceLine.JI_CountryOfOrigin = "US";
			InvoiceLine.JI_PrimaryPreference = "STD";
			InvoiceLine.JI_SecondaryPreference = "01";
			InvoiceLine.JI_ConcessionOrder = "TestOrder";
			InvoiceLine.JI_SupplementaryCode1 = "s1";
			InvoiceLine.JI_SupplementaryCode2 = "s2";
			var supplementarycode1 = InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			supplementarycode1.CY_Code = "addition1";
			var supplementarycode2 = InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			supplementarycode2.CY_Code = "addition2";

			var rateSelectionCriteria = InvoiceLine.AllApplicableRatesSelectionCriteria;
			AssertEquals("EffectiveDate", InvoiceLine.EffectiveAssessmentDate, rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 4, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("s1"));
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("s2"));
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("addition1"));
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("addition2"));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", "", rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);

			rateSelectionCriteria = InvoiceLine.DutyRateSelectionCriteria;
			AssertEquals("EffectiveDate", InvoiceLine.EffectiveAssessmentDate, rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "US", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 4, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("s1"));
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("s2"));
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("addition1"));
			Assert("AdditionalCode", rateSelectionCriteria.AdditionalCodes.Contains("addition2"));
			AssertEquals("ConcessionOrder", "TestOrder", rateSelectionCriteria.ConcessionOrder);
			AssertEquals("RateType", "DTY", rateSelectionCriteria.RateType);
			AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);
		}

		public void TestVATSelectionCriteria_AdditionalCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "tax1";
			var supplementarycode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			supplementarycode1.CY_Code = "addition1";
			invoiceLine.JI_SupplementaryCode1 = "addition2";
			invoiceLine.JI_SupplementaryCode2 = "addition3";

			var vatSelectionCriteria = invoiceLine.VATSelectionCriteria;
			AssertEquals("AdditionalCodes count", 3, vatSelectionCriteria.AdditionalCodes.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "addition1", "addition2", "addition3" }, vatSelectionCriteria.AdditionalCodes);
		}

		public void TestEvaluateConditionValue()
		{
			EvaluateConditionValueTest_ImportOrStandalone(InvoiceLine);
			EvaluateConditionValueTest_ExportAndUCC6(InvoiceLine);
		}

		public void TestEvaluateConditionValueWithStandaloneInvoice()
		{
			EvaluateConditionValueTest_ImportOrStandalone(InvoiceLine);
		}

		void EvaluateConditionValueTest_ExportAndUCC6(T invoiceLine)
		{
			var declaration = invoiceLine.Declaration;
			AssertNotNull(declaration);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				var doc1 = invoiceLine.AdditionalInfos.AddNew();
				doc1.CSI_Code = "C641";
				doc1.CSI_SubType = "REF";
				var doc2 = invoiceLine.AdditionalInfos.AddNew();
				doc2.CSI_Code = "Y073";
				doc2.CSI_SubType = "INF";
				var doc3 = invoiceLine.AdditionalInfos.AddNew();
				doc3.CSI_Code = "Y080";
				doc3.CSI_SubType = "REF";
				var doc4 = invoiceLine.InvoiceHeader.AdditionalInfos.AddNew();
				doc4.CSI_Code = "Y100";
				doc4.CSI_SubType = "REF";
				var doc5 = declaration.AdditionalInfos.AddNew();
				doc5.CSI_Code = "Y110";
				doc5.CSI_SubType = "REF";

				var doc6 = invoiceLine.SupportingDocuments.AddNew();
				doc6.CSI_Code = "C222";

				invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions.AddNew().PK;
				var doc7 = invoiceLine.EntryInstruction.SupportingDocuments.AddNew();
				doc7.CSI_Code = "XXX1";
				doc7.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("Test SUP/C641", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "C641"));
					AssertEquals("Test SNR/C641", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "C641"));

					AssertEquals("Test SUP/Y073", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y073"));
					AssertEquals("Test SNR/Y073", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y073"));

					AssertEquals("Test SUP/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y078"));
					AssertEquals("Test SNR/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y078"));

					AssertEquals("Test UNK/Y080", false, invoiceLine.EvaluateConditionValue("CTRL", "UNK", "Y080"));

					AssertEquals("Test SUP/Y100", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y100"));
					AssertEquals("Test SNR/Y100", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y100"));

					AssertEquals("Test SUP/Y110", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y110"));
					AssertEquals("Test SNR/Y110", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y110"));

					AssertEquals("Test SUP/C222", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "C222"));
					AssertEquals("Test SNR/C222", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "C222"));

					AssertEquals("Test SUP/XXX1", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "XXX1"));
					AssertEquals("Test SNR/XXX1", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "XXX1"));
				});
			}
		}

		void EvaluateConditionValueTest_ImportOrStandalone(T invoiceLine)
		{
			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_Code = "C640";
			doc1.CSI_ReferenceNumber = "123";
			var doc2 = invoiceLine.SupportingDocuments.AddNew();
			doc2.CSI_Code = "Y072";
			doc2.CSI_ReferenceNumber = "";
			var doc3 = invoiceLine.SupportingDocuments.AddNew();
			doc3.CSI_Code = "Y079";
			doc3.CSI_ReferenceNumber = "";
			var doc4 = invoiceLine.InvoiceHeader.SupportingDocuments.AddNew();
			doc4.CSI_Code = "Y099";
			doc4.CSI_ReferenceNumber = "";

			if (invoiceLine.Declaration != null)
			{
				invoiceLine.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var doc5 = Declaration.SupportingDocuments.AddNew();
				doc5.CSI_Code = "Y098";
				doc5.CSI_ReferenceNumber = "";
			}

			var doc6 = invoiceLine.AdditionalInfos.AddNew();
			doc6.CSI_Code = "Y088";
			doc6.CSI_SubType = "REF";

			invoiceLine.JI_CEI = invoiceLine.Declaration.CustomsEntryInstructions.AddNew().PK;
			var doc7 = invoiceLine.EntryInstruction.SupportingDocuments.AddNew();
			doc7.CSI_Code = "XXX2";
			doc7.CSI_SubType = "REF";

			if (invoiceLine.Declaration == null)
			{
				CombineAssertions("InvoiceLine is standalone", () =>
				{
					AssertEquals("Test SUP/C640", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "C640"));
					AssertEquals("Test SNR/C640", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "C640"));

					AssertEquals("Test SUP/Y072", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y072"));
					AssertEquals("Test SNR/Y072", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y072"));

					AssertEquals("Test SUP/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y078"));
					AssertEquals("Test SNR/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y078"));

					AssertEquals("Test UNK/Y079", false, invoiceLine.EvaluateConditionValue("CTRL", "UNK", "Y079"));

					AssertEquals("Test SUP/Y099", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y099"));
					AssertEquals("Test SNR/Y099", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y099"));

					if (invoiceLine.Declaration != null)
					{
						AssertEquals("Test SUP/Y098", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y098"));
						AssertEquals("Test SNR/Y098", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y098"));
					}

					AssertEquals("Test SUP/Y088", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y088"));
					AssertEquals("Test SNR/Y088", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y088"));

					AssertEquals("Test SUP/XXX2", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "XXX2"));
					AssertEquals("Test SNR/XXX2", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "XXX2"));
				});
			}
			else
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(invoiceLine.Declaration, "IsUCC6Core", false))
				{
					CombineAssertions("Declaration is not UCC6", () =>
					{
						AssertEquals("Test SUP/C640", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "C640"));
						AssertEquals("Test SNR/C640", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "C640"));

						AssertEquals("Test SUP/Y072", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y072"));
						AssertEquals("Test SNR/Y072", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y072"));

						AssertEquals("Test SUP/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y078"));
						AssertEquals("Test SNR/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y078"));

						AssertEquals("Test UNK/Y079", false, invoiceLine.EvaluateConditionValue("CTRL", "UNK", "Y079"));

						AssertEquals("Test SUP/Y099", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y099"));
						AssertEquals("Test SNR/Y099", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y099"));

						if (invoiceLine.Declaration != null)
						{
							AssertEquals("Test SUP/Y098", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y098"));
							AssertEquals("Test SNR/Y098", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y098"));
						}

						AssertEquals("Test SUP/Y088", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y088"));
						AssertEquals("Test SNR/Y088", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y088"));

						AssertEquals("Test SUP/XXX2", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "XXX2"));
						AssertEquals("Test SNR/XXX2", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "XXX2"));
					});
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(invoiceLine.Declaration, "IsUCC6Core", true))
				{
					CombineAssertions("Declaration is UCC6", () =>
					{
						AssertEquals("Test SUP/C640", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "C640"));
						AssertEquals("Test SNR/C640", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "C640"));

						AssertEquals("Test SUP/Y072", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y072"));
						AssertEquals("Test SNR/Y072", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y072"));

						AssertEquals("Test SUP/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y078"));
						AssertEquals("Test SNR/Y078", false, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y078"));

						AssertEquals("Test UNK/Y079", false, invoiceLine.EvaluateConditionValue("CTRL", "UNK", "Y079"));

						AssertEquals("Test SUP/Y099", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y099"));
						AssertEquals("Test SNR/Y099", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y099"));

						if (invoiceLine.Declaration != null)
						{
							AssertEquals("Test SUP/Y098", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y098"));
							AssertEquals("Test SNR/Y098", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y098"));
						}

						AssertEquals("Test SUP/Y088", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "Y088"));
						AssertEquals("Test SNR/Y088", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "Y088"));

						AssertEquals("Test SUP/XXX2", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "XXX2"));
						AssertEquals("Test SNR/XXX2", true, invoiceLine.EvaluateConditionValue("CTRL", Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "XXX2"));
					});
				}
			}
		}

		public void TestIsPreviousDocsRelevant()
		{
			var invoiceLine = Factory.New<T>();
			AssertEquals(true, invoiceLine.IsPreviousDocsRelevant);
		}

		public void TestASNRefresh()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = DefaultOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = DefaultOptions.Codes.CountryOfOrigin;

			CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var invoice = Factory.New<JobComInvoiceHeader>();
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPTEST";
			invoice.JZ_OH_Buyer = importer.PK;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPTEST";
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;

			var line = invoice.InvoiceLines.AddNew();
			var classification = Factory.New<CusClassification>();
			classification.FillWithValidTestData();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "AMYTEST";

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_RN_NKCountryOfOrigin = "CA";
			pivot.PreferenceCode = "SPI";
			var relatedOrg1 = product.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = importer.PK;
			relatedOrg1.OU_Relationship = "OWN";
			var relatedOrg2 = product.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = supplier.PK;
			relatedOrg2.OU_Relationship = "SUP";

			line.JI_PartNo = product.OP_PartNum;
			line.JI_OP = product.PK;
			AssertEquals(pivot, line.Pivot);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the registry", pivot.CI_CC, line.JI_CC);
			AssertEquals("Refreshed by the registry", pivot.CI_RN_NKCountryOfOrigin, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the registry", ZString.Empty, line.JI_Tariff);

			var companyData = importer.CompanyData;
			companyData.ImporterOverride = true;
			companyData.OB_IMProductValueDefaultOptions = "OVR,TAR,PREFF";

			invoice.JZ_JE = ZGuid.Empty;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = Customs.Business.JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			line.JI_CC = ZGuid.Empty;
			line.JI_CountryOfOrigin = ZString.Empty;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("Refreshed by the consignee config", ZGuid.Empty, line.JI_CC);
			AssertEquals("Refreshed by the consignee config", ZString.Empty, line.JI_CountryOfOrigin);
			AssertEquals("Refreshed by the consignee config", pivot.CI_TariffNum, line.JI_Tariff);
		}

		// Ensure GB has a test that inherits this

		public void TestInvoiceLineChargesCurrency_AllLocal()
		{
			// Everything in DKK. CIF is 1000DKK invoice amount, plus 100DKK freight outside EU, gives 1100DKK; then add another charges outside EU to give 1200DKK
			// CIF and customs values are equal because only one currency
			TestInvoiceLineChargesCurrency_Runner(false, 1100m, 1200m, 1100m, 1200m);
		}

		public void TestInvoiceLineChargesCurrency_Mixed()
		{
			// Mixed bag of charges.  Convert to DKK.  1000 USD invoice amount (=500DKK) , plus 100 USD freight outside EU (=50DKK), gives 550DKK.
			// Then add 100 AUD (= 25DKK) gives 575DKK.
			TestInvoiceLineChargesCurrency_Runner(true, 1100m, // CIF when invoice currency = charge currency; neither is local currency
														1150m,// CIF when invoice currency != all charge currencies, so convert second charge to invoice currency; neither is local currency
														550m, // Customs value with one charge
														575m); // customs value with two (mixed curr) charges
		}

		void TestInvoiceLineChargesCurrency_Runner(bool useMixedCurrencies, decimal expectedCiFAmountA, decimal expectedCiFAmountB, decimal expectedCustomsValueINCustomsCurrencyA, decimal expectedCustomsValueINCustomsCurrencyB)
		{
			var currencyCodeOneFakeUSD = "DJC";
			var currencyCodeTwoFakeAUD = "LSC";
			var fakeCurrencyOne = RefCurrency.New(Factory);
			fakeCurrencyOne.RX_Code = currencyCodeOneFakeUSD;
			fakeCurrencyOne.SetCustomsRate(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, 2m);
			var fakeCurrencyTwo = RefCurrency.New(Factory);
			fakeCurrencyTwo.RX_Code = currencyCodeTwoFakeAUD;
			fakeCurrencyTwo.SetCustomsRate(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, 4m);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = useMixedCurrencies ? currencyCodeOneFakeUSD : dec.LocalCurrencyCode.ToString();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)dec.IncoTermAndChargeFactory;
			var freightOutsideEuCharge = invoiceLine.Charges.AddNew();
			freightOutsideEuCharge.J7_ChargeType = chargeFactory.FreightToEUBorderCode;
			freightOutsideEuCharge.J7_IsDutiable = true;
			freightOutsideEuCharge.J7_IsGSTApplicable = true;
			freightOutsideEuCharge.J7_RX_NKCurrency = useMixedCurrencies ? currencyCodeOneFakeUSD : dec.LocalCurrencyCode.ToString();
			freightOutsideEuCharge.J7_Amount = 100m;

			// Don't assert FOB - base is flawed in its calculation, and EU doesn't care for it anyway.

			// Assert CIF Value is 1100, currency USD
			AssertEquals("CIF amount is " + expectedCiFAmountA.ToString(), expectedCiFAmountA, invoiceLine.JI_CIF.Amount);
			AssertEquals("CIF amount is in invoice currency", (useMixedCurrencies ? currencyCodeOneFakeUSD : dec.LocalCurrencyCode.ToString()), invoiceLine.JI_CIF.Currency.Code);
			AssertEquals("JI_CustomsValue is " + expectedCustomsValueINCustomsCurrencyA.ToString(), expectedCustomsValueINCustomsCurrencyA, invoiceLine.JI_CustomsValue);

			// Add another charge that will bring us to Customs Value (e.g. freight outside EU) in *AUD*, $100
			var freightOutsideEuChargeInAUD = invoiceLine.Charges.AddNew();
			freightOutsideEuChargeInAUD.J7_ChargeType = chargeFactory.FreightToEUBorderCode;
			freightOutsideEuChargeInAUD.J7_IsDutiable = true;
			freightOutsideEuChargeInAUD.J7_IsGSTApplicable = true;
			freightOutsideEuChargeInAUD.J7_RX_NKCurrency = useMixedCurrencies ? currencyCodeTwoFakeAUD : dec.LocalCurrencyCode.ToString();
			freightOutsideEuChargeInAUD.J7_Amount = 100m;

			AssertEquals("CIF amount is " + expectedCiFAmountB.ToString(), expectedCiFAmountB, invoiceLine.JI_CIF.Amount);
			AssertEquals("CIF amount is in invoice currency", (useMixedCurrencies ? currencyCodeOneFakeUSD : dec.LocalCurrencyCode.ToString()), invoiceLine.JI_CIF.Currency.Code);
			AssertEquals("JI_CustomsValue is " + expectedCustomsValueINCustomsCurrencyB.ToString(), expectedCustomsValueINCustomsCurrencyB, invoiceLine.JI_CustomsValue);
		}

		public void TestCifVsCustomsvalue()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			var insuranceCharge = invoiceLine.Charges.AddNew();
			insuranceCharge.J7_ChargeType = OverseasInsuranceCode;
			insuranceCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			insuranceCharge.J7_Amount = 500m;
			Assert("Pre-Req - insurance is dutiable", insuranceCharge.J7_IsDutiable);

			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)dec.IncoTermAndChargeFactory;
			var freightOutsideEuCharge = invoiceLine.Charges.AddNew();
			freightOutsideEuCharge.J7_ChargeType = chargeFactory.FreightToEUBorderCode;
			freightOutsideEuCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			freightOutsideEuCharge.J7_Amount = 100m;
			Assert("Pre-Req - freight before is dutiable", freightOutsideEuCharge.J7_IsDutiable);

			var freightAfterEuCharge = invoiceLine.Charges.AddNew();
			freightAfterEuCharge.J7_ChargeType = chargeFactory.FreightAfterEUBorderCode;
			freightAfterEuCharge.J7_IsDutiable = false;
			freightAfterEuCharge.J7_IsGSTApplicable = true;
			freightAfterEuCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			freightAfterEuCharge.J7_Amount = 50m;
			Assert("Pre-Req - freight after is NOT dutiable", !freightAfterEuCharge.J7_IsDutiable);

			AssertEquals("CIF = 1000+500+100+50 = $1650", 1650m, invoiceLine.JI_Calc_CIF);
			AssertEquals("Customs value = 1000+500+100 = $1600", 1600m, invoiceLine.JI_CustomsValue);
		}

		public void TestStatisticalValueIncludingOverride()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			invoiceLine.ZG_StatisticalValueManualOverride = true;
			invoiceLine.ZG_StatisticalValue = 123m;
			AssertEquals(invoiceLine.ZG_StatisticalValue, 123m);
			AssertEquals(invoiceLine.JI_Calc_StatisticalValue, 1000m);
			invoiceLine.ZG_StatisticalValueManualOverride = false;
			AssertEquals(1000m, invoiceLine.ZG_StatisticalValue);  // Now calculated, but no charges to adjust item price

			var insuranceCharge = invoiceLine.Charges.AddNew();
			insuranceCharge.J7_ChargeType = OverseasInsuranceCode;
			insuranceCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			insuranceCharge.J7_Amount = 100m;
			Assert("Pre-Req - insurance is STAT-able", insuranceCharge.J7_IsStatisticalValueApplicable);

			var nonStatCharge = invoiceLine.Charges.AddNew();
			nonStatCharge.J7_ChargeType = "ABC";
			nonStatCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			nonStatCharge.J7_Amount = 50m;
			nonStatCharge.J7_IsStatisticalValueApplicable = false;

			AssertEquals("Stat value  = 1000+100= 1100", 1100m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals(1100m, invoiceLine.ZG_StatisticalValue);

			invoiceLine.ZG_StatisticalValueManualOverride = true;
			AssertEquals(invoiceLine.ZG_StatisticalValue, 1100m);
			AssertEquals(invoiceLine.JI_Calc_StatisticalValue, 1100m);
			invoiceLine.ZG_StatisticalValue = 456m;
			AssertEquals(invoiceLine.ZG_StatisticalValue, 456m);
			AssertEquals(invoiceLine.JI_Calc_StatisticalValue, 1100m);
		}

		public void TestVatValue()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			var insuranceCharge = invoiceLine.Charges.AddNew();
			insuranceCharge.J7_ChargeType = OverseasInsuranceCode;
			insuranceCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			insuranceCharge.J7_Amount = 100m;
			Assert("Pre-Req - insurance is VAT-able", insuranceCharge.J7_IsGSTApplicable);

			var nonStatCharge = invoiceLine.Charges.AddNew();
			nonStatCharge.J7_ChargeType = "ABC";
			nonStatCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			nonStatCharge.J7_Amount = 50m;
			nonStatCharge.J7_IsGSTApplicable = false;

			AssertEquals("VAT value  = 1000+100= $1100", 1100m, invoiceLine.JI_Calc_ValueForVat);
		}

		public void TestAppliedTaxAndFee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today;
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateTaxOrFee("VZR", 0.14, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, startDate, endDate, "VZR Normal");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertNull(invoiceLine.AppliedTaxAndFee);

			invoiceLine.JI_ZZF_NKTaxType = "VZR";
			AssertNotNull(invoiceLine.AppliedTaxAndFee);
			AssertEquals("VZR", invoiceLine.AppliedTaxAndFee.ZZF_Code);
		}

		public virtual void TestIsSupportEmptyPackTypeAndValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var pack = declaration.Packages.AddNew();

			Assert("Precondition Pack is EU Package class", pack is Package);
			Assert("Precondition InvoiceLine is EU InvoiceLine class", invoiceLine1 is T);
			Assert("Precondition SupportsChcPivot", invoiceLine1.SupportsChcPivotBetweenInvoiceLineAndPacking);

			var npbos = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var npbo = npbos.Cast<BaseCusLinkPackage>().FirstOrDefault();
			AssertEquals("Precondition IsLinked", false, npbo?.IsLinked);

			npbo.IsLinked = true;
			AssertHasMessageErrorContaining(npbo.PackQtyInfo, "have not entered");

			pack.CW_PackType = Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;
			npbo.IsLinked = false;
			npbo.IsLinked = true;
			AssertNoMessageErrorContaining(npbo.PackQtyInfo, "have not entered");
		}

		public void TestWhenAttributesAreSuppliedCorrectPartPivotIsReturned()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "ABC";
			part.OP_Desc = "ABC GOODS";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var classification = Factory.New<CusClassification>();
			classification.FillWithValidTestData();
			classification.CC_TariffNum = "0000.00.00 0";
			classification.CC_LookupCode = "ABC";
			classification.CC_ClassificationType = "IMP";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var partPivot1 = part.PivotsForBinding.AddNew();
			partPivot1.CI_ChildType = invoiceLine.GetClassificationTypeProvider().HTICode;
			partPivot1.CI_OH = importer.PK;
			partPivot1.CI_TariffNum = "1111.11.11 1";
			partPivot1.CI_CC = classification.PK;

			var partPivot1Attr1 = partPivot1.Attributes1.AddNew();
			partPivot1Attr1.BG_AttributeValue1 = "1";
			partPivot1Attr1.BG_CI = partPivot1.PK;
			var partPivot1Attr2 = partPivot1.Attributes2.AddNew();
			partPivot1Attr2.BG_AttributeValue1 = "1";
			partPivot1Attr2.BG_CI = partPivot1.PK;
			var partPivot1Attr3 = partPivot1.Attributes3.AddNew();
			partPivot1Attr3.BG_AttributeValue1 = "1";
			partPivot1Attr3.BG_CI = partPivot1.PK;

			var partPivot2 = part.PivotsForBinding.AddNew();
			partPivot2.CI_ChildType = invoiceLine.GetClassificationTypeProvider().HTICode;
			partPivot2.CI_OH = importer.PK;
			partPivot2.CI_TariffNum = "2222.22.22 2";
			partPivot2.CI_CC = classification.PK;

			var partPivot2Attr1 = partPivot2.Attributes1.AddNew();
			partPivot2Attr1.BG_AttributeValue1 = "2";
			partPivot2Attr1.BG_CI = partPivot2.PK;
			var partPivot2Attr2 = partPivot2.Attributes2.AddNew();
			partPivot2Attr2.BG_AttributeValue1 = "2";
			partPivot2Attr2.BG_CI = partPivot2.PK;
			var partPivot2Attr3 = partPivot2.Attributes3.AddNew();
			partPivot2Attr3.BG_AttributeValue1 = "2";
			partPivot2Attr3.BG_CI = partPivot2.PK;

			AssertNullOrEmpty(invoiceLine.JI_PartAttrib1);
			AssertNullOrEmpty(invoiceLine.JI_PartAttrib2);
			AssertNullOrEmpty(invoiceLine.JI_PartAttrib3);
			AssertNullOrEmpty(invoiceLine.JI_PartNo);
			AssertNullOrEmpty(invoiceLine.JI_Description);

			invoiceLine.JI_PartAttrib1 = "1";
			invoiceLine.JI_PartAttrib2 = "1";
			invoiceLine.JI_PartAttrib3 = "1";
			invoiceLine.JI_PartNo = "ABC";

			var tariffFormatter = Business.TariffFormatter.New(dec.CountryCode);

			AssertEquals("Invoice line JI_Tariff should have changed to part classification pivot value", "111111111", invoiceLine.JI_Tariff);
			AssertEquals("Invoice line JI_FormattedTariff should have changed to part classification pivot value", tariffFormatter.DisplayFormat("1111.11.11 1"), invoiceLine.JI_FormattedTariff);
			AssertEquals("Invoice line JI_Description should have changed to part description value", "ABC GOODS", invoiceLine.JI_Description);

			invoiceLine.JI_PartAttrib1 = "2";
			invoiceLine.JI_PartAttrib2 = "2";
			invoiceLine.JI_PartAttrib3 = "2";
			invoiceLine.JI_PartNo = "ABC";

			AssertEquals("Invoice line JI_Tariff should have changed to part classification pivot value", "222222222", invoiceLine.JI_Tariff);
			AssertEquals("Invoice line JI_FormattedTariff should have changed to part classification pivot value", tariffFormatter.DisplayFormat("2222.22.22 2"), invoiceLine.JI_FormattedTariff);
			AssertEquals("Invoice line JI_Description should have changed to part description value", "ABC GOODS", invoiceLine.JI_Description);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));

			var customsChargeTypeList = GetExpectedCustomsChargeTypeList();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		protected virtual CodeDescriptionPairList GetExpectedCustomsChargeTypeList()
		{
			var customsChargeTypeList = new Customs.Business.CustomsChargeTypeList();
			customsChargeTypeList.RemoveCode(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			customsChargeTypeList.AddPair(ChargeTypeList.Codes.InternationalFreight, ChargeTypeList.Descriptions.InternationalFreight);
			customsChargeTypeList.AddPair(ChargeTypeList.Codes.StatisticalValue, ChargeTypeList.Descriptions.StatisticalValue);
			customsChargeTypeList.Sort();
			return customsChargeTypeList;
		}

		public void TestAutoAllocatePackageToInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "M";
			declaration.JE_TotalNoOfPieces = 1;
			var package = declaration.Bills[0].PackingGroups[0].Packages[0];
			package.CW_PackQty = 1;
			var invoice = declaration.Invoices.AddNew();

			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("PackagesForInvoiceLinesForBindingOnly[0].IsLinked", false, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);

			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.InvoiceLines.Add(invoiceLine);
			AssertEquals("PackagesForInvoiceLinesForBindingOnly[0].IsLinked", true, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);

			var package2 = declaration.Bills[0].PackingGroups[0].Packages.AddNew();
			package2.CW_PackQty = 1;
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invoiceLine);
			AssertEquals("PackagesForInvoiceLinesForBindingOnly[0].IsLinked - false - multiple packages", false, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
		}

		public void TestSupportsChcPivotBetweenInvoiceLineAndPackingCore_AndCanDeleteChcEvenWhenNoDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.Bills.AddNew().CU_BillNum = "123";
			var package = dec.Packages.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = Factory.New<T>();
			invLine.JI_JZ = inv.PK;
			AssertEquals(true, invLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
			invLine.WipeJzForTesting();
			AssertEquals("Pre req", null, invLine.Declaration);
			AssertEquals("Still supports even when dec is null", true, invLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
			var chc = Factory.New<InvoiceLinePackagePivot>();
			chc.CHC_JI = invLine.PK;
			chc.CHC_CW = package.PK;
			chc.CHC_NumberOfPacks = 1;
			invLine.Delete();
			AssertEquals(true, chc.IsDeleted);
			Factory.Save();
			Assert("Saved OK", true);
		}

		public virtual void TestGetNewPackagesPivotCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var pack = declaration.Packages.AddNew();

			Assert("Precondition Pack is EU Package class", pack is Package);
			Assert("Precondition InvoiceLine is EU InvoiceLine class", invoiceLine1 is T);
			Assert("Precondition SupportsChcPivot", invoiceLine1.SupportsChcPivotBetweenInvoiceLineAndPacking);

			var npbos = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var npbo = npbos.Cast<BaseCusLinkPackage>().FirstOrDefault();

			AssertEquals("Precondition IsLinked", false, npbo?.IsLinked);
			npbo.IsLinked = true;

			invoiceLine1.PackagesPivot.RunPreSaveValidation();
			var pivot = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
			AssertHasMessageErrorContaining(pivot.CHC_NumberOfPacksInfo, "have not entered");

			pack.CW_PackType = Business.UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;

			invoiceLine1.PackagesPivot.RunPreSaveValidation();
			pivot = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
			AssertNoMessageErrorContaining(pivot.CHC_NumberOfPacksInfo, "have not entered");
		}

		public virtual void TestProcedureLookupsAndValidation()
		{
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusProcedure"); // I would gladly use the factory if you gave us a way to delete without loading. Until then, shut up.

			var currentCountry = GlbCompany.CurrentCompany.Country.Code;  // Latvia for base EU tests.
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");

			// Note to anyone who copies and pastes this unit test. Current country is Latvia, but you can't save the ZZ6 record to the databse unless you also insert Latvia into table ZZZ or it will violate the FK.

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";

			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			invLine.JI_RN_NKCountryOfExport = GlbCompany.CurrentCompany.Country.Code;
			AssertEquals("JI_CEI automatically set when assigning JE_DeclarationType", cei.PK, invLine.JI_CEI);
			AssertEquals(null, invLine.CusProcedure);
			var cpcs = invLine.Lookups.CPCList;
			AssertEquals("Base EU lookups has all CPCs filters by shipmentType=IMP & declarationType=IFD", 2, cpcs.Count);
			Assert("Base EU lookups has all CPCs filters by shipmentType=IMP & declarationType=IFD", cpcs.Contains(procedure1));
			Assert("Base EU lookups has all CPCs filters by shipmentType=IMP & declarationType=IFD", cpcs.Contains(procedure2));
			invLine.JI_Procedure = "1111111";
			AssertNoMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(procedure1, invLine.CusProcedure);
			invLine.JI_Procedure = "2222222";
			AssertNoMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(procedure2, invLine.CusProcedure);
			invLine.JI_Procedure = "3333333";
			AssertHasMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(procedure3, invLine.CusProcedure);
			invLine.JI_Procedure = "xxxxxxx";
			AssertHasMessageErrorContaining(invLine.JI_ProcedureInfo, "list");
			AssertEquals(null, invLine.CusProcedure);

			var invLineTwo = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertNoMessageErrorContaining(invLine.JI_ProcedureInfo, "series");
			invLine.JI_Procedure = "1111111";
			invLineTwo.JI_Procedure = "3333333";
			AssertHasMessageErrorContaining(invLineTwo.JI_ProcedureInfo, "series");

			dec.JE_MessageType = "EXP";
			cei.CEI_Style = "EFD";
			invLineTwo.JI_CEI = cei.PK;
			cpcs = invLine.Lookups.CPCList;
			AssertEquals("Base EU lookups has all CPCs filters by shipmentType=EXP & declarationType=EFD", 1, cpcs.Count);
			Assert("Base EU lookups has all CPCs filters by shipmentType=EXP & declarationType=EFD", cpcs.Contains(procedure4));
			invLine.JI_Procedure = "4444444";
			AssertEquals(procedure4, invLine.CusProcedure);
		}

		public void TestNumberOfSupplementaryCodesAllowed()
		{
			var supplementaryCodeProvider = SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(InvoiceLine);
			for (int i = 0; i < supplementaryCodeProvider.NumberOfCodes; i++)
			{
				InvoiceLine.AdditionalSupplementaryCodes.AddNew();
			}
			Assert("Should not be able to add more supplementary codes", !InvoiceLine.AdditionalSupplementaryCodes.AllowNew);
		}

		public void TestAdditionalSupplementaryCodesCalculatedField()
		{
			var collection = InvoiceLine.AdditionalSupplementaryCodes;
			collection.AddNew("ABCD");
			collection.AddNew("EFGH");
			AssertEquals("ABCD,EFGH", InvoiceLine.JI_AdditionalSupplements);
		}

		public void TestDefaultValuesOnCreation()
		{
			var invoiceline = Factory.New<T>();

			CombineAssertions(() =>
			{
				AssertEquals("JI_CustomsThirdQuantity", ZDecimal.Zero, invoiceline.JI_CustomsThirdQuantity);
				AssertEquals("JI_PrimaryPreference", ZString.Empty, invoiceline.JI_PrimaryPreference);
				AssertEquals("ZG_StatisticalValue", ZDecimal.Zero, invoiceline.ZG_StatisticalValue);
				AssertEquals("JI_ValuationCode", ZString.Empty, invoiceline.JI_ValuationCode);
				AssertEquals("JI_ConcessionOrder", ZString.Empty, invoiceline.JI_ConcessionOrder);
				AssertEquals("JI_ValuationMarkup", ZDecimal.Zero, invoiceline.JI_ValuationMarkup);
				AssertEquals("JI_OA_SupervisingOffice", ZGuid.Empty, invoiceline.JI_OA_SupervisingOffice);
			});
		}

		public void TestSetProductBringsInDocsAndTaxes()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "TestTEST";
			var orgRelation = product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = GetJobMessageTypeForTestingSetProductBringsInDocsAndTaxes == Common.Shared.SharedJobMessageTypeList.Codes.Import ? ClassificationType.IMP : ClassificationType.EXP;
			pivot.CI_OH = orgRelation.OU_OH;
			pivot.CI_TariffNum = "10101010";
			var sdPart1 = pivot.SupportingDocuments.AddNew();
			var sdPart2 = pivot.SupportingDocuments.AddNew();
			var pdPart1 = pivot.PreviousDocuments.AddNew();
			var pdPart2 = pivot.PreviousDocuments.AddNew();
			var aiPart1 = pivot.AdditionalInfos.AddNew();
			var aiPart2 = pivot.AdditionalInfos.AddNew();
			var taxPartA00 = pivot.Taxes.AddNew();
			var taxPartB00 = pivot.Taxes.AddNew();
			sdPart1.CSI_Code = "SD01";
			sdPart1.CSI_ReferenceNumber = "FromPart";
			sdPart2.CSI_Code = "SD02";
			pdPart1.CSI_Code = "PD1";
			pdPart2.CSI_Code = "PD2";
			aiPart1.CSI_Code = "AI01";
			aiPart2.CSI_Code = "AI02";
			taxPartA00.Data.G4_Type = "A00";
			taxPartA00.Data.G4_RateDuty = "NEW";
			taxPartB00.Data.G4_Type = "B00";
			pivot.CI_TariffNum = "1234";
			pivot.CI_CPC = "4000001";
			pivot.PreferenceCode = "400";
			pivot.CI_RN_NKCountryOfOrigin = "US";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = GetJobMessageTypeForTestingSetProductBringsInDocsAndTaxes;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var sdExisting1 = invoiceLine.SupportingDocuments.AddNew();
			var pdExisting1 = invoiceLine.PreviousDocuments.AddNew();
			var aiExisting1 = invoiceLine.AdditionalInfos.AddNew();
			var taxExistingA00 = invoiceLine.Taxes.AddNew();
			var taxExistingD10 = invoiceLine.Taxes.AddNew();
			sdExisting1.CSI_Code = "SD01";
			pdExisting1.CSI_Code = "PD1";
			aiExisting1.CSI_Code = "AI01";
			taxExistingA00.Data.G4_Type = "A00";
			taxExistingA00.Data.G4_RateDuty = "OLD";
			taxExistingD10.Data.G4_Type = "D10";
			taxExistingD10.Data.G4_RateDuty = "OLD";

			invoiceLine.JI_PartNo = "TestTEST";
			AssertEquals("CPC set from product's pivot", "4000001", invoiceLine.JI_Procedure);
			AssertEquals("Pref set from product's pivot", "400", invoiceLine.JI_PrimaryPreference);
			AssertEquals("Country set from product's pivot", "US", invoiceLine.JI_CountryOfOrigin);
			AssertEquals("Two new, plus one existing SD", 3, invoiceLine.SupportingDocuments.Count);
			AssertEquals("SD01", invoiceLine.SupportingDocuments[0].KeyToDeterimeUniqueness);
			AssertEquals("SD01FromPart", invoiceLine.SupportingDocuments[1].KeyToDeterimeUniqueness);
			AssertEquals("SD02", invoiceLine.SupportingDocuments[2].KeyToDeterimeUniqueness);

			AssertEquals("Existing", 2, invoiceLine.PreviousDocuments.Count);
			AssertEquals("PD1", invoiceLine.PreviousDocuments[0].KeyToDeterimeUniqueness);
			AssertEquals("PD2", invoiceLine.PreviousDocuments[1].KeyToDeterimeUniqueness);

			AssertEquals(2, invoiceLine.AdditionalInfos.Count);
			AssertEquals("AI01", invoiceLine.AdditionalInfos[0].KeyToDeterimeUniqueness);
			AssertEquals("AI02", invoiceLine.AdditionalInfos[1].KeyToDeterimeUniqueness);

			AssertEquals("Two existing, two new on pivot, with overlap of one, result is 3", 3, invoiceLine.Taxes.Count);
			AssertEquals("Existed already, not removed", "A00", invoiceLine.Taxes[0].Data.G4_Type);
			AssertEquals("Existed already, updated", "NEW", invoiceLine.Taxes[0].Data.G4_RateDuty);
			AssertEquals("Existed already, not removed, not on product", "D10", invoiceLine.Taxes[1].Data.G4_Type);
			AssertEquals("Existed already, not removed, not on product", "OLD", invoiceLine.Taxes[1].Data.G4_RateDuty);
			AssertEquals("Added via product's pivot", "B00", invoiceLine.Taxes[2].Data.G4_Type);
		}

		protected virtual ZString GetJobMessageTypeForTestingSetProductBringsInDocsAndTaxes => Common.Shared.SharedJobMessageTypeList.Codes.Import;

		public void TestSetProductBringsInAdditionalDocuments()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "TestTEST";
			var orgRelation = product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.EXP;
			pivot.CI_OH = orgRelation.OU_OH;
			var adPart1 = pivot.AdditionalInfos.AddNew();
			var adPart2 = pivot.AdditionalInfos.AddNew();
			adPart1.CSI_Code = "AI01";
			adPart1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			adPart1.CSI_ReferenceNumber = "REF001";
			adPart1.CSI_Description = "DESC001";
			adPart2.CSI_Code = "AI02";
			adPart2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			adPart2.CSI_ReferenceNumber = "REF002";
			adPart2.CSI_Description = "DESC002";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var invoiceLine1 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var adExisting1 = invoiceLine1.AdditionalInfos.AddNew();
			adExisting1.CSI_Code = "AI01";
			adExisting1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			adExisting1.CSI_ReferenceNumber = "REF001";
			adExisting1.CSI_Description = "DESC001";

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "UCCAdditionalInfosSupportCore", true))
			using (ConfigurationTestHelper.TemporarilyClearPartPivotConfigurationAndThenSetPartPivotConfiguration(pivot, "UCCAdditionalInfosSupportCore", true))
			{
				invoiceLine1.JI_PartNo = "TestTEST";
				AssertEquals(2, invoiceLine1.AdditionalInfos.Count);
				AssertEquals("AD 1 Unique", "AI01", invoiceLine1.AdditionalInfos[0].CSI_Code);
				AssertEquals("AD 2 Unique", "AI02", invoiceLine1.AdditionalInfos[1].CSI_Code);
				AssertEquals("AD 2 Sub-type", AdditionalInfoSubTypeList.Codes.AdditionalReference, invoiceLine1.AdditionalInfos[1].CSI_SubType);
				AssertEquals("AD 2 Reference", "REF002", invoiceLine1.AdditionalInfos[1].CSI_ReferenceNumber);
				AssertEquals("AD 2 Description", "DESC002", invoiceLine1.AdditionalInfos[1].CSI_Description);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "UCCAdditionalInfosSupportCore", false))
			using (ConfigurationTestHelper.TemporarilyClearPartPivotConfigurationAndThenSetPartPivotConfiguration(pivot, "UCCAdditionalInfosSupportCore", false))
			{
				var invoiceLine2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine2.JI_PartNo = "TestTEST";
				AssertEquals(1, invoiceLine2.AdditionalInfos.Count);
				AssertEquals("AD 1 Unique", "AI01", invoiceLine2.AdditionalInfos[0].CSI_Code);
			}
		}

		public void TestSetProductBringsInAdditionalDocuments_UCC6Filter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "TestTEST";
			var orgRelation = product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.EXP;
			pivot.CI_OH = orgRelation.OU_OH;
			var adPart1 = pivot.AdditionalInfos.AddNew();
			var adPart2 = pivot.AdditionalInfos.AddNew();
			adPart1.CSI_Code = "AI01";
			adPart1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			adPart1.CSI_ReferenceNumber = "REF001";
			adPart1.CSI_Description = "DESC001";
			adPart2.CSI_Code = "AI02";
			adPart2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			adPart2.CSI_ReferenceNumber = "REF002";
			adPart2.CSI_Description = "DESC002";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Factory.Save();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "UCCAdditionalInfosSupportCore", true))
			{
				var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "TestTEST";

				AssertEquals(2, invoiceLine.AdditionalInfos.Count);
				AssertEquals("AD 1 Unique", "AI01", invoiceLine.AdditionalInfos[0].CSI_Code);
				AssertEquals("AD 2 Unique", "AI02", invoiceLine.AdditionalInfos[1].CSI_Code);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "UCCAdditionalInfosSupportCore", false))
			{
				var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "TestTEST";

				AssertEquals(1, invoiceLine.AdditionalInfos.Count);
				AssertEquals("AD 1 Unique", "AI01", invoiceLine.AdditionalInfos[0].CSI_Code);
			}
		}

		public void TestCusEntryLine()
		{
			var ji = Factory.New<T>();
			var cl = Factory.New<CusEntryLine>();
			ji.JI_CL = cl.PK;
			AssertType(GetExpectedCusEntryLineType(), ji.CusEntryLine);
		}

		protected virtual Type GetExpectedCusEntryLineType()
		{
			return typeof(CusEntryLine);
		}

		public void TestEntryInstruction()
		{
			var invoiceLine = Factory.New<T>();
			var instruction = Factory.New<CusEntryInstruction>();
			invoiceLine.JI_CEI = instruction.PK;

			AssertType(GetExpectedEntryInstructionType(), invoiceLine.EntryInstruction);
		}

		protected virtual Type GetExpectedEntryInstructionType()
		{
			return typeof(CusEntryInstruction);
		}

		public void TestCloneInvoiceLineDoesNotCopyOfChildObjects()
		{
			var sourceLine = Factory.New<T>();
			sourceLine.PreviousDocuments.AddNew().CSI_ReferenceNumber = "DAN1";
			sourceLine.AdditionalInfos.AddNew().CSI_Description = "DAN2";
			sourceLine.SupportingDocuments.AddNew().CSI_ReferenceNumber = "DAN3";
			sourceLine.Taxes.AddNew().Data.G4_RateDuty = "DC4";

			var alternativeFactoryToInstantiateCloneIn = new BusinessObjectFactory();
			var args = Enterprise.Customs.Business.CustomsBusinessObjectCloneArgs.GetCloneArgs(Enterprise.Customs.Business.CloneType.DeepTemplateCopy, sourceLine.GetType(), alternativeFactoryToInstantiateCloneIn);
			var result = (T)sourceLine.Clone(args);

			AssertEquals("Plain vanilla clone on line should not clone its children too. Let JobDeclarationDeepCloneStrategy orchestrate that.", 0, result.PreviousDocuments.Count);
			AssertEquals("Plain vanilla clone on line should not clone its children too. Let JobDeclarationDeepCloneStrategy orchestrate that.", 0, result.AdditionalInfos.Count);
			AssertEquals("Plain vanilla clone on line should not clone its children too. Let JobDeclarationDeepCloneStrategy orchestrate that.", 0, result.SupportingDocuments.Count);
			AssertEquals("Plain vanilla clone on line should not clone its children too. Let JobDeclarationDeepCloneStrategy orchestrate that.", 0, result.Taxes.Count);
		}

		public void TestIUltimateDistributeeCalculateOwnGstVatRate()
		{
			var invLine = Factory.New<T>();
			IUltimateDistributee iud = invLine;
			AssertNotNull("JobComInvoiceLine should implement IUltimateDistributee", iud);
			AssertEquals(true, iud.IsCapableOfCalculatingOwnGstVatRate);
			AssertEquals(0m, iud.CalculateOwnGstVatRate());
			var b00Tax = invLine.Taxes.AddNew();
			b00Tax.Data.G4_Type = "B00";
			b00Tax.Data.G4_Amount = "69";
			b00Tax.Data.G4_BaseAmount = 100m;
			AssertEquals(0.69m, iud.CalculateOwnGstVatRate());
		}

		public void TestSetFecChallengeFlagToTrue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.ZG_FecChallengeDST);
			invoiceLine.SetFecChallengeFlag("ZG_FecChallengeDST", true);
			AssertEquals(true, invoiceLine.ZG_FecChallengeDST);
			invoiceLine.SetFecChallengeFlag("ZG_FecChallengeDST", false);
			AssertEquals(false, invoiceLine.ZG_FecChallengeDST);
		}

		public void TestReadFecDST()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.ZG_FecDST);
		}

		public void TestJI_CustomsValueForImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_InvoiceAmount = 1200.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.Charges.AddNew("OFT", 160.00m, "GBP").J7_IsIncludedInITOT = true;
			invoiceHeader.Charges.AddNew("ONS", 40.00m, "GBP").J7_IsIncludedInITOT = true;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1200m;
			AssertEquals(1200m, invoiceLine.JI_CustomsValue);
		}

		public void TestPullingDataFromClassification()
		{
			Declaration.JE_MessageType = "IMP";
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "DUMMYTEST";
			classification.CC_TariffNum = "3100000023";
			classification.CC_ProcedureCode = "PDS";

			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.JI_Procedure = ZString.Empty;
			InvoiceLine.JI_CC = classification.PK;
			AssertEquals("JI_Tariff", "3100000023", InvoiceLine.JI_Tariff);
			AssertEquals("JI_Procedure", "PDS", InvoiceLine.JI_Procedure);

			InvoiceLine.JI_CC = ZGuid.Invalid;
			AssertEquals("JI_Tariff", "3100000023", InvoiceLine.JI_Tariff);
			AssertEquals("JI_Procedure", "PDS", InvoiceLine.JI_Procedure);
		}

		public void TestSupervisingOffice()
		{
			InvoiceLine.JI_OA_SupervisingOffice = ZGuid.Empty;
			AssertEquals("InvoiceLine.SupervisingOfficeOrgHeader", null, InvoiceLine.SupervisingOffice);

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgAddress address = organisation.MainAddress;
			address.OA_City = "SYD";

			InvoiceLine.JI_OA_SupervisingOffice = address.PK;
			AssertEquals("InvoiceLine.SupervisingOfficeOrgHeader.MainAddress.OA_City", address, InvoiceLine.SupervisingOffice);
		}

		public void TestICanBeImportOrExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (T)invoiceHeader.JobComInvoiceLines.AddNew();

			var iore = invoiceLine as ICanBeImportOrExport;
			AssertNotNull("invoiceLine as ICanBeImportOrExport", iore);
			AssertEquals("Item", iore.Level);
			AssertEquals("Country Code", declaration.CountryCode, iore.TrueCountryCode);
			AssertEquals("Data Grouping", declaration.GetDefaultDataGroupingCode(), iore.DataGroupingCode);

			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", true, iore.IsExport); //when message type is empty, isExport returns true on the Declaration

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("iore.IsImport", true, iore.IsImport);
			AssertEquals("iore.IsExport", false, iore.IsExport);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", true, iore.IsExport);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", false, iore.IsExport);

			invoiceLine = Factory.New<T>();
			iore = invoiceLine;
			AssertEquals("iore.IsImport", false, iore.IsImport);
			AssertEquals("iore.IsExport", false, iore.IsExport);
		}

		public void TestSupportingDocuments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (T)invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			SupportingDocument supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "1";
			supportingDocument.CSI_ReferenceNumber = "3";
			supportingDocument.CSI_SubType = "4";
			supportingDocument.CSI_Quantity = 5;
			supportingDocument.CSI_Description = "6";
			Factory.Save();
			invoiceLine.SupportingDocuments.RemoveAndDeleteAll();

			var secondFactory = new BusinessObjectFactory();
			var loadedSupportingDocument = secondFactory.LoadTop1<SupportingDocument>(new ZQuery(CusSupportingInfoSchema.CSI_ParentID, invoiceLine.PK));
			AssertNotNull("Re-Loaded SupportingDocument", loadedSupportingDocument);
			invoiceLine.SupportingDocuments.Add(loadedSupportingDocument);
			AssertEquals(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, loadedSupportingDocument.CSI_Type);
			AssertEquals("loadedSupportingDocument.CSI_Code", "1", loadedSupportingDocument.CSI_Code);
			AssertEquals("loadedSupportingDocument.CSI_ReferenceNumber", "3", loadedSupportingDocument.CSI_ReferenceNumber);
			AssertEquals("loadedSupportingDocument.CSI_SubType", "4", loadedSupportingDocument.CSI_SubType);
			AssertEquals("loadedSupportingDocument.CSI_Quantity", 5m, loadedSupportingDocument.CSI_Quantity);
			AssertEquals("loadedSupportingDocument.CSI_Description", "6", loadedSupportingDocument.CSI_Description);
		}

		public void TestPartsAndPivotsLoadedForCorrectCountry_Issue00834259_ThankYouAnton()
		{
			var countryCode = GetLocalPortCode().Substring(0, 2);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();

				var product = (Customs.Business.OrgSupplierPart)Factory.New<AU.IOrgSupplierPart>();
				product.OP_PartNum = "TestTEST";
				product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
				product.PivotsForBinding.AddNew();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
				{
					var gbDeclaration = (JobDeclaration)Factory.New<Integration.Customs.GB.IJobDeclaration>();
					gbDeclaration.JE_OH_Importer = importer.PK;
					var gbLine = gbDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
					gbLine.JI_PartNo = "TestTEST";

					AssertNoExceptionThrown(() => Factory.Save());
				}
			}
		}

		protected virtual string GetLocalPortCode()
		{
			return "LVLPX";
		}

		public void TestPartType()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader importer = factory2.New<OrgHeader>();
			importer.FillWithValidTestData();
			BaseOrgSupplierPart product = (BaseOrgSupplierPart)factory2.New<AU.IOrgSupplierPart>();
			product.OP_PartNum = "TestTEST";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			factory2.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var invoiceLine = (T)declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TestTEST";
			AssertType("Product type gets changed depending on who is requesting", GetExpectedPartType(), invoiceLine.Part);

			Factory.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var declarationLoadedAsBase = factory3.Load<BaseJobDeclaration>(declaration.PK);
			AssertType("product type still the right type even when loaded from base declaration", GetExpectedPartType(), declarationLoadedAsBase.InvoiceLines[0].Part);
		}

		protected virtual Type GetExpectedPartType()
		{
			return typeof(OrgSupplierPart);
		}

		public void TestTypeDeciderEuAndGb()
		{
			Assert("Type decider for LV", Factory.New(typeof(BaseJobComInvoiceLine)).GetType() == GetExpectedBusinessObjectType());
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				Assert("Type decider for GB", Factory.New(typeof(BaseJobComInvoiceLine)).GetType().FullName.Contains(".GB."));
			}
		}

		public void TestSetSupervisingOfficeFromDeclarant()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			JobComInvoiceHeader inv = dec.Invoices.AddNew();
			var line = (T)inv.InvoiceLines.AddNew();
			Factory.Save();

			AssertEquals("Line should have no SPOFF out of the box", ZGuid.Empty, line.JI_OA_SupervisingOffice);
			line.SetSupervisingOfficeFromDeclarant();
			AssertEquals("Line should have no SPOFF even after pressing the button, as declarant has none", ZGuid.Empty, line.JI_OA_SupervisingOffice);

			OrgHeader declarant = Factory.New<OrgHeader>();
			OrgHeader hmrc = Factory.New<OrgHeader>();
			hmrc.Addresses.AddNewMainAddress();
			hmrc.OH_Code = "HMRC";
			declarant.OH_Code = "DCDEC";
			dec.JE_OA_DeclarantAddress = declarant.Addresses.AddNewMainAddress().PK;
			Factory.Save();
			line.SetSupervisingOfficeFromDeclarant();
			AssertEquals("Line should have no SPOFF, declarant is defined but has no related party", ZGuid.Empty, line.JI_OA_SupervisingOffice);

			OrgRelatedParty relation = Factory.New<OrgRelatedParty>();
			relation.PR_OH_RelatedParty = hmrc.PK;
			relation.PR_OH_Parent = declarant.PK;
			relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relation.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			Factory.Save();

			AssertEquals("Line should have no SPOFF, declarant is defined and has a related party, but we ain't pressed the button yet", ZGuid.Empty, line.JI_OA_SupervisingOffice);
			line.SetSupervisingOfficeFromDeclarant();
			AssertEquals("We have a declarant with a releated party, so pressing the button should set the line's spoff", hmrc.MainAddress.PK, line.JI_OA_SupervisingOffice);
		}

		public void TestSetProductAndClassification()
		{
			var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "~~~";
			product.OP_PartNum = "APPLE";
			product.RelatedOrganisations.RemoveAndDeleteAll();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "11111111";
			pivot.CI_ChildType = ClassificationType.IMP;
			pivot.CI_CPC = "P444444";
			pivot.CI_Supplement1 = "P111";
			pivot.CI_Supplement2 = "P222";
			pivot.CI_ConcessionOrder = "111";

			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "22222222";
			classification.CC_ProcedureCode = "C444444";
			classification.CC_EcSupplement1 = "C111";
			classification.CC_EcSupplement2 = "C222";
			pivot.CI_CC = classification.PK;

			var declaration = GetJobDeclarationForTesting();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var supplementaryCodeProvider = SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(InvoiceLine);
			var numberOfAdditionalSupplementaryCodes = supplementaryCodeProvider.NumberOfCodes;
			for (int i = 3; i <= numberOfAdditionalSupplementaryCodes + 2; i++)
			{
				pivot.AdditionalSupplementaryCodes.AddNew(ZString.Format("P{0}{0}{0}", i).Substring(0, 4));
			}
			for (int i = 3; i <= numberOfAdditionalSupplementaryCodes + 2; i++)
			{
				classification.AdditionalSupplementaryCodes.AddNew(ZString.Format("C{0}{0}{0}", i).Substring(0, 4));
			}

			invoiceLine.JI_CC = ZGuid.Invalid;
			invoiceLine.JI_PartNo = "APPLE";
			AssertEquals("Invoice lines gets tariff from dbo.CusClassPartPivot preferentially", "11111111", invoiceLine.JI_Tariff);
			AssertEquals("Invoice lines gets CPC from dbo.CusClassPartPivot preferentially", "P444444", invoiceLine.JI_Procedure);
			AssertEquals("Invoice lines gets EC tariff from dbo.CusClassPartPivot preferentially", "P111", invoiceLine.JI_SupplementaryCode1);
			AssertEquals("Invoice lines gets EC tariff from dbo.CusClassPartPivot preferentially", "P222", invoiceLine.JI_SupplementaryCode2);
			for (int i = 3; i <= numberOfAdditionalSupplementaryCodes + 2; i++)
			{
				AssertEquals("Invoice lines gets EC tariff from dbo.CusClassPartPivot preferentially", ZString.Format("P{0}{0}{0}", i).Substring(0, 4), invoiceLine.AdditionalSupplementaryCodes[i - 3].CY_Code);
			}
			AssertEquals("Invoice lines gets Quota from dbo.CusClassPartPivot preferentially", "111", invoiceLine.JI_ConcessionOrder);
			AssertEquals(classification.PK, invoiceLine.JI_CC);

			pivot.CI_TariffNum = "";
			pivot.CI_CPC = "";
			pivot.CI_Supplement1 = "";
			pivot.CI_Supplement2 = "";
			pivot.CI_ConcessionOrder = "";
			pivot.AdditionalSupplementaryCodes.RemoveAndDeleteAll();

			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CC = ZGuid.Invalid;
			invoiceLine2.JI_PartNo = "APPLE";
			AssertEquals("Invoice lines gets tariff from classification because pivot's is empty", "22222222", invoiceLine2.JI_Tariff);
			AssertEquals("Invoice lines gets CPC from classification because pivot's is empty", "C444444", invoiceLine2.JI_Procedure);
			AssertEquals("Invoice lines gets EC tariff from classification because pivot's is empty", "C111", invoiceLine2.JI_SupplementaryCode1);
			AssertEquals("Invoice lines gets EC tariff from classification because pivot's is empty", "C222", invoiceLine2.JI_SupplementaryCode2);
			for (int i = 3; i <= numberOfAdditionalSupplementaryCodes + 2; i++)
			{
				AssertEquals("Invoice lines gets EC tariff from dbo.CusClassPartPivot preferentially", ZString.Format("C{0}{0}{0}", i).Substring(0, 4), invoiceLine2.AdditionalSupplementaryCodes[i - 3].CY_Code);
			}
			AssertEquals("Quota is empty because pivot's is empty", ZString.Empty, invoiceLine2.JI_ConcessionOrder);
			AssertEquals(classification.PK, invoiceLine2.JI_CC);
		}

		public void TestSetTariffEtcDataFromProductsPivotCore()
		{
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Import, ClassificationType.IMP, ("1234567890", "DE", "CPC", "Supp1", "Supp2", "AddSupp1,AddSupp2", 3.33m, "PRC"));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Import, ClassificationType.EXP, ("", "", "", "", "", "", 0m, ""));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Import, ClassificationType.Both, ("1234567890", "DE", "", "", "", "", 0m, ""));

			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Export, ClassificationType.IMP, ("", "", "", "", "", "", 0m, ""));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Export, ClassificationType.EXP, ("12345678", "DE", "CPC", "Supp1", "Supp2", "AddSupp1,AddSupp2", 3.33m, "PRC"));
			SetTariffEtcDataFromProductsPivotCore(JobMessageTypeList.Codes.Export, ClassificationType.Both, ("12345678", "DE", "", "", "", "", 0m, ""));

			void SetTariffEtcDataFromProductsPivotCore(ZString messageType, ZString childType,
				(string expectedJI_Tariff, string expectedJI_CountryOfOrigin, string expectedJI_Procedure, string expectedJI_SupplementaryCode1, string expectedSupplementaryCode2, string expectedAdditionalSupplementaryCodes, decimal expectedJI_CustomsThirdQuantity, string expectJI_PrimaryPreference) tuple)
			{
				var product = (OrgSupplierPart)OrgSupplierPart.New(Factory);
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_Code = "~~~";
				product.OP_PartNum = "ABC";
				product.RelatedOrganisations.RemoveAndDeleteAll();
				product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_OP = product.PK;
				pivot.CI_ChildType = childType;
				pivot.CI_TariffNum = "1234567890";

				pivot.CI_Supplement1 = "Supp1";
				pivot.CI_Supplement2 = "Supp2";
				var addSupp1 = pivot.AdditionalSupplementaryCodes.AddNew();
				var addSupp2 = pivot.AdditionalSupplementaryCodes.AddNew();
				addSupp1.CY_Code = "AddSupp1";
				addSupp2.CY_Code = "AddSupp2";

				pivot.CI_CPC = "CPC";

				pivot.CI_ConcessionOrder = "ConOrder";
				pivot.CI_ThirdQty = 3.33m;
				pivot.CI_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Germany;
				pivot.PreferenceCode = "PRC";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = messageType;
				var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

				invLine.JI_CC = ZGuid.Invalid;
				invLine.JI_PartNo = product.OP_PartNum;

				CombineAssertions(string.Join(" ", "Message Type:", messageType, ", ChildType:", childType), () =>
				{
					AssertEquals("JI_Tariff", tuple.expectedJI_Tariff, invLine.JI_Tariff);
					AssertEquals("JI_CountryOfOrigin", tuple.expectedJI_CountryOfOrigin, invLine.JI_CountryOfOrigin);
					AssertEquals("JI_Procedure", tuple.expectedJI_Procedure, invLine.JI_Procedure);
					AssertEquals("JI_SupplementaryCode1", tuple.expectedJI_SupplementaryCode1, invLine.JI_SupplementaryCode1);
					AssertEquals("SupplementaryCode2", tuple.expectedSupplementaryCode2, invLine.JI_SupplementaryCode2);
					AssertEquals("AdditionalSupplementaryCodes", tuple.expectedAdditionalSupplementaryCodes, invLine.AdditionalSupplementaryCodes.AsString);
					AssertEquals("JI_CustomsThirdQuantity", tuple.expectedJI_CustomsThirdQuantity, invLine.JI_CustomsThirdQuantity);
					AssertEquals("JI_PrimaryPreference", tuple.expectJI_PrimaryPreference, invLine.JI_PrimaryPreference);
				});
			}
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			var invoiceLine = Factory.New<T>();
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			AssertEquals("Customs unit qty equal to KG and Qty field should be editable", false, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			invoiceLine.JI_CustomsUnitQty = ZString.Empty;
			AssertEquals("Customs unit qty empty Qty field should be readonly", true, invoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ICusCodeDataTypeSupporter supporter = invoiceLine;
			AssertEquals(typeof(SupplementaryCode), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.SupplementaryCode]);
			AssertEquals(ExpectedAdditionalProcedureCode, supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.AdditionalProcedureCode]);
			AssertEquals(typeof(NationalAdditionalCode), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.NationalAdditionalCode]);
		}

		public virtual void TestIAdditionalProcedureParent()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateCusCodeList(
				currentCountry,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdvancedProcedureCode,
				"111", "Advanced Procedure Code 111",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime
			);
			Factory.Save();
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");

			var dec = GetJobDeclarationForTesting();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = currentCountry;
			invLine.JI_CEI = cei.PK;

			IAdditionalProcedureParent cpcParent = invLine;

			AssertEquals(2, cpcParent.AdditionalProcedureCodeList.Count);
			Assert("CPC List", cpcParent.AdditionalProcedureCodeList.ContainsCode("1111111"));
			AssertEquals("Advanced Procedure Code 111", cpcParent.AdditionalProcedureCodeList.GetDescriptionFromCode("1111111"));
			Assert("CPC List", cpcParent.AdditionalProcedureCodeList.ContainsCode("1111222"));

			invLine.JI_Procedure = "1111222";
			AssertEquals(1, cpcParent.AdditionalProcedureCodeList.Count);
			Assert("CPC List", cpcParent.AdditionalProcedureCodeList.ContainsCode("1111111"));

			AssertEquals("1111222", cpcParent.MainProcedure);
			AssertEquals("1111", cpcParent.MainProcedurePrefix);
		}

		public void TestConvertCustomsQtyFromNetWeight()
		{
			var invoiceLine = Factory.New<T>();
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			AssertEquals("Precondition:JI_CustomsUnitQty", 0m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = 10m;
			AssertEquals("JI_CustomsQuantity", 0m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("JI_CustomsQuantity", 10m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeight = 1000m;
			AssertEquals("JI_CustomsQuantity", 1000m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("JI_CustomsQuantity", 1m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_NetWeightUQ = "X";
			AssertEquals("JI_CustomsQuantity", 1m, invoiceLine.JI_CustomsQuantity);
		}

		public override void TestPivot()
		{
			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";

			var supRelation1 = part.RelatedOrganisations.AddNew();
			supRelation1.OU_OH = Factory.New<OrgHeader>().PK;
			supRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var supRelation2 = part.RelatedOrganisations.AddNew();
			supRelation2.OU_OH = Factory.New<OrgHeader>().PK;
			supRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var ownRelation1 = part.RelatedOrganisations.AddNew();
			ownRelation1.OU_OH = Factory.New<OrgHeader>().PK;
			ownRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var ownRelation2 = part.RelatedOrganisations.AddNew();
			ownRelation2.OU_OH = Factory.New<OrgHeader>().PK;
			ownRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var importClassification = Factory.New<BaseCusClassification>();
			importClassification.CC_ClassificationType = ClassificationType.Both;
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_CC = importClassification.PK;
			importPivot.CI_ChildType = ClassificationType.IMP;

			var exportClassification = Factory.New<BaseCusClassification>();
			exportClassification.CC_ClassificationType = ClassificationType.Both;
			var exportPivot = part.PivotsForBinding.AddNew();
			exportPivot.CI_CC = exportClassification.PK;
			exportPivot.CI_ChildType = ClassificationType.EXP;

			Declaration.JE_MessageType = DeclarationExportMessageType;
			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation1.OU_OH;
			InvoiceLine.JI_PartNo = part.OP_PartNum;

			AssertNotNull(InvoiceLine.Part);
			AssertEquals(exportPivot.PK, InvoiceLine.Pivot.PK);

			Declaration.JE_MessageType = DeclarationImportMessageType;
			AssertEquals(importPivot.PK, InvoiceLine.Pivot.PK);

			Declaration.JE_MessageType = DeclarationExportMessageType;
			AssertEquals(exportPivot.PK, InvoiceLine.Pivot.PK);

			part.PivotsForBinding.RemoveFromRelationship(importPivot);
			part.PivotsForBinding.RemoveFromRelationship(exportPivot);

			var pivotIMP1 = part.PivotsForBinding.AddNew();
			pivotIMP1.CI_ChildType = ClassificationType.IMP;
			pivotIMP1.CI_OH = ownRelation1.OU_OH;
			pivotIMP1.CI_TariffNum = "1234";

			var pivotIMP2 = part.PivotsForBinding.AddNew();
			pivotIMP2.CI_ChildType = ClassificationType.IMP;
			pivotIMP2.CI_OH = ownRelation2.OU_OH;
			pivotIMP2.CI_TariffNum = "4321";

			var pivotEXP1 = part.PivotsForBinding.AddNew();
			pivotEXP1.CI_ChildType = ClassificationType.EXP;
			pivotEXP1.CI_OH = supRelation1.OU_OH;
			pivotEXP1.CI_TariffNum = "2345";

			var pivotEXP2 = part.PivotsForBinding.AddNew();
			pivotEXP2.CI_ChildType = ClassificationType.EXP;
			pivotEXP2.CI_OH = supRelation2.OU_OH;
			pivotEXP2.CI_TariffNum = "5432";

			Declaration.JE_OH_Importer = ownRelation1.OU_OH;
			Declaration.JE_OH_Supplier = supRelation1.OU_OH;

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			Factory.ClearCachedValue<CusClassPartPivot>();
			AssertEquals(pivotEXP1.PK, InvoiceLine.Pivot.PK);

			InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation2.OU_OH;
			Factory.ClearCachedValue<CusClassPartPivot>();
			AssertEquals(pivotEXP2.PK, InvoiceLine.Pivot.PK);

			Declaration.JE_MessageType = DeclarationImportMessageType;
			Factory.ClearCachedValue<CusClassPartPivot>();
			AssertEquals(pivotIMP1.PK, InvoiceLine.Pivot.PK);

			InvoiceLine.InvoiceHeader.JZ_OH_Buyer = ownRelation2.OU_OH;
			Factory.ClearCachedValue<CusClassPartPivot>();
			AssertEquals(pivotIMP2.PK, InvoiceLine.Pivot.PK);
		}

		public void TestPivotByTypeSupplierAndImporter()
		{
			Declaration.JE_MessageType = DeclarationExportMessageType;
			Func<OrgSupplierPart, ZString, ZGuid, OrgPartRelation> addANewRelatedOrg = (parentPart, relationshipType, ohpk) =>
			{
				var result = parentPart.RelatedOrganisations.AddNew();
				result.OU_Relationship = relationshipType;
				result.OU_OH = ohpk;
				return result;
			};

			Func<OrgSupplierPart, ZString, ZString, ZGuid?, BaseCusClassPartPivot> addACusClassPartPivot = (parentPart, childType, tariffNum, ohpk) =>
			{
				var result = parentPart.PivotsForBinding.AddNew();
				result.CI_ChildType = childType;
				if (ohpk.HasValue)
				{
					result.CI_OH = ohpk.Value;
				}
				result.CI_TariffNum = tariffNum;
				return result;
			};

			var oneSharedSUP = Factory.New<OrgHeader>();

			#region Preparation Part 1

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			part.OP_Desc = "PART 1";

			var supRelation11 = addANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>().PK);
			var supRelation12 = addANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Supplier, oneSharedSUP.PK);
			var ownRelation11 = addANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);
			var ownRelation12 = addANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);

			var pivotHTI11 = addACusClassPartPivot(part, ClassificationType.IMP, "123", ownRelation11.OU_OH);
			var pivotHTI12 = addACusClassPartPivot(part, ClassificationType.IMP, "223", supRelation12.OU_OH);
			var pivotHTE11 = addACusClassPartPivot(part, ClassificationType.EXP, "323", supRelation11.OU_OH);
			var pivotHTE12 = addACusClassPartPivot(part, ClassificationType.EXP, "423", supRelation12.OU_OH);
			var pivotHTB11 = addACusClassPartPivot(part, ClassificationType.Both, "523", ownRelation12.OU_OH);
			var pivotHTB12 = addACusClassPartPivot(part, ClassificationType.Both, "623", supRelation11.OU_OH);

			#endregion

			#region Preparation Part 2

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PARTNUM";
			part2.OP_Desc = "PART 2";
			var supRelation21 = addANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>().PK);
			var supRelation22 = addANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Supplier, oneSharedSUP.PK);
			var supRelation23 = addANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>().PK);
			var ownRelation21 = addANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);
			var ownRelation22 = addANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);

			var pivotHTI21 = addACusClassPartPivot(part2, ClassificationType.IMP, "1234", ownRelation21.OU_OH);
			var pivotHTI22 = addACusClassPartPivot(part2, ClassificationType.IMP, "2234", supRelation22.OU_OH);
			var pivotHTE21 = addACusClassPartPivot(part2, ClassificationType.EXP, "3234", supRelation21.OU_OH);
			var pivotHTE22 = addACusClassPartPivot(part2, ClassificationType.EXP, "4234", supRelation22.OU_OH);

			#endregion

			CombineAssertions("IMP", () =>
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = oneSharedSUP.PK;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals("Multi Part Match, pick Random 1", true, InvoiceLine.Pivot.PK == pivotHTI12.PK || InvoiceLine.Pivot.PK == pivotHTI22.PK);

				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation11.OU_OH;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTB12.PK, InvoiceLine.Pivot.PK);

				Declaration.JE_OH_Importer = ownRelation21.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation22.OU_OH;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTI21.PK, InvoiceLine.Pivot.PK);

				var pivotHTB2 = addACusClassPartPivot(part2, ClassificationType.Both, "5234", null);
				Declaration.JE_OH_Importer = ownRelation22.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation21.OU_OH;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTB2.PK, InvoiceLine.Pivot.PK);
				pivotHTB2.Delete();
			});

			CombineAssertions("EXP", () =>
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = oneSharedSUP.PK;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals("Multi Part Match, pick Random 1", true, InvoiceLine.Pivot.PK == pivotHTE12.PK || InvoiceLine.Pivot.PK == pivotHTE22.PK);

				Declaration.JE_OH_Importer = ownRelation11.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation12.OU_OH;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTE12.PK, InvoiceLine.Pivot.PK);

				Declaration.JE_OH_Importer = ownRelation11.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation11.OU_OH;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertNull("When ambiguity exist, no pivot should be picked.", InvoiceLine.Pivot);

				var pivotHTB2 = addACusClassPartPivot(part2, ClassificationType.Both, "5234", null);
				Declaration.JE_OH_Importer = ownRelation22.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation23.OU_OH;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTB2.PK, InvoiceLine.Pivot.PK);
			});
		}

		public override void TestWipeNKTaxType()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");

			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			dec.JE_MessageType = "IMP";
			cei.CEI_Style = "IFD";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();

			var taxOrFeeDetailEntity = new TaxOrFeeDetailEntity();
			taxOrFeeDetailEntity.VATCode = "RED";
			taxOrFeeDetailEntity.Description = "Reduced, V904, A505";
			taxOrFeeDetailEntity.AdditionalCode = "V904";
			taxOrFeeDetailEntity.Category = "A505";
			line.Lookups.TaxOrFeeDetailEntities.Add(taxOrFeeDetailEntity);

			line.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			Assert(line.JI_TaxOrFeeDetail != ZGuid.Empty);
			AssertEquals("RED", line.JI_ZZF_NKTaxType);
			line.JI_Procedure = "1111111";
			line.JI_CEI = cei.PK;
			Assert(line.ShouldWipeNKTaxType);
			AssertEquals(ZGuid.Empty, line.JI_TaxOrFeeDetail);
			AssertEquals(ZString.Empty, line.JI_ZZF_NKTaxType);
			AssertNull(line.JI_TaxOrFeeDetailEntity);

			line.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			line.JI_Procedure = "1111222";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals(taxOrFeeDetailEntity.PK, line.JI_TaxOrFeeDetail);
			AssertEquals("RED", line.JI_ZZF_NKTaxType);
			AssertNotNull(line.JI_TaxOrFeeDetailEntity);

			var additionalProcedure = line.AdditionalProcedureCodes.AddNew();
			additionalProcedure.CY_Code = "1111111";
			AssertNotNull(line.AdditionalProcedureWithCalculateVATIsFalse);
			Assert(line.ShouldWipeNKTaxType);
			AssertEquals(ZGuid.Empty, line.JI_TaxOrFeeDetail);
			AssertEquals(ZString.Empty, line.JI_ZZF_NKTaxType);
			AssertNull(line.JI_TaxOrFeeDetailEntity);

			line.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
			additionalProcedure.CY_Code = "1111222";
			AssertNull(line.AdditionalProcedureWithCalculateVATIsFalse);
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals(taxOrFeeDetailEntity.PK, line.JI_TaxOrFeeDetail);
			AssertEquals("RED", line.JI_ZZF_NKTaxType);
			AssertNotNull(line.JI_TaxOrFeeDetailEntity);

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.JI_Procedure = "1111111";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals(taxOrFeeDetailEntity.PK, line.JI_TaxOrFeeDetail);
			AssertEquals("RED", line.JI_ZZF_NKTaxType);
			AssertNotNull(line.JI_TaxOrFeeDetailEntity);
		}

		protected override ZString TariffDataGrouping => Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
		protected override ZString UniversalTariffTypeForDefaultTaxOrFeeCode => Enterprise.Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;

		public virtual void TestJI_DescriptionDefaultingFromTariff()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var importTariffType = helper.CreateNewOrGetExistingTariffType(currentCountryCode, "IMP");
			importTariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(currentCountryCode, importTariffType.PK, "0105111100", startDate, endDate, description: "Laying stocks", compositeKey: "01.01..05.1.1.10.10");
			helper.CreateTariff(currentCountryCode, importTariffType.PK, "9999999999", startDate, endDate, description: new string('0', 526), compositeKey: "");
			helper.CreateNomenclatureGroup(currentCountryCode, "01", startDate, endDate, "LIVE ANIMALS", compositeKey: "01.01", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(currentCountryCode, "0105", startDate, endDate, "Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls", compositeKey: "01.01..05", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(currentCountryCode, "010511", startDate, endDate, "Fowls of the species Gallus domesticus", compositeKey: "01.01..05.1.1", nomenclatureGroupType: "CN");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("JI_Description should be", "LIVE ANIMALS LIVE POULTRY, THAT IS TO SAY, FOWLS OF THE SPECIES GALLUS DOMESTICUS, DUCKS, GEESE, TURKEYS AND GUINEA FOWLS FOWLS OF THE SPECIES GALLUS DOMESTICUS LAYING STOCKS", invoiceLine.JI_Description);
			invoiceLine.JI_Tariff = "";
			AssertNullOrEmpty("JI_Description should be empty", invoiceLine.JI_Description);

			invoiceLine.JI_Tariff = "0105111100";
			AssertNotNullOrEmpty("JI_Description should have been restored", invoiceLine.JI_Description);
			invoiceLine.JI_Tariff = "1234567890";
			AssertEquals("JI_Description should be", "", invoiceLine.JI_Description);

			invoiceLine.JI_Tariff = "";
			invoiceLine.JI_Description = "some description";
			invoiceLine.JI_Tariff = "0105111100";
			AssertEquals("JI_Description should be", "some description", invoiceLine.JI_Description);

			invoiceLine.JI_Description = "";
			invoiceLine.JI_Tariff = "9999999999";
			AssertEquals("JI_Description has been truncated", invoiceLine.JI_DescriptionInfo.MaxLength, invoiceLine.JI_Description.Length);
			AssertEquals("JI_Description has been truncated", new string('0', invoiceLine.JI_DescriptionInfo.MaxLength), invoiceLine.JI_Description);
		}

		public virtual void TestNationalRateSelectionCriteria()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "1";
			invoiceLine.JI_SupplementaryCode2 = "2";

			var countervailingCriteria = invoiceLine.NationalRateSelectionCriteria;
			AssertNotNull("CountervailingRateSelectionCriteria", countervailingCriteria);
			AssertEquals(0, countervailingCriteria.Count());
		}

		public void TestSupplementaryCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals("When NO Supplementary Codes are entered, SupplementaryCodes Count", 0, invoiceLine.SupplementaryCodes.Count());

			invoiceLine.JI_SupplementaryCode1 = "S001";
			AssertEquals("SupplementaryCodes Count", 1, invoiceLine.SupplementaryCodes.Count());
			AssertContainsExactElementsInAnyOrder("", new ZString[] { "S001" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());

			invoiceLine.JI_SupplementaryCode2 = "S002";
			var additionalSupplementaryCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode3.CY_Code = "S003";
			additionalSupplementaryCode3.CY_Order = 3;
			var additionalSupplementaryCode4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode4.CY_Code = "";
			additionalSupplementaryCode3.CY_Order = 4;

			AssertEquals("SupplementaryCodes Count", 4, invoiceLine.SupplementaryCodes.Count());
			AssertContainsExactElementsInAnyOrder("", new ZString[] { "S001", "S002", "S003", "" }, invoiceLine.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
		}

		public void TestTariffFormatter()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			ITariffFormatProvider invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();

			if (!TariffFormatterTest.ExpectedTariffFormatterTypes.ContainsKey(currentCountry))
			{
				Assert($"Please make a decision about which TariffFormatter should be used for {currentCountry}. Add it to TariffFormatter and ExpectedTairffFormatterTypes", false);
			}
			else
			{
				var expected = TariffFormatterTest.ExpectedTariffFormatterTypes[currentCountry];
				AssertType($"TariffFormatter for {currentCountry} is incorrect", expected, invoiceLine.TariffFormatter);
			}
		}

		public void TestJI_ConcessionOrder_ClearValue()
		{
			InvoiceLine.ZG_SecondQuota = "999999";
			CombineAssertions(() =>
			{
				InvoiceLine.JI_ConcessionOrder = "123456";
				AssertEquals("JI_ConcessionOrder not empty, ZG_SecondQuota remains", "999999", InvoiceLine.ZG_SecondQuota);

				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertEquals("JI_ConcessionOrder empty, ZG_SecondQuota cleared", ZString.Empty, InvoiceLine.ZG_SecondQuota);
			});
		}

		public void TestZG_SecondQuotaReadOnly_Import()
		{
			CombineAssertions(() =>
			{
				InvoiceLine.JI_ConcessionOrder = "123456";
				AssertEquals("JI_ConcessionOrder not empty, ZG_SecondQuota editable", false, InvoiceLine.ZG_SecondQuotaInfo.ReadOnly);

				InvoiceLine.JI_ConcessionOrder = ZString.Empty;
				AssertEquals("JI_ConcessionOrder empty, ZG_SecondQuota read-only", true, InvoiceLine.ZG_SecondQuotaInfo.ReadOnly);
			});
		}

		public void TestJI_SecondQuotaReadOnly_Export()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_ConcessionOrder = ZString.Empty;
			AssertEquals("JI_ConcessionOrder empty, ZG_SecondQuota editable", false, InvoiceLine.ZG_SecondQuotaInfo.ReadOnly);
		}

		public void TestCustomsWeight()
		{
			var invoiceLine = Factory.New<T>();
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			AssertEquals("CustomsWeight in Kilograms (without KGM to KG mapping)", new ZWeight(100m, Core.Constants.Weight.Kilograms), invoiceLine.CustomsWeight);

			invoiceLine.JI_CustomsQuantity = 23.45m;
			invoiceLine.JI_CustomsUnitQty = RefCusCodeList.CustomsUq.Weight.Kilogram;
			AssertEquals("CustomsWeight in Kilograms (with KGM to KG mapping)", new ZWeight(23.45m, Core.Constants.Weight.Kilograms), invoiceLine.CustomsWeight);

			invoiceLine.JI_CustomsQuantity = 200m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Grams;
			AssertEquals("CustomsWeight in Grams", new ZWeight(200m, Core.Constants.Weight.Grams), invoiceLine.CustomsWeight);
		}

		protected virtual void AssertEffectiveVatApplicabilitesWithoutUniversalFeeCalculation(T invoiceLine)
		{
			AssertEquals("No Vat Applicabilities Expected", false, invoiceLine.GetEffectiveVATApplicabilities().Any());
		}

		public void TestZG_CusNumber_Caption()
		{
			AssertEquals("CUS Code", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_CusNumberInfo).Caption);
		}

		public void TestCustomsQuantityConverter()
		{
			AssertType(typeof(CustomsQuantityConverter), InvoiceLine.CustomsQuantityConverter);
		}

		public void TestCustomsQuantity2Converter()
		{
			AssertType(typeof(CustomsQuantityConverter), InvoiceLine.CustomsQuantity2Converter);
		}

		#region Implementation

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			return dec;
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected new T InvoiceLine => (T)base.InvoiceLine;

		#endregion

		public void TestTotalBondedWhsQuantity()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_BondedWhsQuantity = 23.45m;
			invoiceLine2.JI_BondedWhsQuantity = 23.55m;

			AssertEquals(47m, invoiceLine1.TotalBondedWhsQuantity);
			AssertEquals(47m, invoiceLine2.TotalBondedWhsQuantity);
		}

		public void TestTotalCustomsQuantity()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_CustomsQuantity = 33.45m;
			invoiceLine2.JI_CustomsQuantity = 33.55m;

			AssertEquals(67m, invoiceLine1.TotalCustomsQuantity);
			AssertEquals(67m, invoiceLine2.TotalCustomsQuantity);
		}

		public void TestTotalWeight()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_Weight = 43.45m;
			invoiceLine2.JI_Weight = 43.55m;

			AssertEquals(87m, invoiceLine1.TotalWeight);
			AssertEquals(87m, invoiceLine2.TotalWeight);
		}

		public void TestTotalLinePrice()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_LinePrice = 53.45m;
			invoiceLine2.JI_LinePrice = 53.55m;

			AssertEquals(107m, invoiceLine1.TotalLinePrice);
			AssertEquals(107m, invoiceLine2.TotalLinePrice);
		}

		public virtual void TestJI_BondedWhsQuantityCaption()
		{
			AssertEquals("Bonded Whs. Qty", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().JI_BondedWhsQuantityInfo).Caption);
		}

		public virtual void TestJI_BondedWhsUnitQtyCaption()
		{
			AssertEquals("Bonded Whs. UQ.", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().JI_BondedWhsUnitQtyInfo).Caption);
		}

		public virtual void TestJI_PreviousEntryNumberCaption()
		{
			AssertEquals("Prev. Entry No.", DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceLine>().JI_PreviousEntryNumberInfo).Caption);
		}

		public virtual void TestJI_PreviousEntryLineNumberCaption()
		{
			var previousEntryLineNumberInfo = Factory.New<JobComInvoiceLine>().JI_PreviousEntryLineNumberInfo;
			AssertEquals("Prev. Entry Line No.", DataBoundResourceStrings.GetDataForProperty(previousEntryLineNumberInfo).Caption);
			AssertEquals("Prev. Line No.", DataBoundResourceStrings.GetDataForProperty(previousEntryLineNumberInfo).ShortCaption);
		}

		public virtual void TestOnCusProcedureChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			var procedure = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Import, intoWarehouse: true, outOfWarehouse: false);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = GetJobDeclarationForTesting();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				invoiceLine.JI_InvoiceQuantity = 123m;
				invoiceLine.JI_InvoiceUQ = "KGM";

				CombineAssertions(() =>
				{
					invoiceLine.JI_Procedure = "1234567";
					AssertEquals("Warehouse quantity defaulted from invoice quantity.", 123m, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals("Warehouse unit defaulted from invoice unit.", "KGM", invoiceLine.JI_BondedWhsUnitQty);

					invoiceLine.JI_Procedure = "2345678";
					AssertEquals("Warehouse quantity cleared.", 0m, invoiceLine.JI_BondedWhsQuantity);
					AssertEquals("Warehouse unit cleared.", "", invoiceLine.JI_BondedWhsUnitQty);
				});
			}
		}

		public void TestEffectiveCountryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			declaration.JE_GoodsDestination = "AU";
			invoiceLine.ZG_CountryOfDestination = "DE";
			AssertEquals("EffectiveCountryOfDestination", "DE", invoiceLine.EffectiveCountryOfDestination);

			invoiceLine.ZG_CountryOfDestination = "";
			AssertEquals("When ZG_CountryOfDestination is Empty, fallback to JE_GoodsDestination. EffectiveCountryOfDestination", "AU", invoiceLine.EffectiveCountryOfDestination);
		}

		public virtual void TestDutyAmountsAsStringCore()
		{
			AssertNoExceptionThrown(() => { var dutyAmount = Factory.New<JobComInvoiceLine>().DutyAmountsAsString; });
		}

		public void TestFiscalReferencesDefault()
		{
			var declaration = GetJobDeclarationForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var organisation = Factory.New<OrgHeader>();
			organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", CountryCodes.Finland);

			var reference = invoiceLine.FiscalReferences.AddNew();
			reference.CFR_OA_Owner = organisation.MainAddress.PK;

			if (declaration.Configuration.UseEoriForFiscalReference)
			{
				AssertEquals("FI1234", reference.CFR_Reference);
			}
			else
			{
				AssertNullOrEmpty(reference.CFR_Reference);
			}
		}

		public void TestCountryOfOriginFallback()
		{
			CreateEUCountry();

			var bo = Factory.New<BaseJobComInvoiceLine>();
			bo.JI_CountryOfOrigin = "EU";

			CombineAssertions(() =>
			{
				AssertEquals("Code of CountryOfOriginFallback of the invoice line should refer to EU", "EU", bo.CountryOfOriginFallback.RN_Code);
				AssertEquals("Description of CountryOfOriginFallback of the invoice line should refer to " + euDescription, euDescription, bo.CountryOfOriginFallback.RN_Desc);
			});
		}

		public void TestCountryOfExportFallback()
		{
			CreateEUCountry();

			var bo = Factory.New<BaseJobComInvoiceLine>();
			bo.JI_RN_NKCountryOfExport = "EU";

			CombineAssertions(() =>
			{
				AssertEquals("Code of CountryOfExportFallback of the invoice line should refer to EU", "EU", bo.CountryOfExportFallback.RN_Code);
				AssertEquals("Description of CountryOfExportFallback of the invoice line should refer to " + euDescription, euDescription, bo.CountryOfExportFallback.RN_Desc);
			});
		}

		public virtual void TestGetNewLinkPackValidation()
		{
			AssertType<InvoiceLinePackageValidation>(InvoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew().Validation);
		}

		protected virtual void CreateEUCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup = helper.CreateTradeGroup(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EuropeanUnionForCustoms, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var country = helper.AddCountry(tradeGroup, "EU", countryCodeDescription: euDescription);

			var refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = "EU";
			refCountry.RN_Desc = euDescription;
		}

		protected virtual string euDescription => "European Union";

		protected virtual JobDeclaration GetJobDeclarationForTesting() => Factory.New<JobDeclaration>();

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(InvoiceLineChargeCollection<InvoiceLineCharge>);

		protected virtual Type ExpectedAdditionalProcedureCode => typeof(AdditionalProcedureCode);

		protected virtual Type ExpectedLinkPackValidation => typeof(InvoiceLinePackageValidation);
	}

	class JobComInvLineForTest : JobComInvoiceLine
	{
		public JobComInvLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZInt MaxNumberOfAdditionalProcedureCode => 1;

		protected override ZBool ShouldCheckMissingPreviousDocumentsCore => shouldCheckMissingPreviousDocumentsIndex == 0;
		int shouldCheckMissingPreviousDocumentsIndex;

		public DisposableAction SuspendCheckMissingPreviousDocuments() => new DisposableAction(() => shouldCheckMissingPreviousDocumentsIndex++, () => shouldCheckMissingPreviousDocumentsIndex--);

		public override bool HasOutOfInwardProcessingProcedure => true;

		public new ZDecimal ComponentPrice => base.ComponentPrice;

		public new T GetInvoiceEffectiveValueToReturnIfNeeded<T>(T baseValue, string fieldNameInInvoiceLine, string fieldNameInInvoiceHeader) where T : IZType
			=> base.GetInvoiceEffectiveValueToReturnIfNeeded(baseValue, fieldNameInInvoiceLine, fieldNameInInvoiceHeader);

		public new T GetEntryInstructionEffectiveValueToReturnIfNeeded<T>(T baseValue, string fieldNameInInvoiceLine, string fieldNameInInstruction) where T : IZType
			=> base.GetEntryInstructionEffectiveValueToReturnIfNeeded(baseValue, fieldNameInInvoiceLine, fieldNameInInstruction);

		public new T GetDeclarationEffectiveValueToReturnIfNeeded<T>(T baseValue, string fieldNameInInvoiceLine, string fieldNameInDeclaration) where T : IZType
			=> base.GetDeclarationEffectiveValueToReturnIfNeeded(baseValue, fieldNameInInvoiceLine, fieldNameInDeclaration);

		public new T GetEffectiveValueToSetCompareToInvoice<T>(T valuePassed, string fieldNameInInvoiceHeader) where T : IZType
			=> base.GetEffectiveValueToSetCompareToInvoice(valuePassed, fieldNameInInvoiceHeader);

		public new T GetEffectiveValueToSetCompareToEntryInstruction<T>(T valuePassed, string fieldNameInInstruction) where T : IZType
			=> base.GetEffectiveValueToSetCompareToEntryInstruction(valuePassed, fieldNameInInstruction);

		public new T GetEffectiveValueToSetCompareToDeclaration<T>(T valuePassed, string fieldNameInJobDeclaration) where T : IZType
			=> base.GetEffectiveValueToSetCompareToDeclaration(valuePassed, fieldNameInJobDeclaration);
	}
}
