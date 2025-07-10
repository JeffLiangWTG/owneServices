using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class BillOfDischargeUserControl : ZUserControl
	{
		public BillOfDischargeUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetCaption();
		}

		protected void SetCaption()
		{
			BillOfDischargeGroupBox.CaptionResourceString = GroupBoxCaption();
			BillOfDischargeGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("FD1A5487-04A3-4690-8277-FE8382CC0D64", "Bill Of Discharge");
	}
}
