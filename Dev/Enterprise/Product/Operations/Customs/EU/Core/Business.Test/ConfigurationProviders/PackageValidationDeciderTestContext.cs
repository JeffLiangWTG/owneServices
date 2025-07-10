using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class PackageValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IPackageValidationDecider>
	{
		public PackageValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<DeclarationConfiguration> configuration, IPackageValidationDecider validationDecider)
		{
			base.SetupConfiguration(configuration, validationDecider);

			_ = configuration.Protected()
				.Setup<IPackageValidationDecider>("GetPackageValidationDeciderCore", ItExpr.IsAny<BusinessObject>())
				.Returns(validationDecider);
		}
	}
}
