using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CreditTemporaryIncreaseAuthorisationSettingsRegistryItemEditor))]
	sealed class CreditTemporaryIncreaseAuthorisationSettingsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CreditTemporaryIncreaseAuthorisationSettingsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CreditTemporaryIncreaseAuthorisationSettingsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CreditTemporaryIncreaseAuthorisationSettingsCollection();

			CreditTemporaryIncreaseAuthorisationSettings settings = collection.AddNew();
			settings.Range = settings.RangeList[0].Code;
			settings.AuthorisationRequirement = settings.AuthorisationRequirementList[0].Code;
			settings.Amount = 200;
			settings.DaysToExpiry = 10;
			settings = collection.AddNew();
			settings.Range = settings.RangeList[1].Code;
			settings.AuthorisationRequirement = settings.AuthorisationRequirementList[1].Code;
			settings.Amount = 200;
			settings.DaysToExpiry = 20;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
