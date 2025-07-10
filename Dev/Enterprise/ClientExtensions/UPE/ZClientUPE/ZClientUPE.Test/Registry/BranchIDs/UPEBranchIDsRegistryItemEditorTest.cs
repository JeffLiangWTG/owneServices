using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(UPEBranchIDsRegistryItemEditor))]
	public class UPEBranchIDsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new UPEBranchIDsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((UPEBranchIDsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(UPEBranchIDsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new UPEBranchIDsRegistryItem("", null, null, RegistryStorageFlags.System, new UPEBranchIDsRegistryObjectCollection(), null);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override object[] GetValidRegistryValues()
		{
			UPEBranchIDsRegistryObjectCollection collection = new UPEBranchIDsRegistryObjectCollection();
			UPEBranchIDsRegistryObject deposit = collection.AddNew();
			deposit.FirstArrivalPort = GlbBranch.CurrentBranch.HomePort.PK;
			return new object[] { collection };
		}
		#endregion
	}
}
