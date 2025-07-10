using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.GenericCollection;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Popups;

public partial class FilterPopupForm : ZChildForm
{
	public FilterPopupForm(IFilterableCollection filterableCollection) : base(filterableCollection)
	{
		var filterControl = new ZFilterStripBaseControl(filterableCollection.FilterObject);
		filterControl.Dock = DockStyle.Fill;
		filterControl.TabIndex = 1;
		Controls.Add(filterControl);

		ApplyBtn.AllowOverlap(filterControl);
		CancelBtn.AllowOverlap(filterControl);
	}

	void ApplyBtn_Click(object sender, System.EventArgs e)
	{
		((IFilterableCollection)BusinessEntity).ApplyFilter();
	}
}
