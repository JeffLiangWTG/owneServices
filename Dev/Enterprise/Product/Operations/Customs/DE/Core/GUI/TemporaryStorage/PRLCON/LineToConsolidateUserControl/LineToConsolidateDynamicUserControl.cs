using System;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class LineToConsolidateDynamicUserControl : ZUserControl
	{
		public LineToConsolidateDynamicUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var storageDec = CurrentDataItem as PRLCONCusTempStorageDec;
			if (storageDec != null)
			{
				storageDec.STH_IdentificationIndicatorInfo.ValueChanged -= OnSTH_IdentificationIndicatorChanged;
				storageDec.STH_IdentificationIndicatorInfo.ValueChanged += OnSTH_IdentificationIndicatorChanged;
				OnSTH_IdentificationIndicatorChanged(null, null);
			}
		}

		void OnSTH_IdentificationIndicatorChanged(object sender, EventArgs e)
		{
			var storageDec = CurrentDataItem as PRLCONCusTempStorageDec;
			if (storageDec != null && storageDec.Lookups.IdentificationIndicatorList.ContainsCode(storageDec.STH_IdentificationIndicator))
			{
				var isAWBDeclaration = storageDec.IsAWBDeclaration;
				AWBLineToConsolidateUserControl.Visible = isAWBDeclaration;
				REGLineToConsolidateUserControl.Visible = !isAWBDeclaration;
			}
		}

		protected override void Dispose(bool disposing)
		{
			var storageDec = DataSource as PRLCONCusTempStorageDec;
			if (disposing && storageDec != null)
			{
				storageDec.STH_IdentificationIndicatorInfo.ValueChanged -= OnSTH_IdentificationIndicatorChanged;
			}

			base.Dispose(disposing);
		}
	}
}
