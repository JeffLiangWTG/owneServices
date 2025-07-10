using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GeographyEditTestClass : ZGeographyEdit
	{
		public GeographyEditTestClass() { }

		public string LayoutErrors
		{
			get
			{
				var result = "";
				var newLine = System.Environment.NewLine;

				if (GeographyTextBox.TabIndex != 0)
				{
					result += "GeographyTextBox.TabIndex should be 0" + newLine;
				}

				if (GeographyTextBox.TextAlign != HorizontalAlignment.Left)
				{
					result += "Text should be left-aligned" + newLine;
				}

				return result;
			}
		}
	}
}
