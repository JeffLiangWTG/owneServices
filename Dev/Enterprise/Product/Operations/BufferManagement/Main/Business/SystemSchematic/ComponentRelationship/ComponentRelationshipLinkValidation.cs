using System.Linq;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentRelationshipLinkValidation : BMComponentLinkValidation
	{
		public ComponentRelationshipLinkValidation(AutoBMComponentLink parent)
			: base(parent)
		{
		}

		new ComponentRelationshipLink Parent
		{
			get { return (ComponentRelationshipLink)base.Parent; }
		}

		protected override void CheckFL_FC_ComponentTo()
		{
			var destinationComponent = Parent.ComponentTo;

			if (destinationComponent != null)
			{
				if (!destinationComponent.IsBuffer && !destinationComponent.IsBucket)
				{
					Parent.FL_FC_ComponentToInfo.AddError(Res.GetString("42b2bcba-e7d8-4ae5-9b63-fedde847bf5a", "Please select only buckets or buffers. Other component types may not be included in component relationships."));
				}

				if (destinationComponent.IsChildComponent)
				{
					Parent.FL_FC_ComponentToInfo.AddError(Res.GetString("786321b8-231d-4d6b-9526-db790f081d5a", "Please do not select subcomponents as they may not be included in component relationships."));
				}

				var componentFrom = Parent.ComponentFrom as ComponentRelationship;

				if (componentFrom != null && componentFrom.RelatedComponentLinks.Any(link => link.ComponentTo != null && link.ComponentTo.FC_Type != destinationComponent.FC_Type))
				{
					Parent.FL_FC_ComponentToInfo.AddError(Res.GetString("3beef2b6-f0e4-459e-8402-27a4ce072654", "All components in a relationship must have the same type. Please remove unwanted components."));
				}
			}

			base.CheckFL_FC_ComponentTo();
		}

		protected override void CheckFL_IsReleaseGateRuleApplied()
		{
			if (Parent.FL_IsReleaseGateRuleApplied)
			{
				Parent.FL_IsReleaseGateRuleAppliedInfo.AddError(Res.GetString("874db67f-2abd-4d8c-9cfa-738ea34417a6", "Please disable the release gate rule as it does not apply to component relationships."));
			}
		}

		protected override void CheckFL_TransferRulesEnabled()
		{
			if (Parent.FL_TransferRulesEnabled)
			{
				Parent.FL_TransferRulesEnabledInfo.AddError(Res.GetString("447d0334-d626-42dc-8c3e-a4231d52c904", "Please disable transfer rules as they do not apply to component relationships."));
			}
		}
	}
}
