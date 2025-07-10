using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class LocationOfGoodsUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember, ITopLevelDataSourceType
	{
		public LocationOfGoodsUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void MoreButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is ICusGoodsLocationProvider provider)
			{
				if (provider.GoodsLocation is CusGoodsLocation goodsLocation)
				{
					using (var form = GetCusGoodsLocationForm(provider))
					{
						goodsLocation.BeginEdit();
						ZFormModaliser.ShowDialogAndDispose(form);
					}
					LocationOfGoodsDescription.Text = provider.GoodsLocationDescription;
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("cb61d000-8636-476d-8d9e-438fefa58e71", "The Location of Goods is currently being edited by another user. Please try later."));
				}
			}
		}

		protected virtual CusGoodsLocationForm GetCusGoodsLocationForm(ICusGoodsLocationProvider provider) => new CusGoodsLocationForm(provider);

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(ICusGoodsLocationProvider.GoodsLocationDescription);

		Type ITopLevelDataSourceType.DataSourceType => DesignModeFinder.IsDesigning ? DataSourceType : CurrentDataItem?.GetType() ?? CusGoodsLocationProviderType ?? DataSourceType;

		[Browsable(false)]
		public Type CusGoodsLocationProviderType { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
