using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public partial class TempStorageRegTransactionNewForm : ZChildForm
{
	TempStorageRegTransactionNewForm()
	{
		InitializeComponent();
	}

	public TempStorageRegTransactionNewForm(CusTempStorageRegLine regLine, CusTempStorageRegLineTransaction transaction, CusTempStorageRegLineTransactionFormEditable newEditableTransaction) : base(newEditableTransaction)
	{
		this.regLine = regLine;
		this.transaction = transaction;
		SetMainDetailsLayout();
	}
	protected CusTempStorageRegLine regLine;
	readonly CusTempStorageRegLineTransaction transaction;

	protected CusTempStorageRegLineTransactionFormEditable newEditableTransaction => (CusTempStorageRegLineTransactionFormEditable)BusinessEntity;

	public override string FormCaption => Res.GetString("F85CA9F1-A2B4-44E1-9ADB-BBBC21ED44C7", "Manual Transaction");

	void SetMainDetailsLayout()
	{
		var layout = new TempStorageRegTransactionNewLayout();
		if (layout != null && MainDynamicLayoutPanel != null)
		{
			MainDynamicLayoutPanel.UpdateLayout(layout);
		}
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		_ = CloseButton.Focus();

		if (DialogResult == System.Windows.Forms.DialogResult.Cancel)
		{
			newEditableTransaction.Delete();
			regLine.CusTempStorageRegLineTransactions.Delete(transaction);
			transaction.Delete();
		}
		else
		{
			newEditableTransaction.RunPreSaveValidation();
			if (newEditableTransaction.NotificationsIncludingChildren.GetErrors().Count() > 0)
			{
				Globals.Message.ShowError(Res.GetString("EDC963DF-F38A-42C8-B505-04707262E521", "The form has errors. Please fix them before continuing."));
				e.Cancel = true;
			}
			else
			{
				transaction.PhysicalInOutDate = newEditableTransaction.PhysicalInOutDate;
				transaction.TransactionDate = newEditableTransaction.TransactionDate;
				transaction.SRT_GrossWeight = newEditableTransaction.SRT_GrossWeight;
				transaction.SRT_PackageQty = newEditableTransaction.SRT_PackageQty;
				transaction.SRT_InternalReferenceType = newEditableTransaction.SRT_InternalReferenceType;
				transaction.SRT_InternalReferenceNumber = newEditableTransaction.SRT_InternalReferenceNumber;
				transaction.SRT_ReferenceType = newEditableTransaction.SRT_ReferenceType;
				transaction.SRT_Reference = newEditableTransaction.SRT_Reference;
				transaction.SRT_Comments = newEditableTransaction.SRT_Comments;
			}
		}

		base.OnClosing(e);
	}

	void OnSaveButton_Click(object sender, EventArgs e) => Close();

	void OnCloseButton_Click(object sender, EventArgs e) => Close();
}
