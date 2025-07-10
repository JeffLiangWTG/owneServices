using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class NctsBillValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsBillValidationDecider
	{
		public NctsBillValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var billConfiguration = new Mock<BillConfiguration> { CallBase = true };
			billConfiguration
				.Protected()
				.Setup<INctsBillValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDecider);

			configuration
				.Protected()
				.Setup<BillConfiguration>("GetNewBillConfiguration")
				.Returns(billConfiguration.Object);
		}
	}
}
