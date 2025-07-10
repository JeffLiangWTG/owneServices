using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class Phase5GuaranteesUserControl : EU.NCTS.GUI.Phase5GuaranteesUserControl
	{
		public Phase5GuaranteesUserControl()
		{
			InitializeComponent();
		}

		protected override void AddAndRemoveColumns()
		{
			base.AddAndRemoveColumns();

			var bondNumber2Info = GuaranteesGrid.GetColumnStyle(AutoCusBondDetail.Schema.PW_BondNumber2);
			bondNumber2Info.IsVisible = false;
		}
	}
}
