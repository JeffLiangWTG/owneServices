using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsEuOfficeCodeValidationDeciderTestContext<T> : IDisposable where T : class, INctsEuOfficeCodeValidationDecider
	{
		readonly Mock<T> validationDeciderMock;
		readonly IDisposable cleanup;

		public NctsEuOfficeCodeValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
		{
			validationDeciderMock = new Mock<T> { CallBase = true };

			var asMethod = validationDeciderMock.GetType().GetMethod("As");
			foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
			{
				asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
			}

			var nctsEuOfficeCodeConfigurationConfiguration = new Mock<NctsEuOfficeCodeConfiguration> { CallBase = true };
			nctsEuOfficeCodeConfigurationConfiguration
				.Protected()
				.Setup<INctsEuOfficeCodeValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDeciderMock.Object);

			var configuration = new Mock<NctsConfiguration> { CallBase = true };
			configuration
				.Protected()
				.Setup<NctsEuOfficeCodeConfiguration>("GetNewNctsEuOfficeCodeConfiguration")
				.Returns(nctsEuOfficeCodeConfigurationConfiguration.Object);

			cleanup = NctsConfigurationTestHelper.UpdateObjectFactory(factory, configuration);
		}

		public void EnableRule(Expression<Func<T, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(true);
		}

		public void DisableRule(Expression<Func<T, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(false);
		}

		public void AssertRuleChecked(Expression<Func<T, bool>> rule)
		{
			validationDeciderMock.VerifyGet(rule, Times.AtLeastOnce, "Expected IsRuleXXXXXActive property to be checked at least once per test case.");
			validationDeciderMock.Invocations.Clear();
		}

		public void Dispose() => cleanup.Dispose();
	}
}
