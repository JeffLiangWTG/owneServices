using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class DestinationLocationOfGoodsUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember, ITopLevelDataSourceType
	{
		public DestinationLocationOfGoodsUserControl() : base()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		void MoreButton_Click(object sender, EventArgs e)
		{
			var header = CurrentDataItem as TemporaryStorageHeader;
			var goodsLocation = header.DestinationGoodsLocation;
			if (goodsLocation != null)
			{
				using (var form = GetCusGoodsLocationForm(header))
				{
					goodsLocation.BeginEdit();
					ZFormModaliser.ShowDialogAndDispose(form);
				}
				LocationOfGoodsDescription.Text = header.DestinationGoodsLocationDescription;
			}
		}

		protected DestinationCusGoodsLocationForm GetCusGoodsLocationForm(TemporaryStorageHeader header) => new DestinationCusGoodsLocationForm(header);

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(TemporaryStorageHeader.DestinationGoodsLocationDescription);

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
