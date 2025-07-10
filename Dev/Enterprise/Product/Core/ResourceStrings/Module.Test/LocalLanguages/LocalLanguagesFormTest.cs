using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.Testing
{
	[TestedType(typeof(LocalLanguagesForm))]
	public class LocalLanguagesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var language = Factory.New<RefLocalLanguage>();
			var form = new LocalLanguagesForm(language);
			form.Width = 800;
			form.Height = 600;
			return form;
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
