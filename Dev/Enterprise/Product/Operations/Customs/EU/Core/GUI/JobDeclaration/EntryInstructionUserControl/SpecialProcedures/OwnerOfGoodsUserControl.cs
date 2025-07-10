using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class OwnerOfGoodsUserControl : ZUserControl
	{
		public OwnerOfGoodsUserControl()
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
			OwnersOfGoodsGroupBox.CaptionResourceString = GroupBoxCaption();
			OwnersOfGoodsGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("023ee088-b5a4-4c4f-a4c7-2179b292c7ac", "Owners of Goods");
	}
}
