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
	[TestedType(typeof(DiscountSuspensionPolicyRegistryEditor))]
	public class DiscountSuspensionPolicyRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DiscountSuspensionPolicyRegistryItem("DiscountSuspensionPolicies", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null, RegistryStorageFlags.System, new DiscountSuspensionPolicyCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DiscountSuspensionPolicyRegistryEditor(new DiscountSuspensionPolicyDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DiscountSuspensionPolicyControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new DiscountSuspensionPolicyCollection();
			collection.AddNew("CW1", "ALW");
			return new object[] { collection };
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
			return !((DiscountSuspensionPolicyControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
