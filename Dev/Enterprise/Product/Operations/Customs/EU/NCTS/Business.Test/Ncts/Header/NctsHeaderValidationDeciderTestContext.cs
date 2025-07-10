using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class NctsHeaderValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsHeaderValidationDecider
	{
		public NctsHeaderValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			_ = configuration
				.Protected()
				.Setup<INctsHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDecider);
		}
	}
}
