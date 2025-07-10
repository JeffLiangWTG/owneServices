using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GLJournalApprovalThresholdRegistryItemEditor))]
	public class GLJournalApprovalThresholdRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new GLJournalApprovalThresholdRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GLJournalApprovalThresholdControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GLJournalApprovalThresholdControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GLJournalApprovalThresholdRegistryItem("", null, null, null, RegistryStorageFlags.System, new GLJournalApprovalThresholdCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new GLJournalApprovalThresholdCollection();
			var copy = collection.AddNew();
			copy.Type = "RSN";
			copy.ReportSection = "AS";
			var setting = copy.AuthorisationSettings.AddNew();
			setting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting.Amount = 100;
			var setting2 = copy.AuthorisationSettings.AddNew();
			setting2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			setting2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting2.Amount = 100;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
