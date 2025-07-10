using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class GoodsItemsGridUserControl : ZUserControl, ISupportingInfoUserControls
	{
		public GoodsItemsGridUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			GoodsItemsGrid.SetColumnCaption(NctsCommonCargoDesc.Schema.BY_MonetaryValue, !DesignModeFinder.IsDesigning ? MovementHeader.CustomsValueCaption : (NoResString)ZString.Empty);
			(GoodsItemsGrid.GetColumnStyle(NctsCommonCargoDesc.Schema.BY_FormattedHarmonisedTariff) as TariffColumnStyleInfo).GetEffectiveDate = () => MovementHeader?.BM_ValuationDate ?? ZDateTime.Today;
		}

		void GoodsItemsGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			var showDeleteConfirmation = NctsHeader.Configuration.GoodsItemsConfiguration.DeleteConfirmationSupport(NctsHeader);

			if (showDeleteConfirmation)
			{
				var message = Res.GetString("5256BF5A-79DB-4CF6-876F-63FA47E096E3", "Confirm the deletion of the selected Goods item row/rows?");
				var caption = Res.GetString("90795698-CE5B-4906-9591-D9491A057B9F", "Confirm");

				e.Cancel = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;
			}
		}

		string ISupportingInfoUserControls.GridBindingMember => ".";
		ZGrid ISupportingInfoUserControls.Grid => GoodsItemsGrid;

		NctsDepartureMovementHeader MovementHeader => NctsHeader.MovementHeader;

		NctsHeader NctsHeader => (NctsHeader)DataSource;
	}
}
