using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditor))]
	sealed class AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AmountOrPercentageAuthorisationSettingsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AmountOrPercentageAuthorisationSettingsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();

			AmountOrPercentageBasedThreeLevelAuthorisationRequirement settings = collection.AddNew();
			settings.Range = settings.RangeList[0].Code;
			settings.AuthorisationRequirement = settings.AuthorisationRequirementList[0].Code;
			settings.Amount = 200;
			settings = collection.AddNew();
			settings.Range = settings.RangeList[1].Code;
			settings.AuthorisationRequirement = settings.AuthorisationRequirementList[1].Code;
			settings.Amount = 200;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
