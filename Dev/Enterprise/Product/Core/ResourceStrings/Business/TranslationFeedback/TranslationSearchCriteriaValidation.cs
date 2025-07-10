using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class TranslationSearchCriteriaValidation : AutoTranslationSearchCriteriaValidation
	{
		public TranslationSearchCriteriaValidation(AutoTranslationSearchCriteria parent)
			: base(parent)
		{ }

		#region Implementation

		public new TranslationSearchCriteria Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (TranslationSearchCriteria)base.Parent; }
		}

		#endregion

		protected override void CheckSourceText()
		{
			if (Parent.SearchSource)
			{
				MandatoryValidation.CheckEntered(Parent.SourceTextInfo);
				if (Parent.UseRegularExpressions)
				{
					try
					{
						new Regex(Parent.SourceText);
					}
					catch (ArgumentException e)
					{
						Parent.SourceTextInfo.AddError("Regular expression parse error: " + e.Message);
					}
				}
			}

			base.CheckSourceText();
		}

		protected override void CheckTargetText()
		{
			if (Parent.SearchTarget)
			{
				MandatoryValidation.CheckEntered(Parent.TargetTextInfo);
				if (Parent.UseRegularExpressions)
				{
					try
					{
						new Regex(Parent.TargetText);
					}
					catch (ArgumentException e)
					{
						Parent.TargetTextInfo.AddError("Regular expression parse error: " + e.Message);
					}
				}
			}

			base.CheckTargetText();
		}

		protected override void CheckTargetLanguage()
		{
			MandatoryValidation.CheckEntered(Parent.TargetLanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TargetLanguageInfo, Parent.Languages);
			base.CheckTargetLanguage();
		}

		protected override void CheckSearchSource()
		{
			CheckSearchSourceAndOrTarget(Parent.SearchSourceInfo);
			base.CheckSearchSource();
		}

		protected override void CheckSearchTarget()
		{
			CheckSearchSourceAndOrTarget(Parent.SearchTargetInfo);
			base.CheckSearchTarget();
		}

		void CheckSearchSourceAndOrTarget(ZPropertyInfo propertyInfo)
		{
			if (!Parent.SearchSource && !Parent.SearchTarget)
			{
				propertyInfo.AddError("You must select something to search by");
			}
		}
	}
}
