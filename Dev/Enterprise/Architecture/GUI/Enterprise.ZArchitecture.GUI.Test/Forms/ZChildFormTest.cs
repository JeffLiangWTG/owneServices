using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZChildFormTest : TestCaseWithDummy
	{
		protected class ZChildFormTester : ZChildForm
		{
			public new MainMenu MainMenu
			{
				get { return base.MainMenu; }
			}
		}

		public void TestNoMainMenu()
		{
			using (var form = new ZChildFormTester())
			{
				AssertNull("ZChildForm MainMenu should be null", form.MainMenu);
			}
		}

#if !WINZOR
//IPastableControl.TryPaste() is disabled in Winzor
		public void TestPaste()
		{
			PasteFired = false;

			using (var form = new ZChildForm())
			{
				var testControl = new TestPastingTextBox(this);
				form.Controls.Add(testControl);
				form.Show();

				KeySender.PostKeyDown(testControl, Keys.Control | Keys.V);

				Application.DoEvents();
				AssertEquals("Paste Fired", true, PasteFired);
			}
		}
#endif

		public void TestForceDialogRendering()
		{
			Env.Registry.SetIsTerminalServiceModeForTest(true);
			Assert("PreCodition:ShouldForceDialogRendering should enable", Env.Registry.ShouldForceDialogRendering);

			using (var form = new ForceDialogRenderingEnableFormForTest())
			{
				AssertEquals("After Construtor, FormBorderStyle should be set to FixedDialog.", FormBorderStyle.FixedDialog, form.FormBorderStyle);
				form.Show();
				Application.DoEvents();
				AssertEquals("After Shown,FormBorderStyle should be restrore", FormBorderStyle.None, form.FormBorderStyle);
			}
		}

		#region Implementation

		public class ForceDialogRenderingEnableFormForTest : ZChildForm
		{
			public ForceDialogRenderingEnableFormForTest()
			{
				FormBorderStyle = FormBorderStyle.None;
				ChangeFormBorderStyleForRendering();
			}

			protected override void OnShown(EventArgs e)
			{
				base.OnShown(e);
			}
		}

#if !WINZOR
		bool PasteFired;
#endif

		protected class TestPastingTextBox : ZTextBox, IPastableControl
		{
			public TestPastingTextBox(ZChildFormTest parentTest)
			{
				this.ParentTest = parentTest;
			}

			protected readonly ZChildFormTest ParentTest;

			#region IPastableControl Members

#if !WINZOR

			bool IPastableControl.TryPaste()
			{
				return ParentTest.PasteFired = true;
			}

#endif

			#endregion
		}

		#endregion
	}
}
