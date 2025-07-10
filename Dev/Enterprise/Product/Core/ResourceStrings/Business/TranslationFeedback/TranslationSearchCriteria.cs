using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public class TranslationSearchCriteria : AutoTranslationSearchCriteria
	{
		public TranslationSearchCriteria()
		{
			using (SuspendSettingHasChanges())
			{
				this.SearchTarget = true;
				this.SearchSource = false;
				this.Replace = false;

				if (!Res.IsSystemDefinedEnglish(Res.CurrentLanguage))
				{
					this.TargetLanguage = Res.CurrentLanguage;
				}
			}
		}

		public override ZBool SearchSource
		{
			get { return base.SearchSource; }
			set
			{
				base.SearchSource = value;
				Validation.ValidateSearchTarget();
				Validation.ValidateSourceText();
			}
		}

		public override ZBool SearchTarget
		{
			get { return base.SearchTarget; }
			set
			{
				base.SearchTarget = value;
				Validation.ValidateSearchSource();
				Validation.ValidateTargetText();
			}
		}

		public override ZBool UseRegularExpressions
		{
			get { return base.UseRegularExpressions; }
			set
			{
				base.UseRegularExpressions = value;
				Validation.ValidateSourceText();
				Validation.ValidateTargetText();
			}
		}

		public ICodeDescriptionPairList Languages
		{
			get
			{
				if (languages == null)
				{
					var list = new CodeDescriptionPairList(OLookUpEditType.Language);
					foreach (ICodeDescription language in list.ToArray())
					{
						if (Res.IsSystemDefinedEnglish(language.Code))
						{
							list.Remove(language);
						}
					}

					languages = list;
				}

				return languages;
			}
		}

		ICodeDescriptionPairList languages;

		public override ZPropertyInfo TargetLanguageInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.TargetLanguage, Res.GetString("a05829e3-bb75-4359-9d5a-78814f810269", "Language"));
			}
		}
	}
}
