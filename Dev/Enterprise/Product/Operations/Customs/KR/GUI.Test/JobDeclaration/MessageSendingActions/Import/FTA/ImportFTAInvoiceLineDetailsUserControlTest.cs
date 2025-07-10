using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportFTAInvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsTabPage()
		{
			using (var userControl = new ImportFTAInvoiceLineDetailsUserControl())
			{
				var grid = userControl.FindSingle<ZGrid>("FTAInvoiceLineDetailsGrid");
				AssertGrid(nameof(MessageSendingInvoiceLine.EntryLineNo), typeof(ZCalcEditColumnStyleInfo));
				AssertGrid(nameof(MessageSendingInvoiceLine.InvoiceLineNo), typeof(ZCalcEditColumnStyleInfo));
				AssertGrid(nameof(MessageSendingInvoiceLine.CertificateOfOriginNo), typeof(ZTextBoxColumnStyleInfo));
				AssertGrid(nameof(MessageSendingInvoiceLine.CertificateOfOriginSeq), typeof(ZCalcEditColumnStyleInfo));
				AssertGrid(nameof(MessageSendingInvoiceLine.CertificateOfOriginUsedQuantity), typeof(ZCalcEditColumnStyleInfo));
				AssertGrid(nameof(MessageSendingInvoiceLine.CertificateOfOriginUsedUQ), typeof(ZTextBoxColumnStyleInfo));

				void AssertGrid(string name, Type infoType)
				{
					var checkColumn = grid.GetColumnStyle(name);
					AssertEquals(name, checkColumn.ColumnName);
					AssertEquals(infoType, checkColumn.GetType());
					AssertEquals(true, checkColumn.IsReadOnly);
				}
			}
		}
	}
}
