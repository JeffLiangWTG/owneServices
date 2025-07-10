using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZRadioButtonTest : ZControlBaseTestCase<ZRadioButton>
	{
		public void TestColors()
		{
			using (var theForm = new ZForm())
			{
				var testControl2 = new ZRadioButton();
				TestControl.Parent = theForm;
				testControl2.Parent = theForm;
				theForm.Show();
				TestControl.Focus();
				AssertEquals("Control1 must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, TestControl.BackColor);
				AssertEquals("Control2 must be colored in standard color", theForm.BackColor, testControl2.BackColor);

				testControl2.Focus();
				AssertEquals("Control1 must be colored in standard color", theForm.BackColor, TestControl.BackColor);
				AssertEquals("Control2 must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, testControl2.BackColor);
			}
		}

		public void TestTabStop()
		{
			using (var testForm = new ZForm())
			{
				var group1 = new ZGroupBox();
				var group2 = new ZGroupBox();
				testForm.Controls.Add(group1);
				testForm.Controls.Add(group2);

				var group1Radio1 = new ZRadioButton { Name = "Group1Radio1" };
				var group1Radio2 = new ZRadioButton { Name = "Group1Radio2" };
				var group1Radio3 = new ZRadioButton { Name = "Group1Radio3" };
				group1.Controls.Add(group1Radio1);
				group1.Controls.Add(group1Radio2);
				group1.Controls.Add(group1Radio3);

				var group2Radio1 = new ZRadioButton { Name = "Group2Radio1" };
				var group2Radio2 = new ZRadioButton { Name = "Group2Radio2" };
				var group2Radio3 = new ZRadioButton { Name = "Group2Radio3" };
				group2.Controls.Add(group2Radio1);
				group2.Controls.Add(group2Radio2);
				group2.Controls.Add(group2Radio3);

				group1Radio2.Checked = true;
				group2Radio3.Checked = true;
				group1Radio2.Focus();
				testForm.Show();

				KeySender.PostKeyDown(group1Radio2, Keys.Up);
				Application.DoEvents();
				AssertEquals("Up arrow selects previous radio button.", group1Radio1.Name, testForm.ActiveControl.Name);
				Assert("Radio button should be checked", group1Radio1.Checked);

				KeySender.PostKeyDown(group1Radio1, Keys.Tab);
				Application.DoEvents();
				AssertEquals("Should tab to checked radio button in next group.", group2Radio3.Name, testForm.ActiveControl.Name);
			}
		}

		public void TestShowFocusCues()
		{
			using (var testForm = new ZForm())
			{
				var group1 = new ZGroupBox();
				testForm.Controls.Add(group1);

				var group1Radio1 = new ZRadioButton { Name = "Group1Radio1" };
				var group1Radio2 = new ZRadioButton { Name = "Group1Radio2" };
				var group1Radio3 = new ZRadioButton { Name = "Group1Radio3" };
				group1.Controls.Add(group1Radio1);
				group1.Controls.Add(group1Radio2);
				group1.Controls.Add(group1Radio3);

				group1Radio2.Checked = true;
				group1Radio2.Focus();
				testForm.Show();

				// the focus cue should show without having to hit the Tab key
				Assert("No Focus cue was shown", group1Radio2.ShowFocusCues_Exposed);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZRadioButton.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZRadioButton)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZRadioButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		#region Implementation

		ZRadioButton TestControl
		{
			get { return testControl ?? (testControl = new ZRadioButton()); }
		}
		ZRadioButton testControl;

		protected override void TearDown()
		{
			base.TearDown();

			if (TestControl != null)
			{
				TestControl.Dispose();
			}
		}

		protected override string[] BindablePropertyNames
		{
			get { return new[] { "Checked", "ReadOnly", "Text" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		#endregion
	}
}
