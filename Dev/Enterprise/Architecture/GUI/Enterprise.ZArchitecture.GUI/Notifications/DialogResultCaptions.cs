using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public static class DialogResultCaptions
	{
		public static string GetCaptionForDialogResult(DialogResult result)
		{
			switch (result)
			{
				case DialogResult.Abort: return Res.GetString("MessageBoxButton|Abort", "&Abort");
				case DialogResult.Cancel: return Res.GetString("MessageBoxButton|Cancel", "&Cancel");
				case DialogResult.Ignore: return Res.GetString("MessageBoxButton|Ignore", "&Ignore");
				case DialogResult.No: return Res.GetString("MessageBoxButton|No", "&No");
				case DialogResult.OK: return Res.GetString("MessageBoxButton|Ok", "&OK");
				case DialogResult.Retry: return Res.GetString("MessageBoxButton|Retry", "&Retry");
				case DialogResult.Yes: return Res.GetString("MessageBoxButton|Yes", "&Yes");
				default: return string.Empty;
			}
		}

		public static string GetTextForDialogResult(DialogResult result) => CaptionFormatter.WithoutHotKeyFormatting(GetCaptionForDialogResult(result));
	}
}
