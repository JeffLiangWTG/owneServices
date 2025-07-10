using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportD72MessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestImportD72MessageDetailsUserControl()
		{
			using (var userControl = new ImportD72MessageDetailsUserControl())
			{
				AssertEquals(true, userControl.FindSingle<ZGroupBox>("InvoiceLineGroupBox").Visible);

				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				AssertEquals(((ZCalcEditColumnStyleInfo)entryLineGrid.ColumnStyles[0]).ColumnName, nameof(MessageSendingEntryLineObject.EntryLineNo));
				AssertEquals(((ZTextBoxColumnStyleInfo)entryLineGrid.ColumnStyles[1]).ColumnName, nameof(MessageSendingEntryLineObject.FormattedHSCode));
				AssertEquals(((ZTextBoxColumnStyleInfo)entryLineGrid.ColumnStyles[2]).ColumnName, nameof(MessageSendingEntryLineObject.HSDescription));
				AssertEquals(((ZTextBoxColumnStyleInfo)entryLineGrid.ColumnStyles[3]).ColumnName, nameof(MessageSendingEntryLineObject.Preference));

				var invoiceLineGrid = userControl.FindSingle<ZGrid>("InvoiceLineGrid");
				AssertEquals(((ZCalcEditColumnStyleInfo)invoiceLineGrid.ColumnStyles[0]).ColumnName, nameof(MessageSendingInvoiceLine.EntryLineNo));
				AssertEquals(((ZCalcEditColumnStyleInfo)invoiceLineGrid.ColumnStyles[1]).ColumnName, nameof(MessageSendingInvoiceLine.InvoiceLineNo));
				AssertEquals(((ZTextBoxColumnStyleInfo)invoiceLineGrid.ColumnStyles[2]).ColumnName, nameof(MessageSendingInvoiceLine.ItemDescription));
				AssertEquals(((ZCalcEditColumnStyleInfo)invoiceLineGrid.ColumnStyles[3]).ColumnName, nameof(MessageSendingInvoiceLine.Quantity));
				AssertEquals(((ZTextBoxColumnStyleInfo)invoiceLineGrid.ColumnStyles[4]).ColumnName, nameof(MessageSendingInvoiceLine.UQ));
				AssertEquals(((ZCalcEditColumnStyleInfo)invoiceLineGrid.ColumnStyles[5]).ColumnName, nameof(MessageSendingInvoiceLine.LinePrice));
				AssertEquals(((ZTextBoxColumnStyleInfo)invoiceLineGrid.ColumnStyles[6]).ColumnName, nameof(MessageSendingInvoiceLine.AmountCurrency));
				AssertEquals(((ZTextBoxColumnStyleInfo)invoiceLineGrid.ColumnStyles[7]).ColumnName, nameof(MessageSendingInvoiceLine.Remark));
			}
		}
	}
}
