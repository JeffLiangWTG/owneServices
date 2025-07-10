using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(AviationSecurityTrainingRestrictionRegistryItemEditor))]
	sealed class AviationSecurityTrainingRestrictionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectNoExceptions]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction()))
			{
				RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction() { Enabled = true, ApplyCertificationRestriction = false });
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AviationSecurityTrainingRestrictionRegistryItemEditor((AviationSecurityTrainingRestrictionDataType)RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AviationSecurityTrainingRestrictionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AviationSecurityTrainingRestrictionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AviationSecurityTrainingRestrictionRegistryItem("", null, null, null, RegistryStorageFlags.System, new AviationSecurityTrainingRestriction());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new AviationSecurityTrainingRestriction() };
		}
	}
}
