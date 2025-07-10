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
	[TestedType(typeof(CostVarianceApprovalRegistryItemEditor))]
	public class CostVarianceApprovalRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CostVarianceApprovalRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CostVarianceApprovalRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CostVarianceApprovalRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CostVarianceApprovalRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			CostVarianceApproval item = new CostVarianceApproval();
			CostVarianceApprovalAuthorisationRequirement requirement = item.AuthorisationRequirements.AddNew();
			requirement.Amount = 100;
			requirement.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			requirement.AuthorisationRequirement = requirement.AuthorisationRequirementList[0].Code;

			requirement = item.AuthorisationRequirements.AddNew();
			requirement.Amount = 100;
			requirement.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			requirement.AuthorisationRequirement = requirement.AuthorisationRequirementList[1].Code;

			return new object[] { item };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
