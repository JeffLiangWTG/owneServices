using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.ZArchitecture.Environment.RegistryItemWrapper;

namespace Enterprise.Registry.Business
{
	public static class RegistryItemUpdateLoggerFactory
	{
		#region Bind Logger

		public static void BindLogger(RegistryItemWrapper wrapper)
		{
			if (wrapper != null && !ExistSensitiveInfo(wrapper) && wrapper.OnBuildLogReference == null)
			{
				var handler = LoadLogger(wrapper);
				if (handler != null)
				{
					wrapper.OnBuildLogReference = handler;
				}
			}
		}

		#endregion

		#region Load Logger

		public static BuildLogReferenceHandler LoadLogger(IRegistryItem registryItem)
		{
			Argument.NotNull(registryItem, nameof(registryItem));

			return registryItem switch
			{
				BooleanRegistryItem => LogBooleanRegistryItem,
				CodePairRegistryItem => LogCodePairRegistryItem,
				CodePairWithAdditionalEventRegistryItem => LogCodePairRegistryItem,
				CurrencyDecimalSeparatorRegistryItem => LogSeparatorRegistryItem,
				CurrencyGroupSeparatorRegistryItem => LogSeparatorRegistryItem,
				CurrencyGroupSizesStringListRegistryItem => LogCurrencyGroupSizesStringListRegistryItem,
				DateTimeRegistryItem => LogDateTimeRegistryItem,
				DecimalRegistryItem => LogDecimalRegistryItem,
				DeleteExpiredRatesRegistryItem => LogDeleteExpiredRatesRegistryItem,
				GmailOAuth2JsonFileRegistryItem => LogGmailOAuth2JsonFileRegistryItem,
				GuidArrayRegistryItem => LogGuidArrayRegistryItem,
				GuidRegistryItem => LogGuidRegistryItem,
				ImageRegistryItem => LogImageRegistryItem,
				IntRegistryItem => LogIntRegistryItem,
				JsonStringArrayRegistryItem => LogStringArrayRegistryItem,
				MultilingualStringRegistryItem => LogMultilingualStringRegistryItem,
				NumberDecimalSeparatorRegistryItem => LogSeparatorRegistryItem,
				NumberGroupSeparatorRegistryItem => LogSeparatorRegistryItem,
				NumberGroupSizesStringListRegistryItem => LogNumberGroupSizesStringListRegistryItem,
				QuoteValidityRegistryItem => LogQuoteValidityRegistryItem,
				RatesServiceUrlRegistryItem => LogUrlRegistryItem,
				ServiceUrlRegistryItem => LogUrlRegistryItem,
				StringArrayRegistryItem => LogStringArrayRegistryItem,
				StringRegistryItem => LogStringRegistryItem,
				WebPrintNudgeRegistryItem => LogWebPrintNudgeRegistryItem,
				_ => TryLoadFromGenericType(registryItem)
			};
		}

		#endregion

		static bool ExistSensitiveInfo(RegistryItemWrapper wrapper)
		{
			if (wrapper.Options.HasFlag(RegistryOptions.IsPasswordVisibleForControllerUser) ||
				wrapper.Options.HasFlag(RegistryOptions.IsOnlyForClassic_eAdaptor))
			{
				return true;
			}

			return wrapper.EditorInfo is TextRegistryEditorInfo { EditorType: TextEditorType.Password };
		}

		static BuildLogReferenceHandler TryLoadFromGenericType(IRegistryItem registryItem)
		{
			var type = registryItem.GetType();
			var baseType = type.BaseType;

			if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(StronglyTypedRegistryItem<>))
			{
				var genericArguments = baseType.GetGenericArguments();
				if (genericArguments.Length == 1)
				{
					var genericArgument = genericArguments[0];
					if (genericArgument == typeof(string))
					{
						return LogStringRegistryItem;
					}

					if (genericArgument == typeof(int))
					{
						return LogIntRegistryItem;
					}

					if (genericArgument == typeof(bool))
					{
						return LogBinaryRegistryItem;
					}
				}
			}

			return null;
		}

		#region Common Methods

