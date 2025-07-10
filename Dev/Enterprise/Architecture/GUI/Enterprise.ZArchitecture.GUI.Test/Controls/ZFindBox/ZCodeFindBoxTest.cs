using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZCodeFindBoxTest : BaseZCodeFindBoxTest
	{
		#region TestDescriptionWithListProviderChanged

		public void TestDescriptionWithListProviderChanged()
		{
			RunTestForListProviderChanged = true;
			try
			{
				Dummy.SS_Dummy = ZString.Empty;
				CreateControls(Form, "SS_Dummy");
				Form.Show();

				FindBox.CodeBox.Focus();
				FindBox.CodeBox.Text = "AAXXX";

				KeySender.SendKeyDownToProcessCmdKey(ZCodeFindBox.CodeBox, Keys.Enter);
				UserIdleWorker.Flush();

				AssertEquals("AAXXX Description", ZCodeFindBox.DescriptionBox.Text);
			}
			finally
			{
				RunTestForListProviderChanged = false;
			}
		}

		protected override void SetBindTo(ZFindBoxUserControl findBox)
		{
			if (RunTestForListProviderChanged)
			{
				findBox.BindTo = "SS_Dummy";
				findBox.BindToList = "DummiesForBindToListChange";
			}
			else
			{
				base.SetBindTo(findBox);
			}
		}

		bool RunTestForListProviderChanged { get; set; }

		#endregion

		public void TestFindBoxPopupWithBindToForDescription()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummyChild1 = dummy1.Collection.AddNew();
			dummyChild1.Z0_Code = "Bob1";

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummyChild2 = dummy2.Collection.AddNew();
			dummyChild2.Z0_Code = "Bob2";

			Factory.Save();

			using (var findBox = new ZCodeFindBox())
			{
				findBox.ModuleID = DummyModuleIDs.Dummy;
				var popupDecisionProvider = new PopupModuleDecisionProvider(findBox);
				findBox.DescriptionBox.Clear();
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild1 });
				AssertEquals("Default", findBox.DescriptionBox.Text);

				findBox.BindToForDescription = "Z0_Description";
				findBox.DescriptionBox.Clear();
				popupDecisionProvider.HandleFindBoxOKButton(new[] { dummyChild2 });
				AssertNullOrEmpty("Do not default Description value if BindToForDescription is not empty", findBox.DescriptionBox.Text);
			}
		}

		public void TestEmptyDescriptionIfCodeNotFound()
		{
			var dummy = Factory.New<DummyWithCodes>();
			using (var form = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				codeFindBox.BindTo = "Code";
				form.Controls.Add(codeFindBox);
				form.Show();

				Application.DoEvents();

				AssertEquals("Default value is false", false, codeFindBox.EmptyDescriptionIfCodeNotFound);

				codeFindBox.EmptyDescriptionIfCodeNotFound = true;

				AssertEquals("Vaue changed", true, codeFindBox.EmptyDescriptionIfCodeNotFound);

				codeFindBox.CodeBox.Text = "AFD";
				codeFindBox.UpdateDescriptionFromCodeExposed();

				AssertEquals("Empty Description for invalid code", string.Empty, codeFindBox.Description);

				codeFindBox.EmptyDescriptionIfCodeNotFound = false;
				codeFindBox.CodeBox.Text = "AH3";
				codeFindBox.UpdateDescriptionFromCodeExposed();

				AssertEquals("Invalid selection Description for invalid code", Constants.FindBoxMessages.InvalidSelection, codeFindBox.Description);
			}
		}

		public void TestBindToForDescription_BindTwiceBeforeIdleWorkerFlush()
		{
			var dummy1 = Factory.New<DummyWithCodes>();
			dummy1.Code = "AAA";

			var dummy2 = Factory.New<DummyWithCodes>();
			dummy2.Code = "BBB";

			using (var form = new ZForm())
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				form.Controls.Add(codeFindBox);
				form.Show();

				codeFindBox.BindTo = "Code";
				codeFindBox.BindToForDescription = "Code";
				codeFindBox.SetDataBinding(dummy1, "Code");
				codeFindBox.SetDataBinding(dummy2, "Code");

				AssertNoExceptionThrown(() =>
				{
					UserIdleWorker.Flush();
				});

				AssertEquals("BBB", codeFindBox.DescriptionBox.Text);
			}
		}

		public void TestDescriptionChangedWithSameCode()
		{
			var dummy1 = Factory.New<DummyWithCodes>();
			dummy1.Z0_Code = "AAA";
			dummy1.Z0_Description = "AAADescription";
			using (var form = new ZForm())
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				form.Controls.Add(codeFindBox);
				form.Show();

				codeFindBox.BindTo = "Z0_Code";
				codeFindBox.SetDataBinding(dummy1, "Z0_Code");
				UserIdleWorker.Flush();
				AssertEquals("AAADescription", codeFindBox.DescriptionBox.Text);
				dummy1.Z0_Description = "BBBDescription";
				codeFindBox.Description = "BBBDescription";
				codeFindBox.CodeBox.Focus();
				codeFindBox.DescriptionBox.Focus();
				UserIdleWorker.Flush();
				AssertEquals("BBBDescription", codeFindBox.DescriptionBox.Text);
			}
		}

		public void TestBindingForCodeFindBoxCodeBox_RefreshImmediatelyWhenLostFocus()
		{
			var dummy = Factory.New<DummyWithCodes>();
			dummy.CodeWithFormat = "AAA";
			using (var form = new ZForm())
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				form.Controls.Add(codeFindBox);
				form.Show();
				codeFindBox.SetDataBinding(dummy, "CodeWithFormat");
				AssertEquals("AAA IS FORMATTED", codeFindBox.CodeBox.Text);

				codeFindBox.CodeBox.Focus();
				codeFindBox.CodeBox.Text = "BBB";
				codeFindBox.DescriptionBox.Focus();
				AssertEquals("BBB IS FORMATTED", codeFindBox.CodeBox.Text);
				AssertNotEquals("BBB", codeFindBox.CodeBox.Text);
			}
		}

		public void TestChangingBothCodeAndDescriptionBeforeAnIdleWorkerFlush()
		{
			var dummy = Factory.New<DummyWithCodes>();

			var dummy1 = dummy.Codes.AddNew();
			dummy1.Z0_Code = "A1";
			dummy1.Z0_Description = "Aaa Aaa";

			var dummy2 = dummy.Codes.AddNew();
			dummy2.Z0_Code = "B2";
			dummy2.Z0_Description = "Bbb Bbb";

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				codeFindBox.BindTo = "Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();

				codeFindBox.CodeBox.Text = "A1";
				codeFindBox.DescriptionBox.Text = "Bbb Bbb";
				UserIdleWorker.Flush();

				AssertEquals("B2", codeFindBox.CodeBox.Text);
				AssertEquals("Bbb Bbb", codeFindBox.DescriptionBox.Text);

				codeFindBox.DescriptionBox.Text = ZString.Empty;
				codeFindBox.CodeBox.Text = ZString.Empty;
				UserIdleWorker.Flush();

				codeFindBox.DescriptionBox.Text = "Bbb Bbb";
				codeFindBox.CodeBox.Text = "A1";
				UserIdleWorker.Flush();

				AssertEquals("A1", codeFindBox.CodeBox.Text);
				AssertEquals("Aaa Aaa", codeFindBox.DescriptionBox.Text);
			}
		}

		public void TestDescriptionUpdatesOnTextChange()
		{
			CreateDummies();
			Dummy.SS_Dummy = ZString.Empty;
			Dummy.SS_Name = ZString.Empty;

			CreateControls(Form, "SS_Dummy");
			Form.Show();

			SetValueBoundToToGetAABCDTestValues();
			UserIdleWorker.Flush();
			AssertEquals("ZCodeFindBox.CodeBox.Text", "AABCD", ZCodeFindBox.CodeBox.Text);
			AssertEquals("ZCodeFindBox.DescriptionBox.Text", "AABCD Description", ZCodeFindBox.DescriptionBox.Text);
		}

		public void TestCodeAndDescriptionSetFromRetrieve()
		{
			CreateDummies();
			Dummy.SS_Dummy = "AABCD";

			CreateControls(Form, "SS_Dummy");
			Form.Show();
			Application.DoEvents();
			UserIdleWorker.Flush();

			AssertEquals("List after Show", Dummy.Dummies, ZCodeFindBox.List);
			AssertEquals("Code after Show", "AABCD", ZCodeFindBox.CodeBox.Text);
			AssertEquals("Description after Show", "AABCD Description", ZCodeFindBox.DescriptionBox.Text);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestMaxLength()
		{
			CreateDummies();
			CreateControls(Form, "SS_Dummy");
			Form.Show();
			Application.DoEvents();

			Dummy.SS_Dummy_MaxLength = 3;
			ZCodeFindBox.CodeBox.Focus();
			ZCodeFindBox.CodeBox.Text = "ExceedsMaxLength";
			KEndCurrentEdit.EndCurrentEdit(ZCodeFindBox.CodeBox);
		}

		public void TestCodeAndDescriptionSetFromRetrieveWithMaxLength()
		{
			CreateDummies();
			Dummy.SS_Dummy = "AABCD";
			Dummy.SS_Dummy_MaxLength = 5;

			CreateControls(Form, "SS_Dummy");
			Form.Show();

			AssertEquals("List after Show", Dummy.Dummies, ZCodeFindBox.List);
			AssertEquals("Code after Show", "AABCD", ZCodeFindBox.CodeBox.Text);
			UserIdleWorker.Flush();
			AssertEquals("Description after Show", "AABCD Description", ZCodeFindBox.DescriptionBox.Text);
		}

		public void TestBindingAfterShow()
		{
			CreateDummies();
			Dummy.SS_Dummy = "AABCD";

			Form.Show();
			CreateControls(Form, "SS_Dummy");

			AssertEquals("List available after Show", Dummy.Dummies, ZCodeFindBox.List);
			AssertEquals("Code after Show", "AABCD", ZCodeFindBox.CodeBox.Text);
			UserIdleWorker.Flush();
			AssertEquals("Description after Show", "AABCD Description", ZCodeFindBox.DescriptionBox.Text);
		}

		public void TestCodeAndDescriptionSetFromInputAndValidate()
		{
			CreateDummies();
			CreateControls(Form, "");
			Form.Show();
			FindBox.Focus();

			AssertEquals("List after Show", Dummy.Dummies, ZCodeFindBox.List);
			AssertEquals("Code after Show", "", ZCodeFindBox.CodeBox.Text);
			UserIdleWorker.Flush();
			AssertEquals("Description after Show", "{None Selected}", ZCodeFindBox.DescriptionBox.Text);

			SendKeyPressToCodeBox('A');
			SendKeyPressToCodeBox('=');
			UserIdleWorker.Flush();

			AssertEquals("Code set FindBox after AutoComplete", "AABCD", ZCodeFindBox.CodeBox.Text);
			AssertEquals("Description of AABCD", "AABCD Description", ((IFindBoxListProvider)FindBox).DescriptionFromCode("AABCD"));
			AssertEquals("Description set FindBox after AutoComplete", "AABCD Description", ZCodeFindBox.DescriptionBox.Text);
			AssertEquals("Code set in BusinessObject before changing focus", "", Dummy.SS_Dummy);

			ChangeFocusToInvokeBinding();

			AssertEquals("Code set in BusinessObject after changing focus", "AABCD", Dummy.SS_Dummy);
			AssertEquals("Code set FindBox after changing focus", "AABCD", ZCodeFindBox.CodeBox.Text);
			AssertEquals("Code set FindBox after changing focus", "AABCD Description", ZCodeFindBox.DescriptionBox.Text);
		}

		public void TestReadOnly()
		{
			CreateControls(Form, "");
			Form.Show();

			AssertEquals("Initial FindBox DescriptionBox ReadOnly state", true, ZCodeFindBox.DescriptionBox.ReadOnly);
			AssertEquals("Initial FindBox CodeBox ReadOnly state", false, ZCodeFindBox.CodeBox.ReadOnly);

			Dummy.SS_Dummy_ReadOnly = true;

			AssertEquals("FindBox DescriptionBox ReadOnly state after setting ReadOnly on BizObj to true", true, ZCodeFindBox.DescriptionBox.ReadOnly);
			AssertEquals("FindBox CodeBox ReadOnly state after setting ReadOnly on BizObj to true", true, ZCodeFindBox.CodeBox.ReadOnly);

			Dummy.SS_Dummy_ReadOnly = false;

			AssertEquals("FindBox DescriptionBox ReadOnly state after setting ReadOnly on BizObj to false", true, ZCodeFindBox.DescriptionBox.ReadOnly);
			AssertEquals("FindBox CodeBox ReadOnly state after setting ReadOnly on BizObj to false", false, ZCodeFindBox.CodeBox.ReadOnly);
		}

		public void TestBindToList()
		{
			CreateControls(Form, "");
			Form.Show();

			AssertEquals("FindBox List after binding", Dummy.Dummies, FindBox.List);
		}

		#region Reference by Description

		public void TestCanReferenceByDescription()
		{
			var dummy = Factory.New<DummyWithCodes>();

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				codeFindBox.BindTo = "Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				Assert(codeFindBox.CanReferenceByDescriptionExposed);
			}

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				codeFindBox.BindTo = "Z0_Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				Assert(!codeFindBox.CanReferenceByDescriptionExposed);
			}
		}

		public void TestDescriptionBox()
		{
			using (var codeFindBox = new ZCodeFindBox())
			{
				AssertEquals("CodeFindBoxDescriptionBox", codeFindBox.DescriptionBox.GetType().Name);
			}
		}

		public void TestDescriptionBoxReadOnly()
		{
			var dummy = Factory.New<DummyWithCodes>();

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox())
			{
				codeFindBox.BindTo = "Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				codeFindBox.ReadOnly = true;
				Assert(codeFindBox.DescriptionBox.ReadOnly);

				codeFindBox.ReadOnly = false;
				Assert("Editable for referenceable description", !codeFindBox.DescriptionBox.ReadOnly);

				Assert(codeFindBox.DescriptionBox.TabStop);
			}

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox())
			{
				codeFindBox.BindTo = "Z0_Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				codeFindBox.ReadOnly = true;
				Assert(codeFindBox.DescriptionBox.ReadOnly);

				codeFindBox.ReadOnly = false;
				Assert("Always readonly for non-referenceable description", codeFindBox.DescriptionBox.ReadOnly);

				Assert(!codeFindBox.DescriptionBox.TabStop);
			}
		}

		public void TestICustomizableFindBoxPopup()
		{
			var dummy = Factory.New<DummyWithCodes>();

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox())
			{
				codeFindBox.BindTo = "Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				codeFindBox.CodeBox.Text = "AAA";
				codeFindBox.ActiveControl = codeFindBox.CodeBox;
				AssertEquals("AAA", ((ICustomizableFindBoxPopup)codeFindBox).CodeForPopup);
				AssertEquals("Z0_Code", ((ICustomizableFindBoxPopup)codeFindBox).PropertyNameForPopup);

				codeFindBox.DescriptionBox.Text = "Aaa Aaa";
				codeFindBox.ActiveControl = codeFindBox.DescriptionBox;
				AssertEquals("Aaa Aaa", ((ICustomizableFindBoxPopup)codeFindBox).CodeForPopup);
				AssertEquals("Z0_Description", ((ICustomizableFindBoxPopup)codeFindBox).PropertyNameForPopup);

				dummy.GetCodePropertyNameForTesting = new Func<Type, string>((x) => "CodeWithFormat");
				codeFindBox.InvalidateList();
				AssertEquals("CodeWithFormat", ((ICustomizableFindBoxPopup)codeFindBox).PropertyNameForPopup);
			}
		}

		public void TestCodeFromDescription()
		{
			var dummy = Factory.New<DummyWithCodes>();

			var dummy1 = dummy.Codes.AddNew();
			dummy1.Z0_Code = "A1";
			dummy1.Z0_Description = "Aaa Aaa";

			var dummy2 = dummy.Codes.AddNew();
			dummy2.Z0_Code = "B2";
			dummy2.Z0_Description = "Bbb Bbb";

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				codeFindBox.BindTo = "Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				AssertEquals("A1", codeFindBox.CodeFromDescriptionExposed("Aaa Aaa"));
				AssertEquals("B2", codeFindBox.CodeFromDescriptionExposed("Bbb Bbb"));
				AssertEquals("", codeFindBox.CodeFromDescriptionExposed(""));
				AssertEquals("?", codeFindBox.CodeFromDescriptionExposed("XYZ"));
				AssertEquals("?", codeFindBox.CodeFromDescriptionExposed("A1"));
			}
		}

		public void TestAutoCompleteDescription()
		{
			var dummy = Factory.New<DummyWithCodes>();

			var dummy1 = dummy.Codes.AddNew();
			dummy1.Z0_Code = "A1";
			dummy1.Z0_Description = "Aaa Aaa";

			var dummy2 = dummy.Codes.AddNew();
			dummy2.Z0_Code = "B2";
			dummy2.Z0_Description = "Bbb Bbb";

			using (var testForm = new ZForm(dummy))
			using (var codeFindBox = new ZCodeFindBox4Test())
			{
				codeFindBox.BindTo = "Code";
				testForm.Controls.Add(codeFindBox);

				testForm.Show();
				Application.DoEvents();

				codeFindBox.DescriptionBox.Text = "a";
				codeFindBox.AutoCompleteDescriptionExposed();
				AssertEquals("Aaa Aaa", codeFindBox.DescriptionBox.Text);
				AssertEquals("A1", codeFindBox.CodeBox.Text);

				codeFindBox.DescriptionBox.Text = "b";
				codeFindBox.AutoCompleteDescriptionExposed();
				AssertEquals("Bbb Bbb", codeFindBox.DescriptionBox.Text);
				AssertEquals("B2", codeFindBox.CodeBox.Text);

				codeFindBox.DescriptionBox.Text = "";
				codeFindBox.AutoCompleteDescriptionExposed();
				AssertEquals("", codeFindBox.DescriptionBox.Text);
				AssertEquals("", codeFindBox.CodeBox.Text);

				codeFindBox.DescriptionBox.Text = "x";
				codeFindBox.AutoCompleteDescriptionExposed();
				AssertEquals("x", codeFindBox.DescriptionBox.Text);
				AssertEquals("?", codeFindBox.CodeBox.Text);
			}
		}

		#region Test Classes

		internal class ZCodeFindBox4Test : ZCodeFindBox
		{
			internal bool IsAutoCompleted;

			public bool CanReferenceByDescriptionExposed
			{
				get { return CanReferenceByDescription; }
			}

			public new string Description
			{
				get => base.Description;
				set => base.Description = value;
			}

			public void UpdateDescriptionFromCodeExposed() => UpdateDescriptionFromCode();

			public string CodeFromDescriptionExposed(string code)
			{
				return CodeFromDescription(code);
			}

			public void AutoCompleteDescriptionExposed()
			{
				AutoCompleteDescription(true);
			}

			protected override bool AutoCompleteText(bool explicitAutoComplete)
			{
				IsAutoCompleted = true;
				return base.AutoCompleteText(explicitAutoComplete);
			}
		}

		#endregion

		#endregion

		#region IDataBoundControl

		public void TestDataSourceType()
		{
			using (var findBox = new ZCodeFindBox())
			{
				AssertEquals(typeof(ZString), findBox.DataSourceType);
			}
		}

		#endregion

		#region IFetchHintGenerator

		public void TestAddFetchHint()
		{
			using (var findBox = new ZCodeFindBox())
			{
				var bizObj = Factory.New<DummyDependantBusinessObject>();
				bizObj.ZD1_Code = "Code";
				findBox.BindTo = "ZD1_Code";
				findBox.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
				AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

				((IFetchHintGenerator)findBox).AddFetchHint(bizObj, "");
				AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			}
		}

		#endregion

		#region Implementation

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
