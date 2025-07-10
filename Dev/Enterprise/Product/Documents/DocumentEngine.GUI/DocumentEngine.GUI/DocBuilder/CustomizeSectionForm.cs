using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocBuilder.SectionEditing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocBuilder
{
	public partial class CustomizeSectionForm : ZChildForm, ICustomizeSectionView
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CustomizeSectionForm()
		{
			InitializeComponent();
		}

		public CustomizeSectionForm(CustomizeSectionManager manager)
			: base(manager)
		{
			InitializeComponent();
			controller = new CustomizeSectionController(manager, this);
		}

		readonly CustomizeSectionController controller;

		public new CustomizeSectionManager BusinessEntity
		{
			get { return base.BusinessEntity as CustomizeSectionManager; }
		}

		void HandleCopySection(object sender, EventArgs e)
		{
			if (sectionsGrid.ListManager != null && sectionsGrid.ListManager.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("ca829e0b-1aaa-4f77-b295-43a9e99ff8df", "Please select a record in the grid."));
				return;
			}
			var section = sectionsGrid.ListManager.GetCurrent() as TemplateSection;
			if (section != null)
			{
				try
				{
					controller.CopySection(section, BusinessEntity.Language);
					IsAtLeastOneSectionCopied = true;
				}
				catch (MissingWorksheetException ex)
				{
					Globals.Message.ShowError(Res.GetString("40e9e572-9e9b-4b65-b4be-f9bd77ffb595",
						"Could not find worksheet named [{0}] in template [{1}].\r\nPlease verify this template before proceeding.",
						ex.WorksheetName, ex.TemplateName));
				}
				catch (ExcelLimitationForThisFileFormatException ex)
				{
					Globals.Message.ShowError(ex.InnerException?.Message ?? ex.Message);
				}
			}
		}

		internal bool IsAtLeastOneSectionCopied { get; private set; }

		#region ICustomizeSectionView Members

		public ITemplateEditor GetTemplateEditor(StmTemplateBase template)
		{
			return new TemplateEditor(template, this, true);
		}

		static string Caption => Res.GetString("f83f3d14-51d8-475d-9857-fe6730f3a35a", "Copy Section");

		public bool ShouldOverrideExistingSection(ZString sectionName, ZString templateName)
		{
			var message = Res.GetString("c1844adb-c11a-43ae-ae48-bec554b10a83"
				, @"The section [{0}] already exists in [{1}].

Do you want to copy over the existing Customizable Section override?", sectionName, templateName);
			return Globals.Message.Show(message, Caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}

		public void ShowEditOnlyMessage(ZString sectionName, ZString templateName)
		{
			var message = Res.GetString("a93f5952-020f-4367-a156-cf7c8aebbb18"
				, @"The section [{0}] cannot be copied to [{1}] as it 
only exists in [{1}].

Click OK to edit [{0}] in [{1}] instead.", sectionName, templateName);
			Globals.Message.Show(message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion
	}
}
