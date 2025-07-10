using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class LocationOfGoodsUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember, ITopLevelDataSourceType
	{
		public LocationOfGoodsUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var header = (NctsHeader)DataSource;
			if (header != null)
			{
				LocationOfGoodsDescription.CharacterCasing = header.Configuration.AllowMixedCaseAuthorisationNumbers ? CharacterCasing.Normal : LocationOfGoodsDescription.CharacterCasing;
			}
		}

		public ZArchitecture.ZTextBox LocationOfGoodsDescriptionTextBox => LocationOfGoodsDescription;

		void MoreButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is ICusGoodsLocationProvider provider)
			{
				if (provider.GoodsLocation is EU.Business.CusGoodsLocation goodsLocation)
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
					Globals.Message.ShowError(Res.GetString("0911385A-80A6-42AE-9B1E-7A0075120547", "The Location of Goods is currently being edited by another user. Please try later."));
				}
			}
		}

		protected virtual EU.GUI.CusGoodsLocationForm GetCusGoodsLocationForm(ICusGoodsLocationProvider provider) => new EU.GUI.CusGoodsLocationForm(provider);

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
