#nullable enable
using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI.Controls.Interfaces;

namespace Enterprise.ZArchitecture.GUI.Internal;

[ToolboxItem(false)]
public partial class ZFilterStripDropCodeBox : ZDropCodeBox
{
	protected new ZFilterStripDropEdit ParentDropEdit
	{
		get { return (ZFilterStripDropEdit)base.ParentDropEdit; }
	}

	bool refreshList;

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (!ReadOnly)
		{
			if (e.KeyData == Keys.Delete)
			{
				refreshList = true;
			}
		}

		base.OnKeyDown(e);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (!ReadOnly)
		{
			if (e.KeyChar == '#')
			{
				AutoCompleteText(e);
			}

			base.OnKeyPress(e);

			ParentDropEdit.ProposedText = Text;

			if (!ParentDropEdit.DropButton.IsDroppedDown)
			{
				ParentDropEdit.DropButton.ShowDropDown(false);
			}

			ParentDropEdit.UpdateDropDown();
		}
	}

	protected override bool ProcessCmdKeyCore(Keys keyData)
	{
		if (keyData == Keys.Escape || keyData == Keys.Tab || keyData == Keys.Enter)
		{
			var e = new KeyEventArgs(keyData);
			if (CommandKeyHandler is null)
			{
				ParentDropEdit.HandleCommandKey(e);
			}
			else
			{
				CommandKeyHandler.Handle(e);
			}

			if (keyData != Keys.Tab && keyData != Keys.Enter && e.Handled)
			{
				return true;
			}
		}

		return false;
	}

	protected override void OnTextChanged(EventArgs e)
	{
		base.OnTextChanged(e);

		if (refreshList)
		{
			refreshList = false;
			ParentDropEdit.ProposedText = Text;
			ParentDropEdit.UpdateDropDown();
		}
	}

	protected override void UpdateDescription(string description)
	{
		if (ParentDropEdit.FormattingEnabled)
		{
			description = description.ReFormatForRightToLeftLanguages();
		}

		base.UpdateDescription(description);
	}

	internal override void AutoCompleteText(KeyPressEventArgs e)
	{
		var oldSelectionStart = SelectionStart;

		var keyChar = new String(e.KeyChar, 1);
		if (CharacterCasing == CharacterCasing.Upper)
		{
			keyChar = keyChar.ToUpperInvariant();
		}
		else if (CharacterCasing == CharacterCasing.Lower)
		{
			keyChar = keyChar.ToLowerInvariant();
		}

		var startText = new CargoWise.Types.ZString(Text).SubstringSafe(0, SelectionStart) + keyChar;
		var endText = new CargoWise.Types.ZString(Text).SubstringSafe(SelectionStart + SelectionLength);
		var newText = startText + endText;

		if (MaxLength == -1 || MaxLength == 0 || newText.Length <= MaxLength)
		{
			Text = newText;
			Select(oldSelectionStart + 1, 0);
		}

		e.Handled = true;
	}

	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);

		if (ShouldClearText)
		{
			Text = "";
		}
	}

	public IKeyEventHandler? CommandKeyHandler { get; set; }

	bool ShouldClearText
	{
		get { return FilterStripDataSource != null && FilterStripDataSource.IsFilterDescriptionEmpty; }
	}

	FilterStrip? FilterStripDataSource
	{
		get { return (DataBindings["Text"] != null) ? DataBindings["Text"].DataSource as FilterStrip : null; }
	}
}
