using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentLinkValidation : AutoBMComponentLinkValidation
	{
		public BMComponentLinkValidation(AutoBMComponentLink parent)
			: base(parent)
		{
		}

		new BMComponentLink Parent
		{
			get { return (BMComponentLink)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateFilterStrips();
			ValidateComponentToSystemPK();
		}

		protected override void CheckFL_FC_ComponentFrom()
		{
			base.CheckFL_FC_ComponentFrom();
			CompareValidation.CheckNotEqual(Parent.FL_FC_ComponentFromInfo, Parent.FL_FC_ComponentToInfo);
		}

		protected override void CheckFL_FC_ComponentTo()
		{
			base.CheckFL_FC_ComponentTo();
			CompareValidation.CheckNotEqual(Parent.FL_FC_ComponentToInfo, Parent.FL_FC_ComponentFromInfo);
		}

		protected override void CheckFL_IsReleaseGateRuleApplied()
		{
			base.CheckFL_IsReleaseGateRuleApplied();

			if (Parent.FL_IsReleaseGateRuleApplied)
			{
				if (BMSRegistry.Instance.DisableCapacityCalculations.Value)
				{
					var path = BMSRegistry.Instance.DisableCapacityCalculations.GetLocation();
					Parent.FL_IsReleaseGateRuleAppliedInfo.AddError(Res.GetString("27bc1035-2ac1-42e0-bd89-b29b22c5c62b", "Release Gate links are not available when the [{0}] registry item is enabled.", path));
				}

				if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.EnhancedWorkflow)
				{
					var path = BMSRegistry.Instance.WorkflowManagementMode.GetLocation();
					Parent.FL_IsReleaseGateRuleAppliedInfo.AddError(Res.GetString("7a146cee-4a62-4057-8e52-f1c74ffbbb78", "Release Gate links are not available when the registry item [{0}] is set to EWF - Enhanced Workflow Management.", path));
				}

				var destinationComponent = Parent.ComponentTo;
				if (destinationComponent != null && !destinationComponent.IsBuffer)
				{
					Parent.FL_IsReleaseGateRuleAppliedInfo.AddError(Res.GetString("1f439911-f0d4-4260-ad55-28431865d65c", "Only links to a Buffer component can be marked as part of a Release Gate."));
				}
			}
		}

		void ValidateFilterStrips()
		{
			RelatedModuleFiltersHelper.ValidateFilterStrips(Parent.FilterRule);

			Parent.ToggleRowErrorSafe(Parent.HasNoFilterWhenRequired, FilterRequiredError);
		}

		public static string FilterRequiredError
		{
			get { return Res.GetString("A5F042D6-12DA-479E-B6B6-D144B2123D6E", "Filters are required for all active links, except a link between the entry component and another component, and links to buffer components marked as a 'release gate' link."); }
		}

		public void ValidateComponentToSystemPK()
		{
			ValidateCalculatedProperty(Parent.ComponentToSystemPKInfo);
		}

		protected void CheckComponentToSystemPK()
		{
			MandatoryValidation.CheckEntered(Parent.ComponentToSystemPKInfo);
			TypeValidation.CheckValidGuid(Parent.ComponentToSystemPKInfo);
		}
	}
}
