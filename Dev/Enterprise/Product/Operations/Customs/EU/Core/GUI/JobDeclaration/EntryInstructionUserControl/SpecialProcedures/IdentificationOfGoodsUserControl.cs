using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class IdentificationOfGoodsUserControl : ZUserControl
	{
		public IdentificationOfGoodsUserControl()
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
			IdentificationofGoodsSubGroupBox.CaptionResourceString = GetIdentificationofGoodsGroupSubBoxCaption();
			ProcessedProductsGroupBox.CaptionResourceString = GetProcessedProductsGroupBoxCaption();
			IdentificationofGoodsSubGroupBox.RefreshCaptionLabel();
			ProcessedProductsGroupBox.RefreshCaptionLabel();
		}

		protected virtual ResourceStringData GetIdentificationofGoodsGroupSubBoxCaption() => Res.GetData("4D772B0E-65CB-43DA-A906-8BCA3855137E", "Identification Of Goods");

		protected virtual ResourceStringData GetProcessedProductsGroupBoxCaption() => Res.GetData("569861DD-5564-4F64-A282-4A99618839C5", "Processed Products");
	}
}
