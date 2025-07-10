#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.Tools.SpellCheck.GUI;

namespace CargoWise.Tools.SpellCheck.TestFramework
{
	/// <summary>
	/// Test integration of the spell checker on forms.
	/// To use: 
	/// <code>
	/// using(SpellCheckFormTestHelper helper = new SpellCheckFormTestHelper(Handler))
	/// {
	///     // trigger spellcheck
	///     // Assert helper.DisplayCount
	/// }
	/// 
	/// SpellCheckFormAction Handler(SpellCheckForm form)
	/// {
	///     // return action to handle the spell check form
	/// }
	/// </code>
	/// </summary>
	public class SpellCheckFormTestHelper : IDisposable
	{
		public SpellCheckFormTestHelper(SpellCheckFormTestHandler handler)
		{
			Start(handler);
		}

		SpellCheckFormTestHandler handler;
		int displayCount;

		void Start(SpellCheckFormTestHandler handler)
		{
			SpellCheckerForm.HandleSpellingError += new EventHandler(SpellCheckerForm_HandleSpellingErrorInTest);
			this.handler = handler;
			displayCount = 0;
		}

		void End()
		{
			SpellCheckerForm.HandleSpellingError -= new EventHandler(SpellCheckerForm_HandleSpellingErrorInTest);
			handler = null;
		}

		public void Dispose()
		{
			End();
		}

		public int DisplayCount
		{
			get { return displayCount; }
		}

		void SpellCheckerForm_HandleSpellingErrorInTest(object sender, EventArgs e)
		{
			var form = (ISpellCheckerForm)sender;
			if (form == null)
			{
				return;
			}
			displayCount++;
			try
			{
				var action = handler(form);
				if (action != null)
				{
					handler = action.NextHandler;
					ActOnForm((Form)form, action);
				}
			}
			finally
			{
				form.Dispose();
			}
		}

		void ActOnForm(Form form, SpellCheckFormAction action)
		{
			if (action.SelectSuggestion > 0)
			{
				((ListBox)GetControl(form, "listBoxSuggestions")).SelectedIndex = action.SelectSuggestion;
			}
			else if (action.ManuallyEditedText != null)
			{
				((RichTextBox)GetControl(form, "richTextBoxContext")).Text = action.ManuallyEditedText;
			}

			switch (action.ButtonAction)
			{
				case SpellCheckerFormResult.Cancel:
					InvokeEventHandler(form, "buttonCancel_click");
					break;

				case SpellCheckerFormResult.Ignore:
					InvokeEventHandler(form, "buttonIgnoreUndo_click");
					break;

				case SpellCheckerFormResult.IgnoreAll:
					InvokeEventHandler(form, "buttonIgnoreAll_click");
					break;

				case SpellCheckerFormResult.Change:
				case SpellCheckerFormResult.ChangeManual:
					InvokeEventHandler(form, "buttonChange_click");
					break;

				case SpellCheckerFormResult.ChangeAll:
				case SpellCheckerFormResult.ChangeAllManual:
					InvokeEventHandler(form, "buttonChangeAll_Click");
					break;
			}
		}

		public static void InvokeEventHandler(Form form, string name)
		{
			InvokeEventHandler(form, name, null, null);
		}

		public static void InvokeEventHandler(Form form, string name, object sender, EventArgs args)
		{
			form.GetType().GetMethod(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase)
				.Invoke(form, new object[] { sender, args });
		}

		public static Control GetControl(Form form, string name)
		{
			return form.Controls.Find(name, true)[0];
		}

		public static void FillAllTextBoxes(Control control, bool goodSpelling)
		{
			if (control is TextBoxBase)
			{
				control.Text = goodSpelling ? "a" : "zfoo";
			}
			else
			{
				foreach (Control subcontrol in control.Controls)
				{
					FillAllTextBoxes(subcontrol, goodSpelling);
				}
			}
		}
	}

	public delegate SpellCheckFormAction SpellCheckFormTestHandler(ISpellCheckerForm form);
}

#endif
