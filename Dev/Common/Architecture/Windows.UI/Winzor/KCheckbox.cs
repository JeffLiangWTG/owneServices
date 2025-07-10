using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	public partial class KCheckBox
	{
		public override string ToolTipText
		{
			get
			{
				if (!AutoEllipsis && Enabled)
				{
					var caption = IsCaptionOverridden ? Text : Captions?.OrderLongToShort().FirstOrDefault() ?? Text;
					if (!string.IsNullOrEmpty(caption) && caption.Any(char.IsLetterOrDigit))
					{
						var fixedMnemonic = Regex.Replace(caption, "&(?!&)", "");
						return fixedMnemonic;
					}
				}
				return null;
			}
		}
	}
}
