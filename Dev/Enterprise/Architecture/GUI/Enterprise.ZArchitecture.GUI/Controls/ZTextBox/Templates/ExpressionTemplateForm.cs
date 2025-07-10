using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ExpressionTemplateForm : TextTemplateForm
	{
		public ExpressionTemplateForm(ExpressionNoteTemplate template, BusinessObject[] context, bool multiLine, Type[] mapContextOverride, bool shouldEscapeAllSpecialCharacters = false)
			: base(template, context, multiLine, shouldEscapeAllSpecialCharacters)
		{
			InitializeComponent();
			this.mapContextOverride = mapContextOverride;
			previewButton.Visible = false;
			ReassignTabIndices();
			CaptionResourceString = Res.GetData("ExpressionTemplateForm|51f63f39-debf-4725-bb09-f633a9a2400d", "Expression Template");
		}

		readonly Type[] mapContextOverride;

		protected override bool AllowMacroEditing
		{
			get { return true; }
		}

		protected override Type[] GetMapTreeParentTypes()
		{
			return mapContextOverride;
		}

		void ReassignTabIndices()
		{
			descriptionTextBox.TabIndex = 1;
			TemplateLabel.TabIndex = 2;
			insertMacroButton.TabIndex = 3;
			previewButton.TabIndex = 4;
			PlaceholderButton.TabIndex = 5;
			templateTextBox.TabIndex = 6;
			PlaceholdersLabel.TabIndex = 7;
			PlaceholdersGrid.TabIndex = 8;
			publicCheckbox.TabIndex = 9;
			allCompaniesCheckbox.TabIndex = 10;
			deleteButton.TabIndex = 11;
			postingButtonsUserControl.TabIndex = 12;
		}

		void PlaceholderButton_Click(object sender, EventArgs e)
		{
			templateTextBox.Focus();
			templateTextBox.SelectionStart = lastTemplateTextBoxSelectionStart;
			templateTextBox.SelectionLength = lastTemplateTextBoxSelectionLength;
			templateTextBox.SelectedText = ExpressionNoteTemplate.Placeholder;
		}
	}
}
