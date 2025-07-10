using System.Windows.Forms;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class SchemeManagerForTest : GridColourSchemeManager
	{
		public SchemeManagerForTest(ZGrid grid)
			: base(grid)
		{
		}

		protected override GridColourScheme ShowManageSchemeForm(object sender)
		{
			var scheme = (GridColourScheme)((MenuItem)sender).Tag;
			scheme.ColourStrips[0].BGColor = System.Drawing.Color.FromArgb(666);
			scheme.ColourStrips[0].RuleName = "rule";
			foreach (var strip in scheme.ColourStrips)
			{
				strip.SaveLayout(strip.RuleName);
			}
			scheme.Factory.Save();
			return scheme;
		}
	}
}
