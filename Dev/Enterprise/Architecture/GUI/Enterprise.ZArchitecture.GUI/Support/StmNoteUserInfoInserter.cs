using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Adds UserInitials and DateTime to the NoteTextBox supplied in the constructor.
	/// </summary>
	public static class StmNoteUserInfoInserter
	{
		public static void RegisterHotkeys(ZTextBox noteTextBox)
		{
			noteTextBox.Hotkeys.RegisterHotKey(Keys.F5, InsertUserInfoIntoZTextBox, Res.GetString("AA84A7A7-AD9B-4E62-9F45-7024EFD0372C", "Insert User Info"));
		}

		public static void RegisterHotkeys(ZRichTextBox richEdit)
		{
			richEdit.Hotkeys.RegisterHotKey(Keys.F5, InsertUserInfoIntoRichTextBox, Res.GetString("AA84A7A7-AD9B-4E62-9F45-7024EFD0372C", "Insert User Info"));
		}

		#region Implementation

		public static string GetUserText()
		{
			return RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.Value ? EnvProxy.Instance.CurrentUser.InitialsAndDateTimeGmt : EnvProxy.Instance.CurrentUser.InitialsAndDateTime;
		}

		static bool InsertUserInfoIntoRichTextBox(object sender, Keys keyPressed)
		{
			var richEdit = (ZRichTextBox)sender;

			#if !WINZOR
			var oldStart = richEdit.SelectionStart;
			#endif
			var userText = GetUserText();

			richEdit.InsertObject(userText);
			#if !WINZOR
			richEdit.SelectionStart = oldStart + userText.Length;
			#endif 
			return true;
		}

		static bool InsertUserInfoIntoZTextBox(object sender, Keys keysPressed)
		{
			var noteTextBox = (TextBoxBase)sender;

			var oldStart = noteTextBox.SelectionStart;
			var startText = noteTextBox.Text.Substring(0, noteTextBox.SelectionStart);

			var userText = GetUserText();
			var endText = noteTextBox.Text.Substring(noteTextBox.SelectionStart + noteTextBox.SelectionLength);

			noteTextBox.Text = startText + userText + endText;
			noteTextBox.SelectionStart = oldStart + userText.Length;

			return true;
		}

		#endregion
	}
}
