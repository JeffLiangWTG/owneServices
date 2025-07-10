using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class NctsGuaranteeValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T> where T : class, INctsGuaranteeValidationDecider
	{
		public NctsGuaranteeValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes) : base(factory, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var guaranteeConfiguration = new Mock<GuaranteeConfiguration> { CallBase = true };
			guaranteeConfiguration
				.Protected()
				.Setup<INctsGuaranteeValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDecider);
			configuration.Protected()
				.Setup<GuaranteeConfiguration>("GetNewGuaranteeConfiguration")
				.Returns(guaranteeConfiguration.Object);
		}
	}
}
