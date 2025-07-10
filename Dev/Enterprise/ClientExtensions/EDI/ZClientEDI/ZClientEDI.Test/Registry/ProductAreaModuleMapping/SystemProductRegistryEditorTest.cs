using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(SystemProductRegistryEditor))]
	public class SystemProductRegistryEditorTest : RegistryItemEditorTestCase
	{
		public void TestEditorPane()
		{
			var systemProducts = new SystemProductCollection();
			var editorInfo = new SystemProductRegistryEditorInfo(ModuleListType.MenuSection, true);
			var editor = new SystemProductRegistryEditor(null, editorInfo);
			using (var form = new ZForm())
			using (var mappingsControl = (SystemProductRegistryControl)editor.NewWinFormsEditorPane())
			{
				mappingsControl.SetDataBinding(systemProducts, "");
				form.Controls.Add(mappingsControl);
				form.Show();
				mappingsControl.ModuleCaption = "Menu Section";
				AssertEquals("Lookups.MenuSectionSourceModuleList", ((ZDropEditColumnStyleInfo)mappingsControl.SourceModuleGrid.GetColumnStyle("Code")).BindToList);
			}

			editorInfo = new SystemProductRegistryEditorInfo(ModuleListType.Cr8, true);
			editor = new SystemProductRegistryEditor(null, editorInfo);
			using (var form = new ZForm())
			using (var mappingsControl = (SystemProductRegistryControl)editor.NewWinFormsEditorPane())
			{
				mappingsControl.SetDataBinding(systemProducts, "");
				form.Controls.Add(mappingsControl);
				form.Show();
				mappingsControl.ModuleCaption = "Requirement";
				AssertEquals("Lookups.Cr8SourceModuleList", ((ZDropEditColumnStyleInfo)mappingsControl.SourceModuleGrid.GetColumnStyle("Code")).BindToList);
			}

			editorInfo = new SystemProductRegistryEditorInfo(ModuleListType.Cr9, true);
			editor = new SystemProductRegistryEditor(null, editorInfo);
			using (var form = new ZForm())
			using (var mappingsControl = (SystemProductRegistryControl)editor.NewWinFormsEditorPane())
			{
				mappingsControl.SetDataBinding(systemProducts, "");
				form.Controls.Add(mappingsControl);
				form.Show();
				mappingsControl.ModuleCaption = "Service";
				AssertEquals("Lookups.Cr9SourceModuleList", ((ZDropEditColumnStyleInfo)mappingsControl.SourceModuleGrid.GetColumnStyle("Code")).BindToList);
			}
		}

		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SystemProductRegistryItem("", null, null, null, new SystemProductRegistryEditorInfo(ModuleListType.MenuSection, false), RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			var defaultCollection = new SystemProductCollection();
			foreach (CodeDescriptionPair pair in new IncidentApprovalLookups(null).MenuSectionList)
			{
				defaultCollection.AddNew(pair.Code, pair.Description, true);
			}

			var editorInfo = new SystemProductRegistryEditorInfo(ModuleListType.MenuSection, true);
			return new SystemProductRegistryEditor(new SystemProductRegistryDataType(defaultCollection), editorInfo);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SystemProductRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new SystemProductCollection();
			collection.AddNew("AAA", "Module AAA", false);
			collection.AddNew("BBB", "Module BBB", false);
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SystemProductRegistryControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
