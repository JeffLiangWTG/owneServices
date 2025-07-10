using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class EUH7PackUserControl : ZUserControl, IAdditionalTabPage
	{
		public EUH7PackUserControl()
		{
			InitializeComponent();
			UpdateColumnAvailability();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			SetMandatoryColumns();
		}

		public new AsycudaBill CurrentDataItem => (AsycudaBill)base.CurrentDataItem;

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("a51ea044-206f-4382-ae43-dc7cce4212a6", "Packs");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 10;

		void SetMandatoryColumns()
		{
			var header = CurrentDataItem?.Header;
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			using (PacksGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var packsGridMandatoryColumns = provider?.GetPacksGridMandatoryColumns();
				if (packsGridMandatoryColumns != null && packsGridMandatoryColumns.Length > 0)
				{
					foreach (var column in PacksGrid.Columns)
					{
						if (!packsGridMandatoryColumns.Contains(column.ColumnName))
						{
							PacksGrid.SetColumnVisible(false, column.ColumnName);
						}
						else
						{
							PacksGrid.SetColumnMandatory(column.ColumnName, true);
						}
					}
				}
			}
		}

		void UpdateColumnAvailability()
		{
			foreach (var packsGridColumnAvailabilityData in PacksGridColumnAvailability())
			{
				PacksGrid.SetAvailability(packsGridColumnAvailabilityData.Key, packsGridColumnAvailabilityData.Value);
			}
		}

		protected virtual Dictionary<bool, string[]> PacksGridColumnAvailability() => new();
	}
}
