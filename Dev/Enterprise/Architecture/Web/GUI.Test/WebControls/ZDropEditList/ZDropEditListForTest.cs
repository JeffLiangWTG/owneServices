using System;
using System.Web.UI.HtmlControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDropEditListForTest : ZDropEditList
	{
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

		public string SelectItemHandlerForTest
		{
			get { return SelectItemHandler; }
		}

		public string KeysHandlerForTest
		{
			get { return KeysHandler; }
		}

		public string ButtonClickHandlerForTest
		{
			get { return ButtonClickHandler; }
		}

		public ZWebResource ScriptFileForTest
		{
			get { return ScriptFile; }
		}

		public HtmlInputButton ButtonControlForTest
		{
			get { return ButtonControl; }
		}

		public string PopupIDForTest
		{
			get { return PopupID; }
		}
	}
}
