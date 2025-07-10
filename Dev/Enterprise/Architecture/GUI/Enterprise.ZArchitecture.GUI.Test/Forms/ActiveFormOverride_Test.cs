using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ActiveFormOverride_Test : NUnit.Framework.TestCase
	{
		public void TestActiveFormOverride()
		{
			AssertEquals(Form.ActiveForm, ZFormModaliser.Instance.ApplicationActiveForm);
			var form = new Form();
			using (new ZFormModaliser.ActiveFormOverride(form))
			{
				AssertEquals(form, ZFormModaliser.Instance.ApplicationActiveForm);
			}
			AssertEquals(Form.ActiveForm, ZFormModaliser.Instance.ApplicationActiveForm);
		}
	}
}
