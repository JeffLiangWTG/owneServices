using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionAdditionalComponentValidation : AutoBMBoardSectionAdditionalComponentValidation
	{
		public BMBoardSectionAdditionalComponentValidation(AutoBMBoardSectionAdditionalComponent parent)
			: base(parent)
		{
			this.parent = (BMBoardSectionAdditionalComponent)parent;
		}

		readonly BMBoardSectionAdditionalComponent parent;

		protected override void CheckBSA_FC_Component()
		{
			base.CheckBSA_FC_Component();

			var section = parent.Section;
			if (section != null)
			{
				if (section.SectionConfiguration.IsReleaseScheduler)
				{
					parent.BSA_FC_ComponentInfo.AddError(Res.GetString("ae4415a8-aee5-488d-9c2f-1f65c764ad67", "Release Scheduler sections may not have additional components specified."));
				}
				else if (section.MS_FC_Component.IsEmpty)
				{
					parent.BSA_FC_ComponentInfo.AddError(Res.GetString("8c02615f-1de3-43ca-be7f-74a42bb88e6f", "A primary board section component must be selected before specifying an additional component."));
				}
				else if (section.MS_FC_Component == parent.BSA_FC_Component)
				{
					parent.BSA_FC_ComponentInfo.AddError(Res.GetString("b9d13406-ae11-454e-af20-58b2481373fc", "This component has already been chosen as the primary board section component."));
				}
				else
				{
					var primaryComponent = section.Component;
					var additionalComponent = parent.Component;

					if (primaryComponent != null)
					{
						if (additionalComponent is ComponentRelationship additionalRelationship)
						{
							if (!additionalRelationship.RelatedComponentLinks.Any())
							{
								parent.BSA_FC_ComponentInfo.AddError(Res.GetString("228d786a-fa17-4225-adc2-566fc9b555b4", "This relationship is empty and cannot be added as an additional component."));
							}
							else if (primaryComponent.FC_Type != additionalRelationship.RelatedComponentLinks.First().ComponentTo.FC_Type)
							{
								parent.BSA_FC_ComponentInfo.AddError(Res.GetString("8a8aff5e-3071-46af-b22b-cf11a9b253ae", "Components in this relationship must be of the same type as the primary board section component."));
							}
						}
						else if (additionalComponent != null && primaryComponent.FC_Type != additionalComponent.FC_Type)
						{
							parent.BSA_FC_ComponentInfo.AddError(Res.GetString("48fe7cbe-ba57-4cdd-b860-a158c2edcd27", "Additional components must be the same type of component as the primary board section component."));
						}
					}

					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.BSA_FC_ComponentInfo, section.SectionConfiguration.AdditionalComponents);
				}
			}
		}
	}
}
