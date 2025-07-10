using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class TariffDetachUserControl : ZUserControl
	{
		public TariffDetachUserControl()
		{
			InitializeComponent();
		}

		protected void TariffDetach_Button_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is ITariffDetachParent parent)
			{
				TariffDetachCollectionForm.ShowDialog(parent.TariffDetachs);
				parent.TariffDetachConcatenatedInfo.RefreshBinding();
			}
		}
	}
}
