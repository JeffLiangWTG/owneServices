using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportEntryLineWrapper))]
	sealed class LocalExportEntryLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = new LocalExportEntryLine();
			return new LocalExportEntryLineWrapper(entryLine, Factory);
		}

		public void TestEntryLineFull()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DPWithFullData();
			var wrapper = new EntryDocumentWrapper(entry, Factory).LocalExportEntryWrapper;

			var entryLineItems1 = wrapper.EntryLineItems[0];
			var entryLineItems1entryLine = entryLineItems1.EntryLine;

			AssertEquals(1, entryLineItems1entryLine.EntryLineNo);
			AssertEquals("001", entryLineItems1.FormattedEntryLineNo);
			AssertEquals("1234567890", entryLineItems1entryLine.HSCode);
			AssertEquals("1234.56-7890", entryLineItems1.FormattedHSCode);
			AssertEquals("STAINLESS STEEL", entryLineItems1entryLine.InvoiceDescription);
			AssertEquals("000000000", entryLineItems1entryLine.GoodsNo);
			AssertEquals("KG", entryLineItems1entryLine.QuantityUnit);
			AssertEquals("L172770912345", entryLineItems1entryLine.DocumentNo);
			AssertEquals("1", entryLineItems1entryLine.DocumentType);
			AssertEquals("VL", entryLineItems1entryLine.PackagesType);
			AssertEquals("010151234567001999", entryLineItems1entryLine.PreviousTransactionReferenceNo);
			AssertEquals("01", entryLineItems1entryLine.PreviousTransactionReferenceNoType);
			AssertEquals(OriginalStateDocTypeList.Descriptions._01, entryLineItems1.PreviousTransactionReferenceNoTypeName);
			AssertEquals(1000m, entryLineItems1entryLine.FOBAmount);

			var entryLineItems2 = wrapper.EntryLineItems[1];
			var entryLineItems2entryLine = entryLineItems2.EntryLine;
			AssertEquals(2, entryLineItems2entryLine.EntryLineNo);
			AssertEquals("002", entryLineItems2.FormattedEntryLineNo);
			AssertEquals("0987654321", entryLineItems2entryLine.HSCode);
			AssertEquals("0987.65-4321", entryLineItems2.FormattedHSCode);
			AssertEquals("STAINLESS STEEL2", entryLineItems2entryLine.InvoiceDescription);
			AssertEquals("1111111111", entryLineItems2entryLine.GoodsNo);
			AssertEquals("KG", entryLineItems2entryLine.QuantityUnit);
			AssertEquals("L172770925459", entryLineItems2entryLine.DocumentNo);
			AssertEquals("2", entryLineItems2entryLine.DocumentType);
			AssertEquals("VL", entryLineItems2entryLine.PackagesType);
			AssertEquals("999100765432151010", entryLineItems2entryLine.PreviousTransactionReferenceNo);
			AssertEquals("02", entryLineItems2entryLine.PreviousTransactionReferenceNoType);
			AssertEquals(OriginalStateDocTypeList.Descriptions._02, entryLineItems2.PreviousTransactionReferenceNoTypeName);
			AssertEquals(3333m, entryLineItems2entryLine.FOBAmount);
		}
	}
}
