using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public sealed class HeaderOrLineValueValidator<TOut>
	{
		public HeaderOrLineValueValidator(
			Func<TOut> headerValueProvider,
			Func<IEnumerable<TOut>> lineValuesProvider,
			string ruleCode = null)
		{
			_headerValueProvider = Argument.NotNull(headerValueProvider, nameof(headerValueProvider));
			_lineValuesProvider = Argument.NotNull(lineValuesProvider, nameof(lineValuesProvider));
			_ruleCode = ruleCode;
		}

		public Func<TOut, bool> IsEmptyFunc { get; set; }

		public Func<string> EmptyHeaderAndLinesMessageProvider { get; set; }

		public Func<string> IgnoredHeaderSameLinesMessageProvider { get; set; }

		public Func<string> IgnoredHeaderWithLinesValuesMessageProvider { get; set; }

		public Func<string> LineValueEmptyMessageProvider { get; set; }

		public string FieldName { get; set; }

		public void ValidateHeader(ZPropertyInfo headerInfo)
		{
			Argument.NotNull(headerInfo, nameof(headerInfo));

			var valuesAtLineLevelHashSet = _lineValuesProvider().ToHashSet();
			if (valuesAtLineLevelHashSet.Count == 0)
			{
				return;
			}

			var headerValue = _headerValueProvider();
			var headerValueIsEmpty = IsEmpty(headerValue);
			var valuesAtAllLinesAreEmpty = valuesAtLineLevelHashSet.Count == 1 && IsEmpty(valuesAtLineLevelHashSet.Single());

			if (headerValueIsEmpty
				&& valuesAtAllLinesAreEmpty)
			{
				headerInfo.AddMessageError(GetEmptyHeaderAndLinesMessage(headerInfo));
			}

			if (!headerValueIsEmpty
				&& !valuesAtLineLevelHashSet.Contains(headerValue)
				&& !valuesAtLineLevelHashSet.Any(x => IsEmpty(x)))
			{
				var headerWillBeIgnoredMessage = valuesAtLineLevelHashSet.Count == 1
					? GetIgnoredHeaderSameLinesMessage(headerInfo)
					: GetIgnoredHeaderWithLinesValuesMessage(headerInfo);

				headerInfo.AddWarning(headerWillBeIgnoredMessage);
			}
		}

		public void ValidateLine(ZPropertyInfo lineInfo, TOut lineValue)
		{
			Argument.NotNull(lineInfo, nameof(lineInfo));

			var headerValue = _headerValueProvider();

			if (IsEmpty(headerValue)
				&& IsEmpty(lineValue))
			{
				var allLinesValuesAreEmpty = _lineValuesProvider().All(x => IsEmpty(x));

				var lineValueRequiredMessage = allLinesValuesAreEmpty
					? GetEmptyHeaderAndLinesMessage(lineInfo)
					: GetLineValueEmptyMessage(lineInfo);

				lineInfo.AddMessageError(lineValueRequiredMessage);
			}
		}

		bool IsEmpty(TOut value)
		{
			var isEmptyFunc = IsEmptyFunc;

			return isEmptyFunc is null
				? value is null
				: isEmptyFunc(value);
		}

		string GetEmptyHeaderAndLinesMessage(ZPropertyInfo info)
			=> EmptyHeaderAndLinesMessageProvider?.Invoke() ?? GetDefaultEmptyHeaderAndLinesMessage(info);

		string GetIgnoredHeaderSameLinesMessage(ZPropertyInfo info)
			=> IgnoredHeaderSameLinesMessageProvider?.Invoke() ?? GetDefaultIgnoredHeaderSameLinesMessage(info);

		string GetIgnoredHeaderWithLinesValuesMessage(ZPropertyInfo info)
			=> IgnoredHeaderWithLinesValuesMessageProvider?.Invoke() ?? GetDefaultIgnoredHeaderWithLinesValuesMessage(info);

		string GetLineValueEmptyMessage(ZPropertyInfo info)
			=> LineValueEmptyMessageProvider?.Invoke() ?? GetDefaultLineValueEmptyMessage(info);

		string GetDefaultEmptyHeaderAndLinesMessage(ZPropertyInfo info)
			=> FormatMessageWithRuleCodeIfRequired(Res.GetString("505BCE15-996F-4460-AB5D-9F1DF236E517", "A {0} must be declared at header or line level.", FieldName ?? info.HumanReadableName));

		string GetDefaultIgnoredHeaderSameLinesMessage(ZPropertyInfo info)
			=> FormatMessageWithRuleCodeIfRequired(Res.GetString("878B33A6-DF19-493E-9B27-CCF4E522DD9D", "The {0} declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.", FieldName ?? info.HumanReadableName));

		string GetDefaultIgnoredHeaderWithLinesValuesMessage(ZPropertyInfo info)
			=> FormatMessageWithRuleCodeIfRequired(Res.GetString("D4A6CD75-D946-4E38-9FFF-EE2AAD08E9DC", "The {0} declared in the header will be ignored because all lines have values, which different from the header one.", FieldName ?? info.HumanReadableName));

		string GetDefaultLineValueEmptyMessage(ZPropertyInfo info)
			=> FormatMessageWithRuleCodeIfRequired(MandatoryValidation.YouHaveNotEnteredMessage(FieldName ?? info.HumanReadableName));

		string FormatMessageWithRuleCodeIfRequired(string message)
		{
			if (_ruleCode.IsNullOrEmpty())
			{
				return message;
			}

			return $"[{_ruleCode}] {message}";
		}

		readonly Func<IEnumerable<TOut>> _lineValuesProvider;
		readonly Func<TOut> _headerValueProvider;
		readonly string _ruleCode;
	}
}
