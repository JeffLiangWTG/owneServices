using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(ValidationRuleMessages))]
	public abstract class ValidationRuleMessagesAbstractTest<TMessages> : TestCase
		where TMessages : ValidationRuleMessages
	{
		public void TestAllRuleCodeProperties() => CombineAssertions(() =>
		{
			var count = 0;
			foreach (var property in configuration.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				var nameMatch = Regex.Match(property.Name, RuleCodePropertyNamePattern);
				if (nameMatch.Success)
				{
					count++;
					var euRuleCode = nameMatch.Groups[1].Value.Replace('_', '-');
					AssertEquals(property.Name, RuleCodeReplacements.TryGetValue(euRuleCode, out var replacedCode) ? replacedCode : euRuleCode, property.GetValue(configuration));
				}
			}
			AssertNotEquals("No property matches; test probably wrong", 0, count);
		});

		public void TestRuleCodeReplacedInMessageProperties() => CombineAssertions(() =>
		{
			var count = 0;
			var euMessages = new ValidationRuleMessages();
			var replacements = RuleCodeReplacements;

			if (replacements.Count > 0)
			{
				foreach (var property in configuration.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					var nameMatch = Regex.Match(property.Name, RuleMessagePropertyNamePattern);
					if (nameMatch.Success)
					{
						count++;
						var euMessage = property.GetValue(euMessages).ToString();
						var replacement = RuleCodeReplacements.FirstOrDefault(x => euMessage.StartsWith($"[{x.Key}]"));
						if (replacement.Key != null)
						{
							Assert(property.Name, property.GetValue(configuration).ToString().StartsWith($"[{replacement.Value}]"));
						}
					}
				}
				AssertNotEquals("No property matches; test propably wrong", 0, count);
			}
			else
			{
				Assert(true);
			}
		});

		public void TestAllRuleCodeReplacementsAreForKnownCodes() => CombineAssertions(() =>
		{
			if (RuleCodeReplacements.Any())
			{
				foreach (var replacement in RuleCodeReplacements)
				{
					var propertyName = RuleCodePropertyName(replacement.Key);
					var property = configuration.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
					AssertNotNull($"Property {propertyName}", property);
				}
			}
			else
			{
				Assert("No RuleCodeReplacements", true);
			}
		});

		protected virtual Dictionary<string, string> RuleCodeReplacements => new() { { "C0101-1", "C0101, R0859" } };

		static string RuleCodePropertyName(string ruleCode) => ruleCode.Replace("-", "_") + "RuleCode";
		protected const string RuleCodePropertyNamePattern = @"^([A-Za-z0-9_]{4,})RuleCode";
		protected const string RuleMessagePropertyNamePattern = @"^([A-Za-z0-9_]{4,})Message";

		#region Implementation

		protected TMessages configuration;

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (TMessages)Activator.CreateInstance(typeof(TMessages));
		}

		#endregion
	}

	[TestedType(typeof(ValidationRuleMessages))]
	sealed class ValidationRuleMessagesBaseOnlyTest : ValidationRuleMessagesAbstractTest<ValidationRuleMessages>
	{
		public void TestFormatMessage()
		{
			AssertEquals("[R0001] Message Text", ValidationRuleMessages.FormatMessage("R0001", "Message Text"));
		}
	}
}
