using System;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class REXDISDeclarationUserControl : ZUserControl
	{
		public REXDISDeclarationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			var dec = CusTempStorageJobHeader?.REXDISCusTempStorageDec;
			if (dec != null)
			{
				dec.STH_IdentificationIndicatorInfo.ValueChanged -= STH_IdentificationIndicator_ValueChanged;
			}
			base.OnAfterFirstBinding(e);

			if (dec != null)
			{
				dec.STH_IdentificationIndicatorInfo.ValueChanged += STH_IdentificationIndicator_ValueChanged;
				STH_IdentificationIndicator_ValueChanged(this, null);
			}
		}

		CusTempStorageJobHeader CusTempStorageJobHeader
		{
			get { return DataSource as CusTempStorageJobHeader; }
		}

		void STH_IdentificationIndicator_ValueChanged(object sender, EventArgs e)
		{
			var dec = CusTempStorageJobHeader?.REXDISCusTempStorageDec;
			if (dec != null)
			{
				var isAWB_SINDeclaration = dec.IsAWBDeclaration || dec.IsSINDeclaration;
				REXDISREGDeclarationUserControl.Visible = !isAWB_SINDeclaration;
				REXDISAWBDeclarationUserControl.Visible = isAWB_SINDeclaration;

				using (LinesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					if (isAWB_SINDeclaration)
					{
						LinesGrid.RemoveFromAvailableColumns("SumALine+TSL_ReferenceNumberLine", "SumALine+ReferenceNumber");
					}
					else
					{
						LinesGrid.AddToAvailableColumns("SumALine+TSL_ReferenceNumberLine", "SumALine+ReferenceNumber");
					}

					LinesGrid.ReOrderColumns(new string[]
					{
						"TSL_LineNo",
						"SumALine+TSL_ReferenceNumberLine",
						"SumALine+ReferenceNumber",
						"TSL_OwnerReferenceType",
						"TSL_OwnerReferenceNumber",
						"TSL_PackageQty",
						"TSL_DestinationPlace",
						"TSL_CustomsStatus"
					});
				}
			}
		}
	}
}
