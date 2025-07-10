using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SMDataProviderTest : XMLMessageTestHelper<GOVCBR5SMDataProviderTest>
	{
		[TestDate(2021, 1, 1)]
		public void TestHeaderWithFormC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "899999999", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Tariff description");

			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			entry.EntryNumber = "88888211001U";
			entry.Declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "88888211002U";
			entryNum.CE_EntryLineReference = "1";
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SM;
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;

			var invoice = entry.Declaration.Invoices[0];

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "DESCRIPTION";
			invoiceLine.JI_BrandName = "BRAND";
			invoiceLine.JI_Model = "MODEL";
			invoiceLine.JI_Ingredient = "INGREDIENT";
			invoiceLine.JI_Tariff = "899999999";
			invoiceLine.JI_CL = entryLine.PK;

			invoice.ValuationQuestions.RemoveAndDeleteAll();
			#region questions
			var valuationQuestions = invoice.ValuationQuestions;

			invoice.ValuationQuestion5A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion5B = "01";
			invoice.ValuationQuestion5EA = "99";
			invoice.ValuationQuestion5EB = "기타법률";
			invoice.ValuationQuestion6A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion6B = YesNoList.Codes.Yes;
			invoice.ValuationQuestion7A_5SM = YesNoList.Codes.Yes;
			invoice.ValuationQuestion7B_5SM = YesNoList.Codes.Yes;

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			invoice.ValuationQuestion8A = YesNoList.Codes.No;
			invoice.ValuationQuestion8B = YesNoList.Codes.Yes;
			invoice.ValuationQuestion8C = YesNoList.Codes.Yes;
			invoice.ValuationQuestion8D = YesNoList.Codes.Yes;

			invoice.ValuationQuestion9A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion9B = YesNoList.Codes.Yes;

			invoice.ValuationQuestion10A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion10B = YesNoList.Codes.Yes;
			invoice.ValuationQuestion10C = YesNoList.Codes.Yes;
			invoice.ValuationQuestion10D = YesNoList.Codes.No;

			invoice.ValuationQuestion11A = YesNoList.Codes.Yes;
			invoice.ValuationQuestion11B = YesNoList.Codes.Yes;
			invoice.ValuationQuestion11C = YesNoList.Codes.Yes;
			invoice.ValuationQuestion11D = YesNoList.Codes.No;
			#endregion
			Factory.Save();

			var import5SM = new Import5SMHeaderCreator().Create(entry);

			AssertEquals("88888211002U", import5SM.ValueDeclarationTemplateNumber);
			AssertEquals("Purchase No. 123456", import5SM.PurchaseOrderNo);
			AssertEquals(new ZDate(2022, 7, 7), import5SM.PurchaseOrderDate);
			AssertEquals("A", import5SM.ValuationMethod);
			AssertEquals("10", import5SM.DeclarationCustomsDivision);
			AssertEquals("130", import5SM.DeclarationCustomsOffice);
			AssertNotNull(import5SM.Payer);
			AssertNotNull(import5SM.Supplier);
			AssertNotNull(import5SM.Importer);

			#region FormCData
			AssertEquals(23, import5SM.FormCData.Questions.Length);
			var questions = import5SM.FormCData.Questions.ToArray();

			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5A).AnswerCode);
			AssertEquals("01", questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5B).AnswerCode);
			AssertEquals(YesNoList.Codes.No, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5C).AnswerCode);
			AssertEquals(YesNoList.Codes.No, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5D).AnswerCode);
			AssertEquals("99", questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5E).AnswerCode);
			AssertEquals("기타법률", questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._5E).AnswerOtherDescription);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._6A).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._6B).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._7A).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._7B).AnswerCode);
			AssertEquals(YesNoList.Codes.No, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._8A).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._8B).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._8C).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._8D).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._9A).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._9B).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._10A).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._10B).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._10C).AnswerCode);
			AssertEquals(YesNoList.Codes.No, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._10D).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._11A).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._11B).AnswerCode);
			AssertEquals(YesNoList.Codes.Yes, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._11C).AnswerCode);
			AssertEquals(YesNoList.Codes.No, questions.First(x => x.QuestionCode == PriceQuestionCodeList.Codes._11D).AnswerCode);

			#endregion

			AssertEquals("Manager", import5SM.Author.DepartmentAndPosition);
			AssertEquals("Staff 1", import5SM.Author.Name);
			AssertEquals("130", import5SM.Author.TelephoneNumber);
			AssertEquals("Leader", import5SM.ResponsiblePerson.DepartmentAndPosition);
			AssertEquals("Staff 2", import5SM.ResponsiblePerson.Name);
			AssertEquals("131", import5SM.ResponsiblePerson.TelephoneNumber);

			AssertEquals(3, import5SM.EntryLines.Length);
			var line5SM = import5SM.EntryLines.ToArray()[2];
			AssertEquals(3, line5SM.EntryLineNo);
			AssertEquals("Tariff description", line5SM.HSDescription);
			AssertEquals("899999999", line5SM.HSCode);
			AssertEquals("DESCRIPTION", line5SM.InvoiceDescription);
			AssertEquals("BRAND", line5SM.BrandName);
			AssertEquals("MODEL", line5SM.ItemDescription);
			AssertEquals("INGREDIENT", line5SM.Ingredient);
		}

		public void TestHeaderWithFormD()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			entry.EntryNumber = "88888211002U";

			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryNum = "88888211002U";
			entryNum.CE_EntryLineReference = "1";
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SM;
			entryNum.CE_IssueDate = ZDateTime.Today;

			var invoice = entry.Declaration.Invoices[0];
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;

			invoice.ValuationSupportingDocument1 = "11";
			invoice.ValuationSupportingDocument2 = "22";

			invoice.ValuationDeclarationCode301 = true;
			invoice.ValuationDeclarationCode302 = true;
			invoice.ValuationDeclarationCode303 = true;
			invoice.ValuationDeclarationCode304 = true;
			invoice.ValuationDeclarationCode305 = true;
			invoice.ValuationDeclarationCode306 = true;
			invoice.ValuationDeclarationCode307 = "기타사유 - Use Code";
			invoice.ValuationDeclarationCode401 = true;
			invoice.ValuationDeclarationCode402 = false;
			invoice.ValuationDeclarationCode403 = false;
			invoice.ValuationDeclarationCode404 = false;
			invoice.ValuationDeclarationCode405 = "기타사유 - Goods Pricing";

			var import5SM = new Import5SMHeaderCreator().Create(entry);

			AssertEquals("B", import5SM.FormDData.ValuationMethod);
			AssertEquals("11", import5SM.FormDData.ValuationSupportingDocument1);
			AssertEquals("22", import5SM.FormDData.ValuationSupportingDocument2);
			var usecodes = import5SM.FormDData.UseCodes.ToArray();
			AssertEquals(7, usecodes.Length);
			AssertEquals("301", usecodes[0].Code);
			AssertEquals("Y", usecodes[0].CodeOtherDescription);
			AssertEquals("302", usecodes[1].Code);
			AssertEquals("Y", usecodes[1].CodeOtherDescription);
			AssertEquals("303", usecodes[2].Code);
			AssertEquals("Y", usecodes[2].CodeOtherDescription);
			AssertEquals("304", usecodes[3].Code);
			AssertEquals("Y", usecodes[3].CodeOtherDescription);
			AssertEquals("305", usecodes[4].Code);
			AssertEquals("Y", usecodes[4].CodeOtherDescription);
			AssertEquals("306", usecodes[5].Code);
			AssertEquals("Y", usecodes[5].CodeOtherDescription);
			AssertEquals("307", usecodes[6].Code);
			AssertEquals("기타사유 - Use Code", usecodes[6].CodeOtherDescription);

			var goodsPricingBasis = import5SM.FormDData.GoodsPricingBasis.ToArray();
			AssertEquals(5, goodsPricingBasis.Length);
			AssertEquals("401", goodsPricingBasis[0].Code);
			AssertEquals("Y", goodsPricingBasis[0].CodeOtherDescription);
			AssertEquals("402", goodsPricingBasis[1].Code);
			AssertEquals("", goodsPricingBasis[1].CodeOtherDescription);
			AssertEquals("403", goodsPricingBasis[2].Code);
			AssertEquals("", goodsPricingBasis[2].CodeOtherDescription);
			AssertEquals("404", goodsPricingBasis[3].Code);
			AssertEquals("", goodsPricingBasis[3].CodeOtherDescription);
			AssertEquals("405", goodsPricingBasis[4].Code);
			AssertEquals("기타사유 - Goods Pricing", goodsPricingBasis[4].CodeOtherDescription);
		}

		public void TestImporterAndSupplier()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryWithFullGOVCBR934Data();
			var declaration = entry.Declaration;
			var import5SM = new Import5SMHeaderCreator().Create(entry);
			AssertNotNull(import5SM.Importer);
			AssertEquals("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", import5SM.Importer.CompanyName);
			AssertEquals("홍길동", import5SM.Importer.RepresentativeName);
			AssertEquals("서울 강남구 테헤란로 129", import5SM.Importer.AddressLine1);
			AssertEquals("8층", import5SM.Importer.AddressLine2);

			AssertNotNull(import5SM.Supplier);
			AssertEquals("레디코리아2", import5SM.Supplier.CompanyName);
			AssertEquals("김택윤", import5SM.Supplier.RepresentativeName);
			AssertEquals("서울특별시 서초구 동광로 41", import5SM.Supplier.AddressLine1);
			AssertEquals("레디인빌딩", import5SM.Supplier.AddressLine2);
			AssertEquals("KR", import5SM.Supplier.CountryCode);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;

			import5SM = new Import5SMHeaderCreator().Create(entry);
			AssertNull(import5SM.Importer);
			AssertNull(import5SM.Supplier);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
