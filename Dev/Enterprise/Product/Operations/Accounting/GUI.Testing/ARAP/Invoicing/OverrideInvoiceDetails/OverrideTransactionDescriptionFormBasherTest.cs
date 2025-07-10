using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideTransactionDescriptionForm))]
	public class OverrideTransactionDescriptionFormBasherTest : OverrideInvoiceDetailsFormTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(Factory, Factory.New<ARInvoice>().PK));
		}

		#endregion

		public void TestNewCaption()
		{
			var expectedCaption = NoResourceStringData.GetData("New caption");
			using (var descriptionForm = new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(Factory, Factory.New<ARInvoice>().PK), expectedCaption))
			{
				AssertEquals("The new caption should be expected caption.", expectedCaption, descriptionForm.CaptionResourceString);
			}
		}

		public void TestPostingGroupAndSellReferenceColumns()
		{
			using (AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var descriptionFormWithOnlySellReferenceColumn = new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(Factory, Factory.New<ARInvoice>().PK)))
			{
				descriptionFormWithOnlySellReferenceColumn.Show();
				Application.DoEvents();
				AssertEquals("The Posting Group column should not be visible.", false, descriptionFormWithOnlySellReferenceColumn.InvoicesGrid_ForTestOnly.Columns.Contains(TransactionHeader.Schema.PostingGroup));
				AssertEquals("The Sell Reference column should be visible.", true, descriptionFormWithOnlySellReferenceColumn.InvoicesGrid_ForTestOnly.GetColumnStyle(TransactionHeader.Schema.AH_ChequeOrReference).IsVisible);
				AssertEquals("The Sell Reference column should be readonly.", true, descriptionFormWithOnlySellReferenceColumn.InvoicesGrid_ForTestOnly.GetColumnStyle(TransactionHeader.Schema.AH_ChequeOrReference).IsReadOnly);
			}

			using (AccountingConfigurationRegistry.Instance.PopupARInvoiceDescriptionOverrideOnPosting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var descriptionFormWithColumnsShow = new OverrideTransactionDescriptionForm(new OverrideTransactionDescriptionHelper(Factory, Factory.New<ARInvoice>().PK)))
			{
				descriptionFormWithColumnsShow.Show();
				Application.DoEvents();
				AssertEquals("The Posting Group column should be visible.", true, descriptionFormWithColumnsShow.InvoicesGrid_ForTestOnly.GetColumnStyle(TransactionHeader.Schema.PostingGroup).IsVisible);
				AssertEquals("The Sell Reference column should be visible.", true, descriptionFormWithColumnsShow.InvoicesGrid_ForTestOnly.GetColumnStyle(TransactionHeader.Schema.AH_ChequeOrReference).IsVisible);
				AssertEquals("The Posting Group column should be readonly.", true, descriptionFormWithColumnsShow.InvoicesGrid_ForTestOnly.GetColumnStyle(TransactionHeader.Schema.PostingGroup).IsReadOnly);
				AssertEquals("The Sell Reference column should be readonly.", true, descriptionFormWithColumnsShow.InvoicesGrid_ForTestOnly.GetColumnStyle(TransactionHeader.Schema.AH_ChequeOrReference).IsReadOnly);
			}
		}
	}
}
