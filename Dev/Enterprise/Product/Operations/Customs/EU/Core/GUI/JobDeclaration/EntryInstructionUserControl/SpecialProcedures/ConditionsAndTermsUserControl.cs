using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class ConditionsAndTermsUserControl : ZUserControl
	{
		public ConditionsAndTermsUserControl()
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
			ConditionsAndTermsGroupBox.CaptionResourceString = GroupBoxCaption();
			ConditionsAndTermsGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("259291c0-321c-4e84-ac8a-cb2c3c93dc07", "Conditions and Terms");
	}
}
