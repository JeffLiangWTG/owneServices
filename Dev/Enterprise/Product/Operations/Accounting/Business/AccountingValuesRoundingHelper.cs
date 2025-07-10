using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public sealed class AccountingValuesRoundingHelper
	{
		AccountingValuesRoundingHelper() { }

		public static bool PropertyHasChanges(BusinessObject parent, bool hasChanges)
		{
			if (parent.HasContext(BusinessContext.ChangingCurrencyToDifferentDecimalPlaces))
			{
				return true;
			}

			return hasChanges;
		}

		internal static void ReportErrorIfPropertiesNotRounded(BusinessObject parent, IEnumerable<string> propertiesToCheck, string oldCurrency, string newCurrency, FunctionalitySuspender reportingSuspender)
		{
			if (HasChangeInDecimalPlaces(parent.Factory, oldCurrency, newCurrency))
			{
				ReportErrorIfPropertiesNotRoundedCore(parent, propertiesToCheck, oldCurrency, newCurrency, reportingSuspender);
			}
		}

		static void ReportErrorIfPropertiesNotRoundedCore(BusinessObject parent, IEnumerable<string> propertiesToCheck, string oldCurrency, string newCurrency, FunctionalitySuspender reportingSuspender)
		{
			var incorectlyRoundedValue = false;
			var decimals = RefCurrency.LoadFromCurrencyCode(parent.Factory, newCurrency).Decimals;
			List<string> propertiesRoundedWrong = null;

			foreach (var property in propertiesToCheck)
			{
				if (parent[property] is ZDecimal number && number.DecimalPlaces > decimals)
				{
					if (propertiesRoundedWrong == null)
					{
						propertiesRoundedWrong = new List<string>();
					}

					incorectlyRoundedValue = true;
					propertiesRoundedWrong.Add(FormattableString.Invariant($"{property}: {number}"));
				}
			}

			if (!incorectlyRoundedValue || (reportingSuspender?.IsSuspended ?? false))
			{
				return;
			}

			StringBuilder result = new StringBuilder();
			result.AppendLine(FormattableString.Invariant($"In {parent.GetType().ToString()} -"));
			result.AppendLine((NoResString)"Properties requiring to be rounded on currency change have more decimal places than allowed, this may be due to setter not propagating values due to has changes check.");
			result.AppendLine((NoResString)"Incorrectly rounded values are:");

			foreach (string s in propertiesRoundedWrong)
			{
				result.AppendLine(s);
			}

			result.AppendLine(FormattableString.Invariant($"Currency {oldCurrency} was changed to {newCurrency}"));
			result.AppendLine(FormattableString.Invariant($"Only {decimals} decimals are allowed."));
			ErrorReporter.ReportOnce("PropertiesNotRoundedOnCurrecyChange", result.ToString());
		}

		internal static bool HasChangeInDecimalPlaces(BusinessObjectFactory factory, string prevCurrencyCode, string newCurrencyCode)
		{
			var oldCurrency = RefCurrency.LoadFromCurrencyCode(factory, prevCurrencyCode);
			var newCurrency = RefCurrency.LoadFromCurrencyCode(factory, newCurrencyCode);
			return newCurrency != null && (oldCurrency == null || newCurrency.Decimals != oldCurrency.Decimals);
		}

		public static DisposableAction GetActionForChangeInDecimalPlaces(BusinessObject bizo, IEnumerable<string> propertiesRequiringRounding, string prevCurrencyCode, string newCurrencyCode, FunctionalitySuspender reportingSuspender)
		{
			var changeInDecimalPlaces = HasChangeInDecimalPlaces(bizo.Factory, prevCurrencyCode, newCurrencyCode);

			if (!changeInDecimalPlaces)
			{
				return DisposableAction.NoAction;
			}

			var setContext = new Action(() => {
				bizo.SetContext(BusinessContext.ChangingCurrencyToDifferentDecimalPlaces);
			});

			var removeContext = new Action(() => {
				bizo.RemoveContext(BusinessContext.ChangingCurrencyToDifferentDecimalPlaces);
				ReportErrorIfPropertiesNotRoundedCore(bizo, propertiesRequiringRounding, prevCurrencyCode, newCurrencyCode, reportingSuspender);
			});

			return new DisposableAction(setContext, removeContext);
		}
	}
}
