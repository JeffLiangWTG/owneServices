using System;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Testing.ConfigurationProviders
{
	public sealed class DeclarationValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IDeclarationValidationDecider>
	{
		public DeclarationValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6 = true, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configurationMock, IDeclarationValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			_ = configurationMock.Protected()
				.Setup<EU.Business.Declaration.IDeclarationValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobDeclaration>())
				.Returns(validationDecider);
		}
	}
}
