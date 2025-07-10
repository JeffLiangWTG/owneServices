using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.BarcodeParsing.Business
{
	public abstract class BarcodeDiagnosticsRunner<TRule> : IBarcodeDiagnosticsRunner
	{
		protected BarcodeDiagnosticsRunner()
		{
		}

		public BarcodeDiagnosticsResult Run(IBarcodeParsingConsumer consumer, LoadMatchingRulesParameters loadMatchingRulesParameters, string barcode, string targetField)
		{
			Argument.NotNull(consumer, nameof(consumer));

			var consumerGenericType = consumer.GetType().BaseType;
			if (consumerGenericType?.GetGenericTypeDefinition() == typeof(BarcodeParsingConsumer<>))
			{
				var enumType = consumerGenericType.GetGenericArguments()[0];
				var rules = GetRules(consumer.Factory, loadMatchingRulesParameters);
				return (BarcodeDiagnosticsResult)MethodInfo.MakeGenericMethod(new[] { enumType }).Invoke(this, new object[] { consumer, rules, barcode, targetField });
			}
			else
			{
				throw new InvalidOperationException("BarcodeParsingConsumer must have a valid Enum for Target Fields.");
			}
		}

		protected abstract IReadOnlyCollection<TRule> GetRules(BusinessObjectFactory factory, LoadMatchingRulesParameters parameters);

		protected abstract BarcodeDiagnosticsResult GetResult<TEnum>(IBarcodeParsingConsumer consumer, IEnumerable<TRule> rules, string barcode, string targetField)
			where TEnum : struct, IFormattable, IConvertible, IComparable;

		MethodInfo MethodInfo => methodInfo ??= GetType().GetMethod(nameof(GetResult), BindingFlags.NonPublic | BindingFlags.Instance);

		MethodInfo methodInfo;
	}
}
