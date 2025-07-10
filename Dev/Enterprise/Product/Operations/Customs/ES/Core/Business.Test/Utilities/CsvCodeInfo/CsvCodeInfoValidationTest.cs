using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class CsvCodeInfoValidationTest : TestCaseWithFactory
	{
		public void TestCheckCsvCodeFromUser()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			CombineAssertions(() =>
			{
				AssertNoErrors("No errors when CsvCodeFromUser is empty", csvCodeInfo.CsvCodeFromUserInfo);

				csvCodeInfo.CsvCodeFromUser = "+--**__aa/#€&@";
				AssertHasErrorContaining("There is an error when CsvCodeFromUser has an incorrect format", csvCodeInfo.CsvCodeFromUserInfo, "+--**__aa/#€&@ format is incorrect. Please fill with a correct CSV Clearance Code");

				csvCodeInfo.CsvCodeFromUser = "ABCDEFGHIJKLMNOP";
				AssertNoErrors("No errors when CsvCodeFromUser is filled with the correct format", csvCodeInfo.CsvCodeFromUserInfo);
			});
		}

		public void TestCheckSecondaryCsvCodeFromUser_Import()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			CombineAssertions(() =>
			{
				AssertNoErrors("No errors when SecondaryCsvCodeFromUser is empty", csvCodeInfo.SecondaryCsvCodeFromUserInfo);

				csvCodeInfo.SecondaryCsvCodeFromUser = "+--**__aa/#€&@";
				AssertHasErrorContaining("There is an error when SecondaryCsvCodeFromUser has an incorrect format", csvCodeInfo.SecondaryCsvCodeFromUserInfo, "+--**__aa/#€&@ format is incorrect. Please fill with a correct CSV Import Certificate Code");

				csvCodeInfo.SecondaryCsvCodeFromUser = "ABCDEFGHIJKLMNOP";
				AssertNoErrors("No errors when SecondaryCsvCodeFromUser is filled with the correct format", csvCodeInfo.SecondaryCsvCodeFromUserInfo);
			});
		}

		public void TestCheckSecondaryCsvCodeFromUser_Export()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			CombineAssertions(() =>
			{
				AssertNoErrors("No errors when SecondaryCsvCodeFromUser is empty", csvCodeInfo.SecondaryCsvCodeFromUserInfo);

				csvCodeInfo.SecondaryCsvCodeFromUser = "+--**__aa/#€&@";
				AssertHasErrorContaining("There is an error when SecondaryCsvCodeFromUser has an incorrect format", csvCodeInfo.SecondaryCsvCodeFromUserInfo, "+--**__aa/#€&@ format is incorrect. Please fill with a correct CSV T2L Code");

				csvCodeInfo.SecondaryCsvCodeFromUser = "ABCDEFGHIJKLMNOP";
				AssertNoErrors("No errors when SecondaryCsvCodeFromUser is filled with the correct format", csvCodeInfo.SecondaryCsvCodeFromUserInfo);
			});
		}

		public void TestCheckThirdCsvCodeFromUser()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			CombineAssertions(() =>
			{
				AssertNoErrors("No errors when ThirdCsvCodeFromUser is empty", csvCodeInfo.ThirdCsvCodeFromUserInfo);

				csvCodeInfo.ThirdCsvCodeFromUser = "+--**__aa/#€&@";
				AssertHasErrorContaining("There is an error when ThirdCsvCodeFromUser has an incorrect format", csvCodeInfo.ThirdCsvCodeFromUserInfo, "+--**__aa/#€&@ format is incorrect. Please fill with a correct CSV Exit Certificate Code");

				csvCodeInfo.ThirdCsvCodeFromUser = "ABCDEFGHIJKLMNOP";
				AssertNoErrors("No errors when ThirdCsvCodeFromUser is filled with the correct format", csvCodeInfo.ThirdCsvCodeFromUserInfo);
			});
		}

		public void TestCheckClearanceDateFromUser()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			CombineAssertions(() =>
			{
				AssertNoErrors("No errors when ClearanceDateFromUser is empty and CsvCodeFromUser is also empty", csvCodeInfo.ClearanceDateFromUserInfo);

				csvCodeInfo.ClearanceDateFromUser = ZDateTime.Invalid;
				AssertHasErrorContaining("There is an error when ClearanceDateFromUser has an invalid value", csvCodeInfo.ClearanceDateFromUserInfo, "Clearance Date is invalid. Please fill with a correct date");

				csvCodeInfo.CsvCodeFromUser = "ABCDEFGHIJKLMNOP";
				csvCodeInfo.ClearanceDateFromUser = ZDateTime.Now;
				AssertNoErrors("No errors when ClearanceDateFromUser is filled with the correct value", csvCodeInfo.ClearanceDateFromUserInfo);

				csvCodeInfo.ClearanceDateFromUser = ZDateTime.Empty;
				AssertHasErrorContaining("There is an error when ClearanceDateFromUser is empty and CsvCodeFromUser is not empty", csvCodeInfo.ClearanceDateFromUserInfo, "Clearance Date is mandatory when CSV Clearance is entered");
			});
		}

		CusEntryHeader CreateEntryHeader(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];

			return entryHeader;
		}
	}
}
