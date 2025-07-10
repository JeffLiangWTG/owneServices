using System;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	class RuntimeOptionUserControlFactory
	{
		public static RuntimeOptionUserControl New(FilterField field)
		{
			return New(field.SuggestedUserControlType);
		}

		public static RuntimeOptionUserControl New(FilterFieldSuggestedUserControlType suggestedUserControlType)
		{
			return New(GetType(suggestedUserControlType));
		}

		public static RuntimeOptionUserControl New(Type suggestedUserControlType)
		{
			if (typeof(RuntimeOptionUserControl).IsAssignableFrom(suggestedUserControlType))
			{
				return (RuntimeOptionUserControl)Activator.CreateInstance(suggestedUserControlType);
			}
			else if (suggestedUserControlType == null)
			{
				return null;
			}
			else
			{
				return new UnknownUserControl();
			}
		}

		static Type GetType(FilterFieldSuggestedUserControlType suggestedUserControlType)
		{
			switch (suggestedUserControlType)
			{
				case FilterFieldSuggestedUserControlType.AccountingPeriodFieldUserControl:
					return typeof(AccountingPeriodFieldUserControl);

				case FilterFieldSuggestedUserControlType.AccountingPeriodsRangeUserControl:
					return typeof(AccountingPeriodsRangeUserControl);

				case FilterFieldSuggestedUserControlType.CodeListMultipleChoiceUserControl:
					return typeof(CodeListMultipleChoiceUserControl);

				case FilterFieldSuggestedUserControlType.CodeLookupFieldUserControl:
					return typeof(CodeLookupFieldUserControl);

				case FilterFieldSuggestedUserControlType.ColumnConfigurationFieldUserControl:
					return typeof(ColumnConfigurationFieldUserControl);

				case FilterFieldSuggestedUserControlType.DateFieldUserControl:
					return typeof(DateFieldUserControl);

				case FilterFieldSuggestedUserControlType.DateRangeFieldUserControl:
					return typeof(DateRangeFieldUserControl);

				case FilterFieldSuggestedUserControlType.DateTimeOffsetFieldUserControl:
					return typeof(DateTimeOffsetFieldUserControl);

				case FilterFieldSuggestedUserControlType.DateTimeOffsetRangeFieldUserControl:
					return typeof(DateTimeOffsetRangeFieldUserControl);

				case FilterFieldSuggestedUserControlType.LookupFieldUserControl:
					return typeof(LookupFieldUserControl);

				case FilterFieldSuggestedUserControlType.MultipleChoiceUserControl:
					return typeof(MultipleChoiceUserControl);

				case FilterFieldSuggestedUserControlType.MultipleSelectionLookupUserControl:
					return typeof(MultipleSelectionLookupUserControl);

				case FilterFieldSuggestedUserControlType.NumberNotInRangeUserControl:
					return typeof(NumberNotInRangeUserControl);

				case FilterFieldSuggestedUserControlType.NumberRangeUserControl:
					return typeof(NumberRangeUserControl);

				case FilterFieldSuggestedUserControlType.AccountingNumberRangeUserControl:
					return typeof(AccountingNumberRangeUserControl);

				case FilterFieldSuggestedUserControlType.NumberUserControl:
					return typeof(NumberUserControl);

				case FilterFieldSuggestedUserControlType.OptionGroupUserControl:
					return typeof(OptionGroupUserControl);

				case FilterFieldSuggestedUserControlType.SecurityFilterControl:
					return typeof(SecurityFilterControl);

				case FilterFieldSuggestedUserControlType.SingleAccountingPeriodUserControl:
					return typeof(SingleAccountingPeriodUserControl);

				case FilterFieldSuggestedUserControlType.TextFieldUserControl:
					return typeof(TextFieldUserControl);

				case FilterFieldSuggestedUserControlType.TextRangeFieldUserControl:
					return typeof(TextRangeFieldUserControl);

				case FilterFieldSuggestedUserControlType.ZCheckBox:
					return typeof(ZCheckBox);

				case FilterFieldSuggestedUserControlType.ZMultiLineTextFieldUserControl:
					return typeof(ZMultiLineTextFieldUserControl);

				case FilterFieldSuggestedUserControlType.RegistrationCodedUserControl:
					return typeof(RegistrationCodeUserControl);

				case FilterFieldSuggestedUserControlType.SalesTradeLaneChecklistUserControl:
					return typeof(SalesTradeLaneChecklistUserControl);

				case FilterFieldSuggestedUserControlType.PermitTypeChecklistUserControl:
					return typeof(PermitTypeChecklistUserControl);

				case FilterFieldSuggestedUserControlType.MonthYearPeriodUserControl:
					return typeof(MonthYearPeriodUserControl);

				default:
					return null;
			}
		}
	}
}
