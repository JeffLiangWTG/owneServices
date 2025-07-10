using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business
{
	public class RefLocalLanguageValidation : AutoRefLocalLanguageValidation
	{
		public RefLocalLanguageValidation(AutoRefLocalLanguage parent) : base(parent) { }

		public new RefLocalLanguage Parent
		{
			get { return (RefLocalLanguage)base.Parent; }
		}

		protected override void CheckRA_Code()
		{
			base.CheckRA_Code();
			CheckFullLanguageCode();
			MandatoryValidation.CheckEntered(Parent.RA_CodeInfo);
		}

		protected override void CheckRA_RN_NKCountryCode()
		{
			base.CheckRA_RN_NKCountryCode();
			CheckFullLanguageCode();
			ListValidation.ErrorIfInvalidCode(Parent.RA_RN_NKCountryCodeInfo);
		}

		protected override void CheckRA_Description()
		{
			base.CheckRA_Description();
			MandatoryValidation.CheckEntered(Parent.RA_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.RA_DescriptionInfo);
		}

		public void ValidateParentLanguageCode()
		{
			((IValidationInternals)this).Validate(Parent.ParentLanguageCodeInfo, GetParentLanguageCodeValidationInvoker());
		}

		RunValidationInvoker GetParentLanguageCodeValidationInvoker()
		{
			return delegate
			{
				CheckParentLanguageCode();
			};
		}

		protected void CheckParentLanguageCode()
		{
			if (string.IsNullOrEmpty(Parent.ParentLanguageCode))
			{
				Parent.ParentLanguageCodeInfo.AddWarning(Res.GetString("5e0f7bb8-e7c2-4a92-bbc5-367fd541d468", "Parent language will be defaulted to English."));
			}

			if (!Parent.ParentLanguageCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.ParentLanguageCodeInfo);
				if (!string.IsNullOrEmpty(Parent.ParentLanguageCode) && Parent.ParentLanguageCode == Parent.FullLanguageCode)
				{
					Parent.ParentLanguageCodeInfo.AddError(Res.GetString("b8f58d5e-7b59-471b-877d-d6a4bbfc9a61", "Parent language cannot be the language itself."));
				}
				else if (IsParentLanguageCircularReferenceExist())
				{
					Parent.ParentLanguageCodeInfo.AddError(Res.GetString("ad8b7323-fd51-4ed8-8d22-c6705cbd4756", "Current language is the parent of this language. Please select another language."));
				}
			}
		}

		void CheckFullLanguageCode()
		{
			Parent.FullLanguageCodeInfo.ClearAllNotifications();
			if (!string.IsNullOrEmpty(Parent.FullLanguageCode))
			{
				var query = new ZQuery(RefLocalLanguageSchema.RA_Code, Parent.RA_Code);
				query.AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, Parent.RA_RN_NKCountryCode);
				query.AddToFilter(RefLocalLanguageSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var localLanguage = Parent.Factory.LoadTop1<RefLocalLanguage>(query);
				if (localLanguage != null)
				{
					Parent.FullLanguageCodeInfo.AddError(Res.GetString("8592c46e-19c5-41e4-bc48-d85dcb240915", "Full language code must be unique, please change the ISO language code or change the country/region."));
				}
			}
		}

		bool IsParentLanguageCircularReferenceExist()
		{
			var result = false;
			var parentLanguages = new HashSet<string>();
			parentLanguages.Add(Parent.FullLanguageCode);
			var currentLanguage = Parent;
			while (!currentLanguage.RA_RA_ParentLanguage.IsEmpty)
			{
				currentLanguage = Parent.Factory.Load<RefLocalLanguage>(currentLanguage.RA_RA_ParentLanguage);
				if (currentLanguage == null)
				{
					break;
				}
				else if (parentLanguages.Contains(currentLanguage.FullLanguageCode))
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
