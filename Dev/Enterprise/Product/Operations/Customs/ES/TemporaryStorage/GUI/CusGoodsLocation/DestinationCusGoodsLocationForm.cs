using System;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class DestinationCusGoodsLocationForm : ZChildForm
	{
		public DestinationCusGoodsLocationForm(TemporaryStorageHeader header) : base(Argument.NotNull(header, nameof(header)).DestinationGoodsLocation)
		{
			Header = header;
			UpdateDynamicGoodsLocationPanelLayout();
		}

		public TemporaryStorageHeader Header { get; }

		void UpdateDynamicGoodsLocationPanelLayout()
		{
			var layout = GetGoodsLocationLayout();
			if (layout != null)
			{
				DynamicGoodsLocationPanel.UpdateLayout(layout);
			}
		}

		IPanelLayoutProvider GetGoodsLocationLayout() => GoodsLocationFormLayoutProvider.GetGoodsLocationLayout();

		void OKButton_Click(object sender, EventArgs e)
		{
			Header.DestinationGoodsLocationDescriptionInfo?.RefreshBinding();
			if (Header.DestinationGoodsLocation.HasErrors)
			{
				Globals.Message.ShowInformation(Res.GetString("39B29F18-1B1A-4B03-B0BF-2BE92D4F69CA", "Please resolve all errors before saving."));
			}
			else
			{
				Close();
			}
		}

		protected IGoodsLocationFormLayoutProvider GoodsLocationFormLayoutProvider
		{
			get
			{
				if (goodsLocationFormLayoutProvider is null)
				{
					goodsLocationFormLayoutProvider = EU.GUI.GoodsLocationFormLayoutProvider.GetLayoutProvider(Header);
				}
				return goodsLocationFormLayoutProvider;
			}
		}

		IGoodsLocationFormLayoutProvider goodsLocationFormLayoutProvider;
	}
}
