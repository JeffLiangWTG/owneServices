using System.Drawing;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ZManifestMessageHistoryUserControl : Customs.GUI.ZManifestMessageHistoryUserControl
	{
		public ZManifestMessageHistoryUserControl()
		{
			InitializeComponent();
		}

		protected override ColorPair GetColorsForStatus(string status)
		{
			ColorPair result = base.GetColorsForStatus(status);

			if (status == Declaration.Business.ManifestStatus.Idle.AsString)
			{
				result.BackColor = Color.LightYellow;
				result.ForeColor = Color.LightCoral;
			}

			return result;
		}
	}
}
