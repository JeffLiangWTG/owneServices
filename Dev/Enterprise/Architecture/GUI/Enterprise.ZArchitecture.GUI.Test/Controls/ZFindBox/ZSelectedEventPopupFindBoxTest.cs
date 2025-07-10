using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZSelectedEventPopupFindBoxTest : BaseFindBoxTest
	{
		public void TestPressingF3OnCodeBoxDoNothing()
		{
			CreateDummies();
			Dummy.SS_Dummy = new ZString("AABCD");

			using (var form = new ZChildForm())
			{
				form.Show();
				CreateControls(form, "SS_Dummy");
				ZFormModaliser.LastFormShownForTest = null;
				KeySender.SendKeyDownToProcessCmdKey(popupFindBox.CodeBox, (int)Keys.F3);
				Application.DoEvents();
				AssertNull("Pressing F3 does nothing", ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestPressingF4OnCodeBoxDoesNotShowCodeStrip()
		{
			CreateDummies();
			Dummy.SS_Dummy = new ZString("AABCD");

			using (var form = new ZChildForm())
			{
				form.Show();
				CreateControls(form, "SS_Dummy");
				ZFormModaliser.LastFormShownForTest = null;
				KeySender.SendKeyDownToProcessCmdKey(popupFindBox.CodeBox, (int)Keys.F4);
				Application.DoEvents();
				var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				AssertEquals(0, ((FilterStripBusinessObject)((IFilterModuleInternalsForTesting)modulePopup.Module).FilterBusinessObject).ActiveModuleFilters.Count);
			}
		}

		public void TestPressingPopupButtonDoesNotShowCodeStrip()
		{
			CreateDummies();
			Dummy.SS_Dummy = new ZString("AABCD");

			using (var form = new ZChildForm())
			{
				form.Show();
				CreateControls(form, "SS_Dummy");
				ZFormModaliser.LastFormShownForTest = null;
				popupFindBox.PopupButton.PerformClick();
				Application.DoEvents();
				var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				AssertEquals(0, ((FilterStripBusinessObject)((IFilterModuleInternalsForTesting)modulePopup.Module).FilterBusinessObject).ActiveModuleFilters.Count);
			}
		}

		public void TestCodeSetFromRetrieve()
		{
			CreateDummies();
			Dummy.SS_Dummy = new ZString("AABCD");

			using (var form = new ZChildForm())
			{
				CreateControls(form, "SS_Dummy");
				form.Show();
				Application.DoEvents();
				AssertEquals("List after Show", Dummy.Dummies, popupFindBox.List);
				AssertEquals("Code after Show", "AABCD", popupFindBox.CodeBox.Text);
			}
		}

		public void TestBindingAfterShow()
		{
			CreateDummies();
			Dummy.SS_Dummy = new ZString("AABCD");

			using (var form = new ZChildForm())
			{
				form.Show();
				CreateControls(form, "SS_Dummy");
				AssertEquals("List after Show", Dummy.Dummies, popupFindBox.List);
				AssertEquals("Code after Show", "AABCD", popupFindBox.CodeBox.Text);
			}
		}

		public void TestCodeSetFromInputAndValidate()
		{
			CreateDummies();

			using (var form = new ZChildForm())
			{
				CreateControls(form, "");
				form.Show();
				popupFindBox.Focus();

				AssertEquals("List after Show", Dummy.Dummies, popupFindBox.List);
				AssertEquals("Code after Show", "", popupFindBox.CodeBox.Text);

				SendKeyPressToCodeBox('A');
				SendKeyPressToCodeBox('=');

				AssertEquals("Code set FindBox after AutoComplete", "AABCD", popupFindBox.CodeBox.Text);
				AssertEquals("Code set in BusinessObject before changing focus", "", Dummy.SS_Dummy);

				ChangeFocusToInvokeBinding();

				AssertEquals("Code set in BusinessObject after changing focus", "AABCD", Dummy.SS_Dummy);
				AssertEquals("Code set FindBox after changing focus", "AABCD", popupFindBox.CodeBox.Text);
			}
		}

		public void TestBindToList()
		{
			using (var form = new ZChildForm())
			{
				CreateControls(form, "");
				form.Show();
				AssertEquals("FindBox List after binding", Dummy.Dummies, popupFindBox.List);
			}
		}

		#region Implementation

		protected override void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			base.CreateControls(testForm, "");
			popupFindBox.ModuleID = DummyModuleIDs.Dummy;
		}

		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZSelectedEventPopupFindBox(); }
		}

		ZSelectedEventPopupFindBox popupFindBox
		{
			get { return (ZSelectedEventPopupFindBox)FindBox; }
		}
		#endregion
	}
}
