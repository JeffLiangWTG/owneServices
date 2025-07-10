using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class FirstPlaceOfUseOrProcessingUserControl : ZUserControl
	{
		public FirstPlaceOfUseOrProcessingUserControl()
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
			FirstPlaceOfUseOrProcessingGroupBox.CaptionResourceString = GroupBoxCaption();
			FirstPlaceOfUseOrProcessingGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("69509A43-C8D0-4741-B927-3E3B60877248", "First Place of Use or Processing", "[Annex A 4/5] Dates, Times, Periods and Places > First Place of Use or Processing");
	}
}
