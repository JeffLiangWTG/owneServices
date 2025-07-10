using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(ValidationRuleConfiguration))]
	public abstract class ValidationRuleConfigurationAbstractTest<TConfiguration> : TestCaseWithFactory
		where TConfiguration : ValidationRuleConfiguration
	{
		public void TestValidationRuleConfigurationIsSealed()
		{
			if (typeof(TConfiguration) == typeof(ValidationRuleConfiguration))
			{
				Assert("ValidationRuleConfiguration itself doesn't have to be sealed", true);
			}
			else
			{
				Assert("Class deriving from ValidationRuleConfiguration should be sealed", typeof(TConfiguration).IsSealed);
			}
		}

		public void TestAllRulesInAlphabeticalOrder()
		{
			var providerType = TestedTypeHelper.GetTestedType(GetType());
			var propertyNameList = providerType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).Where(x => Regex.IsMatch(x, IsRuleActivePropertyNamePattern)).ToArray();
			var propertiesForOrder = new List<(string PropertyName, (string RuleName, string SubRuleCode) Rule)>();
			var regex = new Regex(IsRuleActivePropertyNamePattern);

			foreach (var propertyName in propertyNameList)
			{
				var match = regex.Match(propertyName);
				if (match.Success)
				{
					var ruleName = match.Groups[1].Value;
					if (ruleName.Contains('_'))
					{
						var ruleInfo = ruleName.Split('_');
						propertiesForOrder.Add((PropertyName: propertyName, (RuleName: ruleInfo[0], SubRuleCode: ruleInfo[1])));
					}
					else
					{
						propertiesForOrder.Add((PropertyName: propertyName, (RuleName: ruleName, SubRuleCode: "0")));
					}
				}
			}

			var expectedPropertyNameList = propertiesForOrder
				.OrderBy(x => x.Rule.RuleName)
				.ThenBy(x => x.Rule.SubRuleCode)
				.Select(x => x.PropertyName).ToList();
			AssertSequencesEqual("Rule properties should be sorted in alphanumeric order.", expectedPropertyNameList, propertyNameList);
		}

		public void TestAllRulesHaveATestCase()
		{
			var existingTestMethods = GetType().GetMethods().Where(methodInfo => methodInfo.Name.StartsWith("Test")).Select(methodInfo => ExtractMethodNameFromTestMethodName(methodInfo.Name)).Distinct().ToArray();
			var providerType = TestedTypeHelper.GetTestedType(GetType());
			var providerRuntimeProperties = providerType.GetRuntimeProperties();
			var publicPropertyInfos = providerRuntimeProperties.Where(pi => pi.GetGetMethod(true).IsPublic && (pi.DeclaringType == providerType || pi.DeclaringType == typeof(ValidationRuleConfiguration)) && Regex.IsMatch(pi.Name, IsRuleActivePropertyNamePattern));
			var explicitPropertyInfos = providerRuntimeProperties.Where(pi => pi.GetGetMethod(true).IsPrivate && pi.Name.Contains("."));
			var propertyInfosToTest = publicPropertyInfos.Union(explicitPropertyInfos);
			if (propertyInfosToTest.Any())
			{
				CombineAssertions(() =>
				{
					foreach (var propertyInfo in propertyInfosToTest)
					{
						var name = ExtractNameFromPropertyInfo(propertyInfo.Name);
						Assert($"Property: {name}", existingTestMethods.Contains("Test" + name));
					}
				});
			}
			else
			{
				Assert("There are no differences to test from the abstract base class or explicit interface implementation", true);
			}

			string ExtractMethodNameFromTestMethodName(string name)
			{
				const string keyword = "Active";
				var posUnderscore = name.IndexOf($"{keyword}_");
				return posUnderscore == -1 ? name : name.Substring(0, posUnderscore + keyword.Length);
			}

			string ExtractNameFromPropertyInfo(string name)
			{
				var lastFullStop = name.LastIndexOf('.');
				return lastFullStop == -1 ? name : name.Substring(lastFullStop + 1);
			}
		}

		protected const string IsRuleActivePropertyNamePattern = @"^IsRule([A-Za-z0-9_]{4,})Active$";

		public virtual void TestIsRuleB1811Active()
		{
			AssertEquals(true, configuration.IsRuleB1811Active);
		}

		public virtual void TestIsRuleB1820_1Active()
		{
			AssertEquals(false, configuration.IsRuleB1820_1Active);
		}

		public virtual void TestIsRuleB1820_2Active()
		{
			AssertEquals(false, configuration.IsRuleB1820_2Active);
		}

		public virtual void TestIsRuleB1822Active()
		{
			AssertEquals(false, configuration.IsRuleB1822Active);
		}

		public virtual void TestIsRuleB1848_1Active()
		{
			AssertEquals(false, configuration.IsRuleB1848_1Active);
		}

		public virtual void TestIsRuleB1877_1Active()
		{
			AssertEquals(false, configuration.IsRuleB1877_1Active);
		}

		public virtual void TestIsRuleB1896Active()
		{
			AssertEquals(false, configuration.IsRuleB1896Active);
		}

		public virtual void TestIsRuleC0001_2Active()
		{
			AssertEquals(false, configuration.IsRuleC0001_2Active);
		}

		public virtual void TestIsRuleC0030Active()
		{
			AssertEquals(true, configuration.IsRuleC0030Active);
		}
		public virtual void TestIsRuleC0065Active()
		{
			AssertEquals(true, configuration.IsRuleC0065Active);
		}

		public virtual void TestIsRuleC0101Active()
		{
			AssertEquals(true, configuration.IsRuleC0101Active);
		}

		public virtual void TestIsRuleC0111Active()
		{
			AssertEquals(true, configuration.IsRuleC0111Active);
		}

		public virtual void TestIsRuleC0186Active()
		{
			AssertEquals(true, configuration.IsRuleC0186Active);
		}

		public virtual void TestIsRuleC0215Active()
		{
			AssertEquals(true, configuration.IsRuleC0215Active);
		}

		public virtual void TestIsRuleC0236Active()
		{
			AssertEquals(true, configuration.IsRuleC0236Active);
		}

		public virtual void TestIsRuleC0240_3Active()
		{
			AssertEquals(true, configuration.IsRuleC0240_3Active);
		}

		public virtual void TestIsRuleC0337Active()
		{
			AssertEquals(true, configuration.IsRuleC0337Active);
		}

		public virtual void TestIsRuleC0382Active()
		{
			AssertEquals(true, configuration.IsRuleC0382Active);
		}

		public virtual void TestIsRuleC0394Active()
		{
			AssertEquals(true, configuration.IsRuleC0394Active);
		}

		public virtual void TestIsRuleC0411Active()
		{
			AssertEquals(true, configuration.IsRuleC0411Active);
		}

		public virtual void TestIsRuleC0505Active()
		{
			AssertEquals(true, configuration.IsRuleC0505Active);
		}

		public virtual void TestIsRuleC0542Active()
		{
			AssertEquals(true, configuration.IsRuleC0542Active);
		}

		public virtual void TestIsRuleC0542_1Active()
		{
			AssertEquals(false, configuration.IsRuleC0542_1Active);
		}

		public virtual void TestIsRuleC0587Active()
		{
			AssertEquals(true, configuration.IsRuleC0587Active);
		}

		public virtual void TestIsRuleC0587_1Active()
		{
			AssertEquals(false, configuration.IsRuleC0587_1Active);
		}

		public virtual void TestIsRuleC0587_2Active()
		{
			AssertEquals(false, configuration.IsRuleC0587_2Active);
		}

		public virtual void TestIsRuleC0823Active()
		{
			AssertEquals(true, configuration.IsRuleC0823Active);
		}

		public virtual void TestIsRuleC0839Active()
		{
			AssertEquals(true, configuration.IsRuleC0839Active);
		}

		public virtual void TestIsRuleC0904Active()
		{
			AssertEquals(true, configuration.IsRuleC0904Active);
		}

		public virtual void TestIsRuleE1102Active()
		{
			AssertEquals(true, configuration.IsRuleE1102Active);
		}

		public virtual void TestIsRuleE1102_1Active()
		{
			AssertEquals(false, configuration.IsRuleE1102_1Active);
		}

		public virtual void TestIsRuleE1104_1Active()
		{
			AssertEquals(false, configuration.IsRuleE1104_1Active);
		}

		public virtual void TestIsRuleE1401_1Active()
		{
			AssertEquals(false, configuration.IsRuleE1401_1Active);
		}

		public virtual void TestIsRuleE1406Active()
		{
			AssertEquals(true, configuration.IsRuleE1406Active);
		}

		public virtual void TestIsRuleG0090Active()
		{
			AssertEquals(false, configuration.IsRuleG0090Active);
		}

		public virtual void TestIsRuleG0123_1Active()
		{
			AssertEquals(false, configuration.IsRuleG0123_1Active);
		}

		public virtual void TestIsRuleG0321Active()
		{
			AssertEquals(false, configuration.IsRuleG0321Active);
		}

		public virtual void TestIsRuleG0587Active()
		{
			AssertEquals(false, configuration.IsRuleG0587Active);
		}

		public virtual void TestIsRuleNR0002Active()
		{
			AssertEquals(false, configuration.IsRuleNR0002Active);
		}

		public virtual void TestIsRuleNR0010Active()
		{
			AssertEquals(false, configuration.IsRuleNR0010Active);
		}

		public virtual void TestIsRuleNR0022Active()
		{
			AssertEquals(false, configuration.IsRuleNR0022Active);
		}

		public virtual void TestIsRuleNR0048Active()
		{
			AssertEquals(false, configuration.IsRuleNR0048Active);
		}

		public virtual void TestIsRuleNR0053Active()
		{
			AssertEquals(false, configuration.IsRuleNR0053Active);
		}

		public virtual void TestIsRuleNR0054Active()
		{
			AssertEquals(false, configuration.IsRuleNR0054Active);
		}

		public virtual void TestIsRulePLR0601Active()
		{
			AssertEquals(true, configuration.IsRulePLR0601Active);
		}

		public virtual void TestIsRuleR0003Active()
		{
			AssertEquals(true, configuration.IsRuleR0003Active);
		}

		public virtual void TestIsRuleR0076Active()
		{
			AssertEquals(true, configuration.IsRuleR0076Active);
		}

		public virtual void TestIsRuleR0100Active()
		{
			AssertEquals(true, configuration.IsRuleR0100Active);
		}

		public virtual void TestIsRuleR0315Active()
		{
			AssertEquals(true, configuration.IsRuleR0315Active);
		}

		public virtual void TestIsRuleR0350Active()
		{
			AssertEquals(true, configuration.IsRuleR0350Active);
		}

		public virtual void TestIsRuleR0520Active()
		{
			AssertEquals(false, configuration.IsRuleR0520Active);
		}

		public virtual void TestIsRuleR0601Active()
		{
			AssertEquals(true, configuration.IsRuleR0601Active);
		}

		public virtual void TestIsRuleR0850Active()
		{
			AssertEquals(true, configuration.IsRuleR0850Active);
		}

		public virtual void TestIsRuleR0850_1Active()
		{
			AssertEquals(false, configuration.IsRuleR0850_1Active);
		}

		public virtual void TestIsRuleR0859Active()
		{
			AssertEquals(true, configuration.IsRuleR0859Active);
		}

		public virtual void TestIsRuleRP16Active()
		{
			AssertEquals(false, configuration.IsRuleRP16Active);
		}

		public virtual void TestIsRuleTR0001Active()
		{
			AssertEquals(true, configuration.IsRuleTR0001Active);
		}

		public virtual void TestIsRuleTR0002Active()
		{
			Assert(configuration.IsRuleTR0002Active);
		}

		public virtual void TestIsRuleTR0003Active()
		{
			Assert(configuration.IsRuleTR0003Active);
		}

		public virtual void TestIsRuleTR0004Active()
		{
			Assert(configuration.IsRuleTR0004Active);
		}

		public virtual void TestIsRuleTR0006Active()
		{
			Assert(configuration.IsRuleTR0006Active);
		}

		public virtual void TestIsRuleTR0007Active()
		{
			Assert(configuration.IsRuleTR0007Active);
		}

		public virtual void TestIsRuleTR0008Active()
		{
			Assert(configuration.IsRuleTR0008Active);
		}

		public virtual void TestIsRuleTR0009Active()
		{
			Assert(configuration.IsRuleTR0009Active);
		}

		public virtual void TestIsRuleTR0011Active()
		{
			AssertEquals(true, configuration.IsRuleTR0011Active);
		}

		public virtual void TestIsRuleTR0016Active()
		{
			AssertEquals(true, configuration.IsRuleTR0016Active);
		}

		public virtual void TestIsRuleTR0023Active()
		{
			AssertEquals(true, configuration.IsRuleTR0023Active);
		}

		public virtual void TestIsRuleTR0024Active()
		{
			AssertEquals(true, configuration.IsRuleTR0024Active);
		}

		public virtual void TestIsRuleTR0025Active()
		{
			AssertEquals(true, configuration.IsRuleTR0025Active);
		}

		public virtual void TestIsRuleTR0026Active()
		{
			AssertEquals(true, configuration.IsRuleTR0026Active);
		}

		public virtual void TestIsRuleTR0027Active()
		{
			AssertEquals(true, configuration.IsRuleTR0027Active);
		}

		public virtual void TestIsRuleTR0028Active()
		{
			AssertEquals(true, configuration.IsRuleTR0028Active);
		}

		public virtual void TestIsRuleTR0029Active()
		{
			AssertEquals(true, configuration.IsRuleTR0029Active);
		}

		public virtual void TestIsRuleTR0030Active()
		{
			AssertEquals(true, configuration.IsRuleTR0030Active);
		}

		public virtual void TestIsRuleTR0036Active()
		{
			AssertEquals(true, configuration.IsRuleTR0036Active);
		}

		public virtual void TestIsRuleTR0037Active()
		{
			AssertEquals(true, configuration.IsRuleTR0037Active);
		}

		public virtual void TestIsRuleTR0038Active()
		{
			AssertEquals(true, configuration.IsRuleTR0038Active);
		}

		public virtual void TestIsRuleTR0046Active()
		{
			AssertEquals(true, configuration.IsRuleTR0046Active);
		}

		public virtual void TestIsRuleTR0052Active()
		{
			AssertEquals(false, configuration.IsRuleTR0052Active);
		}

		public virtual void TestIsRuleTR0055Active()
		{
			AssertEquals(false, configuration.IsRuleTR0055Active);
		}

		public virtual void TestIsRuleTR0056Active()
		{
			AssertEquals(false, configuration.IsRuleTR0056Active);
		}

		public virtual void TestIsRuleTR0064Active()
		{
			AssertEquals(true, configuration.IsRuleTR0064Active);
		}

		public virtual void TestIsRuleTR0067Active()
		{
			AssertEquals(true, configuration.IsRuleTR0067Active);
		}

		public virtual void TestIsRuleTR0073Active()
		{
			AssertEquals(true, configuration.IsRuleTR0073Active);
		}

		public virtual void TestIsRuleTR0074Active()
		{
			AssertEquals(true, configuration.IsRuleTR0074Active);
		}

		public virtual void TestIsRuleTR0075Active()
		{
			AssertEquals(true, configuration.IsRuleTR0075Active);
		}

		public virtual void TestIsRuleTR0084Active()
		{
			AssertEquals(true, configuration.IsRuleTR0084Active);
		}

		public virtual void TestIsCountryCodeRequiredToBeSameAsCurrentCompany()
		{
			AssertEquals(false, configuration.IsCountryCodeRequiredToBeSameAsCurrentCompany);
		}

		public virtual void TestGetNewMessages()
		{
			AssertType<ValidationRuleMessages>(configuration.Messages);
		}

		#region Implementation

		protected TConfiguration configuration;

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (TConfiguration)Activator.CreateInstance(typeof(TConfiguration));
		}

		#endregion
	}

	[TestedType(typeof(ValidationRuleConfiguration))]
	sealed class ValidationRuleConfigurationBaseOnlyTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
	{
	}
}
