using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Grid.Internal
{
	public class ZMultiCombinationControlTest : TestCaseWithFactory
	{
		public void TestShowingTextEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Text);
				Assert("EditControl", multiControl.CurrentEditor is ZTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, ((ZTextBox)multiControl.CurrentEditor).CharacterCasing);
				multiControl.CharacterCasing = CharacterCasing.Upper;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, ((ZTextBox)multiControl.CurrentEditor).CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestShowingDateEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Date);
				Assert("EditControl", multiControl.CurrentEditor is ZDateEdit);
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, ((ZDateEdit)multiControl.CurrentEditor).DateTimeFormat);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		public void TestShowingDropEdit()
		{
			RunBindToListTest(FieldType.TextDropEdit, typeof(ZGridDropEdit));
		}

		[RequiresSTA]
		public void TestPasswordChar()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Text);
				Assert("EditControl", multiControl.CurrentEditor is ZTextBox);
				AssertEquals("PasswordChar Empty", '\0', ((ZTextBox)multiControl.CurrentEditor).PasswordChar);
				multiControl.PasswordChar = '*';
				AssertEquals("PasswordChar", '*', ((ZTextBox)multiControl.CurrentEditor).PasswordChar);
			}
		}

		[RequiresSTA]
		public void TestShowingDateTimeEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.DateTime);
				Assert("EditControl", multiControl.CurrentEditor is ZDateEdit);
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, ((ZDateEdit)multiControl.CurrentEditor).DateTimeFormat);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestGetDateTimeEditValue()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.DateTime);
				multiControl.CurrentEditor.Text = "130524";
				AssertEquals("GetValue from DateTime", "13-MAY-24 00:00", multiControl.GetValue(Factory.New<DummyBusinessObject>(), "Z0_AnotherDate"));
			}
		}

		public void TestGetDateEditValue()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Date);
				multiControl.CurrentEditor.Text = "130524";
				AssertEquals("GetValue from Date", "13-MAY-24", multiControl.GetValue(Factory.New<DummyBusinessObject>(), "Z0_AnotherDate"));
			}
		}

		public void TestSwitchingFromTextToDateEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Text);
				Assert("EditControl", multiControl.CurrentEditor is ZTextBox);
				multiControl.CharacterCasing = CharacterCasing.Upper;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, ((ZTextBox)multiControl.CurrentEditor).CharacterCasing);

				multiControl.ControlType = FieldType.Date;
				Assert("EditControl", multiControl.CurrentEditor is ZDateEdit);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, multiControl.CharacterCasing);
			}
		}

		public void TestShowingDateTimeOffsetEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.DateTimeOffset);
				Assert("EditControl", multiControl.CurrentEditor is ZDateTimeOffsetEdit);
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, ((ZDateTimeOffsetEdit)multiControl.CurrentEditor).DateTimeFormat);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		public void TestShowingTimeEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Time);
				Assert("EditControl", multiControl.CurrentEditor is ZTimeEdit);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		public void TestShowingGeographyEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Geography);
				Assert("EditControl", multiControl.CurrentEditor is ZGeographyEdit);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestSwitchingFromTextToDateTimeOffsetEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Text);
				Assert("EditControl", multiControl.CurrentEditor is ZTextBox);
				multiControl.CharacterCasing = CharacterCasing.Upper;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, ((ZTextBox)multiControl.CurrentEditor).CharacterCasing);

				multiControl.ControlType = FieldType.DateTimeOffset;
				Assert("EditControl", multiControl.CurrentEditor is ZDateTimeOffsetEdit);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, multiControl.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestShowingByteEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Byte);
				Assert("EditControl", multiControl.CurrentEditor is ZCalcEdit);
				AssertEquals("Decimals", 0, ((ZCalcEdit)multiControl.CurrentEditor).Decimals);
				AssertEquals("CoreZ.BindToType", typeof(ZByte), GetBindToType(((ZCalcEdit)multiControl.CurrentEditor).Core));
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestShowingIntegerEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Integer);
				Assert("EditControl", multiControl.CurrentEditor is ZCalcEdit);
				AssertEquals("Decimals", 0, ((ZCalcEdit)multiControl.CurrentEditor).Decimals);
				AssertEquals("CoreZ.BindToType", typeof(ZInt), GetBindToType(((ZCalcEdit)multiControl.CurrentEditor).Core));
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		public void TestBindToDecimalPlaces()
		{
			using (var testForm = new ZForm())
			{
				using (var multiControl = GetNewMultiCombinationControl())
				{
					multiControl.BindToDecimalPlaces = "DecimalPlaces";
					multiControl.ControlType = FieldType.Decimal;
					multiControl.Dock = DockStyle.Fill;
					testForm.Controls.Add(multiControl);
					testForm.Show();
					AssertEquals("EditControl", typeof(ZCalcEdit.Bare), multiControl.CurrentEditor.GetType());
					AssertEquals("BindToDecimalPlaces", "DecimalPlaces",
						((ZCalcEdit.Bare)multiControl.CurrentEditor).BindToDecimalPlaces);
				}
			}
		}

		[RequiresSTA]
		public void TestShowingDecimalEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Decimal);
				Assert("EditControl", multiControl.CurrentEditor is ZCalcEdit);
				AssertEquals("Decimals", 6, ((ZCalcEdit)multiControl.CurrentEditor).Decimals);
				AssertEquals("CoreZ.BindToType", typeof(ZDecimal), GetBindToType(((ZCalcEdit)multiControl.CurrentEditor).Core));
				AssertEquals("CharacterCasing", CharacterCasing.Normal, multiControl.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestShowingCheckBox()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.Boolean);
				Assert("EditControl", multiControl.CurrentEditor is XPCheckBox);
			}
		}

		public void TestShowingGuidDropEdit()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.GuidDropEdit);
				var dropEdit = multiControl.CurrentEditor as ZGridGuidDropEdit;
				AssertNotNull("EditControl", dropEdit);
			}
		}

		public void TestShowingLinkLabel()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.LinkLabel);
				var linkLabel = multiControl.CurrentEditor as ZLinkLabel;
				AssertNotNull("EditControl", linkLabel);
			}
		}

		public void TestShowingGuidFindBox()
		{
			RunBindToListTest(FieldType.Guid, typeof(ZGridGuidFindBox));
		}

		public void TestShowingCodeFindBox()
		{
			RunBindToListTest(FieldType.TextCodeFindBox, typeof(ZGridFindBox));
		}

		public void TestShowingMultilineTextBox()
		{
			using (var testForm = new ZForm())
			{
				var multiControl = ShowWithControlType(testForm, FieldType.TextMultiLine);
				Assert("EditControl", multiControl.CurrentEditor is ZTextBox);

				var textBox = (ZTextBox)multiControl.CurrentEditor;
				Assert("Should be multiline", textBox.Multiline);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, textBox.CharacterCasing);
				AssertEquals("BorderStyle", BorderStyle.None, textBox.BorderStyle);
				AssertEquals("ScrollBars", ScrollBars.Both, textBox.ScrollBars);
				Assert("AcceptsReturn", textBox.AcceptsReturn);
				AssertEquals("FamilyName", "Tahoma", textBox.Font.FontFamily.Name);
				AssertEquals("Font size", 8f, textBox.Font.Size);
			}
		}

		Type GetBindToType(ZCalcEditCore calcEditCore)
		{
			return (Type)typeof(ZCalcEditCore).GetProperty("BindToType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(calcEditCore, null);
		}

		public void RunBindToListTest(FieldType fieldType, Type expectedEditorType)
		{
			RunBindToListTest(fieldType, expectedEditorType, true);
		}

		public void RunBindToListTest(FieldType fieldType, Type expectedEditorType, bool needToCheckBindToList)
		{
			// test setting ControlType first
			using (var testForm = new ZForm())
			{
				var multiControl = GetNewMultiCombinationControl();
				multiControl.ControlType = fieldType;
				multiControl.BindToList = "TestList";
				multiControl.ModuleID = DummyModuleIDs.Dummy;
				multiControl.Dock = DockStyle.Fill;
				testForm.Controls.Add(multiControl);

				testForm.Show();
				AssertEquals("EditControl", expectedEditorType, multiControl.CurrentEditor.GetType());
				if (needToCheckBindToList)
				{
					AssertEquals("BindToList", "TestList", ((IBindToList)multiControl.CurrentEditor).BindToList);
				}
				if (expectedEditorType == typeof(ZGridFindBox)) // bit ugly
				{
					AssertEquals("ModuleID", DummyModuleIDs.Dummy, ((ZGridFindBox)multiControl.CurrentEditor).ModuleID);
				}
			}

			// test setting BindToList first
			using (var testForm = new ZForm())
			{
				var multiControl = GetNewMultiCombinationControl();
				multiControl.BindToList = "TestList";
				multiControl.ModuleID = DummyModuleIDs.Dummy;
				multiControl.ControlType = fieldType;
				multiControl.Dock = DockStyle.Fill;
				testForm.Controls.Add(multiControl);

				testForm.Show();
				AssertEquals("EditControl", expectedEditorType, multiControl.CurrentEditor.GetType());
				if (needToCheckBindToList)
				{
					AssertEquals("BindToList", "TestList", ((IBindToList)multiControl.CurrentEditor).BindToList);
				}
				if (expectedEditorType == typeof(ZGridFindBox))
				{
					AssertEquals("ModuleID", DummyModuleIDs.Dummy, ((ZGridFindBox)multiControl.CurrentEditor).ModuleID);
				}
			}
		}

		public virtual ZMultiCombinationControl GetNewMultiCombinationControl()
		{
			return new ZMultiCombinationControl();
		}

		ZMultiCombinationControl ShowWithControlType(ZForm testForm, FieldType controlType)
		{
			var multiControl = GetNewMultiCombinationControl();
			multiControl.ControlType = controlType;
			multiControl.Dock = DockStyle.Fill;
			testForm.Controls.Add(multiControl);

			testForm.Show();
			return multiControl;
		}
	}
}
