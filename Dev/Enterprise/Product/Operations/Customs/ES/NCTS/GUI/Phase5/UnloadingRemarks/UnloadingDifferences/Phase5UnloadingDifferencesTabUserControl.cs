using System;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Phase5UnloadingDifferencesTabUserControl : EU.NCTS.GUI.Phase5UnloadingDifferencesTabUserControl
	{
		public Phase5UnloadingDifferencesTabUserControl() : base() { }

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (DataSource is NctsHeader header)
				{
					header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifierInfo.ValueChanged -= ArrivalGoodsLocationValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (DataSource is NctsHeader header)
			{
				header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifierInfo.ValueChanged -= ArrivalGoodsLocationValueChanged;
				header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifierInfo.ValueChanged += ArrivalGoodsLocationValueChanged;
				ArrivalGoodsLocationValueChanged(null, null);
			}
		}

		protected void ArrivalGoodsLocationValueChanged(object sender, EventArgs e)
		{
			if (DataSource is NctsHeader header)
			{
				GuaranteeGroupBox.Visible = header.ArrivalMovementHeader.ShouldGuaranteeForArrivalBeVisible;
			}
		}
	}
}
