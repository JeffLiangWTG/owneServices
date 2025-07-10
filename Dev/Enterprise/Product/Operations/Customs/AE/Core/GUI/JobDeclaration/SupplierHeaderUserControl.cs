using System;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AE.GUI;

public partial class SupplierHeaderUserControl : Customs.GUI.LayoutDeclarationInvoiceHeaderUserControl
{
	public SupplierHeaderUserControl()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	void ApportionedChargesGrid_Bound(object sender, EventArgs e)
	{
		ApportionedChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
		ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, DutiableString);
	}

	void InvoiceChargesGrid_AfterBind(object sender, EventArgs e)
	{
		InvoiceChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
		InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, DutiableString);
	}

	void BaseGroupChargesGrid_AfterBind(object sender, EventArgs e)
	{
		BaseGroupChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.J7_IsDutiable.Name);
		BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, DutiableString);
	}

	static string DutiableString => Res.GetString("B339A5D3-6B8A-4C8A-834D-90A7F89308D2", "Dutiable");
}
