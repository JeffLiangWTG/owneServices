using System.Windows.Forms;

namespace Enterprise.ResourceStrings.Module.TranslationFeedback.Testing
{
	class TranslationFeedbackModuleForTest : TranslationFeedbackModule
	{
		public MenuItem[] GetNewStandardMenuItemsExposed()
		{
			return base.GetNewStandardMenuItems();
		}
	}
}
