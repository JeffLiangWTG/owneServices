using System;

namespace Enterprise.BufferManagement.Business
{
	public class ModuleGridSectionConfigurationValidation : BMBoardSectionConfigurationWithOverridableSectionNameValidation
	{
		public ModuleGridSectionConfigurationValidation(ModuleGridSectionConfiguration sectionConfiguration)
			: base(sectionConfiguration)
		{
			this.sectionConfiguration = sectionConfiguration;
		}

		readonly ModuleGridSectionConfiguration sectionConfiguration;

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(ModuleGridSectionConfigurationValidation); }
		}

		protected override IBoardSectionNameOverridable SectionConfiguration => sectionConfiguration;

		#endregion
	}
}
