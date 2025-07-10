using System;
using System.Drawing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class ConsolidatedDeclarationDetailsUserControl : ZUserControl
	{
		public ConsolidatedDeclarationDetailsUserControl()
		{
			InitializeComponent();

			MessageStatusTextBox.TextChanged += MessageStatusTextBox_TextChanged;
		}

		void MessageStatusTextBox_TextChanged(object sender, EventArgs e)
		{
			var hasConsolidatedEntryChanges = DataSource is ConsolidatedDeclaration consolidatedEntry && consolidatedEntry.HasConsolidatedEntryChanges;
			MessageStatusTextBox.ColorChanger.ForceBackColor(hasConsolidatedEntryChanges ? Color.LightSalmon : SystemColors.Control);
		}
	}
}
