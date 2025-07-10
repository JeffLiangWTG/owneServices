using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(BulkLookupChangeForm))]
	sealed class BulkLookupChangeFormTest : ZFormBasherTest
	{
		public void ContinueBtn_ClickTest()
		{
			BulkLookupChanger businessObject = new BulkLookupChanger(Factory);
			using (BulkLookupChangeForm form = new BulkLookupChangeForm(businessObject))
			{
				form.AcceptButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				businessObject.RemoveTreatmentCode = true;
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore() => new BulkLookupChangeForm(new BulkLookupChanger(Factory));
	}
}
