using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(InvoiceTotalRoundingRegistryItemEditor))]
	public class InvoiceTotalRoundingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor() => new InvoiceTotalRoundingRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((InvoiceTotalRoundingUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(InvoiceTotalRoundingUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new InvoiceTotalRoundingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var rule = collection.AddNew();
			rule.Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			rule.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundDown;
			rule.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.OneMajorUnit;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
