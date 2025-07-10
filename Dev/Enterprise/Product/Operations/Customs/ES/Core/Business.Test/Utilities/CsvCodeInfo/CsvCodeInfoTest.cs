using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CsvCodeInfo))]
	public class CsvCodeInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadNew()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine.Declaration is null", () => CsvCodeInfo.LoadNew(null));
				AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine.Declaration is null", () => CsvCodeInfo.LoadNew(Factory.New<CusEntryHeader>()));
				var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
				AssertNoExceptionThrown("No exception expected", () => CsvCodeInfo.LoadNew(entryHeader));

				var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
				AssertEquals("CsvCodeFromUser is CSVClearance from EntryHeader", "ABCDEFGHIJKLMNOP", csvCodeInfo.CsvCodeFromUser);
				AssertEquals("SecondaryCsvCodeFromUser is ZG_CSVImportCertificate from an Import EntryHeader", "6543210987654321", csvCodeInfo.SecondaryCsvCodeFromUser);
				AssertEquals("ClearanceDateFromUser is CH_EntryReleaseDate", new ZDateTime(2023, 06, 14, 11, 12, 13), csvCodeInfo.ClearanceDateFromUser);

				entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Export);
				csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
				AssertEquals("SecondaryCsvCodeFromUser is ZG_CSVT2L from an Export EntryHeader", "1234568790123456", csvCodeInfo.SecondaryCsvCodeFromUser);

				AssertEquals("ThirdCsvCodeFromUser is ZG_CSVExitCertificate", "PONMLKJIHGFEDCBA", csvCodeInfo.ThirdCsvCodeFromUser);
			});
		}

		public void TestValidation()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			AssertType<CsvCodeInfoValidation>(csvCodeInfo.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryHeader = CreateEntryHeader(EU.Business.MessageTypeList.Codes.Import);
			var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeader);
			return csvCodeInfo;
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
			entryHeader.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2023, 06, 14, 11, 12, 13);
			entryHeader.ZG_CSVT2L = "1234568790123456";
			entryHeader.ZG_CSVImportCertificate = "6543210987654321";
			entryHeader.ZG_CSVExitCertificate = "PONMLKJIHGFEDCBA";

			return entryHeader;
		}
	}
}
