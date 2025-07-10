using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusSealValidationDeciderTestContext<T> : IDisposable where T : class, ICusSealValidationDecider
	{
		readonly Mock<T> validationDeciderMock;
		readonly IDisposable cleanup;

		public CusSealValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
		{
			validationDeciderMock = new Mock<T> { CallBase = true };

			var asMethod = validationDeciderMock.GetType().GetMethod("As");
			foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
			{
				asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
			}

			var cusSealConfigurationConfiguration = new Mock<CusSealConfiguration> { CallBase = true };
			cusSealConfigurationConfiguration
				.Protected()
				.Setup<ICusSealValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDeciderMock.Object);

			var configuration = new Mock<NctsConfiguration> { CallBase = true };
			configuration
				.Protected()
				.Setup<CusSealConfiguration>("GetNewCusSealConfiguration")
				.Returns(cusSealConfigurationConfiguration.Object);

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

		public void Dispose() => cleanup.Dispose();
	}
}
