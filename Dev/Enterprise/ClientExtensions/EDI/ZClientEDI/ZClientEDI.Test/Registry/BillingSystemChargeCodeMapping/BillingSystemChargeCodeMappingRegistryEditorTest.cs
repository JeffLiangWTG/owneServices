using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(BillingSystemChargeCodeMappingRegistryEditor))]
	public class BillingSystemChargeCodeMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillingSystemChargeCodeMappingRegistryItem("BillingSystemChargeCodeMappings", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BillingSystemChargeCodeMappingRegistryEditor(new BillingSystemChargeCodeMappingDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BillingSystemChargeCodeMappingControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var items = new BillingSystemChargeCodeMappingCollection[1];
			items[0] = new BillingSystemChargeCodeMappingCollection();
			var item = items[0].AddNew();
			item.ProductCode = "ENT";
			item.SystemCode = "AAA";
			item.SubModule = "XXX";
			item.Description = "Chargeable";
			item.ChargeCode = "CHARGECODE";
			return items;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BillingSystemChargeCodeMappingControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
