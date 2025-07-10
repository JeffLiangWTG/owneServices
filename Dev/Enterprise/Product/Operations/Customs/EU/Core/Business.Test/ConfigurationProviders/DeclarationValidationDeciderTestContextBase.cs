using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing;

public abstract class DeclarationValidationDeciderTestContext<TValidationDecider>
	: ValidationDeciderTestContext<TValidationDecider, DeclarationConfiguration>
	where TValidationDecider : class
{
	protected DeclarationValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes)
		: base(declaration?.Factory, ruleDeciderInterfaceTypes)
	{
		this.declaration = declaration;
		this.isUCC6 = isUCC6;

		AddAutoCacheResetObject(declaration);
	}

	protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, TValidationDecider validationDecider)
	{
		_ = configurationMock.Protected()
			.Setup<ZBool>("IsUCC6Core", ItExpr.IsAny<BusinessObject>())
			.Returns(isUCC6);
	}

	protected sealed override IDisposable UpdateObjectFactory(BusinessObjectFactory factory, Mock<DeclarationConfiguration> configurationMock)
	{
		return ConfigurationTestHelper.UpdateObjectFactory(factory, configurationMock, declaration.GetDefaultDataGroupingCode());
	}

	readonly JobDeclaration declaration;
	readonly bool isUCC6;
}
