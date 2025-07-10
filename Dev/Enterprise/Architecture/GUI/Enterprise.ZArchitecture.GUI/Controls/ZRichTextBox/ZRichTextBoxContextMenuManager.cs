using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	class ZRichTextBoxContextMenuManager : ZTextBoxBaseContextMenuManager
	{
		#region Implements

		public ZRichTextBoxContextMenuManager(IDataBoundControl dataBoundControl, TextBoxBase textBox, IMacroBox macroBox)
			: base(dataBoundControl, textBox, macroBox)
		{
		}

		public ZRichTextBoxContextMenuManager(CurrencyManager dataSource, string mappingName, TextBoxBase textBox, IMacroBox macroBox)
			: base(dataSource, mappingName, textBox, macroBox)
		{
		}

		ToolStripMenuItem pasteTextOnlyItem;

		protected override void AddExtraMenuItems()
		{
			textBox.ContextMenuStrip.Items.Insert(textBox.ContextMenuStrip.Items.IndexOf(pasteItem) + 1, pasteTextOnlyItem = new ZToolStripMenuItem(Res.GetData("CD4EE340-CE3E-46EE-8B38-15064EEC5321", "Paste Text Only"), new EventHandler(PasteTextOnly_Click)));
			OnAddExtraMenuItems?.Invoke(textBox.ContextMenuStrip, textBox.ContextMenuStrip.Items.IndexOf(templatesItem));
		}
		internal event EventHandler<int> OnAddExtraMenuItems;

		internal override void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			base.ContextMenuStrip_Opening(sender, e);
			pasteTextOnlyItem.Enabled = !textBox.ReadOnly;
		}

		#endregion

		void PasteTextOnly_Click(object sender, EventArgs e)
		{
			((ZRichTextBox.MyRichTextBox)textBox).Outer.PasteTextOnly();
		}
	}
}
