using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		protected override ZString ExpectedFallbackEntrylineDescription => "LINE";

		public void TestConfirmedFeesReadOnlyType()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertType<CusEntryLineConfirmedFeeWrapperCollection>(entryLine.ConfirmedFeesReadOnly);
		}

		public void TestIsProductOfNegligibleValueToDROM()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine.PK;

			Assert("Prerequisite: first invoice line is not a product of negligible value to DROM.", !invoiceLine1.IsProductOfNegligibleValueToDROM);
			Assert("Prerequisite: second invoice line is not a product of negligible value to DROM.", !invoiceLine2.IsProductOfNegligibleValueToDROM);
			Assert("The entry line should not be considered as product of negligible value to DROM when no invoice line is", !entryLine.IsProductOfNegligibleValueToDROM);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.ProductOfNegligibleValueToDROMSupplementaryCode;
			Assert("Prerequisite: second invoice line is a product of negligible DROM", invoiceLine2.IsProductOfNegligibleValueToDROM);
			Assert("The entry line should be considered as a product of negligible value to DROM when any of it's invoice line is", entryLine.IsProductOfNegligibleValueToDROM);
		}

		public void TestCL_Calc_GuaranteeAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Description, euGrouping);
			Factory.Save();
			var euDtyRateType = helper.CreateNewOrGetExistingRateType(euGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "";
			usage.AGC_OH_Owner = ZGuid.Empty;
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault() ?? declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.FirstOrDefault() ?? entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 12);
			entryLine.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29);
			entryLine.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";
			CombineAssertions("To make sure EntryLine fields return expected result.", () =>
			{
				AssertEquals(12m, entryLine.DutyAmount);
				AssertEquals(29m, entryLine.GSTVATAmount);
				AssertEquals(43m, entryLine.ParaFiscal);
			});
			AssertEquals("(DutyAmount 12 x 100%) + (GSTVATAmount 29 x 5%) + (ParaFiscal 43 x 15%)", 19.9m, entryLine.SpecificRegimeGuaranteeAmount);
			AssertEquals("CL_Calc_GuaranteeAmount should be equal to the rounding of SpecificRegimeGuaranteeAmount", 20m, entryLine.CL_Calc_GuaranteeAmount);

			entryLine.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 17);
			CombineAssertions("To make sure EntryLine fields return expected result.", () =>
			{
				AssertEquals(12m, entryLine.DutyAmount);
				AssertEquals(17m, entryLine.GSTVATAmount);
				AssertEquals(43m, entryLine.ParaFiscal);
			});
			AssertEquals("(DutyAmount 12 x 100%) + (GSTVATAmount 17 x 5%) + (ParaFiscal 43 x 15%)", 19.3m, entryLine.SpecificRegimeGuaranteeAmount);
			AssertEquals("CL_Calc_GuaranteeAmount should be equal to the rounding of SpecificRegimeGuaranteeAmount", 19m, entryLine.CL_Calc_GuaranteeAmount);
		}
		internal static CusAuthorisationRule CreateCusAuthorisationRule(CusAuthorisationHeader authHeader, string code, string value)
		{
			var rule = authHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = code;
			rule.CPR_ValueFrom = value;
			return rule;
		}

		public void TestValidationType()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertType<CusEntryLineValidation>(entryLine.Validation);
		}

		public void TestIsPromotionalProductToDROM()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine.PK;

			Assert("Prerequisite: first invoice line is not promotional product to DROM.", !invoiceLine1.IsPromotionalProductToDROM);
			Assert("Prerequisite: second invoice line is not promotional product to DROM.", !invoiceLine2.IsPromotionalProductToDROM);
			Assert("The entry line should not be considered as promotional product to DROM when no invoice line is.", !entryLine.IsPromotionalProductToDROM);

			invoiceLine2.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.PromotionalProductToDROMSupplementaryCode;
			Assert("Prerequisite: second invoice line is not promotional product to DROM.", invoiceLine2.IsPromotionalProductToDROM);
			Assert("The entry line should be considered as promotional product to DROM when any invoice line is.", entryLine.IsPromotionalProductToDROM);
		}

		public void TestHasNegligibleValueProcedure()
		{
			var entryLine  = Factory.New<CusEntryLine>();

			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine1.JI_Procedure = "1122000";
			invoiceLine2.JI_Procedure = "3344000";
			Assert("HasNegligibleValueProcedure should be false when no invoice line has procedure concession C07.", !entryLine.HasNegligibleValueProcedure);

			invoiceLine1.JI_Procedure = "1122" + FRConstants.ThresholdsAndLimits.NegligibleValueProcedure;
			invoiceLine2.JI_Procedure = "3344000";
			Assert("HasNegligibleValueProcedure should be true when at least one invoice line have procedure concession C07.", entryLine.HasNegligibleValueProcedure);

			invoiceLine1.JI_Procedure = "1122" + FRConstants.ThresholdsAndLimits.NegligibleValueProcedure;
			invoiceLine2.JI_Procedure = "3344" + FRConstants.ThresholdsAndLimits.NegligibleValueProcedure;
			Assert("HasNegligibleValueProcedure should be true when all invoice lines have procedure concession C07.", entryLine.HasNegligibleValueProcedure);
		}

		public void TestHasC2CProcedure()
		{
			var entryLine = Factory.New<CusEntryLine>();

			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine1.JI_Procedure = "1122000";
			invoiceLine2.JI_Procedure = "3344000";
			Assert("HasC2CProcedure should be false when no invoice line has procedure concession C08.", !entryLine.HasC2CProcedure);

			invoiceLine1.JI_Procedure = "1122" + FRConstants.ThresholdsAndLimits.C2CValueProcedureSuffix;
			invoiceLine2.JI_Procedure = "3344000";
			Assert("HasC2CProcedure should be true when at least one invoice line have procedure concession C08.", entryLine.HasC2CProcedure);

			invoiceLine1.JI_Procedure = "1122" + FRConstants.ThresholdsAndLimits.C2CValueProcedureSuffix;
			invoiceLine2.JI_Procedure = "3344" + FRConstants.ThresholdsAndLimits.C2CValueProcedureSuffix;
			Assert("HasC2CProcedure should be true when all invoice lines have procedure concession C08.", entryLine.HasC2CProcedure);
		}

		public void TestIsT2LApplicable()
		{
			var testCase = new MultiFactorTestCase<CusEntryLine>(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_Style = "A";
				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = cei.PK;
				Factory.Save();
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				declaration.JE_MessageType = "IMP";

				var merger = new LineMerger(declaration);
				merger.DoMerge();
				return declaration.CustomsEntryHeaders[0].MergedLines[0];
			});

			var isExport = new FieldPreq<CusEntryLine>(line => line.Declaration.JE_MessageTypeInfo).Values(EU.Business.MessageTypeList.Codes.Export).NotValues(EU.Business.MessageTypeList.Codes.Import);
			var transportModeSea = new FieldPreq<CusEntryLine>(line => line.Declaration.JE_TransportModeInfo).Values(Core.Constants.TransportModes.Sea).NotValues(Core.Constants.TransportModes.Air);
			var decHasT2LSupDoc = new Preq<CusEntryLine>(line => line.Declaration.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument, RemoveAllSupportingDocuments);
			var invoiceHasT2LSupDoc = new Preq<CusEntryLine>(line => line.Declaration.Invoices[0].SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument, RemoveAllSupportingDocuments);
			var invoiceLineHasT2LSupDoc = new Preq<CusEntryLine>(line => line.Declaration.Invoices[0].InvoiceLines[0].SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument, RemoveAllSupportingDocuments);

			testCase.SetUpCondition(isExport && transportModeSea && (decHasT2LSupDoc || invoiceHasT2LSupDoc || invoiceLineHasT2LSupDoc));
			testCase.RunAssertion(line => Assert(line.IsT2LApplicable), line => Assert(!line.IsT2LApplicable));
		}

		public void TestIsT2LFApplicable()
		{
			var testCase = new MultiFactorTestCase<CusEntryLine>(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_Style = "A";
				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = cei.PK;
				Factory.Save();
				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				declaration.JE_MessageType = "IMP";

				var merger = new LineMerger(declaration);
				merger.DoMerge();
				return declaration.CustomsEntryHeaders[0].MergedLines[0];
			});

			var isExport = new FieldPreq<CusEntryLine>(line => line.Declaration.JE_MessageTypeInfo).Values(EU.Business.MessageTypeList.Codes.Export).NotValues(EU.Business.MessageTypeList.Codes.Import);
			var decHasT2LSupDoc = new Preq<CusEntryLine>(line => line.Declaration.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument, RemoveAllSupportingDocuments);
			var invoiceHasT2LSupDoc = new Preq<CusEntryLine>(line => line.Declaration.Invoices[0].SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument, RemoveAllSupportingDocuments);
			var invoiceLineHasT2LSupDoc = new Preq<CusEntryLine>(line => line.Declaration.Invoices[0].InvoiceLines[0].SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LFDocument, RemoveAllSupportingDocuments);

			testCase.SetUpCondition(isExport && (decHasT2LSupDoc || invoiceHasT2LSupDoc || invoiceLineHasT2LSupDoc));
			testCase.RunAssertion(line => Assert(line.IsT2LFApplicable), line => Assert(!line.IsT2LFApplicable));
		}

		void RemoveAllSupportingDocuments(CusEntryLine entryLine)
		{
			entryLine.Declaration.SupportingDocuments.RemoveAndDeleteAll();
			entryLine.Declaration.Invoices[0].SupportingDocuments.RemoveAndDeleteAll();
			entryLine.Declaration.Invoices[0].InvoiceLines[0].SupportingDocuments.RemoveAndDeleteAll();
		}

		public void TestAdditionalInfos_ShouldBeMerged_WhenSameCSI_SubTypeAndCSI_ReferenceNumberAndCSI_CodeShared()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			var informationAdditionalInfo1 = invoiceLine.AdditionalInfos.AddNew();
			informationAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo1.CSI_Code = "INF1";
			informationAdditionalInfo1.CSI_ReferenceNumber = "REF1";

			var informationAdditionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF1";
			informationAdditionalInfo2.CSI_ReferenceNumber = "REF2";

			var informationAdditionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
			informationAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo3.CSI_Code = "INF2";
			informationAdditionalInfo3.CSI_ReferenceNumber = "REF2";

			var informationAdditionalInfo4 = invoiceLine.AdditionalInfos.AddNew();
			informationAdditionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			informationAdditionalInfo4.CSI_Code = "TRA1";
			informationAdditionalInfo4.CSI_ReferenceNumber = "REF1";
			informationAdditionalInfo4.CSI_Status = "ST1";

			var informationAdditionalInfo5 = invoiceLine.AdditionalInfos.AddNew();
			informationAdditionalInfo5.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			informationAdditionalInfo5.CSI_Code = "TRA1";
			informationAdditionalInfo5.CSI_ReferenceNumber = "REF1";
			informationAdditionalInfo5.CSI_Status = "ST2";

			Factory.Save();

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_MessageType = "IMP";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertContainsExactElementsInAnyOrder("Only additional infos sharing same CSI_SubType, CSI_Code and CSI_ReferenceNumber should merge.", new string[] { "INF|INF1|REF1", "INF|INF1|REF2", "INF|INF2|REF2", "TRA|TRA1|REF1" }, declaration.CustomsEntryHeaders[0].MergedLines[0].AdditionalInfos.Select(x => x.CSI_SubType + "|" + x.CSI_Code + "|" + x.CSI_ReferenceNumber));
		}

		public void TestAdditionalInfos_ShouldContainAddInfosFromInvoiceAndInvoiceLines_WhenDeclarationIsNonUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			CusEntryHeaderTest.SetUpDataForAdditionalInfosTest(declaration);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			AssertContainsExactElementsInAnyOrder(new ZString[] { "INV1", "INV2", "INV3", "LIN1", "LIN2", "LIN3" }, entryLine.AdditionalInfos.Select(x => x.CSI_Code));
		}

		public void TestAdditionalInfos_ShouldContainAddInfosFromInvoiceLines_WhenDeclarationIsUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			CusEntryHeaderTest.SetUpDataForAdditionalInfosTest(declaration);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			AssertContainsExactElementsInAnyOrder(new ZString[] { "LIN1", "LIN2", "LIN3" }, entryLine.AdditionalInfos.Select(x => x.CSI_Code));
		}

		public void TestConfirmedCL_ConfirmedOrCalculatedStatisticalValue()
		{
			var entryLineMock = Factory.NewMoq<CusEntryLine>();
			entryLineMock.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES055;

			var entryLine = entryLineMock.Object;
			entryLine.CL_CH = entryHeader.PK;
			entryLine.CL_ConfirmedStatisticalValue = 3m;
			entryLine.CL_StatisticalValue = 4m;

			AssertEquals(3m, entryLine.CL_ConfirmedOrCalculatedStatisticalValue);
			entryLineMock.VerifyAll();

			var entryLineMock2 = Factory.NewMoq<CusEntryLine>();
			entryLineMock2.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(false);
			var entryLine2 = entryLineMock2.Object;
			entryLine2.CL_CH = entryHeader.PK;
			entryLine2.CL_ConfirmedStatisticalValue = 3m;
			entryLine2.CL_StatisticalValue = 4m;

			AssertEquals(4m, entryLine2.CL_ConfirmedOrCalculatedStatisticalValue);
			entryLineMock2.VerifyAll();
		}

		public void TestCL_ConfirmedOrCalculatedCustomsValue()
		{
			var entryLineMock = Factory.NewMoq<CusEntryLine>();
			entryLineMock.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES055;

			var entryLine = entryLineMock.Object;
			entryLine.CL_CH = entryHeader.PK;
			entryLine.CL_ConfirmedCustomsValue = 3m;
			entryLine.CL_CustomsValue = 4m;

			AssertEquals(3m, entryLine.CL_ConfirmedOrCalculatedCustomsValue);
			entryLineMock.VerifyAll();

			var entryLineMock2 = Factory.NewMoq<CusEntryLine>();
			entryLineMock2.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(false);
			var entryLine2 = entryLineMock2.Object;
			entryLine2.CL_CH = entryHeader.PK;
			entryLine2.CL_ConfirmedCustomsValue = 3m;
			entryLine2.CL_CustomsValue = 4m;

			AssertEquals(4m, entryLine2.CL_ConfirmedOrCalculatedCustomsValue);
			entryLineMock2.VerifyAll();
		}

		public void TestCL_ConfirmedOrCalculatedValueForVAT()
		{
			var entryLineMock = Factory.NewMoq<CusEntryLine>();
			entryLineMock.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES055;

			var entryLine = entryLineMock.Object;
			entryLine.CL_CH = entryHeader.PK;
			entryLine.CL_ConfirmedValueForVAT = 3m;
			entryLine.CL_ValueForVAT = 4m;

			AssertEquals(3m, entryLine.CL_ConfirmedOrCalculatedValueForVAT);
			entryLineMock.VerifyAll();

			var entryLineMock2 = Factory.NewMoq<CusEntryLine>();
			entryLineMock2.Protected().Setup<ZBool>("CusEntryLinesConfirmedValueHasBeenPopulated").Returns(false);
			var entryLine2 = entryLineMock2.Object;
			entryLine2.CL_CH = entryHeader.PK;
			entryLine2.CL_ConfirmedValueForVAT = 3m;
			entryLine2.CL_ValueForVAT = 4m;

			AssertEquals(4m, entryLine2.CL_ConfirmedOrCalculatedValueForVAT);
			entryLineMock2.VerifyAll();
		}

		public void TestDutyAmount()
		{
			// Create rate codes of different types
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);

			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, "DEV", UniversalReferenceConstants.RefCusRateCodes.Q422);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var fee1 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent);
			fee1.CF_ChargeAmount = 100m;

			CombineAssertions("EntryLine with 1 DTY type fees: EA.", () =>
			{
				AssertEquals("DutyDetails", 100m, entryLine.DutyAmount);
			});

			var fee2 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
			fee2.CF_ChargeAmount = 50m;

			CombineAssertions("After adding non DTY type fee VAT, not included in the duty amount calculation.", () =>
			{
				AssertEquals("DutyDetails", 100m, entryLine.DutyAmount);
			});

			var fee3 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty);
			fee3.CF_ChargeAmount = 70m;

			CombineAssertions("EntryLine with 3 fees, 2 of which Duty types (ADD, CVD): EA, A40.", () =>
			{
				AssertEquals("DutyDetails", 170m, entryLine.DutyAmount);
			});

			var fee4 = entryLine.Fees.GetOrAddFeeByFeeType(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);
			fee4.CF_ChargeAmount = 40m;

			CombineAssertions("EntryLine with 4 fees, 3 of which Duty types (ADD, CVD): EA, A40, A35.", () =>
			{
				AssertEquals("DutyDetails", 210m, entryLine.DutyAmount);
			});

			var fee6 = entryLine.Fees.GetOrAddFeeByFeeType(CommunautaryChargeCodeList.Codes._1I1);
			fee6.CF_ChargeAmount = 40m;

			CombineAssertions("EntryLine with 5 fees, 4 of which Duty types (ADD, CVD, DEV): EA, A40, A35, 1|1", () =>
			{
				AssertEquals("DutyDetails", 250m, entryLine.DutyAmount);
			});
		}

		public void TestSpecificRegimeNumberDaysOfDischarge()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			customer.OH_RL_NKClosestPort = "GBLON";
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			authHeader.CPH_OH_PermitHolder = customer.PK;
			authHeader.CPH_Number = "TST_ATH_001";
			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "TST_ATH_001";
			usage.AGC_OH_Owner = customer.PK;

			AssertNull("NumberDaysOfDischarge", entryLine.SpecificRegimeNumberDaysOfDischarge);

			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.STO, "9");
			AssertEquals("NumberDaysOfDischarge", 9, entryLine.SpecificRegimeNumberDaysOfDischarge);
		}

		public void TestHasIntoWarehouseProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals(false, entryLine.HasIntoRegimeProcedure);

			invoiceLine2.JI_Procedure = "1071F61";
			AssertEquals(true, entryLine.HasIntoRegimeProcedure);
		}

		public void TestHasOutOfWarehouseProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals(false, entryLine.HasOutOfRegimeProcedure);

			invoiceLine2.JI_Procedure = "1071F61";
			AssertEquals(true, entryLine.HasOutOfRegimeProcedure);
		}

		public void TestCusEntryLineFeeCollectionType()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertType<ConfirmedCusEntryLineFeeCollection>(entryLine.ConfirmedFees);
			AssertType<EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>>(entryLine.Fees);
		}

		public void TestGetTaxBoxSupporterListCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			AssertEquals(0, entryLine.GetTaxBoxSupporterList().Count());
			var confirmFee = entryLine.ConfirmedFees.AddNew();
			confirmFee.FillWithValidTestData();
			confirmFee.CF_BaseValue = 10;

			AssertEquals(1, entryLine.GetTaxBoxSupporterList().Count());
			var taxsupporter = entryLine.GetTaxBoxSupporterList().ElementAt(0);
			AssertEquals("10", taxsupporter.TaxBase);
		}

		public void TestGetTaxBoxSupporterListCore_IncludeEntryHeaderCharges()
		{
			var entry = Factory.New<CusEntryHeader>();
			var charge1 = entry.ConfirmedCharges.AddNew();
			charge1.C1_ChargeType = "A01";
			charge1.C1_MethodOfPayment = "1";
			charge1.C1_ChargeAmount = 11m;
			var charge2 = entry.ConfirmedCharges.AddNew();
			charge2.C1_ChargeType = "A02";
			charge2.C1_MethodOfPayment = "2";
			charge2.C1_ChargeAmount = 22m;
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var confirmFee1 = entryLine1.ConfirmedFees.AddNew();
			confirmFee1.CF_ChargeType = "B01";
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var confirmFee2 = entryLine2.ConfirmedFees.AddNew();
			confirmFee2.CF_ChargeType = "B02";

			CombineAssertions(() =>
			{
				var taxBoxSupporters1 = entryLine1.GetTaxBoxSupporterList();
				AssertEquals("Entryheader charges added to the first EntryLine", 3, taxBoxSupporters1.Count());
				AssertEquals("Entryheader charge1", "A01", taxBoxSupporters1.ElementAt(0).Type);
				AssertEquals("Entryheader charge2", "A02", taxBoxSupporters1.ElementAt(1).Type);
				AssertEquals("EntryLine1 fee", "B01", taxBoxSupporters1.ElementAt(2).Type);

				var taxBoxSupporters2 = entryLine2.GetTaxBoxSupporterList();
				AssertEquals("Only EntryLine fee", 1, taxBoxSupporters2.Count());
				AssertEquals("EntryLine2 fee", "B02", taxBoxSupporters2.ElementAt(0).Type);
			});
		}

		public void TestHasNonEmptyPackage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 0;
			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 0;
			package1.CW_PackType = "AA";
			package1.CW_MarksAndNos = "AAAAAAAAAAAA";

			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 0;

			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			AssertEquals(false, entryLine.HasNonEmptyPackage);

			package1.CW_PackType = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
			AssertEquals(false, entryLine.HasNonEmptyPackage);

			invoiceLine1.JI_InvoiceQuantity = 1;
			AssertEquals(true, entryLine.HasNonEmptyPackage);

			package1.CW_PackType = "AA";
			package1.CW_PackQty = 1;
			packing1.PackQty = 1;
			AssertEquals(true, entryLine.HasNonEmptyPackage);
		}

		public void TestPackage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 10;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 20;
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 40;
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 80;
			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 160;

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "AA";
			package1.CW_MarksAndNos = "AAAAAAAAAAAA";

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "AA";
			package2.CW_MarksAndNos = "AAAAAAAAAAAA";

			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackQty = 4;
			package3.CW_PackType = "AA";
			package3.CW_MarksAndNos = "BBBBBBBBBBBB1";

			var package4 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package4.CW_PackQty = 8;
			package4.CW_PackType = "BB";
			package4.CW_MarksAndNos = "AAAAAAAAAAAA";

			var package5 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package5.CW_PackQty = 16;
			package5.CW_PackType = "BB";
			package5.CW_MarksAndNos = "BBBBBBBBBBBB2";

			var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 1;

			var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1];
			packing2.IsLinked = true;
			packing2.PackQty = 2;

			var packing3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly[2];
			packing3.IsLinked = true;
			packing3.PackQty = 4;

			var packing4 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[3];
			packing4.IsLinked = true;
			packing4.PackQty = 8;

			var packing5 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[4];
			packing5.IsLinked = true;
			packing5.PackQty = 16;

			var entryLine = Factory.New<CusEntryLine>();

			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine3.JI_CL = entryLine.PK;
			invoiceLine4.JI_CL = entryLine.PK;
			invoiceLine5.JI_CL = entryLine.PK;

			AssertEquals(5, entryLine.PackagingDetails.Count());
			AssertEquals("AA", entryLine.Package.Type);
			AssertEquals("AAAAAAAAAAAA, BBBBBBBBBBBB1", entryLine.Package.MarksAndNos);
			AssertEquals(7, entryLine.Package.Count);
			AssertEquals(70m, entryLine.Package.ItemsCount);
		}

		public void TestCL_BaseTVA()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);

			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entry = entryHeader.MergedLines.AddNew();

			entry.CL_ValueForVAT = 1000;
			entry.Fees.SetAmount(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 200);
			AssertEquals("Pre-condition", 200m, entry.DutyAmount);
			AssertEquals("BaseVATableValue should equal ValueForVat plus the Duty Amount", 1200m, entry.CL_BaseVATableValue);

			entry.CL_ValueForVAT = 0;
			AssertEquals("BaseVATableValue should not be less than zero", ZDecimal.Zero, entry.CL_BaseVATableValue);
		}

		public void TestCL_BaseTVAUQ()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entry = entryHeader.MergedLines.AddNew();

			AssertEquals("BaseVATableValue UQ should use the local currency.", "EUR", entry.CL_BaseVATableValueUQ);
		}

		public void TestGetTaxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var tax1 = invoiceLine.Taxes.AddNew().Data;
			tax1.G4_Type = "ABC";
			tax1.G4_Amount = "15";
			tax1.G4_BaseAmount = 15;
			var tax2 = invoiceLine.Taxes.AddNew().Data;
			tax2.G4_Type = "DEF";
			tax2.G4_Amount = "25";
			tax2.G4_BaseAmount = 25;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			var charge1 = entry.Charges.AddNew();
			charge1.C1_ChargeType = "GHI";
			charge1.C1_ChargeAmount = 10;
			var charge2 = entry.Charges.AddNew();
			charge2.C1_ChargeType = "JKL";
			charge2.C1_ChargeAmount = 20;

			var taxes = entryLine.Taxes;

			AssertEquals(4, taxes.Count);
			AssertEquals("15.00", taxes[0].G4_Amount_InDeclarationCurrency);
			AssertEquals("25.00", taxes[1].G4_Amount_InDeclarationCurrency);
			AssertEquals("10.00", taxes[2].G4_Amount_InDeclarationCurrency);
			AssertEquals("20.00", taxes[3].G4_Amount_InDeclarationCurrency);
		}

		public void TestSupportingDocumentsDTP()
		{
			SetupForSupportingDocumentTests();

			AssertEquals(2, entryLine.SupportingDocumentsDTP.Count());
			Assert(entryLine.SupportingDocumentsDTP.Any(x => x.CSI_Description == "B"));
			Assert(entryLine.SupportingDocumentsDTP.Any(x => x.CSI_Description == "C"));
		}

		public void TestSupportingDocumentsToCustoms()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;

			var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
			declarationSupportingDocument.CSI_Code = "ZZZZ";
			declarationSupportingDocument.CSI_ReferenceNumber = "ZZZZZZZ";
			declarationSupportingDocument.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_CEI = instruction.PK;

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "2700";
			supportingDocument1.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument1.CSI_Description = "APPLE";
			supportingDocument1.CSI_LineNo = 1;

			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "2700";
			supportingDocument2.CSI_ReferenceNumber = "AAAAAAAA";
			supportingDocument2.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);
			supportingDocument1.CSI_Description = "ORANGES";
			supportingDocument2.CSI_LineNo = 2;

			var supportingDocument3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "BBBB";
			supportingDocument3.CSI_ReferenceNumber = "BBBBBBBB";
			supportingDocument3.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);

			var supportingDocument4 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "BBBB";
			supportingDocument4.CSI_ReferenceNumber = "BBBBBBBB";
			supportingDocument4.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);

			var supportingDocument5 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "CCCC";
			supportingDocument5.CSI_ReferenceNumber = "CCCCCCC";
			supportingDocument5.CSI_DateOfIssue = new ZDateTime(2021, 01, 01);

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("SupportingDocumentsToCustoms return a list of merged entry line and header documents + authorisation document. Permits merge key is on purpose reduced to code, reference and date.", 4, entryLine.SupportingDocumentsToCustoms.Count());
			AssertContainsExactElementsInAnyOrder(new string[] { "ZZZZ|ZZZZZZZ|20210101", "2700|AAAAAAAA|20210101", "BBBB|BBBBBBBB|20210101", "CCCC|CCCCCCC|20210101" }, entryLine.SupportingDocumentsToCustoms.Select(x => x.CSI_Code + "|" + x.CSI_ReferenceNumber + "|" + x.CSI_DateOfIssue.ToString("yyyyMMdd")));
		}

		public void TestCalculateInvoicedDocumentaryAmount()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine11 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine11.JI_LinePrice = 1000m;

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 1000m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine21 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine21.JI_LinePrice = 1000m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				var entryLine2 = entry.MergedLines[0];

				AssertEquals(2, entryLine2.InvoiceLines.Count);
				AssertEquals(1438.85m, entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency);
				AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, entryLine2.CL_RX_NKInvoiceAmountCurrency);
				AssertEquals(2000m, entryLine2.CL_InvoiceAmount);

				invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				merger.DoMerge();

				AssertEquals(1209.62m, entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency);
				AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, entryLine2.CL_RX_NKInvoiceAmountCurrency);
				AssertEquals(1209.62m, entryLine2.CL_InvoiceAmount);
			});
		}

		[TestDate(2022, 4, 21)]
		public void TestCalculateInvoicedDocumentaryAmount_MultiCurrencyInEntryLineLevel()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine11 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine11.JI_LinePrice = 1000m;
			invoiceLine11.ZG_CountryOfDispatch = "US";

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 1000m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var invoiceLine21 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine21.JI_LinePrice = 1000m;
			invoiceLine21.ZG_CountryOfDispatch = "AU";

			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_InvoiceAmount = 1000m;
			invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var invoiceLine31 = invoiceHeader3.InvoiceLines.AddNew();
			invoiceLine31.JI_LinePrice = 1000m;
			invoiceLine31.ZG_CountryOfDispatch = "AU";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines.FirstOrDefault(x => x.PK == invoiceLine11.JI_CL);
			var entryLine2 = entry.MergedLines.FirstOrDefault(x => x.PK == invoiceLine21.JI_CL);

			CombineAssertions(() =>
			{
				AssertEquals("entryLine1.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency (EUR)", 719.42m, entryLine1.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency);
				AssertEquals("entryLine1.GetInvoicedDocumentaryAmountCurrency: single currency in entryline level though multi in entry level", Core.Constants.CurrencyCodes.UnitedStates, entryLine1.CL_RX_NKInvoiceAmountCurrency);
				AssertEquals("entryLine1.GetInvoicedDocumentaryAmount", 1000m, entryLine1.CL_InvoiceAmount);

				AssertEquals("entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency (EUR)", 1490.20m, entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInLocalCurrency);
				AssertEquals("entryLine2.GetInvoicedDocumentaryAmountCurrency: multi currency in entryline level", Core.Constants.CurrencyCodes.EuropeanUnion, entryLine2.CL_RX_NKInvoiceAmountCurrency);
				AssertEquals("entryLine2.GetInvoicedDocumentaryAmount", 1490.20m, entryLine2.CL_InvoiceAmount);
			});
		}

		public void TestCalculateInvoicedDocumentaryAmount_InvoiceCurrencyInEntryLineLevel()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 1000m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine11 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine11.JI_LinePrice = 1000m;
			invoiceLine11.ZG_CountryOfDispatch = "US";

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 500m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			var invoiceLine21 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine21.JI_LinePrice = 500m;
			invoiceLine21.ZG_CountryOfDispatch = "JP";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine1 = entry.MergedLines.FirstOrDefault(x => x.PK == invoiceLine11.JI_CL);
			var entryLine2 = entry.MergedLines.FirstOrDefault(x => x.PK == invoiceLine21.JI_CL);

			CombineAssertions(() =>
			{
				AssertEquals("entryLine1.CL_Calc_InvoicedDocumentaryAmountValueInInvoiceCurrency", 1000m, entryLine1.CL_Calc_InvoicedDocumentaryAmountValueInInvoiceCurrency);
				AssertEquals("entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInInvoiceCurrency", 500m, entryLine2.CL_Calc_InvoicedDocumentaryAmountValueInInvoiceCurrency);
			});
		}

		public void TestSupportingDocuments()
		{
			SetupForSupportingDocumentTests();

			AssertEquals(3, entryLine.SupportingDocuments.Count());
			AssertEquals(1, entryHeader.SupportingDocuments.Count());
			Assert(entryLine.SupportingDocuments.Any(x => x.CSI_Description == "A"));
			Assert(entryLine.SupportingDocuments.Any(x => x.CSI_Description == "B"));
			Assert(entryLine.SupportingDocuments.Any(x => x.CSI_Description == "C"));
			Assert(entryHeader.SupportingDocuments.Any(x => x.CSI_Description == "D"));
		}

		public void TestGetWarehouseType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_Procedure = "1071F61";

			var warehouseAddress = Factory.New<OrgAddress>();
			entryHeader.CH_CEI_Instruction = entryinstruction.PK;
			entryHeader.EntryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var auth = warehouseAddress.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("WHS 001");
			AssertEquals(WarehouseTypeList.Codes.Type_U, entryLine.GetWarehouseType());

			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertEquals(WarehouseTypeList.Codes.Type_R, entryLine.GetWarehouseType());

			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			AssertEquals(WarehouseTypeList.Codes.Type_S, entryLine.GetWarehouseType());
		}

		public override void TestDutyRateDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = entryHeader.AllEntryLines.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = cusEntryLine.PK;
			AssertEquals(ZDecimal.Zero, cusEntryLine.GSTRate);
			AssertEquals(ZString.Empty, cusEntryLine.DutyRateDescription);
			AssertEquals(ZString.Empty, invoiceLine.DutyAmountsAsString);
			var b00Fee = cusEntryLine.Fees.AddNew();
			b00Fee.CF_Rate = 17.5;
			b00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			var a30Fee = cusEntryLine.Fees.AddNew();
			a30Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			a30Fee.CF_ChargeAmount = 202m;
			a30Fee.CF_BaseValue = 404m;
			a30Fee.CF_Rate = 50;
			var a00Fee = cusEntryLine.Fees.AddNew();
			a00Fee.CF_ChargeAmount = 33m;
			a00Fee.CF_BaseValue = 100m;
			a00Fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			a00Fee.CF_Rate = 33;
			AssertEquals("GetGSTRate(): GST/VAT rates pulls only the B00 & B05 tax lines and looks at its Calculated Percentage", 17.5m, cusEntryLine.GSTRate);
			AssertEquals("GetDutyRateDescription(): Pulls all but VAT, sorted by code, formatted % per line", "A00:33%\r\nA30:50%", cusEntryLine.DutyRateDescription);
			AssertEquals("DutyAmountsAsString(Core): Pulls all but VAT, sorted by code, formatted £ per line", "A00:33.00\r\nA30:202.00", invoiceLine.DutyAmountsAsString);
		}

		public void TestComplementaryJobIST()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";
			var cusEntryNum = CusEntryNumber.New(ist2, CusEntryNumberTypes.France.DDT, Core.Constants.CountryCodes.France);
			cusEntryNum.CE_EntryNum = "DDT_SAMPLE";

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var pd = invoice.PreviousDocuments.AddNew();
			pd.CSI_Code = PreviousDocumentCodeList.Codes.IM;
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			var previousIST = entryLine.PreviousISTHeader;
			AssertNull("Cannot find previous IST because the previous document code is not IST", previousIST);

			pd.CSI_Code = PreviousDocumentCodeList.Codes.IST;
			previousIST = entryLine.PreviousISTHeader;
			AssertSame("Find previous IST from the previous IST job reference", ist1, previousIST);

			pd.CSI_ReferenceNumber = "DDT_SAMPLE";
			previousIST = entryLine.PreviousISTHeader;
			AssertSame("Find previous IST from the previous DDT number", ist2, previousIST);
		}

		public void TestSpecificRegimeGuaranteeAmount()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Description, euGrouping);
			Factory.Save();
			var euDtyRateType = helper.CreateCusRateType(euGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, euDtyRateType.PK);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing;
			var authHeader = Factory.New<CusAuthorisationHeader>();
			authHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			authHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authHeader.CPH_StartDate = ZDate.Today;
			authHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			var usage = instruction.CusAuthorizationUsages.AddNew();
			usage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			usage.AGC_Number = "";
			usage.AGC_OH_Owner = ZGuid.Empty;
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCD, "100");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCV, "5");
			CreateCusAuthorisationRule(authHeader, CusAuthorisationRuleTypeList.Codes.PCP, "15");
			var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault() ?? declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.FirstOrDefault() ?? entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 12); // Duty: 12.46m
			entryLine.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 29); // VAT: 29.12m
			entryLine.Fees.AddOrUpdate("A387", 43).NationalFeeTypeCode = "A387";  // ParaFiscal: 43.34m
			CombineAssertions("To make sure EntryLine fields return expected result.", () =>
			{
				AssertEquals(12m, entryLine.DutyAmount);
				AssertEquals(29m, entryLine.GSTVATAmount);
				AssertEquals(43m, entryLine.ParaFiscal);
			});
			AssertEquals("(DutyAmount 12 x 100%) + (GSTVATAmount 29 x 5%) + (ParaFiscal 43 x 15%)", 19.9m, entryLine.SpecificRegimeGuaranteeAmount);
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			newCurrency.SetCustomsRate(new ZDateTime(2005, 6, 1), new ZDateTime(2005, 6, 5), RatesAreReciprocal ? 2m : 0.5m);

			var declaration = ImportJobDeclaration;

			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			var line1OFT = line1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			line1OFT.J7_Amount = 10.0m;
			var line2ONS = line1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			var line2OFT = line1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight);
			line2OFT.J7_Amount = 20.0m;

			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("FOB", 690.0m, entryLine.FOBInLocalCurrency.Amount);
				AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
				AssertEquals("Overseas Freight", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
				AssertEquals("Overseas Insurance", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
				AssertEquals("T and I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
			});
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var declaration = base.ImportJobDeclaration;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				return declaration;
			}
		}
		protected override Type ExpectedTypeOfFees => typeof(EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		void SetupForSupportingDocumentTests()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("0001", isImport: true, hasPermitAttribute: false));

			var suppDoc1 = Factory.New<SupportingDocumentForTest>();
			suppDoc1.CSI_Code = "0001";
			suppDoc1.CSI_Description = "A";
			suppDoc1.CSI_IsDTP = false;

			var suppDoc2 = Factory.New<SupportingDocumentForTest>();
			suppDoc2.CSI_Code = "0001";
			suppDoc2.CSI_Description = "B";
			suppDoc2.CSI_IsDTP = true;

			var suppDoc3 = Factory.New<SupportingDocumentForTest>();
			suppDoc3.CSI_Code = "0001";
			suppDoc3.CSI_Description = "C";
			suppDoc3.CSI_IsDTP = true;

			var suppDoc4 = Factory.New<SupportingDocumentForTest>();
			suppDoc4.CSI_Code = "0001";
			suppDoc4.CSI_Description = "D";
			suppDoc4.CSI_IsDTP = false;

			var declaration = Factory.New<JobDeclaration>();
			declaration.SupportingDocuments.Add(suppDoc4);

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.SupportingDocuments.Add(suppDoc1);
			invoiceLine.SupportingDocuments.Add(suppDoc2);
			invoiceLine.SupportingDocuments.Add(suppDoc3);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Add(invoiceLine);
		}

		public void TestPreviousDocuments_UCC6()
		{
			var entryLine = (CusEntryLine)GetNewBusinessObject();
			entryLine.Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			entryLine.Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var invLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];

			var invLinePrevDoc1 = invLine.PreviousDocuments.AddNew();
			invLinePrevDoc1.CSI_Code = "111";
			invLinePrevDoc1.CSI_ReferenceNumber = "111";

			var invLinePrevDoc2 = invLine.PreviousDocuments.AddNew();
			invLinePrevDoc2.CSI_Code = "222";
			invLinePrevDoc2.CSI_ReferenceNumber = "222";

			var invHeaderPrevDoc = invLine.InvoiceHeader.PreviousDocuments.AddNew();
			invHeaderPrevDoc.CSI_Code = "333";
			invHeaderPrevDoc.CSI_ReferenceNumber = "333";

			var declarationPrevDoc = invLine.Declaration.PreviousDocuments.AddNew();
			declarationPrevDoc.CSI_Code = "444";
			declarationPrevDoc.CSI_ReferenceNumber = "444";

			// In case of UCC6 , PreviousDocuments in the message should be taken from PreviousDocuments at invoice line level only
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, entryLine.PreviousDocuments.Cast<PreviousDocument>().Select(x => x.CSI_Code));
		}

		public void TestPreviousDocuments_NotUCC6()
		{
			var entryLine = (CusEntryLine)GetNewBusinessObject();

			var invLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];

			var invLinePrevDoc1 = invLine.PreviousDocuments.AddNew();
			invLinePrevDoc1.CSI_Code = "111";
			invLinePrevDoc1.CSI_ReferenceNumber = "111";

			var invLinePrevDoc2 = invLine.PreviousDocuments.AddNew();
			invLinePrevDoc2.CSI_Code = "222";
			invLinePrevDoc2.CSI_ReferenceNumber = "222";

			var invHeaderPrevDoc = invLine.InvoiceHeader.PreviousDocuments.AddNew();
			invHeaderPrevDoc.CSI_Code = "333";
			invHeaderPrevDoc.CSI_ReferenceNumber = "333";

			var declarationPrevDoc = invLine.Declaration.PreviousDocuments.AddNew();
			declarationPrevDoc.CSI_Code = "444";
			declarationPrevDoc.CSI_ReferenceNumber = "444";

			// In case it's not UCC6 , PreviousDocuments in the message should be taken from PreviousDocuments at declaration, invoice header and invoice line levels
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222", "333", "444" }, entryLine.PreviousDocuments.Cast<PreviousDocument>().Select(x => x.CSI_Code));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var line = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = line.PK;
			return line;
		}

		CusEntryLine entryLine;
		CusEntryHeader entryHeader;
	}

	public class SupportingDocumentForTest : SupportingDocument
	{
		public SupportingDocumentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZZRefCusCodeListCombined GetRefCusCodeCore()
		{
			var refCusCode = Factory.New<ZZRefCusCodeListCombined>();
			refCusCode.ZZD_CodeType = "DOC44";
			var attribute = refCusCode.Attributes.AddNew();
			attribute.ZZE_ZXE_NKName = "LEVEL";
			attribute.ZZE_Value = "ITEM";

			return refCusCode;
		}
	}
}
