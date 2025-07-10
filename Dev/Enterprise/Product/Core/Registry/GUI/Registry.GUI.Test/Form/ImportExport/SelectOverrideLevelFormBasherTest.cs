using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SelectOverrideLevelForm))]
	sealed class SelectOverrideLevelFormBasherTest : ZFormBasherTest
	{
		public void TestItemsArePopulated()
		{
			using (var form = new SelectOverrideLevelForm(Factory))
			{
				TreeViewAssertion.AssertHasPath("root", form.treeView.Nodes, "System");
				TreeViewAssertion.AssertHasPath("root", form.treeView.Nodes, "System/Departments");
				TreeViewAssertion.AssertHasPath("root", form.treeView.Nodes, "Companies");
			}
		}

		public void TestGetSelectedItem()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "MyCompany";

			using (var form = new SelectOverrideLevelForm(Factory))
			{
				form.Show();

				form.treeView.PerformSelect(form.treeView.Nodes["System"]);
				AssertEquals(typeof(SystemOverrideLevel), form.SelectedOveride.GetType());

				form.treeView.PerformSelect(form.treeView.Nodes["Companies"].Nodes["MyCompany"]);
				AssertEquals(typeof(CompanyOverrideLevel), form.SelectedOveride.GetType());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestCompareButton()
		{
			using (var form = new SelectOverrideLevelForm(Factory))
			{
				form.Show();

				form.treeView.PerformSelect(form.treeView.Nodes["Companies"]);
				AssertNull(form.SelectedOveride);
				form.compareButton.PerformClick();

				AssertEquals("We didnt select anything and shouldnt be able to close the form", form, Application.OpenForms[form.Name]);
				AssertEquals("Please select a valid level", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);

				form.treeView.PerformSelect(form.treeView.Nodes["Default"]);
				form.compareButton.PerformClick();
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new SelectOverrideLevelForm(Factory);
		}

		#endregion
	}
}
