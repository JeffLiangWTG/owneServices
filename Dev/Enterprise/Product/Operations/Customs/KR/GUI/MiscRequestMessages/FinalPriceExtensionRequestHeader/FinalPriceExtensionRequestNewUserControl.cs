using System;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class FinalPriceExtensionRequestNewUserControl : ZUserControl
	{
		public FinalPriceExtensionRequestNewUserControl()
		{
			InitializeComponent();
		}

		public new FinalPriceReportByDateExtensionHeader CurrentDataItem => base.CurrentDataItem as FinalPriceReportByDateExtensionHeader;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetFinalPriceExtensionRequestHeaderLayouts();
		}

		public void SetFinalPriceExtensionRequestHeaderLayouts()
		{
			FinalPriceExtensionRequestHeaderPanel.UpdateLayout(new FinalPriceExtensionRequestNewLayout());
		}
	}
}
