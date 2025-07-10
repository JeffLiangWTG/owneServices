using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTimeEditListForTest : ZTimeEditList
	{
		public ZTimeEditListForTest(ZDateEditBox relatedControl)
			: base(relatedControl)
		{
		}

		public ZTextBox TextBoxControlForTest
		{
			get { return TextBoxControl; }
		}

		public ZMultiTextListBox ListBoxControlForTest
		{
			get { return ListBoxControl; }
		}

		public void OnPreRenderForTest()
		{
			OnPreRender(EventArgs.Empty);
		}

		public string NormalizeAndValidateTimeHandlerForTest
		{
			get { return NormalizeAndValidateTimeHandler; }
		}

		public string RelatedControlIDForTest
		{
			get { return RelatedControlID; }
		}

		public string ButtonBackgroundStyleForTest
		{
			get { return ButtonBackgroundStyle; }
		}

		public ZWebResource ZTimeEditListScriptFileForTest
		{
			get { return ZTimeEditListScriptFile; }
		}

		public ZWebResource ZTimeEditListButtonImageForTest
		{
			get { return ZTimeEditListButtonImage; }
		}
	}
}
