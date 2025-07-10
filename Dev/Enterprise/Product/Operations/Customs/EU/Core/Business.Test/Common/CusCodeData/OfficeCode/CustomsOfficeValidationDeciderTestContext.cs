using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CustomsOfficeValidationDeciderTestContext : DeclarationValidationDeciderTestContext<ICustomsOfficeValidationDecider>
	{
		public CustomsOfficeValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, ICustomsOfficeValidationDecider validationDecider)
		{
			_ = configurationMock.Protected()
				.Setup<ICustomsOfficeValidationDecider>("GetCustomsOfficeValidationDeciderCore", ItExpr.IsAny<BusinessObject>())
				.Returns(validationDecider);
		}
	}
}
