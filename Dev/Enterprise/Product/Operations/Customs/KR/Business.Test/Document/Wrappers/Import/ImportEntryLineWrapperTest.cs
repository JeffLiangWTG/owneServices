using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportEntryLineWrapper))]
	sealed class ImportEntryLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entryLine = new ImportEntryLine();
			return new ImportEntryLineWrapper(entryLine, ZDecimal.Zero, ZString.Empty, ZBool.False);
		}

		public void TestImportEntryLine()
		{
			new TestDataSetupHelper(Factory).SetEntry929Tariff();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice1 = declaration.Invoices.AddNew();

			#region entryLine
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_AdValoremTariff = "0208100000";
			#endregion

			#region invoiceLine
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;

			invoiceLine.JI_Tariff = "0208100000";
			invoiceLine.JI_NetWeight = 99999.9m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "DZ";
			invoiceLine.JI_CustomsQuantity = 30m;
			#endregion

			#region invoiceLine2
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_SequenceNumber = 2;

			invoiceLine2.JI_Tariff = "0208100000";
			invoiceLine2.JI_NetWeight = 10.3;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_CustomsUnitQty = "DZ";
			invoiceLine2.JI_CustomsQuantity = 20m;
			#endregion

			var invoice2 = declaration.Invoices.AddNew();
			#region entryLine2
			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0208122222";
			#endregion

			#region invoiceLine3
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine2.PK;
			invoiceLine3.JI_SequenceNumber = 1;

			invoiceLine3.JI_Tariff = "0208122222";
			invoiceLine3.JI_NetWeight = 99999.9m;
			invoiceLine3.JI_NetWeightUQ = "KG";
			invoiceLine3.JI_CustomsUnitQty = "G";
			invoiceLine3.JI_CustomsQuantity = 30m;
			#endregion
			var entryHeader = new ImportEntryHeaderCreator().Create(entry);
			var wrapper = new ImportEntryHeaderWrapper(entry.PK, entryHeader, Factory);
			AssertEquals(2, wrapper.EntryLineItems.Count);

			var importEntryLine1 = wrapper.EntryLineItems[0];
			AssertEquals("TACKS", importEntryLine1.EntryLine.HSDescription);
			AssertEquals(50m, importEntryLine1.EntryLine.Quantity);

			var importEntryLine2 = wrapper.EntryLineItems[1];
			AssertEquals("TACKS2", importEntryLine2.EntryLine.HSDescription);
			AssertEquals(0m, importEntryLine2.EntryLine.Quantity);
			AssertEquals(99999.9m, importEntryLine2.EntryLine.NetWeightInKG);
		}
	}
}
