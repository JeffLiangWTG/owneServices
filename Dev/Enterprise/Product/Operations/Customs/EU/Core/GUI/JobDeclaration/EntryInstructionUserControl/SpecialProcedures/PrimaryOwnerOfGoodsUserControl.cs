using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class PrimaryOwnerOfGoodsUserControl : ZUserControl
	{
		public PrimaryOwnerOfGoodsUserControl()
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
			PrimaryOwnerOfGoodsGroupBox.CaptionResourceString = GroupBoxCaption();
			PrimaryOwnerOfGoodsGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GroupBoxCaption() => Res.GetData("CDB82074-2BB8-4719-BEB9-AEB67714524A", "Primary Owner of Goods", "[Annex A 3/8] Parties > Owner of the Goods");
	}
}
