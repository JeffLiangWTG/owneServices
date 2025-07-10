using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AgreedRateMessageSendingObject))]
	sealed class AgreedRateMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new AgreedRateMessageSendingObject(entry);
		}

		public void TestData()
		{
			var universalhelper = new UniversalReferenceTestDataHelper(Factory);
			universalhelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth);
			universalhelper.CreatePreferenceForCountry("FCN1", "한ㆍ중국 FTA협정세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_AgreedDutyRate = 111111111111.11m;
			instruction.CEI_AgreedDutyRatePreferenceCode = "FCN1";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1234522000008";
			entry.EntryNumbers[0].CE_IssueDate = new ZDate("2023-12-01");
			entry.CH_CEI_Instruction = instruction.PK;

			var sendingObj = new AgreedRateMessageSendingObject(entry);
			AssertEquals("12345-22-000008", sendingObj.FormattedEntryNumber);
			AssertEquals(new ZDate("2023-12-01"), sendingObj.DeclarationDate);
			AssertEquals(111111111111.11m, sendingObj.DutyRate);
			AssertEquals("한ㆍ중국 FTA협정세율(선택1)", sendingObj.PreferenceCodeDescription);
		}

		public void TestDetailsData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "PEN NIBS1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521023", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "PEN NIBS2");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "8429521022";
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "8429521023";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8429521022";
			invoiceLine1.JI_Description = "모델 규격1";
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8429521023";
			invoiceLine2.JI_Description = "모델 규격2";
			invoiceLine2.JI_CL = entryLine2.PK;

			var sendingObj = new AgreedRateMessageSendingObject(entry);
			AssertEquals(2, sendingObj.Details.Count);

			AssertEquals(1, sendingObj.Details[0].EntryLineNo);
			AssertEquals("8429.52-1022", sendingObj.Details[0].FormattedHSCode);
			AssertEquals("PEN NIBS1", sendingObj.Details[0].HSDescription);
			AssertEquals("모델 규격1", sendingObj.Details[0].ModelName);

			AssertEquals(2, sendingObj.Details[1].EntryLineNo);
			AssertEquals("8429.52-1023", sendingObj.Details[1].FormattedHSCode);
			AssertEquals("PEN NIBS2", sendingObj.Details[1].HSDescription);
			AssertEquals("모델 규격2", sendingObj.Details[1].ModelName);
		}
	}
}
