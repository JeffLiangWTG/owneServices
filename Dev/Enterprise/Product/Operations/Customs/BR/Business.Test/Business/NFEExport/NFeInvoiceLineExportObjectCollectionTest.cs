using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(NFeInvoiceLineExportObjectCollection))]
	class NFeInvoiceLineExportObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NFeInvoiceLineExportObjectCollection>
	{
		protected override NFeInvoiceLineExportObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntry = declaration.ActiveEntryHeaders.AddNew();
			return new NFeInvoiceLineExportObjectCollection(cusEntry);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NFeInvoiceLineExportObject(Factory.New<JobComInvoiceLine>());
		}

		public void TestAllowNew()
		{
			var nfeObjectCollection = GetCollectionToTest();
			AssertEquals("Should not allow new", false, nfeObjectCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var nfeObjectCollection = GetCollectionToTest();
			AssertEquals("Should not allow delete", false, nfeObjectCollection.AllowRemove);
		}

		public void TestLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_Tariff = "2";
			invoiceLine2.JI_LinePrice = 400m;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.JI_Tariff = "3";
			invoiceLine3.JI_LinePrice = 400m;

			var cusEntryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader1.CH_Status = BRMessageStatusList.Codes.Accepted;
			cusEntryHeader1.MovementReferenceNumberSetter("2000010001");

			var entryLine1 = cusEntryHeader1.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.InvoiceLines.Add(invoiceLine2);

			var cusEntryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader2.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			var entryLine2 = cusEntryHeader2.MergedLines.AddNew();

			var nfeLines = declaration.NFeExportObject.Entries[0].Lines;
			AssertEquals("nfeLines.Count should be", 2, nfeLines.Count);

			AssertContainsExactElementsInAnyOrder("New NFeInvoiceLineExportObject for invoiceLine1 & invoiceLine2",
				new[] { invoiceLine1, invoiceLine2 }, nfeLines.Cast<NFeInvoiceLineExportObject>().Select(x => x.InvoiceLine));

			var nfeLine2 = nfeLines.Cast<NFeInvoiceLineExportObject>().FirstOrDefault(x => x.InvoiceLine == invoiceLine2);
			entryLine1.InvoiceLines.Remove(invoiceLine2);
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine1.InvoiceLines.Add(invoiceLine3);
			cusEntryHeader1.ResetTotalsAndCachedValues();
			nfeLines.Load();

			AssertEquals("nfeLines.Count should be", 2, nfeLines.Count);
			Assert("Delete NFeInvoiceLineExportObject for invoiceLine1", nfeLine2.IsDeleted);
			AssertContainsExactElementsInAnyOrder("New NFeInvoiceLineExportObject for invoiceLine1 & invoiceLine3",
				new[] { invoiceLine1, invoiceLine3 }, nfeLines.Cast<NFeInvoiceLineExportObject>().Select(x => x.InvoiceLine));
		}
	}
}
