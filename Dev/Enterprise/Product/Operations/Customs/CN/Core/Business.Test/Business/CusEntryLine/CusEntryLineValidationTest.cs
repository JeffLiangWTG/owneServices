using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryLine()
		{
			var parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Validation.EntryLine, parent);
		}

		public void TestCheckGoodsSpecModel()
		{
			var maxLength = AdditionalInformationHelper.GoodsSpecModelMaxLength;
			var cusEntryHeaderItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				Factory.Save();
			});
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00423", "00352", "99999");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var entryHeader = cusEntryHeaderItems.EntryHeader;
			var entryLine = cusEntryHeaderItems.EntryLine;
			var invoiceHeader = cusEntryHeaderItems.InvoiceHeader;
			var invoiceLine1 = cusEntryHeaderItems.InvoiceLine;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			entryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
			invoiceLine1.JI_Tariff = "2713200000";
			invoiceLine2.JI_Tariff = "2713200000";
			invoiceLine1.XC_GoodsSpecModel = new string('X', maxLength - 4);
			invoiceLine2.XC_GoodsSpecModel = new string('X', maxLength - 4);
			entryLine.Validation.ValidateGoodsSpecModel();
			AssertNoMessageErrorContaining(entryLine.GoodsSpecModelInfo, "exceeds the maximum allowed");
			invoiceLine1.XC_GoodsSpecModel += "|A|B";
			invoiceLine2.XC_GoodsSpecModel += "|C|D";
			entryLine.Validation.ValidateGoodsSpecModel();
			AssertHasMessageErrorContaining(entryLine.GoodsSpecModelInfo, "exceeds the maximum allowed");
		}

		public void TestValidationModeProvider()
		{
			var cusEntryHeaderItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { Factory.Save(); });
			var entryLine = cusEntryHeaderItems.EntryLine;
			ValidationExtensionsTest.AssertValidationModeProvider(cusEntryHeaderItems.JobDeclaration, entryLine.Validation.ValidationModeProvider);

			var parent = Factory.New<CusEntryLine>();
			AssertNull(parent.Validation.ValidationModeProvider);
		}
	}
}
