using System;
using System.Linq;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.DE.GUI
{
	public partial class REGLineToConsolidateUserControl : ZUserControl
	{
		public REGLineToConsolidateUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var style = (ZCodeFindBoxWithSelectedEventColumnStyleInfo)LinesGrid.GetColumnStyle(nameof(PRLCONCusTempStorageLineToConsolidate.FormattedReferenceNumber));
			style.Selected += ZCodeFindBoxWithSelectedEvent_Selected;
		}

		void ZCodeFindBoxWithSelectedEvent_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var selectedRegLine = e.SelectedBusinessObjects.Cast<CusTempStorageRegLine>().Single();
			var currentListItem = LinesGrid.ListManager.GetCurrent() as CusTempStorageLine;

			if (currentListItem != null)
			{
				currentListItem.TSL_PackageQty = selectedRegLine.SRL_PackagesRemaining;
				currentListItem.TSL_ReferenceNumberLine = selectedRegLine.SRL_LineNumber;
			}
		}
	}
}
