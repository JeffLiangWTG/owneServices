using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	public partial class KButton
	{
		public override string ToolTipText
		{
			get
			{
				if (Enabled)
				{
					// adapted from https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FCommon%2FArchitecture%2FWindows.UI%2FControlLabels%2FButtonBaseVariableLengthCaptionRenderer.cs&version=GBmaster&_a=contents
					if (!AutoEllipsis)
					{
						var caption = CaptionRenderer.IsCaptionOverridden ? Text : CaptionRenderer.ToolTipCaption ?? CaptionRenderer.Captions?.OrderLongToShort().FirstOrDefault() ?? Text;
						if (!string.IsNullOrEmpty(caption) && caption.Any(char.IsLetterOrDigit))
						{
							var fixedMnemonic = Regex.Replace(caption, "&(?!&)", "");
							if (base.ToolTipText is null || CaptionRenderer.HasAutoSetToolTip)
							{
								CaptionRenderer.HasAutoSetToolTip = false;
								base.ToolTipText = fixedMnemonic;
							}
						}
					}
					return base.ToolTipText;
				}
				return null;
			}
		}

#if DEBUG
		public ButtonBaseVariableLengthCaptionRenderer CaptionRendererExposedForTest => CaptionRenderer;
#endif
	}
}
