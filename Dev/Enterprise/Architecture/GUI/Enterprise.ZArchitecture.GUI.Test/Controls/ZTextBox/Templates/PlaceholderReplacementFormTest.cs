using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(PlaceholderReplacementForm))]
	sealed class PlaceholderReplacementFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new PlaceholderReplacementForm(Factory.New<ExpressionNoteTemplate>());
		}
	}
}
