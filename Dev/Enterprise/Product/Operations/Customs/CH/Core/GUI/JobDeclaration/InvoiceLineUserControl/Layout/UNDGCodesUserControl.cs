using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class UNDGCodesUserControl : ZUserControl
{
	public UNDGCodesUserControl()
	{
		InitializeComponent();
	}

	void MoreButton_Click(object sender, EventArgs e)
	{
		UndgManager?.ShowMultipleItemForm();
	}

	UNDGDataItemFormManager UndgManager => undgManager ??= GetUndgManager();
	UNDGDataItemFormManager undgManager;

	UNDGDataItemFormManager GetUndgManager()
	{
		UNDGDataItemFormManager undgManager = null;
		var invoiceLineUserControl = GetInvoiceLineUserControl();
		if (invoiceLineUserControl != null)
		{
			undgManager = new UNDGDataItemFormManager(invoiceLineUserControl.CustomsInvoiceLinesBoundGrid, "", UNDGDataItemFormManagerConfig.IMOShowProperties());
			undgManager.Initialize();
		}
		return undgManager;
	}

	BaseInvoiceLineUserControl GetInvoiceLineUserControl()
	{
		var parent = Parent;
		while (parent != null)
		{
			if (parent is BaseInvoiceLineUserControl invoiceLineUserControl)
			{
				return invoiceLineUserControl;
			}
			parent = parent.Parent;
		}
		return null;
	}
}
