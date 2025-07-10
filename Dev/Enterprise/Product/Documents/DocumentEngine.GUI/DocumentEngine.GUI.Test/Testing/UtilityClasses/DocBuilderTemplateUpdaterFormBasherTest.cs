using System.Windows.Forms;
using Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing.UtilityClasses.Testing
{
	[TestedType(typeof(DocBuilderTemplateUpdaterForm))]
	sealed class DocBuilderTemplateUpdaterFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var docBuilderTemplateUpdater = new DocBuilderTemplateUpdater(Factory);
			return new DocBuilderTemplateUpdaterForm(docBuilderTemplateUpdater);
		}

		#endregion
	}
}
