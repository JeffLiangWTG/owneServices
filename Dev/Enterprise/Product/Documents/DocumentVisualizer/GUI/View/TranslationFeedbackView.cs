using System.Windows.Forms;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	static class TranslationFeedbackView
	{
		public static void OpenFeedbackForm(Control control, string unquotedMacro)
		{
			if (control == null
				|| string.IsNullOrWhiteSpace(unquotedMacro))
			{
				return;
			}

			var caption = unquotedMacro.GetResStringCaption();
			var key = caption.GetResStringKey();

			if (string.IsNullOrWhiteSpace(key))
			{
				var message = Res.GetString("4612f866-d769-407f-b24f-279500edfafc", "The following macro cannot be translated: {0}", unquotedMacro);
				Globals.Message.ShowInformation(message);
				return;
			}

			var res = Res._GetData(ResourceStringExtensions.Asmid, key, caption);

			if (res == null)
			{
				var message = Res.GetString("0953fc67-37a4-4759-a877-01588aed20d6", "Could not find resource sting with key: {0} and caption: {1}", key, caption);
				Globals.Message.ShowInformation(message);
				return;
			}

			TranslationFeedbackManager.OpenFeedbackForm(control, res);
		}
	}
}
