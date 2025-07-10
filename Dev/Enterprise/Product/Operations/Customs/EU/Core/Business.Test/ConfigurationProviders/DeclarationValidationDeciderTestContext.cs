using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	public sealed class DeclarationValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IDeclarationValidationDecider>
	{
		public DeclarationValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6 = false, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, IDeclarationValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			_ = configurationMock.Protected()
				.Setup<IDeclarationValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<BusinessObject>())
				.Returns(validationDecider);
		}
	}
}
