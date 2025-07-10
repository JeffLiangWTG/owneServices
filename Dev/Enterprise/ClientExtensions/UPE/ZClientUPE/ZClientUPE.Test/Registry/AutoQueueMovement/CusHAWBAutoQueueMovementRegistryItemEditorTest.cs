using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Client.UPE.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Registry.Testing
{
	[TestedType(typeof(CusHAWBAutoQueueMovementRegistryItemEditor))]
	internal class CusHAWBAutoQueueMovementRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CusHAWBAutoQueueMovementRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CusHAWBAutoQueueMovementControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CusHAWBAutoQueueMovementControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CusHAWBAutoQueueMovementRegistryItem("", "", "", "");
		}

		protected override object[] GetValidRegistryValues()
		{
			CusHAWBAutoQueueMovementCollection collection = new CusHAWBAutoQueueMovementCollection();
			CusHAWBAutoQueueMovement movement1 = collection.AddNew();
			movement1.FreeTextSegmentName = "ABCD";
			movement1.FreeTextSegmentValue = "EFGHIJK";
			movement1.Queue.QueueName = CargoReportQueueCodeDescriptionPairList.Codes.Intervention;
			movement1.Queue.Status = ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease;
			movement1.Queue.SubStatus = "";
			movement1.Priority = 1;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
