namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	using System;
	using Enterprise.DocumentEngine.Business;
	using Enterprise.DocumentEngine.DocBuilder;

	public interface IStmMenuDocumentConfigForm
	{
		event EventHandler PreviewButtonClicked;
		event EventHandler AddButtonClicked;
		event EventHandler RemoveButtonClicked;
		event EventHandler TemplateSectionDoubleClicked;
		event EventHandler ConfigItemDoubleClicked;
		event EventHandler MoveDownButtonClicked;
		event EventHandler MoveUpButtonClicked;
		event EventHandler PreviewSectionButtonClicked;
		event EventHandler PreviewConfigItemButtonClicked;

		void ShowError(string message, string caption);
		void ShowPreview(PrintTask printTask);
		void ShowSectionPreview(SectionPreviewManager manager);

		void SelectConfigItems(params StmMenuDocumentConfigItem[] configItems);

		TemplateSection[] GetSelectedSections();
		StmMenuDocumentConfigItem[] GetSelectedConfigItems();
	}
}