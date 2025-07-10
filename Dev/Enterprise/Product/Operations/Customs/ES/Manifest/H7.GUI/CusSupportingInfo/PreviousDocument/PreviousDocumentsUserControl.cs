using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class PreviousDocumentsUserControl : EU.H7.GUI.PreviousDocumentsUserControl
	{
		protected override void CustomizeLayoutCore()
		{
			var zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			PreviousDocumentsGrid.ColumnStyles.Insert(0, zDropEditColumnStyleInfo1);
		}
	}
}
