using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	class ZFindBoxUserControlTest : TestCaseWithFactory
	{
		#region Auto Complete

		public void TestAutoCompleteText()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("AutoComplete fired", false, findBox.IsAutoCompleted);

				KeySender.SendKeyPress(findBox.CodeBox, findBox.CodeBox.Handle, '=');
				Application.DoEvents();

				AssertEquals("AutoComplete fired", true, findBox.IsAutoCompleted);
				AssertEquals("AutoComplete EXPLICITLY fired", true, findBox.LastAutoCompleteIsExplicitFlag);
			}
		}

		public void TestAutoCompleteDisabled()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				findBox.AutoCompleteDisabledExposer = true;
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("AutoComplete fired", false, findBox.IsAutoCompleted);

				KeySender.SendKeyPress(findBox.CodeBox, findBox.CodeBox.Handle, '=');
				Application.DoEvents();

				AssertEquals("AutoComplete fired", false, findBox.IsAutoCompleted);
				AssertEquals("AutoComplete EXPLICITLY fired", false, findBox.LastAutoCompleteIsExplicitFlag);
			}
		}

		public void TestAutoCompleteTextOnCommit_FocusNextControl()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				findBox.ShouldAutoCompleteOnCommit = true;
				testForm.Controls.Add(findBox);
				testForm.Show();
				AssertEquals("Precondition: AutoComplete not fired", false, findBox.IsAutoCompleted);

				testForm.ActiveControl = findBox.CodeBox;
				testForm.ActiveControl = null;
				Application.DoEvents();
				AssertEquals("AutoComplete not fired as codebox has no text", false, findBox.IsAutoCompleted);

				findBox.CodeBox.Text = "hello";
				testForm.ActiveControl = findBox.CodeBox;
				testForm.ActiveControl = null;
				Application.DoEvents();
				AssertEquals("AutoComplete fired", true, findBox.IsAutoCompleted);
				AssertEquals("AutoComplete NOT EXPLICITLY fired", false, findBox.LastAutoCompleteIsExplicitFlag);
			}
		}

		public void TestAutoCompleteTextOnCommit_TabToNextControl()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				findBox.ShouldAutoCompleteOnCommit = true;
				testForm.Controls.Add(findBox);
				testForm.Show();
				AssertEquals("Precondition: AutoComplete not fired", false, findBox.IsAutoCompleted);

				testForm.ActiveControl = findBox.CodeBox;
				findBox.SendTabKeyToCodeBox();
				Application.DoEvents();
				AssertEquals("AutoComplete not fired as codebox has no text", false, findBox.IsAutoCompleted);

				findBox.CodeBox.Text = "hello";
				testForm.ActiveControl = findBox.CodeBox;
				findBox.SendTabKeyToCodeBox();
				Application.DoEvents();
				AssertEquals("AutoComplete fired", true, findBox.IsAutoCompleted);
				AssertEquals("AutoComplete NOT EXPLICITLY fired", false, findBox.LastAutoCompleteIsExplicitFlag);
			}
		}

		public void TestAutoCompleteTextWhenReadOnly()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				findBox.ReadOnly = true;
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("AutoComplete fired", false, findBox.IsAutoCompleted);

				KeySender.SendKeyPress(findBox.CodeBox, findBox.CodeBox.Handle, '=');
				Application.DoEvents();

				AssertEquals("AutoComplete fired", false, findBox.IsAutoCompleted);
			}
		}

		#endregion

		public void TestAllowModuleMultiSelect()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("Default value", false, findBox.AllowModuleMultiSelect);
			}
		}

		public void TestZCodeBoxIAdditionalInformationMembers()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);
				testForm.Show();
				findBox.BindToList = "Lookups+List1";
				IAdditionalInformation additionalInformation = findBox.CodeBox;
				AssertEquals("Lookups+List1", additionalInformation.AdditionalInformation);
			}
		}

		public void TestSelectFromPopupForm()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("SelectFromPopupForm fired", false, findBox.IsSelectedFromPopupForm);

				KeySender.SendKeyDownToProcessCmdKey(findBox.CodeBox, (int)Keys.F4);
				Application.DoEvents();

				AssertEquals("SelectFromPopupForm fired", true, findBox.IsSelectedFromPopupForm);
			}
		}

		public void TestSelectFromPopupFormWhenReadOnly()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				findBox.ReadOnly = true;
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("SelectFromPopupForm fired", false, findBox.IsSelectedFromPopupForm);

				KeySender.SendKeyDownToProcessCmdKey(findBox.CodeBox, (int)Keys.F4);
				Application.DoEvents();

				AssertEquals("SelectFromPopupForm fired", false, findBox.IsSelectedFromPopupForm);
			}
		}

		public void TestShowEditOrViewForm()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);
				testForm.Show();

				AssertEquals("SelectFromPopupForm fired", false, findBox.IsShowingEditOrViewForm);

				KeySender.SendKeyDownToProcessCmdKey(findBox.CodeBox, (int)Keys.F3);
				Application.DoEvents();

				AssertEquals("SelectFromPopupForm fired", true, findBox.IsShowingEditOrViewForm);
			}
		}

		public void TestButtonReturnsFocusToCodeBoxWhenClicked()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);
				testForm.Show();

				findBox.PopupButton.Focus();
				MouseSender.PostMessage(findBox.PopupButton, findBox.PopupButton.Handle, WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
				Application.DoEvents();

				MouseSender.PostMessage(findBox.PopupButton, findBox.PopupButton.Handle, WindowsMessage.WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
				Application.DoEvents();

				AssertEquals("CodeBox Focus after ButtonClick", true, findBox.CodeBox.Focused);
			}
		}

		#region TestSelectFromPopupFormWithoutDisplaying

		public void TestSelectFromPopupFormWithoutDisplaying()
		{
			const string expectedValidationError = "There are multiple possible DummyBizos. Select the Id cell and press F4 to choose one.";

			var dummy = Factory.New<DummyWithLookupList>();
			using (var testForm = new ZForm(dummy))
			{
				var findBox = new ZFindBoxUserControlTester { BindTo = "Z0_Guid" };
				testForm.Controls.Add(findBox);
				testForm.Show();
				testForm.SetDataBinding(dummy, "");
				Application.DoEvents();

				findBox.CodeBox.Text = "X:123";

				findBox.SilentSelectResultForTest = SilentSelectResult.FoundNothing;
				findBox.SendTabKeyToCodeBox();
				Application.DoEvents();
				dummy.Z0_GuidInfo.ClearAllNotifications();
				dummy.Z0_GuidInfo.RunAdditionalValidation();
				Assert("Should not have additional validation errors", !dummy.Z0_GuidInfo.HasErrors());

				findBox.SilentSelectResultForTest = SilentSelectResult.MultipleResults;
				findBox.SendTabKeyToCodeBox();
				Application.DoEvents();
				dummy.Z0_GuidInfo.ClearAllNotifications();
				dummy.Z0_GuidInfo.RunAdditionalValidation();
				Assert("Should have new additional validation error", dummy.Z0_GuidInfo.HasError(expectedValidationError));

				dummy.Z0_GuidInfo.ClearAllNotifications();
				dummy.Z0_GuidInfo.RunAdditionalValidation();
				Assert("Still should have additional validation error", dummy.Z0_GuidInfo.HasError(expectedValidationError));

				findBox.SendTabKeyToCodeBox();
				Application.DoEvents();
				dummy.Z0_GuidInfo.ClearAllNotifications();
				dummy.Z0_GuidInfo.RunAdditionalValidation();
				Assert("Yet still should have additional validation error", dummy.Z0_GuidInfo.HasError(expectedValidationError));

				dummy.Z0_Guid = ZGuid.NewZGuid(); // First call is to emulate ValueChanged on text commit in FindBox
				dummy.Z0_GuidInfo.ClearAllNotifications();
				dummy.Z0_GuidInfo.RunAdditionalValidation();
				Assert("Should have additional validation error because of delayed unhook", dummy.Z0_GuidInfo.HasError(expectedValidationError));

				dummy.Z0_Guid = ZGuid.NewZGuid(); // Actual change to new value [by user]
				dummy.Z0_GuidInfo.ClearAllNotifications();
				dummy.Z0_GuidInfo.RunAdditionalValidation();
				Assert("All additional validation errors should be cleared", !dummy.Z0_GuidInfo.HasErrors());

				ExceptionReporterTestListener.Instance.Clear(); // Remove message about ClearAllNotifications()
			}
		}

		class DummyWithLookupList : DummyBusinessObject
		{
			public DummyWithLookupList(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("Dummies")]
			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set { base.Z0_Guid = value; }
			}

			public DummiesColleciton Dummies
			{
				get { return new DummiesColleciton(Factory); }
			}
		}

		[ComponentModel.ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.Dummy)]
		class DummiesColleciton : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummiesColleciton(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		public void TestPopupButtonReadonly()
		{
			using (var testForm = new ZChildForm())
			{
				var findBox = new ZFindBoxUserControlTester();
				testForm.Controls.Add(findBox);

				findBox.ReadOnly = true;
				Assert(findBox.PopupButton.ReadOnly);

				findBox.PopupButton.ReadOnly = false;
				Assert(findBox.PopupButton.ReadOnly);

				findBox.PopupButtonReadonlyCanBeDifferent_Exposed = true;
				findBox.PopupButton.ReadOnly = false;
				Assert(!findBox.PopupButton.ReadOnly);
			}
		}

		#region ZFindBoxUserControlTester

		protected class ZFindBoxUserControlTester : ZFindBoxUserControl
		{
			public bool IsShowingEditOrViewForm;
			public bool IsAutoCompleted;
			public bool LastAutoCompleteIsExplicitFlag;
			public bool IsSelectedFromPopupForm;
			public bool ShouldAutoCompleteOnCommit;

			public bool PopupButtonReadonlyCanBeDifferent_Exposed
			{
				get { return PopupButtonReadonlyCanBeDifferent; }
				set { PopupButtonReadonlyCanBeDifferent = value; }
			}

			public bool AutoCompleteDisabledExposer
			{
				set { ((IFindBoxUserControl)this).AutoCompleteDisabled = value; }
			}

			protected override bool AutoCompleteText(bool explicitAutoComplete)
			{
				IsAutoCompleted = true;
				LastAutoCompleteIsExplicitFlag = explicitAutoComplete;
				return true;
			}

			public override void SelectFromPopupForm(bool autoSelect = false)
			{
				IsSelectedFromPopupForm = true;
			}

			protected override void ShowEditOrViewForm()
			{
				IsShowingEditOrViewForm = true;
			}

			protected override bool AutoCompleteOnCommit
			{
				get { return ShouldAutoCompleteOnCommit; }
			}

			public void SendTabKeyToCodeBox()
			{
				CodeBox.GetType().GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(CodeBox, new object[] { new Message(), Keys.Tab });
			}

			public override SilentSelectResult SelectFromPopupFormWithoutDisplaying()
			{
				IsSelectedFromPopupForm = true;
				return SilentSelectResultForTest;
			}

			public SilentSelectResult SilentSelectResultForTest = SilentSelectResult.None;

			public override bool PrefixExistsInModule(string prefix)
			{
				return true;
			}
		}

		#endregion
	}
}
