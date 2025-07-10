using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public abstract class BMBoardSectionConfigurationWithOverridableSectionNameValidation : ZValidation
	{
		protected BMBoardSectionConfigurationWithOverridableSectionNameValidation(BusinessObject sectionConfiguration)
			: base(sectionConfiguration)
		{
		}

		protected abstract IBoardSectionNameOverridable SectionConfiguration { get; }

		public void ValidateSectionNameOverride()
		{
			ValidateCalculatedProperty(SectionConfiguration.SectionNameOverrideInfo);
		}

		protected void CheckSectionNameOverride()
		{
			if (SectionConfiguration.SectionNameIsOverridden && string.IsNullOrWhiteSpace(SectionConfiguration.SectionNameOverride))
			{
				SectionConfiguration.SectionNameOverrideInfo.AddError(Res.GetString("E2D0A25C-26A3-4CC6-9C17-94BE1F1CE567", "Custom section name cannot be empty when overriding section name."));
			}
		}

		public override void ValidateAll()
		{
			ValidateSectionNameOverride();
		}
	}
}
