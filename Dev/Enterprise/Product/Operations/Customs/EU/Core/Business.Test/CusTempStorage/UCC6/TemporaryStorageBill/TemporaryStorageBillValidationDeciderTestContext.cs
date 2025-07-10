using System;
using System.Linq.Expressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageBillValidationDeciderTestContext<T> : IDisposable
		where T : ITemporaryStorageBillValidationDecider
	{
		readonly Mock<ITemporaryStorageBillValidationDecider> validationDeciderMock;
		readonly IDisposable cleanup;

		public TemporaryStorageBillValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
		{
			validationDeciderMock = new Mock<ITemporaryStorageBillValidationDecider> { CallBase = true };
			var configuration = new Mock<TemporaryStorageConfiguration> { CallBase = true };
			SetupRuleDeciderInterfaceTypes(ruleDeciderInterfaceTypes);
			SetupConfiguration(configuration, validationDeciderMock.Object);
			cleanup = UpdateObjectFactory(factory, configuration);
		}

		internal static IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<TemporaryStorageConfiguration> temporaryStorageConfigurationMock)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			factory.ClearCachedValue<TemporaryStorageConfiguration>($"TemporaryStorageConfiguration_{currentCountryCode}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(temporaryStorageConfigurationMock.Object);

			var temporaryStorageConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ currentCountryCode, objectHandleMock.Object }
			};

			return ObjectFactory.Substitute("TemporaryStorageConfiguration", temporaryStorageConfiguration);
		}

		void SetupConfiguration(Mock<TemporaryStorageConfiguration> configuration, ITemporaryStorageBillValidationDecider validationDecider)
		{
			var billConfiguration = new Mock<TemporaryStorageBillConfiguration> { CallBase = true };
			billConfiguration
				.Protected()
				.Setup<ITemporaryStorageBillValidationDecider>("GetValidationDeciderCore")
				.Returns(validationDecider);

			configuration
				.Protected()
				.Setup<TemporaryStorageBillConfiguration>("GetNewBillConfiguration")
				.Returns(billConfiguration.Object);
		}

		void SetupRuleDeciderInterfaceTypes(Type[] ruleDeciderInterfaceTypes)
		{
			var asMethod = validationDeciderMock.GetType().GetMethod("As");
			foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
			{
				asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
			}
		}

		public void EnableRule(Expression<Func<ITemporaryStorageBillValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(true);
		}

		public void EnableRule(Expression<Func<ITemporaryStorageBillValidationDecider, Func<AsycudaBill, bool>>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns((x) => true);
		}

		public void EnableRule(Expression<Func<ITemporaryStorageBillValidationDecider, Func<TemporaryStorageBill, bool>>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns((x) => true);
		}

		public void DisableRule(Expression<Func<ITemporaryStorageBillValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(false);
		}

		public void DisableRule(Expression<Func<ITemporaryStorageBillValidationDecider, Func<TemporaryStorageBill, bool>>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns((x) => false);
		}

		public void Dispose() => cleanup.Dispose();
	}
}
