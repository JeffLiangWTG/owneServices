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
	[TestedType(typeof(BillingDbUsageCodesRegistryEditor))]
	public class BillingDbUsageCodesRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillingDbUsageCodesRegistryItem("BillingDbUsageCodess", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BillingDbUsageCodesRegistryEditor(new BillingDbUsageCodesDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BillingDbUsageCodesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var items = new BillingDbUsageCodesCollection[1];
			items[0] = new BillingDbUsageCodesCollection();
			var item = items[0].AddNew();
			item.Category = "ACC";
			item.PriceItemCode = "IT1";
			item.PriceHeaderCode = "STL";
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
			return !((BillingDbUsageCodesControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
