using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(NewFeatureControlForm))]
	public class NewFeatureControlFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizObj = new NewFeatureControlBizObj();
			var form = new NewFeatureControlForm(bizObj);
			form.Show();
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("zLabel1", true).Single());
			return form;
		}
	}
}
