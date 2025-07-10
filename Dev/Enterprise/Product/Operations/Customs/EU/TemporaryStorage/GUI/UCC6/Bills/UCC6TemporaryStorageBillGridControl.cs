using System;
using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public partial class UCC6TemporaryStorageBillGridControl : ZUserControl
{
	public UCC6TemporaryStorageBillGridControl()
	{
		InitializeComponent();
		UpdateGridColumnLayout();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		if (TemporaryStorageBillGrid.ContextMenu != null)
		{
			TemporaryStorageBillGrid.ContextMenu.Popup += ContextMenu_Popup;
		}
		TemporaryStorageBillGrid.RowsDeleting += BillsGrid_RowDelete;

		if (Header != null)
		{
			Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
			Header.AMA_MessageTypeInfo.ValueChanged += TemporaryStorageHeader_AMA_MessageTypeChanged;
			UpdateGridColumnLayout();
			TemporaryStorageHeader_AMA_MessageTypeChanged(this, EventArgs.Empty);
		}
	}

	void UpdateGridColumnLayout()
	{
		var gridColumnLayoutProvider = LayoutProvider
			?.GetTemporaryStorageGridColumnLayoutProviderFactory()
			?.CreateTemporaryStorageGridColumnLayoutProviderForBill() ?? new UCC6TemporaryStorageBillGridColumnLayout();

		TemporaryStorageBillGrid.ApplyGridColumnLayout(gridColumnLayoutProvider);
	}

	void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs args)
	{
		if (Header != null)
		{
			TemporaryStorageBillGrid.SetAvailability(!Header.IsTransfer, AsycudaBillSchema.Constants.ABL_UCRNumber);
			TemporaryStorageBillGrid.SetAvailability(!Header.IsTransfer, nameof(TemporaryStorageBill.ConsignorOrgPK));
			TemporaryStorageBillGrid.SetAvailability(!Header.IsTransfer, AsycudaBillSchema.Constants.ABL_OA_Shipper);
			TemporaryStorageBillGrid.SetAvailability(!Header.IsTransfer, nameof(TemporaryStorageBill.ConsigneeOrgPK));
			TemporaryStorageBillGrid.SetAvailability(!Header.IsTransfer, AsycudaBillSchema.Constants.ABL_OA_Consignee);
		}
	}

	TemporaryStorageHeader Header => CurrentDataItem as TemporaryStorageHeader;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (TemporaryStorageBillGrid?.ContextMenu != null)
			{
				TemporaryStorageBillGrid.ContextMenu.Popup -= ContextMenu_Popup;
				TemporaryStorageBillGrid.RowsDeleting -= BillsGrid_RowDelete;
			}
			if (components != null)
			{
				components.Dispose();
			}
		}

		if (Header != null)
		{
			Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
		}

		base.Dispose(disposing);
	}

	void ContextMenu_Popup(object sender, EventArgs eventArgs)
	{
		TemporaryStorageBill currentTemporaryStorageBill = (TemporaryStorageBill)TemporaryStorageBillGrid?.ListManager?.GetCurrent();
		var isCurrentRowMaster = currentTemporaryStorageBill?.ABL_Calc_IsMaster ?? true;
		var isAnySelectedRowMaster = TemporaryStorageBillGrid.GetSelectedRows().Cast<TemporaryStorageBill>().Any(b => b.ABL_Calc_IsMaster);
		TemporaryStorageBillGrid.DeleteMenuItem.Enabled = !isCurrentRowMaster && !isAnySelectedRowMaster;
	}

	void BillsGrid_RowDelete(object sender, RowsDeletingEventArgs e)
	{
		e.Cancel = e.Objects.Cast<TemporaryStorageBill>().Any(b => b.ABL_Calc_IsMaster);
	}

	ITemporaryStorageLayoutProvider LayoutProvider => fLayoutProvider ??= TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header);
	ITemporaryStorageLayoutProvider fLayoutProvider;
}
