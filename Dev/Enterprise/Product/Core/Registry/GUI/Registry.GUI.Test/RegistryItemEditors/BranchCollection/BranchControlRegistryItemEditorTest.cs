using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BranchControlRegistryItemEditor))]
	sealed class BranchControlRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BranchCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BranchControlRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BranchCollectionControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var master = new BranchProxyMaster();
			var branchProxy = new BranchProxy { ProxyPK = GlbBranch.CurrentBranch.PK };
			master.Items.Add(branchProxy);

			return new object[] { master };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BranchCollectionControl)editorPane).ProxyCollectionGrid.ButtonsReadOnlyExposedForTesting;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
