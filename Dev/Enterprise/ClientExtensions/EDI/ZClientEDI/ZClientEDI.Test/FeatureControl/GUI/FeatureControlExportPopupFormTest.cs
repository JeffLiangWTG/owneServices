using System.Windows.Forms;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(FeatureControlExportPopupForm))]
	public class FeatureControlExportPopupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new FeatureControlExportPopupForm(new FeatureControlExportBizObj());
		}
	}
}
