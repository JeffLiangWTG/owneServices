using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class TagLinkValidation : AutoTagLinkValidation
	{
		public TagLinkValidation(AutoTagLink parent)
			: base(parent)
		{
		}

		protected new TagLink Parent
		{
			get { return (TagLink)base.Parent; }
		}

		protected override void CheckTGL_TGM_Magnitude()
		{
			base.CheckTGL_TGM_Magnitude();
			MandatoryValidation.CheckEntered(Parent.TGL_TGM_MagnitudeInfo);

			var definition = Parent.Definition;
			var tagable = Parent.Parent;
			var magnitude = Parent.Magnitude;

			if (definition != null)
			{
				if (tagable != null && definition.TGD_IsExclusive &&
				tagable.TagLinks.Cast<TagLink>().Any(link => link.TagDefinitionPk == definition.PK && link.PK != Parent.PK))
				{
					Parent.TGL_TGM_MagnitudeInfo.AddError(Res.GetString("35ba75aa-566d-4e30-922a-138acca021f1", "Cannot apply multiple tags from an exclusive Tag Group [{0}].", definition.DisplayText));
				}

				if ((Parent.HasChanges || definition.CanUserUseTags) && !Parent.InOperationalScope && !Parent.IsTemplateTag)
				{
					var usageScopeDescription = definition.Lookups.UsageScopeList.GetDescriptionFromCode(definition.TGD_UsageScope);
					Parent.TGL_TGM_MagnitudeInfo.AddError(Res.GetString("c1753c47-a103-4026-9909-2960b19100fd", "Cannot modify [{0}] from here. Tag Group has scope of [{1}].", magnitude != null ? magnitude.DisplayText : definition.DisplayText, usageScopeDescription));
				}

				if (!Parent.InParentScope)
				{
					Parent.TGL_TGM_MagnitudeInfo.AddError(Res.GetString("35ecfb04-e71e-45d0-937c-89f90cd22d6f", "Cannot apply tags from group [{0}] here. {1}", definition.DisplayText, definition.Lookups.ScopeList.GetDescriptionFromCode(definition.TGD_Scope)));
				}

				if (magnitude != null)
				{
					var queue = magnitude as WorkQueue;

					if (queue != null)
					{
						if ((!Parent.IsInDatabase || Parent.TGL_TGM_MagnitudeInfo.HasChanges) && !WorkQueueSecurity.CheckAddToQueueSecurity(magnitude, showSecurityDialog: false))
						{
							Parent.TGL_TGM_MagnitudeInfo.AddError(Res.GetString("BC1FFE41-E6CA-4DE7-A2BF-368A2FB3DBF4", "You do not have permission to add items to the queue [{0}].", magnitude.DisplayText));
						}
						else
						{
							var processHeader = tagable as ProcessHeader;

							if (processHeader != null)
							{
								string message;

								if (!WorkQueueMembershipValidator.CanAdd(queue, processHeader, out message, ignoreAlreadyInQueueRule: true))
								{
									Parent.TGL_TGM_MagnitudeInfo.AddError(message);
								}
							}
						}
					}
					else if ((!Parent.IsInDatabase || Parent.TGL_TGM_MagnitudeInfo.HasChanges)
						&& (!Parent.IsAddTagValidationSuspended)
						&& !TagSecurity.CheckTagAddSecurity(magnitude, showSecurityDialog: false))
					{
						Parent.TGL_TGM_MagnitudeInfo.AddError(Res.GetString("78f0fe2e-b58e-4f08-b777-658242a96a41", "You do not have permission to add the tag [{0}].", magnitude.DisplayText));
					}
				}
			}
		}

		public void ValidateTagDefinitionPk()
		{
			ValidateCalculatedProperty(Parent.TagDefinitionPkInfo);
		}

		protected void CheckTagDefinitionPk()
		{
			MandatoryValidation.CheckEntered(Parent.TagDefinitionPkInfo);
			TypeValidation.CheckValidGuid(Parent.TagDefinitionPkInfo);
		}

		protected override void CheckTGL_Sequence()
		{
			base.CheckTGL_Sequence();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.TGL_SequenceInfo, 0m);
		}
	}
}
