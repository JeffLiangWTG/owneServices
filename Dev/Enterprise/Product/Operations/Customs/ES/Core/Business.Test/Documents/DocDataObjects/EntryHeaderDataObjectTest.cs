using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(EntryHeaderDataObject))]
	public class EntryHeaderDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new EntryHeaderDataObject(entryHeader);
		}

		public void TestIncoTerms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				var dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("IncoTerms is empty", " ", dataObject.IncoTerms);

				invoiceHeader.JZ_IncoTerm = IncoTerms.FreeOnBoard;
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("IncoTerms only has IncoTerm code", IncoTerms.FreeOnBoard + " ", dataObject.IncoTerms);

				declaration.ZG_AgreedPlaceCode = "1";
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("IncoTerms has IncoTerm code and AgreedPlaceCode", IncoTerms.FreeOnBoard + "1 ", dataObject.IncoTerms);

				invoiceHeader.JZ_IncoTermPlace = "Place";
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("IncoTerms has IncoTerm code, AgreedPlaceCode and ShipmentIncoTermPlace", IncoTerms.FreeOnBoard + "1 Place", dataObject.IncoTerms);

				var invoiceHeader2 = declaration.Invoices.AddNew();
				invoiceHeader2.JZ_IncoTerm = IncoTerms.FreeOnBoard;
				invoiceHeader2.InvoiceLines.AddNew();
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				entry = declaration.CustomsEntryHeaders[0];
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("IncoTerms is empty when there are more than one invoice header", ZString.Empty, dataObject.IncoTerms);
			});
		}

		public void TestInvoiceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				var dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("InvoiceNumber is filled with only invoice number", "1 del ", dataObject.InvoiceNumber);

				invoiceHeader.JZ_InvoiceDate = new ZDateTime(2020, 7, 28);
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("InvoiceNumber is filled with invoice number and date", "1 del 28-07-2020", dataObject.InvoiceNumber);

				var invoiceHeader2 = declaration.Invoices.AddNew();
				invoiceHeader2.InvoiceLines.AddNew();
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				entry = declaration.CustomsEntryHeaders[0];
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("InvoiceNumber is empty when there are more than one invoice header", ZString.Empty, dataObject.InvoiceNumber);
			});
		}

		public void TestMRNTextForDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				var dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("MRNText is empty", ZString.Empty, dataObject.MRNText);

				entry.MovementReferenceNumber = "123456789";
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("MRNText is filled with movement reference number without formatting (not 18 chars)", "123456789", dataObject.MRNText);

				entry.MovementReferenceNumber = "20ES00999910000035";
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("MRNText is filled with movement reference number with formatting (18 chars)", "20 ES009999 1 000003 5", dataObject.MRNText);

				entry.EntryNumber = "987654321";
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("MRNText is filled with entry number without formatting (not 18 chars)", "987654321", dataObject.MRNText);

				entry.EntryNumber = "20ES00999910000034";
				dataObject = new EntryHeaderDataObject(entry);
				AssertEquals("MRNText is filled with entry number with formatting (18 chars)", "20 ES009999 1 000003 4", dataObject.MRNText);
			});
		}
	}
}
