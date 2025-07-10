using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR934DataProviderTest : TestCaseWithFactory
	{
		public void TestHeader()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			entry.Declaration.Invoices[0].JZ_ProvAdditionalAmount = 13000000000.99m;
			entry.MergedLines[0].CL_CustomsValue = 2138883648m;
			var result = new Import934HeaderCreator().Create(entry);

			#region IImport934Header
			AssertEquals(2147483647.99m, result.TotalCustomsValueKRW);
			AssertEquals(13000000000.99m, result.ProvisionalAdditionalAmount);
			result.RoundDecimalValueRoundedWithDecimalPlaces();

			AssertEquals("1235621434585M", result.ImportDeclarationNumber);
			AssertEquals("899999999", result.InvoiceNo);
			AssertEquals(new ZDateTime(2022, 07, 06), result.InvoiceDate);
			AssertEquals("Contract No. 347582", result.ContractNo);
			AssertEquals(new ZDateTime(2022, 01, 01), result.ContractDate);
			AssertEquals(2147483648m, result.TotalCustomsValueKRW);
			AssertEquals(false, result.ProvisionalPricingYN);
			AssertEquals(10.3m, result.ProvisionalAdditionRate);
			AssertEquals(new ZDateTime(2022, 02, 02), result.EstimatedDateOfFinalPrice);
			AssertEquals(new ZDateTime(2022, 12, 31), result.ContractExpirationDate);

			AssertEquals("101", result.ProvisionalPricingReasons[0].Code);
			AssertEquals("Y", result.ProvisionalPricingReasons[0].CodeOtherDescription);
			AssertEquals("119", result.ProvisionalPricingReasons[1].Code);
			AssertEquals("기타비용", result.ProvisionalPricingReasons[1].CodeOtherDescription);
			AssertEquals(13000000001m, result.ProvisionalAdditionalAmount);
			#endregion

			#region IImport934_5SMHeader
			AssertEquals(Constants.ValuationMethod.A, result.ValuationMethod);
			AssertEquals("130", result.DeclarationCustomsOffice);
			AssertEquals("10", result.DeclarationCustomsDivision);
			AssertNotNull(result.Payer);
			AssertNull(result.Supplier);
			AssertNull(result.Importer);

			AssertEquals("Purchase No. 123456", result.PurchaseOrderNo);
			AssertEquals(new ZDateTime(2022, 07, 07), result.PurchaseOrderDate);

			AssertEquals("Manager", result.Author.DepartmentAndPosition);
			AssertEquals("Staff 1", result.Author.Name);
			AssertEquals("130", result.Author.TelephoneNumber);

			AssertEquals("Leader", result.ResponsiblePerson.DepartmentAndPosition);
			AssertEquals("Staff 2", result.ResponsiblePerson.Name);
			AssertEquals("131", result.ResponsiblePerson.TelephoneNumber);
			#endregion

			result = new Import934HeaderCreator().Create(entry);
			AssertEquals("Manager", result.Author.DepartmentAndPosition);
			AssertEquals("Staff 1", result.Author.Name);
			AssertEquals("130", result.Author.TelephoneNumber);

			result = new Import934HeaderCreator().Create(entry);
			AssertEquals("Leader", result.ResponsiblePerson.DepartmentAndPosition);
			AssertEquals("Staff 2", result.ResponsiblePerson.Name);
			AssertEquals("131", result.ResponsiblePerson.TelephoneNumber);
		}

		public void TestEmptyValuationDeclarationCode()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices[0];
			invoice.ValuationDeclarationCodes.RemoveAll();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;

			var valuationDecCode_UseCode1 = invoice.ValuationDeclarationCodes.AddNew();
			valuationDecCode_UseCode1.CY_Code = ZString.Empty;
			valuationDecCode_UseCode1.CY_Data = ZString.Empty;

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(0, result.FormBData.UseCodes.Length);
		}

		public void TestValuationDeclarationCode()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices[0];
			invoice.ValuationDeclarationCodes.RemoveAll();
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;

			invoice.ValuationDeclarationCode301 = true;
			invoice.ValuationDeclarationCode302 = true;
			invoice.ValuationDeclarationCode303 = true;
			invoice.ValuationDeclarationCode401 = true;
			invoice.ValuationDeclarationCode402 = false;
			invoice.ProvisionalPricingReason119 = "TEST6";

			var result = new Import934HeaderCreator().Create(entry);

			AssertEquals(3, result.FormBData.UseCodes.Length);
			AssertEquals("301", result.FormBData.UseCodes[0].Code);
			AssertEquals("Y", result.FormBData.UseCodes[0].CodeOtherDescription);
			AssertEquals("302", result.FormBData.UseCodes[1].Code);
			AssertEquals("Y", result.FormBData.UseCodes[1].CodeOtherDescription);
			AssertEquals("303", result.FormBData.UseCodes[2].Code);
			AssertEquals("Y", result.FormBData.UseCodes[2].CodeOtherDescription);

			AssertEquals(2, result.FormBData.GoodsPricingBasis.Length);
			AssertEquals("401", result.FormBData.GoodsPricingBasis[0].Code);
			AssertEquals("Y", result.FormBData.GoodsPricingBasis[0].CodeOtherDescription);
			AssertEquals("402", result.FormBData.GoodsPricingBasis[1].Code);
			AssertEquals("", result.FormBData.GoodsPricingBasis[1].CodeOtherDescription);
			AssertEquals(1, result.ProvisionalPricingReasons.Length);
			AssertEquals("119", result.ProvisionalPricingReasons[0].Code);
			AssertEquals("TEST6", result.ProvisionalPricingReasons[0].CodeOtherDescription);
		}

		public void TestValuationSupportingDocuments()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			var invoice = declaration.Invoices[0];
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;

			invoice.ValuationSupportingDocument1 = "11";
			invoice.ValuationSupportingDocument2 = "22";

			var result = new Import934HeaderCreator().Create(entry);

			AssertEquals("11", result.FormBData.ValuationSupportingDocument1);
			AssertEquals("22", result.FormBData.ValuationSupportingDocument2);
		}

		public void TestFormAData()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 0.5m);
			CreateCurrency(Core.Constants.CurrencyCodes.Japan, 0.3m);
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices[0];
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			invoice.ValuationQuestion5A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion5B = "01";
			invoice.ValuationQuestion5C = YesNoList.Codes.Yes;
			invoice.ValuationQuestion5D = YesNoList.Codes.Yes;
			invoice.ValuationQuestion5EA = "99";
			invoice.ValuationQuestion5EB = "기타법률";
			invoice.ValuationQuestion6A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion6B = YesNoList.Codes.Yes;
			invoice.ValuationQuestion7A_IMP = Constants.YesNo.Yes;
			invoice.ValuationQuestion7B_IMP = SpecialRelationshipCodeList.Codes._01;
			invoice.ValuationQuestion7C = Constants.YesNo.No;
			invoice.ValuationQuestion7D = Constants.YesNo.Yes;
			invoice.ValuationQuestion7EA = PricingCodeList.Codes._07;
			invoice.ValuationQuestion7EB = "기타";
			invoice.ValuationQuestion8A = Constants.YesNo.Yes;
			invoice.ValuationQuestion8B = Constants.YesNo.No;
			invoice.ValuationQuestion9A = Constants.YesNo.Yes;
			invoice.ValuationQuestion9B = Constants.YesNo.No;

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A102, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A102, 10m, true, true);

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A104, 10m, true, false);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A104, 20m, false, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A104, 30m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A104, 40m, false, false);

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A105, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A105, 10m, true, true);

			var charge106_1 = CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A106, 10m, true, true);
			charge106_1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var charge106_2 = CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A106, 10m, true, true);
			charge106_2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A107, 10m, true, true);
			CreateGroupCharge(invoice, ImportChargeMethodOneCodeList.Codes.A107, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A108, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A109, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A110, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A111, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A112, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A114, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A115, 10m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A116, 10m, true, true);

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A118, 10m, true, false);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A118, 20m, false, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A118, 30m, true, true);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A118, 40m, false, false);

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A119, 10m, false, false);
			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A119, 10m, false, false);

			var charge120_1 = CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A120, 10m, false, false);
			charge120_1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var charge120_2 = CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A120, 10m, false, false);
			charge120_2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;

			CreateCharge(invoice, ImportChargeMethodOneCodeList.Codes.A121, 10m, false, false);
			CreateGroupCharge(invoice, ImportChargeMethodOneCodeList.Codes.A121, false, false);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000m;

			var result = new Import934HeaderCreator().Create(entry);

			AssertNotNull(result.FormAData);

			AssertEquals(17, result.FormAData.Questions.Length);
			AssertEquals("5A", result.FormAData.Questions[0].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[0].AnswerCode);
			AssertEquals("5B", result.FormAData.Questions[1].QuestionCode);
			AssertEquals("01", result.FormAData.Questions[1].AnswerCode);
			AssertEquals("5C", result.FormAData.Questions[2].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[2].AnswerCode);
			AssertEquals("5D", result.FormAData.Questions[3].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[3].AnswerCode);
			AssertEquals("5EA", result.FormAData.Questions[4].QuestionCode);
			AssertEquals("99", result.FormAData.Questions[4].AnswerCode);
			AssertEquals("5EB", result.FormAData.Questions[5].QuestionCode);
			AssertEquals("기타법률", result.FormAData.Questions[5].AnswerCode);
			AssertEquals("6A", result.FormAData.Questions[6].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[6].AnswerCode);
			AssertEquals("6B", result.FormAData.Questions[7].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[7].AnswerCode);
			AssertEquals("7A", result.FormAData.Questions[8].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[8].AnswerCode);
			AssertEquals("7B", result.FormAData.Questions[9].QuestionCode);
			AssertEquals("01", result.FormAData.Questions[9].AnswerCode);
			AssertEquals("7C", result.FormAData.Questions[10].QuestionCode);
			AssertEquals(Constants.YesNo.No, result.FormAData.Questions[10].AnswerCode);
			AssertEquals("7D", result.FormAData.Questions[11].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[11].AnswerCode);
			AssertEquals("7E", result.FormAData.Questions[12].QuestionCode);
			AssertEquals("07", result.FormAData.Questions[12].AnswerCode);
			AssertEquals("기타", result.FormAData.Questions[12].AnswerOtherDescription);
			AssertEquals("8A", result.FormAData.Questions[13].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[13].AnswerCode);
			AssertEquals("8B", result.FormAData.Questions[14].QuestionCode);
			AssertEquals(Constants.YesNo.No, result.FormAData.Questions[14].AnswerCode);
			AssertEquals("9A", result.FormAData.Questions[15].QuestionCode);
			AssertEquals(Constants.YesNo.Yes, result.FormAData.Questions[15].AnswerCode);
			AssertEquals("9B", result.FormAData.Questions[16].QuestionCode);
			AssertEquals(Constants.YesNo.No, result.FormAData.Questions[16].AnswerCode);

			AssertEquals(1000m, result.FormAData.Method1ValuationData.BaseAmount);
			AssertEquals("USD", result.FormAData.Method1ValuationData.BaseAmountCurrency);
			AssertEquals(0.5m, result.FormAData.Method1ValuationData.ExchangeRate);
			AssertEquals(500m, result.FormAData.Method1ValuationData.AmountInKRW);
			AssertEquals(10m, result.FormAData.Method1ValuationData.IndirectPaymentAmount);

			AssertEquals(4, result.FormAData.Method1ValuationData.Deductions.Length);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A118, result.FormAData.Method1ValuationData.Deductions[0].Type);
			AssertEquals(20m, result.FormAData.Method1ValuationData.Deductions[0].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A119, result.FormAData.Method1ValuationData.Deductions[1].Type);
			AssertEquals(10m, result.FormAData.Method1ValuationData.Deductions[1].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A120, result.FormAData.Method1ValuationData.Deductions[2].Type);
			AssertEquals(8m, result.FormAData.Method1ValuationData.Deductions[2].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A121, result.FormAData.Method1ValuationData.Deductions[3].Type);
			AssertEquals(15m, result.FormAData.Method1ValuationData.Deductions[3].Amount);

			AssertEquals(10, result.FormAData.Method1ValuationData.Additions.Length);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A104, result.FormAData.Method1ValuationData.Additions[0].Type);
			AssertEquals(15m, result.FormAData.Method1ValuationData.Additions[0].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A105, result.FormAData.Method1ValuationData.Additions[1].Type);
			AssertEquals(10m, result.FormAData.Method1ValuationData.Additions[1].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A106, result.FormAData.Method1ValuationData.Additions[2].Type);
			AssertEquals(8m, result.FormAData.Method1ValuationData.Additions[2].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A107, result.FormAData.Method1ValuationData.Additions[3].Type);
			AssertEquals(15m, result.FormAData.Method1ValuationData.Additions[3].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A108, result.FormAData.Method1ValuationData.Additions[4].Type);
			AssertEquals(5m, result.FormAData.Method1ValuationData.Additions[4].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A109, result.FormAData.Method1ValuationData.Additions[5].Type);
			AssertEquals(5m, result.FormAData.Method1ValuationData.Additions[5].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A110, result.FormAData.Method1ValuationData.Additions[6].Type);
			AssertEquals(5m, result.FormAData.Method1ValuationData.Additions[6].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A111, result.FormAData.Method1ValuationData.Additions[7].Type);
			AssertEquals(5m, result.FormAData.Method1ValuationData.Additions[7].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A112, result.FormAData.Method1ValuationData.Additions[8].Type);
			AssertEquals(5m, result.FormAData.Method1ValuationData.Additions[8].Amount);
			AssertEquals(PriceDutyTaxFeeTypeCode.Codes.A115, result.FormAData.Method1ValuationData.Additions[9].Type);
			AssertEquals(5m, result.FormAData.Method1ValuationData.Additions[9].Amount);
		}

		public void TestFormBDataWhenValuationMethodIs20()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 0.5m);
			Factory.Save();

			var entry = SetTestDataAboutFormBData(ValuationCodeList.Codes.MethodTwo);
			var invoice = entry.RandomHeader;
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B303, 10m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B304, 20m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B305, 30m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B306, 40m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B307, 50m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B309, 60m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B310, 70m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B311, 80m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B312, 90m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B313, 100m);

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(1500m, result.FormBData.ExpectedCustomsValue);
			AssertEquals(ValuationCodeList.Codes.MethodTwo, result.FormBData.ValuationMethod);
			AssertEquals(1000m, result.FormBData.Method2_3ValuationData.BaseAmount);
			AssertEquals("KRW", result.FormBData.Method2_3ValuationData.BaseAmountCurrency);
			AssertEquals(0.5m, result.FormBData.Method2_3ValuationData.ExchangeRate);
			AssertEquals(500m, result.FormBData.Method2_3ValuationData.AmountInKRW);

			AssertEquals("B303", result.FormBData.Method2_3ValuationData.Deductions[0].Type);
			AssertEquals(15m, result.FormBData.Method2_3ValuationData.Deductions[0].Amount);
			AssertEquals("B304", result.FormBData.Method2_3ValuationData.Deductions[1].Type);
			AssertEquals(20m, result.FormBData.Method2_3ValuationData.Deductions[1].Amount);
			AssertEquals("B305", result.FormBData.Method2_3ValuationData.Deductions[2].Type);
			AssertEquals(25m, result.FormBData.Method2_3ValuationData.Deductions[2].Amount);
			AssertEquals("B306", result.FormBData.Method2_3ValuationData.Deductions[3].Type);
			AssertEquals(30m, result.FormBData.Method2_3ValuationData.Deductions[3].Amount);
			AssertEquals("B307", result.FormBData.Method2_3ValuationData.Deductions[4].Type);
			AssertEquals(35m, result.FormBData.Method2_3ValuationData.Deductions[4].Amount);

			AssertEquals("B309", result.FormBData.Method2_3ValuationData.Additions[0].Type);
			AssertEquals(40m, result.FormBData.Method2_3ValuationData.Additions[0].Amount);
			AssertEquals("B310", result.FormBData.Method2_3ValuationData.Additions[1].Type);
			AssertEquals(45m, result.FormBData.Method2_3ValuationData.Additions[1].Amount);
			AssertEquals("B312", result.FormBData.Method2_3ValuationData.Additions[2].Type);
			AssertEquals(55m, result.FormBData.Method2_3ValuationData.Additions[2].Amount);
		}

		public void TestFormBDataWhenValuationMethodIs30()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 0.6m);
			Factory.Save();

			var entry = SetTestDataAboutFormBData(ValuationCodeList.Codes.MethodThree);
			var invoice = entry.RandomHeader;
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B303, 10m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B304, 20m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B305, 30m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B306, 40m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B307, 50m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B309, 60m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B310, 70m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B311, 80m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B312, 90m);
			CreateCharges(invoice, ImportChargeMethodTwoAndThreeCodeList.Codes.B313, 100m);

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(1500m, result.FormBData.ExpectedCustomsValue);
			AssertEquals(ValuationCodeList.Codes.MethodThree, result.FormBData.ValuationMethod);
			AssertEquals(1000m, result.FormBData.Method2_3ValuationData.BaseAmount);
			AssertEquals("KRW", result.FormBData.Method2_3ValuationData.BaseAmountCurrency);
			AssertEquals(0.5m, result.FormBData.Method2_3ValuationData.ExchangeRate);
			AssertEquals(600m, result.FormBData.Method2_3ValuationData.AmountInKRW);

			AssertEquals("B303", result.FormBData.Method2_3ValuationData.Deductions[0].Type);
			AssertEquals(18m, result.FormBData.Method2_3ValuationData.Deductions[0].Amount);
			AssertEquals("B304", result.FormBData.Method2_3ValuationData.Deductions[1].Type);
			AssertEquals(24m, result.FormBData.Method2_3ValuationData.Deductions[1].Amount);
			AssertEquals("B305", result.FormBData.Method2_3ValuationData.Deductions[2].Type);
			AssertEquals(30m, result.FormBData.Method2_3ValuationData.Deductions[2].Amount);
			AssertEquals("B306", result.FormBData.Method2_3ValuationData.Deductions[3].Type);
			AssertEquals(36m, result.FormBData.Method2_3ValuationData.Deductions[3].Amount);
			AssertEquals("B307", result.FormBData.Method2_3ValuationData.Deductions[4].Type);
			AssertEquals(42m, result.FormBData.Method2_3ValuationData.Deductions[4].Amount);

			AssertEquals("B309", result.FormBData.Method2_3ValuationData.Additions[0].Type);
			AssertEquals(48m, result.FormBData.Method2_3ValuationData.Additions[0].Amount);
			AssertEquals("B310", result.FormBData.Method2_3ValuationData.Additions[1].Type);
			AssertEquals(54m, result.FormBData.Method2_3ValuationData.Additions[1].Amount);
			AssertEquals("B312", result.FormBData.Method2_3ValuationData.Additions[2].Type);
			AssertEquals(66m, result.FormBData.Method2_3ValuationData.Additions[2].Amount);
		}

		public void TestFormBDataWhenValuationMethodIs4A()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 1m);
			Factory.Save();

			var entry = SetTestDataAboutFormBData(ValuationCodeList.Codes.MethodFourA);
			var invoice = entry.RandomHeader;
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B404, 10m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B405, 20m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B406, 30m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B407, 40m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B408, 50m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B409, 60m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B410, 70m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B411, 80m);

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(1500m, result.FormBData.ExpectedCustomsValue);
			AssertEquals(ValuationCodeList.Codes.MethodFourA, result.FormBData.ValuationMethod);
			AssertEquals(1000m, result.FormBData.Method4ValuationData.BaseAmount);
			AssertEquals("USD", result.FormBData.Method4ValuationData.BaseAmountCurrency);
			AssertEquals(0.5m, result.FormBData.Method4ValuationData.ExchangeRate);
			AssertEquals(1000m, result.FormBData.Method4ValuationData.AmountInKRW);
			AssertEquals("공제방법에 대한 세관 참조번호", result.FormBData.Method4ValuationData.DeductionCustomsReferenceNo);
			AssertEquals(1200.12m, result.FormBData.Method4ValuationData.DeductionGeneralCostPercentage);
			AssertEquals("1", result.FormBData.Method4ValuationData.DeductionGeneralCostPercentageType);

			AssertEquals("B404", result.FormBData.Method4ValuationData.Deductions[0].Type);
			AssertEquals(30m, result.FormBData.Method4ValuationData.Deductions[0].Amount);
			AssertEquals("B405", result.FormBData.Method4ValuationData.Deductions[1].Type);
			AssertEquals(40m, result.FormBData.Method4ValuationData.Deductions[1].Amount);
			AssertEquals("B406", result.FormBData.Method4ValuationData.Deductions[2].Type);
			AssertEquals(50m, result.FormBData.Method4ValuationData.Deductions[2].Amount);
			AssertEquals("B407", result.FormBData.Method4ValuationData.Deductions[3].Type);
			AssertEquals(60m, result.FormBData.Method4ValuationData.Deductions[3].Amount);
			AssertEquals("B408", result.FormBData.Method4ValuationData.Deductions[4].Type);
			AssertEquals(70m, result.FormBData.Method4ValuationData.Deductions[4].Amount);
			AssertEquals("B409", result.FormBData.Method4ValuationData.Deductions[5].Type);
			AssertEquals(80m, result.FormBData.Method4ValuationData.Deductions[5].Amount);
			AssertEquals("B410", result.FormBData.Method4ValuationData.Deductions[6].Type);
			AssertEquals(90m, result.FormBData.Method4ValuationData.Deductions[6].Amount);
			AssertEquals("B411", result.FormBData.Method4ValuationData.Deductions[7].Type);
			AssertEquals(100m, result.FormBData.Method4ValuationData.Deductions[7].Amount);
		}

		public void TestFormBDataWhenValuationMethodIs4B()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 2m);
			Factory.Save();

			var entry = SetTestDataAboutFormBData(ValuationCodeList.Codes.MethodFourB);
			var invoice = entry.RandomHeader;
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B404, 10m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B405, 20m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B406, 30m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B407, 40m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B408, 50m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B409, 60m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B410, 70m);
			CreateCharges(invoice, ImportChargeMethodFourCodeList.Codes.B411, 80m);

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(1500m, result.FormBData.ExpectedCustomsValue);
			AssertEquals(ValuationCodeList.Codes.MethodFourB, result.FormBData.ValuationMethod);
			AssertEquals(1000m, result.FormBData.Method4ValuationData.BaseAmount);
			AssertEquals("USD", result.FormBData.Method4ValuationData.BaseAmountCurrency);
			AssertEquals(0.5m, result.FormBData.Method4ValuationData.ExchangeRate);
			AssertEquals(2000m, result.FormBData.Method4ValuationData.AmountInKRW);
			AssertEquals("공제방법에 대한 세관 참조번호", result.FormBData.Method4ValuationData.DeductionCustomsReferenceNo);
			AssertEquals(1200.12m, result.FormBData.Method4ValuationData.DeductionGeneralCostPercentage);
			AssertEquals("1", result.FormBData.Method4ValuationData.DeductionGeneralCostPercentageType);

			AssertEquals("B404", result.FormBData.Method4ValuationData.Deductions[0].Type);
			AssertEquals(60m, result.FormBData.Method4ValuationData.Deductions[0].Amount);
			AssertEquals("B405", result.FormBData.Method4ValuationData.Deductions[1].Type);
			AssertEquals(80m, result.FormBData.Method4ValuationData.Deductions[1].Amount);
			AssertEquals("B406", result.FormBData.Method4ValuationData.Deductions[2].Type);
			AssertEquals(100m, result.FormBData.Method4ValuationData.Deductions[2].Amount);
			AssertEquals("B407", result.FormBData.Method4ValuationData.Deductions[3].Type);
			AssertEquals(120m, result.FormBData.Method4ValuationData.Deductions[3].Amount);
			AssertEquals("B408", result.FormBData.Method4ValuationData.Deductions[4].Type);
			AssertEquals(140m, result.FormBData.Method4ValuationData.Deductions[4].Amount);
			AssertEquals("B409", result.FormBData.Method4ValuationData.Deductions[5].Type);
			AssertEquals(160m, result.FormBData.Method4ValuationData.Deductions[5].Amount);
			AssertEquals("B410", result.FormBData.Method4ValuationData.Deductions[6].Type);
			AssertEquals(180m, result.FormBData.Method4ValuationData.Deductions[6].Amount);
			AssertEquals("B411", result.FormBData.Method4ValuationData.Deductions[7].Type);
			AssertEquals(200m, result.FormBData.Method4ValuationData.Deductions[7].Amount);
		}

		public void TestFormBDataWhenValuationMethodIs50()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 0.1m);
			Factory.Save();

			var entry = SetTestDataAboutFormBData(ValuationCodeList.Codes.MethodFive);
			var invoice = entry.RandomHeader;
			CreateCharges(invoice, ImportChargeMethodFiveAndSixCodeList.Codes.B502, 20m);

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(1500m, result.FormBData.ExpectedCustomsValue);
			AssertEquals(ValuationCodeList.Codes.MethodFive, result.FormBData.ValuationMethod);
			AssertEquals(1000m, result.FormBData.Method5_6ValuationData.BaseAmount);

			AssertEquals("B502", result.FormBData.Method5_6ValuationData.Additions[0].Type);
			AssertEquals(4m, result.FormBData.Method5_6ValuationData.Additions[0].Amount);
		}

		public void TestFormBDataWhenValuationMethodIs60()
		{
			CreateCurrency(Core.Constants.CurrencyCodes.UnitedStates, 0.2m);
			Factory.Save();

			var entry = SetTestDataAboutFormBData(ValuationCodeList.Codes.MethodSix);
			var invoice = entry.RandomHeader;
			CreateCharges(invoice, ImportChargeMethodFiveAndSixCodeList.Codes.B502, 20m);

			var result = new Import934HeaderCreator().Create(entry);
			AssertEquals(1500m, result.FormBData.ExpectedCustomsValue);
			AssertEquals(ValuationCodeList.Codes.MethodSix, result.FormBData.ValuationMethod);
			AssertEquals(1000m, result.FormBData.Method5_6ValuationData.BaseAmount);

			AssertEquals("B502", result.FormBData.Method5_6ValuationData.Additions[0].Type);
			AssertEquals(8m, result.FormBData.Method5_6ValuationData.Additions[0].Amount);
		}

		public void TestImporterAndSupplier()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			var import934 = new Import934HeaderCreator().Create(entry);
			AssertNull(import934.Importer);
			AssertNull(import934.Supplier);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;

			import934 = new Import934HeaderCreator().Create(entry);
			AssertNull(import934.Importer);
			AssertNull(import934.Supplier);
		}

		CusEntryHeader SetTestDataAboutFormBData(ZString valuationCode)
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 500;
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 1000;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_InvoiceCurrExRate = 0.5m;
			invoice.JZ_ValuationCode = valuationCode;
			invoice.JZ_DeductionType = "1";
			invoice.JZ_DeductionRate = 1200.12;
			var invoiceRefs = invoice.InvoiceHeaderRefs.AddNew();
			invoiceRefs.J2_ReferenceNumber = "공제방법에 대한 세관 참조번호";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			return entry;
		}

		void CreateCurrency(ZString currencyCode, ZDecimal sellRate)
		{
			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);

			var rate = Factory.New<RefExchangeRate>();
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			rate.RE_RX_NKExCurrency = currency.RX_Code;
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_SellRate = sellRate;
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
		}

		InvoiceCharge CreateCharge(JobComInvoiceHeader invoice, ZString type, ZDecimal amount, ZBool isNotIncluded, ZBool isDutiable)
		{
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = type;
			charge.J7_Amount = amount;
			charge.J7_IsNotIncludedInInvoice = isNotIncluded;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			return charge;
		}

		BaseApportionedCharge CreateGroupCharge(JobComInvoiceHeader invoice, ZString type, ZBool isNotIncluded, ZBool isDutiable)
		{
			var charge = invoice.GroupCharges.AddNew();
			charge.J7_ChargeType = type;
			charge.J7_Amount = 20m;
			charge.J7_IsNotIncludedInInvoice = isNotIncluded;
			charge.J7_IsDutiable = isDutiable;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			return charge;
		}

		void CreateCharges(JobComInvoiceHeader invoice, ZString code, ZDecimal amount)
		{
			CreateCharge(invoice, code, amount, false, false);
			CreateCharge(invoice, code, amount, true, false);
			CreateCharge(invoice, code, amount, false, true);
			CreateCharge(invoice, code, amount, true, true);

			CreateGroupCharge(invoice, code, false, false);
			CreateGroupCharge(invoice, code, true, false);
			CreateGroupCharge(invoice, code, false, true);
			CreateGroupCharge(invoice, code, true, true);
		}
	}
}
