using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropEditTest : TestCaseWithDummy
	{
		public void TestUserIdleWorkItemOptions()
		{
			using (var dropParent = new ZDropEdit())
			{
				AssertEquals("Default option is None, override me!", UserIdleWorkItemOptions.None, dropParent.WorkItemOption);
			}
		}

		public void TestMouseWithinBoundsOfTheButton()
		{
			// in order to reproduce, comment out all of the changes from this changeset
			// build, run and press on dropdown move your mouse for a second and press on dropdown
			// button again.
			using (var dropParent = new ZDropEdit())
			{
				var parent = (IDropFormParent)dropParent;
				dropParent.ShowDropDown();
				AssertEquals("drop down is closed", false, parent.ShouldCloseOnMouseDown(dropParent.DropButton.PointToScreen(Point.Empty)));

				dropParent.ShowDropDown();
				AssertEquals("drop down is closed but mouse click wasn't on button", true, parent.ShouldCloseOnMouseDown(dropParent.DropButton.PointToScreen(Point.Empty - new Size(50, 50))));
			}
		}

		public void TestIsOnGrid()
		{
			using (var edit = new ZDropEdit())
			{
				((IIsOnGrid)edit).IsOnGrid = true;
				ZDropButton button = null;
				foreach (Control control in edit.Controls)
				{
					button = control as ZDropButton;
					if (button != null)
					{
						break;
					}
				}
				AssertNotNull("Couldnt find button", button);
				Assert("Did not make child ZDropButton IsOnGrid", button.IsOnGrid);
			}
		}

		public void TestBindToDescription()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();
			AssertEquals("Precondition: DummyWithList.Z0_Description", "Default", dummyWithList.Z0_Description);

			using (TestForm = new TestDropEditFormWithDescriptionBound(dummyWithList))
			{
				TestForm.Show();
				Application.DoEvents();

				AssertEquals("DropDown Description", "Default", DropEdit.DescriptionBox.Text);

				dummyWithList.Z0_Code = "ONE";
				AssertEquals("DropDown Description", "Default", DropEdit.DescriptionBox.Text);

				dummyWithList.Z0_Code = "";
				AssertEquals("DropDown Description", "Default", DropEdit.DescriptionBox.Text);

				dummyWithList.Z0_Description = "Real Description";
				AssertEquals("DropDown Description", "Real Description", DropEdit.DescriptionBox.Text);

				dummyWithList.Z0_Description = "";
				AssertEquals("DropDown Description", "", DropEdit.DescriptionBox.Text);
			}
		}

		public void TestBindToForDescription_BindTwiceBeforeIdleWorkerFlush()
		{
			var dummy1 = Factory.New<DummyWithCodeDescriptionPairList>();
			dummy1.Z0_Code = "ONE";

			var dummy2 = Factory.New<DummyWithCodeDescriptionPairList>();
			dummy2.Z0_Code = "TWO";

			using (var form = new ZForm())
			using (var dropEdit = new ZDropEdit())
			{
				form.Controls.Add(dropEdit);
				form.Show();

				dropEdit.BindTo = "Z0_Code";
				dropEdit.BindToForDescription = "Z0_Code";
				dropEdit.BindToList = "DummyList";
				dropEdit.SetDataBinding(dummy1, "Z0_Code");
				dropEdit.SetDataBinding(dummy2, "Z0_Code");

				AssertEquals("TWO", dropEdit.DescriptionBox.Text);
				AssertEquals("Selected Index shouldn't change when BindToForDescription is not empty", -1, dropEdit.SelectedIndex);
			}
		}

		public void TestBindingForDropEditCodeBox_RefreshImmediatelyWhenLostFocus()
		{
			var dummy = Factory.New<DummyWithCodes>();
			dummy.CodeWithFormat = "AAA";
			using (var form = new ZForm())
			using (var dropEdit = new ZDropEdit())
			{
				form.Controls.Add(dropEdit);
				form.Show();
				dropEdit.SetDataBinding(dummy, "CodeWithFormat");
				AssertEquals("AAA IS FORMATTED", dropEdit.CodeBox.Text);

				dropEdit.CodeBox.Focus();
				dropEdit.CodeBox.Text = "BBB";
				dropEdit.DescriptionBox.Focus();
				AssertEquals("BBB IS FORMATTED", dropEdit.CodeBox.Text);
				AssertNotEquals("BBB", dropEdit.CodeBox.Text);
			}
		}

		public void TestBindingForDropEditCodeBox_AddCodeBindingWhenVisible()
		{
			var dummy = Factory.New<DummyWithCodes>();
			using (var form = new ZForm())
			using (var dropEdit = new ZDropEdit())
			{
				dropEdit.Visible = true;
				form.Controls.Add(dropEdit);
				form.Show();
				dropEdit.SetDataBinding(dummy, "Code");

				Thread.Sleep(200);
				Application.DoEvents();

				Assert(dropEdit.CodeBindingAdded);
			}
		}

		public void TestBindingForDropEditCodeBox_AddCodeBindingEvenWhenNotVisible()
		{
			var dummy = Factory.New<DummyWithCodes>();
			using (var form = new ZForm())
			using (var dropEdit = new ZDropEdit())
			{
				dropEdit.Visible = false;
				form.Controls.Add(dropEdit);
				form.Show();
				dropEdit.SetDataBinding(dummy, "Code");

				Thread.Sleep(200);
				Application.DoEvents();

				Assert(dropEdit.CodeBindingAdded);
			}
		}

		public void TestChangingList()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();

			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();

				dummyWithList.SetList(true);
				AssertEquals("DropEdit Text", "ONE", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "Description 01", DropEdit.DescriptionBox.Text);
				AssertEquals("Validation Succeeded, No Errors", false, dummyWithList.Z0_FK_CodeInfo.HasErrors());

				dummyWithList.SetList(false);
				AssertEquals("DropEdit Text", "EIGHT", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "Description 08", DropEdit.DescriptionBox.Text);
				AssertEquals("Validation Succeeded, No Errors", false, dummyWithList.Z0_FK_CodeInfo.HasErrors());
			}
		}

		public void TestInvalidateList()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				var initialList = TestForm.DropEdit.List;
				AssertEquals(initialList, TestForm.DropEdit.List);
				TestForm.DropEdit.InvalidateList();
				Assert("List should change after Invalidate, but did not", initialList != TestForm.DropEdit.List);
			}
		}

		public void TestDisableInvalidation()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				var initialList = TestForm.DropEdit.List;
				AssertNotEquals("Initial list not null", null, initialList);
				TestForm.DropEdit.DisableInvalidation = true;
				TestForm.DropEdit.InvalidateList();
				Assert("List should not have been invalidated", initialList == TestForm.DropEdit.List);
			}
		}

		public void TestDropEdit()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				Application.DoEvents();

				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				TestForm.DropEdit.Focus();

				AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);
				SendKeyToDropEdit(Keys.F4);
				AssertEquals("DropDown Visible", true, DropEdit.IsDroppedDown);

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropEdit Text", "TWO", DropEdit.CodeBox.Text);
				AssertEquals("DropEdit Selection Start", 1, DropEdit.CodeBox.SelectionStart);
				AssertEquals("DropEdit Selection Length", 2, DropEdit.CodeBox.SelectionLength);
				AssertEquals("DropDown Description", "Description 02", DropEdit.DescriptionBox.Text);

				SendKeyToDropEdit(Keys.E);
				AssertEquals("DropEdit Text", "TEN", DropEdit.CodeBox.Text);
				AssertEquals("DropEdit Selection Start", 2, DropEdit.CodeBox.SelectionStart);
				AssertEquals("DropEdit Selection Length", 1, DropEdit.CodeBox.SelectionLength);
				AssertEquals("DropDown Description", "Description 10", DropEdit.DescriptionBox.Text);

				SendKeyToDropEdit(Keys.N);
				AssertEquals("DropEdit Text", "TEN", DropEdit.CodeBox.Text);
				AssertEquals("DropEdit Selection Start", 3, DropEdit.CodeBox.SelectionStart);
				AssertEquals("DropEdit Selection Length", 0, DropEdit.CodeBox.SelectionLength);
				AssertEquals("DropDown Description", "Description 10", DropEdit.DescriptionBox.Text);

				SendKeyToDropEdit(Keys.Enter);
				AssertEquals("DropDown Visible", false, DropEdit.IsDroppedDown);
				AssertEquals("DropDown Description", "Description 10", DropEdit.DescriptionBox.Text);

				SendKeyToDropEdit(Keys.Enter);
				AssertEquals("Bound BizObj Value", "TEN", dummyWithList.Z0_FK_Code);
				AssertEquals("DropEdit Focused", false, DropEdit.ContainsFocus);
			}
		}

		public void TestDescriptionBoxIsProperlyAnchored()
		{
			using (var dropEdit = new ZDropEdit())
			{
				AssertEquals(AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, dropEdit.DescriptionBox.Anchor);
			}
		}

		public void TestMultilingualCodes()
		{
			using (var mockRes = Res.UseMockData())
			using (var grmRes = Res.GetLanguageInstance(SharedConstants.Languages.German).UseMockData())
			{
				var dummyWithList = Factory.New<DummyWithMultilingualCodePairList>();

				mockRes.Put("key", new ResourceStringData("key", "ONE"));
				grmRes.Put("key", new ResourceStringData("key", "GRM-ONE"));
				var accessedToVerifyNotDeleted = dummyWithList.Z0_Code;

				if (dummyWithList.DummyList == null)
				{
					var list = new CodeDescriptionPairList();
					list.Add(new CodeDescriptionPair(ResString.GetMultilingualString("key", "ONE"), "Description 01"));
					dummyWithList.DummyList = list;
				}

				using (TestForm = new TestDropEditForm(dummyWithList))
				{
					TestForm.Show();
					Application.DoEvents();

					AssertEquals("DropEdit List Length", 1, DropEdit.List.Count);
					AssertNotNull("ZDropEdit should be instatiated.", DropEdit);
					TestForm.DropEdit.Focus();
					AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);
					SendKeyToDropEdit(Keys.F4);
					AssertEquals("DropDown Visible", true, DropEdit.IsDroppedDown);

					TestForm.DropEdit.Focus();
					SendKeyToDropEdit(Keys.O);
					SendKeyToDropEdit(Keys.Enter);
					AssertEquals("DropEdit Text", "ONE", DropEdit.CodeBox.Text);
					AssertEquals("DropEdit Description", "Description 01", DropEdit.DescriptionBox.Text);
				}

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.German))
				{
					using (TestForm = new TestDropEditForm(dummyWithList))
					{
						TestForm.Show();
						Application.DoEvents();

						AssertEquals("DropEdit List Length", 1, DropEdit.List.Count);

						AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
						TestForm.DropEdit.Focus();
						AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);
						SendKeyToDropEdit(Keys.F4);
						AssertEquals("DropDown Visible", true, DropEdit.IsDroppedDown);

						TestForm.DropEdit.Focus();
						SendKeyToDropEdit(Keys.O);
						SendKeyToDropEdit(Keys.Enter);
						AssertEquals("No value starting with 'o' in the list, ONE has been translated to GRM-ONE", "O", DropEdit.CodeBox.Text);
						AssertEquals("O is stored in business object", "O", dummyWithList.Z0_FK_Code);

						TestForm.DropEdit.Focus();
						SendKeyToDropEdit(Keys.G);
						SendKeyToDropEdit(Keys.Enter);
						AssertEquals("GRM-ONE is returned from 'g' key press as GRM-ONE is found, the unresolvedstring (ONE) is bound", "GRM-ONE", DropEdit.CodeBox.Text);
						AssertEquals("ONE is stored in business object", "ONE", dummyWithList.Z0_FK_Code);
					}
				}
			}
		}

		#region TestCommitBoundValue

		public void TestCommitBoundValue()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				Application.DoEvents();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				DropEdit.BoundValueCommitted += DropEdit_BoundValueCommitted;

				TestForm.DropEdit.Focus();
				AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);
				Assert(!boundValueCommitted);

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropEdit Text", "TWO", DropEdit.CodeBox.Text);

				DropEdit.CommitBoundValue();
				AssertEquals("Bound BizObj Value", "TWO", dummyWithList.Z0_FK_Code);
				AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);
				Assert(boundValueCommitted);
			}
		}

		bool boundValueCommitted;
		void DropEdit_BoundValueCommitted(object sender, EventArgs e)
		{
			boundValueCommitted = true;
		}

		public void TestEnterKeyWhileDroppedCommitsBoundValue()
		{
			TestCommandKeyCommitsBoundValue(Keys.Enter, true);
		}

		public void TestTabKeyCommitsBoundValue()
		{
			TestCommandKeyCommitsBoundValue(Keys.Tab, true);
			TestCommandKeyCommitsBoundValue(Keys.Tab, false);
		}

		void TestCommandKeyCommitsBoundValue(Keys commandKey, bool showDropDown)
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				TestForm.DropEdit.Focus();
				AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);

				if (showDropDown)
				{
					SendKeyToDropEdit(Keys.F4);
					AssertEquals("DropDown Visible", true, DropEdit.IsDroppedDown);
				}

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropEdit Text", "TWO", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "Description 02", DropEdit.DescriptionBox.Text);

				SendKeyToDropEdit(commandKey);

				AssertEquals("DropDown Visible", false, DropEdit.IsDroppedDown);
				AssertEquals("DropEdit Text", "TWO", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "Description 02", DropEdit.DescriptionBox.Text);
				AssertEquals("Bound BizObj Value", "TWO", dummyWithList.Z0_FK_Code);
			}
		}

		public void TestCommitBoundValue_SetToDifferentValue()
		{
			var dummy = Factory.New<DummyWithOverriddenSetter>();

			using (var form = new ZForm())
			using (var dropEdit = new ZDropEdit())
			{
				dropEdit.SetDataBinding(dummy, DummyBizoSchema.Z0_Code.Name);
				dropEdit.BindToList = "DummyList";
				form.Controls.Add(dropEdit);
				form.Show();

				dropEdit.Focus();
				dropEdit.Text = "123";
				dropEdit.CommitBoundValue();
				AssertEquals("MEH", dropEdit.CodeBox.Text);
			}
		}

		class DummyWithOverriddenSetter : DummyWithCodeDescriptionPairList
		{
			public DummyWithOverriddenSetter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Z0_Code
			{
				get { return base.Z0_Code; }
				set { base.Z0_Code = "MEH"; }
			}
		}

		#endregion

		public void TestSelectedIndex()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				AssertEquals("SelectedIndex", -1, DropEdit.SelectedIndex);
				DropEdit.SelectedIndexChanged += new EventHandler(DropEdit_SelectedIndexChanged);

				TestForm.DropEdit.Focus();
				AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropEdit Text", "TWO", DropEdit.CodeBox.Text);
				AssertEquals("SelectedIndex", 1, DropEdit.SelectedIndex);
				AssertEquals("SelectedIndexChanged", true, SelectedIndexChanged);
				SelectedIndexChanged = false;

				SendKeyToDropEdit(Keys.Delete);
				AssertEquals("DropEdit Text", "T", DropEdit.CodeBox.Text);
				AssertEquals("DropEdit Text", "T", DropEdit.CodeBox.Text);
				AssertEquals("SelectedIndex", -1, DropEdit.SelectedIndex);
				AssertEquals("SelectedIndexChanged", true, SelectedIndexChanged);
				SelectedIndexChanged = false;

				SendKeyToDropEdit(Keys.E);
				AssertEquals("DropEdit Text", "TEN", DropEdit.CodeBox.Text);
				AssertEquals("SelectedIndex", 9, DropEdit.SelectedIndex);
				AssertEquals("SelectedIndexChanged", true, SelectedIndexChanged);
			}
		}

		public void TestReadOnly_AfterShow()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				AssertEquals("DropEdit ReadOnly", false, DropEdit.ReadOnly);

				dummyWithList.Z0_FK_Code_ReadOnly = true;
				AssertEquals("DropEdit ReadOnly", true, DropEdit.ReadOnly);
				AssertEquals("DropEdit CodeBox ReadOnly", true, DropEdit.CodeBox.ReadOnly);

				SendKeyToDropEdit(Keys.F4);
				AssertEquals("DropDown Visible", false, DropEdit.IsDroppedDown);

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropDown CodeBox Text", "", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "", DropEdit.DescriptionBox.Text);
				AssertEquals("SelectedIndex", -1, DropEdit.SelectedIndex);

				dummyWithList.Z0_FK_Code = "TEN";
				AssertEquals("DropDown CodeBox Text", "TEN", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "Description 10", DropEdit.DescriptionBox.Text);
				AssertEquals("SelectedIndex", 9, DropEdit.SelectedIndex);
			}
		}

		public void TestReadOnly_BeforeShow()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				dummyWithList.Z0_FK_Code_ReadOnly = true;
				dummyWithList.Z0_FK_Code = "SIX";

				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				AssertEquals("DropEdit ReadOnly", true, DropEdit.ReadOnly);
				AssertEquals("DropEdit CodeBox ReadOnly", true, DropEdit.CodeBox.ReadOnly);
				AssertEquals("DropDown CodeBox Text", "SIX", DropEdit.CodeBox.Text);
				AssertEquals("DropDown Description", "Description 06", DropEdit.DescriptionBox.Text);
				AssertEquals("SelectedIndex", 5, DropEdit.SelectedIndex);
			}
		}

		public void TestTabbing()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				dummyWithList.Z0_FK_Code_ReadOnly = true;
				TestForm.Show();

				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				AssertEquals("DropEdit ReadOnly", true, DropEdit.ReadOnly);

				TestForm.zCalcEdit1.Focus();
				AssertEquals("CalcEdit1 Focused", true, TestForm.zCalcEdit1.Focused);

				KeySender.PostKeyDown(TestForm.zCalcEdit1, Keys.Tab);
				Application.DoEvents();

				AssertEquals("CalcEdit2 Focused", true, TestForm.zCalcEdit2.Focused);

				KeySender.PostKeyDown(TestForm.zCalcEdit2, Keys.Tab);
				Application.DoEvents();

				AssertEquals("CalcEdit1 Focused", true, TestForm.zCalcEdit1.Focused);

				dummyWithList.Z0_FK_Code_ReadOnly = false;

				KeySender.PostKeyDown(TestForm.zCalcEdit1, Keys.Tab);
				Application.DoEvents();

				KeySender.PostKeyDown(TestForm.zCalcEdit2, Keys.Tab);
				Application.DoEvents();

				AssertEquals("CodeBox Focused", true, DropEdit.CodeBox.Focused);

				KeySender.PostKeyDown(DropEdit.CodeBox, Keys.Shift | Keys.Tab);
				Application.DoEvents();

				AssertEquals("CalcEdit2 Focused", true, TestForm.zCalcEdit2.Focused);

				KeySender.PostKeyDown(TestForm.zCalcEdit2, Keys.Shift | Keys.Tab);
				Application.DoEvents();

				AssertEquals("CalcEdit1 Focused", true, TestForm.zCalcEdit1.Focused);

				KeySender.PostKeyDown(TestForm.zCalcEdit1, Keys.Shift | Keys.Tab);
				Application.DoEvents();

				AssertEquals("CodeBox Focused", true, DropEdit.CodeBox.Focused);
			}
		}

		public void TestShowDescriptionBox()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				var dropEditWidth = DropEdit.Width;
				var descriptionBoxWidth = DropEdit.DescriptionBox.Width;

				DropEdit.ShowDescriptionBox = false;
				AssertEquals("DropEdit Width", dropEditWidth - descriptionBoxWidth, DropEdit.Width);

				DropEdit.ShowDescriptionBox = true;
				AssertEquals("DropEdit Width", dropEditWidth, DropEdit.Width);
				AssertEquals("DescriptionBox Width", descriptionBoxWidth, DropEdit.DescriptionBox.Width);
			}
		}

		public void TestMaxLength()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);

				dummyWithList.Z0_FK_Code_MaxLength = 3;
				dummyWithList.Z0_FK_CodeInfo.RefreshBinding();
				AssertEquals("MaxLength", 3, DropEdit.CodeBox.MaxLength);

				dummyWithList.Z0_FK_Code_MaxLength = 0;
				dummyWithList.Z0_FK_CodeInfo.RefreshBinding();
				AssertEquals("MaxLength", 0, DropEdit.CodeBox.MaxLength);
			}
		}

		public void TestReadOnlyWhenNoCurrent()
		{
			var superDummy = Factory.New<SuperDummyWithListChild>();
			using (TestForm = new TestDropEditFormWithList(superDummy))
			{
				DropEdit.BindTo = "DummiesWithList." + DummyBusinessObject.Schema.Z0_FK_Code;
				DropEdit.BindToList = "DummiesWithList.DummyList";

				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				AssertEquals("ReadOnly value when No Current", true, DropEdit.ReadOnly);

				superDummy.DummiesWithList.AddNew();
				AssertEquals("ReadOnly value when Current", false, DropEdit.ReadOnly);

				superDummy.DummiesWithList.RemoveAll();
				AssertEquals("ReadOnly value when No Current", true, DropEdit.ReadOnly);
			}
		}

		public void TestMixedCaps()
		{
			var dummyWithList = Factory.New<DummyWithSettableCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				DropEdit.CharacterCasing = CharacterCasing.Normal;

				var list = new CodeDescriptionPairList();

				list.AddPair("TeStA", "Desc01");
				list.AddPair("tEsTb", "Desc02");
				list.AddPair("TESTC", "Desc03");
				list.AddPair("testd", "Desc04");

				dummyWithList.SetDummyList(new ReadOnlyCodeDescriptionPairList(list));

				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropEdit CodeBox Text", "TeStA", DropEdit.CodeBox.Text);

				DropEdit.CodeBox.Text = "";
				SendKeyToDropEdit(Keys.T | Keys.Shift);
				AssertEquals("DropEdit CodeBox Text", "TeStA", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.E);
				AssertEquals("DropEdit CodeBox Text", "TeStA", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.S | Keys.Shift);
				AssertEquals("DropEdit CodeBox Text", "TeStA", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.T);
				AssertEquals("DropEdit CodeBox Text", "TeStA", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.B | Keys.Shift);
				AssertEquals("DropEdit CodeBox Text", "tEsTb", DropEdit.CodeBox.Text);
			}
		}

		public void TestCodesWithSpaces()
		{
			var dummyWithList = Factory.New<DummyWithSettableCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				dummyWithList.Z0_FK_Code_MaxLength = 5;

				var list = new CodeDescriptionPairList();

				list.AddPair("CO D1", "Desc01");
				list.AddPair("COD 2", "Desc02");
				list.AddPair("C OD3", "Desc03");
				list.AddPair("CODE4", "Desc04");

				dummyWithList.SetDummyList(new ReadOnlyCodeDescriptionPairList(list));

				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);

				SendKeyToDropEdit(Keys.C);
				AssertEquals("DropEdit CodeBox Text", "CO D1", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.O);
				AssertEquals("DropEdit CodeBox Text", "CO D1", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.D);
				AssertEquals("DropEdit CodeBox Text", "COD 2", DropEdit.CodeBox.Text);

				SendKeyToDropEdit(Keys.E);
				AssertEquals("DropEdit CodeBox Text", "CODE4", DropEdit.CodeBox.Text);
			}
		}

		[ExpectNoExceptions()]
		public void TestNavigatingOnEmptyList()
		{
			var dummyWithList = Factory.New<DummyWithSettableCodeDescriptionPairList>();

			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				dummyWithList.SetDummyList(new ReadOnlyCodeDescriptionPairList());

				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);

				SendKeyToDropEdit(Keys.Down);
				SendKeyToDropEdit(Keys.Down);
				Application.DoEvents();
			}
		}

		public void TestIsDroppedDown()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", DropEdit);
				TestForm.DropEdit.Focus();

				AssertEquals("Focused DropEdit - focus on CodeBox", true, DropEdit.CodeBox.Focused);
				SendKeyToDropEdit(Keys.F4);
				AssertEquals("DropDown Visible", true, DropEdit.IsDroppedDown);

				AssertEquals(DropEdit.DropButton.IsDroppedDown, DropEdit.IsDroppedDown);

				SendKeyToDropEdit(Keys.Enter);
				AssertEquals("DropDown not visible", false, DropEdit.IsDroppedDown);
			}
		}

		public void TestForeColor()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.DropEdit.ForeColor = Color.Aquamarine;
				AssertEquals(Color.Aquamarine, TestForm.DropEdit.ForeColor);
				AssertEquals(Color.Aquamarine, TestForm.DropEdit.CodeBox.ForeColor);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateDescriptionOnIdle_WhenBusinessObjectDeleted()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();
				Application.DoEvents();

				dummyWithList.Z0_FK_Code = "New";
				dummyWithList.Delete();
			}
		}

		#region TestSynchroniseControlSizes

		public void TestSynchroniseControlSizesDoesNotChangesWidthIfItIsZero()
		{
			using (var dropEdit = new ZDropEditForTest())
			{
				dropEdit.Width = 10;
				dropEdit.SynchroniseControlSizesExposed();
				Assert("Update width", dropEdit.Width > 10);

				dropEdit.Width = 0;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("Do not change width if it is 0", 0, dropEdit.Width);
			}
		}

		public void TestSynchroniseControlSizesForCodeBox()
		{
			using (var dropEdit = new ZDropEditForTest())
			{
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("CodeBox.Height should be DropButton.Height - (CodeBox.Bounds.Y * 2)", dropEdit.DropButton.Height - (dropEdit.CodeBox.Bounds.Y * 2), dropEdit.CodeBox.Height);

				// Change the DescriptionBox height as all control sizes are dependant on that
				dropEdit.DescriptionBox.Height = 40;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("CodeBox.Height should be DropButton.Height - (CodeBox.Bounds.Y * 2)", dropEdit.DropButton.Height - (dropEdit.CodeBox.Bounds.Y * 2), dropEdit.CodeBox.Height);
			}
		}

		public void TestSynchroniseControlSizesWithUseFullWidthForCodeBox()
		{
			using (var dropEdit = new ZDropEditForTest())
			{
				dropEdit.UseFullWidthForCodeBox = true;
				dropEdit.Width = 50;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("CodeBox.Width should be Width - Button Width", dropEdit.Width - ZDropEditForTest.ButtonWidth, dropEdit.CodeBox.Width);
				AssertEquals("DropButton.Width should be Width", dropEdit.Width, dropEdit.DropButton.Width);
				AssertEquals("DescriptionBox.Width should be 0", 0, dropEdit.DescriptionBox.Width);

				dropEdit.Width = 40;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("CodeBox.Width should be Width - Button Width", dropEdit.Width - ZDropEditForTest.ButtonWidth, dropEdit.CodeBox.Width);
				AssertEquals("DropButton.Width should be Width", dropEdit.Width, dropEdit.DropButton.Width);
				AssertEquals("DescriptionBox.Width should be 0", 0, dropEdit.DescriptionBox.Width);

				var oldWidth = dropEdit.CodeBox.Width;
				dropEdit.UseFullWidthForCodeBox = false;
				dropEdit.Width = 60;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("CodeBox.Width should not change", oldWidth, dropEdit.CodeBox.Width);

				dropEdit.Width = 50;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals("CodeBox.Width should not change", oldWidth, dropEdit.CodeBox.Width);
			}
		}

		class ZDropEditForTest : ZDropEdit
		{
			public void SynchroniseControlSizesExposed()
			{
				SynchroniseControlSizes();
			}
		}

		#endregion

		public void TestCodeBoxAutoSizeIsFalse()
		{
			using (var dropEdit = new ZDropEditForTest())
			{
				Assert("The CodeBox AutoSize property should be set to false", !dropEdit.CodeBox.AutoSize);
			}
		}

		public void TestShowInDropDownList()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				AssertEquals("Default value", ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, DropEdit.ShowInDropDown);
				AssertEquals(true, DropEdit.ShowCodeInDropDown);
				AssertEquals(true, DropEdit.ShowDescriptionInDropDown);

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
				AssertEquals(ZDropEdit.ShowInDropDownList.OnlyShowCode, DropEdit.ShowInDropDown);
				AssertEquals(true, DropEdit.ShowCodeInDropDown);
				AssertEquals(false, DropEdit.ShowDescriptionInDropDown);

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
				AssertEquals(ZDropEdit.ShowInDropDownList.OnlyShowDescription, DropEdit.ShowInDropDown);
				AssertEquals(false, DropEdit.ShowCodeInDropDown);
				AssertEquals(true, DropEdit.ShowDescriptionInDropDown);
			}
		}

		public void TestGetMultilingualValue()
		{
			var dummyWithList = Factory.New<DummyWithCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				var item = new CodeDescriptionPair("Code", "Description");

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				AssertEquals("Code", DropEdit.GetMultilingualValue(item).ToString());

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
				AssertEquals("Code", DropEdit.GetMultilingualValue(item).ToString());

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
				AssertEquals("Description", DropEdit.GetMultilingualValue(item).ToString());
			}
		}

		public void TestSetSelection()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();

				dummyWithList.SetList(true);

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				DropEdit.SetSelection(dummyWithList.DummyList[1], 0);
				AssertEquals("TWO", DropEdit.CodeBox.Text);

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
				DropEdit.SetSelection(dummyWithList.DummyList[2], 0);
				AssertEquals("THREE", DropEdit.CodeBox.Text);

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;
				DropEdit.SetSelection(dummyWithList.DummyList[3], 0);
				AssertEquals("DESCRIPTION 04", DropEdit.CodeBox.Text);
			}
		}

		public void TestDropButtonAvailability()
		{
			using (var dropEdit = new ZDropEdit())
			{
				Assert("DropButton is enabled", dropEdit.DropButton.Enabled);

				dropEdit.SetDropButtonAvailability(false);
				Assert("DropButton is disabled", !dropEdit.DropButton.Enabled);
			}
		}

		public void TestDropDown_ChildControlLoadedWithoutParentForm()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();

			using (var form1 = new Form())
			using (var form2 = new TestDropEditForm(dummyWithList))
			using (var mainTabControl = new ZTabControl())
			using (var mainTabPage1 = new ZTabPage())
			using (var mainTabPage2 = new ZTabPage())
			using (var mainTabPage2Content = new ZUserControl())
			using (var level2TabControl = new ZTabControl())
			using (var level2TabPage = new ZTabPage())
			using (var level2TabPageContent = new TestDropEditUserControl(dummyWithList))
			{
				//create main form with tabs
				form1.Controls.Add(mainTabControl);

				mainTabControl.TabPages.Add(mainTabPage1);
				mainTabControl.TabPages.Add(mainTabPage2);
				mainTabPage1.CheckForChildrenControlsVisibilityChange = false;
				mainTabPage2.CheckForChildrenControlsVisibilityChange = false;
				mainTabPage1.TabVisible = true;
				mainTabPage2.TabVisible = false;
				mainTabControl.SelectedIndex = 0;

				form1.Show();

				//create page 2 content
				level2TabControl.SelectedIndex = 0;
				mainTabPage2Content.Controls.Add(level2TabControl);
				level2TabPage.Dock = DockStyle.Fill;
				level2TabControl.TabPages.Add(level2TabPage);
				level2TabPage.CheckForChildrenControlsVisibilityChange = false;
				level2TabPage.TabVisible = false;

				//adding content to not visible level2 tabPage
				level2TabPage.Controls.Add(level2TabPageContent);
				level2TabPage.TabVisible = true;

				// adding content to page 2
				mainTabPage2.Controls.Add(mainTabPage2Content);
				mainTabPage2.TabVisible = true;

				//select page 2 to make it visible and select drop edit
				mainTabControl.SelectedIndex++;
				level2TabPageContent.DropEdit.Focus();
				KeySender.PostKeyDown(level2TabPageContent.DropEdit.CodeBox, Keys.F4);
				Application.DoEvents();
				AssertEquals("DropDown is visible.", true, level2TabPageContent.DropEdit.IsDroppedDown);

				form2.Show();
				form2.DropEdit.Focus();
				Application.DoEvents();
				AssertEquals("DropDown is not visible.", false, level2TabPageContent.DropEdit.IsDroppedDown);
			}
		}

		public void TestShowEditForm()
		{
			using (var filterModule = new DummyDependentWithCodeModule())
			{
				filterModule.SetAllowEdit(true);
				filterModule.SetAllowView(true);

				var dummyWithLookups = Factory.New<DummyWithLookups>();
				var allDependents = new DummyDependentWithCodeBusinessObjectCollection(dummyWithLookups, Factory);
				var dependent1 = allDependents.AddNew();
				var dependent2 = allDependents.AddNew();
				var dependent3 = allDependents.AddNew();
				dependent1.ZD1_Code = "ONE";
				dependent2.ZD1_Code = "TWO";
				dependent3.ZD1_Code = "THREE";
				allDependents.Factory.Save();

				using (TestForm = new TestDropEditForm(dummyWithLookups))
				{
					TestForm.Show();

					DropEdit.BindTo = DummyWithLookups.Schema.Z0_Guid;
					DropEdit.BindToList = "Lookups.Dependents";
					DropEdit.FilterModuleForTesting = filterModule;
					DropEdit.ShowDescriptionBox = false;

					Application.DoEvents();

					DropEdit.CodeBox.Text = "TWO";
					SendKeyToDropEdit(Keys.F3);

					using (var popup = filterModule?.LastController?.LastFormCreated)
					{
						AssertNull("Form should not be created", popup);
					}

					DropEdit.EnableShowEditOrViewForm = true;
					SendKeyToDropEdit(Keys.F3);

					using (var popup = filterModule?.LastController?.LastFormCreated)
					{
						AssertNotNull("'Form for Edit' Created", popup);
						AssertEquals("Editing Dummy Code", "TWO", ((DummyDependentWithCodeBusinessObject)popup.BusinessEntity).ZD1_Code);
						AssertEquals("DisplayMode", ODisplayMode.Browse, popup.DisplayMode);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					DropEdit.CodeBox.Text = "FOUR";
					SendKeyToDropEdit(Keys.F3);

					using (var popup = filterModule?.LastController?.LastFormCreated)
					{
						AssertNotNull("'Form for New' Created - No matching object", popup);
						AssertEquals("DisplayMode - No matching object", ODisplayMode.New, popup.DisplayMode);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var dependent4 = allDependents.AddNew();
					dependent4.ZD1_Code = "FOUR";
					SendKeyToDropEdit(Keys.F3);

					using (var popup = filterModule?.LastController?.LastFormCreated)
					{
						AssertNotNull("'Form for New' Created - Matching object not saved", popup);
						AssertEquals("DisplayMode - Matching object not saved", ODisplayMode.New, popup.DisplayMode);
					}
				}
			}
		}

		public void TestShowViewForm()
		{
			using (var filterModule = new DummyDependentWithCodeModule())
			{
				filterModule.SetAllowEdit(false);
				filterModule.SetAllowView(true);

				var dummyWithLookups = Factory.New<DummyWithLookups>();
				var allDependents = new DummyDependentWithCodeBusinessObjectCollection(dummyWithLookups, Factory);
				var dependent1 = allDependents.AddNew();
				var dependent2 = allDependents.AddNew();
				var dependent3 = allDependents.AddNew();
				dependent1.ZD1_Code = "ONE";
				dependent2.ZD1_Code = "TWO";
				dependent3.ZD1_Code = "THREE";
				allDependents.Factory.Save();

				using (TestForm = new TestDropEditForm(dummyWithLookups))
				{
					TestForm.Show();

					DropEdit.BindTo = DummyWithLookups.Schema.Z0_Guid;
					DropEdit.BindToList = "Lookups.Dependents";
					DropEdit.FilterModuleForTesting = filterModule;
					DropEdit.ShowDescriptionBox = false;

					Application.DoEvents();

					DropEdit.CodeBox.Text = "TWO";
					SendKeyToDropEdit(Keys.F3);

					using (var popup = filterModule?.LastController?.LastFormCreated)
					{
						AssertNull("Form should not be created", popup);
					}

					DropEdit.EnableShowEditOrViewForm = true;
					SendKeyToDropEdit(Keys.F3);

					using (var popup = filterModule?.LastController?.LastFormCreated)
					{
						AssertNotNull("'Form for View' Created", popup);
						AssertEquals("View form", "View ZDummyDependentWithCodeForm", popup.Text);
						AssertEquals("Editing Dummy Code", "TWO", ((DummyDependentWithCodeBusinessObject)popup.BusinessEntity).ZD1_Code);
						AssertEquals("DisplayMode", ODisplayMode.ReadOnly, popup.DisplayMode);
					}
				}
			}
		}

		public void TestPasswordChar()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();
			using (TestForm = new TestDropEditForm(dummyWithList))
			{
				TestForm.Show();

				dummyWithList.SetList(true);

				DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				DropEdit.SetSelection(dummyWithList.DummyList[1], 0);
				AssertEquals('\0', DropEdit.CodeBox.PasswordChar);

				DropEdit.PasswordChar = '*';
				AssertEquals('*', DropEdit.CodeBox.PasswordChar);
			}
		}

		public void TestGetBizObjsToEditOrView_NoErrorForActiveCollection()
		{
			var dummyWithLookups = Factory.New<DummyWithLookups>();
			var allDependents = new DummyDependentWithCodeBusinessObjectCollection(dummyWithLookups, Factory);
			var dependent1 = allDependents.AddNew();
			var dependent2 = allDependents.AddNew();
			var dependent3 = allDependents.AddNew();
			dependent1.ZD1_Code = "ONE";
			dependent2.ZD1_Code = "TWO";
			dependent3.ZD1_Code = "THREE";
			allDependents.Factory.Save();

			using (TestForm = new TestDropEditForm(dummyWithLookups))
			{
				TestForm.Show();

				DropEdit.BindTo = DummyWithLookups.Schema.Z0_Guid;
				DropEdit.BindToList = "Lookups.ActiveDependents";
				DropEdit.ShowDescriptionBox = false;

				AssertEquals("Precondition: No errors", 0, ExceptionReporterTestListener.Instance.Count);
				DropEdit.CodeBox.Text = "ONE";
				var collection = DropEdit.GetBizObjsToEditOrView();
				AssertEquals("No developer errors should be reported", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Collection contains value", 1, collection.Count());
				var item = (DummyDependentWithCodeBusinessObject)collection.FirstOrDefault();
				AssertEquals("The ZD1_Code of item should be ONE", "ONE", item.ZD1_Code);

				DropEdit.CodeBox.Text = "TWO";
				collection = DropEdit.GetBizObjsToEditOrView();
				AssertEquals("No developer errors should be reported", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Collection contains value", 1, collection.Count());
				item = (DummyDependentWithCodeBusinessObject)collection.FirstOrDefault();
				AssertEquals("The ZD1_Code of item should be TWO", "TWO", item.ZD1_Code);
			}
		}

		#region Implementation

		void SendKeyToDropEdit(Keys key)
		{
			KeySender.PostKeyDown(TestForm.DropEdit.CodeBox, key);
			Application.DoEvents();
		}

		TestDropEditForm TestForm;
		TestDropEdit DropEdit
		{
			get { return TestForm.DropEdit; }
		}

		bool SelectedIndexChanged;
		void DropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			SelectedIndexChanged = true;
		}

		#endregion
	}
}
