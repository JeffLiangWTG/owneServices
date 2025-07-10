using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class PlaceOfUseOrProcessingGoodsLocationUserControl : ZUserControl
	{
		public PlaceOfUseOrProcessingGoodsLocationUserControl()
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
			PlacesOfUseOrProcessingGroupBox.CaptionResourceString = GroupBoxCaption();
			PlacesOfUseOrProcessingGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("54B097BA-B8B7-454F-A814-68C87A82F72C", "Places of Use or Processing");
	}
}
