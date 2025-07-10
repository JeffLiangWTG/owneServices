using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.TGE.GUI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Business.Testing
{
	[TestedType(typeof(TGEEventsRegistryItemEditor))]
	public class TGEEventsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override object[] GetValidRegistryValues()
		{
			TGEEventRegistryBusinessObjectCollection collection = new TGEEventRegistryBusinessObjectCollection();
			TGEEventRegistryBusinessObject element = collection.AddNew();
			element.Code = Events.CustomsCleared.Code;
			return new object[] { collection };
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TGEEventsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TGEEventsRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TGEEventsRegistryItemControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TGEEventsRegistryItem("", "", "", "", RegistryStorageFlags.System);
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
