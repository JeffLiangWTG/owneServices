using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZColorSchemeRuleForm))]
	sealed class ZColorSchemeRuleFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ZColorSchemeRuleForm();
		}

		public void TestFormSetsRuleName()
		{
			GridColourSchemeTest.FilterStripBusinessObjectForTest bo = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			GridColourStripBusinessObject strip = new GridColourStripBusinessObject(bo, null, null) { RuleName = "obeyme" };
			using (ZColorSchemeRuleFormForTest form = new ZColorSchemeRuleFormForTest(strip))
			{
				AssertEquals("obeyme", form.RuleNameTextBoxText);
			}
		}

		public void TestRuleNameMaxLength()
		{
			GridColourSchemeTest.FilterStripBusinessObjectForTest bo = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			GridColourStripBusinessObject strip = new GridColourStripBusinessObject(bo, null, null) { RuleName = "obeyme" };
			using (ZColorSchemeRuleFormForTest form = new ZColorSchemeRuleFormForTest(strip))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(StmModuleFilterSchema.S9_FilterName.MaxLength, form.RuleNameTextBox.MaxLength);
			}
		}

		class ZColorSchemeRuleFormForTest : ZColorSchemeRuleForm
		{
			public ZColorSchemeRuleFormForTest(GridColourStripBusinessObject strip) : base(strip) { }

			public string RuleNameTextBoxText
			{
				get { return RuleNameTextBox.Text; }
			}
		}
	}
}
