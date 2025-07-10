using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class ArrivalCusTransportMeansValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, IArrivalCusTransportMeansValidationDecider
	{
		public ArrivalCusTransportMeansValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var arrivalCusTransportMeansConfiguration = new Mock<ArrivalCusTransportMeansConfiguration> { CallBase = true };
			arrivalCusTransportMeansConfiguration
				.Protected()
				.Setup<IArrivalCusTransportMeansValidationDecider>("ArrivalCusTransportMeansValidationDeciderCore", ItExpr.IsAny<ArrivalCusTransportMeans>())
				.Returns(validationDecider);

			configuration
				.Protected()
				.Setup<ArrivalCusTransportMeansConfiguration>("GetNewArrivalCusTransportMeansConfiguration")
				.Returns(arrivalCusTransportMeansConfiguration.Object);
		}
	}
}
