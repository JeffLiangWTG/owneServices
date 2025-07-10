using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutomaticProcessRegistryItemEditor))]
	sealed class AutomaticProcessRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AutomaticProcessRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AutomaticProcessRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutomaticProcessRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutomaticProcessRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		[TestDate(2006, 10, 31, 10, 45, 50)]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			base.TestRegistryItemAcceptsEditorValue();
		}

		protected override object[] GetValidRegistryValues()
		{
			AutomaticProcessRegistryBusinessObject bizO = new AutomaticProcessRegistryBusinessObject();
			bizO.Interval = 2;
			bizO.UpdateRuns(new ZDateTime(2006, 10, 31, 11, 45, 50));

			return new object[] { bizO };
		}

		#endregion
	}
}
