using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ContainersUserControlTest : TestCaseWithFactory
	{
		public void TestSecondSealTextBox()
		{
			using (var userControl = new ContainersUserControl())
			{
				var secondSealControl = userControl.FindSingle<ZTextBox>("edSecondSealNum");
				CombineAssertions(() =>
				{
					AssertNotNull("Second Seal edit control", secondSealControl);
					AssertEquals("Second Seal edit control caption", "Second Seal No.", secondSealControl.GetExtension<ILabelCaptionRenderer>().Caption);
				});
			}
		}

		public void TestIsControlCheckBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration.CusContainers))
			using (var userControl = new ContainersUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var isControlCheckBox = userControl.FindSingle<ZCheckBox>("IsControlCheckBox");
				CombineAssertions("Checkboxes are not activated by default", () =>
				{
					AssertNotNull("Control Box edit control", isControlCheckBox);
					AssertEquals("Control Box default visibility", false, isControlCheckBox.Visible);
				});
			}

			var declarationActive = Factory.New<JobDeclarationForTest>();
			using (var form = new ZForm(declarationActive.CusContainers))
			using (var userControl = new ContainersUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var isControlCheckBox = userControl.FindSingle<ZCheckBox>("IsControlCheckBox");
				CombineAssertions("JobDeclaration activated the Checkboxes", () =>
				{
					AssertNotNull("Control Box edit control", isControlCheckBox);
					AssertEquals("Control Box visibility", true, isControlCheckBox.Visible);
				});
			}
		}

		public void TestIsUnloadedCheckBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration.CusContainers))
			using (var userControl = new ContainersUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var isUnloadedCheckBox = userControl.FindSingle<ZCheckBox>("IsUnloadedCheckBox");
				CombineAssertions("Checkboxes are not activated by default", () =>
				{
					AssertNotNull("Unloaded Box edit control", isUnloadedCheckBox);
					AssertEquals("Unloaded Box default visibility", false, isUnloadedCheckBox.Visible);
				});
			}

			var declarationActive = Factory.New<JobDeclarationForTest>();
			using (var form = new ZForm(declarationActive.CusContainers))
			using (var userControl = new ContainersUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var isUnloadedCheckBox = userControl.FindSingle<ZCheckBox>("IsUnloadedCheckBox");
				CombineAssertions("JobDeclaration activated the Checkboxes", () =>
				{
					AssertNotNull("Unloaded Box edit control", isUnloadedCheckBox);
					AssertEquals("Unloaded Box visibility", true, isUnloadedCheckBox.Visible);
				});
			}
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZBool ContainerControlCheckboxVisibleCore => true;

			protected override ZBool ContainerUnloadedCheckboxVisibleCore => true;
		}
	}
}
