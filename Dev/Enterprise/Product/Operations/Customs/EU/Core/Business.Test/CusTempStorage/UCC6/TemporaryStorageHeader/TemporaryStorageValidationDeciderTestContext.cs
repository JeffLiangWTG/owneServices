using System;
using System.Linq.Expressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageValidationDeciderTestContext<T> : IDisposable
		where T : ITemporaryStorageHeaderValidationDecider
	{
		readonly Mock<ITemporaryStorageHeaderValidationDecider> validationDeciderMock;
		readonly IDisposable cleanup;

		public TemporaryStorageValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
		{
			validationDeciderMock = new Mock<ITemporaryStorageHeaderValidationDecider> { CallBase = true };
			var configuration = new Mock<TemporaryStorageConfiguration> { CallBase = true };
			SetupRuleDeciderInterfaceTypes(ruleDeciderInterfaceTypes);
			SetupConfiguration(configuration, validationDeciderMock.Object);
			cleanup = UpdateObjectFactory(factory, configuration);
		}

		internal static IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<TemporaryStorageConfiguration> temporaryStorageHeaderConfigurationMock)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			factory.ClearCachedValue<TemporaryStorageConfiguration>($"TemporaryStorageConfiguration{currentCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(temporaryStorageHeaderConfigurationMock.Object);

			var temporaryStorageHeaderConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ currentCountryCode, objectHandleMock.Object }
			};

			return ObjectFactory.Substitute("TemporaryStorageConfiguration", temporaryStorageHeaderConfiguration);
		}

		void SetupConfiguration(Mock<TemporaryStorageConfiguration> configuration, ITemporaryStorageHeaderValidationDecider validationDecider)
		{
			configuration
				.Protected()
				.Setup<ITemporaryStorageHeaderValidationDecider>("GetValidationDeciderCore")
				.Returns(validationDecider);
		}

		void SetupRuleDeciderInterfaceTypes(Type[] ruleDeciderInterfaceTypes)
		{
			var asMethod = validationDeciderMock.GetType().GetMethod("As");
			foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
			{
				asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
			}
		}

		public void EnableRule(Expression<Func<ITemporaryStorageHeaderValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(true);
		}

		public void DisableRule(Expression<Func<ITemporaryStorageHeaderValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(false);
		}

		public void Dispose() => cleanup.Dispose();
	}
}
