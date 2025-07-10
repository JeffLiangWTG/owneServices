using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(UPEGlbGroupsRegistryItemEditor))]
	public class UPEGlbGroupsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new UPEGlbGroupsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((UPEGlbGroupsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(UPEGlbGroupsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new UPEGlbGroupsRegistryItem(ZString.Empty, null, null, RegistryStorageFlags.System, new UPEGlbGroupsRegistryObjectCollection());
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
			UPEGlbGroupsRegistryObjectCollection collection = new UPEGlbGroupsRegistryObjectCollection();
			collection.AddNew().Group = Core.Constants.Groups.AllPK;
			return new object[] { collection };
		}
		#endregion
	}
}
