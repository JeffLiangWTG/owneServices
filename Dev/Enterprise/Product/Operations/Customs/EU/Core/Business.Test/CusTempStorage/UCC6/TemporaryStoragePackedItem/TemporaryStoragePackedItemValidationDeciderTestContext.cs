using System;
using System.Linq.Expressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

sealed class TemporaryStoragePackedItemValidationDeciderTestContext<T> : IDisposable
	where T : ITemporaryStoragePackedItemValidationDecider
{
	readonly Mock<ITemporaryStoragePackedItemValidationDecider> validationDeciderMock;
	readonly IDisposable cleanup;

	public TemporaryStoragePackedItemValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
	{
		validationDeciderMock = new Mock<ITemporaryStoragePackedItemValidationDecider> { CallBase = true };
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

	void SetupConfiguration(Mock<TemporaryStorageConfiguration> configuration, ITemporaryStoragePackedItemValidationDecider validationDecider)
	{
		var packedItemConfiguration = new Mock<TemporaryStoragePackedItemConfiguration> { CallBase = true };
		packedItemConfiguration
			.Protected()
			.Setup<ITemporaryStoragePackedItemValidationDecider>("GetValidationDeciderCore")
			.Returns(validationDecider);

		configuration
			.Protected()
			.Setup<TemporaryStoragePackedItemConfiguration>("GetNewPackedItemConfiguration")
			.Returns(packedItemConfiguration.Object);
	}

	void SetupRuleDeciderInterfaceTypes(Type[] ruleDeciderInterfaceTypes)
	{
		var asMethod = validationDeciderMock.GetType().GetMethod("As");
		foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
		{
			asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
		}
	}

	public void EnableRule(Expression<Func<ITemporaryStoragePackedItemValidationDecider, bool>> rule)
	{
		validationDeciderMock.SetupGet(rule).Returns(true);
	}

	public void DisableRule(Expression<Func<ITemporaryStoragePackedItemValidationDecider, bool>> rule)
	{
		validationDeciderMock.SetupGet(rule).Returns(false);
	}

	public void Dispose() => cleanup.Dispose();
}
