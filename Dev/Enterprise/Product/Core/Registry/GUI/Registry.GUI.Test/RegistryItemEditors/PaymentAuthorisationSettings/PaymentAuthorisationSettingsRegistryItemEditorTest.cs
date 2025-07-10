using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PaymentAuthorisationSettingsRegistryItemEditor))]
	sealed class PaymentAuthorisationSettingsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new PaymentAuthorisationSettingsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PaymentAuthorisationSettingsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PaymentAuthorisationSettingsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PaymentAuthorisationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			PaymentAuthorisationSettingsCollection collection = new PaymentAuthorisationSettingsCollection();

			PaymentAuthorisationSettings paymentAuthorisationSettings = collection.AddNew();
			paymentAuthorisationSettings.Range = paymentAuthorisationSettings.RangeList[0].Code;
			paymentAuthorisationSettings.AuthorisationRequirement = paymentAuthorisationSettings.AuthorisationRequirementList[0].Code;
			paymentAuthorisationSettings.Amount = 200;
			paymentAuthorisationSettings = collection.AddNew();
			paymentAuthorisationSettings.Range = paymentAuthorisationSettings.RangeList[1].Code;
			paymentAuthorisationSettings.AuthorisationRequirement = paymentAuthorisationSettings.AuthorisationRequirementList[1].Code;
			paymentAuthorisationSettings.Amount = 200;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
