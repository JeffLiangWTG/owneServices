using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(EventsRegistryItemEditor))]
	public class EventsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override object[] GetValidRegistryValues()
		{
			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject element = collection.AddNew();
			element.Code = Events.CustomsCleared.Code;
			element.Reference = "abc";
			return new object[] { collection };
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EventsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EventsRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EventsRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EventsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
