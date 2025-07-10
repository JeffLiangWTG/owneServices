using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(AdditionalSettingsRegistryItemEditor))]
	public class AdditionalSettingsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new AdditionalSettingsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AdditionalSettingsRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AdditionalSettingsRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AdditionalSettingsRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			AdditionalSettingsRegistryBusinessObject bizObject = new AdditionalSettingsRegistryBusinessObject(Factory);
			bizObject.Directory = Env.TempPath;
			bizObject.Interval = 2;
			bizObject.NextRunDateTime = new ZDateTime(2006, 10, 31, 11, 45, 50);
			bizObject.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			bizObject.ExportFileName = "ExportFile";
			return new object[] { bizObject };
		}
		#endregion
	}
}
