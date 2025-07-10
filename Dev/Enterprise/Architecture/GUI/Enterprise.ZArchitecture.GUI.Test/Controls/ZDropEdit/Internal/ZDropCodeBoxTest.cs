using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropCodeBoxTest : TestCaseWithFactory
	{
		public void TestShouldNotBeAccessingAPropertyOnADeletedBusinessObject()
		{
			var huge = Factory.New<HugeDummy>();
			var super1 = huge.SupperDummyCollection.AddNew();
			super1.Z0_FK_Code = "ONE";
			super1.Huge = huge;
			var super2 = huge.SupperDummyCollection.AddNew();
			super2.Z0_FK_Code = "TWO";
			super2.Huge = huge;

			using (var testForm = new TestDropEditForm(huge))
			{
				var dropEdit = testForm.DropEdit;
				dropEdit.BindTo = "SupperDummyCollection." + DummyBusinessObject.Schema.Z0_FK_Code;
				dropEdit.BindToList = "SupperDummyCollection.Lookups+SortedInvoiceList";
				testForm.Show();

				var codeBox = dropEdit.CodeBox;
				AssertEquals(2, dropEdit.List.Count);
				huge.SupperDummyCollection.Remove(super1);
				AssertNoExceptionThrown(() =>
				{
					codeBox.UpdateSelectedIndexAndDescription();
					super1.Delete();
					codeBox.UpdateSelectedIndexAndDescription();
				});
			}
		}

		public void TestAutoCompleteTextWithCultureAndCase()
		{
			using (var form = new ZForm())
			{
				var superDummy = Factory.NewWithValidTestData<SuperDummyBusinessObject>();
				var parent = new ZDropEdit();
				parent.Parent = form;
				form.Show();

				parent.SetDataBinding(superDummy, "SS_Name");

				parent.CodeBox.CharacterCasing = CharacterCasing.Upper;
				parent.CodeBox.AutoCompleteText(new KeyPressEventArgs('a'));
				parent.CodeBox.AutoCompleteText(new KeyPressEventArgs('a'));
				AssertEquals("AA", parent.CodeBox.Text);

				parent.CodeBox.Text = string.Empty;
				parent.CodeBox.CharacterCasing = CharacterCasing.Lower;
				parent.CodeBox.AutoCompleteText(new KeyPressEventArgs('A'));
				parent.CodeBox.AutoCompleteText(new KeyPressEventArgs('A'));
				AssertEquals("aa", parent.CodeBox.Text);

				parent.CodeBox.Text = string.Empty;
				parent.CodeBox.CharacterCasing = CharacterCasing.Upper;
				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("tr-TR")))
				using (Culture.SetTemporarily(EnvProxy.Instance.CurrentCompany.Country.Culture))
				{
					parent.CodeBox.AutoCompleteText(new KeyPressEventArgs('i'));
					parent.CodeBox.AutoCompleteText(new KeyPressEventArgs('i'));
					AssertEquals("II", parent.CodeBox.Text);
				}
			}
		}

		public void TestRunSelectedIndexChangedEventWithLongTime()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			var key = "Whatever blah blah";
			Action action = () =>
			{
				using (var actionForm = new TestDropEditForm(dummyWithList))
				{
					actionForm.DropEdit.ShowDescriptionBox = true;
					actionForm.DropEdit.SelectedIndexChanged += (sender, e) => { Thread.Sleep(TimeSpan.FromSeconds(6)); };
					actionForm.Show();
					Application.DoEvents();
					key = $"SlowMethodOnZDropEditSelectedIndexChangedEvent.{actionForm.Name ?? "UNK_FORM"}.{actionForm.DropEdit.Name}.{"Z0_FK_Code"}";

					dummyWithList.Z0_FK_Code = "SIX";
					actionForm.DropEdit.CodeBox.UpdateSelectedIndexAndDescription();
				}
			};

			ErrorReporter.Clear();
			try
			{
				action();
				AssertEquals("Error should not be reported - 1 action()", false, ErrorReporter.HasBeenReported(key));
				action();
				AssertEquals("Error should be reported - 2 action()", true, ErrorReporter.HasBeenReported(key));
			}
			finally
			{
				ErrorReporter.Clear();
				ZDropCodeBox.ReportedLongRunningControls.Clear();
			}
		}

		public void TestRunSelectedIndexChangedWhenNotShowDescriptionBox()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();

			using (var testForm = new TestDropEditForm(dummyWithList))
			{
				testForm.DropEdit.ShowDescriptionBox = false;
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Selected Index is default to -1", -1, testForm.DropEdit.SelectedIndex);

				dummyWithList.Z0_FK_Code = "SIX";

				AssertNotEquals("Selected Index is updated when ShowDescriptionBox is false", -1, testForm.DropEdit.SelectedIndex);
			}
		}

		public void TestSelectedIndexNotChangedWhenParentDropEditBindToDescriptionIsNotEmpty()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();

			using (var testForm = new TestDropEditForm(dummyWithList))
			{
				testForm.DropEdit.ShowDescriptionBox = false;
				testForm.DropEdit.BindToForDescription = "Z0_FK_Code";
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Selected Index is default to -1", -1, testForm.DropEdit.SelectedIndex);

				dummyWithList.Z0_FK_Code = "SIX";

				AssertEquals("Selected Index is not updated when BindToForDescription is not empty", -1, testForm.DropEdit.SelectedIndex);
			}
		}

		public void TestOnMaxLengthChange()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();

			using (var testForm = new TestDropEditForm(dummyWithList))
			{
				testForm.DropEdit.ShouldResizeByMaxLength = true;
				var width = ControlDpiScalingHelper.ScaleToCurrentDpiX(4500);
				testForm.DropEdit.SetControlWidth(width);
				AssertEquals(width, testForm.DropEdit.Width);

				testForm.DropEdit.ShouldResizeByMaxLength = false;
				width = ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
				testForm.DropEdit.SetControlWidth(width);
				AssertNotEquals(width, testForm.DropEdit.Width);
			}
		}

		public void TestRunFindItemWithLongTime()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			var key = "Whatever blah blah";

			Action action = () =>
			{
				using (var actionForm = new TestLongTimeListGetterDropEditForm(dummyWithList))
				{
					actionForm.DropEdit.ShowDescriptionBox = true;
					actionForm.Show();
					Application.DoEvents();
					key = $"SlowMethodOnZDropEditFindItemExact.{actionForm.Name ?? "UNK_FORM"}.{actionForm.DropEdit.Name}.{"Z0_FK_Code"}";

					dummyWithList.Z0_FK_Code = "SIX";
				}
			};

			ErrorReporter.Clear();
			try
			{
				action();
				AssertEquals("Error should not be reported - 1 action()", false, ErrorReporter.HasBeenReported(key));
				action();
				AssertEquals("Error should be reported - 2 action()", true, ErrorReporter.HasBeenReported(key));
			}
			finally
			{
				ErrorReporter.Clear();
				ZDropCodeBox.ReportedLongRunningControls.Clear();
			}
		}

		#region Class TestLongTimeListGetterDropEditForm

		public class TestLongTimeListGetterDropEditForm : ZChildForm
		{
			public LongTimeListGetterDropEdit DropEdit;
			public TextBox ZCalcEdit1;
			public TextBox ZCalcEdit2;

			public TestLongTimeListGetterDropEditForm(BusinessObject bizObj)
				: base(bizObj)
			{
			}

			protected virtual string BindToPrefix => "";

			#region Windows Form Designer generated code

			protected override void InitializeComponent()
			{
				this.ZCalcEdit1 = new TextBox();
				this.ZCalcEdit2 = new TextBox();
				this.DropEdit = new LongTimeListGetterDropEdit();
				this.SuspendLayout();
				//
				// zCalcEdit1
				//
				this.ZCalcEdit1.Location = new System.Drawing.Point(16, 72);
				this.ZCalcEdit1.Name = "ZCalcEdit1";
				this.ZCalcEdit1.TabIndex = 5;
				this.ZCalcEdit1.Text = "ZCALCEDIT1";
				this.ZCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
				//
				// zCalcEdit2
				//
				this.ZCalcEdit2.Location = new System.Drawing.Point(176, 72);
				this.ZCalcEdit2.Name = "ZCalcEdit2";
				this.ZCalcEdit2.TabIndex = 6;
				this.ZCalcEdit2.Text = "ZCALCEDIT1";
				this.ZCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
				//
				// DropEdit
				//
				this.DropEdit.BindTo = BindToPrefix + DummyBusinessObject.Schema.Z0_FK_Code;
				this.DropEdit.BindToList = BindToPrefix + "DummyList";
				this.DropEdit.Location = new System.Drawing.Point(24, 8);
				this.DropEdit.Name = "DropEdit";
				this.DropEdit.Size = new System.Drawing.Size(232, 20);
				this.DropEdit.TabIndex = 7;
				this.DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				//
				// Form1
				//
				this.ClientSize = new System.Drawing.Size(296, 129);
				this.Controls.Add(this.DropEdit);
				this.Controls.Add(this.ZCalcEdit2);
				this.Controls.Add(this.ZCalcEdit1);
				this.Name = "Form1";
				this.Text = "Form1";
				this.ResumeLayout(false);
			}

			#endregion
		}

		[ToolboxItem(false)]
		public class LongTimeListGetterDropEdit : ZDropEdit
		{
			public override bool TestRunFindItemWithLongTime
			{
				get
				{
					return true;
				}
			}
		}

		#endregion

		public void TestOnKeyPress()
		{
			using (var parent = new ZDropEdit())
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("CD1", "Description 1");
				list.AddPair("CD2", "Description 2");
				list.AddPair("CD3", "Description 3");

				parent.List = list;

				var codeBox = new ZDropCodeBox();
				codeBox.Parent = parent;

				AssertOnKeyPress(codeBox, "CD24", "CD2", ZDropEdit.ShowInDropDownList.ShowCodeAndDescription);
				AssertOnKeyPress(codeBox, "CD34", "CD3", ZDropEdit.ShowInDropDownList.OnlyShowCode);
				AssertOnKeyPress(codeBox, "Description 25", "DESCRIPTION 2", ZDropEdit.ShowInDropDownList.OnlyShowDescription);
			}
		}

		void AssertOnKeyPress(ZDropCodeBox codeBox, string input, string expectedOutput, ZDropEdit.ShowInDropDownList showInDropDown)
		{
			((ZDropEdit)codeBox.Parent).ShowInDropDown = showInDropDown;
			codeBox.Text = input;
			codeBox.SelectionLength = 0;
			codeBox.SelectionStart = codeBox.Text.Length;

			KeySender.SendKeyPress(codeBox, Keys.Back);

			AssertEquals(expectedOutput, codeBox.Text);
		}

		[ExpectNoExceptions]
		public void TestKeyDownWithNullParent_ShouldNotThrowException()
		{
			using (var box = new ZDropCodeBoxWithKeyDownExposed())
			{
				var args = new KeyEventArgs(Keys.A);
				box.PerformKeyDown(args);
			}
		}

		class ZDropCodeBoxWithKeyDownExposed : ZDropCodeBox
		{
			public void PerformKeyDown(KeyEventArgs e)
			{
				OnKeyDown(e);
			}
		}
	}
}
