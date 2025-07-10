using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DepartmentMappingRegistryItemEditor))]
	sealed class DepartmentMappingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestGetCustomValidation()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				Editor.SetValueFromEditorPane(editorPane, "|!@#");
				AssertEquals("Editor.GetCustomValidation()", "Please enter a value.", Editor.GetCustomValidation(editorPane));

				Editor.SetValueFromEditorPane(editorPane, "FEA|!@#");
				AssertEquals("Editor.GetCustomValidation()", "Enter a valid selection.", Editor.GetCustomValidation(editorPane));

				Editor.SetValueFromEditorPane(editorPane, "FEA|CEA");
				AssertEquals("Editor.GetCustomValidation()", "", Editor.GetCustomValidation(editorPane));
			}
		}

		public void TestMyDepartmentMappingControl_NullData()
		{
			using (MyDepartmentMappingControlForTest editorPane = new MyDepartmentMappingControlForTest())
			{
				AssertEquals("Precondition: Data should be null", null, editorPane.Data);
				AssertEquals("Precondition: FieldValue should be empty", "", editorPane.FieldValue);

				editorPane.FieldValue = "FEA|CEA;FEL|CEL;FER|CER;FES|CES;FIA|CIA;FIL|CIL;FIR|CIR;FIS|CIS";
				AssertEquals("FieldValue", "FEA|CEA;FEL|CEL;FER|CER;FES|CES;FIA|CIA;FIL|CIL;FIR|CIR;FIS|CIS", editorPane.FieldValue);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DepartmentMappingRegistryItemEditor(new StringRegistryDataType());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DepartmentMappingRegistryItemEditor.MyDepartmentMappingControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new DepartmentMappingEditorInfo();
			return result;
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DepartmentMappingContainer)editorPane).ReadOnly;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "FEA|CEA;FEL|CEL;FER|CER;FES|CES;FIA|CIA;FIL|CIL;FIR|CIR;FIS|CIS" };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
