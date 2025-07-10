using System;
using CargoWise.Common;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class ValidationRuleConfigurationTestContext : IDisposable
	{
		readonly IDisposable setConfigurationContext;
		readonly Mock<ValidationRuleConfiguration> validationRuleConfigurationMock;

		public ValidationRuleConfigurationTestContext(IDisposable setConfigurationContext, Mock<ValidationRuleConfiguration> validationRuleConfigurationMock)
		{
			this.setConfigurationContext = Argument.NotNull(setConfigurationContext, nameof(setConfigurationContext));
			this.validationRuleConfigurationMock = Argument.NotNull(validationRuleConfigurationMock, nameof(validationRuleConfigurationMock));
		}

		public void Dispose()
		{
			setConfigurationContext.Dispose();
		}

		public void EnableRule(string publicPropertyName)
		{
			var method = string.Format("{0}Core", publicPropertyName);
			validationRuleConfigurationMock.Protected().Setup<bool>(method).Returns(true);
		}

		public void DisableRule(string publicPropertyName)
		{
			var method = string.Format("{0}Core", publicPropertyName);
			validationRuleConfigurationMock.Protected().Setup<bool>(method).Returns(false);
		}

		public void AssertRuleChecked(string publicPropertyName)
		{
			var method = string.Format("{0}Core", publicPropertyName);
			validationRuleConfigurationMock.Protected().Verify<bool>(method, Times.AtLeastOnce());
		}
	}
}
