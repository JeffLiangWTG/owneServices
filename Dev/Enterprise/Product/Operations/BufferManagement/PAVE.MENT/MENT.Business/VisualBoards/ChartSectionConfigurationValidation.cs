using System;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public class ChartSectionConfigurationValidation : ZValidation
	{
		public ChartSectionConfigurationValidation(ChartSectionConfiguration sectionConfiguration)
			: base(sectionConfiguration)
		{
			Parent = sectionConfiguration;
		}

		readonly ChartSectionConfiguration Parent;

		public void ValidateExtractionPK()
		{
			ValidateCalculatedProperty(Parent.ExtractionPKInfo);
		}

		protected void CheckExtractionPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.ExtractionPKInfo);
			MandatoryValidation.CheckEntered(Parent.ExtractionPKInfo);
		}

		public override Type AutoValidationType
		{
			get { return typeof(ChartSectionConfigurationValidation); }
		}

		public override void ValidateAll()
		{
			ValidateExtractionPK();
		}
	}
}
