using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class SectionPreviewManagerValidation : ZValidation
	{
		public SectionPreviewManagerValidation(SectionPreviewManager manager)
			: base(manager)
		{
			this.manager = manager;
		}

		readonly SectionPreviewManager manager;

		public override void ValidateAll()
		{
			ValidateLanguage();
		}

		public void ValidateLanguage()
		{
			ValidateCalculatedProperty(manager.LanguageInfo);
		}

		protected virtual void CheckLanguage()
		{
			MandatoryValidation.CheckEntered(manager.LanguageInfo);
			ListValidation.ErrorIfInvalidCode(manager.LanguageInfo);
		}

		public override Type AutoValidationType
		{
			get { return null; }
		}
	}
}
