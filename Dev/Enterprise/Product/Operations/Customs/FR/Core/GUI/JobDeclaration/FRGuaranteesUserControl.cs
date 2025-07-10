
namespace Enterprise.Customs.FR.GUI
{
	public partial class FRGuaranteesUserControl : EU.NCTS.GUI.GuaranteesUserControl
	{
		public FRGuaranteesUserControl()
		{
		}

		protected override void AddAndRemoveColumns()
		{
			base.AddAndRemoveColumns();
			var codDetailColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo("CusGuarantee+CustomsGuaranteeFriendlyNameForDeltaT", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80));
			codDetailColumnStyleInfo.CaptionResourceString = Res.GetData("D0750572-596E-470C-A95C-4B163D65D4AA", "COD Detail");
			codDetailColumnStyleInfo.IsVisible = true;
			GuaranteesGrid.ColumnStyles.Add(codDetailColumnStyleInfo);
		}
	}
}
