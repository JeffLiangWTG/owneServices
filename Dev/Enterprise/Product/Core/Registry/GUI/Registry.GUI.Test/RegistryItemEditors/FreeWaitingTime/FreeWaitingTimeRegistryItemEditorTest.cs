using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FreeWaitingTimeRegistryItemEditor))]
	sealed class FreeWaitingTimeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new FreeWaitingTimeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((FreeWaitingTimeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FreeWaitingTimeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FreeWaitingTimeRegistryItem("", null, null, null, RegistryStorageFlags.System, new FreeWaitingTimeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = freeWaitingTimeCollection.AddNew();

			freeWaitingTime.DropMode = Enterprise.Core.Constants.EquipmentNeeded.Any;
			freeWaitingTime.CFS = ZDateTime.Now;
			freeWaitingTime.CTO = ZDateTime.Now.AddDays(1);
			freeWaitingTime.CYD = ZDateTime.Now.AddDays(2);
			freeWaitingTime.CNR = ZDateTime.Now.AddDays(3);
			freeWaitingTime.Other = ZDateTime.Now.AddDays(3);

			return new object[] { freeWaitingTimeCollection };
		}

		#endregion
	}
}
