using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPNumbersUserControl : ZUserControl
	{
		public RFPNumbersUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			declaration = ((JobDeclaration)BindingSource.Current);
			declaration.AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo.ValueChanged += ZA_IsAQISCertificateRequest_HiddenInfo_ValueChanged;
			SetRFPNumbersVisibility();
		}

		#region RFP Numbers Visibility

		void ZA_IsAQISCertificateRequest_HiddenInfo_ValueChanged(object sender, EventArgs e)
		{
			SetRFPNumbersVisibility();
		}

		void SetRFPNumbersVisibility()
		{
			bool isVisible = declaration.IsAQISCertificateRequest;
			RFPNumbersPanel.Visible = isVisible;
			RFPNumbersUnavailableLabel.Visible = !isVisible;
		}

		JobDeclaration declaration;

		#endregion
	}
}
