using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DataTransferRegistryItemEditor))]
	public class DataTransferRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DataTransferRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DataTransferRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DataTransferRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DataTransferRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		[TestDate(2006, 10, 31, 10, 45, 50)]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			base.TestRegistryItemAcceptsEditorValue();
		}

		protected override object[] GetValidRegistryValues()
		{
			DataTransferRegistryBusinessObject dataTransferRegistry = new DataTransferRegistryBusinessObject();
			dataTransferRegistry.Directory = Env.TempPath;
			dataTransferRegistry.Interval = 2;
			dataTransferRegistry.UpdateRuns(new ZDateTime(2006, 10, 31, 11, 45, 50));
			dataTransferRegistry.GroupPK = Core.Constants.Groups.PostMastersGroupPK;

			return new object[] { dataTransferRegistry };
		}

		#endregion
	}
}
