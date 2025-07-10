using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ARAPDefaultTaxRecognitionRuleRegistryItemEditor))]
	public class ARAPDefaultTaxRecognitionRuleRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ARAPDefaultTaxRecognitionRuleRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ARAPDefaultTaxRecognitionRuleControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ARAPDefaultTaxRecognitionRuleControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ARDefaultTaxRecognitionRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			ARAPDefaultTaxRecognitionRuleCollection collection = new ARAPDefaultTaxRecognitionRuleCollection();

			ARAPDefaultTaxRecognitionRule taxRecognition = collection.AddNew();
			taxRecognition.LoginCompanyCountryRuleCode = "IEU";
			taxRecognition.OrganizationCountryRuleCode = "OEU";
			taxRecognition.TaxRecognitionCode = "DEF";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
