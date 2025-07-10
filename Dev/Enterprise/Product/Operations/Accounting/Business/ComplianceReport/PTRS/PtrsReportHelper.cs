using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	internal static class PtrsReportHelper
	{
		#region Helper methods

		internal static string GetAustralianBusinessNumber(OrgHeader org) => GetNumbersOnlyAustralianBusinessNumber(org?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber,
				Core.Constants.CountryCodes.Australia)?.OK_CustomsRegNo.KeepChars(ABNValidation.ValidChars) ?? string.Empty);

		internal static ZString GetNumbersOnlyAustralianBusinessNumber(ZString abn) => abn.KeepChars(ABNValidation.ValidChars);

		internal static string FormatAustralianBusinessNumber(ZString abn) => GetNumbersOnlyAustralianBusinessNumber(abn).Left(11).PadLeft(11, '0');

		#endregion

		#region Extension methods

		internal static void SetHeaderDetails(this PtrsReportBase ptrsReport)
		{
			if (ptrsReport.ComplianceReport != null && ptrsReport.ComplianceReport.Company != null && !ptrsReport.IsSubmitted)
			{
				var company = ptrsReport.ComplianceReport.Company;
				var orgProxy = company.OrgProxy;

				// Column A
				ptrsReport.ATR_CompanyName = orgProxy?.OH_FullName ?? string.Empty;
				// Column B
				ptrsReport.ATR_VATRegNo = PtrsReportHelper.GetAustralianBusinessNumber(orgProxy);
				// Optional details
				ptrsReport.ATR_Address1 = company.GC_Address1;
				ptrsReport.ATR_Address2 = company.GC_Address2;
				ptrsReport.ATR_City = company.GC_City;
				ptrsReport.ATR_State = company.GC_State;
				ptrsReport.ATR_PostCode = company.GC_PostCode;
				ptrsReport.ATR_RN_NKCountryCode = company.GC_RN_NKCountryCode;
			}
		}

		internal static ZInt GetOverriddenOrCalculatedNumber(this PtrsReportBase ptrsReport, string columnName, ZInt calculatedValue, Func<bool> commentRequiredCheck)
			=> ptrsReport.GetColumn<PtrsReportNumberColumn>(columnName, commentRequiredCheck: commentRequiredCheck)?.Value ?? calculatedValue;

		internal static void SetOverriddenNumber(this PtrsReportBase ptrsReport, string columnName, ZInt overriddenValue, EventHandler valueChanged, Func<bool> commentRequiredCheck)
			=> ptrsReport.GetOrCreateColumn<PtrsReportNumberColumn>(columnName, valueChanged: valueChanged, commentRequiredCheck: commentRequiredCheck).Value = overriddenValue;

		internal static ZDecimal GetOverriddenOrCalculatedAmount(this PtrsReportBase ptrsReport, string columnName, ZDecimal calculatedValue, Func<bool> commentRequiredCheck)
			=> ptrsReport.GetColumn<PtrsReportAmountColumn>(columnName, commentRequiredCheck: commentRequiredCheck)?.Value ?? calculatedValue;

		internal static void SetOverriddenAmount(this PtrsReportBase ptrsReport, string columnName, ZDecimal overriddenValue, EventHandler valueChanged, Func<bool> commentRequiredCheck)
			=> ptrsReport.GetOrCreateColumn<PtrsReportAmountColumn>(columnName, valueChanged: valueChanged, commentRequiredCheck: commentRequiredCheck).Value = overriddenValue;

		internal static ZString GetOverrideReason(this PtrsReportBase ptrsReport, string numberColumnName, string valueColumnName, Func<bool> commentRequiredCheck)
			=> ptrsReport.GetColumn<PtrsReportNumberColumn>(numberColumnName, commentRequiredCheck: commentRequiredCheck)?.Comment
			?? ptrsReport.GetColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck)?.Comment ?? ZString.Empty;

		internal static ZString GetOverrideReason(this PtrsReportBase ptrsReport, string valueColumnName, Func<bool> commentRequiredCheck)
			=> ptrsReport.GetColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck)?.Comment ?? ZString.Empty;

		internal static void SetOverrideReason(this PtrsReportBase ptrsReport, string numberColumnName, string valueColumnName, ZString reason, Func<bool> commentRequiredCheck, ZInt number, ZDecimal amount)
		{
			var numberColumn = ptrsReport.GetColumn<PtrsReportNumberColumn>(numberColumnName, commentRequiredCheck: commentRequiredCheck);
			if (numberColumn == null)
			{
				numberColumn = ptrsReport.GetOrCreateColumn<PtrsReportNumberColumn>(numberColumnName, commentRequiredCheck: commentRequiredCheck);
				numberColumn.Value = number;
			}
			numberColumn.Comment = reason;
			numberColumn.CommentInfo.RefreshBinding();

			var valueColumn = ptrsReport.GetColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck);
			if (valueColumn == null)
			{
				valueColumn = ptrsReport.GetOrCreateColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck);
				valueColumn.Value = amount;
			}
			valueColumn.Comment = reason;
			valueColumn.CommentInfo.RefreshBinding();
		}

		internal static void SetOverrideReason(this PtrsReportBase ptrsReport, string valueColumnName, ZString reason, Func<bool> commentRequiredCheck)
		{
			var column = ptrsReport.GetOrCreateColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck);
			column.Comment = reason;
			column.CommentInfo.RefreshBinding();
		}

		internal static ZPropertyInfo GetOverrideReasonInfo(this PtrsReportBase ptrsReport, string numberColumnName, string valueColumnName, Func<bool> commentRequiredCheck, ZDecimal calculatedAmount)
		{
			var result = ptrsReport.GetColumn<PtrsReportNumberColumn>(numberColumnName, commentRequiredCheck: commentRequiredCheck)?.CommentInfo
				?? ptrsReport.GetColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck)?.CommentInfo;

			if (result == null)
			{
				// An ultimate fallback option which should not happen
				var amountColumn = ptrsReport.GetOrCreateColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck);
				amountColumn.Value = calculatedAmount;

				result = amountColumn.CommentInfo;
			}
			return result;
		}

		internal static ZPropertyInfo GetOverrideReasonInfo(this PtrsReportBase ptrsReport, string valueColumnName, Func<bool> commentRequiredCheck, ZDecimal calculatedAmount)
		{
			var result = ptrsReport.GetColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck)?.CommentInfo;
			if (result == null)
			{
				var column = ptrsReport.GetOrCreateColumn<PtrsReportAmountColumn>(valueColumnName, commentRequiredCheck: commentRequiredCheck);
				column.Value = calculatedAmount;

				result = column.CommentInfo;
			}
			return result;
		}

		internal static void ValidateCommentsOfNumberOverrideColumns(this PtrsReportBase ptrsReport, params string[] columnNames)
		{
			foreach (var columnName in columnNames)
			{
				(ptrsReport.GetColumn<PtrsReportNumberColumn>(columnName) as IHaveCommentRequiredValidation)?.ValidateComment();
			}
		}

		internal static void ValidateCommentsOfValueOverrideColumns(this PtrsReportBase ptrsReport, params string[] columnNames)
		{
			foreach (var columnName in columnNames)
			{
				(ptrsReport.GetColumn<PtrsReportAmountColumn>(columnName) as IHaveCommentRequiredValidation)?.ValidateComment();
			}
		}

		internal static T GetColumn<T>(this PtrsReportBase ptrsReport, string code, int maxLengthOrDecimalPlaces = -1, EventHandler valueChanged = null, Func<bool> commentRequiredCheck = null, Action<ZPropertyInfo> valueCheck = null) where T : PtrsReportColumnWrapperBase
		{
			var result = ptrsReport.ColumnWrappers.OfType<T>().FirstOrDefault(x => x.Code.EqualsIgnoringCase(code));

			if (result == null)
			{
				var column = ptrsReport.GetExistingAccTaxReturnColumn(code);
				if (column != null)
				{
					result = CreateWrapper<T>(column);
					ptrsReport.ColumnWrappers.Add(result);
				}
			}

			result?.SetupWrapper(maxLengthOrDecimalPlaces, valueChanged, commentRequiredCheck, valueCheck);
			return result;
		}

		internal static T GetOrCreateColumn<T>(this PtrsReportBase ptrsReport, string code, int maxLengthOrDecimalPlaces = -1, EventHandler valueChanged = null, Func<bool> commentRequiredCheck = null, Action<ZPropertyInfo> valueCheck = null) where T : PtrsReportColumnWrapperBase
		{
			var result = ptrsReport.ColumnWrappers.OfType<T>().FirstOrDefault(x => x.Code.EqualsIgnoringCase(code));
			if (result == null)
			{
				var column = ptrsReport.GetExistingAccTaxReturnColumn(code);
				if (column == null)
				{
					column = ptrsReport.Columns.AddNew();
					using (column.SuspendSettingHasChanges())
					{
						column.ATC_ColumnName = code;
						column.ATC_GroupCode = ptrsReport.ComplianceReport?.ACR_ReportType ?? ZString.Empty;
					}
				}

				result = CreateWrapper<T>(column);
				ptrsReport.ColumnWrappers.Add(result);
			}

			result.SetupWrapper(maxLengthOrDecimalPlaces, valueChanged, commentRequiredCheck, valueCheck);
			return result;
		}

		static AccTaxReturnColumn GetExistingAccTaxReturnColumn(this PtrsReportBase ptrsReport, string code)
			=> ptrsReport.Columns.OfType<AccTaxReturnColumn>().FirstOrDefault(x => x.ATC_ColumnName.EqualsIgnoringCase(code));

		static void SetupWrapper<T>(this T columnWrapper, int maxLengthOrDecimalPlaces, EventHandler valueChanged, Func<bool> commentRequiredCheck, Action<ZPropertyInfo> valueCheck) where T : PtrsReportColumnWrapperBase
		{
			if (maxLengthOrDecimalPlaces > 0)
			{
				if (columnWrapper is IHaveMaxLength withMaxLength)
				{
					withMaxLength.MaxLength = maxLengthOrDecimalPlaces;
				}
				else if (columnWrapper is IHaveDecimalPlaces withDecimalPlaces)
				{
					withDecimalPlaces.DecimalPlaces = maxLengthOrDecimalPlaces;
				}
			}

			if (valueChanged != null
				&& (columnWrapper is IHandleValueChanged handlesValueChanged)
				&& !handlesValueChanged.IsValueChangedHandlerAttached)
			{
				handlesValueChanged.AttachValueChangedHandlerOnlyOnce(valueChanged);
			}

			if (commentRequiredCheck != null
				&& (columnWrapper is IHaveCommentRequiredValidation commentRequired)
				&& !commentRequired.HasCommentRequiredCheck)
			{
				commentRequired.SetIsCommentRequiredCheck(commentRequiredCheck);
			}

			if (valueCheck != null
				&& (columnWrapper is IHaveValueValidation withValueCheck)
				&& !withValueCheck.HasValueCheck)
			{
				withValueCheck.SetValueCheck(valueCheck);
			}
		}

		// Not the best but the quickest way to do it
		static T CreateWrapper<T>(AccTaxReturnColumn column) where T : PtrsReportColumnWrapperBase
		{
			if  (typeof(T) == typeof(PtrsReportAmountColumn))
			{
				return new PtrsReportAmountColumn(column) as T;
			}
			if (typeof(T) == typeof(PtrsReportCommentColumn))
			{
				return new PtrsReportCommentColumn(column) as T;
			}
			if (typeof(T) == typeof(PtrsReportDateColumn))
			{
				return new PtrsReportDateColumn(column) as T;
			}
			if (typeof(T) == typeof(PtrsReportNumberColumn))
			{
				return new PtrsReportNumberColumn(column) as T;
			}
			if (typeof(T) == typeof(PtrsReportPercentColumn))
			{
				return new PtrsReportPercentColumn(column) as T;
			}
			if (typeof(T) == typeof(PtrsReportRegNumberColumn))
			{
				return new PtrsReportRegNumberColumn(column) as T;
			}
			if (typeof(T) == typeof(PtrsReportEmailColumn))
			{
				return new PtrsReportEmailColumn(column) as T;
			}

			throw new NotImplementedException("Unsupported subclass of the PtrsReportColumnWrapperBase: " + typeof(T).FullName);
		}

		#endregion
	}

	public interface IHaveMaxLength
	{
		int MaxLength { get; set; }
	}

	public interface IHaveDecimalPlaces
	{
		int DecimalPlaces { get; set; }
	}

	public interface IHaveValueValidation
	{
		void SetValueCheck(Action<ZPropertyInfo> valueCheck);
		bool HasValueCheck { get; }
	}

	public interface IHaveCommentRequiredValidation
	{
		void ValidateComment();
		void SetIsCommentRequiredCheck(Func<bool> isCommentRequired);
		bool HasCommentRequiredCheck { get; }
	}

	internal interface IHandleValueChanged
	{
		void AttachValueChangedHandlerOnlyOnce(EventHandler valueChanged);
		bool IsValueChangedHandlerAttached { get; }
	}
}
