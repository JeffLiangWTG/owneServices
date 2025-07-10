using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class SectionRepositoryValidation : ZValidation
	{
		public SectionRepositoryValidation(SectionRepository templateSectionSuperset)
			: base(templateSectionSuperset)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(SectionRepository); }
		}

		public override void ValidateAll()
		{
		}
	}
}
