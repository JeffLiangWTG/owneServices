using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeFindBoxRegistryItemEditor))]
	sealed class CodeFindBoxRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestModuleID()
		{
			using (ZCodeFindBox editorPane = (ZCodeFindBox)Editor.NewWinFormsEditorPane())
			{
				AssertEquals("ModuleID", ModuleIDs.RefUNLOCO, editorPane.ModuleID);
			}
		}

		public void TestSelectedCode()
		{
			CodeFindBoxRegistryEditorInfo editorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, GetFindBoxCollection);
			CodeFindBoxRegistryItemEditorForTest newEditor = new CodeFindBoxRegistryItemEditorForTest(null, editorInfo);

			using (ZCodeFindBox editorPane = (ZCodeFindBox)newEditor.NewWinFormsEditorPane())
			{
				newEditor.SetValueZString(editorPane, "AUSYD");
				AssertEquals("GetValueFromEditorPane()", "AUSYD", Editor.GetValueFromEditorPane(editorPane));

				newEditor.SetValueZString(editorPane, "");
				AssertEquals("GetValueFromEditorPane()", "", Editor.GetValueFromEditorPane(editorPane));

				newEditor.SetValueZString(editorPane, "ZZZZZ");
				AssertEquals("GetValueFromEditorPane()", "ZZZZZ", Editor.GetValueFromEditorPane(editorPane));
			}
		}

		public void TestGetCustomValidation()
		{
			using (ZCodeFindBox editorPane = (ZCodeFindBox)Editor.NewWinFormsEditorPane())
			{
				Editor.SetValueFromEditorPane(editorPane, ZString.Empty);
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				Editor.SetValueFromEditorPane(editorPane, "AUSYD");
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
				Editor.SetValueFromEditorPane(editorPane, "ZZZZ");
				AssertEquals("ErrorMessage", "Please enter a valid code.", Editor.GetCustomValidation(editorPane));
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CodeFindBoxRegistryItemEditor(null, new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, GetFindBoxCollection));
		}

		IBusinessObjectCollection GetFindBoxCollection(BusinessObjectFactory factory)
		{
			return new RefUNLOCOCollection(factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new CodeFindBoxRegistryItemEditorForTest(null, new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, GetFindBoxCollection)).GetEditorPaneType();
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefUNLOCO, GetFindBoxCollection);
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { "AUSYD", "" };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#region CodeFindBoxRegistryItemEditorForTest

		class CodeFindBoxRegistryItemEditorForTest : CodeFindBoxRegistryItemEditor
		{
			public CodeFindBoxRegistryItemEditorForTest(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
				: base(dataType, editorInfo)
			{
			}

			public Type GetEditorPaneType()
			{
				return typeof(ZCodeFindBox);
			}

			public void SetValueZString(Control editorPane, ZString value)
			{
				((CodeFindBoxBusinessObject)((IDataBoundControl)editorPane).DataSource).SelectedCode = value;
			}
		}

		#endregion

		#endregion
	}
}
