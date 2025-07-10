using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeCurrencyRegistryItemEditor))]
	public class ElectronicProcessingChargeCurrencyRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ElectronicProcessingChargeCurrencyRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new ElectronicProcessingChargeCurrencyCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ElectronicProcessingChargeCurrencyRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ElectronicProcessingChargeCurrencyControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ElectronicProcessingChargeCurrencyCollection();
			collection.Add(new ElectronicProcessingChargeCurrency { CurrencyPK = new TestObjectCreator(Factory).AUD.PK, ValidFromDate = ZDateTime.Now });

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ElectronicProcessingChargeCurrencyControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
