using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn
{
	public static class DocDataPlugInHelper
	{
		public static ZLabel GetHintLabel()
		{
			var hintLabel = new ZLabel();
			hintLabel.CaptionResourceString = Res.GetData("31729AE4-C902-486E-980F-18FF9FB1EC64", "Another user is currently accessing this document note.");
			hintLabel.FontType = OFontTypes.Normal | OFontTypes.SansSerif;
			hintLabel.ForeColor = System.Drawing.Color.Red;
			hintLabel.Location = ControlDpiScalingHelper.NewScaledPoint(500, 0, true);
			hintLabel.Name = "HintLabel";
			hintLabel.Size = ControlDpiScalingHelper.NewScaledSize(366, 13, true);
			hintLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			hintLabel.Visible = false;
			return hintLabel;
		}
	}
}
