using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DateEditRegistryItemEditor))]
	sealed class DateEditRegistryItemEditorTest : Testing.RegistryItemEditorTestCase
	{
		public void TestConstructor()
		{
			var editorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
			var editor = new DateEditRegistryItemEditor(null, editorInfo);
			AssertEquals("editor.EditorInfo is correct in value", editorInfo, editor.EditorInfo);
			using (var editorPane = editor.NewWinFormsEditorPane())
			{
				var control = (ZDateEdit)editorPane.Controls[0];
				AssertEquals("The DateTime format of the control is 'Long'", ZDateTimePickerFormat.Long, control.DateTimeFormat);
			}
		}

		public void TestDateEditBoundToWrapper()
		{
			using (var control = Editor.NewWinFormsEditorPane())
			{
				AssertNotNull("DateEdit has bound BusinessObject", ((ZUserControl)control).CurrentDataItem);
				AssertType<DateEditWrapper>("Bound BusinessObject is of type DateEditWrapper", ((ZUserControl)control).CurrentDataItem);
			}
		}

		public void TestValue()
		{
			var editor = GetEditor();
			using (var editorPane = editor.NewWinFormsEditorPane())
			{
				var dateTime = ZDateTime.Now;
				editor.SetValueFromEditorPane(editorPane, dateTime);
				AssertEquals("GetValueFromEditorPane", dateTime.ToDateTime(), editor.GetValueFromEditorPane(editorPane));

				editor.SetValueFromEditorPane(editorPane, ZDateTime.Invalid);
				AssertEquals("GetValueFromEditorPane", DateTime.MinValue, editor.GetValueFromEditorPane(editorPane));

				editor.SetValueFromEditorPane(editorPane, ZDateTime.Empty);
				AssertEquals("GetValueFromEditorPane", DateTime.MinValue, editor.GetValueFromEditorPane(editorPane));
			}
		}

		public void TestDateTimeHandlesInvalidValue()
		{
			var editor = GetEditor();
			using (var editorPane = editor.NewWinFormsEditorPane())
			{
				var control = (ZUserControl)editorPane;
				((DateEditWrapper)control.CurrentDataItem).Value = ZDateTime.Invalid;

				AssertEquals("Please select a valid date.", editor.GetCustomValidation(editorPane));

				((DateEditWrapper)control.CurrentDataItem).Value = new DateTime(1999, 04, 14);

				AssertEquals("", editor.GetCustomValidation(editorPane));
			}
		}

		public void TestHasChangesIsSetWhenBizoIsChanged()
		{
			using (var editorPane = Editor.NewWinFormsEditorPane())
			{
				var control = (ZUserControl)editorPane;
				var bizo = (DateEditWrapper)control.CurrentDataItem;
				Assert("When we have done nothing then 'HasChanges' should be false", !bizo.HasChanges);

				bizo.Value = Convert.ToDateTime("12/12/12");
				Assert("When we change the property 'Value' then 'HasChanges' should be true", bizo.HasChanges);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DateEditRegistryItemEditor(null, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short));
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ZUserControl);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DateTimeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { DateTime.Now };
		}

		#endregion
	}
}
