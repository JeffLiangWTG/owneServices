using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace CargoWise.Tools.SpellCheck.GUI
{
	/// <summary>
	/// Override default behaviors of RichTextBox:
	/// * Undo - disable built in undo function, send event instead
	/// * Paste - paste plain text only, without formatting
	/// * TextChanged, SelectionChanged - don't raise events when content is being changed programmatically (with Start/EndChangingContent)
	/// </summary>
	[DesignerSerializer(typeof(Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	[ToolboxItem(false)]
	public class SpellCheckerRichTextBox : System.Windows.Forms.RichTextBox
	{
		public event EventHandler UndoEvent;

		protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == System.Windows.Forms.Keys.Z)
			{
				OnUndo();
				e.Handled = true;
				return;
			}

#if !WINZOR

			if (e.Control && e.KeyCode == System.Windows.Forms.Keys.V)
			{
				OnPaste();
				e.Handled = true;
				return;
			}

#endif

			base.OnKeyDown(e);
		}

		void OnUndo()
		{
			if (UndoEvent != null)
			{
				UndoEvent(this, EventArgs.Empty);
			}
		}

#if !WINZOR

		void OnPaste()
		{
			Paste(System.Windows.Forms.DataFormats.GetFormat(System.Windows.Forms.DataFormats.Text));
		}

#endif

		int changingContent;

		/// <summary>
		/// Indicate that the text box content is being changed programmatically, to disable certain events
		/// </summary>
		public void StartChangingContent()
		{
			changingContent++;
		}

		public void EndChangingContent()
		{
			changingContent--;
		}

		public bool ChangingContent
		{
			get { return changingContent > 0; }
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (!ChangingContent)
			{
				base.OnTextChanged(e);
			}
		}

		protected override void OnSelectionChanged(EventArgs e)
		{
			if (!ChangingContent)
			{
				base.OnSelectionChanged(e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			UndoEvent = null;
			base.Dispose(disposing);
		}
	}
}
