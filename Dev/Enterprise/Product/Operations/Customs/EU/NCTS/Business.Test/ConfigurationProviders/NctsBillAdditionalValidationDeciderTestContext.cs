using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillAdditionalValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsBillAdditionalDocumentValidationDecider
	{
		public NctsBillAdditionalValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var billConfiguration = new Mock<BillConfiguration> { CallBase = true };
			billConfiguration
				.Protected()
				.Setup<INctsBillAdditionalDocumentValidationDecider>("GetBillAdditionalDocumentValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDecider);

			configuration
				.Protected()
				.Setup<BillConfiguration>("GetNewBillConfiguration")
				.Returns(billConfiguration.Object);
		}
	}
}
