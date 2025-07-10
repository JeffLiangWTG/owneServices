using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Riba.Testing
{
	[TestedType(typeof(AddTransactionsToOrderForm))]
	public class TestAddTransactionsToOrderForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccCollectionOrder testOrder = Factory.New<AccCollectionOrder>();
			OrderTransactionsFilterHolder holder = new OrderTransactionsFilterHolder(testOrder);
			return new AddTransactionsToOrderForm(holder);
		}

		[TestDate(2014, 11, 11)]
		public void TestAddTransactionsIntoOrderWithInvalidDueDate()
		{
			AssertAddTransactionsIntoOrderWithInvalidData(@"Some transactions due date is conflicting with order’s collection date.
Only transactions due on or before this date can be included in this order.

Please revise transactions selection.", new ZDateTime(2014, 10, 10));
		}

		[TestDate(2014, 11, 11)]
		public void TestAddTransactionsIntoOrderWithInvalidAgreedPaymentMethods()
		{
			var paymentMethod = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value;
			paymentMethod.Set("CCD", true);
			paymentMethod.Set("CRQ", false);

			using (OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, paymentMethod))
			{
				AssertAddTransactionsIntoOrderWithInvalidData(@"Collection order only supports INV, CRD, ADJ, JNL with 'CCD' agreed payment method(s).

Please revise transactions selection.", new ZDateTime(2014, 10, 8));
			}
		}

		[TestDate(2014, 11, 11)]
		public void TestAddTransactionsIntoOrderWithInvalidAgreedPaymentMethodsManyValues()
		{
			var paymentMethod = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value;
			paymentMethod.Set("CCD", true);
			paymentMethod.Set("CHK", true);
			paymentMethod.Set("DBC", true);
			paymentMethod.Set("TRF", true);
			paymentMethod.Set("CRQ", false);

			using (OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, paymentMethod))
			{
				AssertAddTransactionsIntoOrderWithInvalidData(@"Collection order only supports INV, CRD, ADJ, JNL with 'CCD', 'CHK', 'DBC', 'TRF' agreed payment method(s).

Please revise transactions selection.", new ZDateTime(2014, 10, 8));
			}
		}

		[TestDate(2014, 11, 11)]
		public void TestAddTransactionsIntoOrderWithInvalidAgreedPaymentMethodsEmptyValue()
		{
			var paymentMethod = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value;
			paymentMethod.Set("CRQ", false);

			using (OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, paymentMethod))
			{
				AssertAddTransactionsIntoOrderWithInvalidData(@"There are no valid agreed payment methods configured for Collection Batches, it is not possible to add transactions to the order.

Please check the registry setting at Master Data -> Organizations -> Code Lists -> Receivables Credit Agreed Payment Methods.", new ZDateTime(2014, 10, 8));
			}
		}

		void AssertAddTransactionsIntoOrderWithInvalidData(string errorMessage, ZDateTime transactionsDate)
		{
			var creator = new TestObjectCreator(Factory);
			var invoices = new AccTransactionHeaderCollection(Factory);
			var invoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.AUD, 1, 10, 0, 10, 0);
			invoice1.AH_OH = creator.ABIGAS.PK;
			invoice1.AH_DueDate = transactionsDate;
			invoice1.AH_AgreedPaymentMethodOverride = "CRQ";
			var invoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			invoice2.AH_OH = creator.ABIGAS.PK;
			invoice2.AH_DueDate = transactionsDate;
			invoice2.AH_AgreedPaymentMethodOverride = "CRQ";
			Factory.Save();

			invoices.AddRange(new InvoicingBase[] { invoice1 });
			var poster = new AccCollectionBatchPoster(Factory);
			poster.CreateBatchOrdersAndFillByTransactions(invoices, AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
			var batch1 = poster.Batch;
			batch1.ACB_AB = creator.AUDBankAccount.PK;
			var order = Factory.LoadTop1<AccCollectionOrder>(new ZQuery());
			order.ACO_CollectionDate = new ZDate(2014, 10, 9);
			Factory.Save();

			var holder = new OrderTransactionsFilterHolder(order);

			using (var form = new AddTransactionsToOrderForm(holder))
			{
				form.Show();

				form.FilterControl_PerformSearch(this, new EventArgs());
				form.InvoicesGrid.SelectAllElements();
				AssertEquals(1, form.InvoicesGrid.SelectedElements.Length);

				form.AddButton.PerformClick();
				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOutstandingAmountCaption()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				var grid = form.FindSingleOrDefault<ZGrid>("InvoicesGrid");
				AssertNotNull(grid);
				var columnName = "AH_OutstandingAmount";
				var columnInfo = grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
				AssertNotNull(columnInfo);
				AssertNull(columnInfo.CaptionResourceString.Caption);
				AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
			}
		}
	}
}
