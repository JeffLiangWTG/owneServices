using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ExchangeRateToleranceRegistryItemEditor))]
	public class ExchangeRateToleranceRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ExchangeRateToleranceRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ExchangeRateToleranceControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExchangeRateToleranceControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var defaultConfiguration = new ExchangeRateToleranceConfiguration();
			defaultConfiguration.ExchangeRateToleranceCollection.Add(Business.ExchangeRateTolerance.GetDefaultExchangeRateTolerance());
			return new ExchangeRateToleranceRegistryItem("", null, null, null, RegistryStorageFlags.System, defaultConfiguration);
		}

		protected override object[] GetValidRegistryValues()
		{
			ExchangeRateToleranceConfiguration copy = new ExchangeRateToleranceConfiguration();

			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
