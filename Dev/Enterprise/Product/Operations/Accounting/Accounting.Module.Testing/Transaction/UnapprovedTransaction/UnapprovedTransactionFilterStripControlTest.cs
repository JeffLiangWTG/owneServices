using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class UnapprovedTransactionFilterStripControlTest : TransactionFilterStripControlTest
	{
		public void TestRemoveExcessColumnStyles()
		{
			APTransactionFilterStripBusinessObject aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			TransactionHeaderCollection gridCollection = new TransactionHeaderCollection(Factory);

			using (UnapprovedTransactionFilterStripControl filterControl = new UnapprovedTransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				bool isObsoleteFieldsExist = false;
				List<ZString> columnsList = new List<ZString>(new ZString[] { AccTransactionHeaderSchema.AH_FullyPaidDate.Name, TransactionHeader.Schema.DepositBatchNumber, TransactionHeader.Schema.DirectDebitNumber, AccTransactionHeaderSchema.AH_InvoicePrinted.Name, TransactionHeader.Schema.AH_TransactionReference, TransactionHeader.Schema.OSOutstandingAmountMatching, TransactionHeader.Schema.AH_Ledger, TransactionHeader.Schema.AH_PostToGL, "JobNumber", "EInvoicingStatus", "EInvoicingError", "EInvoicingLastResponseReceivedUtc", "EInvoicingLastSentTimeUtc" });

				for (int i = 0; i < filterControl.FilteredGrid.ColumnStyles.Count; i++)
				{
					ZGridColumnInfo column = (ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[i];
					if (columnsList.Contains(column.ColumnName))
					{
						isObsoleteFieldsExist = true;
						break;
					}
				}
				Assert("Obsolete fields must be deleted", !isObsoleteFieldsExist);
			}
		}

		public void TestDefaultDisplayColumnsOrder()
		{
			APTransactionFilterStripBusinessObject aPFilterBusinessObject = new APTransactionFilterStripBusinessObject();
			TransactionHeaderCollection gridCollection = new TransactionHeaderCollection(Factory);

			using (UnapprovedTransactionFilterStripControl filterControl = new UnapprovedTransactionFilterStripControl(gridCollection, aPFilterBusinessObject))
			{
				filterControl.Bind_ForTestOnly();
				AssertEquals(TransactionHeader.Schema.AH_IsCancelled, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[0]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_TransactionType, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[1]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_TransactionNum, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[2]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_ConsolidatedInvoiceRef, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[3]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_Desc, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[4]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_TransactionCategory, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[5]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_OH, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[6]).ColumnName);
				AssertEquals("IntercompanyOrgProxy", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[7]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_RX_NKTransactionCurrency, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[8]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_OSTotal, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[9]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_ExchangeRate, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[10]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_LocalTotal, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[11]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_OSTax, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[12]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_PostDate, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[13]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_InvoiceDate, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[14]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_DueDate, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[15]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_GB, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[16]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_GE, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[17]).ColumnName);
				AssertEquals("IsSelfBillingInvoice", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[18]).ColumnName);
				AssertEquals(TransactionHeader.Schema.AH_ChequeOrReference, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[19]).ColumnName);
				AssertEquals(TransactionHeader.Schema.ReceivingOperator, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[20]).ColumnName);
				AssertEquals(TransactionHeader.Schema.ReceivingBranch, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[21]).ColumnName);
				AssertEquals(TransactionHeader.Schema.ReceivingDepartment, ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[22]).ColumnName);

				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[0]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[1]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[2]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[3]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[4]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[5]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[6]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[7]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[8]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[9]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[10]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[11]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[12]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[13]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[14]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[15]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[16]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[17]).IsVisible);
				Assert(!((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[18]).IsVisible);
				Assert(((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[19]).IsVisible);
				Assert(!((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[20]).IsVisible);
				Assert(!((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[21]).IsVisible);
				Assert(!((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[22]).IsVisible);

				Assert(!filterControl.FilteredGrid.Columns[TransactionHeader.Schema.ReceivingOperator].IsVisible);
				Assert(!filterControl.FilteredGrid.Columns[TransactionHeader.Schema.ReceivingBranch].IsVisible);
				Assert(!filterControl.FilteredGrid.Columns[TransactionHeader.Schema.ReceivingDepartment].IsVisible);
			}
		}
	}
}
