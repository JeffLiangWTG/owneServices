using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class PeriodForDischargeUserControl : ZUserControl
	{
		public PeriodForDischargeUserControl()
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
			PeriodForDischargeGroupBox.CaptionResourceString = GroupBoxCaption();
			PeriodForDischargeGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("78848C66-1538-49BF-9F82-0AC9B0B77C2D", "Period for Discharge");
	}
}
