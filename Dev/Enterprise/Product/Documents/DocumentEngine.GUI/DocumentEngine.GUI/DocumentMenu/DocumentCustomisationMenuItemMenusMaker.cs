using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class DocumentCustomisationMenuItemMenusMaker : DocumentCustomisationMenusMaker<MenuItem>
	{
		internal DocumentCustomisationMenuItemMenusMaker(Form parentForm, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, ZDocumentsMenuItemMenuHelper helper)
			: base(parentForm, documentSupportable, userFieldList, helper)
		{
		}

		protected override MenuItem GetSuspendDocBuilderCustomizationsMenuItem()
		{
			return new SuspendDocBuilderCustomizationsMenuItem();
		}

		protected override MenuItem GetSuspendTemplateCachingMenuItem()
		{
			return new SuspendTemplateCachingMenuItem();
		}
	}
}
