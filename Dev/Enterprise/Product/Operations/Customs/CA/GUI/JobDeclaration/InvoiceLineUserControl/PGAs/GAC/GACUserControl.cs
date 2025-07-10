using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("GACPGAHeader")]
	public partial class GACUserControl : ZUserControl
	{
		public GACUserControl()
		{
			InitializeComponent();
			InitializeLazyCreate();
			LPCODetailUserControl.HideIssuanceCountry();
			LPCODetailUserControl.HideCountryOfAuth();
			LPCODetailUserControl.HideMixed();
			LPCODetailUserControl.HideExpiryDate();
		}

		protected void InitializeLazyCreate()
		{
			LPCOGridUserControl.RemoveExceptAvailableColumns(GACPGAHeader.AvailableLPCOFields);
		}

		public new GACPGAHeader CurrentDataItem => base.CurrentDataItem as GACPGAHeader;

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			ClothingAndTextileDetailsGroup.DataBindings.RemoveBinding(IsVisibleForBindingString);
			TPLPermitLabel.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				ClothingAndTextileDetailsGroup.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, GACPGAHeader.Schema.AreClothingAndTextileDetailsVisibility, false, System.Windows.Forms.DataSourceUpdateMode.Never));
				TPLPermitLabel.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, GACPGAHeader.Schema.IsFTAProcessingCodeFA01, false, System.Windows.Forms.DataSourceUpdateMode.Never));
			}

			GACSplitContainer.SplitterDistance = ClothingAndTextileDetailsGroup.Visible ? 140 : 70;
		}
	}
}
