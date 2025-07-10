using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(TextTemplatePreviewForm))]
	sealed class TextTemplatePreviewFormTest : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var form = new TextTemplatePreviewForm();
			form.PreviewText = "Sugar";
			return form;
		}
	}
}