		static string LogText(BuildLogReferenceArgs args, string description = "")
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}|OLD={1}|NEW={2}", description, args.OriginalValue, args.NewValue);
		}

		static string LogText(string oldValueLogs, string newValueLogs, string description = "")
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}|OLD={1}|NEW={2}", description, oldValueLogs, newValueLogs);
		}

		static string GetArrayLogs<T>(T[] array)
		{
			if (array != null)
			{
				var builder = new StringBuilder($"[Count: {array.Length}");
				foreach (var item in array)
				{
					builder.Append($", {item.ToString()}");
				}

				builder.Append("]");
				return builder.ToString();
			}

			return string.Empty;
		}

		#endregion

		static string LogBinaryRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Binary");
		}

		static string LogBooleanRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Boolean");
		}

		static string LogCodePairRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"CodePair");
		}

		static string LogCurrencyGroupSizesStringListRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Currency Group Size");
		}

		static string LogDateTimeRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"DateTime");
		}

		static string LogDecimalRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Decimal");
		}

		#region LogDeleteExpiredRatesRegistryItem

		static string LogDeleteExpiredRatesRegistryItem(BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as DeleteExpiredRates;
			var newValue = args.NewValue as DeleteExpiredRates;

			return LogText(GetDeleteExpiredRatesLogs(originalValue), GetDeleteExpiredRatesLogs(newValue), (NoResString)"Delete Expired Rates");
		}

		static string GetDeleteExpiredRatesLogs(DeleteExpiredRates rates)
		{
			if (rates != null)
			{
				return $"[Years:{rates.ExpiredRatesPeriodInYears}, Size:{rates.BatchSize}]";
			}

			return string.Empty;
		}

		#endregion

		#region LogGmailOAuth2JsonFileRegistryItem

		static string LogGmailOAuth2JsonFileRegistryItem(BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as GmailOAuth2JsonFile;
			var newValue = args.NewValue as GmailOAuth2JsonFile;

			return LogText(GetGmailOAuth2JsonFileLogs(originalValue), GetGmailOAuth2JsonFileLogs(newValue), (NoResString)"Json File");
		}

		static string GetGmailOAuth2JsonFileLogs(GmailOAuth2JsonFile jsonFile)
		{
			if (jsonFile != null)
			{
				return $"[{jsonFile.FileName}]";
			}

			return string.Empty;
		}

		#endregion

		static string LogGuidArrayRegistryItem(BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as Guid[];
			var newValue = args.NewValue as Guid[];

			return LogText(GetArrayLogs(originalValue), GetArrayLogs(newValue), (NoResString)"GuidArray");
		}

		static string LogGuidRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Guid");
		}

		#region LogImageRegistryItem

		static string LogImageRegistryItem(BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as Image;
			var newValue = args.NewValue as Image;

			return LogText(GetImageLogs(originalValue), GetImageLogs(newValue), (NoResString)"Image");
		}

		static string GetImageLogs(Image image)
		{
			if (image != null)
			{
				return $"[Width: {image.Width}, Height: {image.Height}, Format: {image.PixelFormat}]";
			}

			return string.Empty;
		}

		#endregion

		static string LogIntRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Int");
		}

		static string LogMultilingualStringRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Multilingual");
		}

		static string LogNumberGroupSizesStringListRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Number Group Size");
		}

		static string LogSeparatorRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Separator");
		}

		static string LogQuoteValidityRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Quote");
		}

		static string LogUrlRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"Url");
		}

		static string LogStringArrayRegistryItem(BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as string[];
			var newValue = args.NewValue as string[];

			return LogText(GetArrayLogs(originalValue), GetArrayLogs(newValue), (NoResString)"String Array");
		}

		internal static string LogStringRegistryItem(BuildLogReferenceArgs args)
		{
			return LogText(args, (NoResString)"String");
		}

		#region LogWebPrintNudgeRegistryItem

		static string LogWebPrintNudgeRegistryItem(BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as WebPrintNudge;
			var newValue = args.NewValue as WebPrintNudge;

			return LogText(GetWebPrintNudgeLogs(originalValue), GetWebPrintNudgeLogs(newValue), (NoResString)"Print Nudge");
		}

		static string GetWebPrintNudgeLogs(WebPrintNudge nudge)
		{
			if (nudge != null)
			{
				return $"[IpEnabled: {nudge.EnableIPAddress}, Hours: {nudge.SwtichBackToIPAddressIntervalInHours}]";
			}

			return string.Empty;
		}

		#endregion
	}
}
