using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class InPlaceDynamicContentControl : ZUserControl, IHaveHelpBar
	{
		public InPlaceDynamicContentControl(IEditorPresenter handler)
		{
			Argument.NotNull(handler, nameof(handler));

			this.handler = handler;

			this.OnTabSelectNextControl += (s, e) =>
			{
				e.Handled = true;
			};
		}

		readonly IEditorPresenter handler;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Shift | Keys.Tab))
			{
				handler.MoveToPreviousEditor();
				return true;
			}

			if (keyData == Keys.Tab)
			{
				handler.MoveToNextEditor();
				return true;
			}

			if (keyData == Keys.Enter && ModifierKeys == Keys.None)
			{
				handler.CommitChanges();
				return true;
			}

			if (keyData == Keys.Escape)
			{
				handler.CancelChanges();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		Control InnerControl
		{
			get { return innerControl ?? (innerControl = Controls.Cast<Control>().FirstOrDefault()); }
		}

		Control innerControl;

		#region IHaveHelpBar

		HelpBarUserControl IHaveHelpBar.HelpBar { get; set; }

		IEnumerable<KeyValuePair<string, string>> IHaveHelpBar.KeyboardHints
		{
			get
			{
				if (keyboardHints == null)
				{
					keyboardHints = new List<KeyValuePair<string, string>>();
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.Enter, KeyboardHints.Hints.Enter));
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.Escape, KeyboardHints.Hints.Escape));
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.TabForward, KeyboardHints.Hints.TabForward));
					keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.TabBackward, KeyboardHints.Hints.TabBackward));

					var textBox = InnerControl as ZTextBox;

					if (textBox != null && textBox.Multiline)
					{
						keyboardHints.Add(new KeyValuePair<string, string>(KeyboardHints.Shortcuts.NewLine, KeyboardHints.Hints.NewLine));
					}
				}

				return keyboardHints;
			}
		}

		List<KeyValuePair<string, string>> keyboardHints;

		#endregion
	}
}