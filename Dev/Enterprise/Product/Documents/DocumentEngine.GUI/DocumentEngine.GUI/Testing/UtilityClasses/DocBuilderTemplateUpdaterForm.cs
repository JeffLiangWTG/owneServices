#if DEBUG
using System;
using Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing.UtilityClasses
{
	public partial class DocBuilderTemplateUpdaterForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public DocBuilderTemplateUpdaterForm()
		{
			InitializeComponent();
		}

		public DocBuilderTemplateUpdaterForm(DocBuilderTemplateUpdater docBuilderTemplateUpdater)
			: base(docBuilderTemplateUpdater)
		{
			InitializeComponent();
			this.docBuilderTemplateUpdater = docBuilderTemplateUpdater;
		}

		readonly DocBuilderTemplateUpdater docBuilderTemplateUpdater;

		void ExecuteButton_Click(object sender, EventArgs e)
		{
			docBuilderTemplateUpdater.Execute();
			Globals.Message.Show("DocBuilder Template Updater has finished executing.");
		}

		void SaveDocBuilderTemplatesToFileButton_Click(object sender, EventArgs e)
		{
			docBuilderTemplateUpdater.SaveDocBuilderTemplatesToFile();
		}
	}
}
#endif