using System;
using System.Linq;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public static class ValidationRuleConfigurationTestHelper
	{
		public static void AssertNoNotificationsWithInactiveRule(
			BusinessObjectFactory factory,
			ZPropertyInfo propertyInfo,
			string message,
			string publicPropertyName,
			params Action[] testCases)
		{
			using (var ruleTestContext = GetValidationRuleConfigurationTestContext(factory))
			{
				ruleTestContext.DisableRule(publicPropertyName);

				foreach (var testCase in testCases)
				{
					testCase?.Invoke();
					Assertion.AssertCollectionNotContains($"Expected notifications would not contain {message}", propertyInfo.Notifications.Select(e => e.Message), x => x.Contains(message));

					ruleTestContext.AssertRuleChecked(publicPropertyName);
				}
			}
		}

		public static IDisposable TemporarilyInactivateValidationConfigurationRule(
			BusinessObjectFactory factory,
			string publicPropertyName)
		{
			return NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(factory, publicPropertyName, value: false);
		}

		public static IDisposable TemporarilyActivateValidationConfigurationRule(
			BusinessObjectFactory factory,
			string publicPropertyName)
		{
			return NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(factory, publicPropertyName, value: true);
		}

		public static IDisposable TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(
			BusinessObjectFactory factory, bool isLiabilityCalculationForArrivalSupported,
			string publicPropertyName)
		{
			return NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, isLiabilityCalculationForArrivalSupported, publicPropertyName, value: false);
		}

		public static IDisposable TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(
			BusinessObjectFactory factory, bool isLiabilityCalculationForArrivalSupported,
			string publicPropertyName)
		{
			return NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRuleForArrivalWithLiabilityCalculation(factory, isLiabilityCalculationForArrivalSupported, publicPropertyName, value: true);
		}

		/// <summary>
		/// Creates a context with mocked ValidationRuleConfiguration and substitutes it in <paramref name="factory"/>
		/// </summary>
		/// <param name="factory">A factory to substitute ValidationRuleConfiguration</param>
		/// <returns></returns>
		public static ValidationRuleConfigurationTestContext GetValidationRuleConfigurationTestContext(BusinessObjectFactory factory)
		{
			var validationRuleConfiguration = new Mock<ValidationRuleConfiguration>() { CallBase = true };
			var setConfigurationContext = NctsConfigurationTestHelper.TemporarilySetConfiguration(factory, "GetNewValidationRuleConfiguration", validationRuleConfiguration.Object);

			return new ValidationRuleConfigurationTestContext(setConfigurationContext, validationRuleConfiguration);
		}

		public static IDisposable TemporarilySetUseGuaranteeGridValidation(BusinessObjectFactory factory, bool isSet)
		{
			return NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfigurationUseGuaranteeGridValidation(factory, isSet);
		}
	}
}
