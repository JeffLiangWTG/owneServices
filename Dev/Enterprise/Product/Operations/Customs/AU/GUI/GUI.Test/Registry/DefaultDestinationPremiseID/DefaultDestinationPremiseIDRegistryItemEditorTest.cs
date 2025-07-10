using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(DefaultDestinationPremiseIDRegistryItemEditor))]
	sealed class DefaultDestinationPremiseIDRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DefaultDestinationPremiseIDRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultDestinationPremiseIDRegistryItemEditor((DefaultDestinationPremiseIDRegistryDataType)RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultDestinationPremiseIDControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new DefaultDestinationPremiseIDCollection();
			var item = collection.AddNew();
			item.AirlineCode = "QF";
			item.PortOfDischarge = "AUSYD";
			item.PremiseID = "9914N";
			item.UseDischargePort = true;
			return new[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DefaultDestinationPremiseIDControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
