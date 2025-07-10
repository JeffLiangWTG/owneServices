using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ComboBoxFilterLayoutRegistryItemEditor))]
	sealed class ComboBoxFilterLayoutRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestCharacterCasing()
		{
			using (ZDropEdit editorPane = (ZDropEdit)Editor.NewWinFormsEditorPane())
			{
				AssertEquals("EditorPane.CharacterCasing", CharacterCasing.Normal, editorPane.CharacterCasing);
			}
		}

		public void TestGetListOfModuleFilterLayouts()
		{
			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_Code = "1";
			glbCompany1.GC_Name = "1";

			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "2";
			glbCompany2.GC_Name = "2";

			var glbCompany3 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany3.GC_Code = "3";
			glbCompany3.GC_Name = "3";

			var glbCompany4 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany4.GC_Code = "4";
			glbCompany4.GC_Name = "4";

			Factory.Save();

			var stmModuleFilter1 = Factory.NewWithValidTestData<StmModuleFilter>();
			stmModuleFilter1.S9_GC = glbCompany1.PK;
			stmModuleFilter1.S9_ModuleID = "TrackingDeclarations";
			stmModuleFilter1.S9_FilterName = "Filter1";
			stmModuleFilter1.S9_IsPublished = true;

			var stmModuleFilter2 = Factory.NewWithValidTestData<StmModuleFilter>();
			stmModuleFilter2.S9_GC = glbCompany2.PK;
			stmModuleFilter2.S9_ModuleID = "TrackingDeclarations";
			stmModuleFilter2.S9_FilterName = "Filter2";
			stmModuleFilter2.S9_IsPublished = true;

			var stmModuleFilter3 = Factory.NewWithValidTestData<StmModuleFilter>();
			stmModuleFilter3.S9_GC = glbCompany3.PK;
			stmModuleFilter3.S9_ModuleID = "Trackingshipments";
			stmModuleFilter3.S9_FilterName = "Filter3";
			stmModuleFilter3.S9_IsPublished = true;

			var stmModuleFilter4 = Factory.NewWithValidTestData<StmModuleFilter>();
			stmModuleFilter4.S9_GC = glbCompany4.PK;
			stmModuleFilter4.S9_ModuleID = "Trackingshipments";
			stmModuleFilter4.S9_FilterName = "Filter4";
			stmModuleFilter4.S9_IsPublished = true;
			stmModuleFilter4.S9_RelatedEntityID = ZGuid.NewZGuid(); //fake orgID to make it Org-only

			Factory.Save();

			var editorInfo = new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments");
			var node = new FallbackTreeNode("Test", glbCompany3.PK, new ZGuid(), new ZGuid(), true);
			FallbackLevel fallback = node.GetFallbackLevel();

			var editor = new ComboBoxFilterLayoutRegistryItemEditor(editorInfo, fallback, Factory);
			var listOfModuleFilterLayouts = editor.GetListOfModuleFilterLayouts();

			AssertEquals(2, listOfModuleFilterLayouts.Count);
			AssertEquals("Filter3", listOfModuleFilterLayouts[0].ToString());
			AssertEquals("System Default Layout", listOfModuleFilterLayouts[1].ToString());

			//Filter4 wont be showing as it is Org-only filter
			node = new FallbackTreeNode("Test", glbCompany4.PK, new ZGuid(), new ZGuid(), true);
			fallback = node.GetFallbackLevel();

			editor = new ComboBoxFilterLayoutRegistryItemEditor(editorInfo, fallback, Factory);
			listOfModuleFilterLayouts = editor.GetListOfModuleFilterLayouts();
			AssertEquals(1, listOfModuleFilterLayouts.Count);
			AssertEquals("System Default Layout", listOfModuleFilterLayouts[0].ToString());
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			var editorInfo = new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments");
			var node = new FallbackTreeNode("Test");
			FallbackLevel fallback = node.GetFallbackLevel();
			return new ComboBoxFilterLayoutRegistryItemEditor(editorInfo, fallback, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return new ComboBoxFilterLayoutRegistryItemEditorForTest(null, null, null).GetEditorPaneType();
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ZDropEdit)editorPane).ReadOnly;
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FilterLayoutCodePairRegistryItem("", null, null, null, false, false, new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments"), RegistryStorageFlags.System, RegistryOptions.Default, "", false);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[] { "AUT" };
		}

		#region ComboBoxRegistryItemEditor

		class ComboBoxFilterLayoutRegistryItemEditorForTest : ComboBoxFilterLayoutRegistryItemEditor
		{
			public ComboBoxFilterLayoutRegistryItemEditorForTest(IRegistryEditorInfo editorInfo, FallbackLevel fallback, BusinessObjectFactory factory)
				: base(editorInfo, fallback, factory)
			{
			}

			public Type GetEditorPaneType()
			{
				return typeof(MyDropEdit);
			}
		}

		#endregion

		#endregion
	}
}
