using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DpsWebServiceItemRegistryItemEditor))]
	sealed class DpsWebServiceItemRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DpsWebServiceItemRegistryItemEditor(new DpsWebServiceItemRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DpsWebServiceItemControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DpsWebServiceItemControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DpsWebServiceItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new DpsWebServiceItemCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new DpsWebServiceItemCollection()
			{
				new DpsWebServiceItem()
				{
					Code = "STG1",
					WebServiceUrl = "http://dpswebservice.testurl.net",
					Role = RoleHelper.Code.Staging
				},
				new DpsWebServiceItem()
				{
					Code = "DPS1",
					WebServiceUrl = "http://dpswebservice.testurl.net1",
					Role = RoleHelper.Code.Production
				},
				new DpsWebServiceItem()
				{
					Code = "DPS2",
					WebServiceUrl = "http://dpswebservice.testurl.net2",
					Role = RoleHelper.Code.ProductionFailover
				}
			};

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
