using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(LVXSelectionForm))]
	sealed class LVXSelectionFormTest : ZFormBasherTest
	{
		public void TestFormText()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				AssertEquals("Consolidation Selection Criteria", form.Text);
			}
		}

		protected override Form GetFormToBashCore() => new LVXSelectionForm(new LVXSelectionCriteriaBO(Factory));
	}
}
