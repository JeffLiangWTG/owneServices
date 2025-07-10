using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class CusAuthorizationUsageValidationDeciderTestContext<TValidationDecider> : IDisposable where TValidationDecider : class, ICusAuthorizationUsagePhase5ValidationDecider
{
	readonly Mock<TValidationDecider> validationDeciderMock;
	readonly IDisposable cleanup;

	public CusAuthorizationUsageValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
	{
		validationDeciderMock = new Mock<TValidationDecider> { CallBase = true };
		var asMethod = validationDeciderMock.GetType().GetMethod("As");
		foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
		{
			asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
		}
		var cusAuthorizationUsageConfiguration = new Mock<CusAuthorizationUsageConfiguration> { CallBase = true };
		cusAuthorizationUsageConfiguration
			.Protected()
			.Setup<ICusAuthorizationUsagePhase5ValidationDecider>("GetPhase5ValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
			.Returns(validationDeciderMock.Object);
		var configuration = new Mock<NctsConfiguration> { CallBase = true };
		configuration
			.Protected()
			.Setup<CusAuthorizationUsageConfiguration>("GetNewCusAuthorizationUsageConfiguration")
			.Returns(cusAuthorizationUsageConfiguration.Object);
		cleanup = NctsConfigurationTestHelper.UpdateObjectFactory(factory, configuration);
	}
	public void EnableRule(Expression<Func<TValidationDecider, bool>> rule)
	{
		validationDeciderMock.SetupGet(rule).Returns(true);
	}
	public void DisableRule(Expression<Func<TValidationDecider, bool>> rule)
	{
		validationDeciderMock.SetupGet(rule).Returns(false);
	}
	public void Dispose() => cleanup.Dispose();
}
