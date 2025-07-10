using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TagMagnitudeValidation : AutoTagMagnitudeValidation
	{
		public TagMagnitudeValidation(AutoTagMagnitude parent)
			: base(parent)
		{
		}

		new TagMagnitude Parent
		{
			get { return (TagMagnitude)base.Parent; }
		}

		#region Validate

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateApplyColorToBackground();
			ValidateApplyColorToBorder();
			ValidateBorderStyle();
			ValidateColor();
			ValidateVisualStylePriority();
		}

		public void ValidateApplyColorToBackground()
		{
			ValidateCalculatedProperty(Parent.ApplyColorToBackgroundInfo);
		}

		public void ValidateApplyColorToBorder()
		{
			ValidateCalculatedProperty(Parent.ApplyColorToBorderInfo);
		}

		public void ValidateBorderStyle()
		{
			ValidateCalculatedProperty(Parent.BorderStyleInfo);
		}

		public void ValidateColor()
		{
			ValidateCalculatedProperty(Parent.ColorInfo);
		}

		public void ValidateVisualStylePriority()
		{
			ValidateCalculatedProperty(Parent.VisualStylePriorityInfo);
		}

		#endregion

		#region Check

		protected override void CheckTGM_Code()
		{
			base.CheckTGM_Code();

			MandatoryValidation.CheckEntered(Parent.TGM_CodeInfo);

			var tagGroup = Parent.Definition;

			if (tagGroup != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.TGM_CodeInfo, tagGroup.Magnitudes);

				if (!tagGroup.TGD_IsSystem)
				{
					var otherTagQuery = new ZQuery(TagMagnitudeSchema.TGM_Code, Parent.TGM_Code);
					otherTagQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, SQLComparisonOperator.NotEqual, Parent.TGM_TGD_Tag);

					var otherTag = Parent.Factory.LoadTop1<TagMagnitude>(otherTagQuery);
					var otherTagGroup = otherTag != null ? otherTag.Definition : null;

					if (otherTagGroup != null)
					{
						Parent.TGM_CodeInfo.AddWarning(Res.GetString("53c49381-bde0-4086-8d55-6565d918cc43", "The code {0} is already in use in Tag Group [{1}]. Consider changing the code to avoid the wrong tag being applied mistakenly.", Parent.TGM_Code, otherTagGroup.TGD_DescriptionMultilingual));
					}
				}
			}
		}

		protected void CheckBorderStyle()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BorderStyleInfo, Parent.BorderStyles);
		}

		protected void CheckColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ColorInfo);

			if (Parent.Color.IsEmpty)
			{
				if (Parent.ApplyColorToBorder)
				{
					Parent.ColorInfo.AddError(MandatoryColorMessage(Parent.ApplyColorToBorderInfo));
				}

				if (Parent.ApplyColorToBackground)
				{
					Parent.ColorInfo.AddError(MandatoryColorMessage(Parent.ApplyColorToBackgroundInfo));
				}
			}
		}

		static ZString MandatoryColorMessage(ZPropertyInfo info)
		{
			return Res.GetString("e26b9c98-f275-4254-8548-6eed0b2e877d", "A Color is required when {0} is selected.", info.Description);
		}

		protected override void CheckTGM_RuleRunSequence()
		{
			base.CheckTGM_RuleRunSequence();

			var definition = Parent.Definition;
			if (definition != null && !definition.TGD_IsExclusive && Parent.TGM_RuleRunSequence != 0)
			{
				Parent.TGM_RuleRunSequenceInfo.AddError(Res.GetString("280bcf07-1c95-4bb1-931d-48479d109c76", "Non-exclusive tags should have a run sequence of zero."));
			}
		}

		protected override void CheckTGM_GG_OwnerGroup()
		{
			base.CheckTGM_GG_OwnerGroup();

			ListValidation.ErrorIfInvalidPK(Parent.TGM_GG_OwnerGroupInfo);
		}

		#endregion
	}
}
