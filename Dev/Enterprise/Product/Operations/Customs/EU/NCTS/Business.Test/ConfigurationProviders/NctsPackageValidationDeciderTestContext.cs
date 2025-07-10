using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class NctsPackageValidationDeciderTestContext<TValidationDecider>
		: NctsValidationDeciderTestContext<TValidationDecider>
		where TValidationDecider : class, INctsPackageValidationDecider
	{
		public NctsPackageValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
			var validationDeciderMock = new Mock<TValidationDecider> { CallBase = true };
			var asMethod = validationDeciderMock.GetType().GetMethod("As");
			foreach (var ruleDeciderInterfaceType in ruleDeciderInterfaceTypes)
			{
				asMethod.MakeGenericMethod(ruleDeciderInterfaceType).Invoke(validationDeciderMock, null);
			}
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configurationMock, TValidationDecider validationDecider)
		{
			var nctsPackageConfigurationConfiguration = new Mock<NctsPackageConfiguration> { CallBase = true };
			nctsPackageConfigurationConfiguration
				.Protected()
				.Setup<INctsPackageValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsCommonCargoDesc>())
				.Returns(validationDecider);

			configurationMock
				.Protected()
				.Setup<NctsPackageConfiguration>("GetNewNctsPackageConfiguration")
				.Returns(nctsPackageConfigurationConfiguration.Object);
		}
	}
}
