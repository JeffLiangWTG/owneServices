using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class DocumentCustomisationToolStripMenusMaker : DocumentCustomisationMenusMaker<ToolStripItem>
	{
		internal DocumentCustomisationToolStripMenusMaker(Form parentForm, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, ZDocumentsToolStripMenuHelper helper)
			: base(parentForm, documentSupportable, userFieldList, helper)
		{
		}

		protected override ToolStripItem GetSuspendDocBuilderCustomizationsMenuItem()
		{
			return new SuspendDocBuilderCustomizationsToolStripMenuItem();
		}

		protected override ToolStripItem GetSuspendTemplateCachingMenuItem()
		{
			return new SuspendTemplateCachingToolStripMenuItem();
		}
	}
}
