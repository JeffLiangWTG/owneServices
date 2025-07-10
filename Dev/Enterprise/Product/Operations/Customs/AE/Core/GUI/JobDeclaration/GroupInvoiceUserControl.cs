using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AE.GUI;

public partial class GroupInvoiceUserControl : BaseInvoiceGroupingUserControl
{
	public GroupInvoiceUserControl()
	{
		InitializeComponent();
	}

	void GroupChargeGrid_AfterBind(object sender, EventArgs e)
	{
		GroupChargeGrid.RemoveFromAvailableColumns(BaseGroupInvoiceCharge.Schema.J7_IsDutiable);
		GroupChargeGrid.SetColumnCaption(BaseGroupInvoiceCharge.Schema.J7_IsGSTApplicable, Res.GetString("D3650CEB-6615-4437-82F6-BD1A28F660DA", "Dutiable"));
	}
}
