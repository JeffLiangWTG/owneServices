using System;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public partial class TempStorageRegTransactionUserControl : ZUserControl
{
	public TempStorageRegTransactionUserControl()
	{
		InitializeComponent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		if (CurrentDataItem is CusTempStorageRegLine regLine)
		{
			regLine.CusTempStorageRegLineTransactionsForFilter.SetReadOnlyIncludingChildren(false);
			TransactionsFilterControl = CreateNewTempStorageRegTransactionFilterControl(regLine.CusTempStorageRegLineTransactionsForFilter, new TempStorageRegTransactionFilterStripBusinessObject());
			TransactionsFilterControl.Name = "TempStorageRegTransactionFilterControl";
			TransactionsFilterControl.ShouldRunSearchOnStripsInitialized = true;
			TransactionsFilterControl.Dock = DockStyle.Fill;
			Controls.Add(TransactionsFilterControl);
			TransactionsFilterControl.NewTransactionButton.Visible = !regLine.IsClosed && regLine.RegHeader.Premises.SRP_IsActive;
		}
	}

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);

		if (CurrentDataItem is CusTempStorageRegLine regLine && TransactionsFilterControl is not null)
		{
			regLine.CusTempStorageRegLineTransactionsForFilter.SetReadOnlyIncludingChildren(false);
			TransactionsFilterControl.SetGridCollection(regLine.CusTempStorageRegLineTransactionsForFilter);
			TransactionsFilterControl.FirePerformSearch();
		}
	}

	protected virtual TempStorageRegTransactionFilterControl CreateNewTempStorageRegTransactionFilterControl(CusTempStorageRegLineTransactionCollection cusGuaranteeLineTransactions, TempStorageRegTransactionFilterStripBusinessObject tempStorageRegTransactionFilterStripBusinessObject)
		=> new(cusGuaranteeLineTransactions, tempStorageRegTransactionFilterStripBusinessObject);

	internal TempStorageRegTransactionFilterControl TransactionsFilterControl;
}
