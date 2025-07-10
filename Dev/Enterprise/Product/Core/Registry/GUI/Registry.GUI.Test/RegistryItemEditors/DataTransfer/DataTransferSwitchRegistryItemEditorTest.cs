using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DataTransferSwitchRegistryItemEditor))]
	public class DataTransferSwitchRegistryItemEditorTest : DataTransferRegistryItemEditorTest
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DataTransferSwitchRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DataTransferSwitchRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DataTransferSwitchRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DataTransferSwitchRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		[TestDate(2006, 10, 31, 10, 45, 50)]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			base.TestRegistryItemAcceptsEditorValue();
		}

		protected override object[] GetValidRegistryValues()
		{
			DataTransferSwitchRegistryBusinessObject dataTransferRegistry = new DataTransferSwitchRegistryBusinessObject();
			dataTransferRegistry.Directory = Env.TempPath;
			dataTransferRegistry.Interval = 2;
			dataTransferRegistry.UpdateRuns(new ZDateTime(2006, 10, 31, 11, 45, 50));
			dataTransferRegistry.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			dataTransferRegistry.EnableInterface = true;

			return new object[] { dataTransferRegistry };
		}

		#endregion
	}
}
