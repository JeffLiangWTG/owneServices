using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	internal class BRAdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLineTest : TestCaseWithFactory
	{
		public void TestCreateSuspensionDrawbackCollection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var suspensionDrawback = invoiceLine.SuspensionDrawbackCollection.AddNew();

			suspensionDrawback.CSI_SubType = "3";
			suspensionDrawback.CSI_ReferenceNumber = "123";
			suspensionDrawback.CSI_ReferenceNumber2 = "222";
			suspensionDrawback.CSI_LineNo = 1;
			suspensionDrawback.CSI_Quantity = 20;
			suspensionDrawback.CSI_Tariff = "333";
			suspensionDrawback.CSI_Value = 10m;

			var suspensionDrawbackInvoice = suspensionDrawback.SuspensionDrawbackInvoiceCollection.AddNew();
			suspensionDrawbackInvoice.CSI_ReferenceNumber = "555";
			suspensionDrawbackInvoice.CSI_Quantity = 5;
			suspensionDrawbackInvoice.CSI_Value = 5m;
			suspensionDrawbackInvoice.CSI_DateOfIssue = new ZDateTime(2023, 1, 3);

			var suspensionDrawbackImportEntryDocument = suspensionDrawback.SuspensionDrawbackImportEntryDocumentCollection.AddNew();
			suspensionDrawbackImportEntryDocument.CSI_ReferenceNumber = "444";
			suspensionDrawbackImportEntryDocument.CSI_Quantity = 4;
			suspensionDrawbackImportEntryDocument.CSI_Value = 40m;
			suspensionDrawbackImportEntryDocument.CSI_SubType = "1";
			suspensionDrawbackImportEntryDocument.CSI_LineNo = 2;

			var dataObjectWriterForInvoiceLine = new BRAdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine(invoiceLine);

			var addInfoGroupList = dataObjectWriterForInvoiceLine.CreateCollection()?.ToList();

			CombineAssertions(() =>
			{
				Assert("addInfoGroupList should not be null or empty ", addInfoGroupList != null && addInfoGroupList.Count > 0);
				AssertEquals("addInfoGroupList should be ", 1, addInfoGroupList?.Count);

				var suspensionDrawbackInfo = addInfoGroupList[0];
				AssertEquals("addInfoGroup.Type.Code should be ", Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawback, suspensionDrawbackInfo.Type.Code);
				AssertEquals("AddInfoCollection should be ", 7, suspensionDrawbackInfo.AddInfoCollection?.Count);

				int indexSuspensionDrawback = 0;
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "123", Constants.AddInfoKeys.SuspensionDrawback.CNPJBeneficiary);
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "222", Constants.AddInfoKeys.SuspensionDrawback.CANumber);
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "1", Constants.AddInfoKeys.SuspensionDrawback.CALineItemNumber);
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "20", Constants.AddInfoKeys.SuspensionDrawback.QuantityUsed);
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "3", Constants.AddInfoKeys.SuspensionDrawback.CATypeOfConcessionAct);
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "333", Constants.AddInfoKeys.SuspensionDrawback.TariffOfTheCAImportItem);
				AssertAddInfo(suspensionDrawbackInfo?.AddInfoCollection[indexSuspensionDrawback++], "10", Constants.AddInfoKeys.SuspensionDrawback.ForeignExchangeHedgedVMLE);

				AssertEquals("addInfoGroupList should be ", 2, suspensionDrawbackInfo.AddInfoGroupCollection?.Count);

				var drawbackImportEntryDocumentList = suspensionDrawbackInfo.AddInfoGroupCollection.Where(addInfo => Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackImportEntryDocument.Equals(addInfo.Type.Code)).ToList();

				AssertEquals("drawbackImportEntryDocumentList should be ", 1, drawbackImportEntryDocumentList?.Count);
				var drawbackImportEntryDocument = drawbackImportEntryDocumentList[0];
				AssertEquals("drawbackImportEntryDocumentList should be ", 5, drawbackImportEntryDocument?.AddInfoCollection.Count);

				int indexDrawbackImportEntryDocument = 0;
				AssertAddInfo(drawbackImportEntryDocument?.AddInfoCollection[indexDrawbackImportEntryDocument++], "444", Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.ImportEntry);
				AssertAddInfo(drawbackImportEntryDocument?.AddInfoCollection[indexDrawbackImportEntryDocument++], "4", Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Quantity);
				AssertAddInfo(drawbackImportEntryDocument?.AddInfoCollection[indexDrawbackImportEntryDocument++], "40", Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Value);
				AssertAddInfo(drawbackImportEntryDocument?.AddInfoCollection[indexDrawbackImportEntryDocument++], "1", Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Category);
				AssertAddInfo(drawbackImportEntryDocument?.AddInfoCollection[indexDrawbackImportEntryDocument++], "2", Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.EntryLine);

				var drawbackInvoicetList = suspensionDrawbackInfo.AddInfoGroupCollection.Where(addInfo => Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackInvoice.Equals(addInfo.Type.Code)).ToList();
				AssertEquals("drawbackInvoicetList should be ", 1, drawbackInvoicetList?.Count);
				var drawbackInvoice = drawbackInvoicetList[0];

				int indexDrawbackInvoice = 0;
				AssertEquals("drawbackInvoice?.AddInfoCollection should be ", 4, drawbackInvoice?.AddInfoCollection?.Count);
				AssertAddInfo(drawbackInvoice?.AddInfoCollection[indexDrawbackInvoice++], "555", Constants.AddInfoKeys.SuspensionDrawbackInvoice.InvoiceNumber);
				AssertAddInfo(drawbackInvoice?.AddInfoCollection[indexDrawbackInvoice++], "5", Constants.AddInfoKeys.SuspensionDrawbackInvoice.Quantity);
				AssertAddInfo(drawbackInvoice?.AddInfoCollection[indexDrawbackInvoice++], "5", Constants.AddInfoKeys.SuspensionDrawbackInvoice.TradingCurrencyValue);
				AssertAddInfo(drawbackInvoice?.AddInfoCollection[indexDrawbackInvoice++], "2023-01-03T00:00:00", Constants.AddInfoKeys.SuspensionDrawbackInvoice.Date);
			});
		}

		void AssertAddInfo(AddInfo addInfo, string value, string key)
		{
			AssertEquals($"AddInfo key should be ", key, addInfo?.Key);
			AssertEquals($"{key} should be ", value, addInfo?.Value);
		}
	}
}
