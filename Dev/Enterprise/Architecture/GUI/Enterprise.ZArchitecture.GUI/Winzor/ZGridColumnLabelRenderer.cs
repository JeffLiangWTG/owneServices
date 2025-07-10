using CargoWise.Windows.UI;
namespace Enterprise.ZArchitecture.GUI.Grid
{
	internal class ZGridColumnLabelRenderer : ControlExtension, IAutomaticLabelExtension
	{
		public void Refresh()
		{
			//not implementing any functionality, but class is referenced in several places throughout Winzor and in Tests. removal will cause errors in PAVE and Enterprise.Customs
		}
	}
}
